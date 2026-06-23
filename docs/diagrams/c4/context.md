# C4 Context Diagram

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit architecture |
| Sources | Code/config analysis |
| Confidence | High |
| Related | [Context Diagram](../../architecture/context-diagram.md) |

```mermaid
C4Context
    title Publink Audit — system context
    Person(user, "Treasurer / audit operator", "Prepares for RIO inspection; needs to show who changed what and when on contracts")
    System(system, "Publink Audit", "Read-only contract audit-history explorer: search, timeline, export")
    System_Ext(legacy, "Legacy SQL audit source", "Source of truth for contract change rows; read-only from this system")
    System_Ext(bus, "Azure Service Bus / emulator", "Async message broker for import events and manual-sync commands")
    System_Ext(sql, "MSSQL active/archive stores", "Owned persistence: AuditReadModel and AuditArchive databases")
    Rel(user, system, "Searches, reviews, exports", "Browser")
    Rel(system, legacy, "Reads audit rows after checkpoint", "SQL / TDS")
    Rel(system, bus, "Publishes imported events; consumes projections and sync commands", "AMQP")
    Rel(system, sql, "Persists canonical events and projections; queries read models", "TDS")
```

**MVP boundary.** Inside the boundary: contract-change ingestion from legacy SQL, canonical event storage, search and timeline projections, ZIP export with checksums. Outside the boundary: authentication and authorisation (no identity provider is integrated), writes to the legacy source (it is read-only), legal evidence guarantees (no WORM, signatures or trusted timestamps), and production infrastructure (Docker Compose only). A production deployment must add auth before any real data is served.
