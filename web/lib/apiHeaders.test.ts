import { afterEach, beforeEach, test } from 'node:test';
import assert from 'node:assert/strict';
import { postEvent, requestCheckout, sendEvent } from './api';
import { MDREEL_SESSION_HEADER } from './apiHeaders';

class MemoryStorage {
  private values = new Map<string, string>();
  getItem(key: string): string | null {
    return this.values.get(key) ?? null;
  }
  setItem(key: string, value: string): void {
    this.values.set(key, value);
  }
}

interface FetchCall {
  url: string;
  init: RequestInit;
}

let calls: FetchCall[] = [];

function installBrowser(): void {
  const sessionStorage = new MemoryStorage();
  sessionStorage.setItem('mdreel_session_id', 'sid_test-123');
  Object.defineProperty(globalThis, 'window', {
    value: { sessionStorage, localStorage: new MemoryStorage(), location: { search: '' } },
    configurable: true,
    writable: true,
  });
  Object.defineProperty(globalThis, 'document', { value: { referrer: '' }, configurable: true, writable: true });
  Object.defineProperty(globalThis, 'navigator', { value: {}, configurable: true, writable: true });
  globalThis.fetch = ((url: string, init: RequestInit = {}) => {
    calls.push({ url, init });
    return Promise.resolve({
      ok: true,
      status: 202,
      json: () => Promise.resolve({ eventId: 'evt_1', url: 'https://checkout.test', sessionId: 'cs_1' }),
    } as Response);
  }) as typeof fetch;
}

function header(init: RequestInit, name: string): string | null {
  return new Headers(init.headers).get(name);
}

beforeEach(() => {
  process.env.NEXT_PUBLIC_API_BASE = 'https://api.example.eu';
  calls = [];
  installBrowser();
});

afterEach(() => {
  delete process.env.NEXT_PUBLIC_API_BASE;
  delete (globalThis as { fetch?: unknown }).fetch;
});

test('postEvent adds the first-party session header', async () => {
  await postEvent({ name: 'page_view', session_id: 'sid_payload' });

  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, 'https://api.example.eu/api/v1/events');
  assert.equal(header(calls[0].init, MDREEL_SESSION_HEADER), 'sid_test-123');
});

test('sendEvent fetch fallback adds the session header when sendBeacon is absent', async () => {
  sendEvent({ name: 'page_view', session_id: 'sid_payload' });

  await new Promise((resolve) => setTimeout(resolve, 0));
  assert.equal(calls.length, 1);
  assert.equal(header(calls[0].init, MDREEL_SESSION_HEADER), 'sid_test-123');
  assert.equal(calls[0].init.keepalive, true);
});

test('same-origin checkout adds the session header and keeps credentials', async () => {
  await requestCheckout({ plan: 'pro', tenantId: 'ten_1' });

  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, '/api/v1/checkout');
  assert.equal(calls[0].init.credentials, 'include');
  assert.equal(header(calls[0].init, MDREEL_SESSION_HEADER), 'sid_test-123');
});
