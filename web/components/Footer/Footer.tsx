'use client';

import { usePathname } from 'next/navigation';
import Link from 'next/link';
import { BrandMark } from '../BrandMark';

export function Footer() {
  const pathname = usePathname();
  const inApp = pathname.startsWith('/app');
  const isLanding = pathname === '/';

  if (inApp) return null;

  if (!isLanding) {
    return (
      <footer className="site-footer footer-split-default">
        <div className="wrap footer-bottom footer-bottom-only">
          <span>© 2026 mdreel. Built in the EU.</span>
          <nav className="footer-legal-inline" aria-label="Legal">
            <Link href="/legal/terms">Terms</Link>
            <Link href="/legal/privacy">Privacy</Link>
            <Link href="/legal/imprint">Imprint</Link>
          </nav>
          <span>EU data residency on Google Cloud · sovereignty roadmap published</span>
        </div>
      </footer>
    );
  }

  return (
    <footer className="site-footer footer-split-default">
      <div className="wrap footer-inner">
        <div className="footer-grid">
          <div className="footer-brand">
            <Link className="brand" href="/">
              <BrandMark />
            </Link>
            <p>The portable Markdown file your AI agents can cite — from the video they can&apos;t see. Processed in the EU.</p>
          </div>
          <nav className="footer-col" aria-label="Product">
            <h4>product</h4>
            <Link href="/gallery">Gallery</Link>
            <Link href="/pricing">Pricing</Link>
            <Link href="/signup">Get started</Link>
          </nav>
          <nav className="footer-col" aria-label="Developers">
            <h4>developers</h4>
            <Link href="/docs">API &amp; webhooks</Link>
            <Link href="/docs#mcp">MCP server</Link>
            <Link href="/docs#llms-txt">llms.txt</Link>
          </nav>
          <nav className="footer-col" aria-label="Trust">
            <h4>trust</h4>
            <Link href="/legal/terms">Terms of Service</Link>
            <Link href="/legal/privacy">Privacy Policy</Link>
            <Link href="/legal/imprint">Imprint</Link>
            <Link href="/legal/dpa">DPA</Link>
            <Link href="/legal/subprocessors">Subprocessors</Link>
          </nav>
        </div>
        <div className="footer-bottom">
          <span>© 2026 mdreel. Built in the EU.</span>
          <span>EU data residency on Google Cloud · sovereignty roadmap published</span>
        </div>
      </div>
    </footer>
  );
}
