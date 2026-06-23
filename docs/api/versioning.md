# Versioning

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture/API engineering |
| Sources | Query API routes, `Audit.Contracts`, export service |
| Confidence | High for current versioning |
| Related | [REST API](rest-api.md), [Events](events.md), [ADR 0012](../adr/adr-0012-api-and-contract-versioning.md) |

REST routes are versioned by path under `/api/v1`. There is no `/api/v2`, media-type versioning or formal deprecation behavior in code.

Message contracts are named `AuditEntryImportedV1` and `RequestLegacySynchronizationV1`; both include `SchemaVersion` and currently emit version `1`.

Export manifest contains `schemaVersion = 1`.

Backward compatibility policy beyond current code and tests is not defined: Assumption – requires validation.