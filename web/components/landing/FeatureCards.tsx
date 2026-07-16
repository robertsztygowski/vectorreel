import type { ReactNode } from 'react';

function Icon({ paths }: { paths: string[] }) {
  return (
    <svg width={22} height={22} viewBox="0 0 22 22" fill="none" stroke="currentColor" strokeWidth={1.4} strokeLinecap="square">
      {paths.map((d, i) => (
        <path key={i} d={d} />
      ))}
    </svg>
  );
}

const CLAIMS: { idx: string; paths: string[]; title: ReactNode; body: string }[] = [
  {
    idx: '01',
    paths: ['M6 2.5h7l3.5 3.5v13.5h-10.5z', 'M13 2.5v3.5h3.5', 'M8.5 11h5', 'M8.5 14.5h5'],
    title: 'One portable file, no lock-in',
    body: 'Plain Markdown — not a proprietary retrieval index. The thing platform vendors structurally won’t give you.',
  },
  {
    idx: '02',
    paths: ['M2.5 4h7.5v14H2.5z', 'M12 4h7.5v14H12z', 'M5 8h2.5', 'M14.5 8h2.5', 'M14.5 11.5h2.5'],
    title: 'Spoken vs shown, separated',
    body: 'Distinct blocks for what was said and what was on screen, so your RAG can weight them differently.',
  },
  {
    idx: '03',
    paths: ['M7 4.5 3 11l4 6.5', 'M15 4.5 19 11l-4 6.5', 'M12.5 4 9.5 18'],
    title: 'Built for agents',
    body: 'One deterministic schema across thousands of files — designed to be cited by an LLM, not skimmed by a human.',
  },
  {
    idx: '04',
    paths: ['M11 2.5 18.5 6v5c0 4.5-3 7.5-7.5 9-4.5-1.5-7.5-4.5-7.5-9V6z', 'M7.5 11l2.5 2.5 4.5-4.5'],
    title: 'EU data residency',
    body: 'The reason your DPO says yes — not a markup. Processing and storage stay in-region, under a published DPA.',
  },
  {
    idx: '05',
    paths: ['M4.5 6.5h13', 'M8 6.5V4.5h6v2', 'M6.5 6.5 7.5 18.5h7l1-12', 'M9.5 9.5v6', 'M12.5 9.5v6'],
    title: 'Deleted by default',
    body: 'Source video is erased after processing unless you choose to keep it. Right-to-erasure endpoint, audit-logged.',
  },
  {
    idx: '06',
    paths: ['M6.5 3.5v4', 'M13.5 3.5v4', 'M4.5 7.5h11v3a5.5 5.5 0 0 1-11 0z', 'M10 16v3'],
    title: 'Plugs into your stack',
    body: 'REST API, webhooks and an MCP server, with per-job cost transparency built in — output drops into any repo, vector DB or wiki.',
  },
];

export function FeatureCards() {
  return (
    <section id="features" className="section">
      <div className="claims-head wrap">
        <p className="eyebrow">## why it holds up</p>
        <h2>Not a transcript. A knowledge-base document.</h2>
      </div>
      <div className="claims">
        <div className="claims-grid">
          {CLAIMS.map((c) => (
            <article className="claim" key={c.idx}>
              <div className="claim-top">
                <span className="claim-ic">
                  <Icon paths={c.paths} />
                </span>
                <span className="claim-idx">{c.idx}</span>
              </div>
              <h3>{c.title}</h3>
              <p>{c.body}</p>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
