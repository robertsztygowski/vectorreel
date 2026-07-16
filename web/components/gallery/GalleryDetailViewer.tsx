'use client';

import { useMemo, useState } from 'react';
import Link from 'next/link';
import { highlightMarkdownLines } from '@/components/CodeCard/highlightMarkdown';
import type { LicenceBlock, ParsedMarkdown } from '@/lib/corpus';

function tsToSeconds(ts: string): number {
  const match = /\[(\d{2}):(\d{2}):(\d{2})\]/.exec(ts);
  if (!match) return 0;
  return Number(match[1]) * 3600 + Number(match[2]) * 60 + Number(match[3]);
}

function secondsToTs(seconds: number): string {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = Math.floor(seconds % 60);
  const pad = (value: number) => String(value).padStart(2, '0');
  return `[${pad(h)}:${pad(m)}:${pad(s)}]`;
}

function compact(text: string): string {
  return text.replace(/\s+/g, ' ').trim();
}

export function GalleryDetailViewer({
  videoId,
  specimenNumber,
  categoryLabel,
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
  specimenNumber: string;
  categoryLabel: string;
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

  const playerSrc = `https://www.youtube-nocookie.com/embed/${videoId}?start=${startAt}&autoplay=1&rel=0`;
  const rawLines = highlightMarkdownLines(raw.replace(/\r\n/g, '\n'));

  return (
    <>
      <section className="rule-section">
        <div className="wrap" style={{ padding: '36px 32px 40px' }}>
          <p className="micro" style={{ margin: '0 0 14px' }}>
            <Link href="/gallery">← gallery</Link>
          </p>
          <p className="kicker" style={{ marginBottom: 12 }}>
            ## {categoryLabel} · specimen {specimenNumber}
          </p>
          <h1 className="display-m" style={{ marginBottom: 12, maxWidth: '28ch' }}>
            {title}
          </h1>
          <p className="micro">
            {channel} · {duration} · processed {date} · {filename}
          </p>
        </div>
      </section>

      <section>
        <div className="wrap" style={{ padding: '40px 32px 0' }}>
          <div className="gallery-detail">
            <div className="gallery-video-col">
              <div className="gallery-sticky">
                <div className="gallery-player-shell">
                  {playerOn ? (
                    <iframe
                      src={playerSrc}
                      title={title}
                      style={{ display: 'block', width: '100%', aspectRatio: '16/9', border: 0 }}
                      allow="autoplay; encrypted-media; picture-in-picture"
                      allowFullScreen
                    />
                  ) : (
                    <button className="facade" type="button" onClick={() => setPlayerOn(true)}>
                      <span className="facade-center">
                        <span className="facade-play" />
                        <span className="facade-load">load player · youtube-nocookie.com</span>
                      </span>
                      <span className="facade-note">no third-party request is made until you press play</span>
                      <span className="facade-duration">{duration}</span>
                    </button>
                  )}
                </div>

                <p className="micro gallery-player-tip">
                  {playerOn
                    ? `playing from ${secondsToTs(startAt)} — click any timestamp in the document to jump the player`
                    : 'tip: timestamps in the document seek this player to the exact moment'}
                </p>

                <div className="attribution">
                  <p className="eyebrow">&gt; source &amp; licence</p>
                  <p className="gallery-attribution-line">{licence.attribution}</p>
                  <p className="gallery-source-line">
                    <span className="k">source:</span>{' '}
                    <a href={licence.originalVideoUrl} target="_blank" rel="noopener noreferrer">
                      watch the original on YouTube ↗
                    </a>
                  </p>
                  <p className="gallery-licence-line">
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
                      <a
                        href="#"
                        className={tab === 'rendered' ? 'active' : ''}
                        onClick={(event) => {
                          event.preventDefault();
                          setTab('rendered');
                        }}
                      >
                        rendered
                      </a>
                      <a
                        href="#"
                        className={tab === 'raw' ? 'active' : ''}
                        onClick={(event) => {
                          event.preventDefault();
                          setTab('raw');
                        }}
                      >
                        raw
                      </a>
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center' }}>
                    <button
                      className="viewer-download"
                      type="button"
                      onClick={() => {
                        const blob = new Blob([raw], { type: 'text/markdown' });
                        const url = URL.createObjectURL(blob);
                        const anchor = document.createElement('a');
                        anchor.href = url;
                        anchor.download = filename;
                        anchor.click();
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
                    <h2 className="gallery-doc-title">{title}</h2>

                    {parsed.sections.map((section) => (
                      <section className="doc-section" key={`${section.timestamp}-${section.heading}`}>
                        <div className="gallery-section-head">
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
                          <h3 className="gallery-section-title">{compact(section.heading)}</h3>
                        </div>

                        <div className="block-grid">
                          {section.blocks.map((block, index) => (
                            <div key={`${section.timestamp}-${index}`} style={{ display: 'contents' }}>
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
                  <div className="gallery-raw">
                    {rawLines.map((line, index) => (
                      <div key={line.key} className="gallery-raw-line">
                        <span className="gallery-raw-n">{String(index + 1)}</span>
                        <span className={`gallery-raw-t ${line.className ?? ''}`.trim()}>{line.text}</span>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <p className="micro gallery-doc-note">
                read an <span className="kv">on screen</span> block, click its timestamp, and check it against the video
                with your own eyes
              </p>
            </div>
          </div>
        </div>
      </section>

      <section className="gallery-cta-section">
        <div className="wrap gallery-cta-inner">
          <h2 className="gallery-cta-title">This is what your own footage comes back as.</h2>
          <p className="micro gallery-cta-kicker">no credit card · one portable .md file per video</p>
          <Link href="/signup" className="btn btn-primary gallery-cta-button">
            start free — 1 hour
          </Link>
          <p className="micro gallery-cta-api">
            <Link href="/docs" style={{ color: 'var(--accent)' }}>
              do this via API →
            </Link>
          </p>
        </div>
      </section>
    </>
  );
}
