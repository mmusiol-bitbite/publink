# Error Handling

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit API engineering |
| Sources | `ApiExceptionHandler`, validators, export service, query executors |
| Confidence | High |
| Related | [REST API](rest-api.md), [Troubleshooting](../getting-started/troubleshooting.md) |

The API uses Problem Details. Mapped exception responses include `traceId`.

| Cause | Status | Title/code |
| --- | --- | --- |
| `ArgumentException` | 400 | `invalidRequest` |
| `ExportTooLargeException` | 413 | `exportTooLarge` |
| `LegacySynchronizationUnavailableException` | 503 | `synchronizationUnavailable` |
| `ContractStoreUnavailableException` | 503 | `contractStoreUnavailable` |
| Other exception | 500 | `unexpectedError` |

Validation codes:

- `searchQueryTooShort`
- `searchQueryTooLong`
- `timelineDateRangeInvalid`
- `timelineActorTooLong`
- `timelineChangeTypeInvalid`
- `timelineEntityTypeInvalid`
- `exportLocaleInvalid`

Dapper query executors translate `SqlException` and `TimeoutException` into store-unavailable errors. The frontend converts non-OK responses to `requestFailed:{status}` and renders localized error states.