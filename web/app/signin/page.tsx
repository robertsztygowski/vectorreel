'use client';

import { Suspense, useState, type FormEvent } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { markSignedIn } from '@/lib/session';
import { Field } from '@/components/Field/Field';

// useSearchParams() forces a CSR bailout during prerender; the Suspense wrapper is required
// for `next build` — same pattern as checkout/page.tsx.
export default function SignInPage() {
  return (
    <Suspense fallback={null}>
      <SignInInner />
    </Suspense>
  );
}

function SignInInner() {
  const searchParams = useSearchParams();
  const [email, setEmail] = useState('');
  const [submitted, setSubmitted] = useState(false);

  function submit() {
    // Magic-link sign-in (mock — no email sent, no backend this phase).
    markSignedIn(email);
    setSubmitted(true);
  }

  const showSent = submitted || searchParams.get('state') === 'sent';

  if (showSent) {
    return (
      <div className="wrap">
        <div className="auth-col">
          <p className="sent-line">link expires in 15 minutes</p>
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
            <p className="kicker"># sign in</p>
            <h1>Sign in.</h1>
            <p className="lead">No password — we email you a magic link.</p>
          </div>
        </div>
      </div>
      <div className="wrap">
        <div className="auth-col">
          <form
            onSubmit={(e: FormEvent) => {
              e.preventDefault();
              submit();
            }}
          >
            <Field label="work email" htmlFor="email" style={{ marginBottom: 26 }}>
              <input
                id="email"
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@company.com"
              />
            </Field>
            <div className="auth-actions" style={{ marginBottom: 20 }}>
              <button className="btn btn-primary" type="submit">
                send magic link
              </button>
            </div>
          </form>
          <p className="auth-alt">
            new here? <Link href="/signup">start free — 1 hour</Link>
          </p>
        </div>
      </div>
    </>
  );
}
