import { test, expect } from '@playwright/test';

test('login page loads and shows Google sign-in area', async ({ page }) => {
  await page.goto('/login');
  await expect(page).toHaveTitle(/Parent.?Teacher Interview Scheduling/i);
  await expect(page.locator('app-login')).toBeVisible();
});
