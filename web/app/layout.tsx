import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import localFont from 'next/font/local';
import { Header } from '@/components/Header/Header';
import { Footer } from '@/components/Footer/Footer';
import { AttributionInit } from '@/components/AttributionInit';
import { PageViewTracker } from '@/components/PageViewTracker';
import './globals.css';

// Self-hosted fonts (files committed under app/fonts) — the shipped site serves them from its own
// /_next origin and never requests fonts.googleapis.com / fonts.gstatic.com, keeping rule 10 intact
// (nothing phones home to the US) while still using the design's Newsreader + IBM Plex Mono.
// Newsreader is the variable OFL file, so the comps' font-variation-settings:'opsz' N take effect.
const serif = localFont({
  src: [
    { path: './fonts/NewsreaderVar.ttf', style: 'normal' },
    { path: './fonts/NewsreaderVarItalic.ttf', style: 'italic' },
  ],
  display: 'swap',
  variable: '--font-serif',
});

const mono = localFont({
  src: [
    { path: './fonts/IBMPlexMono-Regular.ttf', weight: '400', style: 'normal' },
    { path: './fonts/IBMPlexMono-Medium.ttf', weight: '500', style: 'normal' },
    { path: './fonts/IBMPlexMono-SemiBold.ttf', weight: '600', style: 'normal' },
  ],
  display: 'swap',
  variable: '--font-mono',
});

export const metadata: Metadata = {
  title: 'mdreel — Turn company video into Markdown your AI agents can cite',
  description:
    "mdreel turns demos, trainings and walkthroughs into one portable, timestamped Markdown file — separating what's said from what's shown on screen. No lock-in, processed entirely in the EU.",
  openGraph: {
    title: 'mdreel — video to Markdown for AI knowledge bases',
    description: 'One portable Markdown file your agent owns — no lock-in, processed in the EU.',
    type: 'website',
  },
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" className={`${serif.variable} ${mono.variable}`}>
      <body>
        <AttributionInit />
        <PageViewTracker />
        <Header />
        <main>{children}</main>
        <Footer />
      </body>
    </html>
  );
}
