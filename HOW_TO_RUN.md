# 🚀 How to Run ServiceHub Pro

**Created by:** Ruchika Rajodiya  
**Portfolio Project:** Full-Stack Development Demonstration

## Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** and npm - [Download here](https://nodejs.org/)
- **SQL Server** (or use Docker Compose)
- **Docker Desktop** (optional, for containerized setup)

---

## Option 1: Docker Compose (Easiest - Recommended)

### Step 1: Start Database & API
```bash
# From project root directory
docker-compose up -d
```

This will:
- Start SQL Server on port 1433
- Build and start the API on port 5000
- Auto-create and seed the database

### Step 2: Start Frontend
```bash
# Open a new terminal
cd frontend
npm install
npm run dev
```

### Step 3: Access the Application
- **Frontend**: http://localhost:5173
- **API (HTTP)**: http://localhost:7000
- **API (HTTPS)**: https://localhost:7001
- **Swagger UI**: http://localhost:7000/swagger

**Default Login Credentials:**
- Email: `owner@demo.com`
- Password: `Password123!`

---

## Option 2: Local Development (Manual Setup)

### Backend Setup

#### Step 1: Install SQL Server
- Install SQL Server locally OR use Docker:
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
  ```

#### Step 2: Ensure SQL Server LocalDB is Running
SQL Server LocalDB comes with Visual Studio or SQL Server Express. Start it if needed:
```powershell
sqllocaldb start MSSQLLocalDB
```

The connection string is already configured in `backend/ServiceHubPro.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ServiceHubProDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

#### Step 3: Run Database Migrations
```bash
cd backend
dotnet ef database update --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API
```

#### Step 4: Run the API
```bash
cd backend/ServiceHubPro.API
dotnet run
```

The API will start at: **http://localhost:5000**
- Swagger UI: http://localhost:5000

### Frontend Setup

#### Step 1: Install Dependencies
```bash
cd frontend
npm install
```

#### Step 2: Configure API URL
Create `.env` file in `frontend/` directory (if not exists):
```env
VITE_API_URL=http://localhost:7000/api
```

#### Step 3: Start Development Server
```bash
npm run dev
```

The frontend will start at: **http://localhost:5173**

**Note**: Make sure the backend API is running on `http://localhost:7000` before starting the frontend.

---

## 🔐 Default Login Credentials

After database seeding, use these credentials:

**Owner Account:**
- Email: `owner@demo.com`
- Password: `Demo@123`
- Role: Owner

**Manager Account:**
- Email: `manager@demo.com`
- Password: `Demo@123`
- Role: Manager

---

## 🧪 Running Tests

### Backend Unit Tests
```bash
cd backend/ServiceHubPro.Tests
dotnet test
```

### Frontend E2E Tests (Playwright)
```bash
cd frontend
npm run test:e2e
# Or with UI:
npm run test:e2e:ui
```

---

## 📝 Quick Commands Reference

### Backend
```bash
# Build solution
cd backend
dotnet build

# Run API
cd ServiceHubPro.API
dotnet run

# Create migration
dotnet ef migrations add MigrationName --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API

# Update database
dotnet ef database update --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API
```

### Frontend
```bash
cd frontend

# Install dependencies
npm install

# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run E2E tests
npm run test:e2e
```

### Docker
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

---

## 🐛 Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Check connection string in `appsettings.json`
- Verify SQL Server port (default: 1433)
- For Docker: Wait for SQL Server to be healthy before starting API

### Port Already in Use
- **Port 7000 (HTTP)**: Change in `backend/ServiceHubPro.API/Properties/launchSettings.json`
- **Port 7001 (HTTPS)**: Change in `backend/ServiceHubPro.API/Properties/launchSettings.json`
- **Port 5173 (Frontend)**: Change in `frontend/vite.config.ts`
- **Port 1433**: Change SQL Server port in Docker or connection string

### Frontend Can't Connect to API
- Check `frontend/.env` - ensure `VITE_API_URL=http://localhost:7000/api`
- Verify CORS settings in `backend/ServiceHubPro.API/Program.cs`
- Ensure API is running on port 7000 (HTTP)
- Restart Vite dev server after changing `.env` file
- Check browser console for CORS or network errors

### Migration Errors
- Delete existing database and recreate:
  ```bash
  dotnet ef database drop --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API
  dotnet ef database update --project ServiceHubPro.Infrastructure --startup-project ServiceHubPro.API
  ```

---

## 📦 Project Structure

```
servicehub-pro/
├── backend/
│   ├── ServiceHubPro.API/          # Web API (Controllers, Middleware)
│   ├── ServiceHubPro.Application/  # Business Logic (CQRS, DTOs)
│   ├── ServiceHubPro.Domain/        # Domain Entities
│   ├── ServiceHubPro.Infrastructure/ # Data Access, Services
│   └── ServiceHubPro.Tests/         # Unit Tests
├── frontend/
│   ├── src/
│   │   ├── pages/                  # React Pages
│   │   ├── components/             # React Components
│   │   ├── services/               # API Client
│   │   └── types/                  # TypeScript Types
│   └── tests/e2e/                  # Playwright Tests
└── docker-compose.yml              # Docker Setup
```

---

## ✅ Verification Checklist

- [ ] SQL Server is running
- [ ] Database migrations applied
- [ ] Backend API starts without errors
- [ ] Swagger UI accessible at http://localhost:5000
- [ ] Frontend dependencies installed
- [ ] Frontend dev server running
- [ ] Can login with demo credentials
- [ ] Can navigate between pages

---

## 🎯 Next Steps

1. **Explore the API**: Visit http://localhost:7000/swagger for Swagger documentation
2. **Login**: Use `owner@demo.com` / `Password123!`
3. **Try Features**: 
   - Create customers
   - Book appointments
   - Create work orders and add items
   - Complete work orders
   - Generate invoices from completed work orders
   - Download invoice PDFs
   - Record payments
   - View dashboard statistics

---

## 👤 About the Developer

**Ruchika Rajodiya**  
Full-Stack Software Engineer

- **Email**: ruchikarajodiya25@gmail.com
- **GitHub**: [github.com/ruchikarajodiya25](https://github.com/ruchikarajodiya25)
- **LinkedIn**: [linkedin.com/in/ruchika-nareshbhai-rajodiya-05199a360](https://www.linkedin.com/in/ruchika-nareshbhai-rajodiya-05199a360)

**Need Help?** Check the `PROJECT_README.md` for detailed architecture and feature documentation.

---

**Built with ❤️ by Ruchika Rajodiya**
