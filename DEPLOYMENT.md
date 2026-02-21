# TaskManager Production Deployment

This checklist is the recommended path to release TaskManager safely to production.

## Render target

This repository includes Render Blueprint support in `render.yaml` and a Render env template in `.env.render.example`.

Important:

- Render does not provide managed SQL Server, so use an external SQL Server (for example Azure SQL or another hosted SQL Server).

## 1. Production prerequisites

- Provision a SQL Server instance with network access from the API host.
- Provision a secrets manager (or protected environment variables) for runtime secrets.
- Provision HTTPS termination (reverse proxy, ingress, or load balancer).
- Ensure CI can run `dotnet test TaskManager.Tests/TaskManager.Tests.csproj`.

## 2. Required production configuration

Start from the tracked template:

```bash
cp .env.production.example .env.production
```

Set these values in `.env.production` on the production host:

- `ASPNETCORE_ENVIRONMENT=Production`
- `UseInMemoryDatabase=false`
- `ConnectionStrings__TaskConnection='Server=<host>,<port>;Database=TaskManager;User ID=<user>;Password=<strong-password>;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=True;'`
- `TASKMANAGER_JWT_KEY=<strong-random-value-at-least-32-characters>`
- `Jwt__Issuer=TaskManager`
- `Jwt__Audience=TaskManagerClients`
- `SeedUsers__Enabled=false`

For Render specifically:

- Set the same keys in the Render service Environment tab, or use `render.yaml` + dashboard secrets.
- Use `.env.render.example` as a copy/paste template for Render env keys.

Notes:

- Do not store production secrets in `appsettings*.json`.
- Keep `TASKMANAGER_JWT_KEY` and SQL credentials in secret storage only.
- If you must bootstrap admin users in production, temporarily set `SeedUsers__Enabled=true` with strong values for `SeedUsers__AdminPassword` and `SeedUsers__ManagerPassword`, then set it back to `false` after first startup.

## 3. Pre-release checklist

- Pull latest code and verify branch/tag to deploy.
- Run tests:
  - `dotnet test TaskManager.Tests/TaskManager.Tests.csproj`
- Build release artifacts:
  - `dotnet build -c Release`
- Confirm all required environment variables are set on the target host.
- Confirm SQL Server allows inbound connection from the app host.
- Confirm backup/restore path for the production database exists.

## 4. Database migration

Run EF migrations against production connection settings:

```bash
set -a
source .env.production
set +a
dotnet ef database update --project TaskManager/TaskManager.csproj
```

Run this from a shell/session where production `ConnectionStrings__TaskConnection` is loaded.

## 5. Application startup

Start the API with production environment variables loaded:

```bash
set -a
source .env.production
set +a
dotnet run --project TaskManager/TaskManager.csproj --configuration Release
```

For managed hosting, use the equivalent service startup command (systemd, container entrypoint, orchestrator command).
For Render Docker deployment, startup is handled by `Dockerfile` and `render.yaml`.

## Render deployment steps

1. Push this repository with `Dockerfile` and `render.yaml`.
2. In Render, create a new Blueprint instance from the repo.
3. Confirm service settings from `render.yaml`:
   - runtime: Docker
   - health check path: `/health`
4. Set secret env vars in Render:
   - `TASKMANAGER_JWT_KEY`
   - `ConnectionStrings__TaskConnection`
5. Trigger deploy.
6. Run migration from a shell where production env vars are loaded:
   - `dotnet ef database update --project TaskManager/TaskManager.csproj`

## 6. Post-deploy smoke tests

Run quick checks from a client that can reach production:

```bash
curl -i -X POST "https://<api-host>/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"<known-user-email>","password":"<known-user-password>"}'
```

Expected:

- Endpoint is reachable (not network blocked).
- API returns an access token for valid credentials.
- Protected endpoints require a bearer token.
- Basic read endpoint returns data without server errors.

## 7. Operational safeguards

- Enable centralized application logs.
- Configure uptime and error-rate alerts.
- Monitor SQL CPU, memory, and connection count.
- Set JWT key rotation procedure (change key and force token refresh during maintenance windows).

## 8. Rollback plan

- Keep previous release artifact available.
- If deployment fails:
  - stop new release
  - redeploy previous stable release
  - restore DB from backup only if a migration caused unrecoverable data issues
- Document root cause before retrying rollout.

## 9. Final go-live sign-off

- Security review complete (secrets, HTTPS, access controls).
- Functional smoke tests pass.
- Monitoring and alerts confirmed.
- Backup and restore test completed.
- Team sign-off recorded.
