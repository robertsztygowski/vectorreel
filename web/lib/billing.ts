// Customer billing portal client. Same-origin (relative /api/v1/*, proxied to the API by
// web/middleware.ts) with credentials:'include' so the Identity cookie authenticates the caller.
// Degrades cleanly: when Stripe is not configured the API answers 503 and this returns null so the
// UI can show an unobtrusive note instead of breaking.

export interface BillingPortalResult {
  url: string;
}

export async function requestBillingPortal(tenantId: string): Promise<BillingPortalResult | null> {
  try {
    const res = await fetch('/api/v1/billing/portal', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ tenant_id: tenantId }),
    });
    if (!res.ok) return null;
    return (await res.json()) as BillingPortalResult;
  } catch {
    return null;
  }
}
