import { test, expect } from '@playwright/test';
import { SAMPLE_VIDEO, pgQuery, uniqueEmail } from '../helpers';

// The real browser through the real funnel: landing → signup (with the N20 volume fields) →
// upload → job progress → output download. The web build posts first-party events to the real
// API (NEXT_PUBLIC_API_BASE baked into the e2e image), so every step is asserted twice — once in
// the UI and once as rows in Postgres, the METRICS.md §6.2 source of truth.
//
// Honest scope note: the panel's upload → progress → output path runs against the web app's own
// mock /api/jobs routes (that is what is wired in the product today — Phase 2R). The same journey
// against the real API is covered request-level in api-funnel.spec.ts; when Phase 3 wires the
// panel to the real API this spec starts exercising it without changes.

test('funnel: landing → signup (N20 fields) → upload → progress → output download', async ({ page }) => {
  const email = uniqueEmail('funnel');

  // Landing. page_view is fired via the API (first-party, cookieless — CLAUDE.md rule 10).
  await page.goto('/?utm_source=e2e&utm_medium=test&utm_campaign=harness');
  await expect(page.locator('body')).toContainText(/markdown/i);
  const sessionId = await page.evaluate(() => window.sessionStorage.getItem('mdreel_session_id'));
  expect(sessionId).toBeTruthy();

  // Signup with the volume questions answered (METRICS.md N20). Email + password (Identity).
  await page.goto('/signup');
  await page.fill('#email', email);
  await page.fill('#password', 'Sup3rSecret!pw');
  await page.getByPlaceholder('~ h in the archive').fill('12');
  await page.getByPlaceholder('~ h added per month').fill('4');
  await page.getByRole('button', { name: 'create account' }).click();

  // Registration provisions the tenant and drops the browser straight into the workspace.
  await page.waitForURL(/\/app/);

  // The signup response handed the browser its tenant — first-touch attribution is now in Postgres.
  await expect
    .poll(async () => {
      const rows = await pgQuery<{ first_utm_source: string | null; trial_credit_hours: number }>(
        'select first_utm_source, trial_credit_hours from tenants where email = $1',
        [email],
      );
      return rows[0]?.first_utm_source ?? null;
    })
    .toBe('e2e');

  const tenantRows = await pgQuery<{ archive_hours: number | null; monthly_hours: number | null }>(
    'select archive_hours, monthly_hours from tenants where email = $1',
    [email],
  );
  expect(tenantRows[0]?.archive_hours).toBe(12);
  expect(tenantRows[0]?.monthly_hours).toBe(4);

  // Into the workspace: upload the committed CC clip.
  await page.goto('/app/upload');
  await page.setInputFiles('input[type="file"]', SAMPLE_VIDEO);
  await expect(page.locator('.file-row')).toContainText('talking_head_nasa_bolten.mp4');
  await page.getByRole('button', { name: 'start processing', exact: true }).click();

  // Job progress page: stages advance to done.
  await page.waitForURL(/\/app\/jobs\//);
  await expect(page.locator('.badge-done')).toBeVisible({ timeout: 30_000 });
  await expect(page.getByText('✓ source video deleted after processing')).toBeVisible();

  // Download the Markdown and check the bytes are the real document.
  const downloadPromise = page.waitForEvent('download');
  await page.getByRole('button', { name: 'download .md' }).click();
  const download = await downloadPromise;
  expect(download.suggestedFilename()).toBe('talking_head_nasa_bolten.md');
  const stream = await download.createReadStream();
  const chunks: Buffer[] = [];
  for await (const chunk of stream) chunks.push(chunk as Buffer);
  const markdown = Buffer.concat(chunks).toString('utf-8');
  expect(markdown.startsWith('---\n')).toBe(true);
  expect(markdown).toContain('## [');

  // Every funnel event from this browser session is a queryable row (METRICS.md §3 schema).
  await expect
    .poll(async () => {
      const rows = await pgQuery<{ name: string }>(
        'select distinct name from events where session_id = $1',
        [sessionId],
      );
      return rows.map((r) => r.name).sort();
    })
    .toEqual(expect.arrayContaining(['job_completed', 'output_downloaded', 'page_view', 'signup', 'upload_started']));
});
