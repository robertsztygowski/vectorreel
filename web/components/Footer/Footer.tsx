import Link from 'next/link';
import { BrandMark } from '../BrandMark';

export function Footer() {
  return (
    <footer className="site-footer footer-split-proposed">
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
            <Link href="/#eu">GDPR</Link>
            <Link href="/#eu">Security</Link>
            <Link href="/#eu">Subprocessors</Link>
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
