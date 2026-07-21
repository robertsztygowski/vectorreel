import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { SHOW_COLLECTIONS } from '@/lib/flags';
import { countsOf, listCollections, loadCollection } from '@/lib/collectionRepository';
import { ConvertCta } from '@/components/ConvertCta';

export const metadata: Metadata = { title: 'Collections — mdreel' };

export default function CollectionsIndexPage() {
  if (!SHOW_COLLECTIONS) {
    notFound();
  }

  const collections = listCollections()
    .map((slug) => loadCollection(slug))
    .filter((c): c is NonNullable<typeof c> => Boolean(c));

  return (
    <>
      <section className="rule-section">
        <div className="wrap" style={{ padding: '72px 32px 56px' }}>
          <p className="kicker"># collections</p>
          <h1 className="display-l" style={{ marginBottom: 20, maxWidth: '24ch' }}>
            A subject, assembled across every talk that covers it.
          </h1>
          <p className="lead" style={{ maxWidth: '58ch' }}>
            Each collection is a topic, not a conference — many events, many speakers, many years,
            organised so an agent can explore and cite it. Every claim carries a timestamp into
            footage you can open yourself.
          </p>
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '32px' }}>
          {collections.map((collection) => {
            const counts = countsOf(collection);
            return (
              <article key={collection.slug} style={{ marginBottom: 32 }}>
                <h2 style={{ marginBottom: 8 }}>
                  <Link href={`/collections/${collection.slug}`}>{collection.manifest.repository.name}</Link>
                </h2>
                <p style={{ maxWidth: '62ch', color: 'var(--ink-soft)' }}>
                  {collection.manifest.repository.description}
                </p>
                <p className="micro" style={{ fontFamily: 'var(--font-mono-stack)', color: 'var(--ink-soft)' }}>
                  {counts.full} sessions · {counts.reference} referenced · {counts.topics} topics ·{' '}
                  {counts.speakers} speakers
                </p>
              </article>
            );
          })}
          {collections.length === 0 && <p className="lead">No collections published yet.</p>}
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '32px' }}>
          <ConvertCta from="collections_index" className="btn btn-primary">
            build your own repository →
          </ConvertCta>
        </div>
      </section>
    </>
  );
}
