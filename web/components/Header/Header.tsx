'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { BrandMark } from '../BrandMark';

const NAV_LINKS = [
  { href: '/gallery', label: 'gallery' },
  { href: '/pricing', label: 'pricing' },
  { href: '/docs', label: 'docs' },
];

export function Header() {
  const pathname = usePathname();
  return (
    <header className="site-header mobile-nav-proposed">
      <div className="wrap header-inner">
        <Link className="brand" href="/" aria-label="mdreel home">
          <BrandMark />
        </Link>
        <input id="nav-open" className="nav-check" type="checkbox" />
        <label htmlFor="nav-open" className="nav-toggle" aria-label="toggle navigation" />
        <nav className="nav" aria-label="Primary">
          {NAV_LINKS.map((link) => (
            <Link key={link.href} href={link.href} className={pathname.startsWith(link.href) ? 'active' : undefined}>
              {link.label}
            </Link>
          ))}
          <Link href="/signin" className="btn btn-ghost btn-sm">
            sign in
          </Link>
          <Link className="btn btn-primary btn-sm" href="/signup">
            start free — 1 hour →
          </Link>
        </nav>
      </div>
    </header>
  );
}
