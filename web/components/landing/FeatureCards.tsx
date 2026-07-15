const FEATURES = [
  {
    icon: '🖥️',
    title: (
      <>
        Spoken <span className="amp">+</span> on-screen
      </>
    ),
    body: 'Captures verbatim on-screen text — slides, UI, code — separated from the transcript so your RAG can weight them independently.',
  },
  {
    icon: '🇪🇺',
    title: 'EU data residency',
    body: 'Compute, storage and AI endpoints pinned to EU regions. Short, published subprocessor list. Honest sovereignty roadmap.',
  },
  {
    icon: '🗑️',
    title: 'Deleted by default',
    body: 'Source video is erased after processing unless you choose to retain it. Right-to-erasure endpoint, audit-logged.',
  },
  {
    icon: '📄',
    title: 'No lock-in',
    body: 'You get plain Markdown files — no proprietary retrieval stack. Bring your own RAG, Obsidian, SharePoint or vector DB.',
  },
  {
    icon: '🤖',
    title: 'Built for agents',
    body: 'A deterministic, consistent schema across every file. Designed to be consumed by LLMs and agents, not just read by humans.',
  },
  {
    icon: '🔌',
    title: 'API-first',
    body: 'First-class REST API with webhooks, per-job cost transparency, and an MCP server on the roadmap. Embed it in your pipeline.',
  },
];

export function FeatureCards() {
  return (
    <section id="features" className="section section-alt">
      <div className="wrap">
        <p className="eyebrow">Why mdreel</p>
        <h2>Plain transcription throws away half the video. We keep all of it.</h2>
        <div className="cards">
          {FEATURES.map((f, i) => (
            <article className="card" key={i}>
              <div className="card-ic">{f.icon}</div>
              <h3>{f.title}</h3>
              <p>{f.body}</p>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
