'use client';

import { useEffect } from 'react';
import { usePathname } from 'next/navigation';
import { trackPageView } from '@/lib/events';
import { trackUmamiFunnelView } from '@/lib/umami';

// `videoId` is optional and only used where the path itself does not carry it — a §4b session slug
// is not a video id, but the funnel event's videoId must keep meaning the same thing everywhere.
export function PageViewTracker({ videoId }: { videoId?: string } = {}) {
  const pathname = usePathname();
  useEffect(() => {
    trackPageView(pathname);
    trackUmamiFunnelView(pathname, videoId);
  }, [pathname, videoId]);
  return null;
}
