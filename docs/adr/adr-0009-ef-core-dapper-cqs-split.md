# ADR-0009: EF Core For Writes, Dapper For Reads (CQS Persistence Split)

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit data architecture |
| Sources | `Audit.Infrastructure/Persistence/Persisters/`, `Audit.Infrastructure/Queries/`, `Audit.Infrastructure/Queries/ContractReadSql.cs` |
| Confidence | High |
| Related | [ADR-0008](adr-0008-canonical-events-and-projections.md), [ADR-0006](adr-0006-masstransit-transactional-outbox.md) |

## Status

Accepted.

## Date

2026-06-20.

## Context

The write side (canonical event append, projection upserts, archive lifecycle state transitions) requires change tracking, migration management and transactional outbox support. The read side (search, timeline, export) requires efficient LIKE-ranked queries across multiple columns, runtime-selected table names (`contract_search` vs `archived_contracts`) and cursor-paginated results that cannot be expressed as a single efficient LINQ query in EF Core.

## Problem

Using EF Core for all reads forces either a round-trip penalty (load entity then project) or leaky raw SQL inside the ORM context. Using Dapper for writes loses change tracking and transactional outbox integration. A single-ORM approach cannot serve both concerns equally well.

## Considered Options

- **Pure EF Core for all reads and writes.** Simpler dependency model but generates inefficient multi-statement queries for LIKE ranking with subquery scoring and parameterised table name selection.
- **Pure Dapper for all reads and writes.** Eliminates ORM overhead but loses change tracking, migration management and the `AddEntityFrameworkOutbox` integration needed for atomic broker publication.
- **EF Core for writes, Dapper for reads.** Each tool handles the concern it is suited for. Persistence ports (`ICanonicalAuditEventPersister`, `IContractProjectionPersister`, `IAuditUnitOfWork`) own the write boundary; read sources (`IContractReadSource`, `ActiveContractReadSource`, `ArchivedContractReadSource`) own the read boundary.

## Decision

Use EF Core for all writes, schema migrations and transactional outbox state. Use Dapper for all read queries via `IContractReadSource` implementations and static `ContractReadSql` definitions. The two paths share no query logic. Raw SQL in read sources is parameterised through Dapper's command model; table names are composed at class construction time (not at runtime from user input) to avoid injection risk.

## Consequences

- Read queries are optimal single-statement SQL with LIKE ranking, alias subquery joins and cursor pagination.
- Write paths benefit from change tracking, idempotency index enforcement and outbox atomicity.
- Two query languages are present in the codebase; developers must be familiar with both.
- Schema changes require EF Core migrations for write-side tables; read-side SQL in `ContractReadSql.cs` must be kept consistent with the migration output manually.
- Production query plan monitoring is not implemented: Assumption – requires validation.
