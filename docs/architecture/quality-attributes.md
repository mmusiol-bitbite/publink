# Quality Attributes

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture/SRE |
| Sources | Build props, CI workflow, middleware, MassTransit, archive executor |
| Confidence | High for implemented mechanisms; Low for measured production SLOs |
| Related | [Observability](../infrastructure/observability.md), [Security](../infrastructure/security.md) |

| Attribute | Implemented | Gap |
| --- | --- | --- |
| Maintainability | Modular projects, warnings as errors, nullable enabled | No architecture tests found. |
| Reliability | Retry, DLQ, idempotency, archive recheck | No DLQ replay tooling. |
| Performance | Dapper reads, indexes, cursor pagination | No load-test results. |
| Security | Headers, CSP, rate limiting | No auth/authz. |
| Operability | Health, trace ID, OpenTelemetry registration | No dashboards/alerts. |
| Portability | Docker Compose | No production IaC/orchestrator. |

Production SLO/SLA values are not defined: Assumption – requires validation.