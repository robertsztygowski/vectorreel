'use client';

import { Fragment, useMemo, useState } from 'react';
import type { OutputBlock, OutputFrontmatter, OutputSection } from '@/lib/outputDocument';

const BLOCK_LABELS: Record<OutputBlock['label'], string> = {
  spoken: 'spoken',
  on_screen: 'on screen',
  visual: 'visual',
};

interface MarkdownOutputCardProps {
  frontmatter: OutputFrontmatter;
  sections: OutputSection[];
  raw: string;
  filename: string;
  onDownload?: () => void;
}

function classifyRawLine(line: string): { className?: string } {
  if (line === '---') return { className: 'fence' };
  if (/^(title|source|duration|language|processed_at|generator|summary|tags):/.test(line)) return { className: 'key' };
  if (/^#\s+/.test(line)) return { className: 'h' };
  if (/^\*\*(Spoken|On screen|Visual):\*\*/i.test(line)) return { className: 'key' };
  if (/^>\s?/.test(line)) return { className: 'quote' };
  return {};
}

export function MarkdownOutputCard({ frontmatter, sections, raw, filename, onDownload }: MarkdownOutputCardProps) {
  const [tab, setTab] = useState<'rendered' | 'raw'>('rendered');
  const rawLines = useMemo(() => raw.replace(/\r\n/g, '\n').split('\n'), [raw]);

  function handleDownload() {
    const blob = new Blob([raw], { type: 'text/markdown' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
    onDownload?.();
  }

  return (
    <div className="viewer">
      <div className="viewer-bar">
        <div style={{ display: 'flex', alignItems: 'stretch', gap: 20, minWidth: 0 }}>
          <span style={{ display: 'flex', alignItems: 'center', fontFamily: 'var(--font-mono-stack)', fontSize: 12.5, fontWeight: 500 }}>
            {filename}
          </span>
          <div className="viewer-tabs" style={{ display: 'flex', alignSelf: 'stretch' }}>
            <button type="button" className={tab === 'rendered' ? 'active' : undefined} onClick={() => setTab('rendered')}>
              rendered
            </button>
            <button type="button" className={tab === 'raw' ? 'active' : undefined} onClick={() => setTab('raw')}>
              raw
            </button>
          </div>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
          <button type="button" className="btn btn-primary btn-sm" onClick={handleDownload}>
            download .md
          </button>
        </div>
      </div>

      {tab === 'rendered' ? (
        <div className="viewer-rendered">
          <p style={{ margin: '0 0 6px', fontFamily: 'var(--font-mono-stack)', fontSize: 11.5, color: 'var(--ink-faint)' }}>
            title: &quot;{frontmatter.title}&quot; · duration: {frontmatter.duration} · lang: {frontmatter.language} · tags: [
            {frontmatter.tags.join(', ')}]
          </p>
          <h2 style={{ margin: '0 0 40px', fontSize: 28, fontWeight: 600, letterSpacing: '-0.01em', fontVariationSettings: "'opsz' 40", paddingBottom: 18, borderBottom: '1px solid var(--hairline)' }}>
            {frontmatter.title}
          </h2>
          {frontmatter.summary && (
            <p style={{ margin: '0 0 28px', fontSize: 15.5, lineHeight: 1.6, color: 'var(--ink-soft)' }}>{frontmatter.summary}</p>
          )}
          {sections.map((section) => (
            <section key={`${section.timestamp}-${section.heading}`} className="doc-section">
              <div style={{ display: 'flex', alignItems: 'baseline', gap: 14, marginBottom: 18 }}>
                <span className="ts-chip">[{section.timestamp}]</span>
                <h3 style={{ margin: 0, fontSize: 20, fontWeight: 600, letterSpacing: '-0.01em' }}>{section.heading}</h3>
              </div>
              <div className="block-grid">
                {section.blocks.map((block, blockIndex) => (
                  <Fragment key={`${section.timestamp}-${section.heading}-${block.label}-${blockIndex}`}>
                    <span
                      className={`block-label ${
                        block.label === 'spoken'
                          ? 'block-label-spoken'
                          : block.label === 'on_screen'
                            ? 'block-label-screen'
                            : 'block-label-visual'
                      }`}
                    >
                      {BLOCK_LABELS[block.label]}
                    </span>
                    {block.label === 'spoken' ? (
                      <p className="block-spoken">{block.text}</p>
                    ) : block.label === 'on_screen' ? (
                      <div className="block-screen">
                        {block.text.split('\n').map((line, lineIndex) => (
                          <div key={`${section.timestamp}-${section.heading}-${blockIndex}-${lineIndex}`}>{line}</div>
                        ))}
                      </div>
                    ) : (
                      <p className="block-visual">{block.text}</p>
                    )}
                  </Fragment>
                ))}
              </div>
            </section>
          ))}
        </div>
      ) : (
        <div style={{ padding: '20px 0' }}>
          <div className="doc-card" style={{ boxShadow: 'none' }}>
            <div className="doc-card-head">
              <span className="file">{filename}</span>
              <span className="meta">text/markdown</span>
            </div>
            <div className="doc-lines">
              {rawLines.map((line, index) => {
                const meta = classifyRawLine(line);
                return (
                  <div key={`${index}-${line}`} className="doc-line">
                    <span className="n">{index + 1}</span>
                    <span className={`t ${meta.className ?? ''}`.trim()}>{line || '\u00A0'}</span>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
