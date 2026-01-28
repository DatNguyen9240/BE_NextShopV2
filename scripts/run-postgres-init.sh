#!/usr/bin/env bash
set -euo pipefail

: "${PGHOST:=localhost}"
: "${PGPORT:=5432}"
: "${PGDATABASE:=nextshop_dev}"
: "${PGUSER:=postgres}"
: "${PGPASSWORD:=postgres}"

# Run the postgres init SQL (located in scripts/db/init_postgres.sql)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
psql "host=${PGHOST} port=${PGPORT} dbname=${PGDATABASE} user=${PGUSER} password=${PGPASSWORD}" -f "${SCRIPT_DIR}/db/init_postgres.sql"
