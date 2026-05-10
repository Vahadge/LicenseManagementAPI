# License Management System

A full-stack platform for managing doctor licenses at a Medical SaaS company. Admins can register doctors, track when their licenses expire, and flip statuses manually. The interesting part: expired licenses get picked up and marked automatically every night — no cron job, no third-party scheduler, just a background service baked into the API.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Next.js](https://img.shields.io/badge/Next.js-14-000000?logo=next.js)
![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB%20%2F%20Express-CC2927?logo=microsoftsqlserver)
![Dapper](https://img.shields.io/badge/ORM-Dapper-blue)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3-38B2AC?logo=tailwindcss)

---

## What's in here

- **Doctor management** — full CRUD with search, filtering, and pagination
- **License expiry tracking** — live hints on the date picker, color-coded status badges
- **Nightly expiry job** — runs at UTC midnight, marks stale Active records as Expired, logs every run to the database
- **JWT auth** — admin login with PBKDF2-hashed passwords; no extra packages, everything built-in
- **Public read / protected write** — anyone can browse the doctor list; mutations require an admin token
- **Soft deletes** — nothing is ever physically removed
- **20 seed records** — ready to go with a mix of Active, Expired, and Suspended doctors

---

## Tech choices, briefly

**Backend:** .NET 8 Web API, Clean Architecture (folder-based — more on this below), Dapper for data access with SQL Server stored procedures, JWT Bearer for auth, FluentValidation for request validation, `BackgroundService` for the nightly job.

**Frontend:** Next.js 14 App Router, TypeScript, Tailwind CSS. Auth token lives in a cookie so the Next.js middleware can read it server-side and protect routes.

**No extra packages for auth or scheduling** — PBKDF2 hashing uses `System.Security.Cryptography` (built-in), JWT uses `Microsoft.AspNetCore.Authentication.JwtBearer` (part of the ASP.NET Core framework), and the nightly job uses `BackgroundService` (also built-in). Hangfire and Quartz were explicitly avoided.

---

## Project layout

```
LicenseManagement/
│
├── LicenseManagementAPI/
│   ├── Domain/                  # Entities (Doctor, User, JobLog) and enums
│   ├── Application/             # Interfaces, DTOs, validators, mapping, and service logic
│   │   ├── Interfaces/          #   IDbConnectionFactory, IDoctorRepository, IPasswordHasher, ...
│   │   ├── DTOs/                #   Request/response shapes
│   │   ├── Mapping/             #   DoctorMapper — converts Doctor entity → DoctorDto
│   │   ├── Validators/          #   FluentValidation validators, one per request type
│   │   └── Services/            #   DoctorService, AuthService
│   ├── Infrastructure/          # Everything that touches the outside world
│   │   ├── Data/                #   SqlConnectionFactory
│   │   ├── Repositories/        #   Dapper-based repository implementations
│   │   ├── Security/            #   PasswordHasher (implements IPasswordHasher)
│   │   ├── Jobs/                #   DoctorExpiryHostedService
│   │   └── Seeders/             #   DatabaseSeeder (creates the default admin)
│   ├── Controllers/             # Thin HTTP layer — no business logic here
│   └── Common/                  # Shared utilities: middleware, exceptions
│
├── LicenseManagementClient/
│   ├── app/doctors/             # Doctor list (public read, admin write)
│   ├── app/login/               # Admin login screen
│   ├── components/              # DoctorTable, DoctorForm, Header, etc.
│   ├── lib/api.ts               # Typed API client — auto-attaches the JWT
│   ├── lib/auth.ts              # Cookie helpers: setAuth, clearAuth, getToken
│   └── middleware.ts            # Redirects unauthenticated users away from /login
│
└── dbSchema.Zip/
        Contains table and stored procedure scripts kept separate for review.
        seeddata.sql is also included if you want pre-populated data. (Optional)

```

---

## How the architecture fits together

Dependencies flow inward — Infrastructure depends on Application, Application depends on Domain, nothing points outward. Controllers only talk to services, services only talk to repository interfaces, repositories get their DB connection from a factory interface.

**DB connection** — Rather than injecting `IConfiguration` into every repository (which would drag a framework concern into the Application layer), there's a small `IDbConnectionFactory` interface defined in `Application` and implemented in `Infrastructure`:

```csharp
// Application/Interfaces/IDbConnectionFactory.cs
public interface IDbConnectionFactory
{
    IDbConnection Create();
}

// Infrastructure/Data/SqlConnectionFactory.cs — the only place that knows about
// connection strings and SqlConnection
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    public SqlConnectionFactory(IConfiguration cfg) =>
        _connectionString = cfg.GetConnectionString("DefaultConnection")!;
    public IDbConnection Create() => new SqlConnection(_connectionString);
}
```

Every repository constructor just takes `IDbConnectionFactory` — clean, testable, and `IConfiguration` never crosses the layer boundary.

**Entity → DTO mapping** — Dapper maps query results into `Domain/Entities/Doctor.cs`. `Application/Mapping/DoctorMapper.cs` then converts those entities into `DoctorDto` before they reach the service layer. The persistence representation and the API response shape are kept explicitly separate.

**Password hashing** — `IPasswordHasher` lives in `Application/Interfaces/`. `Infrastructure/Security/PasswordHasher.cs` implements it using PBKDF2 via `System.Security.Cryptography`. `AuthService` and `DatabaseSeeder` both receive it through constructor injection — no static helpers, no framework dependencies leaking into the Application layer.

**Validation** — DataAnnotations are gone from all DTO classes. Each request type has its own validator in `Application/Validators/` (e.g. `CreateDoctorRequestValidator`, `UpdateStatusRequestValidator`). FluentValidation's auto-validation hooks into the MVC pipeline, so invalid requests are rejected with a 400 before they ever reach a service method.

**Status constants** — `DoctorStatus` is a static class in `Domain/Enums/` with `const string` fields (`Active`, `Expired`, `Suspended`). Every place in the codebase — including raw SQL strings in the repository — references these constants. No magic strings.

---

## The nightly background job

### Why not Hangfire or Quartz?

The requirement is simple: mark expired doctors once a day. Hangfire and Quartz are great tools but they bring in extra NuGet packages, a separate persistence store, and a management dashboard. For a single recurring task, `BackgroundService` (built into ASP.NET Core) is the right tool.

### What it actually does

On startup it checks whether the job already ran today. If yes, it skips. If no, it runs immediately (catch-up for missed runs). Then it sleeps until the next UTC midnight and repeats.

```
App starts
  │
  ├─► Did the job run today? (checks JobLog table)
  │       ├─ Yes → skip, sleep until midnight
  │       └─ No  → run now
  │
  └─► Every UTC midnight: same check, same logic
```

The "did it run today?" check is what makes it restart-safe. If the server reboots at 00:05 after the job ran at 00:00, it reads the `JobLog`, sees a completed run for today, and skips. No double-processing.

### JobLog table

Every run — success or failure — gets a row:

| Column | What it stores |
|---|---|
| `StartedAt` | When the job began |
| `CompletedAt` | When it finished (NULL while still running) |
| `RecordsAffected` | How many doctors were marked Expired |
| `Status` | Running → Completed or Failed |
| `ErrorMessage` | Populated only on failure |

---

## API endpoints

### Auth

| Method | Endpoint | Who can call it |
|---|---|---|
| `POST` | `/api/auth/login` | Anyone |

Send `{ "username": "admin", "password": "Admin@123" }`, get back a JWT.

### Doctors

| Method | Endpoint | Who can call it |
|---|---|---|
| `GET` | `/api/doctors` | **Anyone** |
| `GET` | `/api/doctors/expired` | **Anyone** |
| `GET` | `/api/doctors/{id}` | Admin |
| `POST` | `/api/doctors` | Admin |
| `PUT` | `/api/doctors/{id}` | Admin |
| `PATCH` | `/api/doctors/{id}/status` | Admin |
| `DELETE` | `/api/doctors/{id}` | Admin |

The list and expired endpoints are intentionally public — guest users can browse without logging in. The frontend shows a read-only banner and a 401 popup if they try to take any action.

`GET /api/doctors` supports `search`, `status`, `pageNumber`, and `pageSize` query params. `pageSize` is clamped to 100 server-side.

All responses use a consistent envelope:
```json
{ "isSuccess": true, "message": "...", "data": { ... }, "errors": null }
```

---

## Status logic

There are three statuses: `Active`, `Expired`, `Suspended`.

- **Active** — valid license, doctor is practicing
- **Expired** — license date passed; set automatically by the nightly job or immediately on create/update if the date is in the past
- **Suspended** — manual admin action; the system never auto-sets this

When an admin edits a doctor, they can choose Active or Suspended. Expired is shown in the dropdown but disabled — it's system-managed only. The server enforces this too: the API rejects `Status = "Expired"` in a PUT request body.

The server-side resolution on `PUT`:

```
Admin picks Suspended               → always Suspended
Admin picks Active + past date      → overridden to Expired (can't be active with an expired licence)
Admin picks Active + future date    → Active
```

---

## Getting it running

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server — LocalDB (comes with Visual Studio), Express, or full SQL Server all work
- Node.js 18+

### Step 1 — Database

Extract dbSchema zip file and open all the scripts in SQL Management Studio. It creates the tables, indexes, and stored procedures. No hardcoded paths, no machine-specific users — works on any machine.

If you want sample data, also run `seeddata.sql`.

### Step 2 — API

Open `LicenseManagementAPI/appsettings.json` and check the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LicenseManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Change the `Server` value if you're not on LocalDB:

- SQL Server Express → `Server=.\\SQLEXPRESS`
- Default/Docker instance → `Server=localhost`
- Named instance → `Server=HOSTNAME\\INSTANCENAME`

Then:

```bash
cd LicenseManagementAPI
dotnet run
```

The API starts on `http://localhost:5126`. Swagger is at `http://localhost:5126/swagger`. On first startup, `DatabaseSeeder` runs and creates the default admin account — no manual SQL needed.

### Step 3 — Frontend

```bash
cd LicenseManagementClient
npm install
npm run dev
```

Frontend at `http://localhost:3000`. If your API runs on a different port, create `LicenseManagementClient/.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5126/api
```

### Default login

| Username | Password |
|---|---|
| `admin` | `Admin@123` |

---

## Known trade-offs

One deliberate shortcut remains because this is a demonstration project. It's documented here so it's clear it was considered, not missed.

---

**Folders instead of separate projects**

Real Clean Architecture uses four separate C# projects — `Domain`, `Application`, `Infrastructure`, `Api` — each compiled as its own assembly. That way the compiler enforces the dependency rules: if `Application` accidentally references `Infrastructure`, the build fails.

Here, all four layers are folders inside a single project. The boundaries exist by convention, not by the compiler. For a demo this is fine — the intent is identical and the structure is still clearly visible. In a production codebase you'd want the hard enforcement.
