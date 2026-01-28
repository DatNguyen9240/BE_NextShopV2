param(
  [string]$PGHOST = $(if ($env:PGHOST) { $env:PGHOST } else { 'localhost' }),
  [string]$PGPORT = $(if ($env:PGPORT) { $env:PGPORT } else { '5432' }),
  [string]$PGDATABASE = $(if ($env:PGDATABASE) { $env:PGDATABASE } else { 'nextshop_dev' }),
  [string]$PGUSER = $(if ($env:PGUSER) { $env:PGUSER } else { 'postgres' }),
  [string]$PGPASSWORD = $(if ($env:PGPASSWORD) { $env:PGPASSWORD } else { 'postgres' })
)

$env:PGPASSWORD = $PGPASSWORD
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
psql "host=$PGHOST port=$PGPORT dbname=$PGDATABASE user=$PGUSER" -f (Join-Path $scriptDir "db/init_postgres.sql")
