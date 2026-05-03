import path from 'node:path';
import { config as loadEnv } from 'dotenv';
import { defineConfig } from '@playwright/test';

loadEnv({ path: path.resolve(process.cwd(), '..', '.env') });
loadEnv({ path: path.resolve(process.cwd(), '.env'), override: true });

const rawStack = process.env['E2E_STACK_URL']?.trim() ?? '';
const stackUrl = rawStack.length > 0 ? rawStack : undefined;
const useDockerStack = !!stackUrl;
/** Set `PW_CHANNEL=chrome` to record with Google Chrome instead of bundled Chromium (Chrome must be installed). */
const useChromeChannel = process.env['PW_CHANNEL']?.trim().toLowerCase() === 'chrome';

export default defineConfig({
  testDir: 'e2e/showcase',
  timeout: 180_000,
  forbidOnly: !!process.env['CI'],
  retries: 0,
  workers: 1,
  fullyParallel: false,
  use: {
    baseURL: stackUrl ?? 'http://127.0.0.1:4179',
    locale: 'en-US',
    trace: 'off',
    video: { mode: 'on', size: { width: 1280, height: 720 } },
    ...(useChromeChannel ? { channel: 'chrome' as const } : {})
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
