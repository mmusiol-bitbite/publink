# Glossary

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit engineering |
| Sources | `Audit.Contracts`, `Audit.Domain`, persistence models, frontend translations |
| Confidence | High for code-defined terms |
| Related | [Domain](domains/audit-domain.md), [Events](api/events.md), [README](README.md) |

| Term | Meaning |
| --- | --- |
| Active store | MSSQL `AuditReadModel` database used for current contract search and timeline queries. |
| Archive store | MSSQL `AuditArchive` database used for archived contract audit data. |
| Audit event | Canonical imported event represented by `AuditEntryImportedV1`. |
| Contract timeline | Ordered audit history for one contract, read from active or archive timeline tables. |
| Correlation ID | Operation identifier from source audit data, displayed on timeline items. |
| DLQ | Dead-letter queue used after MassTransit retries/max delivery are exhausted. |
| Field change | Field-level before/after/value-kind record in timeline and export output. |
| Idempotency | Duplicate-safe processing through deterministic event ID and unique `(Source, SourceEventId)`. |
| Legacy synchronization | Import process that reads the legacy SQL source to current. |
| Organization ID | Tenant/domain key present in audit data and storage models. |
| Projection | Query-optimized table derived from canonical audit events. |
| Snapshot sequence | Highest source sequence used to stabilize paged timeline reads. |

## Change Codes

| Code | Key |
| --- | --- |
| 1 | Added |
| 2 | Deleted |
| 3 | Modified |

## Entity Codes

| Code | Key |
| --- | --- |
| 1 | ContractHeader |
| 2 | AnnexHeader |
| 3 | AnnexChange |
| 4 | File |
| 5 | Invoice |
| 6 | PaymentSchedule |
| 7 | ContractFunding |
