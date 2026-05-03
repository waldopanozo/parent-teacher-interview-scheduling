# Convenience targets (repo root). Requires .NET SDK, Node/npm, and optionally Docker for e2e stack tests.

.PHONY: backend-test frontend-build frontend-e2e frontend-e2e-stack

backend-test:
	cd backend/InterviewScheduling.Api.Tests && dotnet test

frontend-build:
	cd frontend && npm run build

frontend-e2e:
	cd frontend && npm run test:e2e

frontend-e2e-stack: frontend-build
	cd frontend && npm run test:e2e
