import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { SHOW_COLLECTIONS } from '@/lib/flags';
import { findSession, isFull, loadCollection, loadSessionDocument } from '@/lib/collectionRepository';
import { ConvertCta } from '@/components/ConvertCta';
import { PageViewTracker } from '@/components/PageViewTracker';

type Params = Promise<{ collection: string; slug: string }>;

export async function generateMetadata({ params }: { params: Params }): Promise<Metadata> {
  const { collection: slug, slug: sessionSlug } = await params;
  const collection = loadCollection(slug);
  const session = collection ? findSession(collection, sessionSlug) : undefined;
  return { title: `${session?.title ?? 'Session'} — mdreel` };
}

export default async function SessionPage({ params }: { params: Params }) {
  if (!SHOW_COLLECTIONS) {
    notFound();
  }

  const { collection: collectionSlug, slug: sessionSlug } = await params;
  const collection = loadCollection(collectionSlug);
  const session = collection ? findSession(collection, sessionSlug) : undefined;

  // A reference entry has no session page by contract — it was never processed, so there is
  // nothing to show. 404 rather than invent a thin page that blurs the tiers.
  if (!collection || !session || !isFull(session)) {
    notFound();
  }

  const document = loadSessionDocument(collectionSlug, session);
  if (!document) {
    notFound();
  }

  const videoId = session.source.split('v=')[1]?.split('&')[0];

  return (
    <>
      <PageViewTracker videoId={videoId} />

      <section className="rule-section">
        <div className="wrap" style={{ padding: '56px 32px 32px' }}>
          <p className="kicker">
            <Link href={`/collections/${collectionSlug}`}># {collection.manifest.repository.name}</Link>
          </p>
          <h1 className="display-m" style={{ marginBottom: 12, maxWidth: '28ch' }}>
            {document.frontmatter.title}
          </h1>
          <p className="micro" style={{ fontFamily: 'var(--font-mono-stack)', color: 'var(--ink-soft)' }}>
            {[session.event, session.year].filter(Boolean).join(' ')}
            {session.duration ? ` · ${session.duration}` : ''} · {document.sections.length} sections
          </p>
          <p className="lead" style={{ maxWidth: '58ch' }}>{document.frontmatter.summary}</p>
          <p className="micro">
            <a href={session.source} rel="noreferrer noopener" target="_blank">
              open the original video ↗
            </a>{' '}
            — every timestamp below is checkable against it.
          </p>
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '24px 32px' }}>
          {document.sections.map((section) => (
            <article key={section.timestamp} style={{ marginBottom: 32 }}>
              <h2 style={{ fontSize: 18, marginBottom: 8 }}>
                <a
                  href={`${session.source}${session.source.includes('?') ? '&' : '?'}t=${toSeconds(section.timestamp)}s`}
                  rel="noreferrer noopener"
                  target="_blank"
                  style={{ fontFamily: 'var(--font-mono-stack)', fontSize: 14 }}
                >
                  [{section.timestamp}]
                </a>{' '}
                {section.heading}
              </h2>
              {section.blocks.map((block) => (
                <div key={block.label} style={{ marginBottom: 10 }}>
                  <p
                    className="micro"
                    style={{ fontFamily: 'var(--font-mono-stack)', color: 'var(--ink-soft)', margin: 0 }}
                  >
                    {block.label.replace('_', ' ')}
                  </p>
                  <p style={{ margin: 0, maxWidth: '68ch', whiteSpace: 'pre-wrap' }}>{block.text}</p>
                </div>
              ))}
            </article>
          ))}
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '24px 32px' }}>
          <h2 style={{ fontSize: 16, marginBottom: 8 }}>Source &amp; licence</h2>
          <p className="micro" style={{ color: 'var(--ink-soft)', maxWidth: '62ch' }}>
            {session.attribution ?? document.provenance}
          </p>
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '32px' }}>
          <ConvertCta from="collection_session" videoId={videoId} className="btn btn-primary">
            build your own repository →
          </ConvertCta>
        </div>
      </section>
    </>
  );
}

function toSeconds(timestamp: string): number {
  const [h, m, s] = timestamp.split(':').map(Number);
  return h * 3600 + m * 60 + s;
}
