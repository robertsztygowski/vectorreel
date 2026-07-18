import type { ReactNode } from 'react';
import Link from 'next/link';
import { LEGAL_EFFECTIVE_DATE, LEGAL_ENTITY, LEGAL_VERSION } from '@/lib/legal';

export interface TocItem {
  id: string;
  label: string;
}

interface LegalLayoutProps {
  title: string;
  /** Short intro sentence under the title. */
  lead: string;
  /** Section anchors for the sticky table of contents. */
  toc: TocItem[];
  children: ReactNode;
}

/**
 * Shared shell for every /legal/* page: a page head with title + lead, a sticky table of contents,
 * and the prose column. Effective date and version come from lib/legal.ts so all six pages stay in
 * lock-step. Server component — no client JS.
 */
export function LegalLayout({ title, lead, toc, children }: LegalLayoutProps) {
  return (
    <>
      <div className="page-head" style={{ padding: 0 }}>
        <div className="wrap" style={{ padding: '64px 32px 40px' }}>
          <p className="kicker"># legal</p>
          <h1
            style={{
              fontSize: 'clamp(30px, 3.4vw, 44px)',
              lineHeight: 1.05,
              letterSpacing: '-0.014em',
              fontWeight: 500,
              fontVariationSettings: "'opsz' 64",
              margin: '0 0 16px',
            }}
          >
            {title}
          </h1>
          <p className="lead" style={{ maxWidth: '60ch' }}>
            {lead}
          </p>
          <p className="legal-meta">
            <span>
              Version {LEGAL_VERSION} · Effective{' '}
              <time dateTime={LEGAL_EFFECTIVE_DATE}>{LEGAL_EFFECTIVE_DATE}</time>
            </span>
          </p>
        </div>
      </div>

      <div className="wrap docs-grid">
        <nav className="docs-toc" aria-label="On this page">
          <div className="docs-toc-inner">
            <span className="toc-label">contents</span>
            {toc.map((item) => (
              <a key={item.id} href={`#${item.id}`}>
                {item.label}
              </a>
            ))}
            <span className="toc-label" style={{ marginTop: 8 }}>
              legal pack
            </span>
            <Link href="/legal/terms">terms</Link>
            <Link href="/legal/privacy">privacy</Link>
            <Link href="/legal/imprint">imprint</Link>
            <Link href="/legal/dpa">dpa</Link>
            <Link href="/legal/subprocessors">subprocessors</Link>
            <Link href="/legal/acceptable-use">acceptable use</Link>
          </div>
        </nav>

        <div className="docs-main docs docs-body legal">
          {children}
          <p className="legal-provider">
            Published by <strong>{LEGAL_ENTITY.name}</strong>, {LEGAL_ENTITY.address} · VAT{' '}
            {LEGAL_ENTITY.vat} · <a href={`mailto:${LEGAL_ENTITY.email}`}>{LEGAL_ENTITY.email}</a>.
            Version {LEGAL_VERSION}, effective {LEGAL_EFFECTIVE_DATE}.
          </p>
        </div>
      </div>
    </>
  );
}
