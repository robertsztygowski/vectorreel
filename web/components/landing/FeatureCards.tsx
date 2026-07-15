const FEATURES = [
  {
    icon: '📄',
    title: 'One portable file — no lock-in',
    body: 'You get plain Markdown, not a proprietary retrieval index. Own it, version it, and drop it into your RAG, Obsidian, SharePoint or vector DB. The one thing a platform competitor structurally won\u2019t copy.',
  },
  {
    icon: '🖥️',
    title: (
      <>
        Spoken <span className="amp">vs</span> shown, separated
      </>
    ),
    body: 'What was said and what was on the screen — slides, UI, code — kept in distinct blocks so your RAG can weight them independently. Not a transcript: a knowledge-base document.',
  },
  {
    icon: '🤖',
    title: 'Built for agents',
    body: 'A deterministic schema that stays identical across every file, so hundreds of videos read the same to an agent. Designed to be cited by an LLM, not just read by a human.',
  },
  {
    icon: '🇪🇺',
    title: 'EU data residency',
    body: 'Compute, storage and AI endpoints pinned to EU regions, with an honest sovereignty roadmap. The reason your DPO says yes — not a markup.',
  },
  {
    icon: '🗑️',
    title: 'Deleted by default',
    body: 'Source video is erased after processing unless you choose to retain it. Right-to-erasure endpoint, audit-logged.',
  },
  {
    icon: '🔌',
    title: 'REST, webhooks & MCP',
    body: 'A first-class REST API with webhooks and an MCP server so an assistant can process a video from inside your IDE. Per-job cost transparency built in.',
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
