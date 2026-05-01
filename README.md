# Parent–Teacher Interview Scheduling

Demonstration project for scheduling **15-minute** parent–teacher interviews. Teachers publish **weekly** availability per **subject / course / grade / optional section**; parents browse the catalog and reserve open slots. A **Director** role can approve staff access, manage subjects, assign offerings to teachers, and edit any teacher’s published windows.

Documentation in this repo is **English** for portfolio consistency. UI copy in the app is English.

## Table of contents

1. [What you get](#what-you-get)
2. [Roles](#roles)
3. [Authentication](#authentication)
4. [Demo accounts (Development only)](#demo-accounts-development-only)
5. [Parent meeting registration](#parent-meeting-registration)
6. [Run with Docker (recommended)](#run-with-docker-recommended)
7. [Environment variables](#environment-variables)
8. [Google OAuth setup](#google-oauth-setup)
9. [Local development (optional)](#local-development-optional)
10. [Data model highlights](#data-model-highlights)
11. [API](#api)
12. [Further reading](#further-reading)
13. [Testing](#testing)
14. [Stack & CI](#stack--ci)
15. [License](#license)

## What you get

- **Sign-in options**: **Google Sign-In** (validated `id_token`) and **email + password** (`POST /api/v1/auth/register` and `POST /api/v1/auth/email-login`), both returning the same short-lived **JWT** for REST calls.
- **Login / registration UI**: split-panel screen (login ↔ register), **Google Fonts** (Syne + Outfit), indigo-focused palette; only **Google** is offered as an OAuth provider (no other social buttons).
- **PostgreSQL** + **EF Core** migrations (applied automatically on API startup when using Docker or a normal host).
- **Angular** SPA (lazy routes) behind **nginx**, with `/api` reverse-proxied to the API container.
- **Three roles**: Parent, Teacher, Director (see [docs/flows-and-roles.md](docs/flows-and-roles.md)).
- **Development seed**: baseline **subjects** when the catalog is empty, plus optional **demo users** (one per role) when `ASPNETCORE_ENVIRONMENT=Development` (see [Demo accounts](#demo-accounts-development-only)).
- **Parent meeting registration**: student school email, attendee name, and relationship to the student — required before booking (see below).

This project is aligned with a modern **.NET + Angular** profile (REST, institutional Google sign-in, PostgreSQL, Docker) similar to expectations in external postings such as [Senior Full Stack Developer (.NET Core & Angular)](https://talent.latinolegends.com/jobs/7030828-senior-full-stack-developer-net-core-angular).

## Roles

| Role | How it is assigned | In the app |
|------|--------------------|------------|
| **Parent** | Default on first sign-in if the email is not in bootstrap lists | Complete **meeting registration**, then book interviews and use **request teacher access** (`/app/parent/request-teacher`). |
| **Teacher** | Email listed in `TEACHER_BOOTSTRAP_EMAILS`, **or** Parent approved by a Director via teacher access request | Create offerings, set weekly availability, view bookings. |
| **Director** | Email listed in `DIRECTOR_BOOTSTRAP_EMAILS` on first sign-in | Approve/reject teacher requests, CRUD subjects, create offerings for any teacher, replace weekly availability for any offering. |

**Important:** after a Director **approves** a teacher access request, the user must **sign out and sign in again** so the JWT includes the Teacher role.

See [docs/flows-and-roles.md](docs/flows-and-roles.md) for step-by-step flows and section labels (e.g. same grade, groups A and C).

## Authentication

| Method | Flow |
|--------|------|
| **Google** | SPA loads Google Identity Services, receives an `id_token`, API validates it with `Google:WebClientId` / `GOOGLE_OAUTH_CLIENT_ID`, issues JWT. First-time users get a role from bootstrap lists (Teacher / Director) or default **Parent**. |
| **Email + password** | **Register**: `POST /api/v1/auth/register` with `email`, `password` (min 8 chars), `displayName`. **Login**: `POST /api/v1/auth/email-login` with `email`, `password`. Same domain rules as Google (`Auth__AllowedEmailDomains`, etc.). Passwords are stored with **BCrypt**. |

**Account linking:** if an email already has an email/password account, Google sign-in with that same email is rejected (clear error message) to avoid duplicate identities.

**Environments:** OpenAPI (`/openapi/v1.json`) is mapped in **Development** only. Demo user seeding runs only in **Development** (see below).

## Demo accounts (Development only)

When `ASPNETCORE_ENVIRONMENT` is **Development**, startup seeds three users **if their emails are not already present** (password for all: **`password`**):

| Email | Role |
|-------|------|
| `demo-parent@example.com` | Parent |
| `demo-teacher@example.com` | Teacher |
| `demo-director@example.com` | Director |

These accounts are **not** created in **Production** or in the **Testing** host used by integration tests. Do not deploy with `Development` if you rely on this sample in a public environment.

## Parent meeting registration

Parents authenticate with **their own** Google account. Before booking any slot they must save:

1. **Student school email** — institutional email identifying the child the interview is about (may differ from the parent’s Google email).
2. **Interview attendee name** — full name of the adult who will attend.
3. **Relationship to student** — e.g. mother, father, legal guardian.

These values are copied onto each **booking** so teachers see who will attend. The SPA route is `/app/parent/meeting-profile`; sign-in redirects there until the profile is complete.

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

On **Linux**, the Compose file sets `network: host` for **image builds** so `dotnet restore` / `npm ci` use the host network (helps avoid TLS issues to NuGet/npm through the default Docker bridge).

The API runs EF migrations on startup, seeds baseline **subjects** if the catalog is empty, and (in **Development** only) ensures [demo accounts](#demo-accounts-development-only) exist.

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
| `ASPNETCORE_ENVIRONMENT` | **Optional** in `.env`. Compose defaults the API container to **`Development`** (`${ASPNETCORE_ENVIRONMENT:-Development}`). You **do not** need to set this for local Docker unless you want **Production** (no demo seed, no OpenAPI). |
| `GOOGLE_OAUTH_CLIENT_ID` | Google OAuth **Web** client id: API validates tokens; frontend image receives it at **build** time. |
| `ALLOWED_EMAIL_DOMAINS` | Comma-separated allowed **email domains** (e.g. `northview.edu`). **Empty** = any Google-verified email allowed (and email/password sign-up subject to the same rule). |
| `ALLOW_PERSONAL_GOOGLE_EMAILS` | When domains are **non-empty**, set `true` to also allow `@gmail.com` / `@googlemail.com`. |
| `TEACHER_BOOTSTRAP_EMAILS` | Comma-separated emails that get **Teacher** on first Google sign-in or on email/password **register**. |
| `DIRECTOR_BOOTSTRAP_EMAILS` | Comma-separated emails that get **Director** on first Google sign-in or on email/password **register**. |

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
- **Booking**: parent + offering + UTC slot + snapshot of student email, attendee name, and relationship.
- **TeacherAccessRequest**: Parent asks for promotion; Director approves/rejects.

## API

Summary endpoints live in [docs/api-reference.md](docs/api-reference.md). Common paths:

- `POST /api/v1/auth/google` — exchange Google `id_token` for JWT.
- `POST /api/v1/auth/register` — create account with email + password + display name; returns JWT.
- `POST /api/v1/auth/email-login` — sign in with email + password; returns JWT.
- `GET /api/v1/auth/me` — current user profile (JWT required).
- `GET /api/v1/catalog/teacher-offerings` — bookable offerings (authenticated).
- `POST /api/v1/parent/bookings` — reserve a slot.
- `POST /api/v1/teacher-access-requests` — Parent submits teacher access request.
- `GET|POST /api/v1/director/...` — Director operations (requests, subjects, offerings, weekly availability).

## Further reading

| Document | Content |
|----------|---------|
| [docs/flows-and-roles.md](docs/flows-and-roles.md) | User journeys, section labels, post-approval re-login. |
| [docs/api-reference.md](docs/api-reference.md) | REST v1 paths and short descriptions. |
| [docs/testing.md](docs/testing.md) | How to run unit, integration, and e2e tests locally; what CI runs. |
| [frontend/README.md](frontend/README.md) | Optional local `npm start`, proxy, and frontend test commands. |

## Testing

See [docs/testing.md](docs/testing.md). Summary: **`dotnet test`** on `InterviewScheduling.Api.Tests` (in-memory integration + validation unit tests), **`npm run build`** + **`npm run test:e2e`** (Playwright smoke on the built SPA).

## Stack & CI

- **Backend**: ASP.NET Core 10, EF Core, PostgreSQL (Npgsql), Google ID token validation, JWT bearer, BCrypt for local passwords.
- **Frontend**: Angular (standalone, lazy routes), Google Identity Services, Syne + Outfit (Google Fonts) on the sign-in experience.
- **CI**: [`.github/workflows/build.yml`](.github/workflows/build.yml) on **push** to **`main`**: `dotnet test` (backend test project), `npm ci`, `npm run build`, Playwright browser install, **`npm run test:e2e`**. Karma is not run in CI; use `npm test` locally when changing Angular services or components.

## License

Portfolio sample code; use and modify freely for demonstrations.
