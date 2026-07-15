import Link from 'next/link';
import { BrandMark } from '../BrandMark';

export function Footer() {
  return (
    <footer className="site-footer">
      <div className="wrap footer-grid">
        <div className="footer-brand">
          <Link className="brand" href="/">
            <BrandMark gradientId="footer-g" size={24} />
            <span>mdreel</span>
          </Link>
          <p>Video to Markdown for AI knowledge bases. Processed in the EU.</p>
        </div>
        <nav className="footer-col" aria-label="Product">
          <h4>Product</h4>
          <Link href="/tool">Free tool</Link>
          <Link href="/gallery">Gallery</Link>
          <Link href="/pricing">Pricing</Link>
        </nav>
        <nav className="footer-col" aria-label="Developers">
          <h4>Developers</h4>
          <Link href="/pricing">API access</Link>
          <Link href="/gallery">llms.txt</Link>
        </nav>
        <nav className="footer-col" aria-label="Trust">
          <h4>Trust</h4>
          <Link href="/#eu">GDPR</Link>
          <Link href="/#eu">Security</Link>
          <Link href="/#eu">Subprocessors</Link>
        </nav>
      </div>
      <div className="wrap footer-bottom">
        <span>© 2026 mdreel. Built in the EU.</span>
        <span>EU data residency on Google Cloud · sovereignty roadmap published</span>
      </div>
    </footer>
  );
}
