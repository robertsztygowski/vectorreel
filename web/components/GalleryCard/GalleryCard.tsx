import Link from 'next/link';
import type { CorpusEntry } from '@/lib/corpus';
import { formatHms } from '@/lib/format';
import styles from './GalleryCard.module.css';

const CATEGORY_LABELS: Record<CorpusEntry['category'], string> = {
  slide_talk: 'Slide talk',
  talking_head: 'Talking head',
  screencast: 'Screencast',
};

export function GalleryCard({ entry }: { entry: CorpusEntry }) {
  return (
    <Link href={`/gallery/${entry.video_id}`} className={`card ${styles.card}`}>
      <span className={styles.thumb}>
        {/* eslint-disable-next-line @next/next/no-img-element -- the video's own YouTube
            thumbnail, same content-embed category as the iframe on the detail page (rule 10 is
            about analytics/tracking, not about displaying the source video we're attributing). */}
        <img src={`https://img.youtube.com/vi/${entry.video_id}/hqdefault.jpg`} alt="" loading="lazy" />
      </span>
      <span className={styles.body}>
        <span className={styles.badge}>{CATEGORY_LABELS[entry.category]}</span>
        <h3>{entry.title}</h3>
        <p className={styles.meta}>
          {entry.channel} · {formatHms(entry.duration_s)}
        </p>
      </span>
    </Link>
  );
}
