---
name: saas-architecture
description: >
  Use ONLY when designing, reviewing, or changing the SaaS microservices platform architecture.
  Covers identity, outbox pattern, saga orchestration vs choreography, service boundaries,
  CQRS, event-driven design, and cross-cutting concerns. Trigger on terms like
  "architecture", "service boundary", "outbox", "saga", "microservices", "CQRS",
  "event-driven", "tenant isolation", "platform design".
---

# SaaS Microservices Platform Architecture

This skill defines the architectural blueprint for the event-driven SaaS platform.
Every service, boundary, and cross-cutting decision must align with these rules.

## Foundational shifts

### Identity is external

- The platform is a **resource server**, never an identity provider.
- Use a dedicated OAuth2/OIDC provider (Keycloak, Auth0, Azure AD B2C, Duende IdentityServer).
- JWT validation uses JWKS with rotating keys.
- The platform retains only a `UserProfileService` that mirrors IdP users into the tenant model and owns app-specific claims (roles, tenant membership, feature entitlements).

### Outbox pattern is mandatory

- Never publish events directly after `SaveChangesAsync`.
- Use MassTransit transactional outbox in every service.
- Every domain mutation writes the event to an `outbox_messages` table inside the same transaction as the entity change.
- A relay polls and publishes outbox messages.
- Consumers use inbox tables for exactly-once semantics.

### Sagas: orchestrated vs choreographed

- **Orchestrated sagas** (MassTransit state machine): use only for flows with compensation and a clear coordinator (e.g., order placement).
- **Choreographed reactions**: use plain `IConsumer<T>` for simple event chains without compensation (e.g., order-placed → notification, customer-created → welcome-email, payment-refunded → inventory-restock).

## Service boundaries

### NotificationService
- One-way only. Consumes events, produces side effects (email, SMS, push).
- Does not own queryable data. Delivery logs are ops-only, max 30 days.

### CustomerService
- Denormalized order history is a **CQRS read model**, not a copy.
- Subscribe to order events and project into a read-optimized table.
- Must be versioned and rebuildable from the event stream.
- Provide a rebuild endpoint and snapshot capability.

### PaymentService
- Structure with an anti-corruption layer: `IPaymentProvider` port.
- Implementations: `StripePaymentProvider`, `AdyenPaymentProvider`, `SimulatedPaymentProvider`.
- Domain code never knows about simulation heuristics (e.g., "amount > 0 = success").

### InventoryService
- Reservations must have a TTL (`expires_at`).
- Background job releases expired reservations.
- Saga compensation explicitly cancels reservations on payment failure; do not rely solely on TTL.

## Cross-cutting concerns

### Validation
- Wire FluentValidation via a MediatR pipeline behavior: `ValidationBehavior<TRequest, TResponse>`.
- Write validators for every command.
- Reject invalid requests before they reach handlers.
- Add `LoggingBehavior` and `TransactionBehavior` to the same pipeline.

### Repository abstraction
- Do NOT add `IRepository<T>` over EF Core — `DbContext` is the Unit of Work and `DbSet<T>` is the Repository.
- Use the **specification pattern** for complex queries: extract into named query objects in `Application/Queries`.
- Keep handlers thin; queries must be testable.

### Entity base class
- Every aggregate inherits from `SaaSCommon.Domain.Entity`.
- The base class owns `Id`, `TenantId`, `CreatedAt`, `UpdatedAt`.
- Add a global EF query filter for `TenantId` so tenant isolation is enforced at the data layer.

### Correlation IDs
- Use W3C Trace Context via OpenTelemetry.
- The trace ID is the correlation ID; it propagates through HTTP, MassTransit, and structured logs automatically via `Activity.Current`.
- Do not write custom `CorrelationIdMiddleware`.

## What stays exactly as designed

Preserve these decisions without change:
- Minimal APIs over controllers.
- Database-per-service.
- Async-only cross-service communication (no synchronous HTTP between services).
- CQRS folder split (`Commands` / `Queries`).
- MediatR for in-process dispatch.
- YARP for gateway.
- Result monad for explicit failure semantics.
- Primary constructors and records.
- Scalar over Swagger.
