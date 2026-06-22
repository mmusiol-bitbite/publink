# Application Flows Cheatsheet - EN

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit architecture / technical writing |
| Sources | `docs/README.md`, `docs/architecture/backend-object-catalog.md`, `docs/diagrams/sequence/checkpoint-state-duplicates.md`, `docs/api/rest-api.md`, backend projects |
| Confidence | High for implemented flows; Medium for legacy business semantics |
| Related | [Polish version](application-flows-cheatsheet-pl.md), [Solution Walkthrough](solution-walkthrough.md), [Backend Object Catalog](../architecture/backend-object-catalog.md), [Audit Storage ERD](../diagrams/erd/audit-storage.md) |

This cheatsheet describes the main flows in Publink Audit. Each flow explains step by step what happens in the system, which components participate and which tables/states matter.

Publink Audit is not a classic CRUD application over the legacy database. It is a read-model audit explorer: data comes from legacy SQL, is converted into canonical events, stored in `audit_events`, projected into search/timeline tables and then read by the API and frontend.

## 1. Legacy SQL Import Flow

1. `Audit.Ingestion.Worker` runs periodically as a background worker.
2. The worker reads the last imported `SourceEventId` from `import_checkpoints`.
3. It asks legacy SQL for audit rows after that checkpoint.
4. Each legacy row becomes a `LegacyAuditRecord`.
5. `LegacyAuditEventMapper` maps the record into `AuditEntryImportedV1`.
6. The mapper generates a deterministic `EventId` from `Source` and `SourceEventId`.
7. `Audit.Ingestion.Worker` publishes `AuditEntryImportedV1` to Service Bus.
8. After publishing, the worker saves the new checkpoint in `import_checkpoints`.
9. If the batch is empty, the checkpoint remains unchanged.
10. After a sweep, the worker can mark the source as synchronized.
11. The legacy row is now represented as an event to process.

Meaning: import separates legacy SQL from the rest of the system. Query API and the frontend do not read the legacy database directly.

## 2. Processing And Projection Flow

1. `Audit.Processing.Worker` listens on `audit-projection`.
2. Service Bus delivers `AuditEntryImportedV1`.
3. `AuditEntryImportedConsumer` checks whether `(Source, SourceEventId)` already exists.
4. If it exists, the result is `Duplicate` and projections are not updated.
5. If the event is new, the worker stores it in `audit_events`.
6. `audit_events` stores the canonical imported event inside Publink Audit.
7. The worker updates `contract_timeline_items` with a history row.
8. The worker updates `contract_search` with current searchable fields.
9. The worker updates `contract_search_aliases` with historical values.
10. The unit of work commits the changes.
11. If persistence fails, MassTransit retries.
12. After retry exhaustion, the message can move to the DLQ.

Meaning: processing turns events into a readable model for API, UI and export.

## 3. Duplicate Handling Flow

1. The system assumes at-least-once delivery, so a message can arrive more than once.
2. Duplicates can happen because of retry, broker redelivery or reimport.
3. The idempotency key is `(Source, SourceEventId)`.
4. Processing first attempts to append the canonical event.
5. If the event already exists, `CanonicalAuditEventPersister` returns `Duplicate`.
6. The worker does not add a second timeline item.
7. The worker does not update `contract_search` or aliases again.
8. The message is considered handled.

Meaning: retry does not create duplicate change history.

## 4. Active Contract Search Flow

1. The user types a phrase in the React SPA.
2. The frontend calls `GET /api/v1/contracts/search`.
3. `Audit.Query.Api` validates the request.
4. The API does not query legacy SQL.
5. The API reads the active read model through Dapper.
6. Search uses `contract_search` and `contract_search_aliases`.
7. `contract_search` contains current searchable contract data.
8. `contract_search_aliases` contains historical values.
9. The API returns a list of `ContractSearchResult` items.
10. The frontend shows results and opens timeline after the user selects a contract.

Meaning: search is fast because it reads a dedicated projection, not legacy SQL or full event history.

## 5. Contract Timeline Flow

1. The user selects a contract.
2. The frontend calls `GET /api/v1/contracts/{contractId}/audit-events`.
3. The request can include filters: date range, actor, change type, entity type, limit and cursor.
4. The API validates filters.
5. Query API reads from `contract_timeline_items`.
6. Timeline items are already projected, so the API does not rebuild history from legacy rows.
7. The response contains event ID, sequence, date, correlation ID, change/entity kind, actor, field changes and data-quality issues.
8. The API returns `TimelinePage`.
9. If more data exists, the response includes `nextCursor`.
10. `nextCursor` is an API pagination cursor, not an import checkpoint.

Meaning: timeline shows contract change history ready for review, filtering and export.

## 6. Audit History Export Flow

1. The user chooses to export contract history.
2. The frontend calls `GET /api/v1/contracts/{contractId}/audit-events/export`.
3. Export accepts timeline-compatible filters and `locale`, for example `pl` or `en`.
4. The API validates the request.
5. `ContractAuditExportService` reads the timeline from the read model.
6. If the result has more than 10,000 events, the API returns `413 exportTooLarge`.
7. If the limit is acceptable, a ZIP is generated.
8. The ZIP contains `audit.csv`, `manifest.json` and `checksums.sha256`.
9. The API adds the `X-Content-SHA256` header.
10. The frontend downloads the package as a file.

Meaning: export provides a portable audit-history package. It is not a full legal evidence system because WORM storage, signatures and trusted timestamps are outside MVP scope.

## 7. Manual Synchronization Flow

1. A user or operator starts synchronization.
2. The frontend/API calls `POST /api/v1/synchronization/requests`.
3. Query API creates or joins a lease in `legacy_synchronization_requests`.
4. If sync is already running, the API returns `joined = true`.
5. The API sends `RequestLegacySynchronizationV1` to Service Bus.
6. The API returns `202 Accepted`.
7. `Audit.Ingestion.Worker` consumes the command from `legacy-synchronization`.
8. The worker imports up to the current point in legacy SQL.
9. The worker publishes `AuditEntryImportedV1` for new rows.
10. After completion, it marks the request as completed.
11. Status is available through `GET /api/v1/synchronization/status`.

Meaning: manual sync lets operators force catch-up with the legacy source without calling workers over HTTP.

## 8. Archival Flow

1. `Audit.Archival.Worker` runs periodically.
2. The worker finds contracts inactive longer than the configured period.
3. For a candidate, it creates or updates `contract_archive_transfers`.
4. The transfer enters `Copying`.
5. The worker loads data from `contract_search`, `contract_search_aliases`, `contract_timeline_items` and `audit_events`.
6. Those rows form an archive snapshot.
7. The snapshot is written to archive DB as `archived_contracts`, `archived_contract_aliases`, `archived_timeline_items` and `archived_audit_events`.
8. The worker verifies the snapshot.
9. After verification, the transfer enters `Verified`.
10. Before deleting active data, the worker performs a serializable recheck.
11. If the contract did not change, active rows are deleted and the transfer enters `Archived`.
12. If the contract changed meanwhile, the snapshot is deleted and the transfer returns to `Active`.
13. If an error occurs, the transfer enters `Failed` and stores `ErrorCode`.

Meaning: archival moves inactive contracts from hot storage to archive storage without a distributed ACID transaction.

## 9. Archive Read Flow

1. The user opens archive view.
2. The frontend calls `/api/v1/archive/...` endpoints.
3. The API uses the archive read source instead of the active read source.
4. Archive search reads from `archived_contracts` and `archived_contract_aliases`.
5. Archive timeline reads from `archived_timeline_items`.
6. Event history is preserved in `archived_audit_events`.
7. Archive export uses the same concepts as active export.

Meaning: archive is a separate read model, not just a flag in an active table.

## 10. Health, Status And Operations Flow

1. `GET /health/live` checks whether the process is alive.
2. `GET /health/ready` checks dependency readiness.
3. `GET /api/v1/synchronization/status` reads `import_checkpoints` and `legacy_synchronization_requests`.
4. Status shows the last source event ID, synchronization time and active/latest manual sync.
5. Status also reads DLQ count for `audit-projection`.
6. MassTransit `InboxStates`, `OutboxMessages` and `OutboxStates` are technical reliability tables, not audit history.

Meaning: status/health helps operators distinguish import, processing, broker, database and archival problems.

## Summary

Main flow:

```text
legacy SQL row
  -> LegacyAuditRecord
  -> AuditEntryImportedV1
  -> Service Bus
  -> Audit.Processing.Worker
  -> audit_events
  -> contract_search / contract_search_aliases / contract_timeline_items
  -> Audit.Query.Api
  -> React SPA / export ZIP
```

Most important process rules:

1. Legacy SQL remains the input data source.
2. `Audit.Ingestion.Worker` owns polling and checkpoints.
3. `AuditEntryImportedV1` is the canonical transport event.
4. `audit_events` stores canonical imported events in the active store.
5. Search and timeline are projections, not direct reads from legacy rows.
6. Duplicates are safe because of `(Source, SourceEventId)`.
7. Manual sync is an asynchronous command through Service Bus.
8. Archival copies, verifies, rechecks and only then deletes hot rows.
9. Archive DB has separate snapshots for search, timeline and export.
10. MassTransit inbox/outbox tables are reliability infrastructure, not audit data.
