import { loadCorpusIndex, getVideoMeta, type CorpusEntry } from './corpus';

// Server-only mocked "already processed" library for the authenticated panel (/app). Derived from
// the committed corpus fixtures so the panel review happens on genuine product output, not lorem
// ipsum. There is no product backend this phase — this stands in for the future `jobs` table.

export interface LibraryItem {
  id: string; // corpus video_id, used as the job id in the panel
  title: string;
  channel: string;
  category: CorpusEntry['category'];
  durationSec: number;
  processedAt: string;
}

const CATEGORY_LABEL: Record<CorpusEntry['category'], string> = {
  slide_talk: 'Slide talk',
  talking_head: 'Talking head',
  screencast: 'Screencast',
};

export function categoryLabel(category: CorpusEntry['category']): string {
  return CATEGORY_LABEL[category];
}

export function listLibrary(): LibraryItem[] {
  // Deterministic mock "processed_at" dates spaced a few days apart, newest first.
  const base = Date.UTC(2026, 6, 14); // 2026-07-14
  return loadCorpusIndex().map((entry, i) => ({
    id: entry.video_id,
    title: entry.title,
    channel: entry.channel,
    category: entry.category,
    durationSec: entry.duration_s,
    processedAt: new Date(base - i * 2 * 86_400_000).toISOString(),
  }));
}

export function getLibraryItem(videoId: string): LibraryItem | undefined {
  return listLibrary().find((item) => item.id === videoId);
}

export function getLibraryMarkdown(videoId: string): { raw: string; filename: string } | null {
  const meta = getVideoMeta(videoId);
  if (!meta) return null;
  return { raw: meta.raw, filename: `${videoId}.md` };
}
