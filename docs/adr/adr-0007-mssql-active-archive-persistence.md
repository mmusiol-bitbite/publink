# ADR-0007: MSSQL Active And Archive Persistence

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit data architecture |
| Sources | Active/archive DbContexts, Compose, query sources |
| Confidence | High |
| Related | [ERD](../diagrams/erd/audit-storage.md) |

## Status

Accepted.

## Date

2026-06-19.

## Context

The code defines separate active and archive SQL contexts and connection strings.

## Problem

Current and inactive contract audit histories need different operational paths.

## Considered Options

Single database; separate active/archive MSSQL stores; non-relational archive.

## Decision

Use MSSQL for active read model and archive database.

## Consequences

Active and archive endpoint families can fail independently. Production backup, HA and migration governance are: Assumption – requires validation.