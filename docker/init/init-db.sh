#!/bin/bash
set -euo pipefail

# Wait for SQL Server to start
sleep 15

/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -d master -i /docker-entrypoint-initdb.d/DDL_Scripts.sql
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -d ShipManagement -i /docker-entrypoint-initdb.d/StoredProcedures.sql
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -d ShipManagement -i /docker-entrypoint-initdb.d/SampleData.sql
