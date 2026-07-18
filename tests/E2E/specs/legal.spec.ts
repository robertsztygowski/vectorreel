import { test, expect } from '@playwright/test';

// The legal pack is a launch blocker: mdreel sells EU data residency to DPOs, so the six required
// pages must actually be live, name the operating entity, carry an effective date, and be reachable
// from the site footer. This spec asserts all of that in the real browser against the real web
// build — no mocks needed (the pages are static server components).

const ENTITY = 'Royalcode Robert Sztygowski';

const PAGES: { path: string; heading: RegExp }[] = [
  { path: '/legal/terms', heading: /Terms of Service/i },
  { path: '/legal/privacy', heading: /Privacy Policy/i },
  { path: '/legal/imprint', heading: /Imprint/i },
  { path: '/legal/dpa', heading: /Data Processing Agreement/i },
  { path: '/legal/subprocessors', heading: /Subprocessors/i },
  { path: '/legal/acceptable-use', heading: /Acceptable Use/i },
];

for (const { path, heading } of PAGES) {
  test(`legal page ${path} renders with heading, entity and effective date`, async ({ page }) => {
    const response = await page.goto(path);
    expect(response?.status()).toBe(200);
    await expect(page.getByRole('heading', { level: 1 })).toHaveText(heading);
    await expect(page.locator('body')).toContainText(ENTITY);
    // Every page carries the shared effective date / version meta line.
    await expect(page.locator('.legal-meta')).toContainText(/Effective/i);
    await expect(page.locator('time')).toHaveAttribute('datetime', /^\d{4}-\d{2}-\d{2}$/);
  });
}

test('footer trust column links to the real legal pages', async ({ page }) => {
  await page.goto('/');
  const trust = page.getByRole('navigation', { name: 'Trust' });
  await expect(trust.getByRole('link', { name: 'Terms of Service' })).toHaveAttribute('href', '/legal/terms');
  await expect(trust.getByRole('link', { name: 'Privacy Policy' })).toHaveAttribute('href', '/legal/privacy');
  await expect(trust.getByRole('link', { name: 'Subprocessors' })).toHaveAttribute('href', '/legal/subprocessors');

  // And the footer link actually navigates to a live page.
  await trust.getByRole('link', { name: 'Privacy Policy' }).click();
  await page.waitForURL(/\/legal\/privacy/);
  await expect(page.getByRole('heading', { level: 1 })).toHaveText(/Privacy Policy/i);
});

test('signup shows the B2B agree-notice linking Terms and Privacy', async ({ page }) => {
  await page.goto('/signup');
  const agree = page.locator('.auth-agree');
  await expect(agree).toContainText(/Terms of Service/i);
  await expect(agree).toContainText(/Privacy Policy/i);
  await expect(agree.getByRole('link', { name: 'Terms of Service' })).toHaveAttribute('href', '/legal/terms');
});
