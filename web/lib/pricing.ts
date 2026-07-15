// Sourced from BUSINESS_MODEL.md §6 "Pro" row — the sole MVP plan. BUSINESS_MODEL.md is explicit
// that the full tier table (Pro/Business/API/Enterprise) is a hypothesis gated on assumption A3;
// the MVP pricing page shows ONE plan + the free tool, not a tiered comparison.
export const PRICING = {
  planName: 'Pro',
  priceEur: 149,
  hoursPerMonth: 25,
  overagePerHourEur: 8,
  freeTrialHours: 2,
} as const;
