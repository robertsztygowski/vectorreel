import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { SHOW_COLLECTIONS } from '@/lib/flags';
import { loadCollection, sessionsOf, chronological } from '@/lib/collectionRepository';
import { CollectionEntry } from '@/components/CollectionEntry';
import { ConvertCta } from '@/components/ConvertCta';

type Params = Promise<{ collection: string; slug: string }>;

export async function generateMetadata({ params }: { params: Params }): Promise<Metadata> {
  const { collection: slug, slug: topicSlug } = await params;
  const topic = loadCollection(slug)?.manifest.topics.find((t) => t.slug === topicSlug);
  return { title: `${topic?.label ?? 'Topic'} — mdreel` };
}

export default async function TopicPage({ params }: { params: Params }) {
  if (!SHOW_COLLECTIONS) {
    notFound();
  }

  const { collection: collectionSlug, slug: topicSlug } = await params;
  const collection = loadCollection(collectionSlug);
  const topic = collection?.manifest.topics.find((t) => t.slug === topicSlug);
  if (!collection || !topic) {
    notFound();
  }

  const sessions = chronological(sessionsOf(collection, topic.sessions));

  return (
    <>
      <section className="rule-section">
        <div className="wrap" style={{ padding: '56px 32px 24px' }}>
          <p className="kicker">
            <Link href={`/collections/${collectionSlug}`}># {collection.manifest.repository.name}</Link>
          </p>
          <h1 className="display-m" style={{ marginBottom: 12 }}>{topic.label ?? topic.slug}</h1>
          <p className="lead" style={{ maxWidth: '58ch' }}>
            Everything in this collection that touches {topic.label ?? topic.slug} — cited, never
            restated. Follow a link to read the session, or straight into the original video.
          </p>
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '24px 32px' }}>
          {sessions.map((session) => (
            <CollectionEntry key={session.slug} collection={collectionSlug} session={session} />
          ))}
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '32px' }}>
          <ConvertCta from="collection_topic" className="btn btn-primary">
            build your own repository →
          </ConvertCta>
        </div>
      </section>
    </>
  );
}
