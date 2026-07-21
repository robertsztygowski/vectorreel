// Feature flags. Read at render time so a flip is one env var, not a rebuild of component logic.
//
// SHOW_STARTER_PLAN — the €99/mo hard-capped Starter is pre-built but kept DARK (BUSINESS_MODEL §6
// "the on-ramp canyon"). It ships only if the launch shows the €149 Pro step throttling checkout
// (trial→checkout below the A2 floor, or "too expensive" dominating inbound — METRICS.md N21/N22).
// Default off. Flip with NEXT_PUBLIC_SHOW_STARTER=true.
/**
 * A flag is on only for the exact string "true".
 *
 * Deliberately not truthiness: `"1"`, `"yes"` and `"TRUE"` are all OFF. These flags gate what is
 * visible in public, so an ambiguous value must resolve to the safe state — a typo in a deploy
 * command should never be what publishes something.
 */
export const isEnabled = (value: string | undefined): boolean => value === 'true';

export const SHOW_STARTER_PLAN = isEnabled(process.env.NEXT_PUBLIC_SHOW_STARTER);

// SHOW_COLLECTIONS — the contract-derived collection pages (/collections/*). Built and fully
// tested, but OFF in production until the founder has reviewed licensing and attribution on real
// corpus material: nothing carries mdreel's name in public before that review (founder decision,
// 2026-07-21). When off the routes 404 and nothing links to them. Local, CI and E2E run with it ON
// so the pages are exercised exactly as they will ship. Flip with NEXT_PUBLIC_SHOW_COLLECTIONS=true.
export const SHOW_COLLECTIONS = isEnabled(process.env.NEXT_PUBLIC_SHOW_COLLECTIONS);
