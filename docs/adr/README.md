# Architecture Decision Records

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture |
| Sources | Code/config analysis and generated documentation |
| Confidence | High |
| Related | [System Overview](../architecture/system-overview.md) |

| ADR | Status | Decision |
| --- | --- | --- |
| [ADR-0001](adr-0001-modular-worker-architecture.md) | Accepted | Split backend into API and workers. |
| [ADR-0002](adr-0002-legacy-sql-polling-anti-corruption-layer.md) | Accepted | Poll legacy SQL through an ingestion adapter. |
| [ADR-0003](adr-0003-at-least-once-messaging.md) | Accepted | Use at-least-once broker delivery and idempotent effects. |
| [ADR-0004](adr-0004-mssql-active-archive-persistence.md) | Accepted | Use MSSQL active/archive stores. |
| [ADR-0005](adr-0005-canonical-events-and-projections.md) | Accepted | Store canonical events and query projections. |
| [ADR-0006](adr-0006-contract-level-archival.md) | Accepted | Archive by contract boundary. |
| [ADR-0007](adr-0007-static-demo-tenant.md) | Accepted for MVP | Use static tenant until identity exists. |
| [ADR-0008](adr-0008-api-and-contract-versioning.md) | Accepted | Use `/api/v1` and V1 message contracts. |
| [ADR-0009](adr-0009-docker-compose-runtime.md) | Accepted for local/demo | Use Docker Compose as runtime baseline. |
| [ADR-0010](adr-0010-quality-gates.md) | Accepted | Enforce backend/frontend/container CI gates. |