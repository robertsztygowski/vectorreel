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

export function trackUmamiFunnelView(pathname: string): void {
  const eventName = {
    '/signup': 'signup_view',
    '/pricing': 'pricing_view',
    '/gallery': 'gallery_view',
    '/docs': 'docs_view',
  }[pathname];
  if (eventName) trackUmami(eventName);
}
