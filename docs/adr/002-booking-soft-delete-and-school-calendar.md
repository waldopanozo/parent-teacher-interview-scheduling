# ADR 002: Booking “soft delete”, audit, and school-calendar rules

## Status

Accepted.

## Context

Parents cancel interviews online; schools need an **audit trail** (who cancelled, when) and rules tied to the **school’s** calendar, not only UTC.

## Decision

- Keep booking rows when a parent cancels: set **`CancelledAt`** / **`CancelledByUserId`** instead of deleting the row.
- Enforce “cancel only **before** the interview’s calendar day in the **school time zone**” in `BookingService` (`ParentMayCancelBookingAsync`), aligned with slot generation in `SlotGenerator` and the SPA’s “date” picker semantics.

## Consequences

- **Pros**: Directors can review history and cancellations; visit audit endpoints remain truthful.
- **Cons**: Table growth over years—acceptable for this demo; production would add retention/archival policy.

## Alternatives considered

- **Hard delete**: simpler schema; loses compliance and support value.
- **Cancel until N hours before slot**: harder to explain to parents than “not on the day of the meeting”.
