# Repository history — 2026-05-02 (UTC day)

Commits on **2026-05-02** from `git log --since=2026-05-02 --until=2026-05-03`:

| Commit | Message |
|--------|---------|
| `3bcb86d` | feat(theme): add theme preset and logo management to school settings |
| `80f3b06` | feat(settings): add UI language support and enhance booking management |
| `810f9e2` | feat(bookings): implement booking cancellation and school settings management |

## Summary

- **Bookings**: Parent cancellation rules (before interview day in school calendar); related API and UI.
- **School settings**: Director-managed **time zone**, **UI language** (`en` / `es`), **theme preset** palettes, optional **school logo** upload; public `school-config` / `school-logo` for anonymous branding.
- **Tests / docs (follow-up in workspace)**: Integration tests for `auth/password`, `hasPasswordLogin`, and `catalog/school-config`; Playwright **demo-account** flows behind `E2E_STACK_URL`; changelog and testing documentation updates.

Reproduce locally:

```bash
git log --since='2026-05-02 00:00' --until='2026-05-03 00:00' --oneline
```
