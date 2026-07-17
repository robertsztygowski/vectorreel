import { afterEach, beforeEach, test } from 'node:test';
import assert from 'node:assert/strict';
import { requestBillingPortal } from './billing';

interface FetchCall {
  url: string;
  init: RequestInit;
}

let calls: FetchCall[] = [];

function stubFetch(response: { ok: boolean; status?: number; json?: unknown }): void {
  calls = [];
  globalThis.fetch = ((url: string, init: RequestInit = {}) => {
    calls.push({ url, init });
    return Promise.resolve({
      ok: response.ok,
      status: response.status ?? (response.ok ? 200 : 400),
      json: () => Promise.resolve(response.json ?? {}),
    } as Response);
  }) as typeof fetch;
}

afterEach(() => {
  delete (globalThis as { fetch?: unknown }).fetch;
});

beforeEach(() => {
  calls = [];
});

test('requestBillingPortal posts same-origin with credentials and the tenant id', async () => {
  stubFetch({ ok: true, json: { url: 'https://billing.stripe.test/session/abc' } });
  const result = await requestBillingPortal('ten_1');

  assert.equal(result?.url, 'https://billing.stripe.test/session/abc');
  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, '/api/v1/billing/portal');
  assert.equal(calls[0].init.method, 'POST');
  assert.equal(calls[0].init.credentials, 'include');
  assert.equal(JSON.parse(calls[0].init.body as string).tenant_id, 'ten_1');
});

test('requestBillingPortal returns null when billing is not configured (503)', async () => {
  stubFetch({ ok: false, status: 503 });
  const result = await requestBillingPortal('ten_1');
  assert.equal(result, null);
});

test('requestBillingPortal returns null when the request throws', async () => {
  globalThis.fetch = (() => Promise.reject(new Error('network'))) as typeof fetch;
  const result = await requestBillingPortal('ten_1');
  assert.equal(result, null);
});
