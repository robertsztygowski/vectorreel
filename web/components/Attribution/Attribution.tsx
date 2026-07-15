import type { LicenceBlock } from '@/lib/corpus';
import styles from './Attribution.module.css';

export function Attribution({ licence }: { licence: LicenceBlock }) {
  return (
    <div className={styles.note}>
      <p className={styles.title}>Source &amp; licence</p>
      <p>{licence.attribution}</p>
      <p>
        Original video:{' '}
        <a href={licence.originalVideoUrl} target="_blank" rel="noopener noreferrer">
          {licence.originalVideoUrl}
        </a>
      </p>
      <p>Licence: {licence.licenceLine}</p>
    </div>
  );
}
