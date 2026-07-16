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
  const orderDate = new Date().toISOString().slice(0, 10);

  if (effectiveResult === 'succeeded') {
    return (
      <div className="wrap">
        <div className="auth-col">
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
      </div>
    );
  }

  if (effectiveResult === 'abandoned') {
    return (
      <div className="wrap">
        <div className="auth-col">
          <p className="sent-line">nothing was charged</p>
          <h1>Checkout abandoned.</h1>
          <p className="lead">Your workspace, plan and trial balance are exactly as you left them.</p>
          <Link className="btn btn-ghost" href="/pricing">
            ← back to pricing
          </Link>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-head-inner">
          <div style={{ maxWidth: '52ch' }}>
            <p className="kicker"># checkout</p>
            <h1>Order summary.</h1>
            <p className="lead">Pro, billed monthly. Review the terms below and continue to payment.</p>
          </div>
        </div>
      </div>
      <div className="wrap">
        <div className="auth-col">
          <div className="doc-card order-card">
            <div className="doc-card-head">
              <span className="file">order — jonas@acme.eu</span>
              <span className="meta">{orderDate}</span>
            </div>
            <div className="order-grid">
              <span className="k">plan:</span>
              <span className="v-strong">{plan.id}</span>
              <span className="k">price:</span>
              <span>€{plan.priceEur} / mo</span>
              <span className="k">included:</span>
              <span>{plan.hoursPerMonth} h of video / month</span>
              <span className="k">cap:</span>
              <span>
                {plan.capKind === 'hard'
                  ? 'hard — processing pauses at the limit, never surprise bills'
                  : `metered — €${plan.overagePerHourEur}/h past the cap`}
              </span>
              <span className="k">billing:</span>
              <span>monthly — cancel any time</span>
              <span className="k">vat:</span>
              <span>added at checkout</span>
            </div>
          </div>
          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', marginBottom: 14 }}>
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
          <p className="pay-note">payments by Stripe — card details never touch mdreel servers</p>
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
