# ADR-0007: Static Demo Tenant

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit security architecture |
| Sources | Current tenant implementation, Query API startup |
| Confidence | High |
| Related | [Authentication Authorization](../api/authentication-authorization.md) |

## Status

Superseded (2026-06-22). Static configured tenant context was removed from runtime.

## Date

2026-06-21.

## Context

At the time of the decision, the app used a static configured organization context and had no auth middleware.

## Problem

MVP queries require tenant scoping, but production identity is absent.

## Considered Options

Static configured tenant; claim-based tenant; UI tenant selector.

## Decision

Use static configured tenant for local/demo runtime.

## Consequences

This temporary approach was removed. Current runtime still has no user authentication or authorization, and tenant scoping must be reintroduced through proper identity claims in a future decision.
