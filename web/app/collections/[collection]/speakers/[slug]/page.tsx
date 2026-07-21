import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { SHOW_COLLECTIONS } from '@/lib/flags';
import { loadCollection, sessionsOf, chronological } from '@/lib/collectionRepository';
import { CollectionEntry } from '@/components/CollectionEntry';
import { ConvertCta } from '@/components/ConvertCta';

type Params = Promise<{ collection: string; slug: string }>;

export async function generateMetadata({ params }: { params: Params }): Promise<Metadata> {
  const { collection: slug, slug: speakerSlug } = await params;
  const speaker = loadCollection(slug)?.manifest.speakers.find((s) => s.slug === speakerSlug);
  return { title: `${speaker?.name ?? 'Speaker'} — mdreel` };
}

export default async function SpeakerPage({ params }: { params: Params }) {
  if (!SHOW_COLLECTIONS) {
    notFound();
  }

  const { collection: collectionSlug, slug: speakerSlug } = await params;
  const collection = loadCollection(collectionSlug);
  const speaker = collection?.manifest.speakers.find((s) => s.slug === speakerSlug);
  if (!collection || !speaker) {
    notFound();
  }

  const sessions = chronological(sessionsOf(collection, speaker.sessions));

  return (
    <>
      <section className="rule-section">
        <div className="wrap" style={{ padding: '56px 32px 24px' }}>
          <p className="kicker">
            <Link href={`/collections/${collectionSlug}`}># {collection.manifest.repository.name}</Link>
          </p>
          <h1 className="display-m" style={{ marginBottom: 12 }}>{speaker.name ?? speaker.slug}</h1>
          <p className="lead" style={{ maxWidth: '58ch' }}>
            Follow one person across the collection — how their argument develops, and where to check
            it. Speaker names are curation metadata, never guessed by a model.
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
          <ConvertCta from="collection_speaker" className="btn btn-primary">
            build your own repository →
          </ConvertCta>
        </div>
      </section>
    </>
  );
}
