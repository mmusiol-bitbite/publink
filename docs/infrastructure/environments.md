# Environments

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit DevOps/SRE |
| Sources | Docker Compose, debug overlays, CI workflow |
| Confidence | High for local/CI; Low for production |
| Related | [Docker](docker.md), [Deployment](deployment.md) |

Implemented environments:

| Environment | Evidence |
| --- | --- |
| Local/demo | `docker/docker-compose.yml` |
| Local debug | `docker/docker-compose.debug-*.yml` |
| CI | `.github/workflows/ci.yml` |

Not present: staging config, production config, CD, IaC, Kubernetes/Helm, registry, DNS/TLS/network resources. Production and DR environments are: Assumption – requires validation.