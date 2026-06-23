# ADR-0011: Contract-Level Archival

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit data architecture |
| Sources | Archive lifecycle, executor, policy |
| Confidence | High |
| Related | [Archival Sequence](../diagrams/sequence/archival.md) |

## Status

Accepted.

## Date

2026-06-20.

## Context

Archival worker moves inactive contracts from active store to archive store.

## Problem

Archiving individual events could split a contract audit history.

## Considered Options

Event-level archival; contract-level archival; no archival.

## Decision

Archive by contract boundary using inactivity policy and verified snapshots.

## Consequences

Contract history stays coherent. Legal retention and permanent deletion policy are: Assumption – requires validation.