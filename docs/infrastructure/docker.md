# Docker And Docker Compose

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit DevOps |
| Sources | `docker/docker-compose.yml`, Dockerfiles, Nginx configs, Service Bus config |
| Confidence | High |
| Related | [Deployment Diagram](../architecture/deployment-diagram.md), [Networking](networking.md) |

Services:

| Service | Ports | Role |
| --- | --- | --- |
| `mssql` | `1433:1433` | SQL Server 2022, active/archive databases and emulator backing SQL. |
| `servicebus-emulator` | `5672`, `5300` | Local Azure Service Bus emulator. |
| `processing-worker` | internal | Consumes and projects audit events. |
| `ingestion-worker` | internal | Polls legacy source and handles sync commands. |
| `archival-worker` | internal | Moves inactive contracts to archive. |
| `query-api` | `8080:8080` | REST API. |
| `web` | `3000:80` | Nginx-served React SPA and proxy. |

Volume `mssql-data` persists only application-owned local SQL data. The ingestion worker always connects to the external client legacy database through `ConnectionStrings__LegacySource`, supplied from the required, Git-ignored `docker/.env.client` file.

Service Bus queues, topics and forwarding subscriptions are declared in `docker/servicebus/Config.json` because the local emulator does not fully support MassTransit topology creation through the management API.

The Service Bus emulator image is distroless, so the compose setup uses a separate `servicebus-ready` sidecar whose healthcheck polls `http://servicebus-emulator:5300/health` and gates dependent services until the emulator is actually ready.

Nginx proxies `/api/` to Query API and `/health/` to Query API health endpoints, with SPA fallback for other routes.

No container registry or production image tagging convention is defined: Assumption – requires validation.
