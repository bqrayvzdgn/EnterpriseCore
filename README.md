# EnterpriseCore

Multi-tenant project management API built with ASP.NET Core 9.0 and Clean Architecture.

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | ASP.NET Core 9.0 |
| Database | PostgreSQL |
| ORM | Entity Framework Core 9.0 |
| Cache | Redis |
| Real-time | SignalR |
| Auth | JWT + RBAC |

## Architecture

```
src/
├── EnterpriseCore.API            # Controllers, Middleware, SignalR Hubs
├── EnterpriseCore.Application    # DTOs, Validators, Services, Mappings
├── EnterpriseCore.Domain         # Entities, Enums, Interfaces
└── EnterpriseCore.Infrastructure # EF Core, Repositories, Caching
```

## Prerequisites

- .NET 9.0 SDK
- PostgreSQL
- Redis (optional)

## Getting Started

### 1. Clone and Build

```bash
git clone https://github.com/bqrayvzdgn/EnterpriseCore.git
cd EnterpriseCore
dotnet build
```

### 2. Configure Database

Update connection string in `src/EnterpriseCore.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EnterpriseCore;Username=postgres;Password=yourpassword"
  }
}
```

### 3. Run Migrations

```bash
dotnet ef database update --project src/EnterpriseCore.Infrastructure --startup-project src/EnterpriseCore.API
```

### 4. Run the API

```bash
dotnet run --project src/EnterpriseCore.API
```

- API: http://localhost:5017
- Swagger: http://localhost:5017/swagger
- Health: http://localhost:5017/health

## API Endpoints

### Auth
```
POST /api/auth/register    # Create tenant + admin user
POST /api/auth/login       # Get JWT token
POST /api/auth/refresh     # Refresh token
```

### Projects
```
GET    /api/projects           # List projects
GET    /api/projects/{id}      # Get project
POST   /api/projects           # Create project
PUT    /api/projects/{id}      # Update project
DELETE /api/projects/{id}      # Delete project (soft)
```

### Tasks
```
GET    /api/projects/{id}/tasks    # List tasks
GET    /api/tasks/{id}             # Get task
POST   /api/projects/{id}/tasks    # Create task
PUT    /api/tasks/{id}             # Update task
DELETE /api/tasks/{id}             # Delete task
```

## Features

- **Multi-Tenancy**: Automatic tenant isolation via global query filters
- **Soft Deletes**: Records marked as deleted, not removed
- **Audit Trail**: Automatic tracking of created/updated/deleted timestamps
- **RBAC**: Role-based access control with permissions
- **Real-time**: SignalR hub for live updates (`/hubs/project`)

## License

MIT License - see [LICENSE](LICENSE) file.
