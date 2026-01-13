# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-01-13

### Added

- Initial project setup with Clean Architecture
- Domain layer with core entities (Tenant, User, Role, Permission, Project, TaskItem, Milestone)
- Multi-tenancy support with global query filters
- Soft delete functionality
- Automatic audit trail (created/updated/deleted tracking)
- JWT authentication with refresh tokens
- Role-based access control (RBAC)
- PostgreSQL database with EF Core 9.0
- Redis caching support
- SignalR hub for real-time updates
- Auth endpoints (register, login, refresh)
- Projects CRUD endpoints
- Tasks CRUD endpoints
- FluentValidation for request validation
- AutoMapper for object mapping
- Serilog structured logging
- Swagger/OpenAPI documentation
- Health check endpoint
