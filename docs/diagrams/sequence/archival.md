# Archival Sequence

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit engineering/SRE |
| Sources | Archive lifecycle and executor |
| Confidence | High |
| Related | [Archival State](../state/archival-state.md), [Audit Domain](../../domains/audit-domain.md) |

```mermaid
sequenceDiagram
    participant Worker as archival-worker
    participant Active as AuditReadModel
    participant Archive as AuditArchive
    Worker->>Active: Find inactive contract candidate
    Worker->>Active: Load aliases, timeline items, canonical events
    Worker->>Active: Upsert contract_archive_transfers → Copying
    Worker->>Archive: Replace archive snapshot
    Worker->>Archive: Verify snapshot integrity
    Worker->>Active: Mark transfer → Verified
    Worker->>Active: Serializable recheck (was contract modified during copy?)
    alt unchanged — safe to delete
        Worker->>Active: Delete active rows; mark transfer → Archived
    else changed during copy — rollback
        Worker->>Archive: Delete snapshot
        Worker->>Active: Reset transfer → Active
    end
```

On any exception, the lifecycle marks the transfer `Failed` with an `ErrorCode`, logs the error and retries on the next worker cycle. This makes the entire workflow retryable without distributed transactions.

**The serializable recheck** is the critical safety step. Between the moment the worker loaded the contract data and the moment it would delete active rows, another import or synchronisation cycle could have added new changes. The recheck runs at serializable isolation level to detect this. If the contract changed, the worker discards the archive copy and returns the contract to active handling — no data is lost and the transfer can be retried cleanly.

> **Business implication.** Archived data is never deleted — it moves from the hot `AuditReadModel` database to the cheaper `AuditArchive` database. The treasurer can still search and export archived contracts; they are just served from the archive store. Archival reduces storage cost for old contracts while keeping the complete audit record permanently accessible. The `contract_archive_transfers` table is the state ledger: operators can inspect it to see which contracts are in flight, failed, or successfully archived.
