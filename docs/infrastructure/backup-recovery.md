# Backup And Recovery

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit SRE/DBA |
| Sources | Docker Compose volume, persistence schema, archive workflow |
| Confidence | Medium for data structures; Low for production policy |
| Related | [Runbook](../getting-started/runbook.md), [Data Flow](../architecture/data-flow.md) |

Data requiring backup: `AuditReadModel`, `AuditArchive`, upstream legacy SQL source and broker DLQs/in-flight messages.

Implemented recovery characteristics: import checkpoints, idempotent event processing, retry/DLQ, archive transfer ledger and snapshot verification.

No backup schedule, restore test, RPO, RTO, disaster recovery topology or cross-region policy is defined: Assumption – requires validation.