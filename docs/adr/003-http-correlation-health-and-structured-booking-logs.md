# ADR 003: HTTP correlation id, `/health`, and structured booking logs

## Status

Accepted.

## Context

Demonstrations should show **operability**: tracing a request across logs, proving the process is alive for orchestrators, and highlighting **business-critical** actions without shipping a full APM product.

## Decision

1. **`RequestCorrelationMiddleware`**: accept or generate **`X-Request-Id`**, echo it on the response, store it on `HttpContext.Items`, and wrap the pipeline in **`ILogger` BeginScope** so scoped logs inherit the id.
2. **`GET /health`**: register **`AddDbContextCheck<AppDbContext>`** so readiness reflects database connectivity (InMemory in tests; PostgreSQL in Docker).
3. **`BookingService`**: after a successful **`CreateParentBookingAsync`**, emit one **`LogInformation`** line with structured properties (`bookingId`, `teacherOfferingId`, `parentUserId`, `startUtc`).

## Consequences

- **Pros**: Cheap correlation story for interviews; health endpoint for Kubernetes-style probes; booking creation visible in plain logs.
- **Cons**: No distributed trace exporter; no per-IP rate limiting (see README security section for demo scope).

## Alternatives considered

- **OpenTelemetry + exporter**: excellent for production; disproportionate for this sample.
- **Serilog sinks**: adds dependencies; built-in `ILogger` JSON in cloud hosts is enough for the narrative.
