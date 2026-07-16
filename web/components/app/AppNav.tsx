'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useSearchParams } from 'next/navigation';

const BASE_LINKS = [
  { href: '/app', label: 'library', exact: true },
  { href: '/app/upload', label: 'process a video', exact: false },
];

export function AppNav() {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const usage = searchParams.get('usage') ?? 'trial';
  const showSettings = pathname.startsWith('/app/settings') || pathname.startsWith('/app/jobs') || pathname.startsWith('/app/videos');
  const links = showSettings ? [...BASE_LINKS, { href: '/app/settings', label: 'settings', exact: false }] : BASE_LINKS;

  return (
    <div className="app-bar app-header-default">
      <div className="wrap app-bar-inner">
        <nav className="app-nav" aria-label="Workspace">
          {links.map((l) => {
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
        </div>
      </div>
    </div>
  );
}
