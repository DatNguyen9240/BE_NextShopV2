🚀 NextShopV2 – Backend

Backend của NextShopV2 là một RESTful API được xây dựng bằng ASP.NET Core (C#), sử dụng Entity Framework Core với PostgreSQL.

Hệ thống hỗ trợ:

- 🔐 Authentication & Authorization (JWT, Role-based)
- 🛍️ Quản lý sản phẩm & danh mục
- 🛒 Giỏ hàng & Thanh toán
- 📦 Quản lý đơn hàng
- 💬 Chat realtime (SignalR)
- 🔔 Push Notification (Firebase)
- ⚙️ Background Jobs
- 🐳 Dockerized deployment

---

📑 Mục Lục

1. Giới thiệu tổng quan
2. Kiến trúc hệ thống
3. Chức năng nổi bật
4. Cấu trúc thư mục
5. Thiết kế & Flow hoạt động
6. Công nghệ sử dụng
7. Hướng dẫn chạy local
8. Docker & Deploy
9. Security & Best Practices
10. Quick Commands

---

## 1️⃣ Giới Thiệu Tổng Quan

NextShopV2 Backend được thiết kế theo mô hình Clean Architecture, tách biệt rõ:

- Domain
- Application (Business Logic)
- Infrastructure (Persistence / External Services)
- API Layer

Lợi ích: dễ mở rộng, dễ test, dễ maintain, dễ scale.

## 2️⃣ Kiến Trúc Tổng Quan

Client
  ↓
Controllers (API)
  ↓
Application Services
  ↓
Domain Entities / Interfaces
  ↓
Infrastructure (EF Core, DB, External Services)
  ↓
PostgreSQL

Pattern chuẩn: Controller → Service → Repository → DbContext

## 3️⃣ Chức Năng Nổi Bật

### 🔐 Authentication & Authorization

- Register / Login
- JWT + Refresh Token
- Email Verification
- Role-based Authorization
- Password Hash (BCrypt / PBKDF2)

### 🛍️ Product Management

- CRUD sản phẩm
- Categories
- Variants
- Upload hình ảnh (Local / S3 / Azure Blob)
- Background image optimization

### 🔎 Search & Filter

- Full-text search (Postgres tsvector + GIN index)
- Pagination
- Sorting
- Facets filtering
- Có thể nâng cấp lên Elasticsearch

### 🛒 Cart & Checkout

- Cart cho user login
- Checkout flow: tạo Order → reserve stock → tạo payment intent → redirect payment URL → webhook update status

### 💳 Payments

Interface-based design (`IPaymentService`). Hệ thống hiện chỉ triển khai hai phương thức:

- **PayOS**: gateway online. `IPaymentService` cho PayOS chịu trách nhiệm tạo payment intent, trả về redirect/checkout URL, và có endpoint webhook để verify signature và cập nhật trạng thái `Order`.
- **COD (Cash On Delivery)**: khi khách chọn COD, order được tạo ở trạng thái chờ thu tiền khi giao hàng. Xử lý ở service: đánh dấu phương thức thanh toán là COD, không gọi PayOS.

### 📦 Order Management

Order Status Flow:

Pending → Paid → Processing → Shipped → Completed
                       ↓
                   Cancelled

- Transactional stock update
- Domain events
- Async notification (email / push)

### 💬 Realtime Chat (SignalR)

- ChatHub
- Room-based messaging
- Store message DB
- Offline → Push Notification (Firebase)

### 🔔 Notifications

- Email (SMTP / SendGrid)
- Push (Firebase Admin SDK)
- Background queue + retry policy

### ⚙️ Background Jobs

- `IHostedService` / `BackgroundService`
- Hangfire / Quartz
- Scheduled tasks (email, image processing)

## 4️⃣ Cấu Trúc Thư Mục

```
NextShopV2
│
├── NextShopV2.Api/
│   ├── Controllers/
│   ├── Hubs/
│   ├── Program.cs
│   └── appsettings.json
│
├── NextShopV2.Application/
│   ├── Services/
│   ├── DTOs/
│
├── NextShopV2.Domain/
│   ├── Entities/
│   ├── Interfaces/
│
├── NextShopV2.Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   ├── Migrations/
│   └── ExternalServices/
│
├── NextShopV2.Shared/
│   ├── Extensions/
│   ├── Middlewares/
│
├── Dockerfile
├── docker-compose.yml
└── migrations_postgres.sql
```

## 5️⃣ Thiết Kế Các Chức Năng Chính

### 🔐 Authentication Flow

Client → `AuthController` → `AuthService` → `UserRepository` → Validate password → Generate JWT + Refresh Token → Return token

Middleware mẫu:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* ... */ });
```

### 🛍️ Product CRUD Flow

Controller → ProductService → Validate + Business Logic → ProductRepository → EF Core DbContext

### 🛒 Checkout Flow (Transactional)

1. Begin Transaction
2. Create Order
3. Update Stock
4. Create Payment
5. Commit

Webhook: Verify signature → Update Order Status → Publish domain event

## 6️⃣ Công Nghệ Sử Dụng

| Category | Tech |
|---|---|
| Language | C# |
| Framework | ASP.NET Core |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Realtime | SignalR |
| Notifications | Firebase |
| Background Jobs | Hangfire / Quartz |
| Logging | Serilog |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Container | Docker |
| Cache / Queue | Redis / RabbitMQ |

## 7️⃣ Chạy Local

1) Cấu hình biến môi trường

```powershell
$env:ConnectionStrings__Default = "Host=localhost;Database=nextshop;Username=postgres;Password=postgres"
$env:Jwt__Key = "replace_with_long_secure_key"
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

2) Build & Run

```powershell
dotnet restore
dotnet build
dotnet run --project NextShopV2.Api
```

3) Apply Migrations

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update \
  --project NextShopV2.Infrastructure \
  --startup-project NextShopV2.Api
```

## 8️⃣ Docker

Build image:

```powershell
docker build -t nextshopv2-api:local -f Dockerfile .
```

Run compose:

```powershell
docker-compose up -d
docker-compose logs -f
```

## 9️⃣ Deploy Production

1) Chuẩn bị secrets (Không commit `serviceAccountKey.json`)

Sử dụng: Azure Key Vault, AWS Secrets Manager, hoặc Docker Secrets

2) Publish & Push image

```powershell
dotnet publish NextShopV2.Api -c Release -o out
docker build -t <registry>/nextshopv2-api:v1 .
docker push <registry>/nextshopv2-api:v1
```

3) Triển khai: Azure App Service / Azure Container Instances / Kubernetes / VPS + docker-compose + nginx

4) Monitoring: Seq / ELK, Prometheus + Grafana, Health Checks

## 🔐 Security Best Practices

- Bắt buộc HTTPS, Enable HSTS
- Validate input (FluentValidation)
- Rate limiting
- Webhook signature verification
- Không commit secrets
- DB backup định kỳ
- Test migration trên staging

## 🧠 Quick Commands

Run API:

```powershell
dotnet run --project NextShopV2.Api
```

Apply Migration:

```powershell
dotnet ef database update \
--project NextShopV2.Infrastructure \
--startup-project NextShopV2.Api
```

Docker Compose:

```powershell
docker-compose up -d --build
```

---

✨ Tổng Kết

NextShopV2 Backend được thiết kế theo Clean Architecture — Scalable, Production-ready, Containerized, Secure và Extensible.
