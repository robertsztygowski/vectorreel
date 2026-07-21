import { getSessionId } from './attribution';

type UmamiTracker = {
  track: (name: string, data?: Record<string, unknown>) => void;
};

declare global {
  interface Window {
    umami?: UmamiTracker;
  }
}

export function trackUmami(name: string, data: Record<string, unknown> = {}): void {
  if (typeof window === 'undefined') return;
  window.umami?.track(name, { ...data, sid: getSessionId() });
}

export function trackUmamiFunnelView(pathname: string, videoId?: string): void {
  const eventName = {
    '/signup': 'signup_view',
    '/pricing': 'pricing_view',
    '/gallery': 'gallery_view',
    '/docs': 'docs_view',
  }[pathname];
  if (eventName) {
    trackUmami(eventName);
    return;
  }
  // Collection session pages (/gallery/<videoId>) — the consume side of consume→convert.
  if (pathname.startsWith('/gallery/')) {
    trackUmami('collection_session_view', { videoId: pathname.slice('/gallery/'.length) });
    return;
  }
  // Contract-derived collection pages (/collections/<collection>/sessions/<slug>). Deliberately the
  // SAME event name: the series' continuity was preserved through the pivot on purpose, and a new
  // name would split the measurement in two. `videoId` stays the YouTube id in both cases — the
  // page passes it explicitly, because a §4b session slug is not a video id.
  if (/^\/collections\/[^/]+\/sessions\/[^/]+$/.test(pathname)) {
    trackUmami('collection_session_view', videoId ? { videoId } : {});
  }
}

// The convert click — a visitor moves from consuming a public collection to starting their own
// repository. Fired from collection CTAs only, so the sources panel can attribute signups to the
// consume→convert path without any third-party pixel (CLAUDE.md rule 10).
export function trackConvertClick(from: string, videoId?: string): void {
  trackUmami('collection_convert_click', videoId ? { from, videoId } : { from });
}
