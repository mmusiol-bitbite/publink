# System Overview

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit architecture |
| Sources | Backend solution, frontend app, Docker Compose |
| Confidence | High |
| Related | [Context](context-diagram.md), [Container](container-diagram.md), [Backend Object Catalog](backend-object-catalog.md), [Dependencies](dependencies.md) |

Publink Audit uses a modular event-driven read-model architecture. Legacy ingestion, projection, query and archival are separate runtimes in one .NET solution. The frontend is a React SPA served by Nginx.

This is not event sourcing. Canonical audit events are imported from legacy SQL and used to build read projections.

For a backend object-by-object breakdown with responsibilities, states and process meaning, see [Backend Object Catalog](backend-object-catalog.md).

## Extensibility And Future Services

The system is intentionally simple to extend when new audit services, processing steps or audited business objects appear. The current shape keeps runtime responsibilities isolated while sharing contracts, domain codes, persistence adapters, messaging setup and observability in common libraries.

Adding a new deployable service should follow the existing worker/API pattern:

1. Create a new deployable project under `src/backend`, for example another worker or API host.
2. Reference only the required shared projects: `Audit.Contracts` for messages, `Audit.Domain` for audit codes and value logic, `Audit.Application` for use cases/ports and `Audit.Infrastructure` for SQL, broker and telemetry adapters.
3. Keep service-specific composition in the deployable project: hosted services, consumers, endpoint registration, options binding and health checks.
4. Reuse shared infrastructure registration methods such as persistence, messaging and observability extensions instead of duplicating setup code.
5. Add the service to the solution, Docker Compose/runtime configuration and CI checks when it must run as part of the delivered system.
6. Cover the new boundary with focused tests: application behavior for use cases and infrastructure tests for SQL/broker adapters.

Adding a new audited object or entity type should extend the canonical audit model without changing the service topology by default:

1. Assign or confirm the source `EntityTypeCode` and add the known mapping in `Audit.Domain.AuditedEntityKind` when the object should be displayed as a first-class known entity.
2. Keep imported data on `AuditEntryImportedV1`: entity identity stays in `AuditedEntityV1`, operation identity stays in `AuditOperationV1` and before/after snapshots stay as JSON payloads.
3. Update the legacy mapper only if the source representation changes; simple new entity codes can flow through the existing mapper.
4. Extend projection/persistence logic only when the object needs new searchable fields, timeline enrichment, archive behavior or API response shape.
5. Add or version API/event contracts when consumers need a changed public shape; additive internal handling can stay behind the existing `V1` message if the wire contract remains compatible.
6. Update tests and documentation for the new entity code, fields, query behavior and archive expectations.

This means growth is usually additive: a new object can often be introduced through domain-code mapping and projection updates, while a new service can be introduced as another small runtime composed from the existing shared libraries. A larger architectural change is needed only when the new capability requires a new public contract, a different storage model, a separate scaling boundary or a separate operational lifecycle.

External integrations implemented or configured:

- Legacy SQL source through `ConnectionStrings:LegacySource`.
- MSSQL active/archive stores.
- Azure Service Bus through MassTransit; emulator in Docker Compose.
- OTLP exporter when `OTEL_EXPORTER_OTLP_ENDPOINT` is set.

Not implemented in repository: production auth/authz, Kubernetes, IaC, CD, production monitoring, legal evidence workflow and production backup policy.