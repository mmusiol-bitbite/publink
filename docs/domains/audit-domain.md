# Audit Domain

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit architecture |
| Sources | `Audit.Domain`, `Audit.Contracts`, `Audit.Application`, persistence entities |
| Confidence | High for implemented rules; Medium for legacy semantics |
| Related | [Events](../api/events.md), [Backend Object Catalog](../architecture/backend-object-catalog.md), [Archival State](../diagrams/state/archival-state.md) |

The effective aggregate boundary is a contract. Entity type `ContractHeader` updates the contract search projection. Related audited entities are annexes, annex changes, files, invoices, payment schedules and funding entries.

Core persisted entities:

- `audit_events`: canonical imported events.
- `contract_timeline_items`: projected contract timeline rows.
- `contract_search`: active searchable contract summary.
- `contract_search_aliases`: historical searchable values.
- `import_checkpoints`: legacy import cursor.
- `legacy_synchronization_requests`: manual sync lease/state.
- `contract_archive_transfers`: archive/reactivation ledger.
- `archived_*`: archive snapshots for contract, aliases, events and timeline.

Rules implemented in code:

- Search phrase length is 2-200.
- Timeline actor max length is 200.
- Known change types are 1-3; known entity types are 1-7.
- Export supports locales `pl` and `en`.
- Export limit is 10,000 events.
- Archive eligibility is `LastActivityAt < now - InactivityYears`.
- Archival deletes active data only after snapshot verification and serializable recheck.
- Archive lifecycle coordinates path selection; archival copy/commit, reactivation/recovery and transfer ledger state updates are separate infrastructure responsibilities.
- Duplicate audit events are ignored by `(Source, SourceEventId)`.

Data-quality issues are surfaced when source values are unknown or malformed instead of failing all timeline reads.