# ADR-0003: Quality Gates

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit engineering leads |
| Sources | CI workflow, build props, package scripts |
| Confidence | High |
| Related | [CI/CD](../infrastructure/ci-cd.md) |

## Status

Accepted.

## Date

2026-06-19.

## Context

The repo contains backend, frontend and container artifacts.

## Problem

Regressions can appear in code style, tests, builds or container packaging.

## Considered Options

Build only; separate backend/frontend/container gates; full CD.

## Decision

Run backend restore/format/build/test, frontend lint/format/test/build/audit and Compose config/build in CI.

## Consequences

Broad CI feedback exists. CD is not implemented: Assumption – requires validation.

## MVP Test Coverage Note

The current test suite represents a subset of real-world test scenarios discovered during implementation—that is, specific functional areas and defect patterns encountered rather than exhaustive coverage. Tests serve as regression gates for implemented behaviors.

In a production system, a comprehensive test pyramid would include:

- **Unit tests** for domain logic, mappers and query builders.
- **Integration tests** for persistence, messaging, and projection consistency.
- **End-to-end tests** covering API contracts, synchronization flows and data lifecycle transitions.
- **Smoke tests** post-deployment to verify critical paths in staging/production.
- **Load and stress tests** for synchronization performance, archive transfer throughput and concurrent search/export operations.

Production deployment quality would require all four tiers plus ongoing performance monitoring to establish operational SLOs.