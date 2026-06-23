# Runbook

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit SRE |
| Sources | Health endpoints, synchronization endpoints, MassTransit config |
| Confidence | High for local/demo; Low for production operations |
| Related | [Troubleshooting](troubleshooting.md), [Observability](../infrastructure/observability.md) |

## Health

| Endpoint | Expected |
| --- | --- |
| `GET /health/live` | `200`, `status=healthy` |
| `GET /health/ready` | `200`, `status=ready` when database and sync queue are reachable |

## Manual Synchronization

1. Call `POST /api/v1/synchronization/requests`.
2. Query API acquires or joins a lease in `legacy_synchronization_requests`.
3. Query API sends `RequestLegacySynchronizationV1`.
4. Ingestion worker imports to current and completes the lease.
5. Poll `GET /api/v1/synchronization/status` until `lastSynchronizedAt >= acceptedAt`.

Manual sync is rate-limited and may return `joined=true`.

## Projection Failure

Check `processing-worker` logs, synchronization status and DLQ count. Retry and DLQ are implemented; replay tooling is not present: Assumption – requires validation.

## Archive Failure

Archive endpoint failures should not block active search if active DB is healthy. Check `archival-worker` logs and `contract_archive_transfers` state. Production alerting is not implemented.
