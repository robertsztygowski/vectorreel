// Typed client module for the METRICS.md §3 event schema. Function names/payload keys match that
// section exactly so Phase 4 can grep for them when wiring the real Postgres transport — the only
// change needed then is inside emit(). For now, emit() is a stub: console.log + an in-memory
// buffer (getEventLog()), no network call.
import { getAbArm, getSessionId } from './attribution';

export interface EventContext {
  tenant_id: string | null;
  user_id: string | null;
  session_id: string;
  occurred_at: string;
  referrer: string | null;
  ab_arm: 'A' | 'B';
}

export interface TrackedEvent extends EventContext {
  name: string;
  [key: string]: unknown;
}

const eventLog: TrackedEvent[] = [];

function baseContext(): EventContext {
  return {
    tenant_id: null,
    user_id: null,
    session_id: getSessionId(),
    occurred_at: new Date().toISOString(),
    referrer: typeof document !== 'undefined' ? document.referrer || null : null,
    ab_arm: getAbArm(),
  };
}

function emit(name: string, payload: Record<string, unknown> = {}): void {
  const event: TrackedEvent = { name, ...baseContext(), ...payload };
  console.log('[event]', event);
  eventLog.push(event);
}

export function getEventLog(): TrackedEvent[] {
  return eventLog;
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
