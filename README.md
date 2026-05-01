# Parent–Teacher Interview Scheduling

Demonstration project for scheduling **15-minute** parent–teacher interviews. Teachers publish **weekly** availability per **subject / course / grade / optional section**; parents browse the catalog and reserve open slots. A **Director** role can approve staff access, manage subjects, assign offerings to teachers, and edit any teacher’s published windows.

Documentation in this repo is **English** for portfolio consistency. UI copy in the app is English.

## Table of contents

1. [What you get](#what-you-get)
2. [Roles](#roles)
3. [Run with Docker (recommended)](#run-with-docker-recommended)
4. [Environment variables](#environment-variables)
5. [Google OAuth setup](#google-oauth-setup)
6. [Local development (optional)](#local-development-optional)
7. [Data model highlights](#data-model-highlights)
8. [API](#api)
9. [Further reading](#further-reading)
10. [Stack & CI](#stack--ci)
11. [License](#license)

## What you get

- **Google Sign-In** on the API: validated `id_token` → short-lived **JWT** for subsequent REST calls.
- **PostgreSQL** + **EF Core** migrations (applied automatically on API startup in the default Docker path).
- **Angular** SPA (lazy routes) behind **nginx**, with `/api` reverse-proxied to the API container.
- **Three roles**: Parent, Teacher, Director (see [docs/flows-and-roles.md](docs/flows-and-roles.md)).

This project is aligned with a modern **.NET + Angular** profile (REST, institutional Google sign-in, PostgreSQL, Docker) similar to expectations in external postings such as [Senior Full Stack Developer (.NET Core & Angular)](https://talent.latinolegends.com/jobs/7030828-senior-full-stack-developer-net-core-angular).

## Roles

| Role | How it is assigned | In the app |
|------|--------------------|------------|
| **Parent** | Default on first sign-in if the email is not in bootstrap lists | Book interviews, **request teacher access** (`/app/parent/request-teacher`). |
| **Teacher** | Email listed in `TEACHER_BOOTSTRAP_EMAILS`, **or** Parent approved by a Director via teacher access request | Create offerings, set weekly availability, view bookings. |
| **Director** | Email listed in `DIRECTOR_BOOTSTRAP_EMAILS` on first sign-in | Approve/reject teacher requests, CRUD subjects, create offerings for any teacher, replace weekly availability for any offering. |

**Important:** after a Director **approves** a teacher access request, the user must **sign out and sign in again** so the JWT includes the Teacher role.

See [docs/flows-and-roles.md](docs/flows-and-roles.md) for step-by-step flows and section labels (e.g. same grade, groups A and C).

## Run with Docker (recommended)

### Prerequisites

- [Docker Engine](https://docs.docker.com/engine/install/) and [Docker Compose](https://docs.docker.com/compose/install/) v2+

### 1. Environment file

```bash
cp .env.example .env
```

Edit `.env` (see [Environment variables](#environment-variables)). At minimum set `GOOGLE_OAUTH_CLIENT_ID` and bootstrap emails you need for testing.

### 2. Start the stack

From the repository root:

```bash
docker compose --env-file .env up --build
```

The API runs EF migrations on startup and seeds baseline **subjects** if the database is empty.

### 3. URLs

| Service | URL | Notes |
|---------|-----|--------|
| **Web app** | [http://localhost:3456](http://localhost:3456) | nginx serves the Angular build; `/api` is proxied to the API. |
| **API** | [http://localhost:5103](http://localhost:5103) | OpenAPI is available in Development (`/openapi/v1.json` depending on version). |
| **PostgreSQL** | `localhost:5544` | User `postgres`, password `postgres`, database `pta_interviews`. |

Stop:

```bash
docker compose down
```

Remove DB volume as well:

```bash
docker compose down -v
```

## Environment variables

These are read by **Docker Compose** and mapped into `Auth__*`, `Google__*`, etc. Copy from [.env.example](.env.example).

| Variable | Purpose |
|----------|---------|
| `GOOGLE_OAUTH_CLIENT_ID` | Google OAuth **Web** client id: API validates tokens; frontend image receives it at **build** time. |
| `ALLOWED_EMAIL_DOMAINS` | Comma-separated allowed **email domains** (e.g. `northview.edu`). **Empty** = any Google-verified email allowed. |
| `ALLOW_PERSONAL_GOOGLE_EMAILS` | When domains are **non-empty**, set `true` to also allow `@gmail.com` / `@googlemail.com`. |
| `TEACHER_BOOTSTRAP_EMAILS` | Comma-separated emails that get **Teacher** on first sign-in. |
| `DIRECTOR_BOOTSTRAP_EMAILS` | Comma-separated emails that get **Director** on first sign-in. |

**Typical combinations**

1. **School domain only** — set `ALLOWED_EMAIL_DOMAINS`, `ALLOW_PERSONAL_GOOGLE_EMAILS=false`, list teachers and at least one director in the bootstrap lists.
2. **Gmail-only pilots** — leave `ALLOWED_EMAIL_DOMAINS` empty; use full addresses in `TEACHER_BOOTSTRAP_EMAILS` / `DIRECTOR_BOOTSTRAP_EMAILS`.
3. **Mixed** — non-empty domains **and** `ALLOW_PERSONAL_GOOGLE_EMAILS=true`.

JWT signing in Docker uses `Jwt__SigningKey` from Compose (dev-only default). For production, supply a strong secret and manage secrets outside this sample.

## Google OAuth setup

Create an OAuth **Web application** client in Google Cloud Console.

- **Authorized JavaScript origins**: `http://localhost:3456` (add production origin when you deploy).
- Use **Sign in with Google** (GIS) for the SPA; the API validates the **ID token** server-side.

## Local development (optional)

If you change code frequently without rebuilding Docker images:

- **Backend**: .NET SDK matching `net10.0`, PostgreSQL reachable, `ConnectionStrings:Default` and `Jwt:SigningKey` (≥32 chars) in user secrets or `appsettings.Development.json`.
- **Frontend**: Node.js LTS, `cd frontend && npm install && npm start` — use [frontend/proxy.conf.json](frontend/proxy.conf.json) (see [frontend/README.md](frontend/README.md)) to reach a local API.

## Data model highlights

- **User** (`AppUser`): role enum Parent / Teacher / Director.
- **Subject**: code + name (seeded + Director CRUD).
- **TeacherOffering**: teacher + subject + course title + grade level + **optional `sectionLabel`** (parallel classes, e.g. `A`, `C`, `3ro-A`).
- **WeeklyAvailability**: day + local start/end (school time zone from `Scheduling:SchoolTimeZoneId`).
- **Booking**: parent + offering + UTC slot.
- **TeacherAccessRequest**: Parent asks for promotion; Director approves/rejects.

## API

Summary endpoints live in [docs/api-reference.md](docs/api-reference.md). Common paths:

- `POST /api/v1/auth/google` — exchange Google `id_token` for JWT.
- `GET /api/v1/catalog/teacher-offerings` — bookable offerings (authenticated).
- `POST /api/v1/parent/bookings` — reserve a slot.
- `POST /api/v1/teacher-access-requests` — Parent submits teacher access request.
- `GET|POST /api/v1/director/...` — Director operations (requests, subjects, offerings, weekly availability).

## Further reading

| Document | Content |
|----------|---------|
| [docs/flows-and-roles.md](docs/flows-and-roles.md) | User journeys, section labels, post-approval re-login. |
| [docs/api-reference.md](docs/api-reference.md) | REST v1 paths and short descriptions. |
| [frontend/README.md](frontend/README.md) | Optional local `npm start` and proxy notes. |

## Stack & CI

- **Backend**: ASP.NET Core 10, EF Core, PostgreSQL (Npgsql), Google ID token validation, JWT bearer.
- **Frontend**: Angular (standalone, lazy routes), Google Identity Services.
- **CI**: [`.github/workflows/build.yml`](.github/workflows/build.yml) runs `dotnet build` and `npm run build` on pushes to **`main`** and **`dev`**, and on **pull requests** targeting those branches.

## License

Portfolio sample code; use and modify freely for demonstrations.
