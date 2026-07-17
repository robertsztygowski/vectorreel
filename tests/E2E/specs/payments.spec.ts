import { test, expect, request as playwrightRequest } from '@playwright/test';
import { API_BASE, pgQuery, uniqueEmail } from '../helpers';

// Payments funnel: signup → checkout (browser, real API) → Stripe webhook → rows in Postgres.
// Stripe never runs in the default loop: without STRIPE_SECRET_KEY the API uses the deterministic
// FakePaymentGateway (checkout URL https://checkout.test/…, webhook signature "test-signature").
// Real Stripe stays test-mode-only and out of this suite entirely (task constraint).

const FAKE_STRIPE_SIGNATURE = 'test-signature';

async function signupViaApi(email: string) {
  const api = await playwrightRequest.newContext({ baseURL: API_BASE });
  const res = await api.post('/api/v1/events', {
    data: {
      name: 'signup',
      session_id: `sess_${email}`,
      email,
      archive_hours: 12,
      monthly_hours: 4,
      utm_source: 'e2e',
      utm_medium: 'test',
      utm_campaign: 'harness',
      first_referrer: 'https://e2e.example',
      ab_arm: 'A',
    },
  });
  expect(res.status()).toBe(202);
  const body = await res.json();
  await api.dispose();
  return body as { tenant_id: string; user_id: string };
}

test('checkout → webhook → payment row with first-touch attribution', async () => {
  const email = uniqueEmail('payments');
  const signup = await signupViaApi(email);

  const api = await playwrightRequest.newContext({
    baseURL: API_BASE,
    extraHTTPHeaders: { Authorization: 'Bearer e2e-test-token' },
  });

  // Checkout session (fake gateway — deterministic).
  const checkoutRes = await api.post('/api/v1/checkout', {
    data: { plan: 'pro', tenant_id: signup.tenant_id },
  });
  expect(checkoutRes.status()).toBe(201);
  const checkout = await checkoutRes.json();
  expect(checkout.url).toContain('https://checkout.test/');

  // Stripe calls back. Signature-authenticated, no bearer token (ARCHITECTURE §5).
  const webhookRes = await api.post('/api/v1/webhooks/stripe', {
    headers: { 'Stripe-Signature': FAKE_STRIPE_SIGNATURE, 'Content-Type': 'application/json' },
    data: {
      type: 'checkout.session.completed',
      tenant_id: signup.tenant_id,
      plan: 'pro',
      sessionId: checkout.sessionId,
      amount_cents: 2900,
    },
  });
  expect(webhookRes.status()).toBe(200);

  // A forged signature must bounce.
  const forgedRes = await api.post('/api/v1/webhooks/stripe', {
    headers: { 'Stripe-Signature': 'not-stripe', 'Content-Type': 'application/json' },
    data: { type: 'checkout.session.completed', tenant_id: signup.tenant_id, plan: 'pro' },
  });
  expect(forgedRes.status()).toBe(400);

  // The CAC join (METRICS.md §6.3): payment row carries first-touch attribution copied at webhook
  // time, and the tenant's plan flipped — all queryable in Postgres.
  const payments = await pgQuery<{ plan: string; first_utm_source: string; first_referrer: string; amount_cents: number }>(
    'select plan, first_utm_source, first_referrer, amount_cents from payments where tenant_id = $1',
    [signup.tenant_id],
  );
  expect(payments).toHaveLength(1);
  expect(payments[0]).toMatchObject({
    plan: 'pro',
    first_utm_source: 'e2e',
    first_referrer: 'https://e2e.example',
    amount_cents: 2900,
  });

  const tenants = await pgQuery<{ plan: string }>('select plan from tenants where id = $1', [signup.tenant_id]);
  expect(tenants[0]?.plan).toBe('pro');

  await api.dispose();
});

test('browser checkout hands off to the payment provider URL', async ({ page }) => {
  const email = uniqueEmail('checkout-ui');
  const signup = await signupViaApi(email);

  // The dark Starter plan must not be reachable (BUSINESS_MODEL §6 flag).
  const api = await playwrightRequest.newContext({
    baseURL: API_BASE,
    extraHTTPHeaders: { Authorization: 'Bearer e2e-test-token' },
  });
  const starter = await api.post('/api/v1/checkout', { data: { plan: 'starter', tenant_id: signup.tenant_id } });
  expect(starter.status()).toBe(404);
  await api.dispose();

  // Seed the browser session with the tenant, as the signup page would have.
  await page.goto('/');
  await page.evaluate((tenantId) => window.localStorage.setItem('mdreel_tenant_id', tenantId), signup.tenant_id);

  // The fake gateway's checkout.test domain does not resolve — intercept it so the test asserts
  // the handoff deterministically instead of depending on DNS.
  let handoffUrl: string | null = null;
  await page.route('https://checkout.test/**', async (route) => {
    handoffUrl = route.request().url();
    await route.fulfill({ status: 200, contentType: 'text/html', body: '<h1>stripe-checkout-stub</h1>' });
  });

  await page.goto('/checkout?plan=pro');
  await page.getByRole('button', { name: 'continue to payment →' }).click();
  await page.waitForURL('https://checkout.test/**');
  expect(handoffUrl).toContain(`cs_test_${signup.tenant_id}_pro`);

  // checkout_clicked landed as a first-party event row.
  await expect
    .poll(async () => {
      const rows = await pgQuery<{ count: string }>(
        "select count(*) as count from events where name = 'checkout_clicked' and tenant_id = $1",
        [signup.tenant_id],
      );
      return Number(rows[0]?.count ?? 0);
    })
    .toBeGreaterThan(0);
});
