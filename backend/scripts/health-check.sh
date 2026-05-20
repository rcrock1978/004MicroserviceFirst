#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# Health Check Script
# Checks Postgres per-database connectivity, RabbitMQ queue health, and Redis.
# =============================================================================

DB_USER="saas_admin"
DB_PASS="saas_password"
DB_HOST="localhost"
DB_PORT="5432"

RABBIT_USER="saas"
RABBIT_PASS="saas"
RABBIT_HOST="localhost"
RABBIT_MGMT_PORT="15672"

REDIS_HOST="localhost"
REDIS_PORT="6379"

DATABASES=("identity" "tenant" "order" "payment" "inventory" "customer" "notification")

EXIT_CODE=0

# ---------------------------------------------------------------------------
# Postgres
# ---------------------------------------------------------------------------
echo "=== Postgres Health Check ==="

for db in "${DATABASES[@]}"; do
  if PGPASSWORD="$DB_PASS" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$db" -tc "SELECT 1" > /dev/null 2>&1; then
    echo "  [OK]   Database '$db' is reachable."
  else
    echo "  [FAIL] Database '$db' is NOT reachable."
    EXIT_CODE=1
  fi
done

# ---------------------------------------------------------------------------
# RabbitMQ
# ---------------------------------------------------------------------------
echo ""
echo "=== RabbitMQ Health Check ==="

if curl -s -u "$RABBIT_USER:$RABBIT_PASS" "http://$RABBIT_HOST:$RABBIT_MGMT_PORT/api/overview" > /dev/null 2>&1; then
  echo "  [OK]   RabbitMQ Management API is reachable."
else
  echo "  [FAIL] RabbitMQ Management API is NOT reachable."
  EXIT_CODE=1
fi

# Check that at least one queue exists and is not in a critical state
QUEUE_HEALTH=$(curl -s -u "$RABBIT_USER:$RABBIT_PASS" "http://$RABBIT_HOST:$RABBIT_MGMT_PORT/api/queues" 2>/dev/null || echo "[]")
if [ "$QUEUE_HEALTH" = "[]" ] || [ -z "$QUEUE_HEALTH" ]; then
  echo "  [INFO] No queues found (services may not be running yet)."
else
  # Count queues with > 1000 ready messages as a warning threshold
  HIGH_QUEUES=$(echo "$QUEUE_HEALTH" | grep -o '"messages_ready":[0-9]*' | awk -F: '{if ($2 > 1000) print $2}')
  if [ -n "$HIGH_QUEUES" ]; then
    echo "  [WARN] Some queues have high message counts: $HIGH_QUEUES"
  else
    echo "  [OK]   Queue depths look healthy."
  fi
fi

# ---------------------------------------------------------------------------
# Redis
# ---------------------------------------------------------------------------
echo ""
echo "=== Redis Health Check ==="

if redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" ping | grep -q "PONG"; then
  echo "  [OK]   Redis is responding to PING."
else
  echo "  [FAIL] Redis is NOT responding."
  EXIT_CODE=1
fi

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
echo ""
if [ "$EXIT_CODE" -eq 0 ]; then
  echo "All checks passed."
else
  echo "One or more health checks FAILED."
fi

exit "$EXIT_CODE"
