# Backend Object Catalog

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit architecture / backend engineering |
| Sources | `src/backend/Audit.*` projects, persistence entities, workers, consumers, query/export services |
| Confidence | High for implemented objects; Medium for legacy business semantics |
| Related | [System Overview](system-overview.md), [Audit Domain](../domains/audit-domain.md), [Audit Storage ERD](../diagrams/erd/audit-storage.md), [Checkpoint And State Flow](../diagrams/sequence/checkpoint-state-duplicates.md) |

This catalog describes backend objects that carry process meaning: deployables, messages, domain values, application services, persistence state and infrastructure adapters. Trivial bootstrap, generated migration and test helper files are intentionally not expanded here.

## Runtime Objects

| Object | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `Audit.Query.Api` | `Audit.Query.Api` | Hosts REST endpoints, health, Swagger, search, timeline, export and manual synchronization request handling. | Healthy/unhealthy through health checks; endpoint outcomes include success, validation error, store unavailable and export too large. | Synchronous read boundary for UI and operators. It never reads legacy SQL directly. |
| `Audit.Ingestion.Worker` | `Audit.Ingestion.Worker` | Runs scheduled legacy import and consumes manual sync commands. | Scheduled cycle idle/running; manual sync lease created or joined; import batch published or fails and retries on next cycle/command. | Moves source audit rows into broker messages and advances `import_checkpoints`. |
| `Audit.Processing.Worker` | `Audit.Processing.Worker` | Consumes imported audit events and builds the active read model. | `Appended` or `Duplicate`; MassTransit retry/DLQ on unhandled failure. | Owns idempotent canonical event persistence and projection updates. |
| `Audit.Archival.Worker` | `Audit.Archival.Worker` | Periodically archives inactive contracts and supports reactivation/recovery paths through lifecycle services. | Uses archive transfer states: `Active`, `Copying`, `Verified`, `Archived`, `ReactivationPending`, `ReactivatedCopied`, `Failed`. | Moves inactive contract history from hot read model to archive snapshots without distributed transactions. |

## Message And Contract Objects

| Object | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `AuditContract` | `Audit.Contracts` | Base message contract type. | No runtime state. | Common marker for versioned integration messages. |
| `AuditEntryImportedV1` | `Audit.Contracts` | Canonical imported audit event sent from ingestion to processing. | Delivered, retried, dead-lettered or consumed; persistence outcome is `Appended`/`Duplicate`. | Main event-shaped handoff from legacy polling to read-model projection. |
| `ActorSnapshotV1` | `Audit.Contracts` | Captures source actor identity data. | May contain user ID, email or partial/unknown data. | Preserves who changed data as known by the legacy source. |
| `AuditOperationV1` | `Audit.Contracts` | Captures correlation ID and source change type. | Change type is interpreted by `ChangeKind`; unknown codes are tolerated. | Links audit rows belonging to one operation and explains operation kind. |
| `AuditedEntityV1` | `Audit.Contracts` | Captures source entity type, entity ID, parent ID and primary key. | Entity type may be known or unknown; contract ID may be resolved or unresolved later. | Identifies which audited object changed and how it relates to contract history. |
| `RequestLegacySynchronizationV1` | `Audit.Contracts` | Command from Query API to ingestion worker. | Sent, retried, dead-lettered or consumed; lease is completed after import sweep. | Triggers import-to-current without exposing worker HTTP endpoints. |

## Domain Objects

| Object | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `ChangeKind` | `Audit.Domain` | Maps source change type codes to known semantic keys. | Known codes `1-3`; unknown values are represented without failing reads. | Allows UI/API to display stable change categories while tolerating legacy surprises. |
| `AuditedEntityKind` | `Audit.Domain` | Maps source entity type codes to known audited entity names. | Known codes `1-7`; unknown values remain data-quality issues rather than hard failures. | Defines which source object changed: contract header, annexes, files, invoices and related entities. |
| `FieldChange` | `Audit.Domain` | Represents one before/after field difference. | Can hold before value, after value and source field name. | Atomic unit shown in timeline and export rows. |
| `ChangeSet` | `Audit.Domain` | Groups field changes plus data-quality issues. | Complete, partial, or issue-bearing when source JSON/codes are malformed. | Lets timeline reads surface imperfect legacy data without dropping whole events. |
| `FieldChangeFactory` | `Audit.Domain` | Builds field-level differences from before/after payloads and changed-field metadata. | Produces changes and data-quality issues. | Converts raw source JSON into user-readable timeline details. |

## Import And Synchronization Objects

| Object | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `LegacyAuditRecord` | `Audit.Application.Legacy` | Application model for a raw legacy audit row. | No lifecycle state; row is before mapping. | Anti-corruption input model between legacy SQL and canonical message. |
| `LegacyAuditEventMapper` | `Audit.Application.Legacy` | Maps `LegacyAuditRecord` into `AuditEntryImportedV1`. | Produces deterministic `EventId`; malformed JSON becomes nullable/issue-bearing downstream. | Normalizes legacy audit data and creates stable idempotency identity. |
| `LegacyAuditImporter` | `Audit.Application.Legacy` | Reads rows after checkpoint, publishes mapped events and saves checkpoint progress. | `ImportBatchResult` with count and last source event ID; empty batch keeps checkpoint. | Main import use case used by scheduled and manual synchronization flows. |
| `ImportBatchResult` | `Audit.Application.Legacy` | Summarizes one import batch. | `PublishedCount` can be zero or positive. | Tells workers whether import progressed and where the source cursor ended. |
| `LegacySynchronizationCoordinator` | `Audit.Ingestion.Worker` | Runs import batches until current and marks sweep completion. | Sweep publishes zero or more events and completes checkpoint synchronization timestamp. | Shared orchestration used by scheduled worker and manual sync consumer. |
| `LegacyImportWorker` | `Audit.Ingestion.Worker` | Background scheduled import loop. | Waiting, running cycle, stopped, or failed/logged cycle. | Keeps the read model eventually current without user action. |
| `RequestLegacySynchronizationConsumer` | `Audit.Ingestion.Worker` | Consumes manual synchronization commands. | Unsupported source failure; successful completion; MassTransit retry/DLQ on failure. | Executes operator/API requested synchronization while preserving single-command endpoint concurrency. |
| `LegacySynchronizationRequestLease` | `Audit.Application.Legacy` | Result of acquiring or joining manual sync state. | `Created = true` means new lease; `false` means joined an existing in-flight lease. | Prevents duplicate manual sync work for the same source/correlation window. |
| `ILegacyAuditReader` / `SqlLegacyAuditReader` | Application port / Infrastructure adapter | Reads legacy rows after source event ID. | Returns ordered batch or fails with source/database error. | Isolates legacy SQL shape from application import logic. |
| `IImportCheckpointStore` / `EfImportCheckpointStore` | Application port / Infrastructure adapter | Reads and writes import cursor state. | Cursor advances by source; synchronized timestamp is updated after sweep. | Prevents rereading old rows as normal work and provides freshness status. |
| `IAuditEventPublisher` / `MassTransitAuditEventPublisher` | Application port / Infrastructure adapter | Publishes imported audit events. | Broker publish succeeds or throws for retry/recovery. | Decouples legacy import from processing/projection. |
| `ILegacySynchronizationRequester` / `MassTransitLegacySynchronizationRequester` | Application port / Infrastructure adapter | Sends manual synchronization command. | Command sent or send failure releases API-acquired lease. | Keeps manual sync asynchronous from the API perspective. |
| `ILegacySynchronizationRequestStore` / `EfLegacySynchronizationRequestStore` | Application port / Infrastructure adapter | Acquires, completes and releases manual synchronization leases. | Created, joined, completed, released/failed. | Backing store for `legacy_synchronization_requests`. |

## Processing And Projection Objects

| Object | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `AuditEntryImportedConsumer` | `Audit.Processing.Worker` | Handles `AuditEntryImportedV1`. | `Duplicate` completes without projection; `Appended` applies projections and commits. | Central idempotent projection handler. |
| `AppendAuditEventResult` | `Audit.Application.Persistence` | Result of canonical event append attempt. | `Appended`, `Duplicate`. | Explicitly models at-least-once delivery effects. |
| `ICanonicalAuditEventPersister` / `CanonicalAuditEventPersister` | Application port / Infrastructure adapter | Writes canonical imported events. | Appended or duplicate by `(Source, SourceEventId)`. | Stores the original imported event representation in `audit_events`. |
| `IContractProjectionPersister` / `ContractProjectionPersister` | Application port / Infrastructure adapter | Applies timeline and search projection updates for an event. | Projection applied after canonical append; skipped for duplicates. | Converts events into query-ready active read-model rows. |
| `ContractSearchProjectionWriter` | `Audit.Infrastructure.Persistence.Persisters` | Updates `contract_search` and `contract_search_aliases`. | Creates/updates current search fields; records current/historical aliases. | Maintains the dedicated search projection, including historical searchable values. |
| `ContractTimelineItemMapper` | `Audit.Infrastructure.Persistence.Persisters` | Converts imported event into timeline entity data. | Can mark contract ID unresolved and attach data-quality issues. | Produces user-facing history rows without failing on imperfect source payloads. |
| `IAuditUnitOfWork` | `Audit.Application.Persistence` | Commits canonical event and projection writes together. | Commit succeeds or throws for retry/DLQ behavior. | Keeps processing local effects atomic inside the active database transaction. |

## Query, Export And API Objects

| Object | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `ContractEndpointMappings` / endpoint classes | `Audit.Query.Api.Endpoints` | Maps active/archive search, timeline, export and synchronization routes. | Returns success, validation error, not found/empty results, export too large or infrastructure errors. | Public HTTP shape for the read model. |
| `SearchQuery`, `TimelineQuery`, `ExportQuery` | `Audit.Query.Api.Endpoints.Requests` | Bind HTTP query parameters. | Valid or rejected by validators. | Keeps API input parsing separate from application read models. |
| `SearchQueryValidator`, `TimelineQueryValidator`, `ExportQueryValidator` | `Audit.Query.Api.Endpoints.Validation` | Enforce request rules. | Valid or validation result with API error. | Prevents invalid reads/exports before hitting storage. |
| `ContractAuditDataSource` | `Audit.Application.Queries` | Selects active/archive query sources. | Active, archive or unavailable store outcome. | Gives use cases a consistent read path over hot and archived stores. |
| `ContractSearchQueryExecutor` | `Audit.Infrastructure.Queries` | Executes search SQL over active/archive projections. | Returns page of `ContractSearchResult` rows. | Reads `contract_search` and alias projections instead of legacy SQL. |
| `ContractTimelineQueryExecutor` | `Audit.Infrastructure.Queries` | Executes cursor-paginated timeline SQL. | Returns `TimelinePage` with `nextCursor` or no further cursor. | Reads projected history in a stable order for UI/API pagination. |
| `SqlContractStore` | `Audit.Infrastructure.Queries` | Implements active/archive contract read operations. | Store available/unavailable; wraps low-level SQL failures. | Shared Dapper-backed query facade for active and archive reads. |
| `SynchronizationStatusReader` | `Audit.Infrastructure.Queries` | Reads checkpoint freshness and sync status. | Reports checkpoint timestamp and in-flight/completed synchronization data. | Powers operational status endpoints. |
| `ServiceBusDeadLetterEventCountReader` | `Audit.Infrastructure.Messaging` | Reads broker DLQ count for projection endpoint. | Count available or broker unavailable. | Exposes processing failure backlog in health/status views. |
| `ContractAuditExportService` | `Audit.Application.Exports` | Builds ZIP package from timeline data. | Export succeeds; `ExportTooLargeException`; storage/export failure. | Produces verifiable audit package for a selected contract. |
| `AuditExportPackage` | `Audit.Application.Exports.Models` | Carries generated ZIP bytes, checksum and metadata. | Completed export artifact. | API response payload for download with integrity metadata. |
| `TimelinePage`, `TimelineItem`, `TimelineFieldChange`, `TimelineFilter` | `Audit.Application.Queries.Models` | Application read models for timeline browsing. | Page may have `nextCursor`; items may include data-quality issues. | Stable shape consumed by API and frontend timeline experience. |
| `ContractSearchResult` | `Audit.Application.Queries.Models` | Application read model for search result rows. | Active/archive result item. | UI-facing contract summary from the search projection. |
| `SynchronizationStatus` | `Audit.Application.Queries.Models` | Application read model for synchronization health/status. | Fresh/stale depends on checkpoint timestamp and in-flight sync state. | Lets operators understand import freshness. |
| `ApiExceptionHandler` | `Audit.Query.Api.Middleware` | Maps exceptions into API error responses. | Known domain/application exceptions or generic error. | Keeps API failure contracts consistent. |
| `ResponseSecurityHeadersMiddleware` | `Audit.Query.Api.Middleware` | Adds response security headers. | No process state. | Baseline HTTP hardening for the demo API. |
| `QueryReadinessHealthCheck` | `Audit.Query.Api.Health` | Verifies query dependencies for readiness. | Healthy/unhealthy. | Container/orchestrator readiness signal. |

## Archival Objects

| Object | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `ContractArchivalWorker` | `Audit.Archival.Worker` | Periodic background loop that invokes archive lifecycle. | Waiting, running, failed/logged cycle. | Drives inactive-contract movement out of active storage. |
| `ContractArchivalEligibilityPolicy` | `Audit.Application.Archiving` | Decides whether a contract is old enough to archive. | Eligible/ineligible based on `LastActivityAt` and inactivity years. | Encodes retention cutoff used before archive transfer starts. |
| `ContractArchivalOptions` | `Audit.Application.Archiving` | Holds archival policy settings. | Configuration object. | Makes retention window configurable. |
| `IContractArchiveLifecycle` / `DatabaseContractArchiveLifecycle` | Application port / Infrastructure adapter | Coordinates copy, verify, recheck, delete, recovery and reactivation paths. | `Active`, `Copying`, `Verified`, `Archived`, `ReactivationPending`, `ReactivatedCopied`, `Failed`. | Main archive state machine implementation backed by SQL. |
| `ContractArchivalExecutor` | `Audit.Infrastructure.Archiving.Execution` | Executes archive movement for eligible active contracts. | Candidate copied/archived, reset to active when changed, or failed. | Performs the copy/verify/recheck/delete workflow. |
| `ContractArchiveTransferStore` | `Audit.Infrastructure.Archiving.Transfers` | Reads/writes `contract_archive_transfers`. | Sets transfer state and error code/timestamps. | Durable archive/reactivation ledger. |
| `ContractArchiveSnapshotStore` | `Audit.Infrastructure.Archiving.Snapshots` | Writes and verifies archive snapshots. | Snapshot replaced, verified or deleted. | Owns archived contract, alias, event and timeline snapshot persistence. |
| `ArchivedContractSnapshot` | `Audit.Infrastructure.Archiving.Snapshots` | In-memory aggregate of archive snapshot rows. | Complete snapshot or missing/inconsistent snapshot. | Moves contract archive data through lifecycle services as one unit. |
| `ArchiveEntityMapper` | `Audit.Infrastructure.Archiving.Mapping` | Maps active entities into archived entities. | Mapping only; no state. | Keeps active/archive schema conversion centralized. |
| `ArchivedContractRestorer` | `Audit.Infrastructure.Archiving.Reactivation` | Restores archived rows to active tables. | Restored or fails. | Used when archived contract needs to become active again. |
| `ArchivedContractReactivator` | `Audit.Infrastructure.Archiving.Reactivation` | Coordinates reactivation cleanup and transfer state changes. | `ReactivationPending` -> `ReactivatedCopied` -> `Active`; may reset active if no snapshot exists. | Handles archive-to-active recovery after new activity or explicit reactivation. |

## Persistence State Objects

| Object / table | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `CanonicalAuditEventEntity` / `audit_events` | `Audit.Infrastructure.Persistence.Entities` | Stores canonical imported audit event fields and JSON payloads. | Unique event exists or append is duplicate. | Durable imported event history in active storage. |
| `ContractSearchEntity` / `contract_search` | `Audit.Infrastructure.Persistence.Entities` | Stores active searchable contract summary. | Updated as header events arrive; absent after full archival. | Dedicated search projection for current values. |
| `ContractSearchAliasEntity` / `contract_search_aliases` | `Audit.Infrastructure.Persistence.Entities` | Stores searchable current and historical field values. | Alias can be current or historical through `IsCurrent`. | Allows search by old contract numbers/names after changes. |
| `ContractTimelineItemEntity` / `contract_timeline_items` | `Audit.Infrastructure.Persistence.Entities` | Stores projected timeline row. | Contract ID resolved/unresolved; data-quality issues may be present. | Read-optimized audit history for UI/export. |
| `ImportCheckpointEntity` / `import_checkpoints` | `Audit.Infrastructure.Persistence.Entities` | Stores source import cursor and update timestamp. | Advances monotonically by source event ID. | Controls legacy polling and freshness status. |
| `LegacySynchronizationRequestEntity` / `legacy_synchronization_requests` | `Audit.Infrastructure.Persistence.Entities` | Stores manual synchronization lease. | In-flight when `CompletedAt` is null; completed when set. | Prevents overlapping manual sync requests and exposes sync status. |
| `ContractArchiveTransferEntity` / `contract_archive_transfers` | `Audit.Infrastructure.Persistence.Entities` | Stores archive/reactivation transfer state and counts. | `Active`, `Copying`, `Verified`, `Archived`, `ReactivationPending`, `ReactivatedCopied`, `Failed`. | Durable workflow state for archive and recovery. |
| `ArchivedContractEntity` / `archived_contracts` | `Audit.Infrastructure.Persistence.Entities` | Stores archived contract header snapshot. | Present in archive after successful snapshot; deleted on reactivation cleanup. | Archive-side searchable contract anchor. |
| `ArchivedContractAliasEntity` / `archived_contract_aliases` | `Audit.Infrastructure.Persistence.Entities` | Stores archived searchable aliases. | Snapshot member. | Keeps archived search independent from hot tables. |
| `ArchivedAuditEventEntity` / `archived_audit_events` | `Audit.Infrastructure.Persistence.Entities` | Stores copied canonical event history. | Snapshot member. | Preserves original imported event representation in archive. |
| `ArchivedTimelineItemEntity` / `archived_timeline_items` | `Audit.Infrastructure.Persistence.Entities` | Stores copied timeline projection rows. | Snapshot member. | Enables archived timeline/export reads. |
| `InboxStates`, `OutboxMessages`, `OutboxStates` | MassTransit EF tables in active context | Store message inbox/outbox delivery state. | Pending/delivered/consumed according to MassTransit internals. | Reliability infrastructure, not business audit data. |

## Shared Infrastructure Objects

| Object | Source | Responsibility | States / outcomes | Process meaning |
| --- | --- | --- | --- | --- |
| `AuditDbContext` | `Audit.Infrastructure.Persistence.Contexts` | EF Core active read-model context. | Migrated/available or unavailable. | Owns active persistence, MassTransit EF tables and migrations. |
| `ArchiveDbContext` | `Audit.Infrastructure.Persistence.Contexts` | EF Core archive database context. | Migrated/available or unavailable. | Owns archive snapshots and archive migrations. |
| `AuditDatabaseInitializer`, `ArchiveDatabaseInitializer`, `DatabaseStartupInitializer` | `Audit.Infrastructure.Persistence.Initialization` / `Audit.Infrastructure.Persistence.Core` | Applies database startup initialization. | Startup succeeds or application fails/readiness fails. | Keeps local/demo databases aligned with code. |
| `SqlConnectionFactory`, `ArchiveSqlConnectionFactory` | `Audit.Infrastructure.Persistence.Core` | Creates Dapper SQL connections. | Connection succeeds or fails. | Separates read SQL execution from connection-string handling. |
| `PersistenceServiceCollectionExtensions` | `Audit.Infrastructure.Persistence.Core` | Registers active/archive persistence services. | Composition only. | Shared DI entry point for deployables. |
| `ServiceBusRegistrationExtensions` | `Audit.Infrastructure.Messaging` | Registers MassTransit transport, retry and DLQ policy. | Composition only; endpoint runtime states are broker-managed. | Centralizes messaging behavior across workers/API. |
| `ServiceBusEmulatorCompatibility` | `Audit.Infrastructure.Messaging` | Adjusts Service Bus settings for emulator constraints. | Configuration-only. | Keeps local demo transport compatible with emulator behavior. |
| `ObservabilityExtensions` | `Audit.Infrastructure.Observability` | Registers OpenTelemetry/logging integration. | Exporter enabled when configured. | Shared telemetry setup for all backend runtimes. |

## Audit Data Model - Discovery Phase Note

The active audit read model (`contract_search`, `contract_timeline_items`, related projections) has evolved through multiple discovery iterations as requirements became clear. Its current shape—normalized into separate projection tables with complex join patterns—reflects the exploration phase rather than final optimized design.

**Production model redesign** would denormalize the schema to eliminate complex SELECT queries:

- **Flatten timeline reads**: Current queries require joins across `audit_events`, timeline item tables and entity mappings to produce a single user-visible history row. A production schema would precompute and store denormalized timeline records, reducing queries from multi-table joins to simple ordered scans.
- **Optimize search projections**: Search currently requires union/join logic over multiple projection tables. A mature design would maintain a single, efficiently indexed search table keyed on contract ID and searchable fields.
- **Simplify contract resolution**: Entity-to-contract mapping logic is scattered across inserts and runtime reads. Production would materialize and maintain a stable entity-to-contract index, eliminating runtime resolution complexity.

This would reduce reliance on raw SQL + Dapper and enable straightforward entity-mapped ORM reads. Current architectural choices (SQL queries in readers/query executors, Dapper for projection and timeline reads) reflect the time cost of discovery and the need to unblock feature work within the MVP timeline. Future iterations would prioritize query simplicity and query-plan performance as part of pre-production hardening.

## Intentionally Skipped Files

The catalog skips backend files that do not add standalone process meaning beyond objects already described above:

- `Program.cs` files and most `Bootstrap/*ServiceRegistration.cs` files: composition roots and DI wiring.
- `*Settings`, `*Options` and simple configuration records: runtime configuration, not business/process state.
- EF migrations, model snapshots and design-time factories: schema/tooling mechanics already represented by persistence entities and the ERD.
- Tests, fixtures and `GlobalUsings.cs` under `src/backend/tests`: validation code, not runtime behavior.
- Simple exception classes: important for error handling, but they do not own backend state.
- Small utility/extension classes such as JSON/date/configuration/SQL helper files: support mapping/query code without owning process state.
- Generated or build-output files under `bin`, `obj` and `.vs`: not source-level backend design objects.
