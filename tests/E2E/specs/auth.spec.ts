import { test, expect } from '@playwright/test';
import { pgQuery, uniqueEmail } from '../helpers';

// The auth funnel through the real browser and the real API (ASP.NET Core Identity, cookie mode):
// register → provisions tenant + N33 trial credit → sign out → sign back in. Every step is
// asserted in the UI and (for provisioning) as a row in Postgres, the METRICS.md §6.2 source of
// truth. Auth is same-origin via the Next.js rewrites proxy, so the Identity cookie is first-party.

const PASSWORD = 'Sup3rSecret!pw';

test('auth: register → workspace → sign out → sign back in', async ({ page }) => {
  const email = uniqueEmail('auth');

  // Register.
  await page.goto('/signup');
  await page.fill('#email', email);
  await page.fill('#password', PASSWORD);
  await page.getByRole('button', { name: 'create account' }).click();
  await page.waitForURL(/\/app/);

  // Registration provisioned the tenant with the N33 trial credit (METRICS.md N33).
  await expect
    .poll(async () => {
      const rows = await pgQuery<{ trial_credit_hours: number }>(
        'select trial_credit_hours from tenants where email = $1',
        [email],
      );
      return rows[0]?.trial_credit_hours ?? null;
    })
    .toBeGreaterThan(0);

  // Sign out returns to the marketing site.
  await page.getByRole('button', { name: 'sign out' }).click();
  await page.waitForURL(/\/$|\/#/);

  // Sign back in with the same credentials.
  await page.goto('/signin');
  await page.fill('#email', email);
  await page.fill('#password', PASSWORD);
  await page.getByRole('button', { name: 'sign in', exact: true }).click();
  await page.waitForURL(/\/app/);

  // Wrong password is rejected without leaving the sign-in page.
  await page.getByRole('button', { name: 'sign out' }).click();
  await page.waitForURL(/\/$|\/#/);
  await page.goto('/signin');
  await page.fill('#email', email);
  await page.fill('#password', 'wrong-password');
  await page.getByRole('button', { name: 'sign in', exact: true }).click();
  await expect(page.locator('.auth-error')).toContainText(/wrong email or password/i);
});
