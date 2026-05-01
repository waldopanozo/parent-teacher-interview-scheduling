# Parent–Teacher Interview Scheduling

English-only portfolio system for scheduling **15-minute** parent–teacher interviews. Teachers publish **weekly** availability windows per **subject / course / grade** offering; parents browse the catalog and reserve open slots.

This project is intentionally aligned with a modern **.NET + Angular** full-stack profile (REST APIs, institutional Google sign-in, PostgreSQL, Docker, CI) similar to the expectations described in external role postings such as [Senior Full Stack Developer (.NET Core & Angular)](https://talent.latinolegends.com/jobs/7030828-senior-full-stack-developer-net-core-angular).

## Architecture (how this maps from other workspace work)

- **Layering**: controllers delegate to focused services, EF Core repositories are expressed through `AppDbContext`, and configuration is grouped in `Options` classes. This mirrors the **service + repository separation** described in `resume-api`’s architecture notes (presentation → services → data).
- **Security model**: **Google ID token validation** on the API issues a **stateless JWT** for subsequent calls, comparable to the **JWT gate** approach used in the Java Spring Security module under `portfolio/sistema-seguridad-escolar-eventos-alertas` (stateless API access, role-aware endpoints).
- **Scheduling rules**: weekly templates are expanded into discrete UTC slots using a configured **IANA time zone** (school-local wall clock), reducing ambiguity for remote parents and distributed staff.

## Stack

- **Backend**: ASP.NET Core 10, EF Core, PostgreSQL (Npgsql), Google ID token validation (`Google.Apis.Auth`), JWT bearer authentication.
- **Frontend**: Angular 19 (standalone APIs, lazy routes), Google Identity Services button, `HttpClient` + interceptor.
- **DevOps**: `Dockerfile` for the API, `docker-compose.yml` for PostgreSQL + API, GitHub Actions workflow for build verification.

## Quick start (local)

1. Start PostgreSQL (or use Docker Compose for `db` + `api`).
2. Configure secrets and domain policy:
   - `Google:WebClientId` (or `GOOGLE_OAUTH_CLIENT_ID`) must match the OAuth **Web** client used by the Angular app.
   - `Auth:AllowedEmailDomains` is a comma-separated list of permitted **email domains** (e.g. `northview.edu`).
   - `Auth:TeacherBootstrapEmails` is a comma-separated list of institutional emails that should get the **Teacher** role on first sign-in (everyone else becomes a **Parent**).
3. Run the API:

```bash
cd backend/InterviewScheduling.Api
dotnet run --launch-profile http
```

4. Run the Angular app (proxies `/api` to `http://localhost:5103`):

```bash
cd frontend
npm install
npm start
```

5. Copy `/.env.example` principles into your environment, and set `frontend/src/environments/environment.development.ts` `googleClientId` to the same Web client id.

## Docker Compose

```bash
docker compose --env-file .env up --build
```

The API listens on `http://localhost:5103` (mapped from container port `8080`). Ensure `GOOGLE_OAUTH_CLIENT_ID`, `ALLOWED_EMAIL_DOMAINS`, and `TEACHER_BOOTSTRAP_EMAILS` are set for meaningful authentication tests.

## CI note

The workflow lives in `/.github/workflows/build.yml`, assumes this folder is the **git repository root**, and runs on **pushes to `dev`** (including merges of pull requests into `dev`). If you keep the portfolio monorepo layout, move or adapt the workflow paths to match your repository root.

## API surface (v1)

- `POST /api/v1/auth/google` — exchange Google `id_token` for API JWT.
- `GET /api/v1/subjects` — list catalog subjects (seeded).
- `GET /api/v1/catalog/teacher-offerings` — list bookable offerings.
- `GET /api/v1/catalog/teacher-offerings/{id}/slots?date=YYYY-MM-DD` — list available slots.
- `POST /api/v1/teacher/offerings` — create an offering (Teacher).
- `PUT /api/v1/teacher/offerings/{id}/weekly-availability` — replace weekly windows for an offering (Teacher).
- `POST /api/v1/parent/bookings` — reserve a slot (Parent).
- `GET /api/v1/parent/bookings` / `GET /api/v1/teacher/bookings` — list bookings.

## License

Portfolio sample code; use and modify freely for demonstrations.
