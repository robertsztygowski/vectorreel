'use client';

import { useState, type FormEvent } from 'react';
import Link from 'next/link';
import { markSignedIn } from '@/lib/session';
import { Field } from '@/components/Field/Field';

export default function SignInPage() {
  const [email, setEmail] = useState('');
  const [submitted, setSubmitted] = useState(false);

  function submit() {
    // Magic-link sign-in (mock — no email sent, no backend this phase).
    markSignedIn(email);
    setSubmitted(true);
  }

  if (submitted) {
    return (
      <div className="page-body">
        <div className="wrap page-narrow">
          <h1>Check your inbox</h1>
          <p className="lead">
            We sent a sign-in link to {email || 'your email'} (mock — no email actually sent this phase).
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
          <h1>Sign in</h1>
          <p className="lead">No password — we&apos;ll email you a magic link.</p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap page-narrow">
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
              Send magic link
            </button>
          </form>
          <p className="micro" style={{ marginTop: 20 }}>
            New here? <Link href="/signup">Start free — 1 hour</Link>
          </p>
        </div>
      </div>
    </>
  );
}
