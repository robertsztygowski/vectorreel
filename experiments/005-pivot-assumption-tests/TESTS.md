# 005 — Pivot assumption test pack (M8)

> **Status: ACTIVE PLAN — written 2026-07-21 (pivot marathon run, M8).**
> Point-in-time experiment designs. Results get recorded here when run; the designs freeze at
> that moment. **Numbers and decision rules are cited by METRICS.md name only and live there —
> if this memo ever disagrees with a living doc, the living doc wins.**

Minimal-engineering experiments mapping the pivot ("AI-ready knowledge collections /
repositories") onto the five open assumptions (METRICS.md §2). Every experiment obeys:

- **Ship evidence, not infrastructure** — each "smallest build" is publishing work or an
  already-shipped surface, never a new system.
- **The minimum-sample rule (METRICS.md §2.1) gates every reading.** No verdict below the floor.
- **A5 gates A1–A4.** Experiments 2–5 only produce data once experiment 1 produces traffic.

---

## E1 — A5 · Do collections acquire traffic at all?

- **Hypothesis:** public collections + weekly publishing (DISTRIBUTION.md cadence) draw
  qualified visitors without outreach.
- **Trigger metric:** **N15** (qualified-visitor floor), read per METRICS.md §6.5's
  qualified-visitor caveat; time-boxed by **§2.2 T**.
- **Smallest build:** nothing new — the three launch collections (DISTRIBUTION.md launch
  package) published on mdreel.com + GitHub, weekly batches per the publishing runbook, each
  with its one anchored distribution touch.
- **Read:** Umami sources panel — `gallery_view` + `collection_session_view` volume by
  `utm_source` (`github` vs `linkedin` vs organic).
- **Stop/go:** the §2.2 rule verbatim — N15 not reached by **T** after sustained publishing ⇒
  stop. Interim (T1) honest read only; no verdict earlier.

## E2 — A5b · Does GitHub distribute, specifically?

- **Hypothesis:** a clonable collection repo is itself a channel (stars/watch/clone → site
  visits), distinct from social posts.
- **Trigger metric:** contributes to **N15**; read as the `utm_source=github` row of the
  sources panel (METRICS.md §6.3 contract).
- **Smallest build:** the M5 operating model, already specced — org + three repos + README
  footers with UTM + weekly releases. Founder action: create the org (PLAN.md NEEDS-FOUNDER).
- **Read:** GitHub-native counters (stars, unique clones, release watchers) *alongside* the
  UTM row — GitHub counters are context, only the UTM row counts toward N15.
- **Stop/go:** if after ~3 months of weekly releases the `utm_source=github` row is a flat
  zero while other rows move, demote GitHub to a mirror (keep publishing — it's nearly free —
  but stop investing conventions/issue-forms effort in it).

## E3 — A1 · Is EU residency the wedge, or the capability story?

- **Hypothesis:** the EU-residency arm (A) converts visitors to signup no worse than the
  capability arm (B) — now both framed on the *repository* outcome (M4 hero rewrite).
- **Trigger metric:** headline A/B `signup` rate per arm (METRICS.md §4 A1); floor: hundreds
  of visitors per arm (§2.1).
- **Smallest build:** already shipped — the M4 hero keeps the A/B mechanism with both arms
  pivoted. Zero additional work; the experiment runs itself on E1's traffic.
- **Read:** Umami `signup_view`/`signup` by arm; unprompted GDPR mentions in inbound replies
  logged separately (DISTRIBUTION.md channel 6) as the stronger A1 signal.
- **Stop/go:** METRICS.md §2 A1 rule verbatim — Arm A doesn't clearly beat B ⇒ all positioning
  moves to the capability story. Do not rationalize a loss.

## E4 — A2 · Do consumers of collections convert to buyers?

- **Hypothesis:** the consume→convert path (M7) moves collection browsers into trials, and
  trials into checkout, at rates that clear the A2 rule.
- **Trigger metric:** `checkout_clicked` → `payment_succeeded` (METRICS.md §2 A2); the new
  first-party hop `collection_session_view` → `collection_convert_click` → `signup_view`
  (BUSINESS_MODEL §7.1) is the diagnostic *upstream* of it. Floor: ~100 trials + live checkout
  (§2.1).
- **Smallest build:** already shipped — M7 ConvertCta + events; Stripe checkout live
  (test-mode until launch).
- **Read:** the funnel in order. If `collection_convert_click` is healthy but signups stall,
  the leak is the signup form, not the demo; if clicks themselves are rare, the collections
  aren't creating want — fix the *collections* (better-known talks), not the CTA.
- **Stop/go:** METRICS.md §2 A2 rule verbatim (below-threshold checkout ⇒ vitamin; stop).

## E5 — A4 · Does repository structure earn more trust than flat files?

- **Hypothesis:** cross-session structure (topics/speakers/timeline) is what makes output feel
  citable — visible as deeper collection engagement preceding conversion, and later as
  `upload_repeat`.
- **Trigger metric:** **`upload_repeat`** (METRICS.md §2 A4; floor ~20 trial users). Proxy
  until then: share of `collection_convert_click` events carrying `from=collection_session`
  (arrived via a session deep-link and its indexes) vs `from=collection_index`.
- **Smallest build:** nothing — the events already distinguish `from` context (M7). Optional
  later (only if the proxy is inconclusive): one extra event on topic/timeline index views.
- **Read:** Umami event breakdown by `from` + `videoId`; later the A4 rule on real trials.
- **Stop/go:** A4 rule verbatim. Proxy reading is directional only — never invoke the A4 rule
  on proxy data (§2.1 discipline applies).

## A3 — deliberately not in this pack

A3 (flow vs backfill) has **no cheap pre-launch experiment**: its §2.1 floor is two cohorts
≥ 4 weeks apart or the **N20** proxy already built into signup. Anything else would be
infrastructure, not evidence. It reads itself once E1/E4 produce cohorts.

---

## Sequencing

E3/E4/E5 are passengers on E1's traffic — they are *already built* and cost nothing to run.
The founder's recurring effort goes to exactly one place: **the weekly publishing batch**
(DISTRIBUTION.md runbook). That is the whole point of the pack: after this run, closing A5 is
a publishing habit, not an engineering backlog.
