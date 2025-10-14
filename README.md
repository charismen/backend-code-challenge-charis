# ShipManagement Backend Challenge

Backend API for managing ships, crew, users, and financial records. The solution is built with .NET and organised as a single Web API project plus acceptance-style test projects.

## Contents

- `ShipManagement/src/ShipManagement.API` – main .NET API
- `ShipManagement/tests/ShipManagement.Tests` – BDD-style SpecFlow tests
- `Database/*.sql` – schema, stored procedures, and sample data scripts
- `Makefile` – build, run, database automation, and cert helpers
- `Properties/launchSettings.json` – VS Code debugging profiles

## Prerequisites

- .NET SDK
- SQL Server instance accessible at `localhost`
  - Update `ShipManagement/src/ShipManagement.API/appsettings.json` if using a different host or credentials.
  - SA password currently defaults to `StrongPassword@123`.
- `sqlcmd` CLI (cross-platform) for running database scripts through `make`.
- Optional: VS Code with C# extension for integrated debugging.

## Quick Start

```bash
# Restore packages and build
make build

# Trust the HTTPS development certificate (first-time setup)
make cert

# Provision database schema, stored procedures, and sample data
make db-setup

# Run the API (uses launch profile and Development environment)
make run
```

Once running, the API listens on:

- `https://localhost:5001`
- `http://localhost:5000` (redirects to Swagger UI)

Swagger is available at `/swagger`. The root path `/` redirects to the documentation without exposing an extra endpoint definition.

## Database Scripts

| Script | Purpose |
| --- | --- |
| `Database/DDL_Scripts.sql` | Creates the `ShipManagement` database along with core tables (`Ship`, `CrewMember`, `User`, etc.). |
| `Database/StoredProcedures.sql` | Defines stored procedures used by the repositories/services. |
| `Database/SampleData.sql` | Seeds ships, crew members, and users for local testing. |

The `Makefile` wraps common `sqlcmd` invocations:

- `make db-schema` – create/update schema.
- `make db-sp` – deploy stored procedures.
- `make db-sample` – seed data.
- `make db-drop` – drop the database (forces single-user mode first).
- `make db-setup` – shortcut running schema, stored procedures, and seed in order.

Environment variables you can override when calling `make`:

```bash
SQL_SERVER=localhost SQL_USERNAME=SA SQL_PASSWORD="StrongPassword@123" make db-setup
```

Setting `SQL_TRUST_CERT=false` will omit the `-C` flag when connecting to SQL Server with TLS.

## Running & Debugging

- `make run` – executes `dotnet run` against `ShipManagement.API` with the `ShipManagement.API` launch profile (Development environment & swagger redirect middleware).
- `make watch` – launches `dotnet watch` for hot reload.
- VS Code: use the **Launch ShipManagement.API** configuration defined in `.vscode/launch.json`. It compiles the project, launches via the built DLL, and opens the Swagger UI URL once the app announces it.

Debug tips are output through `make debug`, and the launch profile is declared in `ShipManagement/src/ShipManagement.API/Properties/launchSettings.json`.

## Testing

```bash
make test
# or
dotnet test ShipManagement/tests/ShipManagement.Tests/ShipManagement.Tests.csproj
```

Tests are SpecFlow-based feature files grouped by concern (`Crew`, `Financial`, `Ships`). They exercise service endpoints via scenario definitions and generated step bindings.

## Design Notes

- **Dependency Injection** – Services implement interfaces (`IShipService`, `ICrewService`, etc.) and are registered in `Program.cs` for clear separation between controllers and data access.
- **Database Access** – Uses a custom `SqlConnectionFactory` alongside Dapper for lightweight data mapping while leveraging stored procedures for complex queries.
- **Environment-specific configuration** – `appsettings.Development.json` configures Kestrel endpoints to avoid HTTPS redirection warnings; `launchSettings.json` aligns with the same URLs to keep tooling consistent.
- **Developer Experience** – `Makefile` provides a single entry point for restoring, building, testing, running, and provisioning the database; VS Code settings simplify local debugging without modifying source controllers.
- **Swagger Exposure** – Root URL redirect implemented as middleware to keep Swagger discoverable, while keeping the generated doc free of extra endpoints.

## Additional Files & Tooling

- `Makefile` – automates build/test/run/db tasks documented above.
- `.gitignore` – excludes build outputs, tooling caches, and local env files from version control.
- `ShipManagement/src/ShipManagement.API/Properties/launchSettings.json` – dotnet CLI profiles (development & integration).
- No Dockerfile or CI pipeline are provided; integration points are ready for future extension if desired.

## Troubleshooting

- **Database Connectivity** – The connection string in `appsettings.json` disables server-side encryption (`Encrypt=False`) while trusting the local certificate (`TrustServerCertificate=True`) to bypass SSPI issues on local development boxes. Adjust to match your SQL Server security policies.
- **Missing `sqlcmd`** – Install the new cross-platform `sqlcmd` (`mssql-tools`) or update `Makefile` targets to use an alternate SQL runner if necessary.
- **Ports already in use** – Update `appsettings.Development.json` and `launchSettings.json` to new ports, keeping the values in sync.

Happy shipping!
