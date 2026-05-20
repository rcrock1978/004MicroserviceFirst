---
name: saas-testing
description: >
  Use ONLY when writing, reviewing, or running tests for the SaaS platform.
  Covers unit tests, integration tests with Testcontainers, contract tests with Pact,
  smoke tests, and testing patterns for EF Core, MassTransit, and MediatR.
  Trigger on terms like "test", "xUnit", "Testcontainers", "Pact", "mock",
  "integration test", "unit test", "contract test", "smoke test", "fixture",
  "Arrange-Act-Assert", "TDD", "BDD".
---

# SaaS Testing Strategy

Testing depth and patterns for the event-driven microservices platform.

## Testing pyramid

1. **Unit tests** — fast, in-memory, no external dependencies.
2. **Integration tests** — real Postgres, RabbitMQ, Redis via Testcontainers.
3. **Contract tests** — Pact between services that share event schemas.
4. **Smoke tests** — run against a deployed ephemeral environment in CI.

## Unit tests

- Framework: xUnit.
- Domain logic: test aggregate methods, value object equality, domain event emission.
- Application layer: test handlers by mocking `DbContext` (or using in-memory SQLite/EF In-Memory) and mocking outbound ports.
- API layer: test endpoint mapping and result-to-HTTP translation.
- Use `NSubstitute` or `Moq` for mocking; `FluentAssertions` for assertions.

### In-memory EF caveats

- In-memory EF is acceptable for handler logic tests but **not** for integration tests.
- It does not catch SQL-specific bugs (JSON columns, arrays, transactions, unique constraints, query translation).
- Never rely on in-memory EF for schema or transaction behavior validation.

## Integration tests

- Use **Testcontainers** to spin up real dependencies:
  - `PostgreSQLContainer` for the service database.
  - `RabbitMqContainer` for message broker tests.
  - `RedisContainer` for caching/distributed lock tests.
- Use `WebApplicationFactory` with `TestServer` to run the full service stack.
- Reset database state between tests (truncate tables or recreate container).
- Test the outbox pattern: assert that domain changes and outbox entries are committed atomically.
- Test MassTransit consumers: publish an event and assert side effects in the database.

### Integration test project setup

```csharp
public class IntegrationTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder().Build();
    public RabbitMqContainer RabbitMq { get; } = new RabbitMqBuilder().Build();

    public async Task InitializeAsync()
    {
        await Postgres.StartAsync();
        await RabbitMq.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await RabbitMq.DisposeAsync();
        await Postgres.DisposeAsync();
    }
}
```

## Contract tests (Pact)

- Define event contracts in the producer's `.Contracts` project.
- Producer tests verify that emitted events match the contract schema.
- Consumer tests verify that consumers can handle the contract events.
- Run Pact verification in CI before allowing cross-service deployments.
- Version contracts with the service API version; support backward-compatible changes only.

## Smoke tests

- Run against a fully deployed ephemeral environment (e.g., namespace per PR).
- Cover critical paths: create tenant → create customer → place order → pay → receive notification.
- Use `HttpClient` or tools like `k6`/`Artillery` for load/smoke validation.
- Fail the pipeline if smoke tests do not pass.

## Test naming and structure

- Follow `MethodName_StateUnderTest_ExpectedBehavior` or BDD-style `Given_When_Then`.
- Use `Arrange-Act-Assert` comments or vertical whitespace to separate sections.
- One logical assertion per test; use parameterized tests for multiple cases.

## Test data

- Use builders (e.g., `OrderBuilder`, `CustomerBuilder`) to create test data.
- Avoid shared mutable test state between tests.
- Seed reference data in integration tests via SQL scripts or EF seeding.

## Coverage targets

- Domain layer: >= 90% branch coverage.
- Application layer: >= 80% branch coverage.
- Infrastructure layer: tested via integration tests, not unit coverage targets.
- API layer: tested via integration and smoke tests.

## Continuous testing

- Unit tests run on every build (< 2 minutes total).
- Integration tests run on PR and merge to main (< 10 minutes total).
- Contract tests run when `.Contracts` projects change or on nightly schedule.
- Smoke tests run after deployment to ephemeral environment.
