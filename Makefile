DOTNET ?= dotnet
SOLUTION := backend-code-challenge.sln
API_PROJECT := ShipManagement/src/ShipManagement.API/ShipManagement.API.csproj
TEST_PROJECT := ShipManagement/tests/ShipManagement.Tests/ShipManagement.Tests.csproj

SQLCMD ?= sqlcmd
SQL_SERVER ?= localhost
SQL_DATABASE ?= ShipManagement
SQL_USERNAME ?= SA
SQL_PASSWORD ?= StrongPassword@123
SQL_TRUST_CERT ?= true

SQL_AUTH_FLAGS :=
ifeq ($(strip $(SQL_USERNAME)),)
SQL_AUTH_FLAGS += -E
else
SQL_AUTH_FLAGS += -U "$(SQL_USERNAME)"
ifneq ($(strip $(SQL_PASSWORD)),)
SQL_AUTH_FLAGS += -P "$(SQL_PASSWORD)"
endif
endif

SQLCMD_CERT_FLAG :=
ifeq ($(strip $(SQL_TRUST_CERT)),true)
SQLCMD_CERT_FLAG := -C
endif

SQLCMD_BASE := $(SQLCMD) -S "$(SQL_SERVER)" $(SQL_AUTH_FLAGS) $(SQLCMD_CERT_FLAG)

.PHONY: help restore build clean test run watch debug cert db-drop db-schema db-sp db-sample db-setup

help:
	@printf "Usage: make <target>\n\n"
	@printf "Available targets:\n"
	@printf "  restore     Restore NuGet packages for the solution\n"
	@printf "  build       Build the solution in Debug configuration\n"
	@printf "  clean       Clean build outputs\n"
	@printf "  test        Run the test suite\n"
	@printf "  run         Run the ShipManagement.API launch profile\n"
	@printf "  watch       Run the API with dotnet watch (hot reload)\n"
	@printf "  debug       Build and emit debugging guidance\n"
	@printf "  cert        Trust the ASP.NET Core development certificate\n"
	@printf "  db-drop     Drop the ShipManagement database (if it exists)\n"
	@printf "  db-schema   Create or update the database schema\n"
	@printf "  db-sp       Create or replace stored procedures\n"
	@printf "  db-sample   Seed reference data\n"
	@printf "  db-setup    Run schema, stored procedure, and sample data scripts\n"

restore:
	$(DOTNET) restore $(SOLUTION)

build: restore
	$(DOTNET) build $(SOLUTION)

clean:
	$(DOTNET) clean $(SOLUTION)

test: build
	$(DOTNET) test $(TEST_PROJECT)

run:
	ASPNETCORE_ENVIRONMENT=Development $(DOTNET) run --project $(API_PROJECT) --launch-profile "ShipManagement.API"

watch:
	ASPNETCORE_ENVIRONMENT=Development $(DOTNET) watch --project $(API_PROJECT) run --launch-profile "ShipManagement.API"

debug: build
	@printf "Debug build complete.\n"
	@printf "Tips:\n"
	@printf "  - Start the API with 'make run' (or 'make watch' for hot reload).\n"
	@printf "  - Use VS Code's 'Launch ShipManagement.API' configuration for F5 debugging.\n"

cert:
	$(DOTNET) dev-certs https --trust

db-drop:
	@$(SQLCMD_BASE) -d master -Q "IF DB_ID('$(SQL_DATABASE)') IS NOT NULL BEGIN ALTER DATABASE [$(SQL_DATABASE)] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$(SQL_DATABASE)]; END"

db-schema:
	@$(SQLCMD_BASE) -d master -i Database/DDL_Scripts.sql

db-sp:
	@$(SQLCMD_BASE) -d $(SQL_DATABASE) -i Database/StoredProcedures.sql

db-sample:
	@$(SQLCMD_BASE) -d $(SQL_DATABASE) -i Database/SampleData.sql

db-setup: db-schema db-sp db-sample
