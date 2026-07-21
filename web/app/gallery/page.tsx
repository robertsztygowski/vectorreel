import type { Metadata } from 'next';
import Link from 'next/link';
import { getVideoMeta, loadCorpusIndex, type CorpusEntry } from '@/lib/corpus';
import { GalleryCard, type GalleryPreview } from '@/components/GalleryCard/GalleryCard';
import { ConvertCta } from '@/components/ConvertCta';

export const metadata: Metadata = { title: 'Public collections — mdreel' };

const CATEGORY_LABELS: Record<CorpusEntry['category'], string> = {
  slide_talk: 'slide talk',
  talking_head: 'talking head',
  screencast: 'screencast',
};

const CATEGORY_ORDER: CorpusEntry['category'][] = ['slide_talk', 'talking_head', 'screencast'];

function compact(text: string | undefined): string {
  return (text ?? '').replace(/\s+/g, ' ').trim();
}

function buildPreview(entry: CorpusEntry, rank: string): GalleryPreview {
  const meta = getVideoMeta(entry.video_id);
  const first = meta?.parsed.sections[0];
  const spoken = first?.blocks.find((block) => block.label === 'spoken')?.text ?? first?.blocks[0]?.text ?? '';
  const onScreen = first?.blocks.find((block) => block.label === 'on_screen')?.text ?? '';

  return {
    fileName: `${entry.video_id}.md`,
    rank,
    timestamp: first?.timestamp ?? '[00:00:00]',
    heading: compact(first?.heading ?? entry.title),
    spoken: compact(spoken),
    onScreen: compact(onScreen),
  };
}

export default function GalleryPage() {
  const entries = loadCorpusIndex();
  const groups = CATEGORY_ORDER.map((category) => {
    const grouped = entries.filter((entry) => entry.category === category);
    return {
      category,
      label: CATEGORY_LABELS[category],
      entries: grouped.map((entry, index) => ({
        entry,
        preview: buildPreview(entry, String(index + 1).padStart(2, '0')),
      })),
    };
  });

  return (
    <>
      <section className="rule-section">
        <div className="wrap" style={{ padding: '72px 32px 56px' }}>
          <p className="kicker"># public collections</p>
          <h1 className="display-l" style={{ marginBottom: 20, maxWidth: '22ch' }}>
            Talks you already know, as an AI-ready repository.
          </h1>
          <p className="lead" style={{ maxWidth: '56ch' }}>
            You can&apos;t paste your own URL here — so explore ours instead. Every recording below is a session your
            agent could cite, shown next to the original video so you can verify each line against ground truth you
            already hold. Your own footage comes back in exactly the same shape.
          </p>
        </div>
      </section>

      <section className="rule-section">
        <div className="wrap" style={{ padding: '18px 32px', display: 'flex', alignItems: 'baseline', gap: 24, flexWrap: 'wrap', fontFamily: 'var(--font-mono-stack)', fontSize: 12.5 }}>
          <span className="kv">curated:</span>
          <span className="micro" style={{ color: 'var(--ink-soft)' }}>{entries.length} recordings, hand-picked</span>
          <span style={{ color: 'var(--line-strong)' }}>/</span>
          <span className="micro" style={{ color: 'var(--ink-soft)' }}>all CC-licensed &amp; attributed</span>
          <span style={{ color: 'var(--line-strong)' }}>/</span>
          <span className="micro" style={{ color: 'var(--ink-soft)' }}>original video embedded on every page</span>
        </div>
      </section>

      {groups.map((group) => (
        <section key={group.category} className="rule-section">
          <div className="wrap" style={{ padding: '0 32px' }}>
            <div style={{ display: 'grid', gridTemplateColumns: '220px 1fr' }}>
              <div style={{ padding: '48px 40px 48px 0', borderRight: '1px solid var(--hairline)' }}>
                <p className="kicker" style={{ position: 'sticky', top: 92, marginBottom: 10 }}>
                  ## {group.label}
                </p>
              </div>
              <div style={{ display: 'flex', flexDirection: 'column' }}>
                {group.entries.map(({ entry, preview }) => (
                  <GalleryCard key={entry.video_id} entry={entry} preview={preview} />
                ))}
              </div>
            </div>
          </div>
        </section>
      ))}

      <section className="rule-section">
        <div className="wrap" style={{ padding: '88px 32px', display: 'flex', flexDirection: 'column', alignItems: 'center', textAlign: 'center' }}>
          <h2 style={{ margin: '0 0 18px', fontSize: 'clamp(28px, 3.4vw, 44px)', lineHeight: 1.06, letterSpacing: '-0.012em', fontWeight: 500, fontVariationSettings: "'opsz' 64", maxWidth: '24ch' }}>
            This is what your own archive comes back as.
          </h2>
          <p className="micro" style={{ margin: '0 0 32px', color: 'var(--ink-faint)' }}>
            first hour free · no credit card · same repository shape, your private videos
          </p>
          <ConvertCta
            from="collection_index"
            className="btn btn-primary"
            style={{ height: 48, padding: '0 26px', fontSize: 13.5 }}
          >
            build your own repository →
          </ConvertCta>
          <p className="micro" style={{ margin: '26px 0 0' }}>
            <Link href="/docs" style={{ color: 'var(--accent)' }}>
              do this via API →
            </Link>
          </p>
        </div>
      </section>
    </>
  );
}
