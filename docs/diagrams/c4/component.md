# C4 Component Diagram

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture |
| Sources | Backend projects |
| Confidence | High |
| Related | [Component Diagram](../../architecture/component-diagram.md) |

```mermaid
flowchart TB
    Contracts[Audit.Contracts] --> Application[Audit.Application]
    Domain[Audit.Domain] --> Application
    Application --> Infrastructure[Audit.Infrastructure]
    Infrastructure --> QueryApi[Audit.Query.Api]
    Infrastructure --> Ingestion[Audit.Ingestion.Worker]
    Infrastructure --> Processing[Audit.Processing.Worker]
    Infrastructure --> Archival[Audit.Archival.Worker]
```