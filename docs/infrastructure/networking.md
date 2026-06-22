# Networking

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit DevOps/SRE |
| Sources | Docker Compose, Nginx, Vite config |
| Confidence | High for local/demo |
| Related | [Docker](docker.md), [Security](security.md) |

Docker Compose uses the default project network. Services communicate by service name: `query-api`, `mssql`, `servicebus-emulator`.

Host ports: `3000`, `8080`, `1433`, `5672`, `5300`.

Vite can proxy `/api` and `/health` to `VITE_API_PROXY_TARGET`. Debug Nginx can route traffic to `host.docker.internal:55291`.

TLS, DNS, WAF, ingress, private endpoints and firewall rules are not defined: Assumption – requires validation.