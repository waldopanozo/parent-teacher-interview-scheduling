# Frontend (Angular)

The supported way to run the full stack (UI + API + database) is **Docker Compose** from the **repository root**. See the root [`README.md`](../README.md) (`Run everything with Docker`).

Optional local tooling (requires Node.js): `npm install`, `npm start` (uses `proxy.conf.json` to the API). Use only if you are actively changing frontend code without rebuilding the container.

Routes are lazy-loaded; notable paths include `/app/parent/meeting-profile` (student + attendee + relationship), `/app/parent`, `/app/parent/request-teacher`, `/app/teacher`, and `/app/director` (JWT role guards).

**Tests:** `npm test` runs Karma/Jasmine (local browser). `npm run build` then `npm run test:e2e` runs Playwright: by default **smoke** tests against the static build (`serve -s` on port 4179). Use **`npm run test:e2e:stack`** for **build + all Playwright specs** in one command (point `E2E_STACK_URL` at Docker nginx, e.g. `http://127.0.0.1:3456`, in **`.env` at repo root** or **`frontend/.env`**). CI on `main` runs backend `dotnet test` and frontend build + Playwright only — see [docs/testing.md](../docs/testing.md). From the repo root you can also run **`make frontend-e2e-stack`** after `npm install` in `frontend/`.

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) 19.2.25.
