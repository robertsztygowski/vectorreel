'use client';

import { Suspense, useState } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { getPlan, PLANS } from '@/lib/pricing';
import { trackCheckoutAbandoned, trackPaymentSucceeded } from '@/lib/events';

function CheckoutInner() {
  const searchParams = useSearchParams();
  const plan = getPlan(searchParams.get('plan')) ?? PLANS.pro;
  const [result, setResult] = useState<'succeeded' | 'abandoned' | null>(null);

  if (result === 'succeeded') {
    return (
      <div className="page-body">
        <div className="wrap page-narrow">
          <h1>Payment succeeded</h1>
          <p className="lead">This was simulated — no real Stripe integration until Phase 4. Welcome to mdreel!</p>
          <Link className="btn btn-primary" href="/app">
            Open your workspace
          </Link>
        </div>
      </div>
    );
  }

  if (result === 'abandoned') {
    return (
      <div className="page-body">
        <div className="wrap page-narrow">
          <h1>Checkout abandoned</h1>
          <p className="lead">No worries — you can come back to this any time from the pricing page.</p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>Checkout (mock)</h1>
          <p className="lead">
            A real Stripe payment link goes here in Phase 4 — for now, use the buttons below to exercise both
            outcomes.
          </p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap page-narrow">
          <div className="card" style={{ marginBottom: 20 }}>
            <p style={{ margin: 0, fontWeight: 700, fontSize: 18 }}>
              mdreel {plan.name} — €{plan.priceEur}/mo
            </p>
            <p style={{ margin: '6px 0 0', color: 'var(--ink-faint)', fontSize: 14.5 }}>
              {plan.hoursPerMonth} h/mo ·{' '}
              {plan.capKind === 'hard' ? 'hard cap, no overage' : `€${plan.overagePerHourEur}/h overage`}
            </p>
          </div>
          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
            <button
              className="btn btn-primary"
              type="button"
              onClick={() => {
                trackPaymentSucceeded({ amount_cents: plan.priceEur * 100 });
                setResult('succeeded');
              }}
            >
              Simulate payment_succeeded
            </button>
            <button
              className="btn btn-ghost"
              type="button"
              onClick={() => {
                trackCheckoutAbandoned({ reason: 'just_looking' });
                setResult('abandoned');
              }}
            >
              Simulate abandon
            </button>
          </div>
        </div>
      </div>
    </>
  );
}

export default function CheckoutPage() {
  return (
    <Suspense fallback={null}>
      <CheckoutInner />
    </Suspense>
  );
}
