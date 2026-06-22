# C4 Container Diagram

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture |
| Sources | Docker Compose and startup files |
| Confidence | High |
| Related | [Container Diagram](../../architecture/container-diagram.md) |

```mermaid
flowchart LR
    Browser --> Web[Nginx + React SPA]
    Web --> Api[Query API]
    Api --> Active[(AuditReadModel)]
    Api --> Archive[(AuditArchive)]
    Api --> Bus[(Service Bus)]
    Ingestion[Ingestion Worker] --> Legacy[(Legacy SQL)]
    Ingestion --> Bus
    Bus --> Processing[Processing Worker]
    Processing --> Active
    Archival[Archival Worker] --> Active
    Archival --> Archive
```