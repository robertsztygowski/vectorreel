import { test } from 'node:test';
import assert from 'node:assert/strict';
import { formatCents, formatMinutes, sourceLabel, type AdminSourceOverview } from './admin';

test('admin formatting keeps empty CAC explicit', () => {
  assert.equal(formatCents(null), '—');
  assert.equal(formatMinutes(12.345), '12.3 min');
});

test('sourceLabel shows the attribution tuple with null markers', () => {
  const source: AdminSourceOverview = {
    firstUtmSource: 'google',
    firstUtmMedium: null,
    firstUtmCampaign: 'launch',
    tenantCount: 1,
    payingTenantCount: 1,
    revenueCents: 14900,
    adSpendCents: 3000,
    cacCents: 3000,
  };

  assert.equal(sourceLabel(source), 'google / ∅ / launch');
});
