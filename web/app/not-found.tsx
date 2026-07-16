import Link from 'next/link';

export default function NotFound() {
  return (
    <section className="nf-section">
      <div className="wrap page-narrow">
        <p className="nf-status">
          GET /the-requested-path <span className="sep">·</span> <span className="code">404</span>
        </p>

        <div className="doc-card doc-card-404">
          <div className="doc-card-head">
            <span className="file">404.md</span>
            <span className="meta">text/markdown</span>
          </div>
          <div className="doc-lines">
            <div className="doc-line">
              <span className="n">1</span>
              <span className="t fence">---</span>
            </div>
            <div className="doc-line">
              <span className="n">2</span>
              <span className="t">
                <span className="key">status:</span> 404
              </span>
            </div>
            <div className="doc-line">
              <span className="n">3</span>
              <span className="t">
                <span className="key">path:</span> /the-requested-path
              </span>
            </div>
            <div className="doc-line">
              <span className="n">4</span>
              <span className="t fence">---</span>
            </div>
            <div className="doc-line">
              <span className="n">6</span>
              <span className="t h"># not found</span>
            </div>
            <div className="doc-line">
              <span className="n">7</span>
              <span className="t quote">Nothing is filed under this path.</span>
            </div>
          </div>
        </div>

        <h1 className="display-l" style={{ marginBottom: 16 }}>
          This page does not exist.
        </h1>
        <p className="lead" style={{ margin: '0 auto 36px', maxWidth: '52ch' }}>
          The link is stale, or the path was never real. Nothing was moved and nothing was deleted.
        </p>

        <p className="nf-links">
          <Link href="/">← home</Link>
          <span className="sep">·</span>
          <Link href="/gallery">gallery</Link>
          <span className="sep">·</span>
          <Link href="/docs">docs</Link>
        </p>

        <p className="nf-coda">
          the API answers the same way — RFC 7807 problem+json, <Link href="/docs">see the docs →</Link>
        </p>
      </div>
    </section>
  );
}
