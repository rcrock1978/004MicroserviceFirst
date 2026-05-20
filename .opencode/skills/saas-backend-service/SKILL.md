---
name: saas-backend-service
description: >
  Use ONLY when implementing or refactoring a backend microservice in the SaaS platform.
  Covers service project structure, EF Core, MassTransit, MediatR, minimal APIs,
  Result monad, validation pipeline, and domain modeling. Trigger on terms like
  "service", "API", "endpoint", "handler", "DbContext", "MassTransit", "MediatR",
  "command", "query", "aggregate", "domain model", "backend code".
---

# SaaS Backend Service Implementation

Rules and patterns for coding individual .NET microservices in the platform.

## Project structure (per service)

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

## Technology stack

- .NET 8+ with primary constructors and records.
- EF Core with PostgreSQL (JSON columns, arrays, and advanced types allowed).
- MassTransit with RabbitMQ (or Azure Service Bus in cloud environments).
- MediatR for in-process command/query dispatch.
- FluentValidation for request validation.
- Scalar for API documentation (never Swagger).
- YARP at the gateway layer.

## API layer rules

- Use **minimal APIs** exclusively. No MVC controllers.
- Map endpoints in a static `MapEndpoints` method or dedicated endpoint classes.
- Return `Results<T1, T2, ...>` or a `Result<T>` monad mapped to IResult.
- Keep endpoint delegates thin: validate, dispatch MediatR request, map result to HTTP.

## Domain modeling

- Every aggregate root inherits `SaaSCommon.Domain.Entity`.
- Value objects are `record` types with immutable properties.
- Domain events are POCOs stored on the aggregate and dispatched via the outbox.
- Encapsulate behavior in domain methods; do not expose setters for state transitions.

## EF Core rules

- Use `DbContext` as the unit of work. No `IRepository<T>` wrapper.
- Global query filter: `modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _currentTenantId)`.
- Use strongly-typed IDs (e.g., `record OrderId(Guid Value)`) where practical.
- Migrations are applied by a separate Kubernetes Job, never on app startup.

## MassTransit / messaging

- Configure transactional outbox on every DbContext:
  ```csharp
  cfg.AddEntityFrameworkOutbox<MyDbContext>(o =>
  {
      o.QueryMessageLimit = 100;
      o.UsePostgres();
      o.UseBusOutbox();
  });
  ```
- Define event contracts in the service's `.Contracts` project.
- Use `IConsumer<T>` for choreographed reactions.
- Use saga state machines only for flows requiring compensation and a coordinator.
- Add inbox consumers for idempotency:
  ```csharp
  cfg.UseInMemoryOutbox();
  ```
  (or persistent inbox where needed).

## MediatR pipeline

Register behaviors in this order:
1. `LoggingBehavior<TRequest, TResponse>`
2. `ValidationBehavior<TRequest, TResponse>`
3. `TransactionBehavior<TRequest, TResponse>`

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
```

## Validation

- One validator per command/query.
- Validators live in `MyService.Application/Validators`.
- Use `AbstractValidator<T>` with FluentValidation rules.
- `ValidationBehavior` throws `ValidationException` on failure; map to `400 Bad Request` at the API layer.

## Result monad

- Use a shared `Result<T>` / `Result` type for handler outcomes.
- Explicitly model errors (e.g., `Result<Order>.Failure(OrderErrors.NotFound(id))`).
- Map `Result` states to HTTP status codes in the API layer, not inside handlers.

## Error handling

- Global exception handler middleware maps unhandled exceptions to Problem Details (RFC 7807).
- Domain exceptions are caught and mapped to `422 Unprocessable Entity`.
- Validation failures are `400 Bad Request`.
- Authorization failures are `403 Forbidden`; authentication failures are `401 Unauthorized`.

## Anti-corruption layers

- Every external dependency (payment provider, IdP, email/SMS gateway) gets an interface port in `Application` and an adapter in `Infrastructure`.
- Simulated implementations live in `Infrastructure` and are swappable via DI configuration.
- Never leak external SDK types into the domain or application layers.
