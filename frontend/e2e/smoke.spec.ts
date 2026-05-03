import { test, expect } from '@playwright/test';

test('login page loads and shows sign-in UI', async ({ page }) => {
  await page.goto('/login');
  await expect(page).toHaveTitle(/Parent.?Teacher Interview Scheduling/i);
  await expect(page.locator('app-login')).toBeVisible();
  await expect(page.locator('app-login form.auth-form button.btn-primary')).toBeVisible();
  await expect(page.getByRole('link', { name: /forgot password|olvidaste/i })).toBeVisible();
});

test('forgot-password help page loads without full API', async ({ page }) => {
  await page.goto('/forgot-password');
  await expect(page.locator('app-forgot-password')).toBeVisible();
  await expect(page.getByRole('heading', { name: /password help|ayuda con la contraseña/i })).toBeVisible();
  await expect(page.getByRole('link', { name: /back to sign in|volver al inicio/i })).toBeVisible();
});
