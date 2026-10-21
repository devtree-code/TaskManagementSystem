# Task Management System — Backend & Admin Portal

A modular, production-ready backend built with **ASP.NET Core 8**, designed to serve a **Flutter mobile application** for task submissions while providing administrators with a **Web Management Dashboard** and **dynamic Excel reporting**.

---

## 🌟 Key Features

1. **Mobile-Ready RESTful API (for Flutter)**:
   - **User Authentication**: Secure JWT token generation for user registration and login.
   - **Task Submission**: Employees record what task they did and the exact date/time they did it.
   - **Timeframe Filtering**: Mobile users retrieve their submitted history ordered and filtered by **last week**, **last month**, **last year**, or custom date windows.
2. **Admin Web Dashboard (Razor MVC)**:
   - Modern, responsive web portal built with **Bootstrap 5**.
   - View all tasks submitted across the entire organization.
   - Dynamic interactive filters:
     - Filter by **specific employee** or view all users.
     - Filter by **quick time presets** (Last 7 Days, Last Month, Last Year, All Time).
     - Filter by explicit **From Date** and **To Date** pickers.
3. **Advanced Excel Export (ClosedXML)**:
   - Export filtered task reports directly into styled `.xlsx` workbooks with a single click.
   - Features customized styling: branded title banner, filter summary, steel blue header rows, zebra striping, and auto-adjusted column dimensions.
4. **Clean Architecture & Dependency Injection**:
   - Structured following a strict 3-tier architecture: **DAL** (Data Access Layer), **BL** (Business Logic Layer), and **Controllers**.
   - Fully decoupled using the **Repository Pattern** and interface-driven services.
5. **Dual Authentication Scheme**:
   - **JWT Bearer Authentication** for mobile REST API endpoints (`/api/*`).
   - **Cookie Authentication** for the Admin web portal (`/admin/*`).

---

## 🏗️ Architecture & Folder Structure

```
TaskManagementSystem/
├── BL/                                  # Business Logic Layer
│   ├── DTOs/                            # Data Transfer Objects & ViewModels
│   │   ├── Admin/                       # AdminDashboardViewModel, AdminLoginViewModel
│   │   ├── Auth/                        # RegisterDto, LoginDto, AuthResponseDto, UserDto
│   │   └── Tasks/                       # CreateTaskDto, TaskResponseDto, TaskFilterDto
│   └── Services/                        # Business Services
│       ├── AuthService.cs               # Identity auth, token generation, user listings
│       ├── TaskService.cs               # Task management & filter logic
│       └── ExcelExportService.cs        # ClosedXML spreadsheet generation
├── Controllers/                         # Presentation Layer
│   ├── AdminController.cs               # MVC Controller for Razor Dashboard & Excel export
│   ├── AuthController.cs                # Web API for user registration & login
│   └── TasksController.cs               # Web API for mobile task operations
├── DAL/                                 # Data Access Layer
│   ├── Data/
│   │   ├── AppDbContext.cs              # EF Core context (IdentityDbContext)
│   │   └── DbInitializer.cs             # Automatic schema migration & initial seed data
│   ├── Models/
│   │   ├── ApplicationUser.cs           # Custom IdentityUser entity
│   │   └── UserTask.cs                  # UserTask entity with Description, TaskDate, CreatedAt
│   └── Repo/
│       ├── ITaskRepository.cs           # Task repository interface
│       └── TaskRepository.cs            # EF Core query implementation
├── Views/                               # Razor Views for Admin Portal
│   ├── Admin/
│   │   ├── Index.cshtml                 # Main Admin Dashboard with table & filters
│   │   └── Login.cshtml                 # Admin browser login page
│   └── Shared/
│       └── _Layout.cshtml               # Bootstrap 5 layout & navbar
├── Program.cs                           # Dependency injection & HTTP pipeline bootstrapper
└── appsettings.json                     # Database connection string & JWT settings
```

---

## 🚀 Quick Start Guide

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or **MSSQLLocalDB** (included by default with Visual Studio)

### 1. Run the Project
From the repository root:
```powershell
cd TaskManagementSystem
dotnet run
```
The application will start and listen on:
- **HTTP**: `http://localhost:5153`
- **HTTPS**: `https://localhost:7139`

> **Note**: On startup, `DbInitializer` will automatically create the database (`TaskManagementDb`), apply EF Core migrations, and seed the default roles and administrator account!

---

## 🔑 Default Credentials

### Administrator Portal Account
- **URL**: `http://localhost:5153/admin` (or `http://localhost:5153/admin/login`)
- **Email**: `admin@tasksystem.com`
- **Password**: `Admin@123456`

---

## 💻 Admin Web Portal & Excel Export

1. Open your browser and navigate to:
   ```
   http://localhost:5153/admin
   ```
2. Log in using the admin credentials above.
3. Use the filter bar to:
   - Select a specific employee or view all employees.
   - Choose a quick timeframe (**Last Week**, **Last Month**, **Last Year**, or **All Time**).
   - Or pick custom **From Date** and **To Date** boundaries.
4. Click **Apply** to filter the displayed table.
5. Click **Export to Excel** (green button) to download a formatted `.xlsx` file preserving the exact active filter criteria.

---

## 📱 Mobile (Flutter) API Integration Guide

Base URL: `http://localhost:5153` (or your local network IP/emulator IP, e.g. `10.0.2.2:5153` for Android emulator).

### 1. Register User
`POST /api/auth/register`
```json
{
  "fullName": "Ahmed Fathalla",
  "email": "ahmed@example.com",
  "password": "Password123!"
}
```
**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "ahmed@example.com",
  "fullName": "Ahmed Fathalla",
  "roles": ["User"],
  "expiresAt": "2026-11-20T02:00:00Z"
}
```

### 2. Login User
`POST /api/auth/login`
```json
{
  "email": "ahmed@example.com",
  "password": "Password123!"
}
```
**Response (200 OK):** Returns JWT token and profile payload.

### 3. Submit Task
`POST /api/tasks`  
**Headers**: `Authorization: Bearer <token>`
```json
{
  "title": "Flutter SQLite Cache",
  "description": "Implemented offline caching for task history using sqflite package.",
  "taskDate": "2026-10-21T08:30:00.000Z"
}
```
**Response (201 Created):**
```json
{
  "id": 1,
  "title": "Flutter SQLite Cache",
  "description": "Implemented offline caching for task history using sqflite package.",
  "taskDate": "2026-10-21T08:30:00",
  "createdAt": "2026-10-21T08:32:15",
  "userId": "b4737d9e-13c5-430b-b27b-2878bfce31aa",
  "userName": "Ahmed Fathalla",
  "userEmail": "ahmed@example.com"
}
```

### 4. View Submitted Tasks (With Timeframe Filter)
`GET /api/tasks/my-tasks?period=week`  
**Headers**: `Authorization: Bearer <token>`

**Allowed values for `period`**:
- `week`: Past 7 days.
- `month`: Past 30 days.
- `year`: Past 365 days.
- `all`: All recorded tasks.

You can also use custom range query params:
`GET /api/tasks/my-tasks?fromDate=2026-10-01&toDate=2026-10-21`

---

## 📑 Swagger / OpenAPI Documentation

Interactive Swagger documentation is available out of the box:
```
http://localhost:5153/swagger
```
You can use the **Authorize** button in Swagger to enter your Bearer token and test endpoints interactively.

---

## 🧪 Automated Verification Script

An automated end-to-end verification script is included in the project:
```powershell
powershell -ExecutionPolicy Bypass -File verify_system.ps1
```
This script validates:
- User registration and JWT issuance.
- Task submissions across multiple dates (today, 4 days ago, 20 days ago, 50 days ago).
- Week, month, and all-time query filters for the Flutter API.
- Admin portal login with Anti-Forgery validation.
- Admin dashboard rendering.
- Excel spreadsheet export (`.xlsx`) generation and integrity.

---

## ⚙️ Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Key": "TaskManagementSystem_SuperSecretKey_2026_Secure_Key!#%",
    "Issuer": "TaskManagementAPI",
    "Audience": "TaskManagementClient",
    "DurationInDays": 30
  }
}
```
You can adjust the database connection string to point to any SQL Server instance or production environment.
