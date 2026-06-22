# Deployment

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit DevOps/SRE |
| Sources | Dockerfiles, Docker Compose, CI workflow, startup migrations |
| Confidence | High for local/demo; Low for production |
| Related | [Docker](docker.md), [CI/CD](ci-cd.md), [Environments](environments.md) |

Implemented deployment:

```powershell
docker compose -f docker/docker-compose.yml up --build
```

Backend Dockerfile takes `PROJECT` build argument and publishes one .NET deployable. Frontend Dockerfile builds Vite output and serves it with Nginx.

Backend startup runs database initialization/migrations through `DatabaseStartupInitializer` where registered.

No production rollback, CD, registry, IaC, blue/green, canary or migration expand/contract process is defined: Assumption – requires validation.