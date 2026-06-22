# Events And Contracts

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit integration engineering |
| Sources | `Audit.Contracts`, MassTransit registration, consumers |
| Confidence | High |
| Related | [Sync vs Async](../architecture/sync-vs-async.md), [Versioning](versioning.md) |

## `AuditEntryImportedV1`

Purpose: canonical imported audit event from ingestion worker to processing worker.

Broker behavior:

- Entity name `audit-entry-imported-v1`.
- Consumer endpoint `audit-projection`.
- Default TTL 1 hour.
- Max delivery count 5.
- Exponential retry: 3 retries, 200ms to 5s.
- DLQ transports enabled.

Fields: `EventId`, `SchemaVersion`, `Source`, `SourceEventId`, `SourceSequence`, `OrganizationId`, `OccurredAt`, `IngestedAt`, `Actor`, `Operation`, `Entity`, `Before`, `After`, `ChangedFields`.

Idempotency is based on deterministic event ID and unique `(Source, SourceEventId)` in active/archive event tables.

## `RequestLegacySynchronizationV1`

Purpose: command from Query API to ingestion worker to run import to current.

Broker behavior:

- Queue `legacy-synchronization`.
- Ingestion receive endpoint concurrency limit 1.
- Max delivery count 5.
- Exponential retry and DLQ enabled.

Fields: `CorrelationId`, `Source`, `RequestedAt`, `SchemaVersion`.

## Compatibility

Contracts include `V1` type suffix and `SchemaVersion`. No schema registry, upcaster or multi-version consumer exists: Assumption – requires validation.