// First-touch UTM capture + ab_arm assignment, persisted client-side in localStorage.
//
// Why localStorage, not a cookie: PLAN.md Phase 5 carries forward a founder-owned open question —
// whether first-party UTM attribution to `payment_succeeded` is "strictly necessary" under GDPR,
// or belongs in sessionStorage attaching only at signup. localStorage behind this one module keeps
// that a one-file swap later without pre-empting the call or setting an actual cookie now.
//
// "First-touch" contract: once captured, never overwritten by a later visit with different UTM
// params — read-before-write on every call.

export type AbArm = 'A' | 'B';

export interface FirstTouch {
  utm_source: string | null;
  utm_medium: string | null;
  utm_campaign: string | null;
  utm_term: string | null;
  referrer: string | null;
  captured_at: string;
}

const FIRST_TOUCH_KEY = 'mdreel_first_touch';
const AB_ARM_KEY = 'mdreel_ab_arm';
const SESSION_KEY = 'mdreel_session_id';

function isBrowser(): boolean {
  return typeof window !== 'undefined';
}

export function getFirstTouch(): FirstTouch | null {
  if (!isBrowser()) return null;
  const raw = window.localStorage.getItem(FIRST_TOUCH_KEY);
  return raw ? (JSON.parse(raw) as FirstTouch) : null;
}

export function captureFirstTouchIfAbsent(): FirstTouch | null {
  if (!isBrowser()) return null;
  const existing = getFirstTouch();
  if (existing) return existing;

  const params = new URLSearchParams(window.location.search);
  const firstTouch: FirstTouch = {
    utm_source: params.get('utm_source'),
    utm_medium: params.get('utm_medium'),
    utm_campaign: params.get('utm_campaign'),
    utm_term: params.get('utm_term'),
    referrer: document.referrer || null,
    captured_at: new Date().toISOString(),
  };
  window.localStorage.setItem(FIRST_TOUCH_KEY, JSON.stringify(firstTouch));
  return firstTouch;
}

export function getAbArm(): AbArm {
  if (!isBrowser()) return 'A';
  const existing = window.localStorage.getItem(AB_ARM_KEY);
  return existing === 'A' || existing === 'B' ? existing : 'A';
}

export function assignAbArmIfAbsent(): AbArm {
  if (!isBrowser()) return 'A';
  const existing = window.localStorage.getItem(AB_ARM_KEY);
  if (existing === 'A' || existing === 'B') return existing;
  const arm: AbArm = Math.random() < 0.5 ? 'A' : 'B';
  window.localStorage.setItem(AB_ARM_KEY, arm);
  return arm;
}

export function getSessionId(): string {
  if (!isBrowser()) return 'server';
  const existing = window.sessionStorage.getItem(SESSION_KEY);
  if (existing) return existing;
  const id = crypto.randomUUID();
  window.sessionStorage.setItem(SESSION_KEY, id);
  return id;
}
