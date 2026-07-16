'use client';

import { useState, type FormEvent } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { markSignedIn } from '@/lib/session';
import { Field } from '@/components/Field/Field';

export default function SignInPage() {
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
      <div className="auth-col wrap page-narrow">
        <p className="sent-line">link expires in 15 minutes</p>
        <h1>Check your inbox.</h1>
        <p className="lead">We sent a magic link to {email || 'jonas@acme.eu'} — it signs you straight into your workspace.</p>
          <Link className="btn btn-primary" href="/app">
          open your workspace
          </Link>
      </div>
    );
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <p className="kicker"># sign in</p>
          <h1>Sign in.</h1>
          <p className="lead">No password — we email you a magic link.</p>
        </div>
      </div>
      <div className="auth-col wrap page-narrow">
          <form
            onSubmit={(e: FormEvent) => {
              e.preventDefault();
              submit();
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
            <button className="btn btn-primary" type="submit">
              send magic link
            </button>
          </form>
          <p className="auth-alt">
            new here? <Link href="/signup">start free — 1 hour</Link>
          </p>
      </div>
    </>
  );
}
