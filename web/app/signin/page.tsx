'use client';

import { useState, type FormEvent } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { login } from '@/lib/auth';
import { markSignedIn } from '@/lib/session';
import { Field } from '@/components/Field/Field';

export default function SignInPage() {
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSubmitting(true);
    setError(null);
    try {
      await login(email, password);
      markSignedIn(email);
      router.push('/app');
    } catch {
      setError('Wrong email or password.');
      setSubmitting(false);
    }
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-head-inner">
          <div style={{ maxWidth: '52ch' }}>
            <p className="kicker"># sign in</p>
            <h1>Sign in.</h1>
            <p className="lead">Email and password — your workspace opens straight away.</p>
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
            <Field label="password" htmlFor="password" style={{ marginBottom: 26 }}>
              <input
                id="password"
                type="password"
                required
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="your password"
              />
            </Field>
            {error ? (
              <p className="auth-error" role="alert" style={{ color: 'var(--danger, #b00020)', marginBottom: 16 }}>
                {error}
              </p>
            ) : null}
            <div className="auth-actions" style={{ marginBottom: 20 }}>
              <button className="btn btn-primary" type="submit" disabled={submitting}>
                {submitting ? 'signing in…' : 'sign in'}
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

