# ADR-0010: Persister/Reader Port Pattern (No Generic Repository)

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit architecture |
| Sources | `Audit.Application/Persistence/Interfaces/`, `Audit.Application/Queries/Interfaces/`, `Audit.Infrastructure/Persistence/Persisters/`, `Audit.Infrastructure/Queries/` |
| Confidence | High |
| Related | [ADR-0009](adr-0009-ef-core-dapper-cqs-split.md), [ADR-0008](adr-0008-canonical-events-and-projections.md) |

## Status

Accepted.

## Date

2026-06-21.

## Context

The `Audit.Application` layer owns use cases and business policies. It must write canonical events and projection data, and it must read contracts for query responses and export. In .NET projects this boundary is commonly implemented with a generic `IRepository<T>` abstraction. The alternative is purpose-built port interfaces that expose only the operations each use case requires.

## Problem

A generic repository exposes the full CRUD surface regardless of what a use case actually needs. This makes it easy to add accidental side-effects (e.g. calling `Delete` from a projection writer) and couples the application layer to storage-shaped types rather than domain operations.

## Considered Options

- **Generic `IRepository<T>`.** Familiar pattern, less boilerplate per aggregate. Exposes the full CRUD surface to every consumer; requires discipline to avoid misuse; difficult to mock precisely for unit tests.
- **Purpose-built Persister/Reader ports.** Each interface exposes exactly the methods a use case needs: `ICanonicalAuditEventPersister`, `IContractProjectionPersister`, `IAuditUnitOfWork` for writes; `IContractSearchSource`, `IContractTimelineSource`, `IContractStore` for reads. Implementations live in `Audit.Infrastructure` and are injected by the DI container.
- **No abstraction; call EF Core/Dapper directly from use cases.** Removes indirection but couples application logic to infrastructure and makes unit testing without a database impractical.

## Decision

Use named, purpose-built port interfaces in `Audit.Application`. Write ports: `ICanonicalAuditEventPersister` (append canonical events), `IContractProjectionPersister` (upsert search and timeline projections), `IAuditUnitOfWork` (commit the transaction). Read ports: `IContractSearchSource`, `IContractTimelineSource` (composed as `IContractStore`), implemented by `SqlContractStore` backed by `IContractReadSource`. No generic repository is present in the solution.

## Consequences

- Each use case depends only on the operations it needs; accidental CRUD misuse is impossible by construction.
- Interfaces are narrow enough to mock precisely in unit tests without a database.
- More interface files than a generic repository approach; new persistence needs require adding or extending a port.
- The `IContractReadSource` / `ActiveContractReadSource` / `ArchivedContractReadSource` pattern gives the API layer a clean switch between hot and cold stores without changing use-case logic.
