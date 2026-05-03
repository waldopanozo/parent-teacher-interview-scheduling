import path from 'node:path';
import { config as loadEnv } from 'dotenv';
import { defineConfig } from '@playwright/test';

// Repo root `.env` then `frontend/.env` (latter overrides) so E2E_STACK_URL need not be on the shell.
loadEnv({ path: path.resolve(process.cwd(), '..', '.env') });
loadEnv({ path: path.resolve(process.cwd(), '.env'), override: true });

const stackUrl = process.env['E2E_STACK_URL']?.trim();
const useDockerStack = !!stackUrl;

export default defineConfig({
  testDir: 'e2e',
  timeout: 90_000,
  forbidOnly: !!process.env['CI'],
  retries: 0,
  /** Shared demo DB: avoid parallel tests mutating the same parent/teacher rows. */
  ...(useDockerStack ? { workers: 1, fullyParallel: false } : {}),
  use: {
    baseURL: stackUrl ?? 'http://127.0.0.1:4179',
    trace: 'on-first-retry'
  },
  webServer: useDockerStack
    ? undefined
    : {
        command: 'npx serve dist/frontend/browser -l 4179 -s',
        url: 'http://127.0.0.1:4179',
        reuseExistingServer: !process.env['CI'],
        timeout: 120_000
      }
});
