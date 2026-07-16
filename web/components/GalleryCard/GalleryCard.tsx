import Link from 'next/link';
import type { CorpusEntry } from '@/lib/corpus';
import { formatHms } from '@/lib/format';
import styles from './GalleryCard.module.css';

const CATEGORY_LABELS: Record<CorpusEntry['category'], string> = {
  slide_talk: 'slide talk',
  talking_head: 'talking head',
  screencast: 'screencast',
};

export interface GalleryPreview {
  fileName: string;
  rank: string;
  timestamp: string;
  heading: string;
  spoken: string;
  onScreen: string;
}

function compact(text: string): string {
  return text.replace(/\s+/g, ' ').trim();
}

export function GalleryCard({ entry, preview }: { entry: CorpusEntry; preview: GalleryPreview }) {
  return (
    <Link href={`/gallery/${entry.video_id}`} className={`card ${styles.card}`}>
      <span className={styles.thumbColumn}>
        <span className={styles.thumb}>
          {/* eslint-disable-next-line @next/next/no-img-element -- curated source thumbnail */}
          <img src={`https://img.youtube.com/vi/${entry.video_id}/hqdefault.jpg`} alt="" loading="lazy" />
          <span className={styles.duration}>{formatHms(entry.duration_s)}</span>
        </span>
        <span className={styles.thumbCaption}>thumbnail · i.ytimg.com (attributed)</span>
      </span>
      <span className={styles.body}>
        <span className={styles.headlineRow}>
          <span className={styles.title}>{entry.title}</span>
          <span className={styles.rank}>{preview.rank}</span>
        </span>
        <p className={styles.meta}>
          {entry.channel} · {CATEGORY_LABELS[entry.category]} · {preview.fileName}
        </p>
        <span className={styles.excerpt}>
          <span className={styles.excerptHeading}>
            ## <span className={styles.excerptTs}>{preview.timestamp}</span> {compact(preview.heading)}
          </span>
          <span className={styles.excerptLine}>
            <span className={styles.labelSpoken}>Spoken:</span> {compact(preview.spoken)}
          </span>
          <span className={styles.excerptLine}>
            <span className={styles.labelScreen}>On screen:</span> {compact(preview.onScreen)}
          </span>
        </span>
        <span className={styles.action}>open document + video →</span>
      </span>
    </Link>
  );
}
