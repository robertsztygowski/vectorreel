import { afterEach, beforeEach, test } from 'node:test';
import assert from 'node:assert/strict';
import { buildRegisterBody, fetchSession, login, logout, registerAccount } from './auth';
import { MDREEL_SESSION_HEADER } from './apiHeaders';

class MemoryStorage {
  private values = new Map<string, string>();
  getItem(key: string): string | null {
    return this.values.get(key) ?? null;
  }
  setItem(key: string, value: string): void {
    this.values.set(key, value);
  }
  removeItem(key: string): void {
    this.values.delete(key);
  }
}

interface FetchCall {
  url: string;
  init: RequestInit;
}

let calls: FetchCall[] = [];

function installBrowser(): void {
  const localStorage = new MemoryStorage();
  const sessionStorage = new MemoryStorage();
  sessionStorage.setItem('mdreel_session_id', 'sid_auth');
  Object.defineProperty(globalThis, 'window', {
    value: { localStorage, sessionStorage, location: { search: '' } },
    configurable: true,
    writable: true,
  });
  Object.defineProperty(globalThis, 'document', { value: { referrer: '' }, configurable: true, writable: true });
  Object.defineProperty(globalThis, 'navigator', { value: {}, configurable: true, writable: true });
}

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

beforeEach(() => {
  installBrowser();
});

afterEach(() => {
  delete (globalThis as { fetch?: unknown }).fetch;
});

test('registerAccount posts same-origin with credentials and attribution', async () => {
  stubFetch({ ok: true, json: { email: 'a@b.eu', tenant_id: 'ten_1', trial_credit_hours: 1 } });
  const result = await registerAccount({ email: 'a@b.eu', password: 'Str0ng-Passw0rd!', archive_hours: 12 });

  assert.equal(result.tenant_id, 'ten_1');
  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, '/api/v1/auth/signup');
  assert.equal(calls[0].init.method, 'POST');
  assert.equal(calls[0].init.credentials, 'include');
  assert.equal(new Headers(calls[0].init.headers).get(MDREEL_SESSION_HEADER), 'sid_auth');
  const body = JSON.parse(calls[0].init.body as string);
  assert.equal(body.email, 'a@b.eu');
  assert.equal(body.password, 'Str0ng-Passw0rd!');
  assert.equal(body.archiveHours, 12);
  assert.ok('abArm' in body);
});

test('registerAccount surfaces the API problem message', async () => {
  stubFetch({ ok: false, status: 400, json: { errors: { DuplicateUserName: ['Email already taken.'] } } });
  await assert.rejects(
    () => registerAccount({ email: 'a@b.eu', password: 'x' }),
    /Email already taken\./,
  );
});

test('login targets the cookie login endpoint', async () => {
  stubFetch({ ok: true });
  await login('a@b.eu', 'Str0ng-Passw0rd!');
  assert.equal(calls[0].url, '/api/v1/auth/login?useCookies=true');
  assert.equal(calls[0].init.credentials, 'include');
  assert.equal(new Headers(calls[0].init.headers).get(MDREEL_SESSION_HEADER), 'sid_auth');
});

test('logout never throws', async () => {
  stubFetch({ ok: true });
  await logout();
  assert.equal(calls[0].url, '/api/v1/auth/logout');
  assert.equal(new Headers(calls[0].init.headers).get(MDREEL_SESSION_HEADER), 'sid_auth');
});

test('fetchSession returns null on 401', async () => {
  stubFetch({ ok: false, status: 401 });
  const session = await fetchSession();
  assert.equal(session, null);
});

test('buildRegisterBody maps optional survey fields to null when absent', () => {
  const body = buildRegisterBody({ email: 'a@b.eu', password: 'p' });
  assert.equal(body.archiveHours, null);
  assert.equal(body.monthlyHours, null);
});
