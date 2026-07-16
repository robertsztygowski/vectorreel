// Mock client-side session state for the Phase 2R/4 authenticated panel. There is no real auth
// cookie yet — completing the magic-link form sets this so /app renders a workspace.
const SIGNED_IN_KEY = 'mdreel_signed_in';
const EMAIL_KEY = 'mdreel_email';
const TENANT_ID_KEY = 'mdreel_tenant_id';
const USER_ID_KEY = 'mdreel_user_id';

function isBrowser(): boolean {
  return typeof window !== 'undefined';
}

export function markSignedIn(email: string): void {
  if (!isBrowser()) return;
  window.localStorage.setItem(SIGNED_IN_KEY, 'true');
  if (email) window.localStorage.setItem(EMAIL_KEY, email);
}

export function isSignedIn(): boolean {
  if (!isBrowser()) return false;
  return window.localStorage.getItem(SIGNED_IN_KEY) === 'true';
}

export function getEmail(): string | null {
  if (!isBrowser()) return null;
  return window.localStorage.getItem(EMAIL_KEY);
}

export function setSessionIds(args: { tenant_id?: string | null; user_id?: string | null }): void {
  if (!isBrowser()) return;
  if (args.tenant_id) window.localStorage.setItem(TENANT_ID_KEY, args.tenant_id);
  if (args.user_id) window.localStorage.setItem(USER_ID_KEY, args.user_id);
}

export function getTenantId(): string | null {
  if (!isBrowser()) return null;
  return window.localStorage.getItem(TENANT_ID_KEY);
}

export function getUserId(): string | null {
  if (!isBrowser()) return null;
  return window.localStorage.getItem(USER_ID_KEY);
}

export function clearSessionIds(): void {
  if (!isBrowser()) return;
  window.localStorage.removeItem(TENANT_ID_KEY);
  window.localStorage.removeItem(USER_ID_KEY);
}

export function signOut(): void {
  if (!isBrowser()) return;
  window.localStorage.removeItem(SIGNED_IN_KEY);
  window.localStorage.removeItem(EMAIL_KEY);
  clearSessionIds();
}
