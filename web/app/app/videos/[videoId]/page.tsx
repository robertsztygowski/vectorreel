import { notFound } from 'next/navigation';
import Link from 'next/link';
import { getVideoMeta } from '@/lib/corpus';
import { getLibraryItem } from '@/lib/mockLibrary';
import { MarkdownOutputCard } from '@/components/MarkdownOutputCard/MarkdownOutputCard';
import { DeleteVideoButton } from '@/components/app/DeleteVideoButton';

export default async function LibraryVideoPage({ params }: { params: Promise<{ videoId: string }> }) {
  const { videoId } = await params;
  const item = getLibraryItem(videoId);
  const meta = getVideoMeta(videoId);
  if (!item || !meta) notFound();

  return (
    <>
      <div className="page-head">
        <div className="wrap">
          <div className="app-head-row">
            <div>
              <h1>{item.title}</h1>
              <p className="lead">{item.channel} · processed output</p>
            </div>
            <DeleteVideoButton videoId={item.id} title={item.title} />
          </div>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap" style={{ display: 'grid', gap: 20, maxWidth: 860 }}>
          <p className="micro">
            <Link href="/app">← Back to your library</Link>
          </p>
          <MarkdownOutputCard
            h1={meta.parsed.h1}
            frontmatter={meta.parsed.frontmatter}
            sections={meta.parsed.sections}
            raw={meta.raw}
            filename={`${item.id}.md`}
          />
        </div>
      </div>
    </>
  );
}
