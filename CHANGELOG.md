# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2026-01-14

### Changed

- Restructured project layout: moved projects from `src/` to solution root
- Fixed Entity Framework Core version mismatch (9.0.1 -> 9.0.2)
- Fixed OpenAPI/Swagger version compatibility (10.1.0 -> 7.2.0)
- Made Redis connection optional with graceful fallback
- Added `.claude/` and `CLAUDE.md` to `.gitignore`

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

---

# Değişiklik Günlüğü (Türkçe)

Bu projede yapılan önemli değişiklikler bu dosyada belgelenecektir.

## [Yayınlanmamış]

## [0.2.0] - 2026-01-14

### Değiştirildi

- Proje yapısı yeniden düzenlendi: projeler `src/` klasöründen solution root'a taşındı
- Entity Framework Core sürüm uyumsuzluğu giderildi (9.0.1 -> 9.0.2)
- OpenAPI/Swagger sürüm uyumluluğu düzeltildi (10.1.0 -> 7.2.0)
- Redis bağlantısı opsiyonel hale getirildi
- `.claude/` ve `CLAUDE.md` dosyaları `.gitignore`'a eklendi

## [0.1.0] - 2026-01-13

### Eklendi

- Clean Architecture ile ilk proje kurulumu
- Temel entity'lerle Domain katmanı (Tenant, User, Role, Permission, Project, TaskItem, Milestone)
- Global query filter'lar ile çoklu kiracılık desteği
- Soft delete işlevselliği
- Otomatik denetim izi (oluşturma/güncelleme/silme takibi)
- Refresh token'lı JWT kimlik doğrulama
- Rol tabanlı erişim kontrolü (RBAC)
- EF Core 9.0 ile PostgreSQL veritabanı
- Redis önbellekleme desteği
- Gerçek zamanlı güncellemeler için SignalR hub'ı
- Auth endpoint'leri (register, login, refresh)
- Projects CRUD endpoint'leri
- Tasks CRUD endpoint'leri
- İstek doğrulama için FluentValidation
- Nesne eşleme için AutoMapper
- Serilog yapılandırılmış loglama
- Swagger/OpenAPI dokümantasyonu
- Sağlık kontrolü endpoint'i
