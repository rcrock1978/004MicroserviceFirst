---
name: saas-infrastructure
description: >
  Use ONLY when creating or editing infrastructure, deployment, CI/CD, secrets,
  resilience, health checks, observability, or operational configurations for the
  SaaS platform. Trigger on terms like "Docker", "Kubernetes", "K8s", "Helm",
  "Terraform", "CI/CD", "GitHub Actions", "health check", "Polly", "resilience",
  "mTLS", "secret", "Key Vault", "migration job", "prometheus", "grafana",
  "OpenTelemetry", "observability", "logging", "audit log".
---

# SaaS Infrastructure & Operations

Deployment, resilience, security, and observability rules for the platform.

## Containerization

- Each service has its own `Dockerfile` using multi-stage builds.
- Base image: `mcr.microsoft.com/dotnet/aspnet:8.0` for runtime; `sdk:8.0` for build.
- Non-root user in final image (`USER $APP_UID`).
- Read-only root filesystem where possible.

## Kubernetes deployment

- One `Deployment` + `Service` per microservice.
- Gateway is the only ingress; all other services are cluster-internal.
- Use a service mesh (Linkerd or Istio) for automatic mTLS between services.
- NetworkPolicies default-deny; allow only mesh and gateway traffic.

## Migrations

- Run EF Core migrations as a Kubernetes `Job` before rolling the Deployment.
- The application verifies schema version on boot but never applies migrations.
- Use `dotnet ef migrations script` or `dotnet ef database update` inside the job container.

## Health checks

- Expose three endpoints:
  - `/health/live` — liveness (is the process running?).
  - `/health/ready` — readiness (can it serve traffic?).
  - `/health/startup` — startup probe (has slow initialization finished?).
- Tag each dependency check (`redis`, `postgres`, `rabbitmq`) so readiness fails granularly.
- Register health checks in `Program.cs` and map them explicitly.

## Resilience (Polly)

- Every outbound HTTP call gets a composed `ResiliencePipeline`:
  - Retry with exponential backoff + jitter.
  - Circuit breaker.
  - Timeout.
- Register pipelines in DI with named configurations:
  ```csharp
  services.AddResiliencePipeline("default", builder =>
  {
      builder.AddRetry(new RetryStrategyOptions { ... })
             .AddCircuitBreaker(new CircuitBreakerStrategyOptions { ... })
             .AddTimeout(TimeSpan.FromSeconds(10));
  });
  ```

## Secrets management

- No secrets in `appsettings.json`.
- Use a secrets manager (Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault).
- Inject secrets via runtime environment or mounted volumes, never commit them.
- Use `IConfiguration` with a Key Vault provider or similar at startup.

## Observability

- OpenTelemetry is wired for traces, metrics, and logs.
- Propagate W3C Trace Context (`traceparent`) through HTTP and MassTransit.
- Structured logging (Serilog or built-in `ILogger` with JSON formatter) includes trace ID automatically.
- Export to OTLP collector (Jaeger, Tempo, or cloud APM).

## Audit logging

- Authentication events, permission changes, payment actions, data exports — publish to a dedicated `audit.*` exchange/topic.
- Consumer writes to an append-only store (separate Postgres DB with no UPDATE/DELETE grants, or S3 with object lock).
- Audit events are immutable and retained per compliance requirements.

## CI/CD

- GitHub Actions (or equivalent) pipeline:
  1. Build & unit test every service.
  2. Run integration tests with Testcontainers.
  3. Build and push container images.
  4. Run contract tests (Pact) between dependent services.
  5. Deploy to ephemeral environment and run smoke tests.
  6. Deploy to staging, then production with canary/blue-green.
- Version container images with Git SHA and semantic version tags.

## Infrastructure as Code

- Prefer Terraform or Pulumi for cloud infrastructure.
- Keep Kubernetes manifests in a `k8s/` or `deploy/` folder, or use Helm charts.
- Separate environment configs (dev, staging, prod) via Kustomize overlays or Helm values files.
