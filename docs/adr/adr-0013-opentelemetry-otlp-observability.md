# ADR-0013: OpenTelemetry OTLP As Standard Observability Stack

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit infrastructure |
| Sources | `Audit.Infrastructure/Observability/ObservabilityExtensions.cs`, `Directory.Packages.props` |
| Confidence | High |
| Related | [ADR-0002](adr-0002-modular-worker-architecture.md), [Infrastructure observability](../infrastructure/observability.md) |

## Status

Accepted.

## Date

2026-06-22.

## Context

The system runs as four separate .NET processes (ADR-0002). Debugging latency, failed deliveries or stale projections across process boundaries requires distributed tracing and structured metrics. The recruitment exercise targets an Azure/Microsoft-aligned stack; future production would likely integrate with Azure Monitor or a compatible OTLP backend.

## Problem

Each backend process must emit traces, metrics and logs in a way that can be correlated across the ingestion → bus → processing pipeline without coupling the services to a specific vendor SDK.

## Considered Options

- **Azure Application Insights SDK.** Deep Azure integration but vendor-specific; replaces standard .NET ILogger and Activity APIs with proprietary abstractions, making local development harder and migration costly.
- **Prometheus endpoint + custom exporters.** Pull-based; good for metrics but no native distributed tracing support without additional libraries.
- **OpenTelemetry SDK with OTLP exporter.** Vendor-neutral; compatible with Jaeger, Zipkin, Azure Monitor (via OTLP ingestion), Grafana/Tempo and others. Standard .NET Activity API is used natively by ASP.NET Core, MassTransit and HttpClient.

## Decision

Use the OpenTelemetry SDK with the OTLP exporter (`OpenTelemetry.Exporter.OpenTelemetryProtocol`) as the single observability stack. All four processes call `AddAuditObservability(configuration, serviceName)` from `ObservabilityExtensions`. Instrumented sources: `AspNetCore`, `HttpClient`, `Runtime` metrics and the MassTransit activity source. The OTLP endpoint is configurable via environment variable; no collector is included in Docker Compose (traces are dropped locally unless a collector is added).

## Consequences

- Tracing is vendor-neutral; a production deployment can route OTLP to Azure Monitor, Jaeger or Grafana without changing application code.
- MassTransit message spans are correlated with ASP.NET Core request spans automatically.
- No collector or dashboard is included in the repository; observability is not functional in the local Docker Compose stack without adding one: Assumption – requires validation.
- OpenTelemetry .NET SDK is GA but some instrumentation libraries are still RC; semantic conventions may change between versions.
