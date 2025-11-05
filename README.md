# Industrial Automation Alert Service

ASP.NET Core 8 Web API + EF Core + SignalR + JWT. Simulates telemetry, raises alerts when thresholds exceeded, persists to SQL, broadcasts in realtime, and exposes a secure REST API. Includes unit tests and a minimal Next.js dashboard (folder scaffold ready).

## Solution Structure

- `Industrial.AlertService.sln`
- `src/Domain`: Entities, DTOs, interfaces, threshold evaluator
- `src/Infrastructure`: EF Core DbContext, repositories, services (JWT, Alert, Config), background `TelemetrySimulatorService`, migrations, seeding
- `src/Api`: ASP.NET Core Web API (controllers), SignalR hub endpoint, DI, Swagger
- `src/Tests`: xUnit tests for threshold evaluator and alert creation
- `ui/nextjs-dashboard` (TBD): Minimal Next.js dashboard (login, config, live alerts)

## Requirements Covered

- JWT auth (`POST /auth/login` demo user: demo / Password123!)
- Config endpoints: `GET /config`, `PUT /config` (with validation)
- Alerts endpoints: `GET /alerts`, `POST /alerts/{id}/ack`
- Background service generates telemetry every 3–5 seconds and raises alerts
- SignalR hub at `/hubs/alerts` broadcasting `NewAlert` events
- EF Core migrations creating `Config` and `Alert` tables
- Unit tests for domain logic

## Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server (default), or PostgreSQL (optional). Set connection string accordingly.

### Configure

Edit `src/Api/appsettings.Development.json`:

```json
{
  "Jwt": {
    "Secret": "super_dev_secret_change_me_please_1234567890"
  },
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=AlertServiceDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True"
    // Or PostgreSQL:
    // "Default": "Host=localhost;Port=5432;Database=alertservicedb;Username=postgres;Password=postgres"
  }
}
```

Switching provider is automatic based on the connection string: if it looks like PostgreSQL (starts with `Host=`), Npgsql is used, otherwise SQL Server.

### Run API

```bash
# from repository root
dotnet restore
dotnet build
dotnet run --project src/Api/Api.csproj
```

- API runs at `http://localhost:5000`
- Swagger at `http://localhost:5000/swagger`
- On first run, DB is migrated and a default `Config` row is seeded.

### Apply Migrations explicitly (optional)

Migrations are included in `src/Infrastructure/Migrations`. To re-create:

```bash
# Example: SQL Server
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Api/Api.csproj
```

For PostgreSQL, ensure your connection string points to PostgreSQL and Npgsql server is available.

### Run Tests

```bash
dotnet test
```

## API Examples (cURL)

- Login:

```bash
echo '{"username":"demo","password":"Password123!"}' | curl -s -X POST http://localhost:5000/auth/login -H "Content-Type: application/json" -d @-
```

- Get config:

```bash
curl http://localhost:5000/config -H "Authorization: Bearer <token>"
```

- Update config:

```bash
curl -X PUT http://localhost:5000/config -H "Authorization: Bearer <token>" -H "Content-Type: application/json" -d '{"tempMax":85.0,"humidityMax":70.0}'
```

- List alerts:

```bash
curl "http://localhost:5000/alerts?status=open&from=2025-01-01T00:00:00Z&to=2025-12-01T00:00:00Z" -H "Authorization: Bearer <token>"
```

- Acknowledge alert:

```bash
curl -X POST http://localhost:5000/alerts/{id}/ack -H "Authorization: Bearer <token>"
```

## Notes

- SignalR hub: `/hubs/alerts`. The server sends `NewAlert` events with the alert payload.
- The background simulator polls config each iteration; changes to config reflect automatically without restart.
- CORS is permissive for demo; restrict origins for production.

## Docker (optional)

A simple docker-compose can be added to run API + DB. If needed, request it and it will be generated with SQL Server or PostgreSQL services.


