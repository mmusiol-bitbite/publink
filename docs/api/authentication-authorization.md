# Authentication And Authorization

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit security engineering |
| Sources | Query API startup, frontend API client |
| Confidence | High for implemented absence of auth |
| Related | [Security](../infrastructure/security.md), [Secrets Management](../infrastructure/secrets-management.md) |

No `AddAuthentication`, `AddAuthorization`, `UseAuthentication`, `UseAuthorization` or `RequireAuthorization` was found in Query API. The frontend sends no bearer token or explicit authorization header.

Consequences:

- API requests are not scoped by authenticated tenant context.
- Role-based permissions are not implemented.
- Production identity provider and tenant-claim mapping are: Assumption – requires validation.

Implemented related controls: rate limiting, security headers, Nginx CSP and organization ID filters in data access.
