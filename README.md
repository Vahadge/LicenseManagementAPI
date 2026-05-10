🩺 License Management System
![.NET](https://img.shields.io/badge/.NET-8-blue)  
![Next.js](https://img.shields.io/badge/Next.js-14-black)  
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red)  
![Dapper](https://img.shields.io/badge/ORM-Dapper-lightgrey)  
![Auth](https://img.shields.io/badge/Auth-JWT-green)
A full-stack Doctor License Management platform built for a Medical SaaS use case.
Admins can:
Manage doctor records
Track license expiry
Control access via JWT authentication
A background job automatically updates expired licenses daily.
---
📸 Architecture Overview
```
                    ┌────────────────────────────┐
                    │        Frontend            │
                    │  Next.js 14 (App Router)  │
                    │  Tailwind + TypeScript    │
                    └────────────┬──────────────┘
                                 │ HTTP (JWT)
                                 ▼
                    ┌────────────────────────────┐
                    │        API Layer           │
                    │ ASP.NET Core Web API (.NET 8)
                    │ Controllers + Middleware  │
                    └────────────┬──────────────┘
                                 │
                                 ▼
                    ┌────────────────────────────┐
                    │     Application Layer      │
                    │ Services + DTOs + Interfaces
                    │ Business Logic             │
                    └────────────┬──────────────┘
                                 │
                                 ▼
                    ┌────────────────────────────┐
                    │   Infrastructure Layer     │
                    │ Dapper Repositories        │
                    │ SqlConnectionFactory       │
                    │ Background Jobs            │
                    └────────────┬──────────────┘
                                 │
                                 ▼
                    ┌────────────────────────────┐
                    │       SQL Server DB        │
                    │ Tables + Stored Procedures │
                    └────────────────────────────┘
```
---
🧰 Tech Stack
Layer	Technology
API	.NET 8, ASP.NET Core
Data Access	Dapper
Database	SQL Server
Authentication	JWT
Background Jobs	Hosted Services
Frontend	Next.js 14
Styling	Tailwind CSS
---
📁 Project Structure
```
LicenseManagement/
├── LicenseManagementAPI/
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   ├── Controllers/
│   └── Common/
├── LicenseManagementClient/
└── sqlScripts/
```
---
⚙️ Setup Guide
1️⃣ Prerequisites
.NET 8 SDK
SQL Server (LocalDB or full instance)
Node.js 18+
---
2️⃣ Database Setup
Run scripts in `/sqlScripts` in order.
✔ Safe to re-run  
✔ Creates tables, SPs, seed data
Default admin user is auto-created:
Username: admin  
Password: Admin@123
---
3️⃣ Run Backend
```
cd LicenseManagementAPI
dotnet run
```
API → http://localhost:5126
Swagger → http://localhost:5126/swagger
---
4️⃣ Run Frontend
```
cd LicenseManagementClient
npm install
npm run dev
```
App runs on:
http://localhost:3000
---
🔐 Authentication Flow
```
User → Login → API validates → JWT issued → Stored in cookie
→ Sent in requests → API authorizes endpoints
```
---
📡 API Endpoints
Auth
POST /api/auth/login
Doctors (Protected)
GET /api/doctors
GET /api/doctors/{id}
GET /api/doctors/expired
POST /api/doctors
PUT /api/doctors/{id}
PATCH /api/doctors/{id}/status
DELETE /api/doctors/{id}
---
🧠 Business Rules
License Expiry
Scenario	Result
Past date	Expired
Future date	Active
Suspended by admin	Always Suspended
Background Job
Runs daily at midnight (UTC)
Marks expired doctors automatically
Uses JobLog to prevent duplicate execution
---
🗑️ Soft Delete
No hard deletes.
IsDeleted = 1
---
🏗️ Design Decisions
✅ Applied
Clean layered architecture
Dependency injection (`IDbConnectionFactory`)
Lightweight ORM (Dapper)
Background processing
---
⚖️ Trade-offs
Decision	Reason
Single project	Simpler setup
DTO = DB shape	Avoid unnecessary mapping
Status as string	Simpler with Dapper
DataAnnotations	Faster validation setup
---
⚡ Performance Considerations
Uses connection pooling (ADO.NET)
Short-lived DB connections
Stored procedures for heavy queries
Soft deletes reduce data loss risk
---
▶️ Run Everything
```
# Backend
cd LicenseManagementAPI && dotnet run

# Frontend
cd LicenseManagementClient && npm run dev
```
---
🚀 Future Improvements
Add Redis caching
Introduce FluentValidation
Move to multi-project architecture
Add role-based access control
Dockerize services
---
👨‍💻 Author Notes
This project focuses on:
Real-world SaaS architecture
Clean backend design
Practical trade-offs (not over-engineering)
