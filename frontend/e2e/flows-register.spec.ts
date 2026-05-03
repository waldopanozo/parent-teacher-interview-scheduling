import { test, expect } from '@playwright/test';
import { registerWithEmailPassword, skipWithoutFullStack } from './helpers';

test.describe('Registration (full stack)', () => {
  test.beforeEach(() => {
    skipWithoutFullStack();
  });

  test('new email/password user is redirected to meeting registration', async ({ page }) => {
    const id = Date.now();
    const email = `e2e.reg.${id}@example.com`;
    await registerWithEmailPassword(page, `E2E User ${id}`, email, 'password123');
    await expect(page).toHaveURL(/\/app\/parent\/meeting-profile$/);
    await expect(
      page.getByRole('heading', { name: /Meeting registration|Registro de la reunión/i })
    ).toBeVisible();
  });
});
