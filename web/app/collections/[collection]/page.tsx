import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { SHOW_COLLECTIONS } from '@/lib/flags';
import { chronological, countsOf, loadCollection } from '@/lib/collectionRepository';
import { CollectionEntry } from '@/components/CollectionEntry';
import { ConvertCta } from '@/components/ConvertCta';

export async function generateMetadata({
  params,
}: {
  params: Promise<{ collection: string }>;
}): Promise<Metadata> {
  const { collection: slug } = await params;
  const collection = loadCollection(slug);
  return { title: `${collection?.manifest.repository.name ?? 'Collection'} — mdreel` };
}

export default async function CollectionPage({
  params,
}: {
  params: Promise<{ collection: string }>;
}) {
  if (!SHOW_COLLECTIONS) {
    notFound();
  }

  const { collection: slug } = await params;
  const collection = loadCollection(slug);
  if (!collection) {
    notFound();
  }

  const { manifest } = collection;
  const counts = countsOf(collection);
  const sessions = chronological(manifest.sessions);

  return (
    <>
      <section className="rule-section">
        <div className="wrap" style={{ padding: '72px 32px 40px' }}>
          <p className="kicker"># collection</p>
          <h1 className="display-l" style={{ marginBottom: 20, maxWidth: '24ch' }}>
            {manifest.repository.name}
          </h1>
          <p className="lead" style={{ maxWidth: '58ch' }}>{manifest.repository.description}</p>
          <p className="micro" style={{ fontFamily: 'var(--font-mono-stack)', color: 'var(--ink-soft)' }}>
            {counts.full} sessions · {counts.reference} referenced · {counts.topics} topics ·{' '}
            {counts.speakers} speakers
          </p>
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '24px 32px' }}>
          <h2 style={{ marginBottom: 8 }}>Two tiers, and you can always tell which is which</h2>
          <p style={{ maxWidth: '62ch', color: 'var(--ink-soft)' }}>
            <strong>Sessions</strong> are complete, timestamped documents mdreel produced from
            CC&nbsp;BY recordings. <strong>References</strong> are index entries with deep links into
            talks we may not reproduce — no derived text, nothing processed. Both make the subject
            navigable; only one is a derivative work.
          </p>
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '24px 32px' }}>
          <h2 style={{ marginBottom: 16 }}>Browse</h2>
          <ul style={{ paddingLeft: 18, marginBottom: 24 }}>
            <li>
              <Link href={`/collections/${slug}/timeline`}>Timeline</Link> — chronologically
            </li>
            {manifest.topics.map((topic) => (
              <li key={topic.slug}>
                <Link href={`/collections/${slug}/topics/${topic.slug}`}>{topic.label ?? topic.slug}</Link>
              </li>
            ))}
          </ul>

          <h2 style={{ marginBottom: 16 }}>Speakers</h2>
          <ul style={{ paddingLeft: 18 }}>
            {manifest.speakers.map((speaker) => (
              <li key={speaker.slug}>
                <Link href={`/collections/${slug}/speakers/${speaker.slug}`}>{speaker.name ?? speaker.slug}</Link>
              </li>
            ))}
          </ul>
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '24px 32px' }}>
          <h2 style={{ marginBottom: 16 }}>Everything in this collection</h2>
          {sessions.map((session) => (
            <CollectionEntry key={session.slug} collection={slug} session={session} />
          ))}
        </div>
      </section>

      {manifest.repository.licence_note && (
        <section className="rule-section">
          <div className="wrap" style={{ padding: '24px 32px' }}>
            <p className="micro" style={{ color: 'var(--ink-soft)', maxWidth: '62ch' }}>
              {manifest.repository.licence_note}
            </p>
          </div>
        </section>
      )}

      <section className="rule-section">
        <div className="wrap" style={{ padding: '32px' }}>
          <ConvertCta from="collection_overview" className="btn btn-primary">
            build your own repository →
          </ConvertCta>
        </div>
      </section>
    </>
  );
}
