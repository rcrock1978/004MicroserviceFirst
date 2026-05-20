---
name: event-messaging
description: >
  Use when designing, troubleshooting, or optimizing event-driven messaging,
  message brokers, or async communication patterns. Covers connection failures,
  AMQP issues, dead letter handling, and SDK troubleshooting. Trigger on terms like
  "RabbitMQ", "MassTransit", "event bus", "message", "queue", "topic", "exchange",
  "consumer", "publisher", "AMQP", "dead letter", "retry", "event-driven",
  "async communication", or "message broker".
---

# Event-Driven Messaging

Diagnose and resolve event-driven messaging issues with structured troubleshooting workflows.

Adapted from [microsoft/azure-skills/azure-messaging](https://github.com/microsoft/azure-skills).

## When to Use

- Designing event-driven communication between microservices.
- Troubleshooting message delivery failures, consumer errors, or broker connectivity.
- Configuring retry policies, dead letter queues, and idempotency.
- Optimizing throughput and latency of event pipelines.

## Structured Diagnosis Workflow

1. **Identify the SDK/version** — Know your MassTransit/RabbitMQ client version.
2. **Check resource health** — Verify broker is reachable, queues exist, and permissions are correct.
3. **Match error messages** — Look up specific exceptions (e.g., `MessageLockLostException`, `ConnectionResetException`).
4. **Verify configuration** — Connection strings, topology (exchanges, queues, bindings), and policies.
5. **Apply fixes** — Retry configuration, circuit breaker tuning, or topology changes.

## Common Issues

### Connection Failures
- Verify host, port, and credentials.
- Check TLS/SSL configuration matches broker settings.
- Ensure firewall rules allow outbound AMQP (port 5672 or 5671 for TLS).
- Use WebSocket fallback if direct TCP is blocked.

### Consumer Issues
- **Message lock timeout** — Increase visibility timeout or reduce processing time.
- **Poison messages** — Configure dead letter exchange/queue after max retries.
- **Duplicate processing** — Implement idempotent consumers using inbox pattern.

### Retry and Dead Letter
- Use exponential backoff with jitter for transient failures.
- Set max retry attempts before sending to dead letter.
- Monitor dead letter queues and alert on growth.
- Use MassTransit `UseMessageRetry` with `Exponential` or `Interval` retry policy.

## Design Patterns

- **Transactional Outbox** — Never publish directly after `SaveChangesAsync`. Write to outbox table in the same transaction.
- **Inbox Pattern** — Track processed message IDs to ensure exactly-once semantics.
- **Saga State Machines** — Use for orchestrated flows with compensation (e.g., order placement).
- **Choreographed Consumers** — Use `IConsumer<T>` for simple reaction chains without compensation.

## SaaS Platform Alignment

- This skill complements `saas-architecture` and `saas-backend-service`.
- The platform uses MassTransit + RabbitMQ with mandatory transactional outbox.
- Every consumer must use inbox for exactly-once processing.
- Dead letter handling and retry policies are configured in MassTransit, not manually.
- Event contracts live in `.Contracts` projects shared between services.
