# Prerequisites

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit engineering |
| Sources | `Directory.Build.props`, `package.json`, Dockerfiles, CI workflow |
| Confidence | High |
| Related | [Local Development](local-development.md), [Troubleshooting](troubleshooting.md) |

Required tooling:

| Tool | Version/evidence |
| --- | --- |
| .NET SDK | `10.0.x`, backend targets `net10.0`. |
| Node.js | CI uses Node 22; frontend Dockerfile uses Node 24 Alpine. |
| npm | Required for frontend install, lint, test and build. |
| Docker Compose | Required for MSSQL, Service Bus emulator and full stack. |

Local ports: `1433` MSSQL, `5672` Service Bus AMQP, `5300` emulator admin, `8080` Query API, `3000` web UI, `5173` Vite dev server.

Required configuration is listed in [Configuration](../infrastructure/configuration.md). Production secret source is not defined: Assumption – requires validation.