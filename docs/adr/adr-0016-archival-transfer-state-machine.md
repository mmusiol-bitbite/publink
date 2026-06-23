# ADR-0016: Archival Transfer State Machine Without Distributed ACID

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit data architecture |
| Sources | `Audit.Infrastructure/Archiving/Execution/ContractArchivalExecutor.cs`, `Audit.Infrastructure/Archiving/Lifecycle/DatabaseContractArchiveLifecycle.cs`, `Audit.Infrastructure/Archiving/Lifecycle/ArchiveTransferStates.cs`, `Audit.Infrastructure/Archiving/Transfers/ContractArchiveTransferStore.cs`, `Audit.Infrastructure/Archiving/Snapshots/ContractArchiveSnapshotStore.cs`, `Audit.Infrastructure/Archiving/Reactivation/ArchivedContractReactivator.cs` |
| Confidence | High |
| Sources confirm | `ArchiveTransferStates.cs` enumerates all seven states; `ContractArchivalExecutor.CommitArchivalAsync` performs the serializable recheck |
| Related | [ADR-0007](adr-0007-mssql-active-archive-persistence.md), [ADR-0011](adr-0011-contract-level-archival.md), [Archival Sequence](../diagrams/sequence/archival.md), [Archival State](../diagrams/state/archival-state.md) |

## Status

Accepted.

## Date

2026-06-21.

## Context

Archival moves a contract's full data set (search entry, aliases, timeline items, canonical events) from `AuditReadModel` (active MSSQL database) to `AuditArchive` (separate MSSQL database). These are two independent database connections with no shared transaction coordinator. The operation must therefore be correct without distributed ACID guarantees.

The archival worker runs periodically as a background service. A crash or restart at any point in the process must not leave the system in a permanently inconsistent state (data lost from active, not yet in archive, or vice versa).

## Problem

Copying data across two independent databases with a simple load-insert-delete sequence has a race condition: if the active contract receives new events between the snapshot copy and the active-row deletion, the deletion would silently discard that new data. Additionally, if the worker crashes mid-operation, there is no reliable way to resume or roll back without durable workflow state.

Distributed transactions (XA/2PC with MSDTC) would solve atomicity but introduce infrastructure coupling, are unavailable in some MSSQL-as-a-service configurations, and have a history of operational fragility.

## Considered Options

- **Distributed transaction (XA/2PC).** Atomicity guaranteed by the transaction coordinator. Requires MSDTC or equivalent; adds infrastructure dependency; fragile under network partitions.
- **Saga with compensation messages.** Each step publishes a command; failure triggers a compensating command on the Service Bus. Correct but significantly increases complexity and introduces broker dependency into what is a local data-movement operation.
- **Simple copy-delete without consistency check.** Fast, simple. Data loss if the contract changes between copy and delete. Not acceptable for an audit system.
- **Durable state machine with serializable recheck (chosen).** Persists workflow state in `contract_archive_transfers` in the active database. Final delete is guarded by a serializable read-and-check that re-examines the active contract against the snapshot metrics before committing the deletion.

## Decision

The archival state machine is tracked in `contract_archive_transfers` (`ContractArchiveTransferEntity`) with seven states defined in `ArchiveTransferStates`:

| State | Meaning |
| --- | --- |
| `Active` | Candidate identified; copy not yet started, or rollback complete |
| `Copying` | Snapshot write to `AuditArchive` in progress |
| `Verified` | Snapshot written and integrity-checked against transfer metrics |
| `Archived` | Active rows deleted; archival committed |
| `ReactivationPending` | Operator requested reactivation to active store |
| `ReactivatedCopied` | Data copied back to active; archive snapshot not yet deleted |
| `Failed` | Unrecoverable exception; `ErrorCode` records exception type |

**Archival path** (`ContractArchivalExecutor.ExecuteAsync`):
1. Load all contract data from active store.
2. Upsert transfer to `Copying`, recording `LastActivityAt`, `SnapshotSequence`, `EventCount`.
3. Write snapshot to archive DB and verify integrity (`VerifySnapshotIntegrityAsync` counts events and checks counts match transfer).
4. Advance transfer to `Verified`.
5. Open a serializable transaction on the active DB. Re-read the contract and timeline metrics.
6. **If consistent with snapshot** (same `LastActivityAt`, same sequence and counts): delete active rows, advance transfer to `Archived`, commit.
7. **If inconsistent** (contract received new events between copy and final check): rollback the serializable transaction, delete the archive snapshot, reset transfer to `Active`. This is a normal operating condition — the next archival cycle will retry.

**Failure handling**: any unhandled exception during steps 1–6 transitions the transfer to `Failed` with the exception type as `ErrorCode`. A `Failed` transfer is not retried automatically; operator intervention is needed.

**Reactivation path** (`ArchivedContractReactivator.ReactivateAsync`): `ReactivationPending` → copy archive snapshot back to active store → `ReactivatedCopied` → delete archive snapshot → `Active`. Each step is individually durable; a crash at `ReactivatedCopied` is safely resumed without data loss.

## Consequences

- The archival worker is safe to restart at any point. `contract_archive_transfers` records the last durable state; the next cycle picks up where it left off or retries from `Active`.
- The serializable recheck eliminates the race condition between snapshot copy and active-row deletion without requiring cross-database transactions.
- A contract that is actively receiving new events during an archival window will repeatedly return to `Active` state. This is expected and correct; it will eventually be archived during a quiet period.
- `ReactivationPending` and `ReactivatedCopied` states make reactivation resumable. If the worker crashes after restoring data but before deleting the archive snapshot, the next cycle completes the cleanup.
- Monitoring should alert on `Failed` transfers; they require manual investigation and state reset.
- The state machine does not include `Archived → ReactivationPending` trigger logic at the API layer; that transition must be added when reactivation is exposed to operators.
