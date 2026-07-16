'use client';

import { Suspense, useState } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { getPlan, PLANS } from '@/lib/pricing';
import { trackCheckoutAbandoned, trackPaymentSucceeded } from '@/lib/events';

function CheckoutInner() {
  const searchParams = useSearchParams();
  const plan = getPlan(searchParams.get('plan')) ?? PLANS.pro;
  const forcedState = searchParams.get('state');
  const [result, setResult] = useState<'succeeded' | 'abandoned' | null>(null);
  const effectiveResult = (forcedState === 'done' ? 'succeeded' : forcedState === 'abandoned' ? 'abandoned' : result);

  if (effectiveResult === 'succeeded') {
    return (
      <div className="auth-col wrap page-narrow">
        <p className="sent-line">
          <span className="ok-text">✓ payment confirmed — receipt sent to jonas@acme.eu</span>
        </p>
        <h1>Payment succeeded.</h1>
        <p className="lead">
          Welcome to pro — your plan is active immediately, and 25 hours of processing are ready in your workspace
          starting now.
        </p>
          <Link className="btn btn-primary" href="/app">
          open your workspace
          </Link>
      </div>
    );
  }

  if (effectiveResult === 'abandoned') {
    return (
      <div className="auth-col wrap page-narrow">
        <p className="sent-line">nothing was charged</p>
        <h1>Checkout abandoned.</h1>
        <p className="lead">Your workspace, plan and trial balance are exactly as you left them.</p>
        <Link className="btn btn-ghost" href="/pricing">
          ← back to pricing
        </Link>
      </div>
    );
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <p className="kicker"># checkout</p>
          <h1>Order summary.</h1>
          <p className="lead">
            Pro, billed monthly. Review the terms below and continue to payment.
          </p>
        </div>
      </div>
      <div className="auth-col wrap page-narrow">
          <div className="card" style={{ marginBottom: 20 }}>
            <p style={{ margin: 0, fontFamily: 'var(--font-mono-stack)', fontSize: 13, color: 'var(--ink-faint)' }}>
              order — jonas@acme.eu
            </p>
            <p style={{ margin: '8px 0 0', fontFamily: 'var(--font-mono-stack)', fontSize: 13 }}>
              plan: {plan.id} · price: €{plan.priceEur}/mo · included: {plan.hoursPerMonth} h/mo
            </p>
            <p style={{ margin: '8px 0 0', fontFamily: 'var(--font-mono-stack)', fontSize: 13 }}>
              cap:{' '}
              {plan.capKind === 'hard'
                ? 'hard — processing pauses at the limit, never surprise bills'
                : `metered — €${plan.overagePerHourEur}/h past the cap`}
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
              continue to payment →
            </button>
            <button
              className="btn btn-ghost"
              type="button"
              onClick={() => {
                trackCheckoutAbandoned({ reason: 'just_looking' });
                setResult('abandoned');
              }}
            >
              simulate abandon
            </button>
          </div>
          <p className="pay-note" style={{ marginTop: 14 }}>
            payments by Stripe — card details never touch mdreel servers
          </p>
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
