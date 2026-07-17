import Script from 'next/script';

// Self-hosted, cookieless, EU-only analytics (CLAUDE.md rule 10). The script and website id are
// public by nature; they come from NEXT_PUBLIC_UMAMI_* so the origin can be switched to
// stats.mdreel.com later with a single build-arg change (web/Dockerfile). When either is unset
// (e.g. the e2e/test build) nothing is emitted, so tests never call the analytics origin.
export function UmamiAnalytics() {
  const scriptUrl = process.env.NEXT_PUBLIC_UMAMI_SCRIPT_URL?.replace(/\/+$/, '');
  const websiteId = process.env.NEXT_PUBLIC_UMAMI_WEBSITE_ID;
  if (!scriptUrl || !websiteId) return null;

  return (
    <Script
      src={`${scriptUrl}/script.js`}
      data-website-id={websiteId}
      strategy="afterInteractive"
      defer
    />
  );
}
