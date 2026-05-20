#!/usr/bin/env bash
set -euo pipefail

echo "Waiting for infrastructure to be ready..."

# Wait for Postgres
until pg_isready -h localhost -p 5432 -U saas_admin > /dev/null 2>&1; do
  echo "Waiting for Postgres..."
  sleep 2
done
echo "Postgres is ready."

# Wait for RabbitMQ Management API
until curl -s -u saas:saas http://localhost:15672/api/overview > /dev/null 2>&1; do
  echo "Waiting for RabbitMQ..."
  sleep 2
done
echo "RabbitMQ is ready."

echo "All infrastructure services are ready."
