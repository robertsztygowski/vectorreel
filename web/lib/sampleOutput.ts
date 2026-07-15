// Canonical mock output for the private-upload path. Deliberately NOT one of the CC BY corpus
// fixtures — those are real creators' content, and using one as a stand-in for the user's own
// private recording would misattribute it. Matches the ARCHITECTURE §4 output contract shape.

export interface SampleOutputArgs {
  sourceFilename: string;
  durationSec: number;
  processedAt: string;
}

function formatDuration(totalSeconds: number): string {
  const total = Math.max(0, Math.round(totalSeconds));
  const h = Math.floor(total / 3600);
  const m = Math.floor((total % 3600) / 60);
  const s = total % 60;
  return [h, m, s].map((n) => String(n).padStart(2, '0')).join(':');
}

export function buildSampleOutput({ sourceFilename, durationSec, processedAt }: SampleOutputArgs): string {
  return `---
title: "Q3 Platform Demo — Billing Module"
source_filename: "${sourceFilename}"
duration: "${formatDuration(durationSec)}"
language: "en"
processed_at: "${processedAt}"
generator: "mdreel-mock@phase2"
summary: "A walkthrough of the billing module's invoice workflow, covering proration and the admin panel."
tags: [demo, billing, azure]
---

# Q3 Platform Demo — Billing Module

## [00:00:00] Introduction
**Spoken:** Thanks for joining — today we'll walk through the billing module, focusing on the new invoice workflow and how proration is applied.
**On screen:**
> mdreel — Q3 Platform Demo
**Visual:** Title card, then a cut to the presenter's screen share of the admin dashboard.

## [00:03:40] Invoice workflow walkthrough
**Spoken:** We open the invoice editor and apply proration before sending. The service call underneath is a straightforward async create.
**On screen:**
> Invoices → Create → "Apply proration" checkbox
> code: InvoiceService.CreateAsync(...)
**Visual:** Presenter navigates the admin panel, opens the invoice editor, and toggles the proration checkbox before saving.

## [00:09:12] Wrap-up
**Spoken:** That's the core flow — happy to take questions on edge cases like mid-cycle plan changes.
**On screen:**
> Questions? billing-team@example.com
**Visual:** Presenter returns to the title card.

## Source & licence

This output was generated from your own uploaded recording (mock — Phase 2, no real pipeline yet). Source video deleted after processing per default retention policy.
`;
}
