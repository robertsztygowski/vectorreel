export interface AdminFunnelWindow {
  window: 'today' | '7d' | '30d' | string;
  pageView: number;
  signupView: number;
  signup: number;
  uploadStarted: number;
  jobCompleted: number;
  checkoutClicked: number;
  paymentSucceeded: number;
}

export interface AdminUsageWindow {
  videosProcessed: number;
  videoMinutes: number;
}

export interface AdminOverview {
  funnel: AdminFunnelWindow[];
  usage: {
    today: AdminUsageWindow;
    sevenDays: AdminUsageWindow;
  };
  retention: {
    newLast7d: number;
    newLast30d: number;
    returningThisWeek: number;
    inactive30d: number;
    signupWeeks: { week: string; tenants: number }[];
  };
  sources: AdminSourceOverview[];
}

export interface AdminSourceOverview {
  firstUtmSource: string | null;
  firstUtmMedium: string | null;
  firstUtmCampaign: string | null;
  tenantCount: number;
  payingTenantCount: number;
  revenueCents: number;
  adSpendCents: number;
  cacCents: number | null;
}

export interface AdminAdSpendInput {
  source: string;
  campaign: string;
  amount_cents: number;
  currency: string;
  spent_on: string;
}

export function formatCents(cents: number | null): string {
  if (cents === null) return '—';
  return new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' }).format(cents / 100);
}

export function formatMinutes(minutes: number): string {
  return `${minutes.toFixed(1)} min`;
}

export function sourceLabel(source: AdminSourceOverview): string {
  return [source.firstUtmSource, source.firstUtmMedium, source.firstUtmCampaign]
    .map((part) => part?.trim() || '∅')
    .join(' / ');
}
