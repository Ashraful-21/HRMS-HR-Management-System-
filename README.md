# HRMS — Enterprise Human Resource Management System

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.3-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-6.0-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Vite](https://img.shields.io/badge/Vite-8.3-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Hangfire](https://img.shields.io/badge/Hangfire-1.8-red)](https://www.hangfire.io/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--Time-blue)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![QuestPDF](https://img.shields.io/badge/QuestPDF-2025.1-orange)](https://www.questpdf.com/)
[![Tests](https://img.shields.io/badge/xUnit-8%2F8%20Passed-brightgreen)](tests/HRMS.Tests)

An end-to-end, full-stack, enterprise-grade Human Resource Management System (HRMS) built for modern organizations. Designed with clean architectural principles, robust security (JWT + RBAC with 48 granular permissions), background scheduling with Hangfire, real-time SignalR notifications, QuestPDF automated payslip generation, and an ultra-modern luxury dark glassmorphic single-page application.

---

## 📑 Table of Contents
- [Architecture Overview](#architecture-overview)
- [Enterprise Modules](#enterprise-modules)
- [Technology Stack](#technology-stack)
- [Project Directory Structure](#project-directory-structure)
- [Prerequisites](#prerequisites)
- [Getting Started & Installation](#getting-started--installation)
  - [1. Database Setup](#1-database-setup)
  - [2. Backend API Setup](#2-backend-api-setup)
  - [3. Frontend Web Setup](#3-frontend-web-setup)
- [Default Login Credentials](#default-login-credentials)
- [API Documentation & Health Monitoring](#api-documentation--health-monitoring)
- [Running Automated Tests](#running-automated-tests)
- [Security & RBAC Matrix](#security--rbac-matrix)

---

## 🏛️ Architecture Overview

The system follows Clean Architecture patterns with strict domain separation, standard `ApiResponse<T>` envelopes, asynchronous I/O across all services, audit trail interception, and idempotent database seeding.

```
                           ┌──────────────────────────────────────────────┐
                           │      React 19 + TypeScript (Vite SPA)        │
                           │  - Luxury Dark Glassmorphism UI (index.css)  │
                           │  - Real-time Notifications Bell Drawer       │
                           │  - 11 Domain Workspaces & Modal Workflows    │
                           └──────────────────────┬───────────────────────┘
                                                  │ HTTP REST / SignalR
                                                  ▼
                           ┌──────────────────────────────────────────────┐
                           │          .NET 10 Web API Gateway             │
                           │  - JWT Bearer Auth & Refresh Tokens          │
                           │  - Granular RBAC [HasPermission] Filters     │
                           │  - AspNetCoreRateLimit & Serilog Middleware  │
                           └───────┬──────────────┬───────────────┬───────┘
                                   │              │               │
            ┌──────────────────────┴───┐   ┌──────┴────────┐   ┌──┴─────────────────────┐
            │   Entity Framework Core  │   │  Hangfire     │   │  SignalR Hub           │
            │   Async Linq Repositories│   │  Job Worker   │   │  /hubs/notifications   │
            └──────────────┬───────────┘   └──────┬────────┘   └────────────────────────┘
                           │                      │
                           ▼                      ▼
            ┌───────────────────────────────────────────────────────────────────────────┐
            │                     Microsoft SQL Server (HRMS DB)                        │
            │      63 Normalized Tables • Views • Foreign Keys • Hangfire Storage       │
            └───────────────────────────────────────────────────────────────────────────┘
```

---

## 💼 Enterprise Modules

### 1. 📊 Executive Dashboard
- **Workforce KPIs**: Total headcount, active staff, probation employees, new hires.
- **Real-Time Attendance**: Present today, late arrivals, early departures, attendance rate percentage.
- **Leave Analytics**: Active leave applications, pending approvals.
- **Financial Summary**: Latest payroll run gross and net disbursement totals.
- **Interactive Visualizations**: Department distribution bar chart and company announcements banner.
- **Audit Feed**: Real-time log of administrative and system activities.

### 2. 👥 Workforce & Employee Management
- Complete employee directory with multi-criteria search and department/branch filters.
- Comprehensive employee records: Job profile, manager hierarchy, dates of joining/confirmation.
- Document repository, banking information, home address, and emergency contact details.
- Soft-delete support and employment status lifecycles (`Active`, `Probation`, `Notice`, `Resigned`, `Terminated`).

### 3. ⏱️ Attendance & Time-Tracking
- **Digital Punch Clock**: Instant Check-In and Check-Out with IP address tracking.
- **Daily Attendance Logs**: In/Out timestamps, punch source, working hours, and overtime calculations.
- **Regularization Workflow**: Employee request portal for missing punches and manager review/approval.
- **Monthly Summary**: Aggregated present, absent, late, and half-day counts.

### 4. 🏖️ Leave Management
- Multi-tier leave policies: Annual/Casual Leave, Sick Leave, Maternity/Paternity Leave, Unpaid Leave.
- Real-time leave balance computation, accrual logic, and remaining quota tracking.
- Multi-step application submission, document attachment, and manager approval/rejection pipeline.

### 5. 💰 Payroll & Compensation Engine
- Configurable salary structures: Basic pay, House Rent, Medical Allowance, Conveyance, Special Allowances.
- Statutory deductions: Tax withholding, Provident Fund, and custom penalties.
- Batch payroll run processing with automated net pay computation.
- **QuestPDF Engine**: High-fidelity, print-ready PDF payslip generation and download.

### 6. 🎯 Recruitment & Applicant Tracking System (ATS)
- Job opening management: Title, vacancies, department, required experience, and salary brackets.
- Candidate pipeline tracking through stages: `Applied` ➔ `Screening` ➔ `Interview` ➔ `Offered` ➔ `Hired` ➔ `Rejected`.
- Interview scheduling with panel assignment, round types, and interviewer feedback scorecards.
- Offer letter generation with joining dates and offered salary.

### 7. 🏆 Performance Management
- Review cycle orchestration (Annual, Semi-Annual, Quarterly).
- Employee goal setting with weighted milestones and target dates.
- Multi-tier evaluations: Employee self-assessment and manager review scores.

### 8. 💻 Asset & Hardware Inventory
- Asset categorization (Laptops, Desktops, Monitors, Mobile Devices, Peripherals).
- Hardware lifecycle management: Purchase date, cost, serial number, and operational condition.
- Employee custody allocation and return inspection workflow with condition notes.

### 9. 🏢 Organization Hierarchy
- Multi-branch and department management with hierarchical trees.
- Designations, pay grades, and organizational structures.
- Work shift scheduling (Standard, Morning, Night, Flexible) and public holiday calendar.

### 10. 📈 Reports & Analytics
- Attendance summary register with date-range filters.
- Leave utilization and balance depletion reports.
- Monthly payroll disbursement sheets with printable formats.

### 11. 🛡️ Security & System Administration
- User account provisioning with password hashing via `PasswordHasher<User>`.
- Role-Based Access Control (RBAC) with **48 granular permissions**.
- Audit logging of all state mutations (entity type, action, timestamp, user, payload).
- Global system settings management (working days, company policies, localization).

### 12. 🔔 Real-Time Communications
- Company-wide broadcast announcements with priority flags (`Urgent`, `Normal`).
- SignalR WebSocket notifications with unread count badge and drawer panel.

---

## 🛠️ Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Backend Framework** | .NET 10 (C# 12), ASP.NET Core Web API |
| **Database & ORM** | Microsoft SQL Server 2019+, Entity Framework Core |
| **Authentication & Security** | JWT (JSON Web Tokens), Refresh Tokens, ASP.NET Core Identity Password Hasher, Rate Limiting |
| **Background Processing** | Hangfire with SQL Server Storage |
| **Real-Time Communication** | ASP.NET Core SignalR |
| **PDF Generation** | QuestPDF 2025 |
| **Frontend Framework** | React 19, TypeScript, Vite |
| **Styling & Aesthetics** | Pure Vanilla CSS (Luxury Dark Glassmorphism, CSS Tokens, Custom Scrollbars) |
| **Icons & Assets** | Lucide React |
| **HTTP Client** | Axios |
| **Unit Testing** | xUnit, Microsoft.EntityFrameworkCore.InMemory |
| **Logging & Diagnostics** | Serilog (Console & Rolling File Sinks), Health Checks |

---

## 📂 Project Directory Structure

```text
d:\Project\HRMS\
├── database/
│   ├── HRMS_schema.sql                  # Complete SQL Server schema (63 tables, views, keys)
│   └── changes/                         # Database migration delta scripts
├── HRMS/                                # Core .NET 10 Web API Project
│   ├── Auth/                            # JWT token generation, Claims, [HasPermission] filter
│   ├── Constants/                       # Permissions list, Roles, AuditActions, AppConstants
│   ├── Controllers/                     # REST Controllers for all 10 domain modules
│   ├── Data/                            # HrmsDbContext & DbSeeder (demo data initialization)
│   ├── Dtos/                            # Strongly typed Request & Response DTOs
│   ├── Entities/ & Models/              # EF Core database entities and navigation models
│   ├── Extensions/                      # DI ServiceCollectionExtensions, Middleware registration
│   ├── Hubs/                            # SignalR NotificationHub
│   ├── IServices/                       # Contracts for business logic services
│   ├── Services/                        # Business logic implementations
│   ├── appsettings.json                 # Connection strings, JWT secrets, Rate limits
│   └── Program.cs                       # Application entry point & pipeline configuration
├── tests/
│   ├── HRMS.Tests/                      # xUnit test project
│   │   ├── RecruitmentServiceTests.cs   # Unit tests for Job openings & Candidate pipeline
│   │   ├── PerformanceServiceTests.cs   # Unit tests for Review cycles & Goals
│   │   ├── AssetServiceTests.cs         # Unit tests for Hardware allocation & Returns
│   │   ├── AdminServiceTests.cs         # Unit tests for Users & System settings
│   │   ├── DashboardServiceTests.cs     # Unit tests for Executive KPI computation
│   │   └── TestHelpers.cs               # InMemory DbContext & Mock service factories
│   └── verify_endpoints.ps1             # PowerShell script verifying all 19 live endpoints
└── frontend/                            # React 19 + TypeScript Frontend
    ├── src/
    │   ├── components/                  # Domain views (Dashboard, Employees, Attendance, etc.)
    │   ├── App.tsx                      # Main app shell, Top navigation bar & Auth handler
    │   ├── index.css                    # Luxury dark-mode glassmorphism design system
    │   └── main.tsx                     # React root mount
    ├── package.json                     # Frontend dependencies and build scripts
    └── vite.config.ts                   # Vite bundler configuration
```

---

## ⚙️ Prerequisites

Before running the project locally, ensure you have the following installed:

1. **.NET 10 SDK** (or .NET 9 SDK with preview enabled)
   - Verify: `dotnet --version`
2. **Node.js** (v18.x or later) & **npm**
   - Verify: `node -v` and `npm -v`
3. **Microsoft SQL Server** (2019, 2022, or SQL Express)
   - Running locally on `.\SQLEXPRESS` or your configured server instance.

---

## 🚀 Getting Started & Installation

### 1. Database Setup
The application features **automatic database seeding**. On first launch, `DbSeeder.cs` checks for existing data and automatically seeds:
- Branches, Departments, Designations, Work Shifts, Pay Grades
- SuperAdmin account with all 48 permissions
- Demo employees, attendance logs, leave balances, assets, job openings, and performance cycles.

*Optional*: If you prefer manually creating the database schema first:
```bash
sqlcmd -S .\SQLEXPRESS -E -Q "CREATE DATABASE HRMS;"
sqlcmd -S .\SQLEXPRESS -d HRMS -i database/HRMS_schema.sql
```

### 2. Backend API Setup
1. Verify database connection string in `HRMS/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "Default": "Server=.\\SQLEXPRESS;Database=HRMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
   }
   ```
2. Restore and run the backend project:
   ```bash
   dotnet restore HRMS/HRMS.csproj
   dotnet run --project HRMS/HRMS.csproj --launch-profile http
   ```
3. The API will start on: **`http://localhost:5219`**
   - Health check: `http://localhost:5219/health`
   - OpenAPI Specification: `http://localhost:5219/openapi/v1.json`
   - Scalar API Reference: `http://localhost:5219/scalar/v1`
   - Hangfire Dashboard: `http://localhost:5219/hangfire`

### 3. Frontend Web Setup
1. Open a new terminal and navigate to the `frontend` directory:
   ```bash
   cd frontend
   npm install
   ```
2. Start the Vite development server:
   ```bash
   npm run dev
   ```
3. Open your browser and navigate to: **`http://localhost:5173`**

---

## 🔑 Default Login Credentials

The system initializes with a pre-configured Super Administrator account:

| Attribute | Value |
| :--- | :--- |
| **Login URL** | `http://localhost:5173` |
| **Username** | `superadmin` |
| **Password** | `Admin@123456` |
| **Role** | `SuperAdmin` (All 48 Permissions) |
| **Quick Action** | Click the **"Autofill SuperAdmin"** button on the login screen |

---

## 🧪 Running Automated Tests

### xUnit Service Tests
Run all unit tests in the test suite:
```bash
dotnet test tests/HRMS.Tests/HRMS.Tests.csproj
```
**Test Results**:
```text
Test run for HRMS.Tests.dll (.NETCoreApp,Version=v10.0)
Passed!  - Failed: 0, Passed: 8, Skipped: 0, Total: 8, Duration: 3 s
```

### Live Endpoints Integration Verification
Run the endpoint validation script against the running backend:
```powershell
powershell -ExecutionPolicy Bypass -File "tests/verify_endpoints.ps1"
```

---

## 🛡️ Security & RBAC Matrix

The system enforces granular Role-Based Access Control with **48 permissions** mapped across modules:

- `employees:read`, `employees:write`, `employees:delete`, `employees:export`
- `attendance:read`, `attendance:write`, `attendance:regularize`, `attendance:reports`
- `leave:read`, `leave:apply`, `leave:approve`, `leave:manage-types`
- `payroll:read`, `payroll:process`, `payroll:approve`, `payroll:reports`
- `recruitment:read`, `recruitment:write`, `recruitment:interviews`, `recruitment:offers`
- `performance:read`, `performance:write`, `performance:cycles`, `performance:appraise`
- `assets:read`, `assets:write`, `assets:assign`, `assets:return`
- `organization:read`, `organization:write`
- `reports:read`, `reports:export`
- `users:read`, `users:write`, `roles:read`, `roles:manage`, `settings:manage`, `audit:read`

Role permissions can be customized dynamically in real-time through the **Administration** workspace.

---

## 📄 License
This project is licensed under the MIT License — see the LICENSE file for details.
