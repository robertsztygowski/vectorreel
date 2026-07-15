import type { Metadata } from 'next';
import Link from 'next/link';
import { PricingCard } from '@/components/PricingCard/PricingCard';

export const metadata: Metadata = { title: 'Pricing — mdreel' };

export default function PricingPage() {
  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>Simple pricing</h1>
          <p className="lead">
            One plan for the paid product, plus a free tool for public YouTube videos. No tiers yet — that&apos;s an
            open question we&apos;re still reading the data on.
          </p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap" style={{ display: 'flex', flexWrap: 'wrap', gap: 28 }}>
          <PricingCard />
          <div className="card" style={{ flex: '1 1 320px', maxWidth: 420 }}>
            <div className="card-ic">🎁</div>
            <h3>Free YouTube tool</h3>
            <p>
              Paste any public YouTube URL and get real Markdown back in under a minute — no signup, no card, no
              limit on how many times you try it.
            </p>
            <p style={{ marginTop: 14 }}>
              <Link className="btn btn-ghost" href="/tool">
                Try the free tool
              </Link>
            </p>
          </div>
        </div>
      </div>
    </>
  );
}
