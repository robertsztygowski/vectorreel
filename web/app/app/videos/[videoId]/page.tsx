import { notFound } from 'next/navigation';
import Link from 'next/link';
import { getVideoMeta } from '@/lib/corpus';
import { getLibraryItem } from '@/lib/mockLibrary';
import { MarkdownOutputCard } from '@/components/MarkdownOutputCard/MarkdownOutputCard';
import { DeleteVideoButton } from '@/components/app/DeleteVideoButton';

function formatDate(iso: string): string {
  return new Date(iso).toISOString().slice(0, 10);
}

export default async function LibraryVideoPage({ params }: { params: Promise<{ videoId: string }> }) {
  const { videoId } = await params;
  const item = getLibraryItem(videoId);
  const meta = getVideoMeta(videoId);
  if (!item || !meta) notFound();

  return (
    <div className="app-page">
      <div className="wrap">
        <div className="app-head-row">
          <div>
            <h1>{`${item.id}.md`}</h1>
            <p className="lead">{item.channel} · processed {formatDate(item.processedAt)}</p>
          </div>
          <DeleteVideoButton videoId={item.id} jobId={item.jobId} title={item.title} />
        </div>
        <div style={{ display: 'grid', gap: 20, maxWidth: 880 }}>
          <p className="micro">
            <Link href="/app">← back to your library</Link>
          </p>
          <MarkdownOutputCard
            frontmatter={meta.parsed.frontmatter}
            sections={meta.parsed.sections}
            raw={meta.raw}
            filename={`${item.id}.md`}
          />
        </div>
      </div>
    </div>
  );
}
