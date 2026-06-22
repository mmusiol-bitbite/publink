# ADR-0008: API And Contract Versioning

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit API architecture |
| Sources | Routes, contracts, export manifest |
| Confidence | High |
| Related | [Versioning](../api/versioning.md) |

## Status

Accepted.

## Date

2026-06-21.

## Context

REST endpoints use `/api/v1`; messages use `V1` suffix and `SchemaVersion`.

## Problem

Frontend, API and workers need visible compatibility boundaries.

## Considered Options

Unversioned contracts; path/type versioning; schema registry/upcasters.

## Decision

Use `/api/v1`, `V1` message contracts and schema version fields.

## Consequences

Current version is visible. Formal compatibility matrix is not present: Assumption – requires validation.