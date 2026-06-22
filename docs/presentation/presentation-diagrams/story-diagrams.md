# Presentation Diagrams

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture / technical writing |
| Sources | Generated diagrams and ADRs |
| Confidence | High |
| Related | [Solution Walkthrough](../solution-walkthrough.md) |

```mermaid
flowchart LR
    Problem[Audit question] --> Source[Legacy SQL rows]
    Source --> Event[AuditEntryImportedV1]
    Event --> Projection[Search/timeline projections]
    Projection --> UI[Active/archive explorer]
    UI --> Export[ZIP export]
```

```mermaid
flowchart TB
    Goal[Fast reliable audit review]
    Goal --> Workers[Separate workers and API]
    Goal --> Messaging[Async messaging + idempotency]
    Goal --> Projections[Canonical events + projections]
    Goal --> Archive[Contract-level archive]
    Goal --> Quality[CI checks]
```