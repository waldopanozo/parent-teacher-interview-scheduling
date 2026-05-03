import { test, expect } from '@playwright/test';
import { demo, loginWithEmailPassword, logout, skipWithoutFullStack } from './helpers';

test.describe('Director subject CRUD (full stack)', () => {
  test.beforeEach(() => {
    skipWithoutFullStack();
  });

  test('adds a subject then deletes it', async ({ page }) => {
    const code = `E2E${Date.now().toString(36).toUpperCase().slice(-6)}`;
    await loginWithEmailPassword(page, demo.director.email, demo.director.password);
    await expect(page).toHaveURL(/\/app\/director/);

    const subjectsCard = page.locator('app-director-dashboard .card').nth(1);
    await subjectsCard.getByPlaceholder('e.g. CS101').fill(code);
    await subjectsCard.getByPlaceholder('e.g. Computing').fill(`E2E Subject ${code}`);
    await subjectsCard.getByRole('button', { name: /Add subject|Añadir materia/i }).click();

    await expect(page.getByRole('cell', { name: code, exact: true })).toBeVisible({ timeout: 15_000 });

    page.once('dialog', (d) => d.accept());
    const row = page.getByRole('row').filter({ hasText: code });
    await row.getByRole('button', { name: /^Delete$|^Eliminar$/i }).click();

    await expect(page.getByRole('cell', { name: code, exact: true })).toHaveCount(0);

    await logout(page);
  });
});
