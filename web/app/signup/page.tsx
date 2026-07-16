'use client';

import { Suspense, useState, type FormEvent } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { getAbArm, getFirstTouch } from '@/lib/attribution';
import { trackSignup } from '@/lib/events';
import { markSignedIn } from '@/lib/session';
import { TRIAL_CREDIT_HOURS } from '@/lib/pricing';
import { Field } from '@/components/Field/Field';

// useSearchParams() forces a CSR bailout during prerender; the Suspense wrapper is required
// for `next build` — same pattern as checkout/page.tsx.
export default function SignupPage() {
  return (
    <Suspense fallback={null}>
      <SignupInner />
    </Suspense>
  );
}

function SignupInner() {
  const searchParams = useSearchParams();
  const [email, setEmail] = useState('');
  const [archiveHours, setArchiveHours] = useState('');
  const [monthlyHours, setMonthlyHours] = useState('');
  const [submitted, setSubmitted] = useState(false);

  function submit(skip: boolean) {
    // Assembles the exact future `tenants` row shape (ARCHITECTURE §6) so the forward path to
    // payment_succeeded is proven now, even though there's no backend yet to persist it to.
    const firstTouch = getFirstTouch();
    console.log('[future tenant row]', {
      first_utm_source: firstTouch?.utm_source ?? null,
      first_utm_medium: firstTouch?.utm_medium ?? null,
      first_utm_campaign: firstTouch?.utm_campaign ?? null,
      first_utm_term: firstTouch?.utm_term ?? null,
      first_referrer: firstTouch?.referrer ?? null,
      ab_arm: getAbArm(),
    });

    trackSignup({
      archive_hours: skip || !archiveHours ? null : Number(archiveHours),
      monthly_hours: skip || !monthlyHours ? null : Number(monthlyHours),
    });
    markSignedIn(email);
    setSubmitted(true);
  }

  const showSent = submitted || searchParams.get('state') === 'sent';

  if (showSent) {
    return (
      <div className="wrap">
        <div className="auth-col">
          <p className="sent-line">
            link expires in 15 minutes — <span className="ok-text">your 1 h trial credit is ready</span>
          </p>
          <h1>Check your inbox.</h1>
          <p className="lead">
            We sent a magic link to {email || 'jonas@acme.eu'} — it signs you straight into your workspace.
          </p>
          <Link className="btn btn-primary" href="/app">
            open your workspace
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
          <p className="kicker"># start free</p>
          <h1>Start free — {TRIAL_CREDIT_HOURS} hour of processing.</h1>
          <p className="lead">
            No password, no credit card — we email a magic link and your one-time 1-hour trial credit is waiting, on
            your own footage.
          </p>
          </div>
        </div>
      </div>
      <div className="wrap">
          <div className="auth-col">
            <form
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                submit(false);
              }}
            >
              <Field label="work email" htmlFor="email" style={{ marginBottom: 22 }}>
                <input
                  id="email"
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@company.com"
                />
              </Field>

              <Field
                label="roughly how much video do you have? (optional)"
                hint="helps us understand whether this is an archive to process or an ongoing stream."
                style={{ marginBottom: 30 }}
              >
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                  <input
                    type="number"
                    min={0}
                    placeholder="~ h in the archive"
                    value={archiveHours}
                    onChange={(e) => setArchiveHours(e.target.value)}
                  />
                  <input
                    type="number"
                    min={0}
                    placeholder="~ h added per month"
                    value={monthlyHours}
                    onChange={(e) => setMonthlyHours(e.target.value)}
                  />
                </div>
              </Field>

              <div className="auth-actions">
                <button className="btn btn-primary" type="submit">
                  send magic link
                </button>
                <button className="btn btn-ghost" type="button" onClick={() => submit(true)}>
                  skip &amp; send link
                </button>
              </div>
            </form>
            <p className="auth-alt">
              already have an account? <Link href="/signin">sign in</Link>
            </p>
            <p className="auth-coda">the magic link is the only credential — nothing to leak, nothing to rotate.</p>
          </div>
      </div>
    </>
  );
}
