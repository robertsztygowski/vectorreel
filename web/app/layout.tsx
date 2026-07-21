import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import localFont from 'next/font/local';
import { Header } from '@/components/Header/Header';
import { Footer } from '@/components/Footer/Footer';
import { AttributionInit } from '@/components/AttributionInit';
import { PageViewTracker } from '@/components/PageViewTracker';
import { UmamiAnalytics } from '@/components/UmamiAnalytics';
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
  title: 'mdreel — AI-ready knowledge repositories from your video',
  description:
    'mdreel turns hundreds of hours of demos, trainings and talks into an AI-ready knowledge repository your agents can explore and cite — sessions, topics, speakers, timeline. Plain Markdown inside, no lock-in, processed entirely in the EU.',
  openGraph: {
    title: 'mdreel — AI-ready knowledge repositories from video',
    description:
      'An explorable, citable knowledge repository your agent owns — plain Markdown inside, no lock-in, processed in the EU.',
    type: 'website',
    images: [{ url: '/og-card.png', width: 1200, height: 630 }],
  },
  twitter: {
    card: 'summary_large_image',
    title: 'mdreel — AI-ready knowledge repositories from video',
    description:
      'An explorable, citable knowledge repository your agent owns — plain Markdown inside, no lock-in, processed in the EU.',
    images: ['/og-card.png'],
  },
  icons: {
    icon: [{ url: '/favicon.ico', sizes: '16x16 32x32 48x48', type: 'image/x-icon' }],
  },
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" className={`${serif.variable} ${mono.variable}`}>
      <body>
        <AttributionInit />
        <PageViewTracker />
        <UmamiAnalytics />
        <Header />
        <main>{children}</main>
        <Footer />
      </body>
    </html>
  );
}
