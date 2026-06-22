# Query And Export Sequence

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit API engineering |
| Sources | Query API handlers, Dapper executors, export service |
| Confidence | High |
| Related | [REST API](../../api/rest-api.md), [Error Handling](../../api/error-handling.md) |

```mermaid
sequenceDiagram
    participant Client
    participant API as Query API
    participant Store as Active/Archive MSSQL
    Client->>API: GET timeline/export
    API->>API: Validate filters
    API->>Store: Read snapshot and rows
    alt timeline
        API-->>Client: TimelinePage with nextCursor
    else export <= 10000 events
        API-->>Client: ZIP + X-Content-SHA256
    else export too large
        API-->>Client: 413 exportTooLarge
    end
```