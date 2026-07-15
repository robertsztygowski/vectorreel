'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { BrandMark } from '../BrandMark';

const NAV_LINKS = [
  { href: '/gallery', label: 'Gallery' },
  { href: '/pricing', label: 'Pricing' },
  { href: '/docs', label: 'Docs' },
];

export function Header() {
  const pathname = usePathname();
  return (
    <header className="site-header">
      <div className="wrap header-inner">
        <Link className="brand" href="/" aria-label="mdreel home">
          <BrandMark />
          <span>mdreel</span>
        </Link>
        <nav className="nav" aria-label="Primary">
          {NAV_LINKS.map((link) => (
            <Link key={link.href} href={link.href} className={pathname.startsWith(link.href) ? 'active' : undefined}>
              {link.label}
            </Link>
          ))}
          <Link href="/signin">Sign in</Link>
          <Link className="btn btn-ghost" href="/signup">
            Get started
          </Link>
        </nav>
      </div>
    </header>
  );
}
