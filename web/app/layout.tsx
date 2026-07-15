import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { Header } from '@/components/Header/Header';
import { Footer } from '@/components/Footer/Footer';
import { AttributionInit } from '@/components/AttributionInit';
import { PageViewTracker } from '@/components/PageViewTracker';
import './globals.css';

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
