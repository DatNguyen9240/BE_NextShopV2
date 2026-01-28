This folder previously contained a generated EF migration and an auto-generated Postgres SQL script.

The SQL initialization script has been moved to:

  scripts/db/init_postgres.sql

Please use the scripts in the `scripts/` folder to run the initialization in dev or pre-deploy steps:

  - scripts/run-postgres-init.sh
  - scripts/run-postgres-init.ps1

These scripts use environment variables: PGHOST, PGPORT, PGDATABASE, PGUSER, PGPASSWORD
