# ADR-0015: Audit Export As Verifiable ZIP

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit API / application engineering |
| Sources | `Audit.Application/Exports/ContractAuditExportService.cs`, `Audit.Query.Api/Endpoints/ContractEndpointHandlers.cs`, `Audit.Query.Api/Endpoints/Validation/ExportQueryValidator.cs`, `Audit.Application/Exports/Exceptions/ExportTooLargeException.cs` |
| Confidence | High |
| Related | [ADR-0008](adr-0008-canonical-events-and-projections.md), [REST API](../api/rest-api.md), [Error Handling](../api/error-handling.md) |

## Status

Accepted.

## Date

2026-06-21.

## Context

The treasurer needs to hand over contract audit history to auditors from the Regional Audit Office. The exported data must be portable (usable outside the application), machine-readable (for reconciliation tooling) and verifiable (the auditor must be able to confirm the package has not been modified after generation).

The export endpoint reads from the same `contract_timeline_items` projection used by the timeline view. Both the active and archive read models are exportable through their respective endpoint sets.

## Problem

Raw CSV carries no provenance metadata: the recipient cannot tell when it was generated, which contract it covers, which filters were applied, or whether any field was altered after download. A single export file also gives no way to verify partial tampering. Additionally, exporting contracts with very large event histories in one synchronous response risks unbounded memory consumption on the API server.

## Considered Options

- **CSV only.** Simple, widely supported. No manifest, no checksums, no provenance. A recipient has no way to verify completeness or detect post-export modification.
- **PDF report.** Human-readable but not machine-processable; requires a rendering library; cannot be diffed or re-imported.
- **ZIP with CSV + manifest + checksums (chosen).** Portable container. Each file is independently hashed. The manifest records all provenance metadata. The recipient can run `sha256sum -c checksums.sha256` to verify integrity.
- **Streaming with pagination.** Avoids the memory cap but makes hash computation on the streamed response impossible: SHA-256 requires the full content before the first byte can be sent.

## Decision

`ContractAuditExportService.GenerateAsync` builds the export fully in memory and returns an `AuditExportPackage` record:

- **ZIP entries:** `audit.csv`, `manifest.json`, `checksums.sha256`.
- **`audit.csv`:** bilingual column headers selected by `locale` (`pl` / `en`). Values starting with `=`, `+`, `-`, `@`, `\t` or `\r` are prefixed with `'` to prevent CSV formula injection in spreadsheet applications.
- **`manifest.json`:** includes `contractId`, `dataSource`, `locale`, applied `filters`, `snapshotSequence`, `synchronizedAt`, `eventCount`, `rowCount`, `csvSha256`, `generatedAt`, and a fixed disclaimer: *"This export supports audit work but is not qualified electronic evidence."*
- **`checksums.sha256`:** sha256sum-compatible file listing SHA-256 hashes of `audit.csv` and `manifest.json`. The ZIP itself is hashed and returned in the `X-Content-SHA256` response header.
- **Export cap:** `MaximumEvents = 10_000`. If the timeline projection has a next cursor at that page boundary, the service throws `ExportTooLargeException` → API returns `413 exportTooLarge`. This protects the API from unbounded memory allocation on contracts with unusually long histories.
- **Locale:** `pl` or `en`; defaults to `pl`. Controls CSV column headers and the manifest disclaimer language. An invalid locale value is rejected by `ExportQueryValidator` with `exportLocaleInvalid`.
- Both active (`/api/v1/contracts/{id}/audit-events/export`) and archive (`/api/v1/archive/contracts/{id}/audit-events/export`) share the same service; `dataSource` distinguishes them in the manifest.

## Consequences

- A recipient with standard shell tooling can verify the full package: `sha256sum -c checksums.sha256` inside the ZIP validates both CSV and manifest without the application.
- The `X-Content-SHA256` header allows in-transit integrity verification by middleware or the browser download handler.
- Exports larger than 10,000 events must be split by the operator using timeline filters before exporting.
- The manifest disclaimer is hard-coded English and Polish text — it is not a configurable legal notice. Production deployment may need to replace or extend this with jurisdiction-specific wording.
- This is explicitly **not** a qualified electronic evidence system: WORM storage, cryptographic signatures, trusted timestamps and a legal chain of custody are outside implemented scope.
