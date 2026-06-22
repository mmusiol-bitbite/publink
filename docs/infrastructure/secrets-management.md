# Secrets Management

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit security/DevOps |
| Sources | Docker Compose, settings classes, CI workflow |
| Confidence | High for local/demo; Low for production |
| Related | [Configuration](configuration.md), [Security](security.md) |

Local/demo secrets are environment variables consumed by Compose: MSSQL password, Service Bus connection strings, legacy source connection and demo organization ID.

No production secret store integration is present. No Key Vault, Docker secrets, Kubernetes secrets, GitHub deployment secrets or managed identity configuration is defined.

Required production controls are: managed secret store or managed identity, credential rotation, least-privilege process identities, secret-safe logging and break-glass procedure. All are: Assumption – requires validation.