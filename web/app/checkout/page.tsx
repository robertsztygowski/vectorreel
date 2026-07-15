'use client';

import { useState } from 'react';
import { PRICING } from '@/lib/pricing';
import { trackCheckoutAbandoned, trackPaymentSucceeded } from '@/lib/events';

export default function CheckoutPage() {
  const [result, setResult] = useState<'succeeded' | 'abandoned' | null>(null);

  if (result === 'succeeded') {
    return (
      <div className="page-body">
        <div className="wrap page-narrow">
          <h1>Payment succeeded</h1>
          <p className="lead">This was simulated — no real Stripe integration until Phase 4. Welcome to mdreel!</p>
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
              mdreel {PRICING.planName} — €{PRICING.priceEur}/mo
            </p>
          </div>
          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
            <button
              className="btn btn-primary"
              type="button"
              onClick={() => {
                trackPaymentSucceeded({ amount_cents: PRICING.priceEur * 100 });
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
