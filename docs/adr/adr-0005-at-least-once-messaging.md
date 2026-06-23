# ADR-0005: At-Least-Once Messaging

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit architecture/SRE |
| Sources | MassTransit registrations, consumers, persistence indexes |
| Confidence | High |
| Related | [Events](../api/events.md), [Sync vs Async](../architecture/sync-vs-async.md) |

## Status

Accepted.

## Date

2026-06-19.

## Context

Azure Service Bus/MassTransit can redeliver messages.

## Problem

Duplicate delivery must not create duplicate audit/projection effects.

## Considered Options

Exactly-once broker semantics; at-least-once with idempotency; synchronous calls.

## Decision

Use at-least-once delivery with bounded retry, DLQ and idempotent local effects through `(Source, SourceEventId)`.

## Consequences

Duplicate events are safe. DLQ replay and production monitoring are not implemented: Assumption – requires validation.