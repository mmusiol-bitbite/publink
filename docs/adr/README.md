# Architecture Decision Records

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit architecture |
| Sources | Code/config analysis and generated documentation |
| Confidence | High |
| Related | [System Overview](../architecture/system-overview.md) |

| ADR | Status | Decision | Business consequence |
| --- | --- | --- | --- |
| [ADR-0001](adr-0001-docker-compose-runtime.md) | Accepted for local/demo | Use Docker Compose as runtime baseline. | A reviewer can run the full system with a single command (`docker compose up --build`) without installing any infrastructure dependencies. |
| [ADR-0002](adr-0002-modular-worker-architecture.md) | Accepted | Split backend into API and workers. | Query API stays available when import or archival workers are restarting; failures are isolated and visible per process rather than taking the whole system down. |
| [ADR-0003](adr-0003-quality-gates.md) | Accepted | Enforce backend/frontend/container CI gates. | Every pull request is validated automatically; broken builds or failing tests do not reach the main branch and do not affect the demo environment. |
| [ADR-0004](adr-0004-legacy-sql-polling-anti-corruption-layer.md) | Accepted | Poll legacy SQL through an ingestion adapter. | The legacy Contracts system requires no changes; the audit explorer can be deployed independently without co-ordinating a source-system release. |
| [ADR-0005](adr-0005-at-least-once-messaging.md) | Accepted | Use at-least-once broker delivery and idempotent effects. | No audit event is silently lost; in the worst case an event is processed twice, which is harmless due to idempotency. The DLQ counter in the status view flags any events that could not be processed. |
| [ADR-0006](adr-0006-masstransit-transactional-outbox.md) | Accepted | MassTransit transactional outbox/inbox. | Broker publish and database commit are atomic; a crash between them cannot create duplicate or lost events. Three MassTransit state tables are added to `AuditReadModel`. |
| [ADR-0007](adr-0007-mssql-active-archive-persistence.md) | Accepted | Use MSSQL active/archive stores. | Current audit data is served from a fast, highly available store; older data moves to a cheaper archive without deletion. The treasurer can still search and export archived contracts. |
| [ADR-0008](adr-0008-canonical-events-and-projections.md) | Accepted | Store canonical events and query projections. | Search, timeline and export queries are fast because they read purpose-built tables, not the raw legacy rows. Projections can be rebuilt from canonical events if they need to be corrected. |
| [ADR-0009](adr-0009-ef-core-dapper-cqs-split.md) | Accepted | EF Core for writes, Dapper for reads. | Read queries are optimal single-statement SQL with LIKE ranking; write paths have change tracking and outbox atomicity. Two query languages are present in the codebase. |
| [ADR-0010](adr-0010-persister-reader-port-pattern.md) | Accepted | Purpose-built Persister/Reader ports (no generic repository). | Each use case depends only on the operations it needs; accidental CRUD misuse is impossible by construction; interfaces are narrow enough to mock without a database. |
| [ADR-0011](adr-0011-contract-level-archival.md) | Accepted | Archive by contract boundary. | The entire history of a contract (events, search entries, timeline) moves together; there is no partial or split record between hot and cold storage. |
| [ADR-0012](adr-0012-api-and-contract-versioning.md) | Accepted | Use `/api/v1` and V1 message contracts. | Future API or message-format changes can be introduced as `/api/v2` or `V2` contracts without breaking existing integrations or consumers. |
| [ADR-0013](adr-0013-opentelemetry-otlp-observability.md) | Accepted | OpenTelemetry SDK with OTLP exporter. | Traces and metrics are vendor-neutral; a production deployment can route to Azure Monitor, Jaeger or Grafana without changing application code. No collector is bundled locally. |
| [ADR-0014](adr-0014-frontend-architecture.md) | Accepted | React 19 + Vite + TanStack Query + i18next + Nginx SPA. | Fast dev builds, cached server-state management, bilingual runtime switching and production SPA serving with API proxy — no CORS configuration required on the API. |
| [ADR-0015](adr-0015-audit-export-as-verifiable-zip.md) | Accepted | Export audit history as a verifiable ZIP with manifest and checksums; cap at 10,000 events. | The treasurer hands auditors a self-contained package they can verify offline; the 10,000 event cap prevents unbounded API memory use. |
| [ADR-0016](adr-0016-archival-transfer-state-machine.md) | Accepted | Durable transfer state machine with serializable recheck instead of distributed ACID. | A worker restart never loses data; the recheck prevents deleting active rows that changed after the snapshot was copied; no MSDTC or saga coordinator is needed. |
