# C4 Container Diagram

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit architecture |
| Sources | Docker Compose and startup files |
| Confidence | High |
| Related | [Container Diagram](../../architecture/container-diagram.md) |

```mermaid
C4Container
    title Publink Audit — container view

    Person(user, "Treasurer / audit operator")

    Container(web, "Nginx + React SPA", "Nginx / React 19 + Vite", "Active and archive contract browsing, search, timeline and ZIP export")
    Container(api, "Audit.Query.Api", ".NET 10 / ASP.NET Core", "REST API: search, timeline, export, manual sync, health")
    Container(ingestion, "Audit.Ingestion.Worker", ".NET 10 Worker", "Polls legacy SQL after checkpoint; handles RequestLegacySynchronizationV1")
    Container(processing, "Audit.Processing.Worker", ".NET 10 Worker", "Consumes AuditEntryImportedV1; writes canonical events and projections")
    Container(archival, "Audit.Archival.Worker", ".NET 10 Worker", "Copies inactive contracts to archive; retryable copy/verify/recheck/delete lifecycle")

    ContainerDb(active, "AuditReadModel", "MSSQL", "Canonical events, search projections, checkpoints, synchronisation leases, MassTransit inbox/outbox")
    ContainerDb(archive, "AuditArchive", "MSSQL", "Archived contract snapshots, audit events, search projections")

    System_Ext(bus, "Azure Service Bus / emulator", "AMQP broker")
    System_Ext(legacy, "Legacy SQL audit source", "Read-only source of audit rows")

    Rel(user, web, "Uses", "HTTPS / port 3000")
    Rel(web, api, "API calls", "HTTP / port 8080")
    Rel(api, active, "Reads projections", "TDS")
    Rel(api, archive, "Reads archive projections", "TDS")
    Rel(api, bus, "Sends RequestLegacySynchronizationV1", "AMQP")
    Rel(ingestion, legacy, "Reads audit rows after checkpoint", "TDS")
    Rel(ingestion, bus, "Publishes AuditEntryImportedV1", "AMQP")
    Rel(ingestion, active, "Reads/writes import_checkpoints", "TDS")
    Rel(bus, processing, "Delivers AuditEntryImportedV1", "AMQP")
    Rel(processing, active, "Appends audit_events; updates projections", "TDS")
    Rel(bus, ingestion, "Delivers RequestLegacySynchronizationV1", "AMQP")
    Rel(archival, active, "Reads candidates; manages transfer state", "TDS")
    Rel(archival, archive, "Writes archive snapshots", "TDS")
```

**Why four backend processes instead of one monolith.** Each process has a distinct failure domain: a bug in archival cannot block the query API; a DLQ spike in processing does not stall ingestion. Independent retry and DLQ settings let each boundary absorb its own failure rate without cascading. The split also allows the processes to be scaled, restarted and debugged independently — important when diagnosing stale checkpoint data or a stalled archive transfer.
