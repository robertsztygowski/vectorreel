'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useRouter } from 'next/navigation';
import { BrandMark } from '../BrandMark';
import { getEmail, signOut } from '@/lib/session';
import { logout } from '@/lib/auth';

const NAV_LINKS = [
  { href: '/#features', label: 'product' },
  { href: '/pricing', label: 'pricing' },
  { href: '/docs', label: 'docs' },
  { href: '/gallery', label: 'collections' },
];

export function Header() {
  const pathname = usePathname();
  const router = useRouter();
  const inApp = pathname.startsWith('/app');
  const [email, setEmailState] = useState<string | null>(null);

  useEffect(() => {
    setEmailState(getEmail());
  }, [pathname]);

  async function handleSignOut() {
    await logout();
    signOut();
    router.push('/');
  }

  if (inApp) {
    return (
      <header className="site-header app-shell-header">
        <div className="wrap header-inner app-shell-header-inner">
          <Link className="brand app-brand" href="/" aria-label="mdreel home">
            <BrandMark />
          </Link>
          <nav className="app-shell-nav" aria-label="Primary">
            <Link href="/docs">docs</Link>
            <span className="app-email">{email ?? 'your workspace'}</span>
            <button
              type="button"
              className="btn btn-ghost btn-sm app-shell-signout"
              onClick={() => void handleSignOut()}
            >
              sign out
            </button>
          </nav>
        </div>
      </header>
    );
  }

  return (
    <header className="site-header mobile-nav-default">
      <div className="wrap header-inner">
        <Link className="brand" href="/" aria-label="mdreel home">
          <BrandMark />
        </Link>
        <input id="nav-open" className="nav-check" type="checkbox" />
        <label htmlFor="nav-open" className="nav-toggle" aria-label="toggle navigation" />
        <nav className="nav" aria-label="Primary">
          {NAV_LINKS.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={link.href === '/#features' ? (pathname === '/' ? 'active' : undefined) : pathname.startsWith(link.href) ? 'active' : undefined}
            >
              {link.label}
            </Link>
          ))}
          <Link href="/signin" className="btn btn-ghost btn-sm header-signin">
            sign in
          </Link>
          <Link className="btn btn-primary btn-sm header-cta" href="/signup">
            start free — 1 hour →
          </Link>
        </nav>
      </div>
    </header>
  );
}
