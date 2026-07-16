import type { PlanId } from './pricing';

export interface EventPostResponse {
  eventId: string;
}

export interface SignupEventResponse extends EventPostResponse {
  tenant_id: string;
  user_id: string;
  trial_credit_hours: number;
}

export interface CheckoutResponse {
  url: string;
  sessionId: string;
}

export function getApiBase(): string | null {
  const raw = process.env.NEXT_PUBLIC_API_BASE?.trim();
  return raw ? raw.replace(/\/+$/, '') : null;
}

export function canCallApi(): boolean {
  return typeof window !== 'undefined' && getApiBase() !== null;
}

function endpoint(path: string): string | null {
  const base = getApiBase();
  return base ? `${base}/api/v1${path}` : null;
}

async function postJson<T>(path: string, body: unknown, init: RequestInit = {}): Promise<T | null> {
  const url = endpoint(path);
  if (!url || typeof window === 'undefined') return null;

  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...(init.headers ?? {}) },
    body: JSON.stringify(body),
    ...init,
  });
  if (!res.ok) throw new Error(`POST ${path} failed: ${res.status}`);
  return (await res.json()) as T;
}

export function sendEvent(event: Record<string, unknown>): void {
  const url = endpoint('/events');
  if (!url || typeof window === 'undefined') return;

  const body = JSON.stringify(event);
  try {
    if (typeof navigator !== 'undefined' && typeof navigator.sendBeacon === 'function') {
      const blob = new Blob([body], { type: 'application/json' });
      if (navigator.sendBeacon(url, blob)) return;
    }

    void fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body,
      keepalive: true,
    }).catch(() => undefined);
  } catch {
    // Analytics must never break the page.
  }
}

export function postEvent<T extends EventPostResponse = EventPostResponse>(
  event: Record<string, unknown>,
): Promise<T | null> {
  return postJson<T>('/events', event);
}

export function postCheckout(args: { plan: PlanId; tenant_id: string }): Promise<CheckoutResponse | null> {
  return postJson<CheckoutResponse>('/checkout', args);
}
