# TaskManager API

TaskManager is an ASP.NET Core Web API for managing users, projects, tasks, assignments, dependencies, comments, and attachments.

## Tech stack

- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server (persistent runtime data)
- InMemory EF provider (integration tests or temporary local mode)
- Swagger / OpenAPI
- xUnit integration tests

## Project structure

```text
TaskManager.sln
TaskManager/
  Controllers/
  DTOs/
  Mappings/
  Middleware/
  Migrations/
  Models/
  Program.cs
TaskManager.Tests/
docker-compose.yml
```

## Prerequisites

- .NET 10 SDK
- SQL Server instance
  - easiest local option: Docker Desktop + `docker compose`

## Persistent SQL setup (recommended)

From repository root:

```bash
export TASKMANAGER_JWT_KEY='replace-with-strong-32-plus-char-key'
export SeedUsers__Enabled=true
export SeedUsers__AdminPassword='replace-with-strong-admin-password'
export SeedUsers__ManagerPassword='replace-with-strong-manager-password'
docker compose up -d
dotnet restore
dotnet build
dotnet ef database update --project TaskManager/TaskManager.csproj
dotnet run --project TaskManager/TaskManager.csproj
```

This repo is configured to use SQL Server in Development:

- `TaskManager/appsettings.Development.json` sets `"UseInMemoryDatabase": false`
- Connection string targets `localhost,14333`
- JWT key must come from `TASKMANAGER_JWT_KEY` (or `Jwt:Key` secret config)
- Seed user passwords must be set when `SeedUsers:Enabled=true`

## Temporary in-memory mode (non-persistent)

For quick runs without SQL Server:

```bash
export TASKMANAGER_JWT_KEY='replace-with-strong-32-plus-char-key'
export SeedUsers__AdminPassword='replace-with-strong-admin-password'
export SeedUsers__ManagerPassword='replace-with-strong-manager-password'
UseInMemoryDatabase=true dotnet run --project TaskManager/TaskManager.csproj
```

Data is reset when the process stops.

Security note:

- `appsettings*.json` intentionally contains placeholder secrets.
- The app will fail fast at startup if JWT key or required seed passwords are still placeholder values.

## Local URLs

- `http://localhost:5131`
- `https://localhost:7024`
- Root redirects to Swagger: `http://localhost:5131/` -> `/swagger`

## Authentication and authorization

The API uses JWT Bearer tokens.

- Public auth endpoints:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
- Authenticated endpoint:
  - `GET /api/auth/me`
- Read endpoints require authentication.
- Write/delete endpoints require `Manager` or `Admin` role.

Seeded bootstrap accounts (when seeding is enabled):

- `admin@taskmanager.local` / `Admin@123`
- `manager@taskmanager.local` / `Manager@123`

In Swagger:

1. Call `POST /api/auth/login`.
2. Copy `accessToken`.
3. Click **Authorize** and enter `Bearer <token>`.

## API endpoints

### Users

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`

### Projects

- `GET /api/projects`
- `GET /api/projects/{id}`
- `POST /api/projects`
- `PUT /api/projects/{id}`
- `DELETE /api/projects/{id}`

### Tasks

- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`
- `POST /api/tasks/{id}/assign/{userId}`
- `GET /api/tasks/filter?status=&priority=&projectId=&userId=`

### Task assignments

- `GET /api/taskassignments`
- `GET /api/taskassignments/task/{taskId}`
- `GET /api/taskassignments/user/{userId}`
- `POST /api/taskassignments`
- `DELETE /api/taskassignments/{userId}/{taskId}`

### Task dependencies

- `GET /api/taskdependencies`
- `GET /api/taskdependencies/{taskId}/{dependsOnTaskId}`
- `POST /api/taskdependencies`
- `DELETE /api/taskdependencies/{taskId}/{dependsOnTaskId}`

### Comments (nested under task)

- `GET /api/tasks/{taskId}/comments`
- `POST /api/tasks/{taskId}/comments`
- `DELETE /api/tasks/{taskId}/comments/{commentId}`

### Attachments (nested under task)

- `GET /api/tasks/{taskId}/attachments`
- `POST /api/tasks/{taskId}/attachments`
- `DELETE /api/tasks/{taskId}/attachments/{attachmentId}`

## Validation and errors

- DTO-based request/response contracts
- Task status allowed: `Pending`, `InProgress`, `Done`, `Blocked`
- Task priority allowed: `Low`, `Medium`, `High`, `Critical`
- User roles allowed: `User`, `Manager`, `Admin`
- Global exception middleware returns `application/problem+json`

## Tests

Run integration tests:

```bash
dotnet test TaskManager.Tests/TaskManager.Tests.csproj
```

Current integration coverage includes:

- JWT login and protected endpoint access
- Unauthorized access checks for protected APIs
- Full authenticated workflow smoke test across all controllers and key endpoints
