import { beforeEach, test } from 'node:test';
import assert from 'node:assert/strict';
import { assignAbArmIfAbsent, captureFirstTouchIfAbsent } from './attribution';
import { clearEventLog, emitSignupEvent, getEventLog, trackPageView, trackPaymentSucceeded } from './events';

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

  clear(): void {
    this.values.clear();
  }
}

function installBrowser(search: string, referrer: string) {
  const localStorage = new MemoryStorage();
  const sessionStorage = new MemoryStorage();
  Object.defineProperty(globalThis, 'window', {
    value: { localStorage, sessionStorage, location: { search } },
    configurable: true,
    writable: true,
  });
  Object.defineProperty(globalThis, 'document', {
    value: { referrer },
    configurable: true,
    writable: true,
  });
  Object.defineProperty(globalThis, 'navigator', {
    value: {},
    configurable: true,
    writable: true,
  });
  return { localStorage, sessionStorage };
}

beforeEach(() => {
  delete process.env.NEXT_PUBLIC_API_BASE;
  clearEventLog();
});

test('first-touch attribution survives signup and payment_succeeded events', async () => {
  const { localStorage } = installBrowser(
    '?utm_source=linkedin&utm_medium=cpc&utm_campaign=launch&utm_term=dpo',
    'https://example.eu/article',
  );
  localStorage.setItem('mdreel_ab_arm', 'B');

  const firstTouch = captureFirstTouchIfAbsent();
  const arm = assignAbArmIfAbsent();
  assert.equal(firstTouch?.utm_source, 'linkedin');
  assert.equal(arm, 'B');

  await emitSignupEvent({ email: 'ada@example.eu', archive_hours: 12, monthly_hours: 3 });
  trackPaymentSucceeded({ amount_cents: 14900 });

  const [signup, payment] = getEventLog();
  assert.equal(signup.name, 'signup');
  assert.equal(signup.email, 'ada@example.eu');
  assert.equal(signup.archive_hours, 12);
  assert.equal(signup.monthly_hours, 3);
  assert.equal(signup.utm_source, 'linkedin');
  assert.equal(signup.utm_medium, 'cpc');
  assert.equal(signup.utm_campaign, 'launch');
  assert.equal(signup.utm_term, 'dpo');
  assert.equal(signup.first_referrer, 'https://example.eu/article');
  assert.equal(signup.ab_arm, 'B');

  assert.equal(payment.name, 'payment_succeeded');
  assert.equal(payment.amount_cents, 14900);
  assert.equal(payment.utm_source, signup.utm_source);
  assert.equal(payment.utm_medium, signup.utm_medium);
  assert.equal(payment.utm_campaign, signup.utm_campaign);
  assert.equal(payment.utm_term, signup.utm_term);
  assert.equal(payment.first_referrer, signup.first_referrer);
  assert.equal(payment.ab_arm, signup.ab_arm);
});

test('emit records locally and does not throw when NEXT_PUBLIC_API_BASE is unset', () => {
  installBrowser('', '');

  assert.doesNotThrow(() => trackPageView('/pricing'));

  const [event] = getEventLog();
  assert.equal(event.name, 'page_view');
  assert.equal(event.path, '/pricing');
});
