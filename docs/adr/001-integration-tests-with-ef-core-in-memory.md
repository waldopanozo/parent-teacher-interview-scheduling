# ADR 001: Integration tests with EF Core InMemory

## Status

Accepted.

## Context

The API targets PostgreSQL in production and Docker. Automated tests need to run quickly on CI and developer machines without a real database process.

## Decision

Use **`ASPNETCORE_ENVIRONMENT=Testing`** in `WebApplicationFactory` and register **`UseInMemoryDatabase("IntegrationTests")`** for `AppDbContext`. Migrations are replaced with **`EnsureCreatedAsync()`** in that environment.

## Consequences

- **Pros**: Fast, hermetic builds; no Docker dependency for `dotnet test`; easy seeding of users and bookings per test.
- **Cons**: InMemory does not enforce all PostgreSQL semantics (constraints, raw SQL, some transaction edge cases). Critical paths that depend on Npgsql-specific behavior still warrant occasional manual or containerized checks before release.

## Alternatives considered

- **Testcontainers + PostgreSQL**: closer to production; slower and heavier for a portfolio-sized repo.
- **Shared dev database**: flaky and unsafe for parallel CI.
