# Local Development

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit engineering |
| Sources | Docker Compose, debug Compose overlays, Vite config |
| Confidence | High |
| Related | [Prerequisites](prerequisites.md), [Runbook](runbook.md), [Docker](../infrastructure/docker.md) |

## Full Stack

```powershell
docker compose -f docker/docker-compose.yml up --build
```

Endpoints:

- Web UI: `http://localhost:3000`
- API/Swagger: `http://localhost:8080`
- Health: `/health/live`, `/health/ready`

## Backend Loop

```powershell
dotnet restore src/backend/Publink.Audit.sln
dotnet format src/backend/Publink.Audit.sln --verify-no-changes --no-restore
dotnet build src/backend/Publink.Audit.sln --configuration Release --no-restore
dotnet test src/backend/Publink.Audit.sln --configuration Release --no-build
```

`Audit.Infrastructure.Tests` use Testcontainers.MsSql and require Docker Desktop to be running. Verify the daemon before the full backend test loop with:

```powershell
docker desktop status
docker info
```

## Frontend Loop

```powershell
Push-Location src/frontend
npm ci
$env:VITE_API_PROXY_TARGET = "http://localhost:8080"
npm run dev
Pop-Location
```

Quality command: `npm run check`.
