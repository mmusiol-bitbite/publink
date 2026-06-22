# ADR-0001: Modular Worker Architecture

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture |
| Sources | Solution/projects, startup files, Docker Compose |
| Confidence | High |
| Related | [System Overview](../architecture/system-overview.md) |

## Status

Accepted.

## Date

2026-06-21.

## Context

The backend contains separate deployables for Query API, ingestion, processing and archival.

## Problem

Audit reads, legacy import, projection and archival have different failure modes and operational concerns.

## Considered Options

Single process; modular workers; separate repositories/services.

## Decision

Use one repository/solution with multiple deployables and shared contracts/domain/application/infrastructure libraries.

## Consequences

Clear runtime boundaries and local reproducibility, with more deployment moving parts. Horizontal scaling policy remains: Assumption – requires validation.