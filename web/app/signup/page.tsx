'use client';

import { Suspense, useState, type FormEvent } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { emitSignupEvent } from '@/lib/events';
import { registerAccount } from '@/lib/auth';
import { markSignedIn, setSessionIds } from '@/lib/session';
import { TRIAL_CREDIT_HOURS } from '@/lib/pricing';
import { Field } from '@/components/Field/Field';

export default function SignupPage() {
  return (
    <Suspense fallback={null}>
      <SignupInner />
    </Suspense>
  );
}

function SignupInner() {
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [archiveHours, setArchiveHours] = useState('');
  const [monthlyHours, setMonthlyHours] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSubmitting(true);
    setError(null);
    const archive = archiveHours ? Number(archiveHours) : null;
    const monthly = monthlyHours ? Number(monthlyHours) : null;
    try {
      const account = await registerAccount({ email, password, archive_hours: archive, monthly_hours: monthly });
      setSessionIds({ tenant_id: account.tenant_id });
      markSignedIn(email);
      // Keep the METRICS.md §3 signup event in the funnel (idempotent by email server-side).
      void emitSignupEvent({ email, archive_hours: archive, monthly_hours: monthly });
      router.push('/app');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not create your account.');
      setSubmitting(false);
    }
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-head-inner">
          <div style={{ maxWidth: '52ch' }}>
          <p className="kicker"># start free</p>
          <h1>Start free — {TRIAL_CREDIT_HOURS} hour of processing.</h1>
          <p className="lead">
            Email and a password — no credit card. Your one-time 1-hour trial credit is ready the moment you sign up,
            on your own footage.
          </p>
          </div>
        </div>
      </div>
      <div className="wrap">
          <div className="auth-col">
            <form
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void submit();
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

              <Field label="password" htmlFor="password" style={{ marginBottom: 22 }}>
                <input
                  id="password"
                  type="password"
                  required
                  minLength={8}
                  autoComplete="new-password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="at least 8 characters"
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

              {error ? (
                <p className="auth-error" role="alert" style={{ color: 'var(--danger, #b00020)', marginBottom: 16 }}>
                  {error}
                </p>
              ) : null}

              <div className="auth-actions">
                <button className="btn btn-primary" type="submit" disabled={submitting}>
                  {submitting ? 'creating…' : 'create account'}
                </button>
              </div>
              <p className="auth-agree">
                By creating an account you agree to the{' '}
                <Link href="/legal/terms">Terms of Service</Link> and{' '}
                <Link href="/legal/privacy">Privacy Policy</Link>. mdreel is a business-only
                service — you confirm you are acting in the course of business.
              </p>
            </form>
            <p className="auth-alt">
              already have an account? <Link href="/signin">sign in</Link>
            </p>
            <p className="auth-coda">EU-resident by design — your account and your footage never leave the EU.</p>
          </div>
      </div>
    </>
  );
}

