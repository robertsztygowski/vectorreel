import Link from 'next/link';

export default function GalleryVideoNotFound() {
  return (
    <section className="nf-section">
      <div className="wrap page-narrow">
        <p className="nf-status">
          GET /gallery/rec_7f2k4q <span className="sep">·</span> <span className="code">404</span>
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
                <span className="key">path:</span> /gallery/rec_7f2k4q
              </span>
            </div>
            <div className="doc-line">
              <span className="n">4</span>
              <span className="t fence">---</span>
            </div>
            <div className="doc-line">
              <span className="n">5</span>
              <span className="t">&nbsp;</span>
            </div>
            <div className="doc-line">
              <span className="n">6</span>
              <span className="t h"># Not found</span>
            </div>
            <div className="doc-line">
              <span className="n">7</span>
              <span className="t quote">No specimen is filed under this path.</span>
            </div>
          </div>
        </div>

        <h1 className="display-l" style={{ marginBottom: 16 }}>
          This specimen does not exist.
        </h1>
        <p className="lead" style={{ margin: '0 auto 36px', maxWidth: '52ch' }}>
          It may have been removed along with its licence — the gallery only shows recordings we can attribute.
        </p>
        <p className="nf-links">
          <Link href="/gallery">← back to the gallery</Link>
        </p>
      </div>
    </section>
  );
}
