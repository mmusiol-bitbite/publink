# Security

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit security engineering |
| Sources | Query API pipeline, Nginx config, authentication review |
| Confidence | High for implemented controls; Low for production posture |
| Related | [Authentication Authorization](../api/authentication-authorization.md), [Secrets Management](secrets-management.md) |

Implemented controls: API security headers, Nginx CSP and permissions policy, rate limiting, organization ID filters in queries and CSV formula-injection mitigation in export.

Critical gaps: no app authentication, no route authorization, static configured tenant, no production secret management, no TLS/WAF/ingress config, no export access audit trail, no retention/legal hold policy.

These gaps are production blockers unless covered by validated external controls.