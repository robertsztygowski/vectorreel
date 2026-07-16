import { notFound } from 'next/navigation';
import { getLicenceBlock, getVideoMeta, loadCorpusIndex } from '@/lib/corpus';
import { formatHms } from '@/lib/format';
import { GalleryDetailViewer } from '@/components/gallery/GalleryDetailViewer';

export function generateStaticParams() {
  return loadCorpusIndex().map((entry) => ({ videoId: entry.video_id }));
}

export default async function GalleryDetailPage({ params }: { params: Promise<{ videoId: string }> }) {
  const { videoId } = await params;
  const meta = getVideoMeta(videoId);
  if (!meta) notFound();

  const categoryEntries = loadCorpusIndex().filter((entry) => entry.category === meta.corpus.category);
  const specimenNumber = String(categoryEntries.findIndex((entry) => entry.video_id === videoId) + 1).padStart(2, '0');
  const licence = getLicenceBlock(meta.corpus);

  return (
    <GalleryDetailViewer
      videoId={meta.corpus.video_id}
      specimenNumber={specimenNumber}
      categoryLabel={meta.corpus.category.replace('_', ' ')}
      title={meta.corpus.title}
      duration={formatHms(meta.corpus.duration_s)}
      channel={meta.corpus.channel}
      date={meta.parsed.frontmatter.processed_at}
      parsed={meta.parsed}
      raw={meta.raw}
      filename={`${meta.corpus.video_id}.md`}
      licence={licence}
    />
  );
}
