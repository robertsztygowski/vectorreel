// Feature flags. Read at render time so a flip is one env var, not a rebuild of component logic.
//
// SHOW_STARTER_PLAN — the €99/mo hard-capped Starter is pre-built but kept DARK (BUSINESS_MODEL §6
// "the on-ramp canyon"). It ships only if the launch shows the €149 Pro step throttling checkout
// (trial→checkout below the A2 floor, or "too expensive" dominating inbound — METRICS.md N21/N22).
// Default off. Flip with NEXT_PUBLIC_SHOW_STARTER=true.
export const SHOW_STARTER_PLAN = process.env.NEXT_PUBLIC_SHOW_STARTER === 'true';
