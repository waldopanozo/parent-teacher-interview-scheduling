import { expect, test, type Page } from '@playwright/test';

/** True when `playwright.config.ts` loaded `E2E_STACK_URL` from env / `.env`. */
export const fullStack = !!process.env['E2E_STACK_URL']?.trim();

export function skipWithoutFullStack(): void {
  test.skip(
    !fullStack,
    'Set E2E_STACK_URL in .env (repo root or frontend/) with docker compose up.'
  );
}

export async function loginWithEmailPassword(page: Page, email: string, password: string): Promise<void> {
  await page.goto('/login');
  await page.locator('.auth-tabs__btn').nth(0).click();
  await page.getByLabel(/email|correo/i).first().fill(email);
  await page.locator('app-login input[type="password"]').first().fill(password);
  await page.locator('app-login form.auth-form button.btn-primary').click();
}

export async function registerWithEmailPassword(
  page: Page,
  displayName: string,
  email: string,
  password: string
): Promise<void> {
  await page.goto('/login');
  await page.locator('.auth-tabs__btn').nth(1).click();
  await page.getByLabel(/Display name|Nombre visible/i).fill(displayName);
  await page.getByLabel(/email|correo/i).fill(email);
  await page.locator('app-login input[type="password"]').fill(password);
  await page.locator('app-login form.auth-form button.btn-primary').click();
}

export async function logout(page: Page): Promise<void> {
  await page.getByRole('button', { name: /Sign out|Cerrar sesión/i }).click();
  await expect(page).toHaveURL(/\/login$/);
}

export async function ensureParentProfileComplete(page: Page): Promise<void> {
  if (!page.url().includes('meeting-profile')) return;
  await page.getByLabel(/student school email|correo escolar del estudiante/i).fill('student.demo@school.edu');
  await page
    .getByLabel(/interview attendee full name|nombre completo del asistente/i)
    .fill('Demo Guardian');
  await page.getByLabel(/relationship to student|relación con el estudiante/i).fill('Mother');
  await page.getByRole('button', { name: /save profile|guardar perfil/i }).click();
  await expect(page).toHaveURL(/\/app\/parent$/);
}

/**
 * YYYY-MM-DD for a Mon–Fri at least `minDaysFromToday` calendar days ahead in `timeZoneId`
 * (matches server slot generation for `interviewDate`). Default matches demo `Scheduling:SchoolTimeZoneId`.
 */
export function weekdayYmdAtLeastDaysAhead(
  minDaysFromToday: number,
  timeZoneId = process.env['E2E_SCHOOL_TZ']?.trim() || 'America/New_York'
): string {
  for (let add = minDaysFromToday; add <= minDaysFromToday + 28; add++) {
    const instant = new Date(Date.now() + add * 86_400_000);
    const wd = new Intl.DateTimeFormat('en-US', { timeZone: timeZoneId, weekday: 'short' }).format(instant);
    if (['Mon', 'Tue', 'Wed', 'Thu', 'Fri'].includes(wd)) {
      return new Intl.DateTimeFormat('en-CA', {
        timeZone: timeZoneId,
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      }).format(instant);
    }
  }
  throw new Error('No weekday in range');
}

/** Select first `<option>` whose text matches `predicate` (skips placeholder at index 0). */
export async function selectOfferingOptionByText(page: Page, predicate: RegExp): Promise<void> {
  const sel = page.locator('app-parent-dashboard .card').first().locator('select').first();
  await expect(sel.locator('option')).not.toHaveCount(0, { timeout: 30_000 });
  const labels = await sel.locator('option').allInnerTexts();
  const idx = labels.findIndex((t) => predicate.test(t));
  if (idx <= 0) throw new Error(`No offering option matching ${predicate}: ${JSON.stringify(labels)}`);
  await sel.selectOption({ index: idx });
}

async function schoolTimeZoneFromApi(page: Page): Promise<string> {
  const envTz = process.env['E2E_SCHOOL_TZ']?.trim();
  if (envTz) return envTz;
  const res = await page.request.get('/api/v1/catalog/school-config');
  const j = (await res.json()) as { schoolTimeZoneId?: string };
  return (j.schoolTimeZoneId ?? 'America/New_York').trim();
}

/**
 * Picks the seeded **Algebra I** demo offering (not other courses by the same teacher) and the next
 * school-local weekday with open slots, then clicks Reserve.
 */
export async function bookFirstDemoTeacherSlot(page: Page): Promise<void> {
  const tz = await schoolTimeZoneFromApi(page);
  await selectOfferingOptionByText(page, /Algebra I/i);
  const dateInput = page.locator('app-parent-dashboard input[type="date"]').first();
  const refresh = page.getByRole('button', { name: /Refresh slots|Actualizar franjas/i });

  for (let add = 2; add <= 28; add++) {
    const instant = new Date(Date.now() + add * 86_400_000);
    const wdShort = new Intl.DateTimeFormat('en-US', { timeZone: tz, weekday: 'short' }).format(instant);
    if (!['Mon', 'Tue', 'Wed', 'Thu', 'Fri'].includes(wdShort)) continue;
    const ymd = new Intl.DateTimeFormat('en-CA', {
      timeZone: tz,
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    }).format(instant);
    await dateInput.fill(ymd);
    await refresh.click();
    const reserveBtn = page.locator('app-parent-dashboard div.slots button.btn').filter({ hasText: /Reserve|Reservar/i });
    try {
      await expect(reserveBtn.first()).toBeVisible({ timeout: 5000 });
      await reserveBtn.first().click();
      return;
    } catch {
      /* try next day */
    }
  }

  throw new Error(`No open slots for Algebra I demo offering (school TZ ${tz}).`);
}

export const demo = {
  parent: { email: 'demo-parent@example.com', password: 'password' },
  teacher: { email: 'demo-teacher@example.com', password: 'password' },
  director: { email: 'demo-director@example.com', password: 'password' }
} as const;
