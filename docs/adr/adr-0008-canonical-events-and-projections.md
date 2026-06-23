# ADR-0008: Canonical Events And Projections

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit data architecture |
| Sources | Persisters, projection writer, query executors |
| Confidence | High |
| Related | [Data Flow](../architecture/data-flow.md) |

## Status

Accepted.

## Date

2026-06-19.

## Context

Processing writes canonical imported events and search/timeline projections.

## Problem

Raw legacy rows are not sufficient for efficient UI search, paging and export.

## Considered Options

Direct raw queries; canonical events only; canonical events plus projections.

## Decision

Persist canonical events and maintain query projections.

## Consequences

Read paths are optimized and canonical data remains available. Projection rebuild tooling is not present: Assumption – requires validation.