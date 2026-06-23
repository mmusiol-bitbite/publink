# ADR-0001: Docker Compose Runtime Baseline

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit DevOps |
| Sources | Compose, Dockerfiles, CI |
| Confidence | High |
| Related | [Docker](../infrastructure/docker.md) |

## Status

Accepted for local/demo.

## Date

2026-06-19.

## Context

Compose runs SQL Server, Service Bus emulator, workers, API and web.

## Problem

Developers need a reproducible full-stack environment.

## Considered Options

Manual services; Docker Compose; Kubernetes/Helm.

## Decision

Use Docker Compose as runtime baseline.

## Consequences

Onboarding is reproducible. Production orchestrator/IaC is: Assumption – requires validation.