import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { SHOW_COLLECTIONS } from '@/lib/flags';
import { chronological, dateOf, loadCollection } from '@/lib/collectionRepository';
import { CollectionEntry } from '@/components/CollectionEntry';
import { ConvertCta } from '@/components/ConvertCta';

type Params = Promise<{ collection: string }>;

export async function generateMetadata({ params }: { params: Params }): Promise<Metadata> {
  const { collection: slug } = await params;
  return { title: `Timeline — ${loadCollection(slug)?.manifest.repository.name ?? 'Collection'} — mdreel` };
}

export default async function TimelinePage({ params }: { params: Params }) {
  if (!SHOW_COLLECTIONS) {
    notFound();
  }

  const { collection: collectionSlug } = await params;
  const collection = loadCollection(collectionSlug);
  if (!collection) {
    notFound();
  }

  const sessions = chronological(collection.manifest.sessions);
  const years = [...new Set(sessions.map((s) => dateOf(s)?.slice(0, 4) ?? 'undated'))];

  return (
    <>
      <section className="rule-section">
        <div className="wrap" style={{ padding: '56px 32px 24px' }}>
          <p className="kicker">
            <Link href={`/collections/${collectionSlug}`}># {collection.manifest.repository.name}</Link>
          </p>
          <h1 className="display-m" style={{ marginBottom: 12 }}>Timeline</h1>
          <p className="lead" style={{ maxWidth: '58ch' }}>
            The same subject over time — which is where a collection earns its keep. A single talk
            tells you what someone thought once; the timeline shows what changed.
          </p>
        </div>
      </section>

      {years.map((year) => (
        <section className="rule-section" key={year}>
          <div className="wrap" style={{ padding: '24px 32px' }}>
            <h2 style={{ fontFamily: 'var(--font-mono-stack)', fontSize: 16, marginBottom: 16 }}>{year}</h2>
            {sessions
              .filter((s) => (dateOf(s)?.slice(0, 4) ?? 'undated') === year)
              .map((session) => (
                <CollectionEntry key={session.slug} collection={collectionSlug} session={session} />
              ))}
          </div>
        </section>
      ))}

      <section className="rule-section">
        <div className="wrap" style={{ padding: '32px' }}>
          <ConvertCta from="collection_timeline" className="btn btn-primary">
            build your own repository →
          </ConvertCta>
        </div>
      </section>
    </>
  );
}
