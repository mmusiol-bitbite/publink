# REST API

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit API engineering |
| Sources | `Audit.Query.Api/Endpoints`, query models, frontend `api.ts`, export service |
| Confidence | High |
| Related | [Error Handling](error-handling.md), [Events](events.md), [Authentication Authorization](authentication-authorization.md) |

Base path: `/api/v1`. Query API is an ASP.NET Core minimal API with Swagger enabled.

## Search

| Store | Method | Path |
| --- | --- | --- |
| Active | GET | `/api/v1/contracts/search` |
| Archive | GET | `/api/v1/archive/contracts/search` |

Query parameters:

| Name | Type | Rules |
| --- | --- | --- |
| `searchPhrase` | string | required, trimmed length 2-200 |
| `limit` | integer | optional, default 20, clamped to 1-50 |

Response:

```json
{
  "items": [
    {
      "contractId": "00000000-0000-0000-0000-000000000000",
      "number": "UM/2026/17",
      "internalNumber": null,
      "subject": "Contract subject",
      "contractorName": "Contractor",
      "lastActivityAt": "2026-06-20T10:15:00+00:00",
      "matchedHistoricalValue": true
    }
  ]
}
```

Search matches number, internal number, subject, contractor name and aliases through SQL `LIKE`.

## Timeline

| Store | Method | Path |
| --- | --- | --- |
| Active | GET | `/api/v1/contracts/{contractId}/audit-events` |
| Archive | GET | `/api/v1/archive/contracts/{contractId}/audit-events` |

Query parameters:

| Name | Type | Rules |
| --- | --- | --- |
| `limit` | integer | optional, default 50, clamped to 1-100 |
| `cursor` | string | optional opaque cursor |
| `from` | DateTimeOffset | optional; must be <= `to` |
| `to` | DateTimeOffset | optional; must be >= `from` |
| `actor` | string | optional, max length 200 |
| `changeType` | integer | optional, known codes 1-3 |
| `entityType` | integer | optional, known codes 1-7 |

Response includes `contractId`, `snapshotSequence`, `synchronizedAt`, `items` and `nextCursor`. Items contain event ID, source sequence, occurred time, correlation ID, change kind/code, entity kind/code, actor, field changes and data-quality issues.

## Export

| Store | Method | Path |
| --- | --- | --- |
| Active | GET | `/api/v1/contracts/{contractId}/audit-events/export` |
| Archive | GET | `/api/v1/archive/contracts/{contractId}/audit-events/export` |

Filters match timeline filters. Additional query parameter `locale` accepts `pl` or `en`; default is `pl`.

Successful export:

- `200 OK`
- `application/zip`
- `X-Content-SHA256` response header
- ZIP entries `audit.csv`, `manifest.json`, `checksums.sha256`

If more than 10,000 events match, response is `413 exportTooLarge`.

## Synchronization

`GET /api/v1/synchronization/status` returns source, last source event ID, last synchronized timestamp, status, DLQ event count and active request metadata.

`POST /api/v1/synchronization/requests` returns `202 Accepted` with request ID, source, accepted timestamp, status `Accepted` and `joined` flag.

## Health

| Endpoint | Purpose |
| --- | --- |
| `GET /health/live` | Process liveness. |
| `GET /health/ready` | Database and synchronization queue readiness. |

## Headers

API responses include `X-Trace-Id`, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY` and `Referrer-Policy: no-referrer`.