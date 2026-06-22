# Manual Synchronization Sequence

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit engineering |
| Sources | Synchronization endpoints and ingestion consumer |
| Confidence | High |
| Related | [REST API](../../api/rest-api.md), [Runbook](../../getting-started/runbook.md) |

```mermaid
sequenceDiagram
    participant UI as React SPA
    participant API as Query API
    participant DB as AuditReadModel
    participant Bus as Service Bus
    participant Worker as Ingestion worker
    UI->>API: POST /api/v1/synchronization/requests
    API->>DB: Acquire or join lease
    API->>Bus: Send RequestLegacySynchronizationV1
    API-->>UI: 202 Accepted
    Bus->>Worker: Deliver command
    Worker->>Worker: Run import to current
    Worker->>DB: Complete lease
    UI->>API: Poll status
```

Failure path: if sending the command fails after lease acquisition, Query API releases the lease and returns an error.