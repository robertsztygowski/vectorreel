# vectorreel — Workflow 2 Validation Kit

> **Date:** 2026-07-14 · **Question:** Will anyone want this — and can we even reach them?
> **Authority:** PLAN.md · BUSINESS_MODEL.md · DISTRIBUTION.md · METRICS.md.
> W1 (`experiments/agents/out/w1-decision-memo.md`) is a **superseded** reasoning trail, quoted
> here only where its status banner says it still holds. Per CLAUDE.md: **the living doc wins.**
> **Scope:** demand instrument only. **No product code.**
> Point-in-time artifact — not a living doc, **not authoritative** (CLAUDE.md).

> ## ✅ Graduated 2026-07-14 — these are now owned by living docs, not by this memo
>
> Per DEVELOPMENT.md §7 (*"graduate before you archive"*), everything here that was a **decision**
> rather than a **draft** has been moved into a living doc. **Those copies govern; if this file
> disagrees with them, they win.**
>
> | From this kit | Now lives in |
> |---|---|
> | The signup one-field question (archive-hours vs monthly-hours) — the early A3 read | **METRICS.md N20** |
> | The checkout-abandon micro-survey — the only place "I could build this myself" can be said | **METRICS.md N21** |
> | The reply-to invitation — *receiving* a reply is not *doing* outreach | **METRICS.md N22 + DISTRIBUTION.md §6** |
> | The minimum-sample rule — no verdict below the floor; don't celebrate a false win either | **METRICS.md §2.1** |
> | The positioning statement (EU clause deliberately second) | **BUSINESS_MODEL.md §4** |
> | Trial-start-rate-by-referrer as the earliest high-value read | **METRICS.md §4 (A5)** |
>
> **What remains here and has *not* graduated:** the landing-page copy, the lifecycle email drafts,
> and the channel yield estimates. These are **drafts to be executed in Phase 0.3**, not decisions —
> they graduate into `web/` when the page is built, and should be rewritten freely before then.

---

## 0. Context Recap — and the reshape W2 had to undergo

### What the docs say, in the five bullets that shape this kit

- **The riskiest assumption is no longer a demand assumption. It is A5 — distribution.** The
  founder ruled out customer outreach, so **traffic is the long pole and it kills the company
  first** (DISTRIBUTION.md). A1/A2/A3 are still weak-evidence Value/Business risks, but they are
  now only *reachable through traffic*. W1 ranked A5 fifth; the living docs rank it first, and
  the living docs win.
- **The riskiest *demand* assumption, once traffic exists, is A2 — "buyers will buy rather than
  DIY."** Weak evidence. The ICP is the audience best equipped to build this in a weekend; we are
  selling a wrapper to wrapper-builders. **A1 (EU-as-purchase-driver) is the co-risk**, and
  BUSINESS_MODEL §5a makes its stakes explicit: if A1 fails there is no EU premium, and we compete
  on features inside a €19M SAM against Cloudglue — *"a materially worse business."*
- **ICP: BUSINESS_MODEL §5 #1** — software companies / IT departments, **50–500 employees**,
  building internal AI assistants. Developer buyer, uses the API, has Teams/Zoom recordings and an
  AI-adoption mandate. Beachhead **Poland + Nordics/DACH**.
- **Pricing frame is measured and safe.** All-in COGS **€0.011/video-minute (€0.65/video-hour)**,
  ~7× inside the cheapest list price (Business @ full 150 h = €0.077/min, 85.7% margin). COGS is
  solved and is *not* a risk. But **BUSINESS_MODEL §6's tier table is explicitly gated on A3** and
  must not be locked until Phase 5 — so **this kit publishes no prices.**
- **W2 is PLAN.md Phase 0.3** — *"Demand instrument: publish (no product code) — 🚨 THE LONG
  POLE."* It runs **in parallel** with Phases 0.1/0.2, never after them, and it consumes Phase
  0.2's public CC corpus as its raw material.

### 🛑 Two of the four requested steps are dead. Here is what replaced them.

The workflow brief asked for **cold outreach to ~20 target users** and a **20-minute discovery
interview guide**. Both are now explicitly forbidden:

> *"The founder ruled out customer outreach. Everything in this memo that depends on talking to
> people — the concierge test, the interview script, the cold-outreach A/B, 'sell it by hand' —
> **is dead**."* — W1 memo, supersession banner
>
> *"The previous plan opened with 'Design partners first'. **That is outreach, and the founder has
> ruled it out.** The motion is self-serve / inbound."* — BUSINESS_MODEL §7

**I have not written them.** Writing a cold-email sequence and an interview guide would produce a
document that reads well and that the founder has already decided not to execute — the exact
failure mode the `ead7c89` commit was written to stop. Instead, each agent's craft is redirected
to its inbound equivalent:

| Requested step | Status | What this kit contains instead |
|---|---|---|
| 1 · product-manager — positioning + ICP | ✅ **Alive** | Same, but *"where to find 20 of them"* becomes **"which channel puts 20 of them in front of us"** — a channel spec, not a prospect list. |
| 2 · content-marketer — landing page | ✅ **Alive, and now the centre of gravity** | The landing page *is* the experiment. Both A/B arms + the launch artifact + the LinkedIn post that drives the traffic. |
| 3 · sales-automator — cold outreach | 🛑 **Dead** | **Lifecycle email.** Same craft — sequences, one CTA, objection handling — pointed at people who *already raised their hand*. Automated, 1-to-many, **zero 1:1 contact**. |
| 4 · ux-researcher — interview guide | 🛑 **Dead** | **The no-conversation evidence design.** What removing the interview *costs* us, and the cheapest instruments that recover part of it without a single outbound message. |

### ⚠️ And a third correction: "landing page + no product" no longer reaches the wow

DISTRIBUTION.md's core insight is *"the product is the marketing"* — the free YouTube tool is the
hook. But that tool is **product code** (PLAN.md Phase 3), and W2 is explicitly a no-code pass.

**The resolution — and it costs nothing.** The Phase 0.2 CC corpus is processed **by hand**,
through the `experiments/001/` harness that already exists, and the resulting Markdown is
published as **static pages**. A pre-rendered gallery is indistinguishable from a live tool for
the only purpose that matters right now — *proving the output is real and getting an email
address*. It is the free YouTube tool with the interactivity removed and **~€3 of Vertex spend**
in place of a sprint.

> **W2 therefore ships: a landing page (A/B), a statically pre-rendered gallery, the artifact
> post, email capture, and analytics. Nothing else. No product code.**

### ⚠️ Fourth correction: "~20 target users this week" is not achievable, and shouldn't be the goal

Under an outbound motion, 20 users this week is a send list. Under an **inbound** motion it is
not a thing you can decide to have. DISTRIBUTION.md's own math is **2,000–5,000 qualified
visitors → ~5 paying customers**, and *"content compounds over months."* One LinkedIn post to a
warm audience plausibly yields a few hundred impressions and a few dozen clicks — **not 20
qualified trials, and certainly not in seven days.**

**So W2's honest deliverable is not 20 users. It is: the instrument is live, the events are
firing, and the tripwires are armed** — so that the 20th, 100th and 2,000th visitor each get
counted correctly whenever they arrive. **See §5: at W2 scale, none of METRICS.md's thresholds
can legitimately fire, and pretending otherwise is how a founder talks himself into a false
positive.**

---

## 1. Positioning & ICP  *(agent: volt/product-manager)*

### Positioning statement

> **For EU engineering teams building an internal AI assistant, vectorreel turns the recordings
> your assistant can't see — demos, trainings, incident reviews — into timestamped Markdown it
> can cite, without the footage ever leaving the EU.**

The EU clause sits **second, as a qualifier**, which is the honest reflection of A1's evidence
level. It gets promoted to the front of the sentence **if and only if Arm A wins the headline
A/B**; if it loses, it is cut from the positioning entirely and demoted to a trust section
(DISTRIBUTION.md decision rule — *"do not rationalize a loss"*).

**Not saying:** "EU-sovereign" (Google is US-controlled — BUSINESS_MODEL §4 honesty rule) ·
"better than Cloudglue" · **"transcription"**, ever (BUSINESS_MODEL §8, buyer-confusion risk).

### ICP

| | |
|---|---|
| **Who** | Software company or in-house platform/IT team, **50–500 employees**, EU-domiciled. The person who acts is a **hands-on engineer or eng lead** — Head of Platform, AI Lead, Principal Engineer, CTO at the small end — who has personally been told to *"make our knowledge available to an AI assistant."* |
| **Trigger (the hard screen)** | An **active RAG / internal-assistant / MCP project**, shipped or in flight, in the last 6 months. Without it they are not in market and no amount of copy converts them. |
| **The specific pain** | Their knowledge base has the wiki, Confluence, Slack, the repo. It does **not** have the 40-minute architecture walkthrough, the customer demo, the incident post-mortem, the onboarding series. So the assistant answers confidently from a stale wiki page while the correct answer is something a colleague **said and showed** in a recording six months ago. **The sharp edge: what was on the screen — the config value, the diagram, the click path — is exactly what nobody wrote down, and exactly what a transcript throws away.** |
| **Current workaround** | In expected order: (1) **nothing — video is silently out of scope**, and everyone has quietly accepted the assistant is blind to it; (2) somebody piped Whisper/Teams transcripts in, answers got *worse*, and they were pulled back out; (3) an engineer spent a weekend wiring video into Gemini, it worked on one file, and it is now unmaintained; (4) a human writes summaries by hand. **(1) and (2) are the buyers. (3) is A2 walking around in the wild.** |
| **Disqualify** | No AI/RAG project in flight · <50 or >500 staff · non-EU · the recordings are *public marketing* video (that's the free tool, never the paid product — BUSINESS_MODEL §10). |

### Where 20 of them find *us* — the channel spec

**This replaces "where to find 20 of them." We do not go and get them.** Ranked by
DISTRIBUTION.md's priority order, with the honest yield estimate for **week 1**:

| # | Channel | Why it reaches this ICP | Realistic week-1 yield |
|---|---|---|---|
| **1** | **LinkedIn — the founder's .NET/architecture audience.** Broadcasting, not outreach: zero 1:1 contact. | Cheapest available by an order of magnitude, and **it already exists.** An owned audience is a bootstrapper's single biggest asset. | A few hundred impressions; **tens of clicks; low single-digit email captures.** Not 20 trials. |
| **2** | **The curated gallery** (statically pre-rendered from the Phase 0.2 CC corpus). | Compounding SEO asset + permanent live demo + proof the output is real. **Curated, CC-only, attributed — never a transcript farm** (CLAUDE.md r8). | **~Zero in week 1.** This is a months-scale asset. Ship it now *because* it is slow. |
| **3** | **HN / r/RAG / r/LocalLLaMA** — the artifact post. | Highest-variance surface. Right audience, wrong reliability. | 0 or 2,000. **One-shot, non-compounding — a spike, not a strategy** (DISTRIBUTION.md). |
| **4** | Technical blog — *sell the boring hard parts*. | Directly answers A2/DIY: chunking, cost engineering, runaway generation, GDPR honesty. | Months. Start now anyway. |

> 🚩 **The uncomfortable open question, straight from DISTRIBUTION.md, and W2's most valuable
> byproduct:** *is the .NET/architecture audience the actual **buyer**, or a friendly crowd that
> claps and doesn't convert?* **An owned audience that isn't the ICP is a vanity asset.**
> `page_view` and `signup` both carry `referrer` — **watch trial-start rate by referrer from the
> very first visitor.** If LinkedIn traffic bounces and r/RAG traffic converts, the entire
> channel strategy is wrong and cheap to fix. If we learn that in week one, W2 has paid for itself
> regardless of what A1 or A2 do.

---

## 2. Landing Page Copy  *(agent: wshobson/content-marketer)*

> One page. One CTA. **No pricing table** — A3 is unanswered, and publishing tiers now would
> pre-commit the business model that METRICS.md exists to *derive*.

### Hero — the A1 experiment

Identical below the fold. Assign `ab_arm` on first `page_view`, sticky per session.

**Arm A — EU-lead**
> # Your recordings never leave the EU.
> Turn internal video — demos, trainings, incident reviews — into timestamped Markdown your AI
> assistant can cite. Processed entirely in EU regions. Source video deleted after processing.

**Arm B — capability-lead**
> # Your AI assistant can't see what's on screen in your videos.
> It has read your wiki, your repo, your tickets. It has never seen the 40-minute demo where
> someone actually *showed* how the thing works. vectorreel turns that recording into timestamped
> Markdown your assistant can cite — and it never leaves the EU.

> **Primary metric: `signup` rate, not click-through.** Clicks measure curiosity; signups measure
> intent (METRICS.md A1). **If Arm A does not clearly beat Arm B, EU is a checkbox — move all
> positioning to the capability story, and do not rationalize the loss.**

### Three value props

**1. A transcript throws away the answer.** *(→ A4, and the whole capability claim)*
The config value, the architecture diagram, the exact UI path, the code on the slide — none of it
is spoken aloud, so none of it survives transcription. We capture **what was shown** separately
from **what was said**, so your retrieval layer can weight them differently. *The demo below
answers a question that is only answerable from a screen.*

**2. Every claim carries a timestamp, so your assistant can cite it.** *(→ A4)*
Every section is anchored `[00:14:32]`. When the assistant answers, it links to the second the
answer came from, and a human verifies it in one click. **A hallucinated timestamp poisons a
knowledge base and the trust doesn't come back** — so the output is built to be *checkable*, not
merely fluent.

**3. Plain Markdown files. No lock-in, nothing to rip out.** *(→ A2, honestly)*
You get files. Put them in your repo, your bucket, Obsidian, SharePoint, your vector DB. There is
no retrieval stack of ours to adopt and no format to escape. We do the part nobody enjoys —
chunking long video, holding cost down, keeping the schema identical across 500 files, retrying
the runs that fall over — and then we get out of the way.

### How it works

1. **Video in.** Upload a file, or POST to the API.
2. **We read it twice — audio *and* screen.** Speech transcribed and cleaned; frames read for
   on-screen text, slides, code, UI state; the two fused into one document.
3. **Markdown out.** YAML frontmatter, topic sections, `[hh:mm:ss]` anchors, and a clean split
   between *spoken* and *shown on screen*. **Identical schema on every file.**
4. **The source video is deleted.** Default — not a setting you have to go and find.

*(→ The artifact: side-by-side **plain transcript vs. vectorreel Markdown**, plus a RAG answering
a question only answerable from on-screen content. Built on a **public CC-licensed video** from
Phase 0.2, so it can actually be shared. **This is the entire pitch in one image** and the single
highest-value inbound asset we own — DISTRIBUTION.md.)*

### EU & GDPR — the honest version

> **True today.** Processing runs in **EU regions only** (GCP `europe-central2` / `europe-west3`),
> under Google's DPA with Vertex AI **no-training terms**. Your source video is **deleted after
> processing**, by default. We never train on customer data — contractually, ever. The
> subprocessor list is short and published.
>
> **Not true today, and we won't pretend otherwise.** This is **EU data residency, not EU
> sovereignty.** Google is a US company and therefore in scope of the CLOUD Act. Any vendor
> telling you "EU region" means your data is beyond US legal reach is telling you something your
> DPO will take apart in ten minutes.
>
> **Where we're going.** An **EU-owned infrastructure option** (OVHcloud / Scaleway) is on the
> roadmap; a self-hosted edition after it.

*(BUSINESS_MODEL §4 honesty rule. This page's real reader is the buyer's DPO, and overclaiming
gets destroyed by a competent one. **Honesty is the sales asset.** Note this section is
**gated on A1** — if Arm A loses, it stays on the page but stops being the lead message.)*

### FAQ

**Is this just transcription?**
No — and if transcription were enough you'd be done already: Whisper is free. The value is in what
a transcript *cannot* contain: the slide, the config, the diagram, the click path. We produce one
document that keeps *shown* and *said* apart, and timestamps both.

**Why wouldn't I just send the video to Gemini myself?**
For one video, you should — genuinely, it works, and we'd rather you find that out than be sold
something. It stops working somewhere around video fifty: chunking an hour-long recording without
losing context at the segment boundary, keeping the schema stable across every file so retrieval
doesn't degrade, capping cost per hour, and retrying the **~8% of model calls that try to emit 60k
tokens and fall over.** That plumbing is the product. **We publish our real cost per video-hour so
you can check the arithmetic against your own.**

**What happens to our video?**
Deleted after processing, by default. We keep the Markdown you asked for and the job's cost
ledger. We are not building a video library — **there is nothing here to breach.**

**Is it accurate enough to put in a knowledge base?**
The right question, and *"wrong is worse than absent"* is the standard. Every claim is timestamped,
so any answer is verifiable in one click. We publish benchmarks per content type — screen
recording, slide talk, talking head — **including where the output is weakest.**

**What does it cost?**
There's no pricing page yet, deliberately: we're setting pricing from what early users actually
do, not from a guess. What we can tell you is our measured all-in cost — **€0.65 per video-hour** —
and we publish how it breaks down.

### Primary CTA — one, and only one

> ### See it on a video you already know.
> Pick a talk from the gallery. Read the Markdown it produced. Then ask yourself whether your
> assistant could answer from that.
>
> **[ Browse the gallery → ]**  ·  *secondary, below the fold:* **[ Tell me when I can upload my own → ]** (email)

**Why the gallery, not the email box, is the primary CTA:** DISTRIBUTION.md is explicit that the
old funnel's biggest leak was opening with *"upload your confidential internal recording to a
stranger's website."* An email field is a smaller ask than that, but it is still an ask **placed
before any value has been delivered.** The gallery costs the visitor nothing, risks nothing, and —
because they already know the talk — lets them **verify the output is good without trusting us at
all.** The email capture comes *after* the wow, which is the entire architecture of this funnel.

---

## 3. Lifecycle Email  *(agent: wshobson/sales-automator — redirected)*

> 🛑 **The cold-outreach sequence this step originally called for is not written, and must not
> be.** Outreach is ruled out. **What survives is the craft:** short, one-CTA, value-first
> sequences with real objection handling — pointed at people who **already raised their hand.**
> Every message below is **automated and 1-to-many. Zero 1:1 contact. This is not outreach.**

**Trigger:** the visitor gave an email address after seeing the gallery. That is the *only* entry
point. There is no list, and there is no send button.

### Email 1 — immediate, on email capture · 71 words

**Subject:** the Markdown from that talk

> Thanks for the address.
>
> Here's the full output for the talk you were looking at, as a file: [link]. Drop it into your own
> RAG and ask it something that's only shown on screen, never said out loud. That's the test that
> matters — and it's the one a transcript fails.
>
> Private uploads open shortly. I'll send you one email when they do, and nothing else.
>
> — {name}

*(The promise "nothing else" is load-bearing. It is also the reason this list stays clean.)*

### Email 2 — when private upload opens · 84 words

**Subject:** you can upload your own recording now

> You asked to know when this opened — it's open.
>
> **Two hours of video, free, no card.** Magic link, no password: [link]
>
> The one to try first is the recording your assistant *should* have seen and hasn't — the
> architecture walkthrough, the demo, the incident review. Not the polished marketing video.
>
> Processed in the EU. **Your source file is deleted after processing.** You get Markdown files
> you own outright — there's nothing here to migrate off later.

### Email 3 — on `job_completed`, the wow moment · 66 words

**Subject:** your Markdown is ready

> Done — [download].
>
> **The test I'd run:** ask your assistant something that was only ever *shown* on screen in that
> recording. A config value, a diagram, a click path. If it answers and cites the timestamp,
> that's the whole product.
>
> **It cost us €{cost} to process. Reply and tell me what it got wrong** — I read every one.

> 🔑 **"Reply and tell me what it got wrong" is the most important line in this kit.**
> The founder ruled out *doing* outreach. He did not rule out *receiving* a reply. This costs zero
> founder-hours until someone writes back, and anyone who does is self-selected, high-intent, and
> holding the exact qualitative signal METRICS.md admits we threw away. **It is the only surviving
> channel through which a human sentence can reach us — and it does not violate the constraint.**
> Publishing the per-job cost in the same breath is not a flex; it is the A2 argument (*"here is
> what DIY would cost you, before your time"*) made without a single sales word.

### Email 4 — day 3 after first job, only if no second upload · 58 words

**Subject:** one more?

> One question, then I'll stop.
>
> You processed one recording. **Did it earn a second?**
>
> If yes: [upload another]. If no — hit reply with the reason, even one word. *"Timestamps were
> off"* or *"nobody's asked for this"* are both genuinely more useful to me than silence.

*(This email is instrumentation wearing a friendly hat. **`upload_repeat` is the A4 signal** —
METRICS.md: *"satisfaction surveys measure politeness; a second upload measures trust."* The
reply-with-a-reason ask is the cheapest possible recovery of the interview we're not allowed to
run.)*

### Objection handling — relocated

**There is no call, so there is no objection-handling script.** Every objection must be pre-answered
in copy, or it is never answered at all. That is the real cost of removing the conversation, and
it is why the FAQ above is written the way it is.

| The objection | Where it now lives |
|---|---|
| *"We'd just use Gemini directly."* | FAQ #2 + the technical blog. **The only honest answer is to concede the easy case and show the hard one.** |
| *"Send me pricing."* | FAQ #5. No pricing page — A3 is unanswered. |
| *"Our DPO would have to review this."* | The GDPR section, with the CLOUD Act conceded up front. |
| *"We don't have that much video."* | 🚨 **Nowhere — and this is A3.** It cannot be pre-answered in copy because **it is a question we need to ask *them*.** See §4. |

### Tracking

Per METRICS.md's event schema — `page_view`, `signup`, `upload_started`, `job_completed`,
`output_downloaded`, **`upload_repeat`**, `checkout_clicked`, `payment_succeeded` — all carrying
`referrer` and `ab_arm`. **Not a CRM. Not a marketing automation suite.** A table.

---

## 4. Evidence Without Conversation  *(agent: volt/ux-researcher — redirected)*

> 🛑 **The 20-minute discovery interview guide is not written.** Interviews are outreach.
> **The honest work of this step is to state what that costs, and to design the cheapest
> instruments that recover part of it without a single outbound message.**

### What we gave up — METRICS.md says this itself, and it should not be softened

> *"Removing the conversation means **the Stripe link now carries the entire burden of detecting a
> vitamin.** There is no longer any human signal upstream of payment."* — W1 memo supersession
> banner
>
> *"Enthusiasm without payment is the single most common way a vitamin gets mistaken for a
> painkiller, and it is precisely what the concierge test was designed to expose."* — METRICS.md,
> Anti-metrics

**The three things the interview would have caught that nothing else now catches:**

1. **The *why* behind a non-conversion.** A funnel tells you 95% didn't click checkout. It never
   tells you whether that was price, trust, output quality, or *"nobody has ever asked me for
   this."* **Those four require four completely different responses, and the funnel cannot
   distinguish them.**
2. **A1 unprompted.** The headline A/B measures *which sentence converts better*. It cannot tell
   you whether GDPR is a genuine blocker or a form field, because we are the ones who raised it.
   **A/B-testing our own claim is a weaker instrument than hearing a stranger raise it first, and
   we should be honest that we downgraded here.**
3. **A3 before the cohort matures.** Cohort hour-decay is the *right* metric and it takes **two
   months** to read. A single interview question — *"archive hours vs. hours recorded per month"* —
   predicts it in thirty seconds.

### The four instruments that recover what can be recovered — all zero-outreach

**Instrument 1 — the one-field signup question.** *(A3, early read)*
METRICS.md explicitly asks for this: *"capture at signup, if it can be asked in one field without
friction: archive hours vs. hours recorded per month."*

> **On the magic-link screen, one control, skippable:**
> *"Roughly how much video do you have?"*
> `[ ~___ hours in the archive ]` and `[ ~___ hours added per month ]`

- **A ratio > 20:1 predicts backfill** — two months before the cohort data can say so.
- **Skippable, and the skip rate is itself data.** If 80% skip, we've learned they don't know —
  which for a "we have a video problem" claim is a finding in its own right.
- **This is the single highest-value non-code line in the kit**: one form field, answering the
  assumption that decides *what business this is*.

**Instrument 2 — the checkout-abandon micro-survey.** *(A2, the *why*)*
One question, one click, fires on `checkout_clicked` without `payment_succeeded` inside 24h:

> **"What stopped you?"** → `Too expensive` · `Output wasn't good enough` · `I could build this
> myself` · `Not my decision to make` · `Just looking`

**"I could build this myself" is the A2 kill signal**, and this is the only place in a
conversation-free funnel where it can ever be uttered. Five radio buttons is a poor substitute for
an interview — **it is also infinitely better than the zero we currently have.**

**Instrument 3 — the reply-to invitation.** *(A1/A2/A4, qualitative)*
Every lifecycle email invites a reply (§3). **Inbound replies are not outreach**, they cost nothing
until they arrive, and every one is a self-selected high-intent user. **Read every reply. Log
GDPR mentions separately** — an *unprompted* GDPR mention in an inbound reply is the closest thing
to the real A1 test that survives, and it is worth more than the headline A/B.

**Instrument 4 — `upload_repeat`.** *(A4)*
Not a survey. **A user who uploads once and never returns did not find the output citable,
whatever they would have said in an interview** (METRICS.md). This is the one place where removing
the conversation made the instrument *better*.

---

## 5. Validation Criteria

### The thresholds — METRICS.md is authoritative, reproduced so this kit is self-contained

| | Assumption | Metric | Decision rule |
|---|---|---|---|
| **A5** | Distribution works (**top risk**) | Qualified visitors | **< 2,000 reachable ⇒ the business dies here.** Nothing downstream gets a vote. |
| **A1** | EU is a purchase driver | `signup` rate, headline A/B | Arm A doesn't clearly beat Arm B ⇒ **move all positioning to capability.** |
| **A2** | Buy, not DIY | `checkout_clicked`; **`payment_succeeded`** | < 5% of trials reach checkout, or **0 payments after ~100 trials ⇒ it's a vitamin. Stop.** |
| **A3** | Flow, not backfill | Cohort hour-decay | Month-2 hours < 20% of month-1 ⇒ **not a subscription. Prepaid credit packs.** |
| **A4** | Output is citable | `upload_repeat` | < 30% second-upload ⇒ **the output didn't earn trust.** |

### 🚨 The statistical honesty section — read this before reading any dashboard

**At W2 scale, not one of the five rules above can legitimately fire.** They need ~2,000 visitors
and ~100 trials. Week one will produce tens of visitors and, plausibly, zero trials.

**This is the single most dangerous moment in the whole plan.** A founder staring at a dashboard
with 40 visitors, 2 signups and 0 payments will be *powerfully* tempted to read a verdict into it
— and every available verdict is wrong:

- **"Arm A got 3 signups and Arm B got 1 — EU is the wedge!"** At n=4 this is coin-flipping.
  DISTRIBUTION.md pre-commits to *"do not rationalize a loss"*; the symmetric discipline is **do
  not celebrate a win either.** The A1 A/B needs *hundreds* of visitors per arm before it says
  anything. Until then it is a chart, not evidence.
- **"Nobody paid — it's a vitamin."** With no checkout link built and no trials run, **zero
  payments is not an A2 result.** It is the absence of a result.
- **"Only 40 visitors — distribution is dead."** With one LinkedIn post published, **this is not
  an A5 result either.** A5 is a verdict on a *sustained content effort*, not on a first post.

**Therefore W2 defines its own, honest, exit conditions:**

| W2 is **done** when | W2 has **failed** when |
|---|---|
| The page is live with both arms and `ab_arm` is sticky and recorded | — |
| The gallery renders ≥ 4 CC talks with attribution + embedded original | Phase 0.2 didn't produce publishable output ⇒ **A4 is the blocker, not demand** |
| The artifact post is published and LinkedIn has seen it | — |
| **Every METRICS.md event fires and lands in a table you can query** | Events are missing ⇒ **stop everything.** Cohort hour-decay *cannot be reconstructed retroactively.* |
| The signup form asks archive-hours vs monthly-hours | — |
| **A written "first honest read" date is set — ≥ 6 weeks out — and no verdict is called before it** | A verdict gets called at n=40 |

> **W2's deliverable is a working instrument and an armed tripwire. It is not a verdict, and
> anyone who reports one from this week's data — including me — should be ignored.**

### The one thing W2 *can* read early, and it is worth the whole exercise

**Trial-start rate by `referrer`, from the very first visitors.** It needs far less volume than the
A/B, because the effect size is potentially enormous: if LinkedIn traffic browses and bounces
while r/RAG traffic signs up, **the owned audience is a vanity asset** (DISTRIBUTION.md's own
uncomfortable open question) — and the whole channel strategy is wrong, cheaply, in week one,
before months of content get poured into the wrong pipe. **That is the highest-value thing this
workflow can learn, and it is not on the assumption list.**

---

## 6. Feedback Loop — how results re-rank the assumptions

### Back into `experiments/agents/volt/assumption-mapping.md`

The mapping scores **Importance × Evidence**. W2 changes the **Evidence** column — but only after
the ≥6-week honest-read date, **never from week-one data.**

| Assumption | Evidence now | W2 can move it to | Consequence |
|---|---|---|---|
| **A5** — distribution works | **Weak** — and *the top risk* | **Medium**, in either direction, once a real content effort has run. **W2 is the first genuine evidence A5 has ever had.** | Everything. If traffic can't be moved, **the business dies here, before product quality or pricing get a vote.** |
| **A1** — EU is a driver | **Weak** | **Medium** at best — the A/B needs volume, and an A/B of our own claim is a weaker instrument than an unprompted mention. **Watch for GDPR in inbound replies (§4).** | Decides the permanent headline, whether `/gdpr` is the lead message, and whether the €7.8M SAM-narrow (BUSINESS_MODEL §5a) exists at all. |
| **A2** — buy, not DIY | **Weak** | **Unchanged by W2** — there is no checkout link yet. **Phase 4 owns this.** The abandon micro-survey (§4) is built now so it's live when the link is. | Nothing is proven until a euro moves. |
| **A3** — flow, not backfill | **None** | **Weak→Medium** via the signup one-field question — **a two-month early read on the assumption that decides what business this is.** | Subscription vs. prepaid credit packs. BUSINESS_MODEL §6 stays unlocked until Phase 5. |
| **A4** — output is citable | **Medium** | **Strong or weakened** — but from **Phase 0.2's** category verdicts, not from W2. `upload_repeat` extends it in Phase 3. | Gates whether the gallery is publishable at all. |

**The re-rank to expect:** if W2 lands well, **A5 stays at #1 but stops being a pure unknown**, and
A3 gets its first data point from a form field. If W2 lands badly — a real content effort moves no
traffic — **A5 fires the kill criterion and the other four assumptions never get asked.** That is
the correct order, and it is the order the living docs already put them in.

**Write results to a new memo** (`experiments/agents/out/w2-results-memo.md`) — **do not edit the
W1 memo** (DEVELOPMENT.md §7: a memo's analysis is immutable; only its status changes). Graduate
anything durable into the living docs **in the same commit** (CLAUDE.md rule 4): DISTRIBUTION.md
(measured conversion rates replacing the assumed 2–5% / 5–15%), METRICS.md (real funnel numbers),
BUSINESS_MODEL §8 (risk re-rank).

### What this unblocks in PLAN.md

- **W2 *is* Phase 0.3.** Completing it means the long pole is in the ground and **the clock on
  content compounding has started** — which is the entire reason PLAN.md was reordered.
- **Phase 0.2 is the hard dependency.** No CC corpus ⇒ no gallery ⇒ no artifact post ⇒ **no
  credible landing page.** If Phase 0.2 slips, W2 slips with it. *(Phase 0.1 — the YouTube
  `fileUri` spike — gates 0.2. It is the true critical path, and it is half a session.)*
- **W2 does not gate Phases 1–2.** Engineering runs **in parallel** — that is the governing
  principle. W2 is not a checkpoint to wait at.
- **W2 arms Phase 4.** The event schema, the `ab_arm` assignment and the archive-hours field must
  all exist *before* the first user, or **A3 is lost forever.** This is the one irreversible item
  in the workflow.
- **The kill path stays open and explicit.** If a genuine content effort cannot move traffic
  toward 2,000 qualified visitors — **say so early and out loud, rather than building more product
  to avoid the finding** (DISTRIBUTION.md).

---

### Non-goals of W2 (guardrails)

- **No product code.** The gallery is statically pre-rendered from the `experiments/001/` harness.
- **No cold outreach, no interviews, no concierge test, no design partners.** Outreach is ruled
  out. **Broadcasting is not outreach; a cold email is.**
- **No pricing page.** A3 is unanswered; publishing tiers pre-commits the business model.
- **No YouTube processing sold as the paid product** (BUSINESS_MODEL §10) — Vertex cannot ingest a
  customer's unlisted recording anyway, so offering it would be a promise we cannot keep.
- **No `yt-dlp`, no scraping** — including for the gallery. CC-licensed, attributed, curated
  (CLAUDE.md rule 8).
- **No claim of EU sovereignty.** Residency, stated plainly, CLOUD Act conceded. A DPO will check.
- **No verdict called before the honest-read date.** §5.
