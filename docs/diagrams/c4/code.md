# Code Diagram

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit engineering |
| Sources | Processing and archiving classes |
| Confidence | Medium; selective class-level view |
| Related | [Audit Domain](../../domains/audit-domain.md) |

```mermaid
classDiagram
    class AuditEntryImportedConsumer
    class CanonicalAuditEventPersister
    class ContractProjectionPersister
    class ContractSearchProjectionWriter
    class DatabaseContractArchiveLifecycle
    class ContractArchivalExecutor
    class ContractArchiveSnapshotStore
    AuditEntryImportedConsumer --> CanonicalAuditEventPersister
    AuditEntryImportedConsumer --> ContractProjectionPersister
    ContractProjectionPersister --> ContractSearchProjectionWriter
    DatabaseContractArchiveLifecycle --> ContractArchivalExecutor
    ContractArchivalExecutor --> ContractArchiveSnapshotStore
```