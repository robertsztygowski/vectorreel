import Link from 'next/link';
import { deepLink, isFull, type ManifestSession } from '@/lib/collectionRepository';

/**
 * One entry in a topic / speaker / timeline index, rendered so a reader can tell the two
 * publication tiers apart **on every line** (ARCHITECTURE.md §4b v1.1).
 *
 * A `full` entry links to its session document on mdreel. A `reference` entry links OUT to the
 * original video at the cited timestamp and never to an mdreel session page, because no document
 * exists — it was never processed. A collection that blurs the two is worse than one that publishes
 * less, so the distinction is visible rather than implied.
 */
export function CollectionEntry({
  collection,
  session,
}: {
  collection: string;
  session: ManifestSession;
}) {
  const full = isFull(session);
  const provenance = [session.event, session.year].filter(Boolean).join(' ');

  return (
    <article style={{ marginBottom: 20 }} data-tier={full ? 'full' : 'reference'}>
      <h3 style={{ marginBottom: 4, fontSize: 17 }}>
        {full ? (
          <Link href={`/collections/${collection}/sessions/${session.slug}`}>{session.title}</Link>
        ) : (
          <a href={session.source} rel="noreferrer noopener" target="_blank">
            {session.title}
          </a>
        )}{' '}
        <span
          className="micro"
          data-testid="tier-badge"
          style={{
            fontFamily: 'var(--font-mono-stack)',
            color: 'var(--ink-soft)',
            border: '1px solid currentColor',
            borderRadius: 3,
            padding: '1px 5px',
            fontSize: 11,
            verticalAlign: 'middle',
          }}
        >
          {full ? 'session' : 'reference ↗'}
        </span>
      </h3>

      {provenance && (
        <p className="micro" style={{ fontFamily: 'var(--font-mono-stack)', color: 'var(--ink-soft)', margin: 0 }}>
          {provenance}
          {session.duration ? ` · ${session.duration}` : ''}
        </p>
      )}

      {!full && (
        <>
          <p className="micro" style={{ color: 'var(--ink-soft)', margin: '4px 0 2px', maxWidth: '58ch' }}>
            Indexed, not transcribed — mdreel links into the original rather than reproducing it.
          </p>
          <ul style={{ margin: '4px 0 0', paddingLeft: 18 }}>
            {(session.citations ?? []).map((citation) => (
              <li key={citation.timestamp} className="micro" style={{ fontFamily: 'var(--font-mono-stack)' }}>
                <a href={deepLink(session, citation.timestamp)} rel="noreferrer noopener" target="_blank">
                  [{citation.timestamp}]
                </a>{' '}
                — {citation.label}
              </li>
            ))}
          </ul>
        </>
      )}
    </article>
  );
}
