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
    <header className="site-header">
      <div className="wrap header-inner">
        <Link className="brand" href="/" aria-label="mdreel home">
          <BrandMark />
        </Link>
        <nav className="nav" aria-label="Primary">
          {NAV_LINKS.map((link) => (
            <Link key={link.href} href={link.href} className={pathname.startsWith(link.href) ? 'active' : undefined}>
              {link.label}
            </Link>
          ))}
          <Link href="/signin" className="btn-ghost" style={{ height: 34, padding: '0 14px' }}>
            sign in
          </Link>
          <Link className="btn-mini" href="/signup">
            get started →
          </Link>
        </nav>
      </div>
    </header>
  );
}
