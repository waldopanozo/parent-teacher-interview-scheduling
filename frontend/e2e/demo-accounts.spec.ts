import { test, expect } from '@playwright/test';
import {
  demo,
  ensureParentProfileComplete,
  loginWithEmailPassword,
  skipWithoutFullStack
} from './helpers';

test.describe('Docker demo accounts (full stack)', () => {
  test.beforeEach(() => {
    skipWithoutFullStack();
  });

  test('parent: meeting profile, dashboard, request-teacher page', async ({ page }) => {
    await loginWithEmailPassword(page, demo.parent.email, demo.parent.password);
    await expect(page).toHaveURL(/\/app\/parent(\/meeting-profile)?/);
    await ensureParentProfileComplete(page);

    await expect(
      page.getByRole('heading', { name: /parent workspace|panel de padres/i })
    ).toBeVisible();
    await page
      .getByLabel('Navigation')
      .getByRole('link', { name: /request teacher access|solicitar rol de profesor/i })
      .click();
    await expect(
      page.getByRole('heading', { name: /request teacher access|solicitar acceso/i })
    ).toBeVisible();
  });

  test('parent: account change password route', async ({ page }) => {
    await loginWithEmailPassword(page, demo.parent.email, demo.parent.password);
    await ensureParentProfileComplete(page);

    await page.getByRole('link', { name: /change password|cambiar contraseña/i }).click();
    await expect(page).toHaveURL(/\/app\/account\/password/);
    await expect(
      page.getByRole('heading', { name: /change password|cambiar contraseña/i })
    ).toBeVisible();
  });

  test('teacher: workspace visible', async ({ page }) => {
    await loginWithEmailPassword(page, demo.teacher.email, demo.teacher.password);
    await expect(page).toHaveURL(/\/app\/teacher/);
    await expect(
      page.getByRole('heading', { name: /teacher workspace|panel del profesor/i })
    ).toBeVisible();
  });

  test('director: workspace and school settings card', async ({ page }) => {
    await loginWithEmailPassword(page, demo.director.email, demo.director.password);
    await expect(page).toHaveURL(/\/app\/director/);
    await expect(
      page.getByRole('heading', { name: /director workspace|panel del director/i })
    ).toBeVisible();
    await expect(
      page.getByRole('heading', { name: /school time zone|zona horaria del colegio/i })
    ).toBeVisible();
  });
});
