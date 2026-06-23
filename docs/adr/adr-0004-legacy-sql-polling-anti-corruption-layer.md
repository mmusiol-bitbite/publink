# ADR-0004: Legacy SQL Polling Anti-Corruption Layer

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit integration architecture |
| Sources | Legacy importer, mapper, SQL reader |
| Confidence | High for adapter; Medium for source guarantees |
| Related | [Data Flow](../architecture/data-flow.md) |

## Status

Accepted.

## Date

2026-06-19.

## Context

The source system exposes SQL audit rows and not a native event stream in this repository.

## Problem

The audit system needs to consume legacy data without coupling API/UI reads to legacy schema.

## Considered Options

Direct legacy queries; polling adapter; native outbox/event publication.

## Decision

Use ingestion worker polling with checkpoints and mapping to `AuditEntryImportedV1`.

## Consequences

Normal reads do not depend on legacy SQL. Import is eventually consistent and depends on source ordering semantics: Assumption – requires validation.