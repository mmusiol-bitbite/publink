# CI/CD

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit DevOps |
| Sources | `.github/workflows/ci.yml`, package scripts, build props |
| Confidence | High |
| Related | [Deployment](deployment.md), [Quality Attributes](../architecture/quality-attributes.md) |

Workflow `quality-gate` runs on pull requests and pushes to `main`.

Jobs:

- Backend: restore, format verification, Release build, Release tests on .NET 10.
- Frontend: `npm ci`, lint, Prettier check, Vitest, build, production dependency audit on Node 22.
- Compose: `docker compose config --quiet` and `docker compose build`.

Not implemented: deployment, registry push, release tags, migration gates, SBOM/license/container scanning and branch policy documentation. CD/release strategy is: Assumption – requires validation.