import type { PlanId } from './pricing';
import { withMdreelSessionHeader } from './apiHeaders';
import type { AdminAdSpendInput, AdminOverview } from './admin';

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

function jsonHeaders(headers?: HeadersInit): Headers {
  const next = withMdreelSessionHeader(headers);
  if (!next.has('Content-Type')) next.set('Content-Type', 'application/json');
  return next;
}

async function postJson<T>(path: string, body: unknown, init: RequestInit = {}): Promise<T | null> {
  const url = endpoint(path);
  if (!url || typeof window === 'undefined') return null;

  const res = await fetch(url, {
    method: 'POST',
    ...init,
    headers: jsonHeaders(init.headers),
    body: JSON.stringify(body),
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
      // sendBeacon cannot set custom headers; the event schema already carries session_id.
      if (navigator.sendBeacon(url, blob)) return;
    }

    void fetch(url, {
      method: 'POST',
      headers: jsonHeaders(),
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
  // Checkout requires an authenticated caller (ARCHITECTURE §5). Until magic-link auth issues real
  // session tokens, the tenant id doubles as the bearer credential.
  return postJson<CheckoutResponse>('/checkout', args, {
    headers: { Authorization: `Bearer ${args.tenant_id}` },
  });
}

// Same-origin checkout (relative /api/v1/*, proxied to the API by web/middleware.ts) — the pattern
// that works on the deployed site, where NEXT_PUBLIC_API_BASE is empty. The Identity cookie
// (credentials:'include') authenticates a signed-in caller; the tenant id doubles as the bearer
// credential for the pre-Identity checkout path (ARCHITECTURE §5). Degrades cleanly: when Stripe is
// unconfigured the API answers 503 and this returns null so the UI shows a note instead of
// pretending a payment succeeded.
export async function requestCheckout(args: { plan: PlanId; tenantId: string }): Promise<CheckoutResponse | null> {
  try {
    const headers: Record<string, string> = {};
    if (args.tenantId) headers.Authorization = `Bearer ${args.tenantId}`;
    const res = await fetch('/api/v1/checkout', {
      method: 'POST',
      credentials: 'include',
      headers: jsonHeaders(headers),
      body: JSON.stringify({ plan: args.plan, tenant_id: args.tenantId }),
    });
    if (!res.ok) return null;
    return (await res.json()) as CheckoutResponse;
  } catch {
    return null;
  }
}

export async function getAdminOverview(): Promise<AdminOverview | 'not-authorized' | null> {
  try {
    const res = await fetch('/api/v1/admin/overview', {
      method: 'GET',
      credentials: 'include',
      headers: withMdreelSessionHeader(),
    });
    if (res.status === 404) return 'not-authorized';
    if (!res.ok) return null;
    return (await res.json()) as AdminOverview;
  } catch {
    return null;
  }
}

export async function postAdminAdSpend(input: AdminAdSpendInput): Promise<boolean> {
  try {
    const res = await fetch('/api/v1/admin/ad-spend', {
      method: 'POST',
      credentials: 'include',
      headers: jsonHeaders(),
      body: JSON.stringify(input),
    });
    return res.ok;
  } catch {
    return false;
  }
}
