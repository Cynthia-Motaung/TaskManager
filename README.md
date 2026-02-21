# TaskManager API

TaskManager is an ASP.NET Core Web API for managing users, projects, tasks, assignments, dependencies, comments, and attachments.

## Tech stack

- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server (runtime)
- InMemory EF provider (integration tests)
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
```

## Prerequisites

- .NET 10 SDK
- SQL Server instance for normal app runtime

## Configuration

Set your SQL Server connection string in `TaskManager/appsettings.json`:

```json
"ConnectionStrings": {
  "TaskConnection": "Server=YOUR_SERVER;Database=TaskManager;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
}
```

## Run locally

From repository root:

```bash
dotnet restore
dotnet build
dotnet run --project TaskManager/TaskManager.csproj
```

Default local URLs:

- `http://localhost:5131`
- `https://localhost:7024`

Root URL redirects to Swagger:

- `http://localhost:5131/` -> `/swagger`
- `http://localhost:5131/swagger/index.html`

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

## Validation and error handling

- Request payloads use DTOs with data annotations.
- Task `status` allowed values: `Pending`, `InProgress`, `Done`, `Blocked`.
- Task `priority` allowed values: `Low`, `Medium`, `High`, `Critical`.
- Global exception middleware returns `application/problem+json` for unhandled errors.

## Database migrations

Apply migrations:

```bash
dotnet ef database update --project TaskManager/TaskManager.csproj
```

## Automated tests

Integration tests use `WebApplicationFactory` with an in-memory EF database in `Testing` environment.

Run tests:

```bash
dotnet test TaskManager.Tests/TaskManager.Tests.csproj
```

Current integration coverage includes:

- Root redirect to Swagger
- Task validation scenarios
- Full API workflow smoke test across all controllers and key endpoints
