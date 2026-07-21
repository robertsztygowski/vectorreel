'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { assignAbArmIfAbsent, type AbArm } from '@/lib/attribution';
import { ArtifactCard } from './ArtifactCard';

const ARM_COPY: Record<AbArm, { h1: string; lead: string }> = {
  A: {
    h1: 'Hundreds of hours of video, one repository your AI can explore.',
    lead:
      'mdreel turns your recordings into an AI-ready knowledge repository — sessions, topics, speakers and a timeline your agent can browse and cite down to the second. Plain, portable Markdown files inside; no retrieval stack to lock you in, processed entirely in the EU.',
  },
  B: {
    h1: "Your team's knowledge is trapped in video your AI can't read.",
    lead:
      'mdreel turns demos, trainings and walkthroughs into an AI-ready repository your agent owns — capturing on-screen text and demos, not just the transcript, and cross-linking every session by topic, speaker and timestamp. Plain Markdown inside; bring your own RAG.',
  },
};

export function Hero() {
  // Renders Arm A during SSR (no layout shift), then swaps to whatever arm this visitor is
  // assigned client-side. Calls assignAbArmIfAbsent() directly rather than relying on
  // AttributionInit having already run, so there's no cross-component mount-order dependency.
  const [arm, setArm] = useState<AbArm>('A');
  useEffect(() => {
    setArm(assignAbArmIfAbsent());
  }, []);
  const copy = ARM_COPY[arm];

  return (
    <section className="hero">
      <div className="wrap hero-grid">
        <div className="hero-copy">
          <div className="hero-meta">
            <span><b>region:</b> eu-central</span>
            <span><b>gdpr:</b> compliant</span>
            <span><b>lock-in:</b> none</span>
          </div>
          <h1>{copy.h1}</h1>
          <p className="lead">{copy.lead}</p>
          <div className="cta-row">
            <Link className="btn btn-primary" href="/gallery">
              explore the public collections
            </Link>
            <Link className="btn btn-ghost" href="/signup">
              build one from your videos →
            </Link>
          </div>
          <p className="micro">first hour free · no credit card · plain Markdown inside · bring your own RAG</p>
        </div>

        <div className="hero-visual">
          <ArtifactCard />
          <p className="artifact-note">↑ one session from a repository — agent-citable to the second</p>
        </div>
      </div>
    </section>
  );
}
