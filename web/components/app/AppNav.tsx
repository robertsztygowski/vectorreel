'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { usePathname } from 'next/navigation';
import { useSearchParams } from 'next/navigation';
import { useEffect, useState } from 'react';
import { getEmail, isSignedIn, signOut } from '@/lib/session';

const LINKS = [
  { href: '/app', label: 'Library', exact: true },
  { href: '/app/upload', label: 'Process a video', exact: false },
  { href: '/app/settings', label: 'Settings', exact: false },
];

export function AppNav() {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const router = useRouter();
  const [email, setEmail] = useState<string | null>(null);
  const [signedIn, setSignedIn] = useState(true);
  const usage = searchParams.get('usage') ?? 'trial';

  useEffect(() => {
    setEmail(getEmail());
    setSignedIn(isSignedIn());
  }, []);

  return (
    <div className="app-bar app-header-proposed">
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
        <div style={{ display: 'flex', alignItems: 'center', gap: 14, flexWrap: 'wrap' }}>
          {usage === 'at-cap' ? (
            <span className="usage usage-cap">
              <b>25 / 25 h — processing paused.</b> No overage will be billed.
              <Link className="btn btn-primary btn-sm" href="/pricing">
                upgrade to continue
              </Link>
            </span>
          ) : (
            <span className="usage">
              <span className="usage-meter">
                <i style={{ width: usage === 'plan' ? '70%' : '20%' }} />
              </span>
              {usage === 'plan' ? '17.4 / 25 h used · pro' : '0.2 / 1.0 h trial credit left'}
            </span>
          )}
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
