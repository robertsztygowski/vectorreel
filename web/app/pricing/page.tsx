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
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>Simple, hours-based pricing</h1>
          <p className="lead">
            You pay for hours of video processed — and you get a portable Markdown file your agent owns, with no
            retrieval stack to lock you in. Every plan is EU-only, source deleted after processing.
          </p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap">
          <div className="trial-banner">
            <div>
              <p className="trial-title">
                Start with {TRIAL_CREDIT_HOURS} hour of processing, free
              </p>
              <p className="trial-sub">
                One-time trial credit at signup — no credit card. Run it on your own footage and see the file before
                you pay.
              </p>
            </div>
            <Link className="btn btn-primary" href="/signup">
              Start free — {TRIAL_CREDIT_HOURS} hour
            </Link>
          </div>

          <div className="pricing-grid">
            {plans.map((plan) => (
              <PricingCard key={plan.id} plan={plan} />
            ))}
          </div>

          <p className="pricing-foot">
            Bigger archive or an ongoing pipeline? Volume, API pay-as-you-go and self-hosted options come after we
            learn how teams actually use it. <Link href="/docs">See the API &amp; MCP docs →</Link>
          </p>
        </div>
      </div>
    </>
  );
}
