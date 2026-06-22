# Documentation Gap Analysis

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture / SRE / technical writing |
| Sources | Full source/configuration analysis, old docs review, generated documentation set |
| Confidence | High for current-state gaps; Medium for recommendations |
| Related | [README](README.md), [ADR index](adr/README.md), [Security](infrastructure/security.md) |

## Documented Areas

| Area | Status |
| --- | --- |
| Business overview | Documented in README and presentation. |
| Backend architecture | Documented in architecture/API/domain/ADR docs. |
| Frontend | Documented through README, dependencies, API and local development. |
| Infrastructure | Docker, config, networking, CI/CD, observability, security, backup and deployment documented. |
| Runtime behavior | Import, sync, projection, query/export and archival flows documented with diagrams. |
| Domain model | Contract audit aggregate, persisted entities and rules documented. |
| API | REST endpoints, events, errors, auth status and versioning documented. |
| ADRs | Ten ADRs generated from code/configuration. |
| Presentation | CTO/architect/team walkthrough generated. |

## Validated From Code And Carried Forward

- Four backend deployables plus shared libraries.
- Event-driven import/projection through MassTransit and Azure Service Bus emulator.
- Active/archive MSSQL split.
- Idempotency using `(Source, SourceEventId)`.
- Contract-level archive with snapshot verification and serializable recheck.
- React/Vite frontend with active/archive API calls.
- Docker Compose local runtime.
- GitHub Actions quality gate.

## Requires Validation

| Area | Gap | Risk |
| --- | --- | --- |
| Authentication/authorization | Not implemented in Query API. | Critical production exposure. |
| Tenant resolution | Static configured organization ID. | No user-claim isolation. |
| Secrets | No production secret store. | Credential rotation/leak risk. |
| Production deployment | No CD/IaC/Kubernetes/registry. | Manual/undefined deployment. |
| Observability | No collector/dashboards/alerts. | Failures may go unnoticed. |
| Backup/DR | No RPO/RTO or restore drill. | Recovery uncertainty. |
| DLQ operations | No replay/admin tooling. | Poison messages require manual handling. |
| Performance | No load tests or measured SLOs. | Unknown capacity. |
| Legal retention/evidence | Export says not qualified evidence; policy absent. | Compliance mismatch. |
| Legacy semantics | Append-only/order/timezone guarantees outside repo. | Import correctness dependency. |

## Recommendations

1. Add production identity and authorization design.
2. Define IaC/CD, registry and deployment rollback.
3. Implement secrets management and least-privilege identities.
4. Add observability backend, alerts and SLO dashboards.
5. Define backup/restore, RPO/RTO and retention/legal policy.
6. Add DLQ replay runbook and tooling.
7. Add load tests and production readiness gates.

All unverifiable production statements remain: Assumption – requires validation.