import { test, expect } from '@playwright/test';
import {
  bookFirstDemoTeacherSlot,
  demo,
  ensureParentProfileComplete,
  loginWithEmailPassword,
  logout,
  skipWithoutFullStack
} from './helpers';

test.describe.serial('Demo parent booking journeys (full stack)', () => {
  test.beforeEach(() => {
    skipWithoutFullStack();
  });

  test('parent reserves a slot on a future weekday then cancels it', async ({ page }) => {
    await loginWithEmailPassword(page, demo.parent.email, demo.parent.password);
    await ensureParentProfileComplete(page);

    await bookFirstDemoTeacherSlot(page);
    await expect(page.locator('app-parent-dashboard tbody tr').first()).toBeVisible({ timeout: 15_000 });

    page.once('dialog', (d) => d.accept());
    await page.getByRole('button', { name: /^Cancel$|^Cancelar$/i }).first().click();

    await expect(page.getByText(/No bookings yet|Aún no hay reservas/i)).toBeVisible({ timeout: 15_000 });
  });

  test('teacher saves attendance on parent booking then parent cancels', async ({ page }) => {
    await loginWithEmailPassword(page, demo.parent.email, demo.parent.password);
    await ensureParentProfileComplete(page);

    await bookFirstDemoTeacherSlot(page);
    await expect(page.locator('app-parent-dashboard tbody tr').first()).toBeVisible({ timeout: 15_000 });

    await logout(page);

    await loginWithEmailPassword(page, demo.teacher.email, demo.teacher.password);
    await expect(page.locator('app-teacher-dashboard tbody tr')).not.toHaveCount(0, { timeout: 25_000 });
    const row = page.locator('app-teacher-dashboard tbody tr').filter({ hasText: /demo-parent@example\.com/i });
    await expect(row.first()).toBeVisible({ timeout: 5_000 });
    const targetRow = row.first();
    await targetRow.locator('select').first().selectOption({ index: 1 });
    await targetRow.getByRole('button', { name: /^Save$|^Guardar$/i }).click();
    await expect(page.locator('app-teacher-dashboard p.status')).toHaveCount(0);

    await logout(page);

    await loginWithEmailPassword(page, demo.parent.email, demo.parent.password);
    await expect(page.locator('app-parent-dashboard tbody tr').first()).toContainText(/Attended|Asistió/i);

    page.once('dialog', (d) => d.accept());
    await page.getByRole('button', { name: /^Cancel$|^Cancelar$/i }).first().click();
    await expect(page.getByText(/No bookings yet|Aún no hay reservas/i)).toBeVisible({ timeout: 15_000 });
  });
});
