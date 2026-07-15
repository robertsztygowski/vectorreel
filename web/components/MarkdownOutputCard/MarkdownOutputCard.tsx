'use client';

import { useState } from 'react';
import type { Block, Frontmatter, Section } from '@/lib/corpus';
import { CodeCard } from '../CodeCard/CodeCard';
import styles from './MarkdownOutputCard.module.css';

const BLOCK_LABELS: Record<Block['label'], string> = {
  spoken: 'Spoken',
  on_screen: 'On screen',
  visual: 'Visual',
};

interface MarkdownOutputCardProps {
  h1: string;
  frontmatter: Pick<Frontmatter, 'summary'>;
  sections: Section[];
  raw: string;
  filename: string;
  onDownload?: () => void;
}

export function MarkdownOutputCard({ h1, frontmatter, sections, raw, filename, onDownload }: MarkdownOutputCardProps) {
  const [tab, setTab] = useState<'rendered' | 'raw'>('rendered');

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
    <div className={styles.card}>
      <div className={styles.tabs}>
        <button type="button" className={tab === 'rendered' ? styles.tabActive : styles.tab} onClick={() => setTab('rendered')}>
          Rendered
        </button>
        <button type="button" className={tab === 'raw' ? styles.tabActive : styles.tab} onClick={() => setTab('raw')}>
          Raw
        </button>
        <button type="button" className={`btn btn-ghost ${styles.downloadBtn}`} onClick={handleDownload}>
          Download .md
        </button>
      </div>

      {tab === 'rendered' ? (
        <div className={styles.rendered}>
          <h2>{h1}</h2>
          {frontmatter.summary && <p className={styles.summary}>{frontmatter.summary}</p>}
          {sections.map((section) => (
            <section key={`${section.timestamp}-${section.heading}`} className={styles.section}>
              <h3>
                <span className={styles.timestamp}>[{section.timestamp}]</span>
                {section.heading}
              </h3>
              {section.blocks.map((block, i) => (
                <div key={i} className={styles.block}>
                  <span className={styles.blockLabel}>{BLOCK_LABELS[block.label]}</span>
                  <p>{block.text}</p>
                </div>
              ))}
            </section>
          ))}
        </div>
      ) : (
        <div style={{ padding: 16 }}>
          <CodeCard title={filename} content={raw} />
        </div>
      )}
    </div>
  );
}
