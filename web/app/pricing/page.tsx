import type { Metadata } from 'next';
import Link from 'next/link';
import { PricingCard } from '@/components/PricingCard/PricingCard';
import { TRIAL_CREDIT_HOURS, visiblePlans } from '@/lib/pricing';
import { SHOW_STARTER_PLAN } from '@/lib/flags';

export const metadata: Metadata = { title: 'Pricing — mdreel' };

export default function PricingPage() {
  const plans = visiblePlans(SHOW_STARTER_PLAN);
  return (
    <>
      <div className="page-head" style={{ padding: 0 }}>
        <div className="wrap" style={{ padding: '72px 32px 56px' }}>
          <p className="kicker"># pricing</p>
          <h1
            style={{
              maxWidth: '20ch',
              fontSize: 'clamp(34px, 4vw, 52px)',
              lineHeight: 1.04,
              letterSpacing: '-0.015em',
              fontWeight: 500,
              fontVariationSettings: "'opsz' 68",
              margin: '0 0 20px',
            }}
          >
            Simple, hours-based pricing.
          </h1>
          <p className="lead" style={{ maxWidth: '52ch' }}>
            You pay for hours of video processed. You get a portable, timestamped Markdown document per video —
            processed EU-only, source deleted afterwards.
          </p>
        </div>
      </div>

      <div className="trial-line">
        <div className="wrap trial-line-inner">
          <p>
            <b>trial:</b> {TRIAL_CREDIT_HOURS} hour free at signup · no credit card — process a real video and see the
            file before you pay
          </p>
          <Link className="btn-mini" href="/signup" style={{ height: 42, padding: '0 20px', fontSize: 13 }}>
            start free — {TRIAL_CREDIT_HOURS} hour
          </Link>
        </div>
      </div>

      <div className="section">
        <div className="wrap" style={{ padding: '64px 32px 72px' }}>
          <div className="pricing-grid">
            {plans.map((plan) => (
              <PricingCard key={plan.id} plan={plan} />
            ))}
          </div>
          <p className="pricing-foot">
            ≈ €6 per hour of structured video on Pro. Volume tiers, API pay-as-you-go and a self-hosted edition come
            later — after we learn how teams actually use it. <Link href="/docs">Read the docs →</Link>
          </p>
        </div>
      </div>
    </>
  );
}
