# Agent Guide: SaaS Microservices Platform

This document contains essential context for AI agents working on this event-driven SaaS platform. Read this before making any code changes.

## Project Overview

Multi-tenant SaaS platform built as .NET microservices. The architecture emphasizes reliability, scalability, and operational correctness. Every service is autonomous, owns its data, and communicates asynchronously via events.

**Tech Stack:** .NET 8+, PostgreSQL, RabbitMQ, Redis, MassTransit, MediatR, EF Core, FluentValidation, OpenTelemetry, Docker, Kubernetes.

## Non-Negotiable Architecture Rules

These decisions are fixed. Do not change them without explicit human approval.

### 1. External Identity Only
- Never build identity/login/JWT issuance into any service
- The platform validates JWTs from an external OAuth2/OIDC provider (Keycloak, Auth0, etc.)
- We are a resource server, not an identity provider
- `UserProfileService` mirrors IdP users and owns app-specific claims (roles, tenant membership)

### 2. Transactional Outbox Is Mandatory
- Never publish events directly after `SaveChangesAsync()`
- Every domain mutation writes events to `outbox_messages` table in the same DB transaction
- MassTransit polls and publishes from the outbox
- Consumers use inbox tables for exactly-once processing
- Without outbox, the "event-driven" claim is structurally false under failure

### 3. Sagas: Orchestrated vs Choreographed
- **Orchestrated** (MassTransit state machine): Only for flows with compensation and a clear coordinator (e.g., order placement)
- **Choreographed** (`IConsumer<T>`): For simple reaction chains without compensation (order-placed → notification, payment-refunded → inventory-restock)
- Do not use state machines for simple one-step reactions

### 4. Database-Per-Service
- Each microservice owns its PostgreSQL database
- No shared databases, no direct cross-service DB queries
- Cross-service data needs go through events or the gateway

### 5. Async-Only Cross-Service Communication
- Services communicate via events only
- No synchronous HTTP calls between services
- Gateway (YARP) handles external HTTP ingress only

## Service Boundaries

| Service | Responsibility | Key Rules |
|---|---|---|
| **Gateway** | YARP reverse proxy, JWT validation, rate limiting | Only ingress point; nothing else externally reachable |
| **Identity** | JWT validation, user profile mirroring | Delegates auth to external IdP; no password handling |
| **Tenant** | Tenant provisioning, configuration, feature flags | Multi-tenant isolation enforced at DB layer |
| **Customer** | Customer profiles, order history projections | Order history is CQRS read model, rebuildable from events |
| **Order** | Order lifecycle, placement saga | Owns the order placement saga state machine |
| **Payment** | Payment processing, refunds | Anti-corruption layer: `IPaymentProvider` port with Stripe/Adyen/Simulated implementations |
| **Inventory** | Stock tracking, reservations | All reservations have TTL (`expires_at`); background job cleans expired |
| **Notification** | Email, SMS, push delivery | One-way only; consumes events, produces side effects. No queryable data. 30-day delivery log max |

## Cross-Cutting Requirements

### Validation Pipeline
- FluentValidation validators for EVERY command and query
- `ValidationBehavior<TRequest, TResponse>` in MediatR pipeline
- Invalid requests rejected before reaching handlers

### Tenant Isolation
- Every aggregate inherits `SaaSCommon.Domain.Entity` with `TenantId`
- Global EF Core query filter enforces tenant isolation at data layer
- Never rely on handlers remembering to filter

### No Repository Abstraction
- Do NOT create `IRepository<T>` wrappers over EF Core
- `DbContext` IS the Unit of Work, `DbSet<T>` IS the Repository
- Use **specification pattern** for complex queries (named query objects in `Application/Queries`)

### Correlation IDs = Trace Context
- Use W3C Trace Context via OpenTelemetry
- `traceparent` propagates through HTTP and MassTransit automatically
- No custom `CorrelationIdMiddleware`

### Result Monad
- All handlers return `Result<T>` or `Result`
- Explicitly model errors; never throw exceptions for expected failures
- Map Result states to HTTP codes at API layer, not in handlers

## Project Structure (Per Service)

```
src/
  MyService/
    MyService.API/              # Minimal APIs, DI registration, middleware
    MyService.Application/      # Commands, queries, handlers, behaviors, validators
    MyService.Domain/           # Aggregates, entities, value objects, domain events
    MyService.Infrastructure/   # EF Core, MassTransit consumers, external clients
    MyService.Contracts/        # DTOs, event contracts (shared with other services)
tests/
  MyService.UnitTests/
  MyService.IntegrationTests/
```

## Coding Standards

- **Minimal APIs only** — No MVC controllers, ever
- **Primary constructors and records** — Use modern C# features
- **CQRS folder split** — `Commands/` and `Queries/` in Application layer
- **Scalar** for API docs — Never Swagger
- **No secrets in code** — Connection strings, keys, credentials go to secrets manager only
- **Migrations as K8s Jobs** — Never run `MigrateAsync()` on app startup
- **Domain behavior in methods** — Encapsulate state transitions; no public setters for business logic

## Infrastructure Requirements

- Docker multi-stage builds with non-root user
- Kubernetes with service mesh (Linkerd/Istio) for mTLS
- Health checks: `/health/live`, `/health/ready`, `/health/startup`
- Polly resilience on every outbound HTTP call (retry + jitter, circuit breaker, timeout)
- OpenTelemetry for traces, metrics, logs with OTLP export
- Audit log stream to append-only store (separate DB or S3 with object lock)

## Testing Expectations

- **Unit tests**: xUnit, in-memory where appropriate, >=90% domain coverage
- **Integration tests**: Testcontainers with real Postgres/RabbitMQ/Redis, >=80% application coverage
- **Contract tests**: Pact between services sharing events
- **Smoke tests**: Against ephemeral deployed environment in CI

## Security Posture

- mTLS between services inside the mesh
- Gateway is the only external ingress
- No secrets in `appsettings.json`
- Audit logging for auth events, permission changes, payment actions, data exports
- NetworkPolicies default-deny

## What Stays Unchanged

Do not change these existing decisions:
- Minimal APIs over controllers
- Database-per-service
- Async-only cross-service communication
- CQRS folder split
- MediatR for in-process dispatch
- YARP for gateway
- Result monad for explicit failure
- Primary constructors and records
- Scalar over Swagger

## Skill References

This project uses `.opencode/skills/` for detailed guidance. Read the relevant skill before making changes in that domain.

### Core SaaS Platform Skills
- `saas-architecture` — Architecture decisions, service boundaries, outbox, sagas
- `saas-backend-service` — Service implementation patterns, EF Core, MassTransit, MediatR pipeline
- `saas-infrastructure` — Docker, K8s, CI/CD, resilience, observability, secrets
- `saas-testing` — Testing pyramid, Testcontainers, Pact, coverage targets

### Adapted from skills.sh
- `codebase-architecture` — Analyze architectural friction, propose deep-module refactors, improve testability and navigability
- `tdd` — Test-driven development with red-green-refactor, vertical slicing, behavior-focused tests
- `postgres-best-practices` — PostgreSQL query tuning, indexing, schema design, connection pooling, and advanced features
- `kubernetes-ops` — K8s cluster planning, Day-0 vs Day-1 decisions, health checks, service mesh, and troubleshooting
- `event-messaging` — Event-driven messaging patterns, AMQP troubleshooting, retry/dead letter handling, outbox and inbox patterns

## Common Pitfalls

1. **Publishing events without outbox** — This is the #1 reliability anti-pattern. Always use transactional outbox.
2. **Synchronous service calls** — Never HTTP between services. Use events.
3. **Missing tenant filter** — Rely on the global EF query filter, not manual `.Where(t => t.TenantId == ...)` in every handler.
4. **Validation in handlers** — Put validation in FluentValidation validators, not handler logic.
5. **Migration on startup** — Use K8s Jobs for migrations to avoid race conditions in rolling deployments.
6. **Custom correlation middleware** — Use OpenTelemetry's W3C Trace Context instead.
