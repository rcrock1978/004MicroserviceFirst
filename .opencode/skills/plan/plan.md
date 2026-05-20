# SaaS Microservices Platform — Detailed Execution Plan

**Repository**: Single monorepo under `src/`  
**K8s Target**: Generic Kubernetes (Linkerd service mesh)  
**Priority**: Identity → Gateway → Tenant → Order/Payment/Inventory Saga → Customer → Notification  
**MVP**: Simulation stubs for external providers  
**Testing**: Phase 9 (post-functional)

---

## Phase 0: Project Skeleton & Shared Libraries

### Solution Structure
- [ ] Create root directory `SaaSPlatform/`
- [ ] Create `src/` directory
- [ ] Create `tests/` directory
- [ ] Create `deploy/` directory
- [ ] Create `scripts/` directory
- [ ] Initialize Git repository (`git init`)
- [ ] Create `.gitignore` (Visual Studio, .NET, Docker)
- [ ] Create `global.json` (SDK version: `8.0.x`)
- [ ] Create `Directory.Build.props`
  - [ ] Set `TargetFramework` to `net8.0`
  - [ ] Enable `Nullable` and `ImplicitUsings`
  - [ ] Set `LangVersion` to `12.0`
  - [ ] Add shared properties (Company, Product, Version)
- [ ] Create `Directory.Packages.props`
  - [ ] Pin versions for: MassTransit, EF Core, MediatR, FluentValidation, OpenTelemetry, Polly, Scalar, YARP, xUnit, NSubstitute
- [ ] Create `SaaSPlatform.sln` with solution folders:
  - [ ] `src/`
  - [ ] `tests/`
  - [ ] `deploy/`
  - [ ] `Shared/`
  - [ ] `Services/`

### Shared Library: SaaSCommon.Domain
- [ ] Create project `src/Shared/SaaSCommon.Domain/SaaSCommon.Domain.csproj`
- [ ] Implement `Entity.cs`
  - [ ] Property: `Guid Id` (protected setter)
  - [ ] Property: `TenantId TenantId` (protected setter)
  - [ ] Property: `DateTime CreatedAt` (protected setter)
  - [ ] Property: `DateTime UpdatedAt` (protected setter)
  - [ ] Collection: `List<DomainEvent> DomainEvents`
  - [ ] Method: `AddDomainEvent(DomainEvent event)`
  - [ ] Method: `ClearDomainEvents()`
- [ ] Implement `DomainEvent.cs` (abstract record with `DateTime OccurredOn`)
- [ ] Implement `TenantId.cs` (strongly-typed record: `record TenantId(Guid Value)`)
- [ ] Implement `Result.cs` / `Result<T>.cs`
  - [ ] Static factory: `Success(T value)`
  - [ ] Static factory: `Failure(Error error)`
  - [ ] Property: `bool IsSuccess`
  - [ ] Property: `bool IsFailure`
  - [ ] Property: `T Value` (throws if failure)
  - [ ] Property: `Error Error` (throws if success)
  - [ ] Method: `Match(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)`
  - [ ] Method: `Bind(Func<T, Result<TNext>> func)`
  - [ ] Method: `Map(Func<T, TNext> func)`
- [ ] Implement `Error.cs` (record with `Code`, `Message`, `string? Details = null`)
  - [ ] Predefined errors: `NotFound`, `Validation`, `Unauthorized`, `Conflict`
- [ ] Implement strongly-typed ID helpers (optional base record or source generator)

### Shared Library: SaaSCommon.Application
- [ ] Create project `src/Shared/SaaSCommon.Application/SaaSCommon.Application.csproj`
- [ ] Add references: `SaaSCommon.Domain`, `MediatR`, `FluentValidation`
- [ ] Implement `LoggingBehavior<TRequest, TResponse>`
  - [ ] Log request start/end with timing
  - [ ] Log failures at Warning level
- [ ] Implement `ValidationBehavior<TRequest, TResponse>`
  - [ ] Run `IValidator<TRequest>` if registered
  - [ ] Collect validation failures
  - [ ] Return `Result<T>.Failure(ValidationError)` on failure
- [ ] Implement `TransactionBehavior<TRequest, TResponse>`
  - [ ] Begin transaction before handler
  - [ ] Commit on success
  - [ ] Rollback on failure
- [ ] Implement `ICommand` and `IQuery<T>` marker interfaces (optional)

### Shared Library: SaaSCommon.Infrastructure
- [ ] Create project `src/Shared/SaaSCommon.Infrastructure/SaaSCommon.Infrastructure.csproj`
- [ ] Add references: `SaaSCommon.Domain`, `SaaSCommon.Application`, EF Core, MassTransit, OpenTelemetry, Polly
- [ ] Implement `AddMassTransitWithOutbox<TDbContext>()` extension method
  - [ ] Configure MassTransit with RabbitMQ transport
  - [ ] Add EF outbox with Postgres
  - [ ] Add inbox consumer for exactly-once processing
  - [ ] Configure retry policy (exponential backoff, 3 retries)
- [ ] Implement `AddOpenTelemetryInstrumentation()` extension
  - [ ] Add ASP.NET Core instrumentation
  - [ ] Add HTTP client instrumentation
  - [ ] Add MassTransit instrumentation
  - [ ] Add EF Core instrumentation
  - [ ] Configure OTLP exporter (endpoint from config)
- [ ] Implement `AddResiliencePipelines()` extension
  - [ ] Register default Polly pipeline: Retry + CircuitBreaker + Timeout
- [ ] Implement `AddEfCoreTenantFilter<TEntity>()` extension
  - [ ] Helper to apply global query filter for `TenantId`
- [ ] Implement `CurrentTenantService` (scoped, resolves `TenantId` from HTTP header)

### Docker Compose Local Infrastructure
- [ ] Create `docker-compose.yml`
  - [ ] Service: `postgres` (or individual: `postgres-identity`, `postgres-tenant`, etc.)
    - [ ] Image: `postgres:16-alpine`
    - [ ] Environment: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`
    - [ ] Volume: named volume for persistence
    - [ ] Healthcheck: `pg_isready`
  - [ ] Service: `rabbitmq`
    - [ ] Image: `rabbitmq:3-management-alpine`
    - [ ] Ports: `5672`, `15672`
    - [ ] Healthcheck: `rabbitmq-diagnostics ping`
  - [ ] Service: `redis`
    - [ ] Image: `redis:7-alpine`
    - [ ] Healthcheck: `redis-cli ping`
  - [ ] Service: `jaeger`
    - [ ] Image: `jaegertracing/all-in-one:latest`
    - [ ] Ports: `16686`, `4317`
  - [ ] Service: `seq`
    - [ ] Image: `datalust/seq:latest`
    - [ ] Port: `5341`
- [ ] Create `docker-compose.override.yml`
  - [ ] Dev ports exposed
  - [ ] Local volume mounts for logs
- [ ] Create `scripts/wait-for-infra.sh`
  - [ ] Wait for Postgres to accept connections
  - [ ] Wait for RabbitMQ Management API to respond
- [ ] Create `scripts/init-databases.sh`
  - [ ] Create databases: `identity`, `tenant`, `order`, `payment`, `inventory`, `customer`, `notification`
- [ ] Create `Makefile` or `justfile`
  - [ ] Target: `up` → `docker compose up -d`
  - [ ] Target: `down` → `docker compose down`
  - [ ] Target: `logs` → `docker compose logs -f`
  - [ ] Target: `reset` → `down && docker volume rm ... && up`
- [ ] Create `README.md` (Phase 0)
  - [ ] Prerequisites: Docker, .NET 8 SDK
  - [ ] One-command setup: `make up`
  - [ ] Service URLs and ports table

### Validation
- [ ] Verify `dotnet build` succeeds for all shared projects
- [ ] Verify `docker compose up` starts all infrastructure
- [ ] Verify `make up` works end-to-end

---

## Phase 1: Local Development Infrastructure (Ops-Ready)

### Scripts & Tooling
- [ ] Create `scripts/seed-data.sql` (optional seed data for local dev)
- [ ] Create `scripts/health-check.sh`
  - [ ] Check Postgres connectivity per database
  - [ ] Check RabbitMQ queue health
  - [ ] Check Redis connectivity
- [ ] Create `.env.example` file with all required environment variables
- [ ] Update `README.md` with architecture diagram (ASCII or Mermaid)

### Configuration Management
- [ ] Create `appsettings.Development.json` template
- [ ] Document required environment variables:
  - [ ] `ConnectionStrings__Identity`
  - [ ] `ConnectionStrings__Tenant`
  - [ ] `ConnectionStrings__Order`
  - [ ] `ConnectionStrings__Payment`
  - [ ] `ConnectionStrings__Inventory`
  - [ ] `ConnectionStrings__Customer`
  - [ ] `ConnectionStrings__Notification`
  - [ ] `RabbitMq__Host`
  - [ ] `RabbitMq__Username`
  - [ ] `RabbitMq__Password`
  - [ ] `Jwt__Authority` (JWKS endpoint URL)
  - [ ] `Otlp__Endpoint`

---

## Phase 2: Identity Service

### Project Structure
- [ ] Create `src/Identity/IdentityService.Domain/`
- [ ] Create `src/Identity/IdentityService.Application/`
- [ ] Create `src/Identity/IdentityService.Infrastructure/`
- [ ] Create `src/Identity/IdentityService.Contracts/`
- [ ] Create `src/Identity/IdentityService.API/`
- [ ] Add all projects to solution

### Domain Layer
- [ ] Implement `UserProfile` aggregate (inherits `Entity`)
  - [ ] Property: `string ExternalId`
  - [ ] Property: `string Email`
  - [ ] Property: `string DisplayName`
  - [ ] Property: `List<string> Roles`
  - [ ] Property: `Dictionary<string, string> Claims`
  - [ ] Property: `bool IsActive`
  - [ ] Method: `UpdateClaims(Dictionary<string, string> claims)`
  - [ ] Method: `Deactivate()`
  - [ ] Method: `Activate()`
- [ ] Implement domain events:
  - [ ] `UserProfileCreated`
  - [ ] `UserProfileUpdated`
  - [ ] `UserProfileDeactivated`

### Application Layer
- [ ] Implement commands:
  - [ ] `SyncUserFromIdPCommand` / `Handler`
    - [ ] Upsert user from IdP payload
    - [ ] Emit `UserProfileCreated` or `UserProfileUpdated`
  - [ ] `UpdateUserClaimsCommand` / `Handler`
    - [ ] Update claims, emit `UserProfileUpdated`
  - [ ] `DeactivateUserCommand` / `Handler`
    - [ ] Deactivate user, emit `UserProfileDeactivated`
- [ ] Implement queries:
  - [ ] `GetUserProfileByIdQuery` / `Handler`
  - [ ] `GetUserProfilesByTenantQuery` / `Handler`
- [ ] Implement validators:
  - [ ] `SyncUserFromIdPValidator`
  - [ ] `UpdateUserClaimsValidator`
  - [ ] `DeactivateUserValidator`

### Contracts Layer
- [ ] Implement integration events:
  - [ ] `UserProfileCreatedEvent`
  - [ ] `UserProfileUpdatedEvent`
  - [ ] `UserProfileDeactivatedEvent`

### Infrastructure Layer
- [ ] Implement `IdentityDbContext`
  - [ ] DbSet: `UserProfiles`
  - [ ] Global tenant query filter on `UserProfiles`
  - [ ] Outbox configuration
- [ ] Implement `UserProfileConfiguration` (EF Core entity config)
- [ ] Create EF migration: `InitialCreate`
- [ ] Implement JWKS validation middleware
  - [ ] Fetch JWKS from configured endpoint
  - [ ] Cache signing keys with TTL
  - [ ] Validate JWT signature, expiry, issuer, audience
- [ ] Implement `SimulatedJwksMiddleware` for local dev
  - [ ] Accept any well-formed JWT
  - [ ] Extract claims from JWT payload

### API Layer
- [ ] Implement minimal API endpoints:
  - [ ] `GET /api/users/me` → returns current user profile
  - [ ] `POST /api/users/sync` → `SyncUserFromIdP`
  - [ ] `GET /api/users/{id}` → `GetUserProfileById`
  - [ ] `GET /api/users` → `GetUserProfilesByTenant`
- [ ] Add JWT authentication middleware
- [ ] Add authorization policies
- [ ] Register MediatR pipeline behaviors
- [ ] Register MassTransit with outbox
- [ ] Register OpenTelemetry
- [ ] Configure Scalar documentation
- [ ] Health checks: `/health/live`, `/health/ready`, `/health/startup`

### Configuration
- [ ] `appsettings.json`
- [ ] `appsettings.Development.json`
- [ ] `appsettings.Docker.json` (for containerized local dev)

### Validation
- [ ] Verify `dotnet run` starts service
- [ ] Verify `POST /api/users/sync` creates user
- [ ] Verify `GET /api/users/me` returns user
- [ ] Verify outbox table has entries after write
- [ ] Verify RabbitMQ has published events

---

## Phase 3: Gateway

### Project Structure
- [ ] Create `src/Gateway/Gateway.API/`
- [ ] Add to solution

### YARP Configuration
- [ ] Implement `appsettings.json` with YARP routes:
  - [ ] Route: `/api/identity/**` → `IdentityService`
  - [ ] Route: `/api/tenant/**` → `TenantService`
  - [ ] Route: `/api/order/**` → `OrderService`
  - [ ] Route: `/api/customer/**` → `CustomerService`
- [ ] Configure YARP clusters with service discovery (or direct URLs for local dev)

### Middleware
- [ ] Implement JWT validation (reuse JWKS config)
- [ ] Extract claims and forward headers:
  - [ ] `X-User-Id`
  - [ ] `X-Tenant-Id`
  - [ ] `X-Roles`
- [ ] Implement rate limiting:
  - [ ] Fixed window per `X-Tenant-Id`
  - [ ] Secondary limit per client IP
- [ ] Implement CORS policy for external clients

### Health Checks
- [ ] Implement aggregation endpoint `/health/aggregate`
  - [ ] Calls `/health/ready` on each downstream service
  - [ ] Returns 200 if all healthy, 503 if any unhealthy
  - [ ] Includes per-service status in response body

### Documentation
- [ ] Scalar/OpenAPI configuration (if multi-document aggregation possible)
- [ ] Otherwise, document per-service endpoints

### Configuration
- [ ] `appsettings.json`
- [ ] `appsettings.Development.json`

### Validation
- [ ] Verify Gateway proxies requests to IdentityService
- [ ] Verify JWT validation blocks invalid tokens
- [ ] Verify rate limiting throttles excessive requests
- [ ] Verify health aggregation reflects downstream status

---

## Phase 4: Tenant Service

### Project Structure
- [ ] Create `src/Tenant/TenantService.Domain/`
- [ ] Create `src/Tenant/TenantService.Application/`
- [ ] Create `src/Tenant/TenantService.Infrastructure/`
- [ ] Create `src/Tenant/TenantService.Contracts/`
- [ ] Create `src/Tenant/TenantService.API/`
- [ ] Add to solution

### Domain Layer
- [ ] Implement `Tenant` aggregate (inherits `Entity`)
  - [ ] Property: `string Name`
  - [ ] Property: `string Slug`
  - [ ] Property: `TenantConfiguration Configuration`
  - [ ] Property: `List<FeatureFlag> FeatureFlags`
  - [ ] Property: `TenantStatus Status`
  - [ ] Method: `UpdateConfiguration(TenantConfiguration config)`
  - [ ] Method: `EnableFeatureFlag(string key)`
  - [ ] Method: `DisableFeatureFlag(string key)`
  - [ ] Method: `Activate()`
  - [ ] Method: `Deactivate()`
- [ ] Implement value objects:
  - [ ] `TenantConfiguration` (record with settings)
  - [ ] `FeatureFlag` (record with Key, Enabled, Description)
- [ ] Implement domain events:
  - [ ] `TenantProvisioned`
  - [ ] `TenantActivated`
  - [ ] `TenantDeactivated`
  - [ ] `TenantConfigurationUpdated`
  - [ ] `FeatureFlagToggled`

### Application Layer
- [ ] Implement commands:
  - [ ] `ProvisionTenantCommand` / `Handler`
  - [ ] `UpdateTenantConfigurationCommand` / `Handler`
  - [ ] `EnableFeatureFlagCommand` / `Handler`
  - [ ] `DisableFeatureFlagCommand` / `Handler`
  - [ ] `ActivateTenantCommand` / `Handler`
  - [ ] `DeactivateTenantCommand` / `Handler`
- [ ] Implement queries:
  - [ ] `GetTenantByIdQuery` / `Handler`
  - [ ] `GetTenantBySlugQuery` / `Handler`
  - [ ] `GetTenantFeaturesQuery` / `Handler`
- [ ] Implement validators for all commands

### Contracts Layer
- [ ] Implement integration events:
  - [ ] `TenantProvisionedEvent`
  - [ ] `TenantActivatedEvent`
  - [ ] `TenantDeactivatedEvent`
  - [ ] `TenantConfigurationUpdatedEvent`
  - [ ] `FeatureFlagToggledEvent`

### Infrastructure Layer
- [ ] Implement `TenantDbContext`
  - [ ] DbSet: `Tenants`
  - [ ] Note: Tenant query filter may be bypassed for provisioning endpoints
  - [ ] Outbox configuration
- [ ] Implement `TenantConfiguration` (EF Core entity config)
- [ ] Create EF migration: `InitialCreate`

### API Layer
- [ ] Implement minimal API endpoints:
  - [ ] `POST /api/tenants` → `ProvisionTenant`
  - [ ] `GET /api/tenants/{id}` → `GetTenantById`
  - [ ] `GET /api/tenants/by-slug/{slug}` → `GetTenantBySlug`
  - [ ] `PUT /api/tenants/{id}/configuration` → `UpdateTenantConfiguration`
  - [ ] `POST /api/tenants/{id}/features/{key}/enable` → `EnableFeatureFlag`
  - [ ] `POST /api/tenants/{id}/features/{key}/disable` → `DisableFeatureFlag`
  - [ ] `POST /api/tenants/{id}/activate` → `ActivateTenant`
  - [ ] `POST /api/tenants/{id}/deactivate` → `DeactivateTenant`
- [ ] Register MediatR, MassTransit, OpenTelemetry, health checks
- [ ] Configure Scalar

### Validation
- [ ] Verify tenant provisioning creates tenant
- [ ] Verify configuration updates persist
- [ ] Verify feature flags toggle correctly
- [ ] Verify events published to RabbitMQ

---

## Phase 5: Order / Payment / Inventory Saga

### OrderService

#### Project Structure
- [ ] Create `src/Order/OrderService.Domain/`
- [ ] Create `src/Order/OrderService.Application/`
- [ ] Create `src/Order/OrderService.Infrastructure/`
- [ ] Create `src/Order/OrderService.Contracts/`
- [ ] Create `src/Order/OrderService.API/`
- [ ] Add to solution

#### Domain Layer
- [ ] Implement `Order` aggregate (inherits `Entity`)
  - [ ] Property: `Guid CustomerId`
  - [ ] Property: `List<OrderItem> Items`
  - [ ] Property: `OrderStatus Status`
  - [ ] Property: `decimal TotalAmount`
  - [ ] Property: `string? PaymentProviderReference`
  - [ ] Method: `AddItem(ProductId, Quantity, UnitPrice)`
  - [ ] Method: `Place()` → status: Draft → Placed
  - [ ] Method: `MarkPaymentRequested()`
  - [ ] Method: `MarkAsPaid()`
  - [ ] Method: `MarkAsShipped()`
  - [ ] Method: `Complete()`
  - [ ] Method: `Cancel()`
  - [ ] Method: `MarkPaymentFailed()`
- [ ] Implement `OrderItem` value object
  - [ ] Property: `ProductId`, `Quantity`, `UnitPrice`, `LineTotal`
- [ ] Implement `OrderStatus` enum
  - [ ] `Draft`, `Placed`, `PaymentPending`, `Paid`, `Shipped`, `Completed`, `Cancelled`, `PaymentFailed`
- [ ] Implement domain events:
  - [ ] `OrderCreated`
  - [ ] `OrderPlaced`
  - [ ] `OrderPaymentRequested`
  - [ ] `OrderPaid`
  - [ ] `OrderPaymentFailed`
  - [ ] `OrderShipped`
  - [ ] `OrderCompleted`
  - [ ] `OrderCancelled`

#### Application Layer
- [ ] Implement commands:
  - [ ] `CreateOrderCommand` / `Handler`
  - [ ] `PlaceOrderCommand` / `Handler`
  - [ ] `CancelOrderCommand` / `Handler`
  - [ ] `MarkOrderAsShippedCommand` / `Handler`
- [ ] Implement queries:
  - [ ] `GetOrderByIdQuery` / `Handler`
  - [ ] `GetOrdersByCustomerQuery` / `Handler`
  - [ ] `GetOrdersByStatusQuery` / `Handler`
- [ ] Implement validators for all commands
- [ ] Implement saga state machine: `OrderPlacementStateMachine`
  - [ ] State: `Placed`
  - [ ] State: `AwaitingPayment`
  - [ ] State: `Paid`
  - [ ] State: `PaymentFailed`
  - [ ] State: `Cancelled`
  - [ ] Event: `OrderPlaced` → trigger `ReserveStock`
  - [ ] Event: `StockReserved` → trigger `ProcessPayment`
  - [ ] Event: `PaymentProcessed` → transition to `Paid`
  - [ ] Event: `PaymentFailed` → trigger `ReleaseReservation`, transition to `PaymentFailed`
  - [ ] Event: `StockReservationFailed` → transition to `Cancelled`

#### Contracts Layer
- [ ] Implement integration events:
  - [ ] `OrderCreatedEvent`
  - [ ] `OrderPlacedEvent`
  - [ ] `OrderPaymentRequestedEvent`
  - [ ] `OrderPaidEvent`
  - [ ] `OrderPaymentFailedEvent`
  - [ ] `OrderShippedEvent`
  - [ ] `OrderCompletedEvent`
  - [ ] `OrderCancelledEvent`

#### Infrastructure Layer
- [ ] Implement `OrderDbContext`
  - [ ] DbSet: `Orders`
  - [ ] Global tenant query filter
  - [ ] Outbox + inbox configuration
- [ ] Create EF migration: `InitialCreate`
- [ ] Configure MassTransit saga repository (EF Core or Redis)

#### API Layer
- [ ] Implement endpoints:
  - [ ] `POST /api/orders` → `CreateOrder`
  - [ ] `POST /api/orders/{id}/place` → `PlaceOrder`
  - [ ] `POST /api/orders/{id}/cancel` → `CancelOrder`
  - [ ] `POST /api/orders/{id}/ship` → `MarkOrderAsShipped`
  - [ ] `GET /api/orders/{id}` → `GetOrderById`
  - [ ] `GET /api/orders` → `GetOrdersByCustomer`

### PaymentService

#### Project Structure
- [ ] Create `src/Payment/PaymentService.Domain/`
- [ ] Create `src/Payment/PaymentService.Application/`
- [ ] Create `src/Payment/PaymentService.Infrastructure/`
- [ ] Create `src/Payment/PaymentService.Contracts/`
- [ ] Create `src/Payment/PaymentService.API/`
- [ ] Add to solution

#### Domain Layer
- [ ] Implement `Payment` aggregate (inherits `Entity`)
  - [ ] Property: `Guid OrderId`
  - [ ] Property: `decimal Amount`
  - [ ] Property: `PaymentStatus Status`
  - [ ] Property: `string? ProviderReference`
  - [ ] Property: `DateTime? ProcessedAt`
  - [ ] Method: `Process()`
  - [ ] Method: `Fail(string reason)`
  - [ ] Method: `Refund()`
- [ ] Implement `PaymentStatus` enum
  - [ ] `Pending`, `Processing`, `Succeeded`, `Failed`, `Refunded`
- [ ] Implement domain events:
  - [ ] `PaymentProcessed`
  - [ ] `PaymentFailed`
  - [ ] `PaymentRefunded`

#### Application Layer
- [ ] Implement port: `IPaymentProvider`
  - [ ] Method: `Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)`
  - [ ] Method: `Task<PaymentResult> RefundPaymentAsync(string providerReference)`
- [ ] Implement commands:
  - [ ] `ProcessPaymentCommand` / `Handler`
    - [ ] Call `IPaymentProvider.ProcessPaymentAsync`
    - [ ] Emit `PaymentProcessed` or `PaymentFailed`
  - [ ] `RefundPaymentCommand` / `Handler`
    - [ ] Call `IPaymentProvider.RefundPaymentAsync`
    - [ ] Emit `PaymentRefunded`
- [ ] Implement queries:
  - [ ] `GetPaymentByOrderIdQuery` / `Handler`
- [ ] Implement validators

#### Contracts Layer
- [ ] Implement integration events:
  - [ ] `PaymentProcessedEvent`
  - [ ] `PaymentFailedEvent`
  - [ ] `PaymentRefundedEvent`

#### Infrastructure Layer
- [ ] Implement `SimulatedPaymentProvider`
  - [ ] Amount > 0 → success (configurable)
  - [ ] Amount ≤ 0 → failure
  - [ ] Configurable latency (simulate network delay)
  - [ ] Configurable failure rate (random failures for testing)
- [ ] Implement `PaymentDbContext`
  - [ ] DbSet: `Payments`
  - [ ] Global tenant query filter
  - [ ] Outbox + inbox
- [ ] Create EF migration: `InitialCreate`

#### API Layer
- [ ] Implement endpoints:
  - [ ] `GET /api/payments/order/{orderId}` → `GetPaymentByOrderId`

### InventoryService

#### Project Structure
- [ ] Create `src/Inventory/InventoryService.Domain/`
- [ ] Create `src/Inventory/InventoryService.Application/`
- [ ] Create `src/Inventory/InventoryService.Infrastructure/`
- [ ] Create `src/Inventory/InventoryService.Contracts/`
- [ ] Create `src/Inventory/InventoryService.API/`
- [ ] Add to solution

#### Domain Layer
- [ ] Implement `StockItem` aggregate (inherits `Entity`)
  - [ ] Property: `string ProductId`
  - [ ] Property: `int QuantityAvailable`
  - [ ] Property: `int QuantityReserved`
  - [ ] Property: `List<Reservation> Reservations`
  - [ ] Method: `Reserve(Guid orderId, int quantity, TimeSpan ttl)`
  - [ ] Method: `ReleaseReservation(Guid reservationId)`
  - [ ] Method: `AdjustStock(int delta)`
- [ ] Implement `Reservation` entity
  - [ ] Property: `Guid OrderId`
  - [ ] Property: `int Quantity`
  - [ ] Property: `DateTime ExpiresAt`
  - [ ] Property: `ReservationStatus Status`
- [ ] Implement `ReservationStatus` enum
  - [ ] `Active`, `Released`, `Expired`, `Committed`
- [ ] Implement domain events:
  - [ ] `StockReserved`
  - [ ] `StockReservationReleased`
  - [ ] `StockReservationExpired`
  - [ ] `StockAdjusted`

#### Application Layer
- [ ] Implement commands:
  - [ ] `ReserveStockCommand` / `Handler`
    - [ ] Find stock item, reserve quantity
    - [ ] Emit `StockReserved` or failure
  - [ ] `ReleaseReservationCommand` / `Handler`
    - [ ] Release reservation, emit `StockReservationReleased`
  - [ ] `AdjustStockCommand` / `Handler`
    - [ ] Adjust quantity, emit `StockAdjusted`
  - [ ] `ExpireReservationsCommand` / `Handler`
    - [ ] Process expired reservations (called by background job)
- [ ] Implement queries:
  - [ ] `GetStockByProductQuery` / `Handler`
  - [ ] `GetReservationsByOrderQuery` / `Handler`
- [ ] Implement validators

#### Contracts Layer
- [ ] Implement integration events:
  - [ ] `StockReservedEvent`
  - [ ] `StockReservationReleasedEvent`
  - [ ] `StockReservationExpiredEvent`
  - [ ] `StockAdjustedEvent`

#### Infrastructure Layer
- [ ] Implement `InventoryDbContext`
  - [ ] DbSet: `StockItems`
  - [ ] DbSet: `Reservations`
  - [ ] Global tenant query filter
  - [ ] Outbox + inbox
- [ ] Implement background job: `ReservationExpiryJob`
  - [ ] Quartz.NET recurring job (every 1 minute)
  - [ ] Query: `Reservations.Where(r => r.ExpiresAt < now && r.Status == Active)`
  - [ ] For each: release reservation, emit `StockReservationExpired`
- [ ] Create EF migration: `InitialCreate`

#### API Layer
- [ ] Implement endpoints:
  - [ ] `GET /api/inventory/{productId}` → `GetStockByProduct`
  - [ ] `GET /api/inventory/reservations/order/{orderId}` → `GetReservationsByOrder`
  - [ ] `POST /api/inventory/{productId}/adjust` → `AdjustStock`

### Saga Validation
- [ ] Verify `PlaceOrder` publishes `OrderPlaced`
- [ ] Verify `OrderPlaced` triggers `ReserveStock`
- [ ] Verify `StockReserved` triggers `ProcessPayment`
- [ ] Verify `PaymentProcessed` transitions order to `Paid`
- [ ] Verify `PaymentFailed` releases reservation and marks `PaymentFailed`
- [ ] Verify `StockReservationFailed` cancels order
- [ ] Test compensation path end-to-end

---

## Phase 6: Customer Service

### Project Structure
- [ ] Create `src/Customer/CustomerService.Domain/`
- [ ] Create `src/Customer/CustomerService.Application/`
- [ ] Create `src/Customer/CustomerService.Infrastructure/`
- [ ] Create `src/Customer/CustomerService.Contracts/`
- [ ] Create `src/Customer/CustomerService.API/`
- [ ] Add to solution

### Domain Layer
- [ ] Implement `Customer` aggregate (inherits `Entity`)
  - [ ] Property: `string Email`
  - [ ] Property: `string Name`
  - [ ] Property: `string? Phone`
  - [ ] Property: `DateTime CreatedAt`

### Application Layer
- [ ] Implement commands:
  - [ ] `CreateCustomerCommand` / `Handler`
  - [ ] `UpdateCustomerProfileCommand` / `Handler`
- [ ] Implement queries:
  - [ ] `GetCustomerByIdQuery` / `Handler`
  - [ ] `GetCustomerByEmailQuery` / `Handler`
  - [ ] `GetCustomerOrderHistoryQuery` / `Handler`
    - [ ] Uses specification pattern for filtering/sorting
- [ ] Implement event consumers:
  - [ ] `OrderPlacedConsumer` → add to projection
  - [ ] `OrderPaidConsumer` → update status in projection
  - [ ] `OrderShippedConsumer` → update status
  - [ ] `OrderCompletedConsumer` → update status
- [ ] Implement `RebuildOrderHistoryCommand` / `Handler`
  - [ ] Truncate projection table
  - [ ] Replay all order events from event store / outbox
  - [ ] Rebuild projections

### Infrastructure Layer
- [ ] Implement `CustomerDbContext`
  - [ ] DbSet: `Customers`
  - [ ] DbSet: `CustomerOrderHistory` (projection)
  - [ ] Global tenant query filter
  - [ ] Inbox for event consumers
- [ ] Create EF migration: `InitialCreate`

### API Layer
- [ ] Implement endpoints:
  - [ ] `POST /api/customers` → `CreateCustomer`
  - [ ] `GET /api/customers/{id}` → `GetCustomerById`
  - [ ] `GET /api/customers/by-email/{email}` → `GetCustomerByEmail`
  - [ ] `GET /api/customers/{id}/order-history` → `GetCustomerOrderHistory`
  - [ ] `POST /api/customers/order-history/rebuild` → `RebuildOrderHistory`

### Validation
- [ ] Verify customer creation
- [ ] Verify order history projection updates on events
- [ ] Verify rebuild endpoint recreates projections correctly

---

## Phase 7: Notification Service

### Project Structure
- [ ] Create `src/Notification/NotificationService.Domain/`
- [ ] Create `src/Notification/NotificationService.Application/`
- [ ] Create `src/Notification/NotificationService.Infrastructure/`
- [ ] Create `src/Notification/NotificationService.Contracts/`
- [ ] Create `src/Notification/NotificationService.API/`
- [ ] Add to solution

### Domain Layer
- [ ] Implement `DeliveryLog` entity (not an aggregate — ops only)
  - [ ] Property: `string EventType`
  - [ ] Property: `string Recipient`
  - [ ] Property: `string Channel` (Email/SMS/Push)
  - [ ] Property: `DeliveryStatus Status`
  - [ ] Property: `string? Error`
  - [ ] Property: `DateTime SentAt`

### Application Layer
- [ ] Implement port: `INotificationProvider`
  - [ ] Method: `Task<DeliveryResult> SendEmailAsync(string to, string subject, string body)`
  - [ ] Method: `Task<DeliveryResult> SendSmsAsync(string to, string message)`
  - [ ] Method: `Task<DeliveryResult> SendPushAsync(string deviceToken, string title, string body)`
- [ ] Implement event consumers:
  - [ ] `UserProfileCreatedConsumer` → welcome email
  - [ ] `OrderPlacedConsumer` → order confirmation
  - [ ] `OrderPaidConsumer` → payment receipt
  - [ ] `OrderShippedConsumer` → shipping notification
- [ ] Implement background job: `CleanupDeliveryLogJob`
  - [ ] Delete entries older than 30 days
  - [ ] Run daily

### Infrastructure Layer
- [ ] Implement `SimulatedEmailProvider`
  - [ ] Logs to console/Seq with full payload
  - [ ] Returns success immediately
- [ ] Implement `NotificationDbContext`
  - [ ] DbSet: `DeliveryLogs`
  - [ ] Inbox for consumers
  - [ ] No tenant filter (logs are ops-global, or filter by service)
- [ ] Create EF migration: `InitialCreate`

### API Layer
- [ ] Implement health check endpoints only
- [ ] No business API endpoints (pure consumer service)

### Validation
- [ ] Verify events consumed and processed
- [ ] Verify delivery logs written
- [ ] Verify old logs cleaned up after 30 days

---

## Phase 8: Kubernetes Manifests & CI/CD

### K8s Base Manifests (`deploy/k8s/base/`)
Per service, create:
- [ ] `deployment.yaml`
  - [ ] Container spec with readiness/liveness/startup probes
  - [ ] Resource requests/limits
  - [ ] Non-root security context
  - [ ] Environment variables from ConfigMap/Secret
- [ ] `service.yaml` (ClusterIP)
- [ ] `serviceaccount.yaml`
- [ ] `migration-job.yaml` (EF migrations)
- [ ] `hpa.yaml` (HorizontalPodAutoscaler)
- [ ] `pdb.yaml` (PodDisruptionBudget)

Shared resources:
- [ ] `namespace.yaml`
- [ ] `networkpolicy-default-deny.yaml`
- [ ] `networkpolicy-allow-mesh.yaml`
- [ ] `networkpolicy-allow-gateway.yaml`
- [ ] `ingress-gateway.yaml`

### Kustomize Overlays
- [ ] `deploy/k8s/overlays/dev/`
  - [ ] `kustomization.yaml`
  - [ ] Replicas: 1
  - [ ] Debug logging
  - [ ] Local image tags
- [ ] `deploy/k8s/overlays/staging/`
  - [ ] `kustomization.yaml`
  - [ ] Replicas: 2
  - [ ] Staging domain
  - [ ] Moderate resources
- [ ] `deploy/k8s/overlays/prod/`
  - [ ] `kustomization.yaml`
  - [ ] Replicas: 3+
  - [ ] Production domain
  - [ ] Full resource limits

### CI/CD (`.github/workflows/`)
- [ ] `build.yml`
  - [ ] Build all service projects
  - [ ] Run unit tests
  - [ ] Upload test results
- [ ] `integration-test.yml`
  - [ ] Spin up Testcontainers
  - [ ] Run integration tests
  - [ ] Upload coverage reports
- [ ] `deploy.yml`
  - [ ] Build and push container images (GHCR or ACR)
  - [ ] Run migration Jobs
  - [ ] Deploy to K8s via kubectl/kustomize
  - [ ] Verify rollout success
- [ ] `contract-test.yml`
  - [ ] Run Pact verification
  - [ ] Trigger on Contract project changes or nightly
- [ ] `smoke-test.yml`
  - [ ] Deploy to ephemeral namespace
  - [ ] Run smoke tests
  - [ ] Tear down ephemeral namespace

### Dockerfiles
Per service:
- [ ] Multi-stage build
- [ ] Runtime image: `mcr.microsoft.com/dotnet/aspnet:8.0`
- [ ] Non-root user (`USER $APP_UID`)
- [ ] Read-only root filesystem where possible

### Helm Chart (Optional)
- [ ] `deploy/helm/saas-platform/`
  - [ ] `Chart.yaml`
  - [ ] `values.yaml`
  - [ ] `values-dev.yaml`
  - [ ] `values-staging.yaml`
  - [ ] `values-prod.yaml`
  - [ ] Templates for Deployment, Service, Ingress, etc.

---

## Phase 9: Testing Suite

### Unit Tests
- [ ] Create `tests/SaaSCommon.Domain.Tests/`
  - [ ] `ResultTests.cs` — Success, Failure, Match, Bind, Map
  - [ ] `EntityTests.cs` — Domain events, TenantId
- [ ] Create per-service unit test projects:
  - [ ] `IdentityService.Domain.Tests/`
  - [ ] `IdentityService.Application.Tests/`
  - [ ] `TenantService.Domain.Tests/`
  - [ ] `TenantService.Application.Tests/`
  - [ ] `OrderService.Domain.Tests/`
  - [ ] `OrderService.Application.Tests/`
  - [ ] `PaymentService.Domain.Tests/`
  - [ ] `PaymentService.Application.Tests/`
  - [ ] `InventoryService.Domain.Tests/`
  - [ ] `InventoryService.Application.Tests/`
  - [ ] `CustomerService.Domain.Tests/`
  - [ ] `CustomerService.Application.Tests/`
  - [ ] `NotificationService.Domain.Tests/`
  - [ ] `NotificationService.Application.Tests/`

### Integration Tests
- [ ] Create `tests/IdentityService.IntegrationTests/`
  - [ ] Testcontainers: Postgres, RabbitMQ
  - [ ] `WebApplicationFactory`
  - [ ] Tests: create user, sync from IdP, outbox behavior
- [ ] Create `tests/OrderService.IntegrationTests/`
  - [ ] Testcontainers: Postgres, RabbitMQ
  - [ ] Tests: create order, place order, saga flow
- [ ] Create `tests/PaymentService.IntegrationTests/`
  - [ ] Tests: process payment (simulated), refund
- [ ] Create `tests/InventoryService.IntegrationTests/`
  - [ ] Tests: reserve stock, release, TTL expiry
- [ ] Create `tests/CustomerService.IntegrationTests/`
  - [ ] Tests: create customer, event consumption, projection rebuild

### Contract Tests (Pact)
- [ ] Create `tests/ContractTests/`
  - [ ] Consumer test: OrderService → Payment events
  - [ ] Consumer test: OrderService → Inventory events
  - [ ] Consumer test: OrderService → Customer events
  - [ ] Consumer test: All services → Notification events
  - [ ] Provider verification tests for each service

### Smoke Tests
- [ ] Create `tests/SmokeTests/`
  - [ ] `FullOrderFlowTests.cs`
    - [ ] Step 1: Sync user via Identity
    - [ ] Step 2: Provision tenant
    - [ ] Step 3: Create customer
    - [ ] Step 4: Place order
    - [ ] Step 5: Verify payment processed
    - [ ] Step 6: Verify stock reserved
    - [ ] Step 7: Verify order history updated
    - [ ] Step 8: Verify notification sent
  - [ ] Run against Docker Compose locally
  - [ ] Run against ephemeral K8s in CI

### Test Utilities
- [ ] Create shared test utilities:
  - [ ] `TestWebApplicationFactory<TProgram>` base class
  - [ ] `PostgresContainerFixture` (Testcontainers)
  - [ ] `RabbitMqContainerFixture` (Testcontainers)
  - [ ] `IntegrationTestCollection` (xUnit collection fixtures)
  - [ ] `CustomerBuilder`, `OrderBuilder`, `UserProfileBuilder` (test data builders)
