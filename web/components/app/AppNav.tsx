'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { usePathname } from 'next/navigation';
import { useEffect, useState } from 'react';
import { getEmail, isSignedIn, signOut } from '@/lib/session';
import { TRIAL_CREDIT_HOURS } from '@/lib/pricing';

const LINKS = [
  { href: '/app', label: 'Library', exact: true },
  { href: '/app/upload', label: 'Process a video', exact: false },
];

export function AppNav() {
  const pathname = usePathname();
  const router = useRouter();
  const [email, setEmail] = useState<string | null>(null);
  const [signedIn, setSignedIn] = useState(true);

  useEffect(() => {
    setEmail(getEmail());
    setSignedIn(isSignedIn());
  }, []);

  return (
    <div className="app-bar">
      <div className="wrap app-bar-inner">
        <nav className="app-nav" aria-label="Workspace">
          {LINKS.map((l) => {
            const active = l.exact ? pathname === l.href : pathname.startsWith(l.href);
            return (
              <Link key={l.href} href={l.href} className={active ? 'active' : undefined}>
                {l.label}
              </Link>
            );
          })}
        </nav>
        <div className="app-bar-right">
          <span className="trial-pill">🎁 {TRIAL_CREDIT_HOURS} h trial credit</span>
          {signedIn ? (
            <>
              {email && <span className="app-email">{email}</span>}
              <button
                type="button"
                className="btn btn-ghost"
                onClick={() => {
                  signOut();
                  router.push('/');
                }}
              >
                Sign out
              </button>
            </>
          ) : (
            <Link className="btn btn-ghost" href="/signin">
              Sign in
            </Link>
          )}
        </div>
      </div>
    </div>
  );
}
