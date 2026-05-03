# Convenience targets (repo root). Requires .NET SDK, Node/npm, and optionally Docker for e2e stack tests.

.PHONY: backend-test frontend-build frontend-e2e frontend-e2e-stack frontend-showcase

backend-test:
	cd backend/InterviewScheduling.Api.Tests && dotnet test

frontend-build:
	cd frontend && npm run build

frontend-e2e:
	cd frontend && npm run test:e2e

frontend-e2e-stack: frontend-build
	cd frontend && npm run test:e2e

# Screenshots + walkthrough.webm under docs/showcase/assets (smoke without Docker; full set with E2E_STACK_URL in .env). Optional: PW_CHANNEL=chrome.
frontend-showcase: frontend-build
	cd frontend && npm run capture:showcase
