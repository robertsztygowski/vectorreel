const STEPS = [
  {
    num: '01',
    label: 'upload',
    body: (
      <>
        <strong>Send the video</strong> through the web app or the REST API. Long recordings are chunked automatically.
      </>
    ),
  },
  {
    num: '02',
    label: 'analyze',
    body: (
      <>
        <strong>We read the audio — and the screen.</strong> UI text, code, slides and diagrams are captured alongside
        what the speaker says, on EU-region AI.
      </>
    ),
  },
  {
    num: '03',
    label: 'markdown',
    body: (
      <>
        <strong>Get one timestamped document</strong> per video — consistent structure, YAML frontmatter. Drop it into
        any repo, vector DB or wiki.
      </>
    ),
  },
];

export function Steps() {
  return (
    <section id="how" className="section">
      <div className="wrap steps-grid">
        <div className="steps-title">
          <p className="eyebrow">## how it works</p>
          <h2>Video in. One document out.</h2>
        </div>
        <div className="steps-list">
          {STEPS.map((s) => (
            <div className="step" key={s.num}>
              <span className="step-num">
                {s.num}
                <i> /</i> {s.label}
              </span>
              <p>{s.body}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
