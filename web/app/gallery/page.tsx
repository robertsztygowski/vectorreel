import type { Metadata } from 'next';
import { loadCorpusIndex } from '@/lib/corpus';
import { GalleryCard } from '@/components/GalleryCard/GalleryCard';

export const metadata: Metadata = { title: 'Gallery — mdreel' };

export default function GalleryPage() {
  const entries = loadCorpusIndex();
  return (
    <>
      <div className="page-head">
        <div className="wrap">
          <h1>Gallery</h1>
          <p className="lead">
            Curated, CC-licensed talks processed end-to-end by mdreel — attribution and the original video included.
          </p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap">
          <div className="cards">
            {entries.map((entry) => (
              <GalleryCard key={entry.video_id} entry={entry} />
            ))}
          </div>
        </div>
      </div>
    </>
  );
}
