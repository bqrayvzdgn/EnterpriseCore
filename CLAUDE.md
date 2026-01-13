# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build the solution
dotnet build

# Run the API (http://localhost:5017, https://localhost:7140)
dotnet run --project src/EnterpriseCore.API

# Database migrations
dotnet ef migrations add <MigrationName> --project src/EnterpriseCore.Infrastructure --startup-project src/EnterpriseCore.API
dotnet ef database update --project src/EnterpriseCore.Infrastructure --startup-project src/EnterpriseCore.API

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

**Multi-Tenancy:** All entities implement `IMultiTenant`. Global query filters in `ApplicationDbContext` automatically scope queries by `TenantId`.

**Soft Deletes:** Entities implement `ISoftDeletable`. Deletions set `IsDeleted = true` rather than removing records. Global query filter excludes soft-deleted records.

**Auditing:** Entities implement `IAuditable`. `SaveChangesAsync` automatically sets `CreatedAt/CreatedById`, `UpdatedAt/UpdatedById`, `DeletedAt/DeletedById` with UTC timestamps.

**Result Pattern:** Service methods return `Result<T>` from `Application/Common/Models/Result.cs`. Use `Result.Success(value)` or `Result.Failure(error, errorCode)`.

**Custom Exceptions:** Throw `ValidationException`, `NotFoundException`, `UnauthorizedException`, `ForbiddenException` from `Application/Common/Exceptions/`. `ExceptionHandlingMiddleware` maps these to HTTP status codes.

## Technology Stack

- PostgreSQL with EF Core 9.0 (Npgsql)
- Redis for caching (StackExchange.Redis)
- SignalR for real-time updates (`/hubs/project`)
- JWT Bearer authentication
- FluentValidation for request validation
- AutoMapper for object mapping
- Serilog for structured logging

## Database

Connection configured in `appsettings.Development.json`. Entity configurations in `Infrastructure/Data/Configurations/`. Migrations in `Infrastructure/Data/Migrations/`.

Global filters applied in `ApplicationDbContext.ConfigureGlobalFilters()` for tenant isolation and soft-delete filtering.
