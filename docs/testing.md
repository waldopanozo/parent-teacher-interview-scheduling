# Testing

## Quick reference

| Goal | Where | Command |
|------|--------|---------|
| **Full stack (UI + API + DB)** | Repository root (same folder as `docker-compose.yml`) | `cp .env.example .env` → edit `.env` → `docker compose up --build` |
| **Stop stack** | Repository root | `docker compose down` (add `-v` to drop the Postgres volume) |
| **Backend tests** | `backend/InterviewScheduling.Api.Tests` | `dotnet test` |
| **Playwright smoke only** (no Docker API) | `frontend/` | `npm install` → `npm run build` → `npm run test:e2e` |
| **Playwright smoke + demo flows** (needs live stack) | Terminal A: repo root `docker compose up --build`. Terminal B: add `E2E_STACK_URL=http://127.0.0.1:3456` to **`.env` at repo root** (or `frontend/.env`), then `cd frontend && npm run build && npm run test:e2e` |

**`.env` behavior**

- **Docker Compose**: If the file is named `.env` and sits next to `docker-compose.yml`, Compose loads it **automatically** for `${VAR}` substitution in the YAML. You do **not** need `--env-file .env` unless you use another filename or path.
- **Playwright**: `frontend/playwright.config.ts` loads **`../.env`** then **`./.env`** (second file overrides keys). Set `E2E_STACK_URL` there instead of exporting it in the shell. CI can still set `E2E_STACK_URL` as a job env var.

## Backend (.NET)

Project: `backend/InterviewScheduling.Api.Tests`

- **Unit tests**: `MeetingProfileValidationTests` — validation rules for parent meeting profile fields.
- **Integration tests** (HTTP + in-memory EF, `ASPNETCORE_ENVIRONMENT=Testing`):
  - `ParentMeetingProfileApiTests` — meeting profile PUT/GET and `auth/me` `meetingProfileComplete`.
  - `AuthPasswordApiTests` — `hasPasswordLogin` on `auth/me`, `PUT auth/password` (success, wrong password, Google-only user).
  - `CatalogSchoolConfigApiTests` — anonymous `GET catalog/school-config` returns branding fields.
  - `TeacherBookingsApiTests` — `GET teacher/bookings` maps DTOs with **teacher display name** (regression for EF `Include` on `TeacherOffering.Teacher`).
  - `HealthEndpointTests` — `GET /health` returns **Healthy** in the Testing host.

Run locally:

```bash
cd backend/InterviewScheduling.Api.Tests
dotnet test
```

The main API uses PostgreSQL; tests use **EF Core InMemory** only when `Testing` environment is active (see `Program.cs`).

## Frontend (Angular)

- **Unit**: Karma/Jasmine can target `src/**/*.spec.ts` (optional; not run in CI). Run with `npm test` in `frontend/` (opens a browser by default).
- **E2E**: Playwright in `frontend/e2e/`:
  - **Smoke** (`smoke.spec.ts`): serves the production build with `serve -s` (no API required for these checks). Validates login shell, forgot-password page, and i18n-tolerant selectors.
  - **Demo accounts** (`demo-accounts.spec.ts`): **skipped unless** `E2E_STACK_URL` is set (in the shell or in **`.env` at repo root** or **`frontend/.env`**; Playwright loads both). Exercises `demo-parent@example.com`, `demo-teacher@example.com`, and `demo-director@example.com` (password `password`) against the **full stack** — nginx on port **3456** proxies `/api` to the API. Requires Docker Compose with **Development** (or equivalent) so demo users are seeded.
  - **Full-stack flows** (also require `E2E_STACK_URL` + Docker; `playwright.config.ts` uses **`workers: 1`** and **`fullyParallel: false`** so shared demo data is not mutated concurrently):
    - `flows-register.spec.ts` — email/password registration → meeting registration.
    - `flows-booking-cancel.spec.ts` (serial) — parent books **Algebra I** demo slots (dates follow **`GET /catalog/school-config`** `schoolTimeZoneId`), cancels; teacher marks attendance then parent cancels.
    - `flows-teacher-offering.spec.ts` — teacher creates a new offering and saves a weekly window.
    - `flows-director-subject.spec.ts` — director adds and deletes a subject.
    - `flows-teacher-access.spec.ts` — new parent submits teacher-access request; director approves; user re-signs in as teacher.
  - **Helpers** (`helpers.ts`): login/register/logout, `bookFirstDemoTeacherSlot` (resolves school TZ from the API unless **`E2E_SCHOOL_TZ`** is set in `.env`).

### Playwright (default / CI)

From `frontend/` after `npm install`:

```bash
npm run build
npm run test:e2e
```

This starts `serve` on port **4179** and runs smoke tests only; demo-account tests are **skipped**.

### Playwright against Docker (local)

1. From repo root: `docker compose up --build` (Compose reads a file named `.env` in that folder automatically for `${VAR}` substitution; ensure demo seed is enabled; see README).
2. Add `E2E_STACK_URL=http://127.0.0.1:3456` to `.env` at repo root (see `.env.example`) or to `frontend/.env`.
3. In another terminal:

```bash
cd frontend
npm run build
npm run test:e2e
```

With `E2E_STACK_URL` set (env or `.env`), Playwright does **not** start `serve`; it hits the Compose nginx URL directly. Demo tests run against the **pre-built** assets in `dist/` only for consistency with CI artifacts; the running UI is still the image baked into Compose unless you mount a dev build.

## Continuous integration

On **push to `main`**, GitHub Actions runs `dotnet test` on the test project and `npm run build` + Playwright (`npm run test:e2e`) on the frontend. **Demo Playwright tests stay skipped** in CI unless you add a job that starts Compose and sets `E2E_STACK_URL`. Karma is **not** run in CI.
