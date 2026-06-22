# Configuration

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit engineering/DevOps |
| Sources | appsettings, settings classes, Compose, Vite config |
| Confidence | High |
| Related | [Secrets Management](secrets-management.md), [Docker](docker.md) |

Connection strings:

- `ReadModel`: active MSSQL read model.
- `Archive`: archive MSSQL store.
- `ServiceBus`: broker transport.
- `ServiceBusAdministration`: broker administration, falls back to `ServiceBus`.
- `LegacySource`: external client legacy SQL source for ingestion; it must never point
  to an application-owned or locally simulated legacy database.

Backend hosts load required connection strings through shared infrastructure configuration helpers. A missing required connection string fails startup with a `ConnectionStrings:{Name} is required.` error. `ServiceBusAdministration` remains optional and falls back to `ServiceBus` in Query API, ingestion and processing.

Other backend configuration:

- `LegacyImport:Source`, `BatchSize`, `PollingInterval`, `SourceTimeZone`
- `Synchronization:Source`, `DeadLetterQueue`, `HealthyStatusMaxAge`, `LegacySynchronizationQueueName`
- `Archival:InactivityYears`, `RunInterval`, `MaximumContractsPerRun`
- `OTEL_EXPORTER_OTLP_ENDPOINT` for telemetry export

Frontend configuration: `VITE_API_PROXY_TARGET` for local Vite proxy.

Production configuration source and validation workflow are not defined: Assumption – requires validation.

## Bootstrap Conventions

Shared backend infrastructure owns common bootstrap code for persistence and Service Bus clients:

- active read-model persistence is registered through `AddAuditReadModelPersistence`;
- archive persistence is registered through `AddAuditArchivePersistence`;
- repeated EF Core column lengths and JSON column types are configured through shared persistence mapping helpers;
- startup database migration retry is registered through `AddDatabaseStartupInitializer` and invoked through `InitializeRequiredDatabasesAsync`;
- Azure Service Bus clients, the MassTransit host and the standard consumer retry/DLQ policy are registered through shared messaging extensions.

Deployable projects should keep only service-specific registrations, such as consumers, endpoint names, outbox usage, worker options and API-specific middleware.
