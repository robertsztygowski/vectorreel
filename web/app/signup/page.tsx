'use client';

import { useState, type FormEvent } from 'react';
import Link from 'next/link';
import { getAbArm, getFirstTouch } from '@/lib/attribution';
import { trackSignup } from '@/lib/events';
import { markSignedIn } from '@/lib/session';
import { TRIAL_CREDIT_HOURS } from '@/lib/pricing';
import { Field } from '@/components/Field/Field';

export default function SignupPage() {
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

  if (submitted) {
    return (
      <div className="page-body">
        <div className="wrap page-narrow">
          <h1>Check your inbox</h1>
          <p className="lead">
            We sent a magic link to {email || 'your email'} (mock — no email actually sent this phase). Your{' '}
            {TRIAL_CREDIT_HOURS}-hour trial credit is ready.
          </p>
          <Link className="btn btn-primary" href="/app">
            Open your workspace
          </Link>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>Start free — {TRIAL_CREDIT_HOURS} hour of processing</h1>
          <p className="lead">
            No password, no credit card. We&apos;ll email you a magic link, and your one-time {TRIAL_CREDIT_HOURS}-hour
            trial credit is waiting on your own footage.
          </p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap page-narrow">
          <form
            onSubmit={(e: FormEvent) => {
              e.preventDefault();
              submit(false);
            }}
          >
            <Field label="Work email" htmlFor="email">
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
              label="Roughly how much video do you have? (optional)"
              hint="Helps us understand whether this is an archive to process or an ongoing stream."
            >
              <div style={{ display: 'flex', gap: 12 }}>
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

            <div style={{ display: 'flex', gap: 12, marginTop: 8 }}>
              <button className="btn btn-primary" type="submit">
                Send magic link
              </button>
              <button className="btn btn-ghost" type="button" onClick={() => submit(true)}>
                Skip &amp; send link
              </button>
            </div>
          </form>
          <p className="micro" style={{ marginTop: 20 }}>
            Already have an account? <Link href="/signin">Sign in</Link>
          </p>
        </div>
      </div>
    </>
  );
}
