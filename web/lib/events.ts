// Typed client module for the METRICS.md §3 event schema. Function names/payload keys match that
// section exactly. emit() always records locally, and only sends first-party analytics to our API
// when NEXT_PUBLIC_API_BASE is configured in a browser.
import { getAbArm, getFirstTouch, getSessionId } from './attribution';
import { postEvent, sendEvent, type SignupEventResponse } from './api';
import { getTenantId, getUserId } from './session';

export interface EventContext {
  tenant_id: string | null;
  user_id: string | null;
  session_id: string;
  occurred_at: string;
  referrer: string | null;
  ab_arm: 'A' | 'B';
  utm_source: string | null;
  utm_medium: string | null;
  utm_campaign: string | null;
  utm_term: string | null;
  first_referrer: string | null;
}

export interface TrackedEvent extends EventContext {
  name: string;
  [key: string]: unknown;
}

export interface SignupEventPayload {
  email: string;
  archive_hours: number | null;
  monthly_hours: number | null;
  utm_source: string | null;
  utm_medium: string | null;
  utm_campaign: string | null;
  utm_term: string | null;
  first_referrer: string | null;
}

const eventLog: TrackedEvent[] = [];

function attributionContext() {
  const firstTouch = getFirstTouch();
  return {
    utm_source: firstTouch?.utm_source ?? null,
    utm_medium: firstTouch?.utm_medium ?? null,
    utm_campaign: firstTouch?.utm_campaign ?? null,
    utm_term: firstTouch?.utm_term ?? null,
    first_referrer: firstTouch?.referrer ?? null,
  };
}

function baseContext(): EventContext {
  return {
    tenant_id: getTenantId(),
    user_id: getUserId(),
    session_id: getSessionId(),
    occurred_at: new Date().toISOString(),
    referrer: typeof document !== 'undefined' ? document.referrer || null : null,
    ab_arm: getAbArm(),
    ...attributionContext(),
  };
}

function recordEvent(name: string, payload: object = {}): TrackedEvent {
  const event: TrackedEvent = { name, ...baseContext(), ...payload };
  console.log('[event]', event);
  eventLog.push(event);
  return event;
}

function emit(name: string, payload: object = {}): void {
  try {
    sendEvent(recordEvent(name, payload));
  } catch {
    // Analytics must never break the page.
  }
}

export function getEventLog(): TrackedEvent[] {
  return eventLog;
}

export function clearEventLog(): void {
  eventLog.length = 0;
}

export function buildSignupEventPayload(args: {
  email: string;
  archive_hours: number | null;
  monthly_hours: number | null;
}): SignupEventPayload {
  return {
    email: args.email,
    archive_hours: args.archive_hours,
    monthly_hours: args.monthly_hours,
    ...attributionContext(),
  };
}

export async function emitSignupEvent(args: {
  email: string;
  archive_hours: number | null;
  monthly_hours: number | null;
}): Promise<SignupEventResponse | null> {
  const event = recordEvent('signup', buildSignupEventPayload(args));
  try {
    return await postEvent<SignupEventResponse>(event);
  } catch {
    return null;
  }
}

export function trackPageView(path: string): void {
  emit('page_view', { path });
}

export function trackSignup(args: { archive_hours: number | null; monthly_hours: number | null }): void {
  emit('signup', args);
}

export function trackUploadStarted(args: { duration_sec: number }): void {
  emit('upload_started', args);
}

export function trackJobCompleted(args: {
  job_id: string;
  duration_sec: number;
  cost_cents: number;
  wall_clock_sec: number;
}): void {
  emit('job_completed', args);
}

export function trackOutputDownloaded(args: { job_id: string }): void {
  emit('output_downloaded', args);
}

export function trackUploadRepeat(args: { n_th: number; days_since_first: number }): void {
  emit('upload_repeat', args);
}

export function trackCheckoutClicked(plan: string): void {
  emit('checkout_clicked', { plan });
}

export function trackCheckoutAbandoned(args: { reason: string }): void {
  emit('checkout_abandoned', args);
}

export function trackPaymentSucceeded(args: { amount_cents: number }): void {
  emit('payment_succeeded', args);
}
