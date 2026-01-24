# ServiceHub Pro - Multi-Tenant Business Operations Platform

A production-style, full-stack multi-tenant platform for managing business operations including customers, appointments, work orders, inventory, invoicing, and more.

## 🏗️ Architecture

- **Backend**: ASP.NET Core 8 Web API with Clean Architecture
- **Frontend**: React 18 + TypeScript + Vite + TailwindCSS
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity + JWT (access/refresh tokens)
- **Architecture Pattern**: Clean Architecture (Domain, Application, Infrastructure, API layers)
- **CQRS**: MediatR for command/query separation
- **Validation**: FluentValidation
- **Logging**: Serilog

## 📁 Project Structure

```
.
├── backend/
│   ├── ServiceHubPro.API/          # Web API layer
│   ├── ServiceHubPro.Application/  # Application layer (CQRS, DTOs, Validators)
│   ├── ServiceHubPro.Domain/        # Domain entities and interfaces
│   └── ServiceHubPro.Infrastructure/ # Data access, services, configurations
├── frontend/                        # React + TypeScript application
├── docker-compose.yml               # Docker Compose for local development
└── README.md                        # This file
```

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (or use Docker Compose)
- Docker Desktop (optional, for containerized setup)

### Option 1: Docker Compose (Recommended)

1. **Start services:**
   ```bash
   docker-compose up -d
   ```

2. **Run database migrations:**
   ```bash
   cd backend
   dotnet ef database update --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API
   ```

3. **Access the application:**
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000
   - Frontend: http://localhost:5173

### Option 2: Local Development

#### Backend Setup

1. **Update connection string** in `backend/ServiceHubPro.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=ServiceHubProDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

2. **Create database and run migrations:**
   ```bash
   cd backend
   dotnet ef database update --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API
   ```

3. **Run the API:**
   ```bash
   cd ServiceHubPro.API
   dotnet run
   ```

   API will be available at: http://localhost:5000
   Swagger UI: http://localhost:5000

#### Frontend Setup

1. **Install dependencies:**
   ```bash
   cd frontend
   npm install
   ```

2. **Start development server:**
   ```bash
   npm run dev
   ```

   Frontend will be available at: http://localhost:5173

## 🔐 Default Credentials

After seeding, you can login with:

- **Business Owner:**
  - Email: `owner@demo.com`
  - Password: `Password123!`

- **Manager:**
  - Email: `manager@demo.com`
  - Password: `Password123!`

## 📋 Features

### ✅ Implemented

1. **Authentication & Security**
   - User registration (creates tenant automatically)
   - JWT-based authentication with access/refresh tokens
   - Role-based access control (RBAC)
   - Tenant isolation at database level

2. **Multi-Tenant Architecture**
   - Tenant isolation enforced via TenantId
   - Automatic tenant assignment on entity creation
   - Global query filters for soft-deleted entities

3. **Customer Management**
   - CRUD operations for customers
   - Search and pagination
   - Customer notes and tags

4. **Seed Data**
   - Demo tenant "Demo Auto Care"
   - Sample users, customers, services, products, appointments

### 🚧 In Progress / Planned

5. **Services & Pricing Catalog**
6. **Scheduling / Appointments** (with conflict detection)
7. **Work Orders / Job Tracking**
8. **Inventory & Purchase Orders**
9. **Invoicing & Payments** (with PDF generation)
10. **Notifications Center**
11. **Reports & Dashboards**
12. **Audit Logs / Compliance**

## 🧪 Testing

### Backend Tests

```bash
cd backend
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm test
```

## 📚 API Documentation

Once the API is running, access Swagger UI at:
- http://localhost:5000

The Swagger UI includes:
- All available endpoints
- Request/response schemas
- JWT authentication support (click "Authorize" button)

## 🔧 Configuration

### Backend Configuration

Key settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!",
    "Issuer": "ServiceHubPro",
    "Audience": "ServiceHubPro",
    "AccessTokenExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "7"
  }
}
```

### Frontend Configuration

Create `.env` file in `frontend/`:

```env
VITE_API_URL=http://localhost:5000/api
```

## 🗄️ Database Migrations

### Create a new migration:
```bash
cd backend
dotnet ef migrations add MigrationName --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API
```

### Apply migrations:
```bash
dotnet ef database update --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API
```

## 🏗️ Clean Architecture Layers

- **Domain**: Core business entities and interfaces (no dependencies)
- **Application**: Use cases, DTOs, validators, CQRS handlers (depends on Domain)
- **Infrastructure**: Data access, external services, EF Core (depends on Domain and Application)
- **API**: Controllers, middleware, startup (depends on all layers)

## 🔒 Security Features

- JWT token-based authentication
- Refresh token rotation
- Tenant isolation at database level
- Role-based authorization
- Input validation with FluentValidation
- Global exception handling
- Soft delete pattern

## 📦 Key Dependencies

### Backend
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- ASP.NET Core Identity
- MediatR (CQRS)
- FluentValidation
- Serilog
- AutoMapper

### Frontend
- React 18
- TypeScript
- Vite
- TailwindCSS
- React Query (@tanstack/react-query)
- React Router
- Axios

## 🤝 Contributing

This is a portfolio project demonstrating full-stack development capabilities.

## 📝 License

This project is for portfolio/demonstration purposes.

## 🎯 Next Steps

1. Complete remaining modules (Appointments, Work Orders, Invoicing, etc.)
2. Add unit tests for business logic
3. Add E2E tests with Playwright
4. Implement background jobs for appointment reminders
5. Add PDF invoice generation
6. Implement dashboard with charts and KPIs
7. Add audit logging
8. Create Postman collection
9. Set up GitHub Actions CI/CD

## 📞 Contact

For questions or feedback, please open an issue in the repository.

---

**Built with ❤️ using Clean Architecture and modern web technologies**
