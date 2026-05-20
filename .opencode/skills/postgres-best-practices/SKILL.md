---
name: postgres-best-practices
description: >
  Use when writing SQL, designing schemas, optimizing queries, or reviewing database
  performance for PostgreSQL. Covers indexing, connection pooling, schema design,
  locking, and advanced features. Trigger on terms like "Postgres", "PostgreSQL",
  "SQL", "query", "index", "schema", "performance", "slow query", "EXPLAIN",
  "connection pool", "RLS", or "normalization".
---

# PostgreSQL Best Practices

Comprehensive performance optimization guide for Postgres. Contains rules across 8 categories, prioritized by impact.

Adapted from [supabase/agent-skills/supabase-postgres-best-practices](https://github.com/supabase/agent-skills).

## When to Apply

Reference these guidelines when:
- Writing SQL queries or designing schemas.
- Implementing indexes or query optimization.
- Reviewing database performance issues.
- Configuring connection pooling or scaling.
- Optimizing for Postgres-specific features.

## Rule Categories by Priority

### 1. Query Performance (Critical)
- Use `EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON)` to understand query plans.
- Add indexes on columns used in `WHERE`, `JOIN`, `ORDER BY`, and `GROUP BY`.
- Prefer covering indexes (INCLUDE) to avoid heap lookups.
- Avoid `SELECT *` in production queries.
- Use `LIMIT` with `ORDER BY` on indexed columns for pagination, not `OFFSET` for large tables.

### 2. Connection Management (Critical)
- Use PgBouncer or similar connection pooler in transaction pooling mode.
- Set `max_connections` based on available memory and workload.
- Close connections promptly; never leak them.
- In .NET, use `Npgsql` with proper disposal (`await using`).

### 3. Schema Design
- Normalize to 3NF, then denormalize selectively for read performance.
- Use appropriate data types (e.g., `UUID`, `TIMESTAMPTZ`, `JSONB`, `INET`).
- Add check constraints and foreign keys at the database level.
- Use schemas to organize tables in multi-tenant designs if needed.

### 4. Concurrency
- Understand Postgres locking: row-level, advisory, and deadlock behavior.
- Use `FOR UPDATE`/`FOR SHARE` sparingly and only when necessary.
- Design for optimistic concurrency where possible (version columns).

### 5. Data Access Patterns
- Batch inserts with `INSERT ... VALUES (...), (...)` or `COPY`.
- Use `UPSERT` (`ON CONFLICT`) for idempotent writes.
- Prefer `JSONB` over `JSON` for querying and indexing.

### 6. Monitoring
- Enable `log_min_duration_statement` to catch slow queries.
- Monitor `pg_stat_statements` for top queries by time.
- Set up alerts for connection saturation and replication lag.

### 7. Advanced Features
- Use partial indexes for filtered queries.
- Use expression indexes for computed columns (e.g., `LOWER(email)`).
- Consider `BRIN` indexes for very large, naturally ordered tables.
- Use `LISTEN`/`NOTIFY` for lightweight pub/sub (not a replacement for RabbitMQ).

### 8. Security
- Use Row-Level Security (RLS) for multi-tenant isolation if applicable.
- Grant minimal privileges per service user.
- Encrypt connections with SSL/TLS.

## SaaS Platform Alignment

- Each microservice owns its PostgreSQL database (database-per-service).
- Global EF Core query filters enforce tenant isolation; RLS is an additional defense if needed.
- Use `JSONB` columns for flexible event store or audit log data.
- Connection pooling is critical with many microservices connecting to their own databases.
- Index foreign keys and `TenantId` columns on every table.
