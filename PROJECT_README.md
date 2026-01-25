# ServiceHub Pro - Multi-Tenant Business Operations Platform

A production-style, full-stack multi-tenant platform for managing business operations including customers, appointments, work orders, inventory, invoicing, payments, and more.

**Created by:** Ruchika Rajodiya  
**Portfolio Project:** Full-Stack Development Demonstration

## 🏗️ Architecture

- **Backend**: ASP.NET Core 8 Web API with Clean Architecture
- **Frontend**: React 18 + TypeScript + Vite + TailwindCSS
- **Database**: SQL Server LocalDB with Entity Framework Core 8
- **Authentication**: ASP.NET Core Identity + JWT (access/refresh tokens)
- **Architecture Pattern**: Clean Architecture (Domain, Application, Infrastructure, API layers)
- **CQRS**: MediatR for command/query separation
- **Validation**: FluentValidation
- **Logging**: Serilog
- **State Management**: React Query (TanStack Query) for server state
- **PDF Generation**: jsPDF (frontend) + HTML generation (backend)

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
   - API (HTTP): http://localhost:7000
   - API (HTTPS): https://localhost:7001
   - Swagger UI: http://localhost:7000/swagger
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

1. **Create `.env` file** in `frontend/` directory (if not exists):
   ```env
   VITE_API_URL=http://localhost:7000/api
   ```

2. **Install dependencies:**
   ```bash
   cd frontend
   npm install
   ```

3. **Start development server:**
   ```bash
   npm run dev
   ```

   Frontend will be available at: **http://localhost:5173**

   **Note**: Make sure the backend API is running on `http://localhost:7000` before starting the frontend.

## 🔐 Default Credentials

After seeding, you can login with:

- **Business Owner:**
  - Email: `owner@demo.com`
  - Password: `Password123!`

- **Manager:**
  - Email: `manager@demo.com`
  - Password: `Password123!`

## 📋 Features

### ✅ Fully Implemented

1. **Authentication & Security**
   - User registration (creates tenant automatically)
   - JWT-based authentication with access/refresh tokens
   - Role-based access control (RBAC): Owner, Manager, Staff
   - Tenant isolation at database level with global query filters
   - Token refresh endpoint

2. **Multi-Tenant Architecture**
   - Tenant isolation enforced via `TenantId` on all entities
   - Automatic tenant assignment on entity creation
   - Global query filters for soft-deleted entities
   - Tenant context service for current tenant resolution

3. **Customer Management (CRM)**
   - Full CRUD operations for customers
   - Search, filtering, and pagination
   - Customer notes, tags, and contact information
   - Customer history tracking

4. **Services & Pricing Catalog**
   - Service CRUD with categories
   - Pricing with tax rate support (percent 0-100)
   - Service activation/deactivation
   - Search and filtering

5. **Products & Inventory Management**
   - Product CRUD with SKU tracking
   - Inventory levels and low stock alerts
   - Location-based inventory
   - Product categories and pricing

6. **Locations Management**
   - Multi-location support per tenant
   - Location CRUD operations
   - Location-based filtering for entities

7. **Scheduling & Appointments**
   - Appointment booking with conflict detection
   - Status management (Scheduled, Completed, Cancelled)
   - Customer and service assignment
   - Appointment to Work Order conversion
   - Calendar view support

8. **Work Orders / Job Tracking**
   - Work order creation and management
   - Status workflow (Draft, InProgress, Completed, Cancelled)
   - **Work Order Items** (Services/Products) with:
     - Add/remove items dynamically
     - Quantity, unit price, and tax rate per item
     - Automatic total calculation from items
   - Customer and location assignment
   - Staff assignment
   - Internal notes and descriptions

9. **Invoicing**
   - Invoice creation from completed work orders
   - Invoice items copied from work order items
   - Automatic total calculation (subtotal + tax - discount)
   - Invoice status management (Draft, Sent, Paid, Overdue)
   - **PDF Invoice Generation**:
     - Backend endpoint: `GET /api/invoices/{id}/pdf`
     - Frontend PDF download with jsPDF fallback
     - Printable HTML invoice view
   - Invoice number auto-generation (INV-YYYYMMDD-####)

10. **Payments**
    - Payment recording against invoices
    - Payment methods (Cash, Credit Card, Check, etc.)
    - Payment status tracking
    - Partial payment support

11. **User & Staff Management**
    - User CRUD operations
    - Role assignment (Owner, Manager, Staff)
    - User activation/deactivation
    - Profile management

12. **Notifications**
    - Notification CRUD
    - Notification types (Info, Warning, Error, Success)
    - User-specific notifications
    - Read/unread status

13. **Dashboard & Analytics**
    - Dashboard statistics endpoint
    - Key metrics: customers, appointments, work orders, invoices
    - Low stock product alerts
    - Revenue summaries

14. **Import/Export (CSV)**
    - CSV import/export endpoints
    - Data validation on import
    - Export support for major entities

15. **Seed Data**
    - Demo tenant "Demo Auto Care"
    - Sample users (Owner, Manager, Staff)
    - Sample customers, services, products
    - Sample appointments and work orders
    - Pre-configured with realistic data

### 🔧 Technical Features

- **Tax Rate Handling**: Stored as percent (0-100), e.g., 8 for 8%
- **Work Order Totals**: Calculated from items server-side, persisted correctly
- **Invoice Generation**: Automatic from completed work orders with validation
- **PDF Generation**: Backend HTML-to-PDF + Frontend jsPDF fallback
- **Global Exception Handling**: Consistent error responses with ProblemDetails
- **Soft Delete Pattern**: All entities support soft deletion
- **Decimal Precision**: Configured for money (18,2) and quantities (18,4)
- **Background Jobs**: Appointment reminders and low stock alerts (with DB readiness checks)

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
VITE_API_URL=http://localhost:7000/api
```

**Important**: 
- Use HTTP (not HTTPS) for local development to avoid certificate issues
- The frontend uses React Query with retry disabled and 5-minute cache
- API calls automatically include JWT token from localStorage

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

## 🎯 Completed Features Summary

✅ **All Core Modules Implemented**:
- Authentication & Authorization (JWT + RBAC)
- Multi-Tenant Architecture
- Customer CRM
- Services & Products Catalog
- Inventory Management
- Locations Management
- Appointments Scheduling
- Work Orders with Items
- Invoicing with PDF Generation
- Payments Processing
- User & Staff Management
- Notifications System
- Dashboard & Analytics
- CSV Import/Export

## 🚀 Next Steps / Future Enhancements

1. **Testing**
   - ✅ Unit tests structure (xUnit) - Framework ready
   - ✅ E2E tests structure (Playwright) - Framework ready
   - ⏳ Write comprehensive test coverage

2. **Advanced Features**
   - ⏳ Purchase Orders module
   - ⏳ Advanced reporting with charts (Chart.js/Recharts)
   - ⏳ Audit logging for compliance
   - ⏳ Email notifications (SMTP integration)
   - ⏳ SMS notifications (Twilio integration)

3. **DevOps & Documentation**
   - ⏳ Postman collection export
   - ⏳ GitHub Actions CI/CD pipeline
   - ⏳ Docker Compose for production
   - ⏳ API versioning strategy

4. **Performance & Scalability**
   - ⏳ Caching strategy (Redis)
   - ⏳ Database indexing optimization
   - ⏳ API response compression
   - ⏳ Frontend code splitting

## 🐛 Known Issues & Fixes Applied

### ✅ Fixed Issues

1. **Work Order Totals**: Totals now persist correctly after page refresh (calculated server-side)
2. **Tax Rate Handling**: Fixed to use percent (0-100) consistently across frontend and backend
3. **Invoice Creation**: Fixed 500 errors by ensuring all required fields are populated
4. **Frontend Crashes**: Fixed undefined variable errors in InvoicesPage
5. **Port Configuration**: Standardized to HTTP 7000 / HTTPS 7001
6. **Database Connection**: Configured for SQL Server LocalDB
7. **React Query Retries**: Disabled to prevent excessive API calls
8. **HTTPS Redirection**: Disabled in Development to allow HTTP access
9. **EF Core Cascade Paths**: Fixed multiple cascade path errors with explicit DeleteBehavior configuration
10. **Shadow Foreign Keys**: Resolved TenantId1 warnings with proper relationship configuration

### 📝 Development Notes

- **Tax Rate**: Always stored and sent as percent (0-100). Use `TaxRateHelper.GetDecimalFraction()` for calculations
- **Work Order Items**: Must be added via API (`POST /api/WorkOrders/{id}/items`) to persist correctly
- **Invoice Creation**: Only works for completed work orders with at least one item
- **Database**: Uses SQL Server LocalDB by default (no separate SQL Server installation needed)
- **Frontend API**: Configured to use HTTP in development to avoid certificate issues

## 📞 Contact

For questions or feedback, please contact:
- **Email**: ruchikarajodiya25@gmail.com
- **GitHub**: [github.com/ruchikarajodiya25](https://github.com/ruchikarajodiya25)
- **LinkedIn**: [linkedin.com/in/ruchika-nareshbhai-rajodiya-05199a360](https://www.linkedin.com/in/ruchika-nareshbhai-rajodiya-05199a360)

---

**Built with ❤️ by Ruchika Rajodiya**  
*Using Clean Architecture and modern web technologies*
