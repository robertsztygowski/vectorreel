'use client';

import { useEffect, useState } from 'react';
import { assignAbArmIfAbsent, type AbArm } from '@/lib/attribution';
import { ArtifactCard } from './ArtifactCard';

const ARM_COPY: Record<AbArm, { h1: string; lead: string }> = {
  A: {
    h1: 'Your recordings never leave the EU.',
    lead:
      "One portable, timestamped Markdown file your agent can cite — separating what's said from what's shown on screen. No retrieval stack to lock you in, processed entirely in the EU, source video deleted after processing.",
  },
  B: {
    h1: "Your AI assistant can't cite what's on screen in your videos.",
    lead:
      'mdreel turns your demos, trainings and walkthroughs into one portable Markdown file your agent owns — capturing on-screen text and demos, not just the transcript. No lock-in; bring your own RAG.',
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
            <a className="btn btn-primary" href="/signup">
              start free — 1 hour
            </a>
            <a className="btn btn-ghost" href="#how">
              see how it works ↓
            </a>
          </div>
          <p className="micro">no credit card · one portable .md file · bring your own RAG</p>
        </div>

        <div className="hero-visual">
          <ArtifactCard />
          <p className="artifact-note">↑ actual output — one file, agent-citable</p>
        </div>
      </div>
    </section>
  );
}
