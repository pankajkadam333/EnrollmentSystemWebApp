# Enrollment Dashboard — ASP.NET Core MVC

A secure Enrollment Dashboard built with ASP.NET Core MVC demonstrating clean architecture, OWASP security best practices, and SQL proficiency.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Configuration](#configuration)
4. [Project Architecture](#project-architecture)
5. [Features & How to Test Them](#features--how-to-test-them)
6. [Security Implementation](#security-implementation)
7. [Logging](#logging)
8. [Running Tests](#running-tests)
9. [Architecture Decisions](#architecture-decisions)
10. [What I'd Improve With More Time](#what-id-improve-with-more-time)

---

## Prerequisites

- .NET 8 SDK or later (tested on .NET 10)
- A terminal / command prompt
- SQL Server (optional — the app runs with in-memory mock data by default)

Verify your .NET installation:
```bash
dotnet --version
```

---

## Quick Start

### 1. Clone the repository
```bash
git clone https://github.com/YOUR_USERNAME/enrollment-dashboard.git
cd enrollment-dashboard
```

### 2. Build the project
```bash
dotnet build EnrollmentDashboard/EnrollmentDashboard.csproj
```

### 3. Run the application
```bash
dotnet run --project EnrollmentDashboard
```

### 4. Open in browser
Navigate to the URL shown in the console output (typically `http://localhost:5xxx`), then go to:
- Dashboard: `/Enrollments`
- Detail view: `/Enrollments/Details/1`

### 5. Run tests
```bash
dotnet test EnrollmentDashboard.Tests/EnrollmentDashboard.Tests.csproj
```

That's it — the app runs with mock data out of the box, no database setup needed.

---

## Configuration

All configuration lives in `EnrollmentDashboard/appsettings.json`.

### UseMockData (Data Source Toggle)

```json
"UseMockData": true
```

| Value | Behavior |
|-------|----------|
| `true` (default) | Uses `InMemoryEnrollmentRepository` — all 25 enrollments, 15 participants, and 8 programs from `sample-data.sql` are seeded in C# collections. No database required. |
| `false` | Uses `EnrollmentRepository` — connects to SQL Server using ADO.NET with parameterized queries and stored procedures. Requires a running SQL Server instance with the schema and data loaded. |

**How it works in code** (`Program.cs`):
```csharp
if (builder.Configuration.GetValue<bool>("UseMockData"))
    builder.Services.AddScoped<IEnrollmentRepository, InMemoryEnrollmentRepository>();
else
    builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
```
Both repositories implement the same `IEnrollmentRepository` interface, so the rest of the application is completely unaware of which data source is active.

### ConnectionStrings (SQL Server)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EnrollmentSystem;Integrated Security=true;TrustServerCertificate=true;"
}
```

Only used when `UseMockData` is `false`. To use SQL Server:

1. Create the database and run the provided scripts:
   ```bash
   sqlcmd -S (localdb)\MSSQLLocalDB -i database-schema.sql
   sqlcmd -S (localdb)\MSSQLLocalDB -i sample-data.sql
   ```
2. Update the connection string if your server name differs.
3. Set `"UseMockData": false` in `appsettings.json`.

### Serilog (Logging Configuration)

```json
"Serilog": {
  "WriteTo": [
    { "Name": "Console", ... },
    { "Name": "File", ... }
  ]
}
```

See the [Logging](#logging) section for full details on configuring log sinks.

---

## Project Architecture

```
EnrollmentDashboard/
├── Models/                  Domain entities and ViewModels
│   ├── Participant.cs       Participant entity
│   ├── Program.cs           Program entity
│   ├── Enrollment.cs        Enrollment entity
│   └── ViewModels/
│       ├── DashboardViewModel.cs         Dashboard page model (filters, pagination, summary)
│       ├── EnrollmentListViewModel.cs    Single row in the enrollments table
│       ├── EnrollmentDetailViewModel.cs  Full detail view model
│       └── EnrollmentSummaryViewModel.cs Summary statistics (total, active, completed, withdrawn)
│
├── DataAccess/              Repository layer (data access)
│   ├── IEnrollmentRepository.cs          Interface — defines data operations
│   ├── EnrollmentRepository.cs           SQL Server implementation (ADO.NET, stored procedures)
│   └── InMemoryEnrollmentRepository.cs   In-memory implementation (mock data from sample-data.sql)
│
├── Business/                Business logic layer
│   ├── IEnrollmentService.cs             Interface — defines business operations
│   └── EnrollmentService.cs              Validation, filtering logic, orchestration
│
├── Controllers/             Thin presentation layer
│   └── EnrollmentsController.cs          Dashboard (Index) and Detail views
│
├── Views/
│   ├── Enrollments/
│   │   ├── Index.cshtml     Dashboard — stats cards, filters, table, pagination
│   │   └── Details.cshtml   Enrollment detail — participant, program, notes
│   └── Shared/
│       ├── _Layout.cshtml   Main layout with navigation
│       └── NotFound.cshtml  Custom 404 page
│
├── Program.cs               App entry point — DI registration, Serilog setup
├── appsettings.json         Configuration (data source toggle, connection string, logging)
├── SECURITY-ANALYSIS.md     Code review — 7 vulnerabilities documented
│
EnrollmentDashboard.Tests/
├── DataAccess/              20 tests — repository filtering, pagination, XSS data
├── Business/                13 tests — validation, whitelist, malicious input
├── Controllers/              7 tests — HTTP responses, NotFound handling
└── Integration/             10 tests — full pipeline end-to-end
```

**Data flow:** Controller → Service (validation) → Repository (data access) → back up through ViewModels to Razor Views.

---

## Features & How to Test Them

### Dashboard (`/Enrollments`)

**Summary Statistics Cards**
- Four cards at the top: Total, Active, Completed, Withdrawn
- Cards show counts based on current filters
- Active, Completed, and Withdrawn cards are clickable — click to filter by that status, click again to clear

**How to test:**
1. Go to `/Enrollments` — you should see Total: 25, Active: 10, Completed: 12, Withdrawn: 3
2. Click the "Active" card — table filters to 10 Active enrollments, card shows a ✓ and bold border
3. Click "Active" again — filter clears, all 25 enrollments shown

**Date Range Filter**
- Pick a Start Date and End Date, click Filter
- Only enrollments within that range are shown
- Summary stats update to reflect the filtered range

**How to test:**
1. Set Start Date: `2026-01-01`, End Date: `2026-01-31`, click Filter
2. Should show 4 enrollments from January 2026
3. Try setting Start Date after End Date — a red error message appears and form won't submit

**Status Dropdown Filter**
- Select Active, Completed, or Withdrawn from the dropdown
- Stays in sync with the clickable cards

**Pagination**
- 10 enrollments per page
- Previous/Next buttons and page numbers at the bottom
- Filters are preserved across pages

**How to test:**
1. With no filters, you should see 3 pages (25 records ÷ 10 per page)
2. Click page 3 — should show 5 remaining enrollments
3. Apply a filter, then paginate — filter stays applied

### Detail View (`/Enrollments/Details/{id}`)

Shows full information for a single enrollment:
- Enrollment info: ID, date, completion date, status
- Participant info: name, date of birth, active status
- Program info: name, description
- Notes: rendered as plain text (XSS-safe)

**How to test:**
1. Click "Details" on any enrollment row
2. Go to `/Enrollments/Details/5` — this is the XSS test enrollment. Participant name should show `Robert <script>alert("XSS")</script>` as plain text, not execute JavaScript
3. Go to `/Enrollments/Details/9999` — should show a 404 page

---

## Security Implementation

### SQL Injection Prevention
- `EnrollmentRepository` uses `SqlParameter` for every value passed to SQL
- Stored procedures `sp_GetEnrollments` and `sp_GetEnrollmentSummary` are called via `CommandType.StoredProcedure`
- Detail query uses parameterized inline SQL
- Zero string concatenation anywhere in data access

**Test:** Try `/Enrollments?status='; DROP TABLE Enrollments--` — the status is rejected by the whitelist, all data remains intact.

### XSS Prevention
- All Razor output uses `@` syntax which auto-encodes HTML
- No `Html.Raw()` used anywhere
- Participant #6 has `<script>alert("XSS")</script>` in their last name — renders as visible text
- Enrollment #5 has `<img src=x onerror="alert('XSS')">` in notes — renders as visible text

**Test:** Go to `/Enrollments/Details/5` — verify you see the script tags as text, no alert popups.

### IDOR Protection
- Controller validates ID is present and > 0 before calling service
- Service adds a second guard for IDs ≤ 0
- Non-existent IDs return HTTP 404
- No stack traces or internal details exposed

**Test:** Try `/Enrollments/Details/9999`, `/Enrollments/Details/0`, `/Enrollments/Details/-1` — all return 404.

### Input Validation
- Status: validated against allowlist `["Active", "Completed", "Withdrawn"]` — invalid values silently ignored
- Dates: typed as `DateTime?` (model binding rejects non-dates), reversed ranges auto-swapped, client-side JS prevents form submission
- Page number: clamped to minimum of 1

**Test:** Try `/Enrollments?status=HACKED` — shows all enrollments (invalid status ignored).

### Full Security Analysis
See `SECURITY-ANALYSIS.md` for the code review exercise — 7 vulnerabilities documented with OWASP categories, risk levels, exploitation methods, and fixes.

---

## Logging (This was not asked but implemented by my own.)

The application uses Serilog for structured logging, configured entirely from `appsettings.json`.

### What Gets Logged

| Event | Level | Example |
|-------|-------|---------|
| App startup | INFO | `Starting Enrollment Dashboard application` |
| Data source mode | INFO | `UseMockData = True` |
| Dashboard request | INFO | `Dashboard requested — StartDate=null, EndDate=null, Status=null, Page=1` |
| Dashboard result | INFO | `Dashboard loaded — 10 enrollments on page 1, 25 total records` |
| Detail loaded | INFO | `Enrollment detail loaded for ID: 1` |
| HTTP request | INFO | `HTTP GET /Enrollments responded 200 in 107.07 ms` |
| Reversed dates | WARN | `Date range reversed — swapping StartDate=... and EndDate=...` |
| Invalid status | WARN | `Invalid status value rejected: HACKED` |
| Not found | WARN | `Enrollment not found for ID: 9999` |
| Invalid ID | WARN | `Detail requested with invalid ID: -1` |
| Startup failure | FATAL | `Application terminated unexpectedly` |

### Configuring Log Sinks

Edit the `WriteTo` array in `appsettings.json`. No code changes or rebuild needed.

**Both console and file (default):**
```json
"WriteTo": [
  {
    "Name": "Console",
    "Args": {
      "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    }
  },
  {
    "Name": "File",
    "Args": {
      "path": "Logs/enrollment-.log",
      "rollingInterval": "Day",
      "retainedFileCountLimit": 7,
      "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
    }
  }
]
```

**File only** — remove the Console entry:
```json
"WriteTo": [
  { "Name": "File", "Args": { ... } }
]
```

**Console only** — remove the File entry:
```json
"WriteTo": [
  { "Name": "Console", "Args": { ... } }
]
```

### Log File Location
- Files are written to `EnrollmentDashboard/Logs/enrollment-{date}.log`
- Daily rolling — one file per day
- Retains last 7 days of logs automatically

### Adjusting Log Levels

```json
"MinimumLevel": {
  "Default": "Information",
  "Override": {
    "Microsoft.AspNetCore": "Warning",
    "System": "Warning"
  }
}
```

Change `"Default"` to `"Debug"` for more verbose output, or `"Warning"` to reduce noise.

---

## Running Tests

```bash
dotnet test EnrollmentDashboard.Tests/EnrollmentDashboard.Tests.csproj --verbosity normal
```

**56 tests** across 4 test classes:

| Test Class | Count | What It Covers |
|-----------|-------|----------------|
| `InMemoryEnrollmentRepositoryTests` | 20 | Filtering, pagination, ordering, date ranges, XSS data integrity, summary stats, detail lookups |
| `EnrollmentServiceTests` | 13 | Date swap, status whitelist, malicious input rejection, page clamping, null handling |
| `EnrollmentsControllerTests` | 7 | View result types, NotFound for null/zero/negative/non-existent IDs |
| `DashboardIntegrationTests` | 10 | Full pipeline (real repo + service), consistency, pagination math, XSS preservation |

---

## Architecture Decisions

**Why ADO.NET instead of Entity Framework?**
The exercise specifically asks for stored procedures and parameterized queries to demonstrate SQL proficiency. ADO.NET gives direct control over SQL execution while still being fully parameterized.

**Why the Repository + Service pattern?**
- Repository handles data access only — no business logic
- Service handles validation, filtering rules, and orchestration
- Controller is thin — just maps HTTP to service calls and returns views
(NOte: here we can use minimal API in case of API only.)
- This makes each layer independently testable (56 tests prove it)

**Why an in-memory repository?**
Allows the app to run without SQL Server for development and review. The `UseMockData` toggle makes switching seamless — same interface, same behavior, different data source.
Because I did not had SQL Server on my machine so initially I had started with inMemory data 

**Why Serilog?**
- Config-driven — add/remove sinks without code changes
- Structured logging — properties like `{EnrollmentId}` are queryable
- Industry standard for ASP.NET Core applications

---

