# Repository history — 2026-04-30 (UTC day)

Summary of commits authored on **2026-04-30** in this repo (`git log --since=2026-04-30 --until=2026-05-01`). Merge commits are included for traceability.

| Commit | Message |
|--------|---------|
| `b2de7ee` | Merge pull request #8 from waldopanozo/FE-002 |
| `cc5d670` | feat(seed): enhance demo user seeding logic and documentation |
| `1074a76` | Merge pull request #7 from waldopanozo/FE-001 |
| `a483967` | feat(auth): implement email/password registration and login functionality |
| `f7590eb` | fix(test): restore DI scopes and align EF packages for CI |
| `092da44` | Merge pull request #6 from waldopanozo/dev |
| `38c7440` | Merge pull request #5 from waldopanozo/feat/parent-meeting-profile-tests-ci |
| `3d17f69` | feat(mvp): parent meeting profile, booking snapshots, tests, and CI |
| `cbdec98` | Merge pull request #4 from waldopanozo/feat/director-onboarding-docs-ci |
| `e957491` | chore(ci): update build workflow to run only on push to main branch |
| `aaab0d5` | feat(mvp): director workflows, teacher onboarding, and documentation |
| `4ed5e2b` | Merge pull request #3 from waldopanozo/feat/auth-gmail-and-domain-options |
| `05ee712` | feat(auth): allow Gmail alongside school domains for Google sign-in |
| `42bd602` | docs(README): update project description to clarify it's a demonstration project |
| `b54b283` | Merge pull request #2 from waldopanozo/feat/docker-full-stack-and-ui-port |
| `2226146` | feat(infra): run full stack in Docker and expose UI on host port 3456 |
| `327a57d` | Merge pull request #1 from waldopanozo/feat/parent-teacher-interview-mvp |
| `3ec2758` | Merge pull request #1 from waldopanozo/feat/parent-teacher-interview-mvp |
| `ff0d21b` | chore(ci): update build workflow to target main branch |
| `b46815b` | chore(ci): target dev branch for PRs and Actions |
| `ad0acb4` | feat(mvp): deliver parent–teacher interview scheduling stack |
| `c3b73ab` | Add .gitignore file to exclude environment and build artifacts |

## Themes in those changes (high level)

- **MVP stack**: .NET API, JWT + Google ID token validation, EF Core + PostgreSQL, Angular SPA, Docker Compose, GitHub Actions.
- **Roles & flows**: Parent / Teacher / Director; meeting registration before booking; teacher access requests; director subject/offering/availability management.
- **Auth expansion**: Institutional vs Gmail domain options; **email + password** registration and login with BCrypt; demo user seeding in Development.
- **Quality**: Integration tests, Playwright smoke e2e, CI on `main`.

Later commits (e.g. **2026-05-02**) added booking cancellation, school settings, and UI language support in history; see `git log` for the full timeline.

## 2026 workspace additions (not necessarily committed same day)

Documented in the main [README](../README.md) and [api-reference](api-reference.md):

- **School branding**: director-configurable **color presets**, **custom logo** + default SVG; public `school-config` and `school-logo` endpoints.
- **SPA shell**: sidebar navigation, top bar, bento-style dashboard cards; redesigned sign-in and **forgot password** help page.
- **Password**: `hasPasswordLogin` on profile; **PUT `/api/v1/auth/password`** for signed-in email/password users; no email-based reset in this demo.
- **Developer workflow**: Documented that **Docker Compose** reads a default **`.env`** next to `docker-compose.yml` (no need for `--env-file .env` unless the file lives elsewhere). **Playwright** loads **`dotenv`** in `playwright.config.ts` from repo **`../.env`** and **`./.env`** in `frontend/` so **`E2E_STACK_URL`** can live in `.env` instead of the shell; see [testing.md](testing.md) quick reference and [README](../README.md) Testing / Environment variables.
- **Playwright coverage (full stack)**: Added `flows-*.spec.ts` for registration, parent book/cancel, teacher weekly offering, director subject add/delete, teacher-access approval, plus shared **`e2e/helpers.ts`** (school TZ from **`/catalog/school-config`**, book **Algebra I** demo slots). With **`E2E_STACK_URL`**, Playwright runs **`workers: 1`** to avoid clobbering shared demo data.
- **API fix**: `ListForTeacherAsync` now **`Include`s `TeacherOffering.Teacher`** so `GET /teacher/bookings` no longer throws when mapping to `BookingDto` (unblocks teacher dashboard and E2E).
