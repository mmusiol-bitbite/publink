# Synchronous vs Asynchronous Communication

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture |
| Sources | Query API endpoints, MassTransit registrations |
| Confidence | High |
| Related | [Events](../api/events.md), [ADR 0003](../adr/adr-0003-at-least-once-messaging.md) |

Synchronous paths: browser to web, web to API, API/workers to MSSQL, health checks and export downloads.

Asynchronous paths:

| Message | Producer | Consumer |
| --- | --- | --- |
| `AuditEntryImportedV1` | Ingestion worker | Processing worker endpoint `audit-projection` |
| `RequestLegacySynchronizationV1` | Query API | Ingestion worker endpoint `legacy-synchronization` |

Retry policy on worker endpoints: exponential, 3 retries, 200ms to 5s. Max delivery count: 5. DLQ transports enabled. Processing uses EF Core outbox. Ingestion command endpoint concurrency is 1.

MassTransit EF support tables in the active MSSQL context are `InboxStates`, `OutboxMessages` and `OutboxStates`. These tables store message delivery/outbox state for reliability; audit business history is stored separately in `audit_events`, projections and archive snapshots.

Trade-off: asynchronous import decouples reads from legacy SQL but creates eventual consistency.