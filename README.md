# OnlyUsersService Microservice

`OnlyUsersService` is a high-performance, full-stack microservice built with **ASP.NET Core 8 (.NET 8)**, **Entity Framework Core 8**, **SQL Server**, and **JWT Bearer Authentication**.

It replicates the architectural pattern of `CMS.UserManagement`, featuring:
- Standardized `BaseResponseModel` with RFC-compliant HTTP status mapping (`CustomBaseController`).
- Layered Clean Architecture: Controllers, Domain Services, Repositories (Generic & Entity-specific), EF Core Data Model, and centralized Common utilities.
- OpenAPI / Swagger documentation with integrated JWT Bearer authentication testing.
- Automatic database creation and EF Core migrations on application startup.
- Dual-role Role-Based Access Control (`Admin`, `User`).
- Embedded interactive Single Page Application (SPA) dashboard served at root (`/`) and Swagger UI at (`/swagger`).

---

## 1. Solution & Project Structure

The solution is located at `D:\ONLYUSERSSERVICE`:

```text
D:\ONLYUSERSSERVICE\
├── OnlyUsersService.sln                           # Visual Studio Solution file
├── OnlyUsersService_DB_Setup.sql                  # Standalone idempotent SQL migration script
├── docker-compose.yml                             # Docker Compose stack (API + SQL Server)
├── start-service.bat                              # Quick one-click service launcher
├── test-service.ps1                               # Automated end-to-end verification script
├── README.md                                      # Full system documentation
│
├── OnlyUsersService.API\                          # ASP.NET Core 8 Web API Project
│   ├── OnlyUsersService.API.csproj
│   ├── Program.cs                                 # Host bootstrap & DB auto-migration/seeding
│   ├── Startup.cs                                 # Service DI, JWT, Swagger, and Pipeline config
│   ├── appsettings.json                           # Database connection & security config
│   ├── appsettings.Development.json
│   ├── Dockerfile                                 # Multi-stage container build
│   │
│   ├── Common\                                    # Replicated CMS.UserManagement patterns
│   │   ├── AppRoles.cs                            # Role definitions (Admin, User)
│   │   ├── ConfigureSwaggerOptions.cs             # Versioned OpenAPI & Bearer auth config
│   │   ├── SwaggerDefaultValues.cs                # Swagger parameter & schema filters
│   │   ├── CorrelationIdMiddleware.cs             # Distributed tracing (X-Correlation-ID)
│   │   ├── CustomBaseController.cs                # Unified HandleResult() response mapper
│   │   ├── GlobalActivityHandler.cs               # Action execution timing & tracing
│   │   ├── GlobalExceptionHandler.cs              # Unhandled exception standardizer
│   │   ├── ICurrentUserContext.cs                 # HttpContext user claims abstraction
│   │   ├── CurrentUserContext.cs
│   │   └── ServiceExtensions.cs                   # Clean service/repo DI registration
│   │
│   ├── Data\
│   │   ├── OnlyUsersDbContext.cs                  # EF Core DbContext with Fluent API mappings
│   │   └── DbInitializer.cs                       # Auto-seeds default roles and admin/user
│   │
│   ├── Migrations\                                # EF Core Migrations
│   │   └── 20261014160853_InitialCreate.cs
│   │
│   ├── Models\
│   │   ├── Entities\                              # Database Entities
│   │   │   ├── User.cs                            # User entity
│   │   │   ├── Role.cs                            # Role entity
│   │   │   ├── UserRole.cs                        # Composite join entity (UserId, RoleId)
│   │   │   └── UserProfile.cs                     # Profile details (Bio, Dept, Title, etc.)
│   │   ├── RequestModels\                         # Inbound DTOs
│   │   │   ├── Auth\LoginRequestModel.cs
│   │   │   ├── Auth\RegisterRequestModel.cs
│   │   │   ├── Users\CreateUserRequestModel.cs
│   │   │   ├── Users\UpdateUserRequestModel.cs
│   │   │   ├── Users\UpdateProfileRequestModel.cs
│   │   │   └── Users\AssignRoleRequestModel.cs
│   │   └── ResponseModels\                        # Outbound DTOs
│   │       ├── BaseModels\BaseResponseModel.cs    # Unified response envelope
│   │       ├── BaseModels\ApiResponseCode.cs      # Standard response codes
│   │       ├── Auth\LoginResponseModel.cs
│   │       └── Users\UserResponseModel.cs
│   │
│   ├── Repositories\
│   │   ├── Interface\IGenericRepository.cs
│   │   ├── Interface\IUserRepository.cs
│   │   ├── Interface\IRoleRepository.cs
│   │   ├── Implementation\GenericRepository.cs
│   │   ├── Implementation\UserRepository.cs
│   │   └── Implementation\RoleRepository.cs
│   │
│   ├── Services\
│   │   ├── Interface\IJwtAuthService.cs
│   │   ├── Interface\IUserService.cs
│   │   ├── Interface\IRoleService.cs
│   │   ├── Implementation\JwtAuthService.cs
│   │   ├── Implementation\UserService.cs
│   │   └── Implementation\RoleService.cs
│   │
│   ├── Controllers\
│   │   └── UsersController.cs                     # API Endpoints
│   │
│   └── wwwroot\
│       └── index.html                             # Full-Stack Interactive SPA Dashboard
│
└── OnlyUsersService.Tests\                        # xUnit Unit & Integration Test Suite
    ├── OnlyUsersService.Tests.csproj
    ├── JwtAuthServiceTests.cs                     # Auth, JWT, and BCrypt unit tests
    └── UserServiceTests.cs                        # User CRUD & role assignment tests
```

---

## 2. Database & Auto-Creation

- **Database Name:** `OnlyUsersService_DB`
- **ORM:** Entity Framework Core 8 (`Microsoft.EntityFrameworkCore.SqlServer`)
- **DbContext:** `OnlyUsersDbContext`
- **Automatic Migration & Auto-Creation:**
  On service startup, `Program.cs` automatically invokes:
  ```csharp
  await dbContext.Database.MigrateAsync();
  await DbInitializer.SeedAsync(dbContext, logger);
  ```
  If `OnlyUsersService_DB` does not exist, SQL Server creates it and applies all migrations automatically.
- **Connection String (`appsettings.json`):**
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OnlyUsersService_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
    "SqlExpressConnection": "Server=.\\SQLEXPRESS;Database=OnlyUsersService_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
  ```

---

## 3. Pre-Seeded Default Accounts

The database is pre-seeded with two accounts for testing:

| Username | Password | Role | Description |
| :--- | :--- | :--- | :--- |
| `admin` | `Admin@123456` | `Admin` | Full administrative privileges (user CRUD, role management) |
| `john_doe` | `User@123456` | `User` | Standard user (profile management, self view/edit) |

Passwords are encrypted using standard **SHA-256** cryptographic hashing (`IEncryptionService`), with built-in backward compatibility for BCrypt hashes.

---

## 4. Parameterless Security Design (No User ID or Office ID in Parameters)

In accordance with strict microservice security practices:
1. **Zero Office ID Dependency:** The service completely decouples from CMS legacy structures and requires **NO `OfficeID`** anywhere across endpoints, models, or database schemas.
2. **Context-Driven User Identity:** When an authenticated user interacts with their profile or roles, **NO `UserID` is passed in parameters, routes, or body payloads**.
   - `GET /api/users/profile` and `GET /api/users/me`: Automatically extracts `UserId` from the verified JWT claims (`ICurrentUserContext`).
   - `PUT /api/users/profile`: Updates current caller's profile with zero ID parameters.
   - `GET /api/users/my-roles`: Returns caller's assigned roles with zero ID parameters.
   - `GET /api/users/roles`: Defaults to caller's roles if target user ID is omitted.

---

## 5. API Endpoints Table

All endpoints are available under both `/api/users/...` and versioned `/api/v1/users/...`.

| Method | Endpoint | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/users/hash` (or `/encrypt`) | Anonymous | Compute SHA-256 hash for raw data and optional salt (No User ID or Office ID) |
| `POST` | `/api/users/register` | Anonymous | Register a new user account with SHA-256 password (defaults to `User` role) |
| `POST` | `/api/users/login` | Anonymous | Authenticate credentials (SHA-256 / BCrypt) and receive a JWT Bearer token |
| `GET` | `/api/users/profile` (or `/me`) | `[Authorize]` | Get authenticated user's profile (**No User ID or Office ID parameter**) |
| `PUT` | `/api/users/profile` | `[Authorize]` | Update authenticated user's profile (**No User ID or Office ID parameter**) |
| `GET` | `/api/users/my-roles` | `[Authorize]` | Get authenticated user's assigned roles (**No User ID or Office ID parameter**) |
| `GET` | `/api/users` | `[Authorize(Roles = "Admin")]` | Get all users (with pagination and search) |
| `POST` | `/api/users` (or `/create`) | `[Authorize(Roles = "Admin")]` | Create a new user with specific role(s) |
| `GET` | `/api/users/{id}` | `[Authorize]` | Get user by ID (Self or Admin) |
| `PUT` | `/api/users/{id}` | `[Authorize]` | Update user by ID (Self or Admin) |
| `DELETE` | `/api/users/{id}` | `[Authorize(Roles = "Admin")]` | Delete user by ID (Admin only) |
| `GET` | `/api/users/roles` | `[Authorize]` | Get all available roles, or user roles (defaults to caller if ID omitted) |
| `POST` | `/api/users/roles` | `[Authorize(Roles = "Admin")]` | Assign role(s) to a user (Admin only; defaults to caller if ID omitted) |
| `DELETE`| `/api/users/roles` | `[Authorize(Roles = "Admin")]` | Remove role(s) from a user (Admin only; defaults to caller if ID omitted) |

---

## 5. Testing Directly in Swagger UI

1. Open Swagger UI at: **`http://localhost:5200/swagger`**
2. Scroll to `POST /api/users/login`:
   - Click **Try it out**.
   - Use default admin credentials:
     ```json
     {
       "username": "admin",
       "password": "Admin@123456"
     }
     ```
   - Click **Execute**. Copy the `token` string from the JSON response.
3. Scroll to the top right of the Swagger UI page and click the green **Authorize** button:
   - Paste the token into the value box and click **Authorize**, then **Close**.
4. Now test any protected endpoint (such as `GET /api/users` or `POST /api/users/roles`). Swagger automatically attaches the `Authorization: Bearer <token>` header.

---

## 6. How to Run

### Option A: Quick Batch Launch
Double-click `start-service.bat` in `D:\ONLYUSERSSERVICE` or run:
```powershell
.\start-service.bat
```

### Option B: .NET CLI
```powershell
cd D:\ONLYUSERSSERVICE
dotnet run --project OnlyUsersService.API --urls "http://localhost:5200"
```

### Option C: Run Unit & Integration Tests
```powershell
cd D:\ONLYUSERSSERVICE
dotnet test OnlyUsersService.sln
```

### Option D: Run End-to-End Automated Verification Script
```powershell
powershell -ExecutionPolicy Bypass -File D:\ONLYUSERSSERVICE\test-service.ps1
```

### Option E: Containerization with Docker Compose
```powershell
docker-compose up --build -d
```
The service will be accessible on `http://localhost:5200` with an isolated SQL Server 2022 container.
