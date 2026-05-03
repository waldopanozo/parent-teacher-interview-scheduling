import fs from 'node:fs';
import path from 'node:path';
import { test, expect } from '@playwright/test';
import { E2E_UI_LANG_STORAGE_KEY } from '../../src/app/e2e-ui-lang.constants';
import {
  demo,
  ensureParentProfileComplete,
  fullStack,
  loginWithEmailPassword,
  logout
} from '../helpers';

const assetsDir = path.resolve(process.cwd(), '../docs/showcase/assets');

function ensureAssetsDir(): void {
  fs.mkdirSync(assetsDir, { recursive: true });
}

test.describe('Showcase capture (screenshots + one video)', () => {
  test('walkthrough for static gallery', async ({ page }) => {
    ensureAssetsDir();

    await page.addInitScript((key) => {
      localStorage.setItem(key, 'en');
    }, E2E_UI_LANG_STORAGE_KEY);

    await page.goto('/login');
    await expect(page.locator('app-login')).toBeVisible({ timeout: 30_000 });
    await page.screenshot({ path: path.join(assetsDir, '01-login.png'), fullPage: true });

    await page.goto('/forgot-password');
    await expect(page.locator('app-forgot-password')).toBeVisible();
    await page.screenshot({ path: path.join(assetsDir, '02-forgot-password.png'), fullPage: true });

    if (fullStack) {
      await loginWithEmailPassword(page, demo.parent.email, demo.parent.password);
      await ensureParentProfileComplete(page);
      await expect(page).toHaveURL(/\/app\/parent(\/meeting-profile)?$/);
      await page.goto('/app/parent');
      await expect(page.getByRole('heading', { name: /parent workspace/i })).toBeVisible({
        timeout: 30_000
      });
      await page.screenshot({ path: path.join(assetsDir, '03-parent-dashboard.png'), fullPage: true });

      await logout(page);
      await loginWithEmailPassword(page, demo.teacher.email, demo.teacher.password);
      await expect(page).toHaveURL(/\/app\/teacher/);
      await expect(page.getByRole('heading', { name: /teacher workspace/i })).toBeVisible({
        timeout: 30_000
      });
      await page.screenshot({ path: path.join(assetsDir, '04-teacher-dashboard.png'), fullPage: true });
    }

    // Chromium only finalizes WebM after the page closes; saveAs() would deadlock if awaited first.
    const vid = page.video();
    await page.close();
    if (vid) await vid.saveAs(path.join(assetsDir, 'walkthrough.webm'));
  });
});
