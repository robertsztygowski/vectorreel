export function Steps() {
  return (
    <section id="how" className="section">
      <div className="wrap">
        <p className="eyebrow">How it works</p>
        <h2>Three steps from a raw recording to RAG-ready Markdown.</h2>
        <div className="steps">
          <div className="step">
            <span className="step-num">1</span>
            <h3>Upload</h3>
            <p>Drop a file via the web app or push it through the REST API. Long videos are chunked automatically.</p>
          </div>
          <div className="step">
            <span className="step-num">2</span>
            <h3>Analyze</h3>
            <p>
              We read audio <em>and</em> the screen — slides, code, UI labels, diagrams — with EU-region AI, sampling
              smartly to keep cost low.
            </p>
          </div>
          <div className="step">
            <span className="step-num">3</span>
            <h3>Get Markdown</h3>
            <p>One consistent, timestamped document per video — with YAML frontmatter. Yours to drop into any repo, vector DB or wiki.</p>
          </div>
        </div>
      </div>
    </section>
  );
}
