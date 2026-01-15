# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.4.0] - 2026-01-15

### Added

- **Security Improvements**:
  - SecurityHeadersMiddleware (X-Content-Type-Options, X-Frame-Options, HSTS, CSP, X-XSS-Protection, Referrer-Policy, Permissions-Policy)
  - Rate limiting with global limit (100 req/min) and AuthEndpoints policy (5 req/min)
  - Environment variable support for credentials (JWT_SECRET_KEY, DATABASE_CONNECTION_STRING)
  - SignalR ProjectHub authorization with project access verification
- **Test Project**: EnterpriseCore.Tests with 58 unit tests
  - Validator tests (ResetPassword, ActivityLogQuery, UploadAttachment)
  - Result pattern tests
  - Test fixtures (TestDataFactory, MockCurrentUserService)
- **New Validators**: 5 new request validators
  - RefreshTokenRequestValidator
  - ForgotPasswordRequestValidator
  - ResetPasswordRequestValidator
  - ActivityLogQueryRequestValidator
  - UploadAttachmentRequestValidator
- **Swagger Documentation**: ProducesResponseType attributes on all controllers
- **XML Documentation**: Interface documentation for IAuthService, IProjectService, ITaskService, IUserService, IRoleService, ICacheService

### Changed

- CORS restricted to configured origins (no longer AllowAll)
- TokenService catch block now logs exceptions properly
- ICacheService moved from Infrastructure to Application layer (Clean Architecture)
- Role entity now has proper tenant filter in ApplicationDbContext

### Fixed

- N+1 query issues in DashboardService (server-side aggregation)
- Permissions caching with ICacheService

### Security

- Removed hardcoded credentials from appsettings
- Added rate limiting to prevent brute force attacks
- Added security headers for XSS, clickjacking, and content-type sniffing protection

## [0.3.0] - 2026-01-14

### Added

- **RBAC System**: Full user, role, and permission management
  - UsersController, RolesController, PermissionsController
  - Permission-based authorization with HasPermissionAttribute
  - UserService, RoleService, PermissionService implementations
- **Sprint/Iteration Feature**: Agile sprint management
  - Sprint entity with Planning, Active, Completed, Cancelled statuses
  - SprintsController with start/complete sprint actions
  - Task-to-sprint assignment
- **Attachment System**: File upload and download
  - Attachment entity with multi-tenant support
  - LocalFileStorageService for file storage
  - AttachmentsController with upload/download endpoints
- **Activity Log Queries**: Activity tracking and filtering
  - ActivityLogsController with entity-based queries
  - Pagination support for activity logs
- **Dashboard & Reports**: Analytics and statistics
  - DashboardController with summary, project reports, my-stats, team-workload
  - Task completion percentages, member workloads
- **Performance Improvements**:
  - Repository pagination (GetPagedAsync, GetByIdWithIncludesAsync)
  - Cache strategy with GetOrSetAsync and CacheKeys
  - NullCacheService fallback when Redis unavailable
  - Redis optimization with SCAN and batch delete
  - Performance indexes migration
- **New Validators**: 12 new request validators
  - Project: UpdateProjectRequest, AddProjectMemberRequest
  - Task: UpdateTaskRequest, UpdateTaskStatusRequest, AssignTaskRequest, CreateCommentRequest
  - User: CreateUserRequest, UpdateUserRequest, AssignRolesRequest
  - Role: CreateRoleRequest, UpdateRoleRequest, AssignPermissionsRequest
  - Sprint: CreateSprintRequest, UpdateSprintRequest

### Changed

- TaskItem entity now supports SprintId for sprint assignment
- Repository interface extended with pagination and Include support
- ICacheService extended with GetOrSetAsync method
- RedisCacheService optimized for bulk operations

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

## [0.4.0] - 2026-01-15

### Eklendi

- **Güvenlik İyileştirmeleri**:
  - SecurityHeadersMiddleware (X-Content-Type-Options, X-Frame-Options, HSTS, CSP, X-XSS-Protection, Referrer-Policy, Permissions-Policy)
  - Global limit (100 istek/dk) ve AuthEndpoints politikası (5 istek/dk) ile rate limiting
  - Kimlik bilgileri için ortam değişkeni desteği (JWT_SECRET_KEY, DATABASE_CONNECTION_STRING)
  - Proje erişim doğrulaması ile SignalR ProjectHub yetkilendirmesi
- **Test Projesi**: 58 birim testiyle EnterpriseCore.Tests
  - Validator testleri (ResetPassword, ActivityLogQuery, UploadAttachment)
  - Result pattern testleri
  - Test fixture'ları (TestDataFactory, MockCurrentUserService)
- **Yeni Validator'lar**: 5 yeni istek validator'ı
  - RefreshTokenRequestValidator
  - ForgotPasswordRequestValidator
  - ResetPasswordRequestValidator
  - ActivityLogQueryRequestValidator
  - UploadAttachmentRequestValidator
- **Swagger Dokümantasyonu**: Tüm controller'larda ProducesResponseType attribute'ları
- **XML Dokümantasyonu**: IAuthService, IProjectService, ITaskService, IUserService, IRoleService, ICacheService interface dokümantasyonu

### Değiştirildi

- CORS yapılandırılmış origin'lerle kısıtlandı (artık AllowAll değil)
- TokenService catch bloğu artık exception'ları düzgün logluyor
- ICacheService Infrastructure'dan Application katmanına taşındı (Clean Architecture)
- Role entity'si artık ApplicationDbContext'te düzgün tenant filtresine sahip

### Düzeltildi

- DashboardService'teki N+1 sorgu sorunları (sunucu tarafı agregasyon)
- ICacheService ile izin önbellekleme

### Güvenlik

- appsettings'ten hardcoded kimlik bilgileri kaldırıldı
- Brute force saldırılarını önlemek için rate limiting eklendi
- XSS, clickjacking ve content-type sniffing koruması için güvenlik header'ları eklendi

## [0.3.0] - 2026-01-14

### Eklendi

- **RBAC Sistemi**: Tam kullanıcı, rol ve izin yönetimi
  - UsersController, RolesController, PermissionsController
  - HasPermissionAttribute ile izin tabanlı yetkilendirme
  - UserService, RoleService, PermissionService implementasyonları
- **Sprint/Iterasyon Özelliği**: Agile sprint yönetimi
  - Planning, Active, Completed, Cancelled durumlarıyla Sprint entity'si
  - Sprint başlatma/tamamlama aksiyonlarıyla SprintsController
  - Görev-sprint ataması
- **Dosya Eki Sistemi**: Dosya yükleme ve indirme
  - Çoklu kiracılık destekli Attachment entity'si
  - Dosya depolama için LocalFileStorageService
  - Yükleme/indirme endpoint'leriyle AttachmentsController
- **Aktivite Logu Sorguları**: Aktivite takibi ve filtreleme
  - Entity bazlı sorgularla ActivityLogsController
  - Aktivite logları için sayfalama desteği
- **Dashboard ve Raporlar**: Analitik ve istatistikler
  - Özet, proje raporları, kişisel istatistikler, ekip iş yükü ile DashboardController
  - Görev tamamlanma yüzdeleri, üye iş yükleri
- **Performans İyileştirmeleri**:
  - Repository sayfalama (GetPagedAsync, GetByIdWithIncludesAsync)
  - GetOrSetAsync ve CacheKeys ile önbellek stratejisi
  - Redis kullanılamadığında NullCacheService fallback
  - SCAN ve toplu silme ile Redis optimizasyonu
  - Performans index'leri migration'ı
- **Yeni Validator'lar**: 12 yeni istek validator'ı
  - Proje: UpdateProjectRequest, AddProjectMemberRequest
  - Görev: UpdateTaskRequest, UpdateTaskStatusRequest, AssignTaskRequest, CreateCommentRequest
  - Kullanıcı: CreateUserRequest, UpdateUserRequest, AssignRolesRequest
  - Rol: CreateRoleRequest, UpdateRoleRequest, AssignPermissionsRequest
  - Sprint: CreateSprintRequest, UpdateSprintRequest

### Değiştirildi

- TaskItem entity'si artık sprint ataması için SprintId destekliyor
- Repository interface'i sayfalama ve Include desteğiyle genişletildi
- ICacheService GetOrSetAsync metoduyla genişletildi
- RedisCacheService toplu işlemler için optimize edildi

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
