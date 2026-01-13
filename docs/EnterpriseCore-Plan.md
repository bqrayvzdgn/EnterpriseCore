# EnterpriseCore - Proje Planı

## Proje Özeti

**EnterpriseCore** - Çok kiracılı (multi-tenant) proje yönetim sistemi API'si.

| Özellik | Değer |
|---------|-------|
| Framework | ASP.NET Core 9.0 |
| Mimari | Clean Architecture |
| Veritabanı | PostgreSQL |
| ORM | Entity Framework Core |
| Cache | Redis |
| Real-time | SignalR |
| Auth | JWT + RBAC |

---

## Proje Yapısı

```
EnterpriseCore/
├── EnterpriseCore.sln
├── src/
│   ├── EnterpriseCore.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Filters/
│   │   └── Hubs/
│   ├── EnterpriseCore.Application/
│   │   ├── Common/
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   ├── Projects/
│   │   │   ├── Tasks/
│   │   │   ├── Milestones/
│   │   │   ├── Users/
│   │   │   └── Roles/
│   │   ├── Interfaces/
│   │   └── Mappings/
│   ├── EnterpriseCore.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   └── ValueObjects/
│   └── EnterpriseCore.Infrastructure/
│       ├── Data/
│       ├── Repositories/
│       ├── Services/
│       └── Caching/
└── tests/
    ├── EnterpriseCore.UnitTests/
    └── EnterpriseCore.IntegrationTests/
```

---

## Domain Modelleri

### Core Entities

```
Tenant (Kiracı/Organizasyon)
├── Id, Name, Slug, Settings
├── SubscriptionPlan (Free/Pro/Enterprise)
└── Users[], Projects[]

User (Kullanıcı)
├── Id, Email, PasswordHash, FirstName, LastName
├── TenantId (FK)
└── UserRoles[]

Role (Rol)
├── Id, Name, Description
├── TenantId (FK) - null ise sistem rolü
└── RolePermissions[]

Permission (İzin)
├── Id, Name, Code (projects.create, tasks.delete, vb.)
└── Description

Project (Proje)
├── Id, Name, Description, Status
├── TenantId (FK)
├── OwnerId (FK → User)
├── StartDate, EndDate, Budget
├── Tasks[], Milestones[], Members[]

ProjectMember (Proje Üyesi)
├── ProjectId, UserId
├── Role (Owner/Manager/Member/Viewer)

Task (Görev)
├── Id, Title, Description
├── ProjectId (FK)
├── AssigneeId (FK → User)
├── Status (Todo/InProgress/Review/Done)
├── Priority (Low/Medium/High/Critical)
├── DueDate, EstimatedHours, ActualHours
├── ParentTaskId (alt görevler için)
└── Comments[], Attachments[]

Milestone (Kilometre Taşı)
├── Id, Name, Description
├── ProjectId (FK)
├── DueDate, CompletedDate
└── Tasks[]

TaskComment (Yorum)
├── Id, Content, CreatedAt
├── TaskId (FK)
└── UserId (FK)

ActivityLog (Aktivite Kaydı)
├── Id, Action, EntityType, EntityId
├── UserId, TenantId
├── OldValues, NewValues (JSON)
└── CreatedAt
```

---

## API Endpoints

### Auth
```
POST   /api/auth/register          - Yeni tenant + admin kullanıcı
POST   /api/auth/login             - JWT token al
POST   /api/auth/refresh           - Token yenile
POST   /api/auth/forgot-password   - Şifre sıfırlama emaili
POST   /api/auth/reset-password    - Şifre sıfırla
```

### Users & Roles (RBAC)
```
GET    /api/users                  - Kullanıcı listesi
GET    /api/users/{id}             - Kullanıcı detay
POST   /api/users                  - Kullanıcı oluştur
PUT    /api/users/{id}             - Kullanıcı güncelle
DELETE /api/users/{id}             - Kullanıcı sil
POST   /api/users/{id}/roles       - Rol ata

GET    /api/roles                  - Rol listesi
POST   /api/roles                  - Rol oluştur (özel)
PUT    /api/roles/{id}             - Rol güncelle
DELETE /api/roles/{id}             - Rol sil
GET    /api/permissions            - Tüm izinler
POST   /api/roles/{id}/permissions - İzin ata
```

### Projects
```
GET    /api/projects               - Proje listesi (paginated)
GET    /api/projects/{id}          - Proje detay
POST   /api/projects               - Proje oluştur
PUT    /api/projects/{id}          - Proje güncelle
DELETE /api/projects/{id}          - Proje sil (soft)
GET    /api/projects/{id}/stats    - Proje istatistikleri
POST   /api/projects/{id}/members  - Üye ekle
DELETE /api/projects/{id}/members/{userId} - Üye çıkar
```

### Tasks
```
GET    /api/projects/{projectId}/tasks     - Görev listesi
GET    /api/tasks/{id}                     - Görev detay
POST   /api/projects/{projectId}/tasks     - Görev oluştur
PUT    /api/tasks/{id}                     - Görev güncelle
DELETE /api/tasks/{id}                     - Görev sil
PATCH  /api/tasks/{id}/status              - Durum güncelle
PATCH  /api/tasks/{id}/assign              - Atama yap
GET    /api/tasks/my                       - Bana atanan görevler
```

### Milestones
```
GET    /api/projects/{projectId}/milestones
POST   /api/projects/{projectId}/milestones
PUT    /api/milestones/{id}
DELETE /api/milestones/{id}
```

### Comments
```
GET    /api/tasks/{taskId}/comments
POST   /api/tasks/{taskId}/comments
PUT    /api/comments/{id}
DELETE /api/comments/{id}
```

### Dashboard & Reports
```
GET    /api/dashboard              - Genel dashboard
GET    /api/reports/projects       - Proje raporu
GET    /api/reports/users          - Kullanıcı performansı
GET    /api/reports/timeline       - Zaman çizelgesi
```

---

## Teknik Detaylar

### Multi-Tenancy Stratejisi

**Shared Database + TenantId Column** yaklaşımı:
- Her entity'de `TenantId` kolonu
- Global query filter ile otomatik filtreleme
- JWT token'da `tenant_id` claim'i

### RBAC (Role-Based Access Control)

**Sistem Rolleri** (değiştirilemez):
- `SuperAdmin` - Tüm yetkiler
- `Admin` - Tenant yönetimi
- `Member` - Temel erişim

**Özel Roller** (tenant bazlı):
- Tenant admin'leri özel rol oluşturabilir
- Her role izinler atanabilir

**İzin Yapısı**:
```
projects.view, projects.create, projects.edit, projects.delete
tasks.view, tasks.create, tasks.edit, tasks.delete, tasks.assign
users.view, users.create, users.edit, users.delete
roles.manage
reports.view
```

### Caching Stratejisi (Redis)

- User sessions
- Permission cache (kullanıcı izinleri)
- Project summaries
- Dashboard data
- Rate limiting counters

### SignalR Events

```csharp
// Hub: /hubs/project
TaskCreated(taskId, projectId)
TaskUpdated(taskId, changes)
TaskStatusChanged(taskId, oldStatus, newStatus)
TaskAssigned(taskId, assigneeId)
CommentAdded(taskId, commentId)
MilestoneCompleted(milestoneId)
```

---

## Uygulama Adımları

### Adım 1: Solution ve Proje Yapısı
- [ ] Solution oluştur
- [ ] 4 proje ekle (API, Application, Domain, Infrastructure)
- [ ] Proje referanslarını ayarla
- [ ] NuGet paketlerini ekle

### Adım 2: Domain Katmanı
- [ ] Base entity'ler (BaseEntity, IAuditable, ISoftDeletable, IMultiTenant)
- [ ] Enum'lar (ProjectStatus, TaskStatus, TaskPriority)
- [ ] Core entity'ler (Tenant, User, Role, Permission, Project, Task, Milestone)
- [ ] Repository interface'leri

### Adım 3: Infrastructure Katmanı
- [ ] DbContext ve entity configuration'lar
- [ ] Global query filters (tenant + soft delete)
- [ ] Repository implementasyonları
- [ ] Unit of Work pattern
- [ ] Redis cache service

### Adım 4: Application Katmanı
- [ ] DTOs ve AutoMapper profilleri
- [ ] FluentValidation validator'ları
- [ ] Service interface ve implementasyonları
- [ ] Result pattern

### Adım 5: API Katmanı
- [ ] Controller'lar
- [ ] JWT Authentication
- [ ] Authorization (policy-based)
- [ ] Permission-based authorization handler
- [ ] Global exception handler
- [ ] Rate limiting
- [ ] SignalR hub

### Adım 6: Cross-Cutting Concerns
- [ ] Serilog logging
- [ ] Health checks
- [ ] Swagger/OpenAPI
- [ ] CORS

---

## NuGet Paketleri

### API
```
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.AspNetCore.SignalR
Swashbuckle.AspNetCore
AspNetCoreRateLimit
Serilog.AspNetCore
```

### Application
```
AutoMapper.Extensions.Microsoft.DependencyInjection
FluentValidation.AspNetCore
MediatR (opsiyonel - CQRS için)
```

### Infrastructure
```
Microsoft.EntityFrameworkCore
Npgsql.EntityFrameworkCore.PostgreSQL
Microsoft.EntityFrameworkCore.Tools
StackExchange.Redis
Microsoft.Extensions.Caching.StackExchangeRedis
```

---

## Doğrulama

Projenin çalıştığını doğrulamak için:

1. **Build**: `dotnet build`
2. **Database**: Migration oluştur ve uygula
3. **Run**: `dotnet run --project src/EnterpriseCore.API`
4. **Swagger**: `https://localhost:5001/swagger` adresini kontrol et
5. **Register**: Yeni tenant + user oluştur
6. **Login**: JWT token al
7. **CRUD**: Proje ve görev oluştur/listele
8. **SignalR**: WebSocket bağlantısını test et
