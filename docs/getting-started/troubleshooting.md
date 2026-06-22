# Troubleshooting

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit SRE |
| Sources | Docker Compose, API error handling |
| Confidence | High for local/demo |
| Related | [Runbook](runbook.md), [Error Handling](../api/error-handling.md) |

## Readiness Fails

Check MSSQL, Service Bus emulator and Query API logs:

```powershell
docker compose -f docker/docker-compose.yml ps
docker compose -f docker/docker-compose.yml logs mssql
docker compose -f docker/docker-compose.yml logs servicebus-emulator
docker compose -f docker/docker-compose.yml logs query-api
```

The Service Bus emulator image is distroless, so readiness is checked by the `servicebus-ready` sidecar healthcheck against `http://servicebus-emulator:5300/health`. If the emulator stays unhealthy, inspect both the emulator logs and the helper container health status.

## Sync Accepted But Not Completed

Likely causes: ingestion worker stopped, legacy connection missing, queue delivery failure or unsupported source. Check `ingestion-worker` logs.

## Search Returns No Results

Verify query length is at least 2 characters, synchronization status is healthy and processing worker projected events. Search archived contracts through `/api/v1/archive/contracts/search`.

## Export Returns 413

The export service caps matching events at 10,000. Narrow filters by date, actor, change type or entity type.

## Browser Cannot Reach API In Dev

Set `VITE_API_PROXY_TARGET=http://localhost:8080` before running Vite.

## Testcontainers Cannot Connect To Docker

`Audit.Infrastructure.Tests` require Docker Desktop. If tests fail with `Failed to connect to Docker endpoint at 'npipe://./pipe/docker_engine'`, start Docker Desktop and verify:

```powershell
docker desktop status
docker info
```

The expected status is `running`, and `docker info` should show a `Server` section.

## Authentication Surprise

No app authentication or authorization is implemented. Current tenant is static configuration.