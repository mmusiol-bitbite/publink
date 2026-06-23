# ADR-0006: MassTransit Transactional Outbox/Inbox

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit architecture/SRE |
| Sources | `Audit.Processing.Worker/Bootstrap/MessagingServiceRegistration.cs`, `Audit.Infrastructure/Persistence/Contexts/AuditDbContext.cs` |
| Confidence | High |
| Related | [ADR-0005](adr-0005-at-least-once-messaging.md), [ADR-0009](adr-0009-ef-core-dapper-cqs-split.md) |

## Status

Accepted.

## Date

2026-06-20.

## Context

The processing worker appends a canonical event to `audit_events` and updates `contract_search`, `contract_search_aliases` and `contract_timeline_items` projection tables in a single database transaction. Without an outbox, a crash between the database commit and the broker acknowledgement would cause the message to be redelivered and a duplicate projection write attempt, relying entirely on the `(Source, SourceEventId)` idempotency check to avoid corruption. Without an inbox, concurrent redelivery could bypass that check under race conditions.

## Problem

Idempotency keys protect against duplicate effects, but they do not make broker acknowledgement atomic with the database commit. A crash between commit and ack creates a redelivery that must be handled correctly. An inbox removes the race window; an outbox (used for broker-initiated publishes, e.g. synchronisation commands) ensures broker publication is committed to the database before it is dispatched to the broker.

## Considered Options

- **No outbox/inbox; rely solely on idempotency key.** Simpler, no extra tables. Leaves a redelivery window between commit and ack; race conditions under high redelivery are possible.
- **Custom outbox implementation.** Full control, no MassTransit dependency for this pattern. Higher implementation cost and maintenance burden.
- **MassTransit `AddEntityFrameworkOutbox<AuditDbContext>`.** Integrates with the existing EF Core context. Adds `OutboxMessages`, `OutboxStates` and `InboxStates` tables to `AuditReadModel`. Broker delivery and database commit are atomic from the application's perspective.

## Decision

Use `AddEntityFrameworkOutbox<AuditDbContext>` with `UseBusOutbox()` in the processing worker. The three MassTransit state tables (`InboxStates`, `OutboxMessages`, `OutboxStates`) are part of `AuditDbContext` and are created by EF Core migrations. These tables are broker reliability infrastructure and contain no business audit data.

## Consequences

- Broker publish and database commit are atomic; a crash between them is safe for both producer and consumer.
- Three additional tables are present in `AuditReadModel`; they must be excluded from any business data export or backup restoration logic.
- MassTransit outbox introduces a brief delivery delay (configurable query interval, set to 1 s in Docker Compose).
- Outbox cleanup and dead-letter replay runbooks are not implemented: Assumption – requires validation.
- This decision is distinct from ADR-0005: ADR-0005 addresses at-least-once delivery and idempotency at the business key level; this ADR addresses atomicity between database state and broker state.
