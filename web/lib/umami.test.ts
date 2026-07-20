import { beforeEach, test } from 'node:test';
import assert from 'node:assert/strict';
import { trackUmami, trackUmamiFunnelView } from './umami';

class MemoryStorage {
  private values = new Map<string, string>();
  getItem(key: string): string | null {
    return this.values.get(key) ?? null;
  }
  setItem(key: string, value: string): void {
    this.values.set(key, value);
  }
}

function installBrowser(track?: (name: string, data?: Record<string, unknown>) => void): void {
  const sessionStorage = new MemoryStorage();
  sessionStorage.setItem('mdreel_session_id', 'sid_umami');
  Object.defineProperty(globalThis, 'window', {
    value: { sessionStorage, umami: track ? { track } : undefined },
    configurable: true,
    writable: true,
  });
}

beforeEach(() => {
  installBrowser();
});

test('trackUmami is a no-op when the Umami script is absent', () => {
  assert.doesNotThrow(() => trackUmami('signup_view'));
});

test('trackUmami sends the per-visit sid when Umami is present', () => {
  const calls: Array<{ name: string; data?: Record<string, unknown> }> = [];
  installBrowser((name, data) => calls.push({ name, data }));

  trackUmami('signup_view', { plan: 'pro' });

  assert.deepEqual(calls, [{ name: 'signup_view', data: { plan: 'pro', sid: 'sid_umami' } }]);
});

test('trackUmamiFunnelView maps public funnel pages only', () => {
  const calls: string[] = [];
  installBrowser((name) => calls.push(name));

  trackUmamiFunnelView('/pricing');
  trackUmamiFunnelView('/gallery');
  trackUmamiFunnelView('/docs');
  trackUmamiFunnelView('/signup');
  trackUmamiFunnelView('/app');

  assert.deepEqual(calls, ['pricing_view', 'gallery_view', 'docs_view', 'signup_view']);
});
