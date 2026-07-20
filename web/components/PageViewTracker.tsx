'use client';

import { useEffect } from 'react';
import { usePathname } from 'next/navigation';
import { trackPageView } from '@/lib/events';
import { trackUmamiFunnelView } from '@/lib/umami';

export function PageViewTracker() {
  const pathname = usePathname();
  useEffect(() => {
    trackPageView(pathname);
    trackUmamiFunnelView(pathname);
  }, [pathname]);
  return null;
}
