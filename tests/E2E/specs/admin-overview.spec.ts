import { test, expect, request as playwrightRequest } from '@playwright/test';
import { API_BASE, pgQuery } from '../helpers';

test('admin overview exposes funnel keys and ad-spend CAC for an allowlisted tenant', async () => {
  const anonymous = await playwrightRequest.newContext({ baseURL: API_BASE });
  const campaign = `e2e-admin-${Date.now()}`;
  const adminEmail = 'e2e-admin@example.test';

  await pgQuery(
    `
    with doomed as (select id from tenants where email = $1),
    del_events as (delete from events where tenant_id in (select id from doomed) returning id),
    del_payments as (delete from payments where tenant_id in (select id from doomed) returning id),
    del_usage as (delete from usage_ledger where tenant_id in (select id from doomed) returning id),
    del_subscriptions as (delete from subscriptions where tenant_id in (select id from doomed) returning id),
    del_users as (delete from users where tenant_id in (select id from doomed) returning id)
    delete from tenants where id in (select id from doomed)
    `,
    [adminEmail],
  );

  const signupRes = await anonymous.post('/api/v1/events', {
    data: {
      name: 'signup',
      session_id: `sess_${campaign}`,
      email: adminEmail,
      utm_source: 'google_ads',
      utm_medium: 'cpc',
      utm_campaign: campaign,
      occurred_at: new Date().toISOString(),
    },
  });
  expect(signupRes.status()).toBe(202);
  const signup = (await signupRes.json()) as { tenant_id: string };

  await anonymous.post('/api/v1/events', { data: { name: 'page_view', session_id: `sess_${campaign}`, occurred_at: new Date().toISOString(), path: '/' } });
  await anonymous.post('/api/v1/events', { data: { name: 'signup_view', session_id: `sess_${campaign}`, occurred_at: new Date().toISOString(), path: '/signup' } });

  const admin = await playwrightRequest.newContext({
    baseURL: API_BASE,
    extraHTTPHeaders: { Authorization: `Bearer ${signup.tenant_id}` },
  });

  const spend = await admin.post('/api/v1/admin/ad-spend', {
    data: {
      source: 'google_ads',
      campaign,
      amount_cents: 2500,
      currency: 'EUR',
      spent_on: new Date().toISOString().slice(0, 10),
    },
  });
  expect(spend.status()).toBe(201);

  const overviewRes = await admin.get('/api/v1/admin/overview');
  expect(overviewRes.status()).toBe(200);
  const overview = await overviewRes.json();
  const today = overview.funnel.find((row: { window: string }) => row.window === 'today');
  expect(today).toEqual(expect.objectContaining({
    pageView: expect.any(Number),
    signupView: expect.any(Number),
    signup: expect.any(Number),
    uploadStarted: expect.any(Number),
    jobCompleted: expect.any(Number),
    checkoutClicked: expect.any(Number),
    paymentSucceeded: expect.any(Number),
  }));

  const source = overview.sources.find((row: { firstUtmCampaign: string }) => row.firstUtmCampaign === campaign);
  expect(source).toEqual(expect.objectContaining({ adSpendCents: 2500, cacCents: null }));

  await admin.dispose();
  await anonymous.dispose();
});
