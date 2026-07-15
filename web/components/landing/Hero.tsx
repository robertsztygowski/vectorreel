'use client';

import { useEffect, useState } from 'react';
import { assignAbArmIfAbsent, type AbArm } from '@/lib/attribution';
import { CodeCard } from '../CodeCard/CodeCard';

const ARM_COPY: Record<AbArm, { h1: string; lead: string }> = {
  A: {
    h1: 'Your recordings never leave the EU.',
    lead:
      "mdreel processes trainings, demos and meetings into clean, timestamped Markdown — capturing both what's said and what's shown on screen. Processed entirely in the EU, source video deleted after processing.",
  },
  B: {
    h1: "Your AI assistant can't see what's on screen in your videos.",
    lead:
      'mdreel turns company recordings into clean, timestamped Markdown your agents can actually cite — capturing on-screen text and demos, not just the transcript.',
  },
};

const SAMPLE_MARKDOWN = `---
title: "Q3 Platform Demo — Billing Module"
duration: "00:47:12"
language: "en"
tags: [demo, billing]
---

# Q3 Platform Demo — Billing Module

## [00:03:40] Invoice workflow
**Spoken:** We open the invoice editor and
apply proration before sending…
**On screen:**
> Invoices → Create → "Apply proration"
> code: InvoiceService.CreateAsync(...)
**Visual:** Presenter navigates the admin panel.`;

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
          <span className="pill">🇪🇺 EU-hosted · GDPR-first · no lock-in</span>
          <h1>{copy.h1}</h1>
          <p className="lead">{copy.lead}</p>
          <div className="cta-row">
            <a className="btn btn-primary" href="/signup">
              Start free — 2 hours
            </a>
            <a className="btn btn-ghost" href="#how">
              See how it works
            </a>
          </div>
          <p className="micro">No credit card · Plain Markdown out · Bring your own RAG</p>
        </div>

        <div className="hero-visual" aria-hidden="true">
          <CodeCard title="demo-billing.md" content={SAMPLE_MARKDOWN} rotate />
        </div>
      </div>
    </section>
  );
}
