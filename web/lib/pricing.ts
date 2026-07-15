// Sourced from BUSINESS_MODEL.md §6. The MVP ships EXACTLY two plans plus a one-time trial credit
// (decided 2026-07-15): no free tier, no free tool. Pro is hard-capped (processing pauses at the
// limit); Business meters overage past its cap. The full tier table (API PAYG / Enterprise) stays
// a hypothesis gated on assumption A3 and is NOT shown.
//
// The €99 Starter is pre-built as a one-switch fallback for the on-ramp canyon but kept DARK
// (lib/flags.ts SHOW_STARTER_PLAN) — do not show it by default.

export type PlanId = 'starter' | 'pro' | 'business';

export interface Plan {
  id: PlanId;
  name: string;
  priceEur: number;
  hoursPerMonth: number;
  capKind: 'hard' | 'metered';
  overagePerHourEur?: number; // metered plans only
  tagline: string;
  features: string[];
  highlighted?: boolean;
  dark?: boolean; // pre-built but hidden unless SHOW_STARTER_PLAN
}

// One-time trial credit granted at signup (METRICS.md N33) — replaces the old "2 h free" trial.
export const TRIAL_CREDIT_HOURS = 1;

export const PLANS: Record<PlanId, Plan> = {
  starter: {
    id: 'starter',
    name: 'Starter',
    priceEur: 99,
    hoursPerMonth: 15,
    capKind: 'hard',
    tagline: 'On-ramp fallback — kept dark until the data asks for it.',
    features: [
      '15 h of video processed / month',
      'Hard cap — no overage, upgrade to continue',
      'UI + API access, webhooks, MCP',
      'EU-only processing, source deleted after processing',
    ],
    dark: true,
  },
  pro: {
    id: 'pro',
    name: 'Pro',
    priceEur: 149,
    hoursPerMonth: 25,
    capKind: 'hard',
    tagline: 'For a single team putting its video library to work.',
    features: [
      '25 h of video processed / month',
      'Hard cap — processing pauses at the limit, upgrade to continue',
      'UI + API access, webhooks, MCP server',
      'EU-only processing, source deleted after processing',
    ],
    highlighted: true,
  },
  business: {
    id: 'business',
    name: 'Business',
    priceEur: 690,
    hoursPerMonth: 150,
    capKind: 'metered',
    overagePerHourEur: 6,
    tagline: 'For heavier archives and ongoing pipelines.',
    features: [
      '150 h of video processed / month',
      'Metered overage: €6/h past the cap',
      'Priority support',
      'Everything in Pro',
    ],
  },
};

// The plans shown on the live pricing page, in order. Starter is appended only when its flag is on.
export function visiblePlans(showStarter: boolean): Plan[] {
  const live = [PLANS.pro, PLANS.business];
  return showStarter ? [PLANS.starter, ...live] : live;
}

export function getPlan(id: string | null | undefined): Plan | null {
  if (id === 'starter' || id === 'pro' || id === 'business') return PLANS[id];
  return null;
}

