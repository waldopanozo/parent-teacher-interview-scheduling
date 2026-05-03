import { test, expect } from '@playwright/test';
import {
  demo,
  ensureParentProfileComplete,
  loginWithEmailPassword,
  logout,
  registerWithEmailPassword,
  skipWithoutFullStack
} from './helpers';

test.describe('Teacher access request → director approve (full stack)', () => {
  test.beforeEach(() => {
    skipWithoutFullStack();
  });

  test('parent promoted to teacher after director approves', async ({ page }) => {
    test.setTimeout(120_000);
    const id = Date.now();
    const email = `e2e.taccess.${id}@example.com`;
    const password = 'password123';

    await registerWithEmailPassword(page, `E2E Teacher Path ${id}`, email, password);
    await expect(page).toHaveURL(/\/app\/parent\/meeting-profile$/);

    await page.getByLabel(/student school email|correo escolar del estudiante/i).fill(`student.${id}@school.edu`);
    await page
      .getByLabel(/interview attendee full name|nombre completo del asistente/i)
      .fill('E2E Guardian');
    await page.getByLabel(/relationship to student|relación con el estudiante/i).fill('Father');
    await page.getByRole('button', { name: /save profile|guardar perfil/i }).click();
    await expect(page).toHaveURL(/\/app\/parent$/);

    await page.getByLabel('Navigation').getByRole('link', { name: /request teacher|solicitar rol/i }).click();
    await page.locator('app-parent-request-teacher textarea').fill(`Playwright E2E access request ${id}.`);
    await page.getByRole('button', { name: /Submit request|Enviar solicitud/i }).click();
    await expect(page.locator('app-parent-request-teacher .status')).toContainText(
      /Request submitted|Solicitud enviada/i
    );

    await logout(page);

    await loginWithEmailPassword(page, demo.director.email, demo.director.password);
    await expect(page).toHaveURL(/\/app\/director/);

    await page.getByRole('button', { name: /Refresh list|Actualizar lista/i }).click();
    const row = page.getByRole('row').filter({ hasText: email });
    await expect(row).toBeVisible({ timeout: 30_000 });
    await row.getByRole('button', { name: /Approve|Aprobar/i }).click();

    await logout(page);

    await loginWithEmailPassword(page, email, password);
    await expect(page).toHaveURL(/\/app\/teacher$/);
    await expect(page.getByRole('heading', { name: /teacher workspace|panel del profesor/i })).toBeVisible();
  });
});
