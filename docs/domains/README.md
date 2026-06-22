# Domains

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture |
| Sources | Domain, application and persistence code |
| Confidence | High |
| Related | [Audit Domain](audit-domain.md), [ERD](../diagrams/erd/audit-storage.md) |

Implemented bounded contexts:

| Context | Responsibility |
| --- | --- |
| Legacy ingestion | Convert legacy audit rows to canonical events. |
| Audit processing | Persist canonical events and projections. |
| Contract archive | Move inactive contract audit history to archive. |
| Query experience | Serve active/archive search, timeline, export and sync status. |

See [Audit Domain](audit-domain.md).