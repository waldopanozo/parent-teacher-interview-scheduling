# Parent–Teacher Interview Scheduling

English-only demonstration project for scheduling **15-minute** parent–teacher interviews. Teachers publish **weekly** availability windows per **subject / course / grade** offering; parents browse the catalog and reserve open slots.

This project is intentionally aligned with a modern **.NET + Angular** full-stack profile (REST APIs, institutional Google sign-in, PostgreSQL, Docker, CI) similar to the expectations described in external role postings such as [Senior Full Stack Developer (.NET Core & Angular)](https://talent.latinolegends.com/jobs/7030828-senior-full-stack-developer-net-core-angular).

## Run everything with Docker (recommended)

All runtime pieces—**PostgreSQL**, **ASP.NET Core API**, and **Angular UI (nginx)**—are started together with Docker Compose. You do not need a local Node or .NET SDK install for day-to-day use.

### Prerequisites

- [Docker Engine](https://docs.docker.com/engine/install/) and [Docker Compose](https://docs.docker.com/compose/install/) v2+

### 1. Environment file

Copy the example and edit values (especially the Google Web client id and school domains):

```bash
cp .env.example .env
```

| Variable | Purpose |
|----------|---------|
| `GOOGLE_OAUTH_CLIENT_ID` | Google OAuth **Web** client id (used by the API to validate tokens and baked into the frontend image at build time). |
| `ALLOWED_EMAIL_DOMAINS` | Comma-separated email domains allowed to sign in (e.g. `northview.edu`). |
| `TEACHER_BOOTSTRAP_EMAILS` | Comma-separated institutional emails that receive the **Teacher** role on first sign-in; everyone else is a **Parent**. |

### 2. Google Cloud Console

Create an OAuth **Web application** client. Under **Authorized JavaScript origins**, add:

- `http://localhost:3456`

Under **Authorized redirect URIs**, you typically only need origins for the GIS button flow; keep defaults aligned with Google’s current guidance for Sign In With Google on the web.

### 3. Start the stack

From the repository root:

```bash
docker compose --env-file .env up --build
```

First run applies EF Core migrations and seeds subjects. Wait until all three services are up.

### 4. Open the app

| Service | URL | Notes |
|---------|-----|--------|
| **Web app** | [http://localhost:3456](http://localhost:3456) | Angular static files + nginx (container listens on 80 internally; host uses **3456**). Browser calls `/api/...` on the same origin, proxied to the API. |
| **API (optional direct access)** | [http://localhost:5103](http://localhost:5103) | Useful for debugging or OpenAPI in Development. |
| **PostgreSQL (optional host access)** | `localhost:5544` | User `postgres`, password `postgres`, database `pta_interviews`. |

Stop the stack with `Ctrl+C` or:

```bash
docker compose down
```

To remove the database volume as well:

```bash
docker compose down -v
```

## Stack

- **Backend**: ASP.NET Core 10, EF Core, PostgreSQL (Npgsql), Google ID token validation (`Google.Apis.Auth`), JWT bearer authentication.
- **Frontend**: Angular 19 (standalone, lazy routes), Google Identity Services, production build served by **nginx** with `/api` reverse proxy to the API container.
- **Data**: PostgreSQL 16 in Docker with a named volume for persistence.
- **CI**: GitHub Actions workflow builds backend and frontend on pushes to `dev` (see below).

## Architecture (how this maps from other workspace work)

- **Layering**: controllers delegate to focused services, EF Core is accessed through `AppDbContext`, and configuration lives in `Options` classes—similar to the **service + repository** style described in `resume-api`’s architecture notes.
- **Security model**: Google ID token validation on the API issues a **stateless JWT** for later calls, in the same spirit as a **JWT gate** pattern on stateless APIs.
- **Scheduling rules**: weekly templates expand to UTC slots using a configured **IANA time zone** (`Scheduling:SchoolTimeZoneId`).

## CI note

The workflow in `/.github/workflows/build.yml` assumes this folder is the **git repository root** and runs on **pushes to `dev`**. If this project lives inside a larger monorepo, adjust paths or relocate the workflow.

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
