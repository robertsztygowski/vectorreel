import { notFound } from 'next/navigation';
import { getLicenceBlock, getVideoMeta, loadCorpusIndex } from '@/lib/corpus';
import { Attribution } from '@/components/Attribution/Attribution';
import { YouTubeEmbed } from '@/components/YouTubeEmbed/YouTubeEmbed';
import { MarkdownOutputCard } from '@/components/MarkdownOutputCard/MarkdownOutputCard';

export function generateStaticParams() {
  return loadCorpusIndex().map((entry) => ({ videoId: entry.video_id }));
}

export default async function GalleryDetailPage({ params }: { params: Promise<{ videoId: string }> }) {
  const { videoId } = await params;
  const meta = getVideoMeta(videoId);
  if (!meta) notFound();

  const licence = getLicenceBlock(meta.corpus);

  return (
    <>
      <div className="page-head">
        <div className="wrap">
          <h1>{meta.corpus.title}</h1>
          <p className="lead">{meta.corpus.channel}</p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap" style={{ display: 'grid', gap: 28, maxWidth: 860 }}>
          <YouTubeEmbed videoId={meta.corpus.video_id} title={meta.corpus.title} />
          <Attribution licence={licence} />
          <MarkdownOutputCard
            h1={meta.parsed.h1}
            frontmatter={meta.parsed.frontmatter}
            sections={meta.parsed.sections}
            raw={meta.raw}
            filename={`${meta.corpus.video_id}.md`}
          />
        </div>
      </div>
    </>
  );
}
