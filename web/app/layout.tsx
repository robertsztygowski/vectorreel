import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { Header } from '@/components/Header/Header';
import { Footer } from '@/components/Footer/Footer';
import { AttributionInit } from '@/components/AttributionInit';
import { PageViewTracker } from '@/components/PageViewTracker';
import './globals.css';

export const metadata: Metadata = {
  title: 'mdreel — Turn company video into Markdown your AI agents can use',
  description:
    "mdreel turns trainings, demos and meetings into clean, timestamped Markdown — capturing what's said and what's shown on screen. Processed entirely in the EU.",
  openGraph: {
    title: 'mdreel — video to Markdown for AI knowledge bases',
    description: 'Feed your videos to your agents without leaving the EU.',
    type: 'website',
  },
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
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
