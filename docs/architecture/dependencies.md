# Dependencies

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit engineering |
| Sources | `Directory.Packages.props`, `package.json`, Dockerfiles |
| Confidence | High |
| Related | [System Overview](system-overview.md), [CI/CD](../infrastructure/ci-cd.md) |

Backend: .NET 10, EF Core SQL Server 10.0.9, Dapper 2.1.79, MassTransit 8.5.10, Azure Service Bus transport, OpenTelemetry 1.15/1.16, Swashbuckle 6.9.0, xUnit and Testcontainers.MsSql.

Backend dependency placement:

- `Audit.Infrastructure` owns EF Core, Dapper, MassTransit, Azure Service Bus clients and OpenTelemetry integrations.
  - `Audit.Infrastructure.Persistence` is organized into focused subdirectories for readability:
    - **Core**: infrastructure setup, service collection extensions, database initializers, SQL connection factories
    - **Contexts**: EF Core `DbContext` classes for audit read model and archive stores
    - **Initialization**: startup and lifecycle initialization services
    - **Persisters**: read-model persisters, projection writers, stores and entity mappers
    - **Utilities**: helper extensions (e.g., JSON field access)
    - **Entities**: entity model classes mapped to both active and archive schemas
    - **Migrations**: EF Core migrations separated into `AuditMigrations` (read model) and `ArchiveMigrations` (archive store)
- deployable projects own runtime-specific composition: hosted services, consumers, API middleware and endpoint registration.
- shared infrastructure extension methods are preferred for repeated persistence, EF Core property mapping, configuration, messaging bootstrap and standard consumer failure-policy code.

Frontend: React 19.2.7, Vite 8.0.16, TypeScript 6.0.3, TanStack React Query 5.101.0, i18next/react-i18next, Vitest, Testing Library, ESLint and Prettier.

Runtime images: .NET SDK/ASP.NET 10, Node 24 Alpine, Nginx 1.29 Alpine, SQL Server 2022 and Azure Service Bus Emulator.

SBOM, license scanning and container vulnerability scanning are not defined: Assumption – requires validation.
