import { test, expect } from '@playwright/test';

// Contract-derived collection pages (ARCHITECTURE.md §4b v1.1), rendered from the canonical
// repository fixture. The E2E stack builds with NEXT_PUBLIC_SHOW_COLLECTIONS=true so these ship
// fully exercised even though production has the flag OFF (docker-compose.yml, web/Dockerfile).
//
// The assertions that matter are about the TIER BOUNDARY. A collection that blurs `full` and
// `reference` is worse than one that publishes less: `full` is a near-complete derivative of a
// CC-BY talk, `reference` is a citation of material we may not reproduce. If these tests ever go
// red, do not loosen them — that is the licence discipline failing in the product surface.

const COLLECTION = '/collections/repository';

test('collections index lists the collection and its two tiers', async ({ page }) => {
  await page.goto('/collections');
  await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
  await page.getByRole('link', { name: /canonical AI-ready repository fixture/i }).first().click();
  await expect(page).toHaveURL(new RegExp(COLLECTION));
});

test('the collection page renders both tiers and labels every entry', async ({ page }) => {
  await page.goto(COLLECTION);

  const badges = page.getByTestId('tier-badge');
  await expect(badges.first()).toBeVisible();

  const sessionBadges = page.locator('[data-tier="full"]');
  const referenceBadges = page.locator('[data-tier="reference"]');
  expect(await sessionBadges.count()).toBeGreaterThan(0);
  expect(await referenceBadges.count()).toBeGreaterThan(0);

  // Every entry carries a visible tier marker — no unlabelled middle ground.
  expect(await badges.count()).toBe((await sessionBadges.count()) + (await referenceBadges.count()));
});

test('a reference entry links out to the original and never to an mdreel session page', async ({ page }) => {
  await page.goto(COLLECTION);

  const reference = page.locator('[data-tier="reference"]').first();
  const links = reference.getByRole('link');
  const count = await links.count();
  expect(count).toBeGreaterThan(0);

  for (let i = 0; i < count; i += 1) {
    const href = await links.nth(i).getAttribute('href');
    expect(href, 'reference links point at the original video').toMatch(/^https?:\/\//);
    expect(href, 'a reference entry must never link to an mdreel session page').not.toContain('/sessions/');
  }

  // And its deep links carry a timestamp, which is what makes a citation checkable.
  const deepLink = await reference.locator('a[href*="t="]').first().getAttribute('href');
  expect(deepLink).toMatch(/t=\d+s/);
});

test('a full session page renders timestamped sections and the licence', async ({ page }) => {
  await page.goto(COLLECTION);
  await page.locator('[data-tier="full"]').first().getByRole('link').first().click();

  await expect(page).toHaveURL(/\/collections\/repository\/sessions\//);
  await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
  await expect(page.getByRole('link', { name: /open the original video/i })).toBeVisible();
  await expect(page.getByRole('heading', { name: /source & licence/i })).toBeVisible();

  // Sections are timestamp-linked into the source: this is the "checkable answers" promise, in the
  // one place a reader can act on it.
  const timestampLink = page.locator('a[href*="t="]').first();
  await expect(timestampLink).toBeVisible();
  await expect(timestampLink).toHaveText(/\[\d{2}:\d{2}:\d{2}\]/);
});

test('a reference slug has no session page — it 404s rather than faking a thin one', async ({ page }) => {
  const response = await page.goto('/collections/repository/sessions/2025-10-20-exploring-the-cpython-jit');
  expect(response?.status()).toBe(404);
});

test('topic, speaker and timeline indexes all resolve', async ({ page }) => {
  await page.goto(COLLECTION);

  await page.getByRole('link', { name: 'Timeline' }).click();
  await expect(page.getByRole('heading', { name: 'Timeline' })).toBeVisible();
  await expect(page.getByTestId('tier-badge').first()).toBeVisible();

  await page.goto(`${COLLECTION}/topics/rust`);
  await expect(page.getByRole('heading', { level: 1, name: /rust/i })).toBeVisible();

  await page.goto(`${COLLECTION}/speakers/jon-gjengset`);
  await expect(page.getByRole('heading', { level: 1, name: /jon gjengset/i })).toBeVisible();
});

test('the convert CTA is present on collection surfaces', async ({ page }) => {
  await page.goto(COLLECTION);
  const cta = page.getByRole('link', { name: /build your own repository/i });
  await expect(cta.first()).toBeVisible();
  await expect(cta.first()).toHaveAttribute('href', '/signup');
});
