# vectorreel — Workflow 2 Validation Kit

> **Date:** 2026-07-14 · **Question:** Will anyone actually buy this?
> **Inputs:** `experiments/workflow1-decision-memo.md` (W1) · `BUSINESS_MODEL.md` · `PLAN.md`
> **Agents:** volt/product-manager → wshobson/content-marketer → wshobson/sales-automator → volt/ux-researcher
> **Scope:** demand validation only. **No product code.** Landing page + 20 outreach sends + 5–8 interviews.
> Point-in-time artifact — not a living doc.

---

## 0. Context Recap

**Where the W1 memo actually located the risk.** Every High-importance / Weak-evidence
assumption is **Value** or **Business viability**. Feasibility — the axis a technical founder
de-risks first — is the one quadrant already closed (A8: COGS measured at €0.65/video-hour
all-in, 7× under the cheapest price point). The verdict was 🟡 **RESHAPE**, not BUILD:
*"stop building the SaaS and start selling the pipeline by hand."*

- **The riskiest DEMAND assumption W1 says to test first is A2 — "buyers will buy rather than
  DIY."** Value risk, **weak** evidence. Gemini ingests native video; the ICP is dev teams
  building AI assistants — the audience best equipped to build this in a weekend. We are
  selling a wrapper to wrapper-builders. W1's primary instrument for A2 is the **concierge
  test ending in a request for money** (*"€200 to process your next 20 hours"*). Everything in
  this kit is built to get to that ask.
- **Riding alongside: A1 — "EU residency is a purchase driver, not a checkbox."** Also Value /
  weak. It is the entire differentiator and the most attackable claim we own (BUSINESS_MODEL §4
  already concedes GCP is US-controlled under the CLOUD Act). W1 tests it with a headline A/B.
  **See §6 for a correction: at n=20 the email A/B cannot carry this. The A1 signal in W2 comes
  from whether GDPR surfaces *unprompted* in interviews.**
- **ICP is BUSINESS_MODEL §5 #1, verbatim:** software companies / IT departments, 50–500
  employees, building internal AI assistants — Teams/Zoom recordings, demo libraries, an "AI
  adoption" mandate, developer buyer, uses the API. Beachhead **Poland + Denmark/Nordics/DACH**
  (founder network via Conscensia/Unik). Secondary: AI consultancies/software houses (§5 #3) —
  easiest to reach, and the harshest DIY audience, which is a feature for an A2 test.
- **Pricing frame is fixed by measurement, not by guesswork.** All-in COGS **€0.011/video-minute
  (€0.65/video-hour)**. The ask in this kit is W1's: **€200 for the next 20 hours** = €10/hour =
  **€0.167/video-min → ~93% gross margin**. That is *above* API PAYG list (€0.15/min) and well
  above Pro-at-full-usage (€0.099/min), so it does not undercut BUSINESS_MODEL §6 and does not
  pre-commit us to a subscription. Deliberate: **BUSINESS_MODEL §6 is explicitly gated on A3**
  (backfill vs. flow) and must not be locked until PLAN.md Phase 5. A prepaid pack is the one
  ask that is valid under *either* answer.
- **Where W2 sits in PLAN.md:** it is the demand half of **Phase 0.3** ("Demand instrument:
  publish — 🚨 THE LONG POLE"), running in parallel with Phases 0.1/0.2. It consumes the Phase
  0.2 public CC corpus as its demo asset and it **gates Phase 3** (thin trial slice). W1's rule
  stands: **do not write product code until ≥2 of 5 prepay.**

### ⚠️ Conflict between the source docs — and how this kit resolves it

W1 prescribes hand-selling. PLAN.md §"Governing principle" and BUSINESS_MODEL §9 state the
founder has **ruled out design-partner outreach** — the motion is self-serve/inbound — and
re-rank **A5 (distribution)** to top risk. These cannot both be followed literally.

**Resolution used throughout this kit:** the A5 arithmetic (Pro contributes ~€131/mo; CAC above
~€800 breaks payback) kills outbound as a **scaled acquisition channel**. It says nothing about
outbound as a **research instrument**. Twenty emails that buy five conversations cost €0 and
are the only way to hold A1/A2/A3 evidence inside a month; the inbound funnel that PLAN.md is
built on takes months to compound and cannot answer them sooner.

> **W2 is research, not sales. It does not reopen outbound as a GTM channel, and nothing here
> should be read as a commitment to one.** PLAN.md Phase 0.3's landing page + LinkedIn/HN
> broadcasting proceeds unchanged and in parallel. If W2 produces paying users, that is a
> bonus, not the mechanism.

---

## 1. Positioning & ICP  *(agent: volt/product-manager)*

### Positioning statement

> **For EU engineering teams building an internal AI assistant, vectorreel turns the recordings
> your assistant can't see — demos, trainings, incident reviews — into timestamped Markdown it
> can cite, without the footage ever leaving the EU.**

Anchored on *agents / knowledge bases*, never on "transcription" (BUSINESS_MODEL §8, the
buyer-confusion risk). The EU clause is deliberately placed **second, as a qualifier** — that
is the honest reflection of A1's evidence level. If interviews show GDPR surfacing unprompted,
it earns promotion to the front of the sentence; if not, it gets cut, per the A1 decision rule.

**What we are NOT saying:** not "EU-sovereign" (Google is US-controlled — BUSINESS_MODEL §4
honesty rule), not "better than Cloudglue", not "transcription".

### ICP — one segment, three sourcing lanes

| | |
|---|---|
| **Who** | Software company or in-house IT/platform team, **50–500 employees**, EU-domiciled. Buyer/champion is a **hands-on engineer or eng lead** — Head of Platform / AI Lead / Principal Engineer / CTO at the smaller end — who has personally been told to "make the company's knowledge available to an AI assistant". |
| **Trigger** | An active RAG / internal-assistant / MCP project shipped or in flight **in the last 6 months**. Without this, they are not in market and the interview is worthless. **This is the single hard screen.** |
| **Specific pain** | Their knowledge base has the wiki, Confluence, Slack and the repo in it. It does **not** have the 40-minute architecture walkthrough, the customer demo, the incident post-mortem call, or the onboarding series. So the assistant confidently answers from a stale wiki page while the correct answer is a thing someone *said and showed* in a recording six months ago. **The sharp edge: what was on the screen — the config, the diagram, the UI path — is exactly what nobody wrote down, and exactly what a transcript throws away.** |
| **Current workaround (what to listen for)** | In rough order of what we expect to hear: (1) **nothing — video is silently out of scope** (most likely; the assistant is simply blind to it and everyone has quietly accepted that); (2) somebody dumped Whisper/Teams transcripts into the KB and the answers got *worse*, so they pulled them back out; (3) an engineer spent a weekend piping video into Gemini, it worked on one file, and it is now unmaintained; (4) a human is manually writing summaries of important recordings. **(1) and (2) are our buyers. (3) is the A2 threat, in person.** |
| **Where to find ~20** | **Lane A — founder network (target 8–10, highest reply rate; PL + DK via Conscensia/Unik).** Warm-ish intros beat cold every time and cost nothing. **Lane B — public evidence of the trigger (target 6–8).** People who have *published* about building internal RAG: LinkedIn posts in the .NET/architecture circle the founder already has reach into; speakers at PL/DACH/Nordic AI-engineering meetups; authors of "we built an internal assistant" write-ups. Their post *is* the personalization. **Lane C — AI consultancies / software houses (target 3–4; §5 #3).** Public "we build RAG for clients" positioning; reachable via LinkedIn; they hold the DIY answer most brutally. |
| **Disqualify immediately** | No AI/RAG project in flight · <50 or >500 staff · non-EU-domiciled · the recordings are *public marketing* video (that is the free YouTube tool, never the paid product — BUSINESS_MODEL §10). |

**Why deliberately target the segment most able to DIY:** because A2 is the assumption under
test. Validating "will they buy rather than build" against an audience that *cannot* build is
worthless evidence. If ICP #1 pays, the business is real. If ICP #1 says *"we'd just pipe it to
Gemini ourselves"*, that is W1's stated kill signal and we would rather hear it in week one
than after Phase 3.

---

## 2. Landing Page Copy  *(agent: wshobson/content-marketer)*

> One page. One CTA. No pricing table (A3 is unanswered — publishing tiers now would pre-commit
> the business model). Ships to the Cloud Run page already deployed per PLAN.md Phase 0.3.

### Hero — the A1 A/B

Two arms, identical below the fold. **Arm B is the default** unless/until A1 earns the promotion.

**Arm A — EU-lead** *(tests A1)*
> # Your recordings never leave the EU.
> Turn internal video — demos, trainings, incident reviews — into timestamped Markdown your AI
> assistant can cite. Processed entirely in EU regions. Source video deleted after processing.

**Arm B — capability-lead** *(the control)*
> # Your AI assistant can't see what's on screen in your videos.
> It reads your wiki, your repo, your tickets. It has never seen the 40-minute demo where
> someone actually showed how the thing works. vectorreel turns that recording into timestamped
> Markdown your assistant can cite — and it never leaves the EU.

### Three value props — each tied to a W1 assumption

**1. A transcript throws away the answer.** *(→ A4 / the core capability claim)*
The config value, the architecture diagram, the exact UI path, the code on the slide — none of
it is spoken aloud, so none of it survives transcription. We capture **what was shown**
separately from **what was said**, so your retrieval layer can weight them differently. The
demo below answers a question that is *only* answerable from a screen.

**2. Every claim carries a timestamp, so your assistant can cite it.** *(→ A4)*
Every section is anchored `[00:14:32]`. When the assistant answers, it links to the second the
answer came from — a human can verify it in one click. **A hallucinated timestamp poisons a
knowledge base, and trust doesn't come back**, so the output is designed to be checkable, not
merely fluent.

**3. Plain Markdown files. No lock-in, and nothing to rip out.** *(→ A2 — the honest version)*
You get files. Put them in your repo, your GCS bucket, Obsidian, SharePoint, your vector DB.
There is no retrieval stack of ours to adopt and no proprietary format to escape. We do the
part nobody enjoys — chunking long video, holding cost down, keeping the schema identical
across 500 files, retrying the runs that fall over — and then we get out of the way.

### How it works

1. **Video in.** Upload a file, or POST it to the API. Meetings, demos, trainings, talks.
2. **We read it twice — audio *and* screen.** Speech is transcribed and cleaned; frames are read
   for on-screen text, slides, code and UI state; the two are fused into one document.
3. **Markdown out.** YAML frontmatter, topic sections, `[hh:mm:ss]` anchors, and a clean
   separation of *spoken* vs *shown on screen*. Identical schema on every file.
4. **The source video is deleted.** Default, not a setting you have to find.

*(→ link: side-by-side "plain transcript vs. vectorreel Markdown" + a RAG answering a
screen-only question. Built on a **public CC-licensed video** from PLAN.md Phase 0.2, so it can
actually be shared. This artifact is the single highest-value inbound asset we own.)*

### EU & GDPR — the honest version

> **What is true today.** Processing runs in **EU regions only** (GCP `europe-central2` /
> `europe-west3`), under Google's DPA with Vertex AI **no-training terms**. Your source video is
> **deleted after processing** by default. We never train on customer data — contractually,
> ever. The subprocessor list is short and published.
>
> **What is not true today, and we will not pretend otherwise.** This is **EU data residency,
> not EU sovereignty.** Google is a US company and therefore in scope of the CLOUD Act. Any
> vendor telling you "EU region" means your data is beyond US legal reach is telling you
> something your DPO will take apart in ten minutes.
>
> **Where we're going.** An **EU-owned infrastructure option** (OVHcloud / Scaleway) is on the
> roadmap, and a self-hosted edition after it. If full sovereignty is a hard requirement for you
> today, say so on the call — it moves up the roadmap, and we would rather lose the deal than
> win it on a claim that doesn't hold.

*(Rationale — BUSINESS_MODEL §4 honesty rule: overclaiming gets destroyed by any competent DPO,
and this page's actual reader is that DPO. Honesty here is the sales asset.)*

### FAQ

**Is this just transcription?**
No — and if transcription were enough, you'd already be done: Whisper is free. The value is in
what the transcript *cannot* contain: the slide, the config, the diagram, the UI path. We
produce one document that keeps *shown* and *said* apart and timestamps both.

**Why wouldn't I just send the video to Gemini myself?**
For one video, you should — genuinely, it works, and we'd rather you find that out than be sold
something. It stops working around video number fifty: chunking an hour-long recording within
context limits, keeping the schema stable across every file so retrieval doesn't degrade,
capping cost per hour, and retrying the ~8% of model calls that run away. That plumbing is the
product. **We publish our real cost per video-hour so you can check the maths yourself.**

**What happens to our video?**
Deleted after processing, by default. We keep the Markdown you asked for and the job's cost
ledger. We do not build a video library — there is nothing here to breach.

**Is it accurate enough to put in a knowledge base?**
That is the right question, and *"wrong is worse than absent"* is the standard we hold
ourselves to. Every claim is timestamped so any answer is verifiable in one click. We publish
benchmarks per content type — screen-recording, slide-talk, talking-head — including where the
output is weakest.

**What does it cost?**
We're setting pricing with our first users rather than guessing at it, so there's no pricing
page yet. What we can tell you: our measured all-in cost is **€0.65 per video-hour**, and we
publish it. If you want to move now, the first working arrangement is **€200 for your next 20
hours of video** — flat, prepaid, no subscription.

### Primary CTA

> ### Send us one recording. Get the Markdown back. Free.
> One real internal video — the one your assistant should have seen and hasn't. We'll process it
> and send back the Markdown within 48 hours. Drop it in your own RAG and ask it five questions
> you actually care about. **No signup, no card, no product to install — just reply to a human.**
>
> **[ Book 20 minutes → ]**

*(Single CTA. The email-capture field is the fallback for the not-yet-ready — PLAN.md Phase 0.3:
"Email capture. Nothing more.")*

---

## 3. Cold Outreach Sequence  *(agent: wshobson/sales-automator)*

**Target:** 20 sends (Lane A 8–10 · Lane B 6–8 · Lane C 3–4).
**Ask:** 20-minute call. **Not** a demo, **not** a trial — a call. The concierge delivery is
what happens *after* the call, and the €200 ask happens after that.
**Sequence:** Day 0 → Day 4 → Day 10. Then stop. Three touches, no more.

**Personalization variables**
`{first_name}` · `{company}` · `{trigger}` — the specific public evidence they are in market
(their LinkedIn post, their meetup talk, the assistant they shipped). **No `{trigger}`, no
send.** · `{recording_type}` — the recording *their* company obviously has (sprint demos,
customer calls, onboarding series) · `{referrer}` — Lane A only.

### A1 A/B split

10 sends use the **EU-lead** subject; 10 use the **capability-lead** subject. Stratify across
lanes so one arm doesn't get all the warm intros — that would confound the result completely.
**Log it, but read §6 before believing it: n=20 cannot settle A1.**

| Arm | Subject A/B pair |
|---|---|
| **EU-lead** | "your recordings, processed without leaving the EU" · "{company}'s recordings + a US processor?" |
| **Capability-lead** | "the part of {company}'s demos your assistant can't see" · "your AI assistant hasn't watched the demo" |

*(Lowercase, lightly informal. Nothing that reads like a mail-merge.)*

---

### Email 1 — Day 0 · 98 words

**Subject:** *(per arm, above)*

> Hi {first_name} — {trigger}, which is why I'm writing.
>
> A question I keep hearing when teams do this: the assistant reads the wiki and the repo, but
> it's never seen {recording_type}. And the useful part of those — the config on screen, the
> diagram, the actual click path — was never spoken aloud, so a transcript loses it anyway.
>
> I turn recordings into timestamped Markdown that keeps *shown* and *said* separate, so a RAG
> can cite it. Processed in the EU, source deleted after.
>
> Worth 20 minutes to hear how you handled video? I'm not selling — I want to know if this is
> even a real problem.

*(Lane A opener swaps to: "{referrer} suggested I talk to you — {trigger}.")*

---

### Email 2 — Day 4 · 92 words

**Subject:** Re: *(same thread)*

> Hi {first_name} — following up with something concrete instead of another ask.
>
> Here's a public conference talk run through the pipeline: [link]. Plain transcript on the
> left, the Markdown on the right. At the bottom, a RAG answers a question that's **only**
> answerable from a slide — the transcript version can't touch it.
>
> That's the whole pitch, and it takes 30 seconds to judge.
>
> If it's interesting: send me one real internal recording and I'll process it and send the
> Markdown back, free. If it isn't — reply "not a problem we have" and I'll leave you alone.

*(This email is the workhorse. It is the Phase 0.2 artifact doing the selling, and "reply and
I'll leave you alone" reliably harvests the honest no — which for a research instrument is a
**successful outcome**, not a failure.)*

---

### Email 3 — Day 10 · 74 words

**Subject:** last one — closing the loop on video + RAG

> {first_name} — closing this out, no reply needed.
>
> I'm trying to settle one question this month: whether "our AI assistant can't see our
> recordings" is a real problem people would pay to fix, or just a nice-to-have everyone lives
> with. Genuinely useful answers so far have included "we tried transcripts and pulled them back
> out" and "honestly, nobody's asked".
>
> If either is true at {company}, I'd still take the 20 minutes. If not, thanks for reading.
>
> — {name}

*(The break-up asks for the *answer*, not the meeting. At n=20 an honest "nobody's asked" is
worth more than a polite call.)*

---

### Objection handling

| They say | You say |
|---|---|
| **"We'd just use Gemini directly."** | **Do not fight this — it is the A2 data point, and this is the single most important sentence in the whole workflow. Log it verbatim, then get curious:** "Totally fair — has anyone actually done it yet? What happened?" *If someone tried and it's rotting unmaintained, that's a buyer. If they shipped it and it works, that's the DIY gap closing, and we need to know.* |
| **"Send me pricing."** | "There isn't a pricing page — I'm setting it with the first few users. Measured cost is €0.65/video-hour, and I publish that. If you want to move now: €200 for your next 20 hours, flat, no subscription." |
| **"GDPR/DPO would need to review."** | **A1 gold — dig, don't reassure.** "What did your DPO block last time? What made a vendor easy to approve?" *Then the honest line:* EU regions, no training, source deleted — but Google is US-owned and I won't pretend that's sovereignty. |
| **"We don't have that much video."** | "How much is 'not much'? And is it a back catalogue, or does it keep arriving?" **← This is A3, and it is the most valuable answer in the email thread. Ask it of everyone, including the ones who say no.** |
| **Silence.** | Three touches, then stop. A non-reply is data (weak trigger, or a non-problem); it is not an invitation to a fourth email. |

### Tracking

Per send, one row: `lane · arm · trigger · sent · opened · replied · reply sentiment
(interested / honest-no / DIY / silent) · call booked · recording received · €200 asked · paid`.
A spreadsheet. Do not build a CRM.

**The metric that matters is not reply rate — it is `calls booked` and, downstream, `paid`.**
A 40% reply rate that produces zero recordings has taught us nothing.

---

## 4. Interview Guide — 20 minutes  *(agent: volt/ux-researcher)*

**Objective:** decide A2 (buy vs. DIY), collect A3 (backfill vs. flow), and detect A1 *without
ever naming GDPR*.

**Rules of engagement — the whole validity of this rests on these:**

1. **Never describe the product before question 6.** Every question up to that point is about
   **past behaviour**, never about a hypothetical future. "Would you use a tool that…" produces
   polite, worthless yeses.
2. **Never say "GDPR", "EU", "residency" or "compliance" first.** If we introduce it, we have
   destroyed the A1 test — the buyer will agree it matters, because who wouldn't. **A1 is only
   validated if the buyer raises it unprompted.**
3. **Chase the specific, past, concrete instance.** "The last time", not "generally".
4. **Shut up.** Silence after a question is the instrument. The interviewer talks < 20% of the
   time.

### Questions

**Warm-up (2 min)** — "Tell me about the AI assistant you've built — what's in it, who uses it,
what's it actually good at?" *(Also re-confirms the trigger. If there is no assistant, end the
interview politely; they are not the ICP.)*

**Q1 — The pain, if it exists (4 min)** *→ A2 precondition*
> "Walk me through the last time someone at {company} needed information that only existed in a
> recording. What did they actually do?"

*Listen for:* did they find it, or give up? How long? Did they ask a human instead?
🔴 **Kill signal:** *"they just asked a colleague"* — and nobody minds. Then the pain is social,
not informational, and we are selling a vitamin.

**Q2 — Is video even missed? (3 min)** *→ A2*
> "What's in the assistant's knowledge base today, and who did the work to get each source in?"
> Then: **"Was video ever considered? What happened to that idea?"**

*The gold is in the second half.* "Never came up" and "we tried and it made things worse" are
completely different businesses. The first is a missing-category problem — we must create the
demand. The second is a failed-attempt problem — the demand exists and is unmet, which is much
better news.

**Q3 — The A1 trap (3 min)** *→ A1. Do not lead.*
> "Has anything ever been blocked from, or pulled out of, that knowledge base — and why?"

*Sit in the silence.* If GDPR / legal / "we couldn't send that to a US service" / the DPO
surfaces here **unprompted**, A1 is live. If they talk only about accuracy, noise or effort —
**EU is hygiene, not a wedge**, and the positioning must move to the capability story.

**Q4 — Procurement reality (3 min)** *→ A1, confirmation*
> "Last time you onboarded an AI vendor that touched your data — what did procurement or your
> DPO actually ask you? What made a vendor easy or hard to approve?"

*Listen for:* was "US processor" ever an actual **blocker**, or just a form field? "We asked and
moved on" = checkbox. "We rejected a vendor over it" = wedge. **This distinction is the entire
A1 verdict.**

**Q5 — The volume question (3 min)** *→ A3. The most valuable 3 minutes in the kit.*
> "If someone had to make your last 20 recordings searchable next week — who'd do it, how long,
> and out of whose budget?" Then, explicitly:
> **"And how many hours of recording sit in the archive versus how many hours get recorded in a
> normal month?"**

*Write both numbers down.* Archive hours : monthly hours **is A3**, and A3 decides whether this
is a subscription business or a prepaid-credit business. 200:5 is a backfill business.
40:20 is a flow business. **Ask this of everyone — including the people who tell you no.**

**Q6 — The DIY question (3 min)** *→ **A2, the core.** First time the product may be named.*
> "If I told you this existed — video in, timestamped Markdown out, EU-processed, files you own —
> what's your honest first reaction?"
> Then, **and this is the question that matters:** **"Would you build it or buy it? Walk me
> through how you'd decide."**

*Do not defend.* Let them talk through the build. Listen for whether they estimate the *weekend*
version (naive: one file, one prompt) or the *real* one (chunking, schema stability, cost caps,
the ~8% of runs that fall over, 500 files staying consistent). **If they estimate the real one
and still say build — A2 is in serious trouble, and we should be grateful to know.**

**Q7 — The ask (2 min)** *→ A2, the only answer that counts*
> "Send me one real recording. I'll process it and send back the Markdown in 48 hours — free,
> no strings. Drop it in your RAG, ask it five questions you care about. If it's useful:
> **€200 for your next 20 hours**, flat, no subscription. Fair?"

*Then say nothing.* **Enthusiasm is not the signal. The card is the signal.** Note precisely
where they flinch — at sending a recording (trust), or at the €200 (value)? Those are two
entirely different diagnoses and they need two different fixes.

**Q8 — The referral (1 min)**
> "Who else is wrestling with this? I'll take the intro."

*A weak referral answer after enthusiastic noises is itself a signal — nobody forwards a vitamin.*

---

## 5. Validation Criteria

**Sample:** 20 sends → target 5–8 interviews → target 5 concierge deliveries → 5 €200 asks.
**Cost:** ~€0.40 Vertex spend per delivered video ≈ **€2–5 total.** Time: one week of sends +
two weeks of calls.

### The three signals that decide it

| # | Signal | ✅ Validated | ❌ Kill / Reshape |
|---|---|---|---|
| **1** | **A2 — money (the primary)** | **≥ 2 of 5 concierge recipients prepay €200.** | Enthusiasm, zero payment → **vitamin, not painkiller.** Or, worse, *"we'd just do it ourselves"* recurring at Q6 → **the DIY gap is too small. Stop.** (W1 kill criteria, verbatim.) |
| **2** | **A1 — is EU a wedge?** | **GDPR / DPO / "not a US processor" surfaces UNPROMPTED at Q3 or Q4 in ≥ 4 of 8 interviews**, *and* at least one names a vendor actually rejected over it. | Surfaces only when we raise it, or only as a form field → **EU is a checkbox.** Not a kill — a **reposition**: capability-lead becomes the permanent headline, Arm A is retired, and the EU story demotes to a trust section. |
| **3** | **A3 — flow or backfill?** | **Median archive:monthly-flow ratio < 10:1** across ≥ 6 answers → recurring usage is plausible → BUSINESS_MODEL §6 subscription tiers survive. | **Ratio > 20:1** → **backfill business.** Not a kill — a **repricing**: BUSINESS_MODEL §6 becomes a **prepaid credit pack** ("€200 for 20 hours"), the €149/mo tier is deleted, and PLAN.md Phase 4's Stripe link changes shape. |

### The honest reading of a mixed result

- **Money but no GDPR** (very plausible): the business is real, the *positioning* is wrong. Ship
  it, drop the EU headline. This is a **good** outcome wearing a disappointing hat.
- **GDPR but no money:** the most dangerous result, because it *feels* like validation. A DPO's
  enthusiasm is not a purchase. Treat as A2-invalidated.
- **Neither:** W1's kill criteria are met. Stop, and do not let the sunk benchmark cost argue
  otherwise — the €65M TAM does not justify pushing uphill against a DIY alternative our own
  customers are qualified to build.
- **Zero replies to 20 sends:** **this is not an A1/A2 result, it is an A5 result** — and A5 is
  BUSINESS_MODEL's top risk. Do not conclude "no demand" from an empty inbox; conclude "we
  cannot reach these people," which is a different and equally serious finding.

### ⚠️ Statistical honesty — read before trusting the email A/B

**n=20 cannot settle A1.** At a realistic 15–30% reply rate, each arm has 10 sends and yields
1–3 replies. The difference between arms will be **pure noise**, and Lane A's warm intros will
swamp the subject-line effect entirely. W1 itself specified **~100 sends per arm** and called
even that *"directional, not significant."*

**Therefore, in W2:**
- The email A/B is **logged, not read.** It is a seed for the Phase 0.3 landing-page A/B, which
  is where A1 actually gets tested at volume.
- **The A1 signal in W2 is Q3 + Q4 — unprompted mention.** That is a qualitative instrument and
  it is legitimate at n=8, where a subject-line A/B is not.
- Anyone who reports "Arm A got more replies, EU is validated" off 20 sends should be ignored,
  including us.

---

## 6. Feedback Loop — how the results re-rank the assumptions

### Back into `experiments/agents/volt/assumption-mapping.md` (W1)

The W1 assumption table is scored **Importance × Evidence**. W2 changes only the **Evidence**
column — and that is enough to move assumptions between quadrants and re-order everything
downstream. Re-run the mapping agent with the interview notes as input and apply:

| Assumption | Evidence today | After W2, becomes | Consequence |
|---|---|---|---|
| **A2** — buy, not DIY | **Weak** | **Strong** if ≥2/5 prepay → *Monitor* quadrant.<br>**Dead** if the modal Q6 answer is "we'd build it." | The single gate on PLAN.md Phase 3. Nothing is built until this moves. |
| **A1** — EU is a driver | **Weak** | **Medium** if unprompted in ≥4/8 → keep testing at volume in Phase 0.3.<br>**Refuted** if never unprompted → **drop from the assumption map**, rewrite BUSINESS_MODEL §4's buyer message. | Decides the permanent headline and whether the `/gdpr` page is sales collateral or hygiene. |
| **A3** — flow, not backfill | **None** | **Medium** — the first evidence this assumption has *ever* had. Cannot reach Strong without cohort data (Phase 4). | Decides **subscription vs. prepaid pack**, i.e. what business we are in. |
| **A5** — distribution works | **Weak** | **Unchanged.** 20 hand-sent emails say nothing about an inbound funnel. | Still the top risk. Phase 0.3 owns it. **W2 must not be mistaken for progress on A5.** |
| **A4** — output is citable | Medium | **Strong** if concierge recipients get useful RAG answers on *their own* footage; **weakened** if they find a wrong timestamp. | A real-footage test that Phase 0.2's CC corpus cannot provide. |

**Then re-rank.** W1's top-3 were A1, A2, A3 — all Value/Business, all weak. If W2 lands as
hoped, **A2 closes and A5 is left alone at the top**, which converts vectorreel from a
"does anyone want this?" problem into a "can we reach them?" problem — a strictly better place
to be, and the one PLAN.md is already organised around.

**Write the output to a new point-in-time memo** (`experiments/agents/out/w2-results-memo.md`) —
do **not** edit the W1 memo, which is explicitly a point-in-time artifact. Then update the
living docs in the same commit (CLAUDE.md rule 4): **BUSINESS_MODEL §5** (ICP, sharpened by real
answers), **§6** (pricing — only if A3 is decisive), **§8** (risk re-rank).

### What this unblocks in PLAN.md

- **✅ ≥2 of 5 prepay → PLAN.md Phase 3 (thin trial slice) is unblocked, and only then.** W1's
  rule holds: *"Do not write product code until ≥2 of 5 prepay."* The prepayers are Phase 3's
  first users, and their footage is the golden-test corpus. Phases 0.1/0.2 (YouTube spike, CC
  corpus) run in parallel regardless — they are cheap, they feed the Phase 0.3 artifact, and
  they are not gated on demand.
- **↩️ A1 refuted → PLAN.md Phase 0.3's headline A/B collapses to one arm.** Arm B (capability)
  ships as the permanent headline; the A/B is retired before it runs, saving weeks of waiting on
  a question already answered qualitatively.
- **↩️ A3 says backfill → PLAN.md Phase 4's "ONE Stripe payment link" becomes a prepaid credit
  pack, not a subscription** — and BUSINESS_MODEL §6's tier table is deleted rather than
  refined. Phase 5 ("read the data → choose the pricing model") gets its input a month early.
- **❌ Nobody pays + no unprompted GDPR + "we'd build it ourselves" → stop.** This is W1's kill
  criteria, met. The correct action is to write the negative result up honestly and keep the
  benchmark harness — it is a genuinely useful €1.91 artifact, and it will still be true.

---

### Non-goals of W2 (guardrails)

- **No product code.** None. Not "just the upload endpoint."
- **No pricing page.** A3 is unanswered; publishing tiers now pre-commits the business model.
- **No YouTube processing offered as the paid thing** (BUSINESS_MODEL §10). The public tool is
  distribution; the paid product is private recordings. Vertex cannot ingest a customer's
  unlisted recording anyway, so promising it in an email would be a lie we could not deliver.
- **No `yt-dlp`, no scraping** — including for the Phase 0.2 demo assets (CLAUDE.md rule 8).
- **No claim of "EU sovereignty"** anywhere in this kit's copy. Residency, stated plainly, with
  the CLOUD Act conceded. A DPO will check.
