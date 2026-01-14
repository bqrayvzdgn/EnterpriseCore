# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build the solution
dotnet build

# Run the API (http://localhost:5017, https://localhost:7140)
dotnet run --project EnterpriseCore.API

# Database migrations
dotnet ef migrations add <MigrationName> --project EnterpriseCore.Infrastructure --startup-project EnterpriseCore.API
dotnet ef database update --project EnterpriseCore.Infrastructure --startup-project EnterpriseCore.API

# Swagger UI: http://localhost:5017/swagger
# Health check: http://localhost:5017/health
```

## Architecture

ASP.NET Core 9.0 multi-tenant project management API following Clean Architecture:

```
EnterpriseCore.API          → Controllers, Middleware, SignalR Hubs, JWT Auth
EnterpriseCore.Application  → DTOs, Validators (FluentValidation), AutoMapper profiles, Service interfaces
EnterpriseCore.Domain       → Entities, Enums, Repository/UnitOfWork interfaces
EnterpriseCore.Infrastructure → EF Core DbContext, Repositories, Redis caching, Migrations
```

**Dependencies flow inward:** API → Application → Domain ← Infrastructure

## Key Patterns

**Multi-Tenancy:** All entities implement `IMultiTenant`. Global query filters in `ApplicationDbContext` automatically scope queries by `TenantId`. No manual filtering needed.

**Soft Deletes:** Entities implement `ISoftDeletable`. Deletions set `IsDeleted = true` rather than removing records. Global query filter excludes soft-deleted records.

**Auditing:** Entities implement `IAuditable`. `SaveChangesAsync` automatically sets `CreatedAt/CreatedById`, `UpdatedAt/UpdatedById`, `DeletedAt/DeletedById` with UTC timestamps.

**Result Pattern:** Service methods return `Result<T>` from `Application/Common/Models/Result.cs`. Use `Result.Success(value)` or `Result.Failure(error, errorCode)`. Controllers call `HandleResult()` to map to HTTP responses.

**Custom Exceptions:** Throw `ValidationException`, `NotFoundException`, `UnauthorizedException`, `ForbiddenException` from `Application/Common/Exceptions/`. `ExceptionHandlingMiddleware` maps these to HTTP status codes (400, 404, 401, 403).

## Adding New Features

**New Entity:** Create in `Domain/Entities/`, inherit from `BaseEntity`, implement `IMultiTenant` if tenant-scoped. Add DbSet in `ApplicationDbContext`, create configuration in `Infrastructure/Data/Configurations/`.

**New Endpoint:** Add controller in `API/Controllers/` inheriting `BaseController`. Create DTOs and validators in `Application/Features/<FeatureName>/`. Add service interface in `Application/Interfaces/`, implement in `API/Services/` or `Infrastructure/`.

**New Validator:** Create `<RequestName>Validator` class in `Application/Features/<Feature>/Validators/` using FluentValidation. Auto-registered via `DependencyInjection.cs`.

## Technology Stack

- PostgreSQL with EF Core 9.0 (Npgsql)
- Redis for caching (StackExchange.Redis) via `ICacheService`
- SignalR for real-time updates (`/hubs/project`)
- JWT Bearer authentication with `tenant_id` claim
- FluentValidation for request validation
- AutoMapper for object mapping (profiles in `Application/Mappings/`)
- Serilog for structured logging

## Database

Connection configured in `appsettings.Development.json`. Entity configurations in `Infrastructure/Data/Configurations/`. Migrations in `Infrastructure/Data/Migrations/`.

Global filters applied in `ApplicationDbContext.ConfigureGlobalFilters()` for tenant isolation and soft-delete filtering.

## Key Files

- `EnterpriseCore.API/Program.cs` - DI setup, middleware pipeline, auth configuration
- `EnterpriseCore.API/Middleware/ExceptionHandlingMiddleware.cs` - Global error handling
- `EnterpriseCore.API/Services/CurrentUserService.cs` - Extracts user/tenant from JWT claims
- `EnterpriseCore.Infrastructure/Data/ApplicationDbContext.cs` - EF context with global filters and audit logic
- `EnterpriseCore.Application/Common/Models/Result.cs` - Result pattern implementation
