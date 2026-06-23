# Import And Processing Sequences

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit engineering |
| Sources | Legacy importer, mapper, MassTransit config, processing consumer |
| Confidence | High |
| Related | [Data Flow](../../architecture/data-flow.md), [Events](../../api/events.md) |

## Happy Path

```mermaid
sequenceDiagram
    participant Worker as ingestion-worker
    participant Legacy as Legacy SQL
    participant DB as AuditReadModel
    participant Bus as Service Bus
    participant Processor as processing-worker
    Worker->>DB: Read import_checkpoints
    Worker->>Legacy: Read audit rows after checkpoint position
    Worker->>Bus: Publish AuditEntryImportedV1
    Worker->>DB: Save checkpoint (at-least-once guarantee)
    Bus->>Processor: Deliver AuditEntryImportedV1
    Processor->>DB: Check (Source, SourceEventId) — idempotency
    Processor->>DB: Append to audit_events
    Processor->>DB: Update contract_search, contract_search_aliases, contract_timeline_items
    Processor->>DB: Commit
```

The checkpoint is saved *after* publish, not before. This is the at-least-once guarantee: if the worker crashes between publish and checkpoint save, the same rows will be re-published on the next run and the processing consumer's idempotency check (`(Source, SourceEventId)`) will silently discard the duplicate. No audit event is lost; at worst it is delivered twice and deduplicated.

> **Business implication (happy path).** The treasurer's timeline and search data reflect all imported changes. Freshness depends on the polling interval (configurable, default 1 hour) or a manual synchronisation request.

## Failure/Retry

```mermaid
sequenceDiagram
    participant Bus as Service Bus
    participant Processor as processing-worker
    participant DB as AuditReadModel
    Bus->>Processor: Deliver AuditEntryImportedV1
    Processor->>DB: Write fails
    Processor--xBus: Throw
    Bus->>Processor: Exponential retry
    alt retry exhausted
        Bus->>Bus: Move to DLQ (audit-projection dead-letter)
    else retry succeeds
        Processor->>DB: Commit
    end
```

> **Business implication (failure path).** If a message exhausts all retries and moves to the DLQ, the status endpoint (`GET /api/v1/status`) increments its DLQ counter. The operator sees that missing audit data is flagged rather than silently absent, and can investigate without guessing whether the data was ever imported.

## Duplicate

```mermaid
sequenceDiagram
    participant Bus as Service Bus
    participant Processor as processing-worker
    participant DB as AuditReadModel
    Bus->>Processor: Redeliver AuditEntryImportedV1
    Processor->>DB: (Source, SourceEventId) already exists
    Processor-->>Bus: Complete without projection update
```

> **Business implication (duplicate path).** Redelivery is safe. The treasurer's timeline and search results are unaffected by duplicate delivery because the second message produces no projection change.
