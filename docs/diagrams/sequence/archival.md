# Archival Sequence

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit engineering/SRE |
| Sources | Archive lifecycle and executor |
| Confidence | High |
| Related | [Archival State](../state/archival-state.md), [Audit Domain](../../domains/audit-domain.md) |

```mermaid
sequenceDiagram
    participant Worker as Archival worker
    participant Active as AuditReadModel
    participant Archive as AuditArchive
    Worker->>Active: Find inactive candidate
    Worker->>Active: Load aliases, timeline, events
    Worker->>Active: Upsert transfer Copying
    Worker->>Archive: Replace snapshot
    Worker->>Archive: Verify snapshot
    Worker->>Active: Mark Verified
    Worker->>Active: Serializable recheck
    alt unchanged
        Worker->>Active: Delete active rows and mark Archived
    else changed
        Worker->>Archive: Delete snapshot
        Worker->>Active: Reset transfer Active
    end
```

On exception, lifecycle marks transfer `Failed` with `ErrorCode`, logs and retries on later cycles.