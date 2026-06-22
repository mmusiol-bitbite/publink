# Import And Processing Sequences

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit engineering |
| Sources | Legacy importer, mapper, MassTransit config, processing consumer |
| Confidence | High |
| Related | [Data Flow](../../architecture/data-flow.md), [Events](../../api/events.md) |

## Happy Path

```mermaid
sequenceDiagram
    participant Worker as Ingestion worker
    participant Legacy as Legacy SQL
    participant Bus as Service Bus
    participant Processor as Processing worker
    participant DB as AuditReadModel
    Worker->>DB: Read checkpoint
    Worker->>Legacy: Read rows after checkpoint
    Worker->>Bus: Publish AuditEntryImportedV1
    Worker->>DB: Save checkpoint
    Bus->>Processor: Deliver event
    Processor->>DB: Check Source + SourceEventId
    Processor->>DB: Append canonical event
    Processor->>DB: Update projections
    Processor->>DB: Commit
```

The happy path shows two separate safety points: ingestion advances `import_checkpoints` only after publishing imported events, and processing commits canonical event/projection writes only after idempotency checks pass.

## Failure/Retry

```mermaid
sequenceDiagram
    participant Bus as Service Bus
    participant Processor as Processing worker
    participant DB as AuditReadModel
    Bus->>Processor: Deliver event
    Processor->>DB: Write fails
    Processor--xBus: Throw
    Bus->>Processor: Exponential retry
    alt exhausted
        Bus->>Bus: Move to DLQ
    else succeeds
        Processor->>DB: Commit
    end
```

This failure path depends on at-least-once messaging. A failed database write causes MassTransit retry/redelivery; when retry is exhausted the message goes to the broker DLQ for operational handling.

## Duplicate

```mermaid
sequenceDiagram
    participant Bus as Service Bus
    participant Processor as Processing worker
    participant DB as AuditReadModel
    Bus->>Processor: Redeliver event
    Processor->>DB: Existing key found
    Processor-->>Bus: Complete without projection update
```

Duplicate handling is based on the unique `(Source, SourceEventId)` business key. Redelivery can happen, but the existing canonical event prevents a second timeline/search projection effect.