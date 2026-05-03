import { test, expect } from '@playwright/test';
import { demo, loginWithEmailPassword, skipWithoutFullStack } from './helpers';

test.describe('Teacher creates offering and weekly window (full stack)', () => {
  test.beforeEach(() => {
    skipWithoutFullStack();
  });

  test('creates a new offering and publishes Tuesday availability', async ({ page }) => {
    const id = Date.now();
    await loginWithEmailPassword(page, demo.teacher.email, demo.teacher.password);
    await expect(page).toHaveURL(/\/app\/teacher/);

    const cards = page.locator('app-teacher-dashboard .card');
    const createCard = cards.nth(0);
    await createCard.locator('select').first().selectOption({ index: 1 });
    await createCard.locator('input[placeholder="e.g. Algebra II"]').fill(`E2E Course ${id}`);
    await createCard.locator('input[placeholder="e.g. 10"]').fill('11');
    await Promise.all([
      page.waitForResponse(
        (r) =>
          r.url().includes('/teacher/offerings') && r.request().method() === 'POST' && r.status() === 201
      ),
      createCard.getByRole('button', { name: /Save offering|Guardar oferta/i }).click()
    ]);

    const weekly = cards.nth(1);
    const offeringSelect = weekly.locator('select').first();
    await expect(offeringSelect).toContainText(`E2E Course ${id}`, { timeout: 15_000 });
    const offeringLabels = await offeringSelect.locator('option').allInnerTexts();
    const matchLabel = offeringLabels.find((t) => t.includes(`E2E Course ${id}`))?.trim();
    if (!matchLabel) throw new Error(`Offering "E2E Course ${id}" not found in select`);
    await offeringSelect.selectOption({ label: matchLabel });
    await weekly.locator('select').nth(1).selectOption('Tuesday');
    await weekly.locator('.row .field input').first().fill('10:00');
    await weekly.locator('.row .field input').nth(1).fill('11:00');
    await weekly.getByRole('button', { name: /Save weekly window|Guardar ventana semanal/i }).click();

    await expect(page.locator('app-teacher-dashboard p.status')).toHaveCount(0);
  });
});
