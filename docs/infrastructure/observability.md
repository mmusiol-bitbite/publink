# Observability

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit SRE |
| Sources | Observability registration, middleware, appsettings |
| Confidence | High for instrumentation; Low for production operations stack |
| Related | [Runbook](../getting-started/runbook.md), [Quality Attributes](../architecture/quality-attributes.md) |

OpenTelemetry instrumentation exists for MassTransit, HTTP clients, runtime metrics and ASP.NET Core in Query API. Health endpoints are filtered from ASP.NET Core traces. OTLP exporter is enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is configured.

Query API uses ASP.NET Core health checks for readiness. `/health/live` returns a lightweight liveness response, while `/health/ready` verifies the read-model SQL connection and the legacy synchronization Service Bus queue and returns `ready` or `notReady`.

Service names: `audit-query-api`, `audit-ingestion-worker`, `audit-processing-worker`, `audit-archival-worker`.

Query API adds `X-Trace-Id` and logger scope `TraceId`. Default log level is `Information`; EF Core database command logging is `Warning`.

Missing: OpenTelemetry Collector, dashboards, alerts, log aggregation, SLO dashboards and on-call routing. These are: Assumption – requires validation.