# Testing

## Backend (.NET)

Project: `backend/InterviewScheduling.Api.Tests`

- **Unit tests**: `MeetingProfileValidationTests` — validation rules for parent meeting profile fields.
- **Integration tests**: `ParentMeetingProfileApiTests` — HTTP API with in-memory EF Core (`ASPNETCORE_ENVIRONMENT=Testing`), JWT auth, and `appsettings.Testing.json` for secrets-safe JWT configuration.

Run locally:

```bash
cd backend/InterviewScheduling.Api.Tests
dotnet test
```

The main API uses PostgreSQL; tests use **EF Core InMemory** only when `Testing` environment is active (see `Program.cs`).

## Frontend (Angular)

- **Unit**: `src/app/core/schedule-api.service.spec.ts` — minimal construction test for `ScheduleApiService` (Jasmine + Karma). Run with `npm test` (opens a browser by default).
- **E2E**: Playwright in `frontend/e2e/` — smoke test serves the production build with `serve -s` (SPA fallback) and checks the login route.

Run e2e locally (after `npm install` in `frontend/`):

```bash
cd frontend
npm run build
npm run test:e2e
```

## Continuous integration

On **push to `main`**, GitHub Actions runs `dotnet test` on the test project and `npm run build` + Playwright e2e on the frontend. Karma is **not** run in CI (browser setup varies by runner); use `npm test` locally when changing Angular code.
