// Mock client-side "signed in" flag for the Phase 2R authenticated panel. There is no real auth or
// backend this phase — completing the magic-link form just sets this so /app renders a workspace.
// Phase 4 replaces this with a real session cookie.
const SIGNED_IN_KEY = 'mdreel_signed_in';
const EMAIL_KEY = 'mdreel_email';

export function markSignedIn(email: string): void {
  if (typeof window === 'undefined') return;
  window.localStorage.setItem(SIGNED_IN_KEY, 'true');
  if (email) window.localStorage.setItem(EMAIL_KEY, email);
}

export function isSignedIn(): boolean {
  if (typeof window === 'undefined') return false;
  return window.localStorage.getItem(SIGNED_IN_KEY) === 'true';
}

export function getEmail(): string | null {
  if (typeof window === 'undefined') return null;
  return window.localStorage.getItem(EMAIL_KEY);
}

export function signOut(): void {
  if (typeof window === 'undefined') return;
  window.localStorage.removeItem(SIGNED_IN_KEY);
  window.localStorage.removeItem(EMAIL_KEY);
}
