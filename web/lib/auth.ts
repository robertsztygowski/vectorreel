// Real authentication client for ASP.NET Core Identity (email + password, cookie mode). Every call
// is SAME-ORIGIN (relative /api/v1/auth/*, proxied to the API by next.config.mjs) with
// credentials:'include' so the Identity application cookie is set and sent first-party.
import { getAbArm, getFirstTouch } from './attribution';
import { withMdreelSessionHeader } from './apiHeaders';

export interface AuthSession {
  email: string;
  tenant_id: string | null;
}

export interface RegisterResult {
  email: string;
  tenant_id: string;
  trial_credit_hours: number;
}

export interface RegisterInput {
  email: string;
  password: string;
  archive_hours?: number | null;
  monthly_hours?: number | null;
}

function authUrl(path: string): string {
  return `/api/v1/auth${path}`;
}

async function readProblem(res: Response): Promise<string> {
  try {
    const body = (await res.json()) as { title?: string; detail?: string; errors?: Record<string, string[]> };
    if (body.errors) {
      const first = Object.values(body.errors)[0];
      if (first && first.length) return first[0];
    }
    return body.detail ?? body.title ?? `Request failed (${res.status}).`;
  } catch {
    return `Request failed (${res.status}).`;
  }
}

export function buildRegisterBody(input: RegisterInput): Record<string, unknown> {
  const firstTouch = getFirstTouch();
  return {
    email: input.email,
    password: input.password,
    archiveHours: input.archive_hours ?? null,
    monthlyHours: input.monthly_hours ?? null,
    utmSource: firstTouch?.utm_source ?? null,
    utmMedium: firstTouch?.utm_medium ?? null,
    utmCampaign: firstTouch?.utm_campaign ?? null,
    utmTerm: firstTouch?.utm_term ?? null,
    firstReferrer: firstTouch?.referrer ?? null,
    abArm: getAbArm(),
  };
}

export async function registerAccount(input: RegisterInput): Promise<RegisterResult> {
  const res = await fetch(authUrl('/signup'), {
    method: 'POST',
    credentials: 'include',
    headers: withMdreelSessionHeader({ 'Content-Type': 'application/json' }),
    body: JSON.stringify(buildRegisterBody(input)),
  });
  if (!res.ok) throw new Error(await readProblem(res));
  return (await res.json()) as RegisterResult;
}

export async function login(email: string, password: string): Promise<void> {
  const res = await fetch(authUrl('/login?useCookies=true'), {
    method: 'POST',
    credentials: 'include',
    headers: withMdreelSessionHeader({ 'Content-Type': 'application/json' }),
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) throw new Error(await readProblem(res));
}

export async function logout(): Promise<void> {
  try {
    await fetch(authUrl('/logout'), { method: 'POST', credentials: 'include', headers: withMdreelSessionHeader() });
  } catch {
    // Sign-out must never leave the UI stuck; local session is cleared regardless.
  }
}

export async function fetchSession(): Promise<AuthSession | null> {
  try {
    const res = await fetch(authUrl('/me'), { credentials: 'include', headers: withMdreelSessionHeader() });
    if (!res.ok) return null;
    return (await res.json()) as AuthSession;
  } catch {
    return null;
  }
}
