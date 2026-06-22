# Docker Compose Deployment Diagram

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit DevOps |
| Sources | `docker/docker-compose.yml` |
| Confidence | High |
| Related | [Docker](../../infrastructure/docker.md) |

```mermaid
flowchart TB
    Browser --> Web[web]
    Web --> Api[query-api]
    Api --> DB[(mssql)]
    Api --> SB[servicebus-emulator]
    Ingestion[ingestion-worker] --> DB
    Ingestion --> SB
    Processing[processing-worker] --> DB
    Processing --> SB
    Archival[archival-worker] --> DB
    SB --> DB
    DB --> Volume[(mssql-data)]
```