#!/usr/bin/env bash
set -euo pipefail

DB_USER="saas_admin"
DB_PASS="saas_password"
DB_HOST="localhost"
DB_PORT="5432"

DATABASES=("identity" "tenant" "order" "payment" "inventory" "customer" "notification")

echo "Creating service databases..."

for db in "${DATABASES[@]}"; do
  echo "Creating database: $db"
  PGPASSWORD="$DB_PASS" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -tc "SELECT 1 FROM pg_database WHERE datname = '$db'" | grep -q 1 || \
    PGPASSWORD="$DB_PASS" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "CREATE DATABASE \"$db\";"
done

echo "All databases created successfully."
