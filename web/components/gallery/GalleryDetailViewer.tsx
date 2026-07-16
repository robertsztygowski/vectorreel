'use client';

import { useMemo, useState } from 'react';
import Link from 'next/link';
import { CodeCard } from '@/components/CodeCard/CodeCard';
import type { LicenceBlock, ParsedMarkdown } from '@/lib/corpus';

function tsToSeconds(ts: string): number {
  const m = /\[(\d{2}):(\d{2}):(\d{2})\]/.exec(ts);
  if (!m) return 0;
  return Number(m[1]) * 3600 + Number(m[2]) * 60 + Number(m[3]);
}

function secondsToTs(seconds: number): string {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = Math.floor(seconds % 60);
  return `[${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}]`;
}

export function GalleryDetailViewer({
  videoId,
  title,
  duration,
  channel,
  date,
  parsed,
  raw,
  filename,
  licence,
}: {
  videoId: string;
  title: string;
  duration: string;
  channel: string;
  date: string;
  parsed: ParsedMarkdown;
  raw: string;
  filename: string;
  licence: LicenceBlock;
}) {
  const [playerOn, setPlayerOn] = useState(false);
  const [startAt, setStartAt] = useState(0);
  const [tab, setTab] = useState<'rendered' | 'raw'>('rendered');

  const frontmatterLine = useMemo(() => {
    const tags = parsed.frontmatter.tags?.join(', ') ?? '';
    return `title: "${parsed.frontmatter.title}" · duration: ${duration} · lang: ${parsed.frontmatter.language} · tags: [${tags}]`;
  }, [parsed.frontmatter, duration]);

  return (
    <>
      <section className="page-head">
        <div className="wrap" style={{ paddingTop: 36, paddingBottom: 40 }}>
          <p className="micro" style={{ marginBottom: 14 }}>
            <Link href="/gallery">← gallery</Link>
          </p>
          <p className="eyebrow" style={{ marginBottom: 12 }}>
            ## specimen
          </p>
          <h1 className="display-m" style={{ marginBottom: 12, maxWidth: '28ch' }}>
            {title}
          </h1>
          <p className="micro">
            {channel} · {duration} · processed {date} · {filename}
          </p>
        </div>
      </section>

      <section className="page-body" style={{ paddingTop: 40 }}>
        <div className="wrap gallery-detail">
          <div className="gallery-video-col">
            <div className="gallery-sticky">
              {playerOn ? (
                <iframe
                  src={`https://www.youtube-nocookie.com/embed/${videoId}?start=${startAt}&autoplay=1&rel=0`}
                  title={title}
                  style={{ display: 'block', width: '100%', aspectRatio: '16/9', border: 0, borderRadius: 'var(--radius)' }}
                  allow="autoplay; encrypted-media; picture-in-picture"
                  allowFullScreen
                />
              ) : (
                <button className="facade" type="button" onClick={() => setPlayerOn(true)}>
                  <span className="facade-play" />
                  <p className="facade-note">no third-party request is made until you press play</p>
                </button>
              )}
              <p className="micro" style={{ marginTop: 12 }}>
                {playerOn
                  ? `playing from ${secondsToTs(startAt)} — click any timestamp in the document to jump the player`
                  : 'tip: timestamps in the document seek this player to the exact moment'}
              </p>

              <div className="attribution">
                <p className="eyebrow">&gt; source &amp; licence</p>
                <p>{licence.attribution}</p>
                <p>
                  <span className="k">source:</span>{' '}
                  <a href={licence.originalVideoUrl} target="_blank" rel="noopener noreferrer">
                    watch the original on YouTube ↗
                  </a>
                </p>
                <p>
                  <span className="k">licence:</span> {licence.licenceLine}
                </p>
              </div>
            </div>
          </div>

          <div className="gallery-doc-col">
            <div className="viewer">
              <div className="viewer-bar">
                <div style={{ display: 'flex', alignItems: 'center', gap: 20 }}>
                  <span className="mono" style={{ fontSize: 12.5, fontWeight: 500 }}>
                    {filename}
                  </span>
                  <div className="viewer-tabs" style={{ display: 'flex', alignSelf: 'stretch' }}>
                    <a href="#" className={tab === 'rendered' ? 'active' : ''} onClick={(e) => { e.preventDefault(); setTab('rendered'); }}>
                      rendered
                    </a>
                    <a href="#" className={tab === 'raw' ? 'active' : ''} onClick={(e) => { e.preventDefault(); setTab('raw'); }}>
                      raw
                    </a>
                  </div>
                </div>
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  <button
                    className="btn btn-primary btn-sm"
                    type="button"
                    onClick={() => {
                      const blob = new Blob([raw], { type: 'text/markdown' });
                      const url = URL.createObjectURL(blob);
                      const a = document.createElement('a');
                      a.href = url;
                      a.download = filename;
                      a.click();
                      URL.revokeObjectURL(url);
                    }}
                  >
                    download .md
                  </button>
                </div>
              </div>

              {tab === 'rendered' ? (
                <div className="viewer-rendered">
                  <p className="micro" style={{ marginBottom: 6 }}>
                    {frontmatterLine}
                  </p>
                  <h2 style={{ margin: '0 0 36px', fontSize: 26, borderBottom: '1px solid var(--hairline)', paddingBottom: 16 }}>
                    {title}
                  </h2>

                  {parsed.sections.map((section) => (
                    <section className="doc-section" key={`${section.timestamp}-${section.heading}`}>
                      <div style={{ display: 'flex', alignItems: 'baseline', gap: 14, marginBottom: 16 }}>
                        <button
                          className="ts-chip"
                          type="button"
                          onClick={() => {
                            setPlayerOn(true);
                            setStartAt(tsToSeconds(section.timestamp));
                          }}
                        >
                          {section.timestamp}
                        </button>
                        <h3 style={{ margin: 0, fontSize: 19 }}>{section.heading}</h3>
                      </div>
                      <div className="block-grid">
                        {section.blocks.map((block, i) => (
                          <div key={`${section.timestamp}-${i}`} style={{ display: 'contents' }}>
                            <span
                              className={`block-label ${
                                block.label === 'spoken'
                                  ? 'block-label-spoken'
                                  : block.label === 'on_screen'
                                    ? 'block-label-screen'
                                    : 'block-label-visual'
                              }`}
                            >
                              {block.label === 'spoken' ? 'spoken' : block.label === 'on_screen' ? 'on screen' : 'visual'}
                            </span>
                            {block.label === 'spoken' ? (
                              <p className="block-spoken">{block.text}</p>
                            ) : block.label === 'on_screen' ? (
                              <div className="block-screen">{block.text}</div>
                            ) : (
                              <p className="block-visual">{block.text}</p>
                            )}
                          </div>
                        ))}
                      </div>
                    </section>
                  ))}
                </div>
              ) : (
                <div style={{ padding: 16 }}>
                  <CodeCard title={filename} content={raw} />
                </div>
              )}
            </div>
            <p className="micro" style={{ marginTop: 16 }}>
              read an <span className="kv">on screen</span> block, click its timestamp, and check it against the video
              with your own eyes
            </p>
          </div>
        </div>
      </section>
    </>
  );
}
