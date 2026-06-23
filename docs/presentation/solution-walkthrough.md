# Solution Walkthrough

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit architecture / technical writing |
| Sources | Architecture docs, ADRs and diagrams |
| Confidence | High |
| Related | [Executive Summary](executive-summary.md), [System Overview](../architecture/system-overview.md), [Audit Storage ERD](../diagrams/erd/audit-storage.md) |

> **TL;DR**
> - **Business problem.** A treasurer facing an RIO inspection needs to show who changed what and when on contracts. The legacy system stores audit rows but provides no explorer.
> - **What was built.** A read-only audit explorer: .NET 10 backend (4 processes), React 19 SPA, Azure Service Bus, MSSQL active/archive stores — packaged as Docker Compose for local/demo use.
> - **Key architectural choices.** Legacy SQL polled at-least-once into canonical events → search/timeline projections → hot/cold MSSQL split; export as verifiable ZIP; search by historical field values; DLQ visibility for operators.
> - **Out of scope (deliberate MVP trade-offs).** Authentication/authorisation, legal evidence (WORM/signatures), production infrastructure (no CD, no Kubernetes), write-back to legacy source.

This document explains the solution in the same order as the original presentation story, but as reading material. It is intended for reviewers who want to understand the business need, runtime design, data model, reliability choices and known production gaps without following slide notes.

## 1. Audit Problem

Publink Audit answers audit-history questions for contract-related data: who changed what, when it changed, which entity was affected and what the before/after values were. The legacy Contracts system remains the source of truth. This repository builds a read-only audit explorer around that source instead of replacing the transactional system.

The key constraint is that the source system cannot emit native events in this exercise. The MVP therefore polls legacy SQL audit rows, converts them into canonical audit messages and builds read models optimized for search, timeline review and export.

## 2. Product Experience

The React SPA supports active and archived contract browsing. Users can search by current and historical contract values, inspect a bilingual timeline, filter timeline entries and export audit history as a ZIP package.

Exports include `audit.csv`, `manifest.json` and `checksums.sha256`. This gives reviewers a verifiable package with hashes, but it is not a qualified legal evidence system: WORM storage, signatures and trusted timestamps are outside the implemented scope.

## 3. System Context

Publink Audit sits between users, legacy SQL, Service Bus and owned MSSQL databases. Users interact through the browser. Workers and the API use SQL and messaging infrastructure internally. The legacy source is read, not modified.

The context boundary matters because authentication, authorization and production identity integration are intentionally not implemented. A production deployment must enforce tenant-aware and object-level authorization before returning search, timeline, synchronization or export data.

## 4. Runtime Containers

The backend is split into four .NET processes:

- `Audit.Query.Api` exposes search, timeline, export, synchronization and health endpoints.
- `Audit.Ingestion.Worker` polls legacy SQL and handles manual sync commands.
- `Audit.Processing.Worker` consumes imported audit events and updates the active read model.
- `Audit.Archival.Worker` moves inactive contracts from active storage to archive storage.

This split keeps failure modes clear. Query traffic can remain available while import or archive work is retried, and processing can use independent retry/DLQ behavior for message delivery.

## 5. Import Flow

Ingestion reads the last source position from `import_checkpoints`, loads newer legacy audit rows, maps them to `AuditEntryImportedV1` and publishes those messages. After publishing, the worker advances the checkpoint for the source.

Processing consumes `AuditEntryImportedV1` from the `audit-projection` endpoint. It checks the unique `(Source, SourceEventId)` key, appends the canonical event to `audit_events` and updates projection tables. Duplicate redelivery completes without creating another projection effect.

## 6. Query, Projection And Export Flow

The API does not query legacy audit rows. It reads dedicated read-model tables through Dapper:

- `contract_search` stores the current searchable contract summary.
- `contract_search_aliases` stores historical searchable values.
- `contract_timeline_items` stores timeline rows derived from canonical events.
- `audit_events` stores canonical imported events and source identifiers.

Timeline endpoints return cursor-paginated pages with `nextCursor`. Export reads the same active/archive models and returns a ZIP when the event count is within the configured limit.

## 7. Archival Flow

Archival selects inactive contracts, loads active aliases, timeline items and events, writes a replacement snapshot into the archive database and verifies that snapshot. Before deleting active rows, the worker performs a serializable recheck so a contract that changed during archival returns to active handling.

Archive progress is tracked in `contract_archive_transfers`. That table is the state ledger for copy, verification, archived, failed and reactivation paths.

## 8. Reliability

Messaging is at-least-once. The system expects duplicate delivery and uses idempotency keys to keep local effects safe. MassTransit retry and DLQ settings protect worker endpoints, and EF outbox support records broker publication state in `OutboxMessages`/`OutboxStates`; inbox state is recorded in `InboxStates`.

The important operational distinction is that MassTransit tables are delivery reliability infrastructure. Business audit history lives in `audit_events`, projections, checkpoint/synchronization tables and archive snapshots.

## 9. Security Boundary

**This is an MVP. Authentication and authorisation are intentionally not implemented.** The recruitment brief explicitly asked for a note on what was consciously skipped — this is the most significant gap between the MVP and a production system.

The MVP includes security headers and development/runtime configuration hygiene, but it does not authenticate users or authorise data access. The demo organisation ID is a deterministic local tenant context, not a security boundary — any request that reaches the API can read any contract.

Production must add, at minimum:
- Identity provider integration (e.g. Azure AD / Entra ID) with JWT validation on every API request.
- Tenant-aware and object-level authorisation: a treasurer from organisation A must not see contracts from organisation B.
- Audit logging for privileged operations (export, manual synchronisation).
- Secret management (no connection strings in environment variables without rotation and vault backing).
- A clear data-classification and retention policy.

These are standard production requirements, not optional extras. The MVP omits them because they have no bearing on demonstrating the audit-history capabilities that the recruitment task asked for.

## 10. Delivery And Operations

The implemented delivery target is Docker Compose for local/demo use. CI validates backend, frontend and container builds.

Production deployment remains a gap: no CD pipeline, registry publishing, Kubernetes/Helm, Terraform/Bicep, production alerting or backup automation exists in the repository.

## 11. Key Architecture Decisions

The ADRs document the most important trade-offs: modular worker architecture, temporary legacy SQL polling, at-least-once messaging, MSSQL active/archive persistence, canonical events with projections, contract-level archival, static demo tenant, API/event versioning, Docker Compose runtime and quality gates.

The central design choice is canonical events plus projections. Canonical events preserve the imported event representation; projections make search and timeline reads fast and stable.

## 12. Main Risks And Next Steps

Before production, the largest open items are identity/authorization, legal evidence requirements, source-system integration strategy, infrastructure as code, production monitoring/alerting, backup/recovery validation, secrets management and replay/rebuild runbooks.

The MVP is therefore best read as a reproducible architecture demonstration with explicit boundaries and trade-offs, not as a production-ready compliance platform.