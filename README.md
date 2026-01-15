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
EnterpriseCore.API            # Controllers, Middleware, SignalR Hubs
EnterpriseCore.Application    # DTOs, Validators, Services, Mappings
EnterpriseCore.Domain         # Entities, Enums, Interfaces
EnterpriseCore.Infrastructure # EF Core, Repositories, Caching
EnterpriseCore.Tests          # Unit Tests (xUnit, FluentAssertions, Moq)
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

### 2. Configure Environment Variables

Set the following environment variables (recommended for production):

```bash
# Required
export DATABASE_CONNECTION_STRING="Host=localhost;Database=EnterpriseCore;Username=postgres;Password=yourpassword"
export JWT_SECRET_KEY="your-secret-key-min-32-characters"

# Optional
export REDIS_CONNECTION_STRING="localhost:6379"
```

Or update `EnterpriseCore.API/appsettings.Development.json` for local development:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EnterpriseCore;Username=postgres;Password=yourpassword"
  }
}
```

### 3. Run Migrations

```bash
dotnet ef database update --project EnterpriseCore.Infrastructure --startup-project EnterpriseCore.API
```

### 4. Run the API

```bash
dotnet run --project EnterpriseCore.API
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

### Users (RBAC)
```
GET    /api/users                  # List users
GET    /api/users/{id}             # Get user
POST   /api/users                  # Create user
PUT    /api/users/{id}             # Update user
DELETE /api/users/{id}             # Delete user
PUT    /api/users/{id}/roles       # Assign roles
```

### Roles & Permissions
```
GET    /api/roles                  # List roles
POST   /api/roles                  # Create role
PUT    /api/roles/{id}             # Update role
PUT    /api/roles/{id}/permissions # Assign permissions
GET    /api/permissions            # List all permissions
```

### Sprints
```
GET    /api/projects/{id}/sprints  # List sprints
POST   /api/projects/{id}/sprints  # Create sprint
PUT    /api/sprints/{id}           # Update sprint
POST   /api/sprints/{id}/start     # Start sprint
POST   /api/sprints/{id}/complete  # Complete sprint
POST   /api/sprints/{id}/tasks/{taskId}    # Add task to sprint
DELETE /api/sprints/{id}/tasks/{taskId}    # Remove task from sprint
```

### Attachments
```
POST   /api/attachments            # Upload file
GET    /api/attachments/{id}       # Get attachment info
GET    /api/attachments/{id}/download      # Download file
DELETE /api/attachments/{id}       # Delete attachment
GET    /api/tasks/{id}/attachments         # List task attachments
GET    /api/projects/{id}/attachments      # List project attachments
```

### Activity Logs
```
GET    /api/activity-logs          # List activities (with filters)
GET    /api/activity-logs/entity/{type}/{id}   # Get entity history
```

### Dashboard
```
GET    /api/dashboard/summary              # Overall summary
GET    /api/dashboard/projects/{id}/report # Project report
GET    /api/dashboard/my-stats             # Personal statistics
GET    /api/dashboard/team-workload        # Team workload
```

## Features

- **Multi-Tenancy**: Automatic tenant isolation via global query filters
- **Soft Deletes**: Records marked as deleted, not removed
- **Audit Trail**: Automatic tracking of created/updated/deleted timestamps
- **RBAC**: Role-based access control with permissions
- **Permission-Based Auth**: Fine-grained access control with HasPermissionAttribute
- **Sprint Management**: Agile sprint/iteration support
- **File Attachments**: Upload/download files for tasks and projects
- **Activity Logging**: Track all entity changes with query support
- **Dashboard**: Analytics, reports, and team workload statistics
- **Caching**: Redis with fallback to NullCacheService
- **Real-time**: SignalR hub for live updates (`/hubs/project`)

## Security

- **Security Headers**: HSTS, CSP, X-Frame-Options, X-Content-Type-Options, X-XSS-Protection
- **Rate Limiting**: Global (100 req/min) and AuthEndpoints (5 req/min) policies
- **Environment Variables**: Credentials via env vars (no hardcoded secrets)
- **CORS**: Restricted to configured origins

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

58 unit tests covering validators and core patterns.

## License

MIT License - see [LICENSE](LICENSE) file.

---

# EnterpriseCore (Türkçe)

ASP.NET Core 9.0 ve Clean Architecture ile geliştirilen çok kiracılı proje yönetim API'si.

## Teknoloji Yığını

| Bileşen | Teknoloji |
|---------|-----------|
| Framework | ASP.NET Core 9.0 |
| Veritabanı | PostgreSQL |
| ORM | Entity Framework Core 9.0 |
| Önbellek | Redis |
| Gerçek Zamanlı | SignalR |
| Kimlik Doğrulama | JWT + RBAC |

## Mimari

```
EnterpriseCore.API            # Controller'lar, Middleware, SignalR Hub'ları
EnterpriseCore.Application    # DTO'lar, Validator'lar, Servisler, Mapping'ler
EnterpriseCore.Domain         # Entity'ler, Enum'lar, Interface'ler
EnterpriseCore.Infrastructure # EF Core, Repository'ler, Önbellekleme
EnterpriseCore.Tests          # Birim Testler (xUnit, FluentAssertions, Moq)
```

## Gereksinimler

- .NET 9.0 SDK
- PostgreSQL
- Redis (opsiyonel)

## Başlangıç

### 1. Klonla ve Derle

```bash
git clone https://github.com/bqrayvzdgn/EnterpriseCore.git
cd EnterpriseCore
dotnet build
```

### 2. Ortam Değişkenlerini Yapılandır

Aşağıdaki ortam değişkenlerini ayarla (production için önerilir):

```bash
# Zorunlu
export DATABASE_CONNECTION_STRING="Host=localhost;Database=EnterpriseCore;Username=postgres;Password=sifreniz"
export JWT_SECRET_KEY="en-az-32-karakterlik-gizli-anahtar"

# Opsiyonel
export REDIS_CONNECTION_STRING="localhost:6379"
```

Veya yerel geliştirme için `EnterpriseCore.API/appsettings.Development.json` dosyasını güncelle:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EnterpriseCore;Username=postgres;Password=sifreniz"
  }
}
```

### 3. Migration'ları Çalıştır

```bash
dotnet ef database update --project EnterpriseCore.Infrastructure --startup-project EnterpriseCore.API
```

### 4. API'yi Çalıştır

```bash
dotnet run --project EnterpriseCore.API
```

- API: http://localhost:5017
- Swagger: http://localhost:5017/swagger
- Sağlık Kontrolü: http://localhost:5017/health

## API Endpoint'leri

### Kimlik Doğrulama
```
POST /api/auth/register    # Kiracı + admin kullanıcı oluştur
POST /api/auth/login       # JWT token al
POST /api/auth/refresh     # Token yenile
```

### Projeler
```
GET    /api/projects           # Projeleri listele
GET    /api/projects/{id}      # Proje detayı
POST   /api/projects           # Proje oluştur
PUT    /api/projects/{id}      # Proje güncelle
DELETE /api/projects/{id}      # Proje sil (soft delete)
```

### Görevler
```
GET    /api/projects/{id}/tasks    # Görevleri listele
GET    /api/tasks/{id}             # Görev detayı
POST   /api/projects/{id}/tasks    # Görev oluştur
PUT    /api/tasks/{id}             # Görev güncelle
DELETE /api/tasks/{id}             # Görev sil
```

### Kullanıcılar (RBAC)
```
GET    /api/users                  # Kullanıcıları listele
GET    /api/users/{id}             # Kullanıcı detayı
POST   /api/users                  # Kullanıcı oluştur
PUT    /api/users/{id}             # Kullanıcı güncelle
DELETE /api/users/{id}             # Kullanıcı sil
PUT    /api/users/{id}/roles       # Rol ata
```

### Roller ve İzinler
```
GET    /api/roles                  # Rolleri listele
POST   /api/roles                  # Rol oluştur
PUT    /api/roles/{id}             # Rol güncelle
PUT    /api/roles/{id}/permissions # İzin ata
GET    /api/permissions            # Tüm izinleri listele
```

### Sprint'ler
```
GET    /api/projects/{id}/sprints  # Sprint'leri listele
POST   /api/projects/{id}/sprints  # Sprint oluştur
PUT    /api/sprints/{id}           # Sprint güncelle
POST   /api/sprints/{id}/start     # Sprint başlat
POST   /api/sprints/{id}/complete  # Sprint tamamla
POST   /api/sprints/{id}/tasks/{taskId}    # Görevi sprint'e ekle
DELETE /api/sprints/{id}/tasks/{taskId}    # Görevi sprint'ten çıkar
```

### Dosya Ekleri
```
POST   /api/attachments            # Dosya yükle
GET    /api/attachments/{id}       # Ek bilgisi
GET    /api/attachments/{id}/download      # Dosya indir
DELETE /api/attachments/{id}       # Eki sil
GET    /api/tasks/{id}/attachments         # Görev eklerini listele
GET    /api/projects/{id}/attachments      # Proje eklerini listele
```

### Aktivite Logları
```
GET    /api/activity-logs          # Aktiviteleri listele (filtreli)
GET    /api/activity-logs/entity/{type}/{id}   # Entity geçmişi
```

### Dashboard
```
GET    /api/dashboard/summary              # Genel özet
GET    /api/dashboard/projects/{id}/report # Proje raporu
GET    /api/dashboard/my-stats             # Kişisel istatistikler
GET    /api/dashboard/team-workload        # Ekip iş yükü
```

## Özellikler

- **Çoklu Kiracılık**: Global query filter'lar ile otomatik kiracı izolasyonu
- **Soft Delete**: Kayıtlar silinmez, silinmiş olarak işaretlenir
- **Denetim İzi**: Oluşturma/güncelleme/silme zaman damgaları otomatik takibi
- **RBAC**: İzin tabanlı rol erişim kontrolü
- **İzin Tabanlı Yetkilendirme**: HasPermissionAttribute ile hassas erişim kontrolü
- **Sprint Yönetimi**: Agile sprint/iterasyon desteği
- **Dosya Ekleri**: Görev ve projeler için dosya yükleme/indirme
- **Aktivite Loglama**: Tüm entity değişikliklerini sorgu desteğiyle takip
- **Dashboard**: Analitik, raporlar ve ekip iş yükü istatistikleri
- **Önbellekleme**: NullCacheService fallback ile Redis
- **Gerçek Zamanlı**: Canlı güncellemeler için SignalR hub'ı (`/hubs/project`)

## Güvenlik

- **Güvenlik Header'ları**: HSTS, CSP, X-Frame-Options, X-Content-Type-Options, X-XSS-Protection
- **Rate Limiting**: Global (100 istek/dk) ve AuthEndpoints (5 istek/dk) politikaları
- **Ortam Değişkenleri**: Kimlik bilgileri env var ile (hardcoded secret yok)
- **CORS**: Yapılandırılmış origin'lerle kısıtlı

## Test

```bash
# Tüm testleri çalıştır
dotnet test

# Kapsam ile çalıştır
dotnet test --collect:"XPlat Code Coverage"
```

Validator'lar ve temel pattern'leri kapsayan 58 birim test.

## Lisans

MIT Lisansı - [LICENSE](LICENSE) dosyasına bakınız.
