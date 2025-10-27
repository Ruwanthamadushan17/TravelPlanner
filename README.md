# TravelPlanner

.NET 8 travel planning API. Given trip dates, budget, and destinations, it generates and persists itineraries with costs and shareable plans.

## Features
- ASP.NET Core Web API (.NET 8) with API Versioning (default v1)
- Clean architecture: `Api` → `Application` → `Infrastructure` → `Domain`
- EF Core (SQL Server) with code‑first migrations
- FluentValidation with automatic model validation
- Health checks (live/ready) and fixed-window rate limiting
- AutoMapper mapping profiles
- Tests (unit + integration) using xUnit, Testcontainers, and Respawn

## Architecture
- `TravelPlanner.Api`: HTTP endpoints, middleware, versioning, Swagger
- `TravelPlanner.Application`: services, DTOs, validation, AutoMapper profiles
- `TravelPlanner.Infrastructure`: EF Core `DbContext`, repositories, UoW, migrations
- `TravelPlanner.Domain`: entities and core model
- `TravelPlanner.Migrator`: migration runner for CI/CD and local use
- `TravelPlanner.Tests`: unit and integration tests (API + DB)

## Getting Started

### Prerequisites
- .NET SDK 8.0+
- Docker (required for integration tests; optional for local DB if you use your own SQL Server)

### Clone and Build
- Restore and build: `dotnet build TravelPlanner/TravelPlanner.sln`
- Run tests: `dotnet test TravelPlanner/TravelPlanner.sln`

### Configuration
The API requires a SQL Server connection string via `ConnectionStrings:Sql`.

In Development, config is augmented from a secrets file if present. By default, `TravelPlanner.Api/appsettings.Development.json` 
points to `secrets.development.json` in the API project root.

Example `TravelPlanner/TravelPlanner.Api/secrets.development.json`:
```
{
  "ConnectionStrings": {
    "Sql": "Server=localhost,11433;Database=TravelPlanner;User Id=sa;Password=Password123!;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

Alternatively set an environment variable: `ConnectionStrings__Sql`.

### Run the API Locally
- With a local or containerized SQL Server configured:
  - `dotnet run --project TravelPlanner/TravelPlanner.Api`
- Swagger UI (Development only): `https://localhost:<port>/swagger`

### Docker Compose (API + SQL Edge)
`docker-compose` in the repo spins up the API and Azure SQL Edge.

1) Set `MSSQL_SA_PASSWORD` in your shell or `.env` (min complexity enforced by SQL Server).
2) `docker compose up -d --build`
3) API: `http://localhost:8080`; SQL Server: `localhost,11433`

Compose mounts `TravelPlanner.Api/secrets.development.json` into the container, so ensure it contains the right connection string (see example above).

## Database and Migrations
Migrations are kept in `TravelPlanner.Infrastructure/Migrations`. You can apply them via:

- Migrator (recommended for CI/CD or local):
  - Ensure `ConnectionStrings__Sql` or secrets file is set
  - `dotnet run --project TravelPlanner/TravelPlanner.Migrator`
  - Optional env vars:
    - `MIGRATION_COMMAND_TIMEOUT` (seconds, default 180)
    - `SQL_MAX_RETRIES` (default 3)

- EF Core CLI (developer workflow):
  - Add migration: `dotnet ef migrations add <Name> --project TravelPlanner/TravelPlanner.Infrastructure --startup-project TravelPlanner/TravelPlanner.Api`
  - Update DB: `dotnet ef database update --project TravelPlanner/TravelPlanner.Infrastructure --startup-project TravelPlanner/TravelPlanner.Api`

Production configuration may provide `KeyVault:VaultUri`; when present, the API and migrator load secrets from Azure Key Vault using `DefaultAzureCredential`.

## API
Base path: `/api/v1`

- Trips
  - GET `/api/v1/Trips/{id}` → `200 OK` with `TripDto` or `404`
  - GET `/api/v1/Trips?ownerEmail=<email>` → `200 OK` list of `TripDto`
  - POST `/api/v1/Trips` → `201 Created` with `TripDto`

Example request:
```
POST /api/v1/Trips
Content-Type: application/json

{
  "ownerEmail": "user@example.com",
  "startDate": "2025-06-01",
  "endDate": "2025-06-10",
  "budget": 1500.00,
  "destinations": [
    { "city": "Paris", "country": "France" },
    { "city": "Rome", "country": "Italy" }
  ]
}
```

Validation highlights:
- `OwnerEmail` must be a valid email
- `EndDate` must be after `StartDate`
- `Budget` must be non‑negative
- At least one destination with non‑empty `city` and `country`

Swagger is enabled in Development; use it to explore the full schema.

## Health and Observability
- Liveness: `/health/live` (in‑memory check)
- Readiness: `/health/ready` (includes SQL database check)
- Rate limiting: Fixed window, 100 requests/minute; exceeding returns `429 Too Many Requests`

## Testing
- Unit and integration tests: `dotnet test TravelPlanner/TravelPlanner.sln`
- Integration tests launch SQL Server via Testcontainers; Docker must be available
- Respawn resets DB state between tests

## Project Structure
```
TravelPlanner.sln
└─ TravelPlanner
   ├─ TravelPlanner.Api
   ├─ TravelPlanner.Application
   ├─ TravelPlanner.Domain
   ├─ TravelPlanner.Infrastructure
   ├─ TravelPlanner.Migrator
   └─ TravelPlanner.Tests
```

## Notes and Conventions
- Connection string key: `ConnectionStrings:Sql`
- Migrations live in `Infrastructure`; the migrator targets that assembly
- API versioning uses URL segments: `api/v{version}/...` (default v1)

## License
MIT. See `LICENSE`.

