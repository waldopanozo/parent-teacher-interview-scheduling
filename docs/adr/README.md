# Architecture Decision Records

Short, time-stamped notes explaining **why** the stack is shaped this way—not a full specification.

| ADR | Title |
|-----|--------|
| [001](001-integration-tests-with-ef-core-in-memory.md) | Integration tests with EF Core InMemory |
| [002](002-booking-soft-delete-and-school-calendar.md) | Booking cancellations, audit trail, and school time zone |
| [003](003-http-correlation-health-and-structured-booking-logs.md) | HTTP correlation id, `/health`, and structured booking logs |

When adding a new cross-cutting concern (auth model change, new persistence boundary, observability standard), add a numbered ADR here and link it from the root [README](../README.md).
