# 006 — Collections strategy: what a collection *is*, and what mdreel manufactures

> **Status: point-in-time record, 2026-07-21. NOT authoritative** (CLAUDE.md — `experiments/**` never
> is). This is the reasoning trail behind a set of decisions taken in a founder strategy session on
> 2026-07-21, plus the launch prompt they produced.
>
> **Each decision below lands in a living doc during the next marathon run** — the "Lands in" column
> says where. **Once it has landed, the living doc wins and this file is history.** If you are
> reading this after that run, read the living docs first and use this file only for *why*.
>
> ## ✅ SUPERSEDED 2026-07-21 — D1–D11 have landed. This file is now history.
>
> The first-collection run landed every decision below into the living docs: **D1, D2, D4** →
> DISTRIBUTION.md · **D3** → ARCHITECTURE §4b v1.1 · **D5** → DISTRIBUTION.md + PLAN.md ·
> **D6** → PLAN.md STATUS · **D7** → BUSINESS_MODEL §7 · **D8–D11** → PLAN.md Phase 5 item 4 ·
> **D12** was a guard only and was respected (nothing renamed).
> **Read the living docs. Use this file only for the reasoning, and above all for the
> "Rejected — do not re-propose" table**, which is the part that lives nowhere else.
> Q2 is answered (see `out/SOURCES.md`); Q1, Q3 and Q4 remain open.
>
> Companion file: [`launch-prompt-first-collection.md`](./launch-prompt-first-collection.md) — the
> coordinator prompt these decisions were folded into.

---

## Context

The 2026-07-20/21 pivot run reframed mdreel as an "AI-ready knowledge repository" and shipped the
§4b repository contract, three planned launch collections, the GitHub operating model and the
consume→convert instrumentation. **Nothing had been manufactured yet** — every deployed service
still ran `PipelineModel__Mode=fake`, and no collection existed. This session asked the question the
pivot run left open: *what is a collection actually a collection **of**, and what is mdreel
manufacturing?*

---

## Decisions

| # | Decision | Why | Lands in |
|---|---|---|---|
| D1 | **The unit of a collection is a TOPIC, not a conference or event.** Conference names are provenance metadata on a session. | Nobody wakes up needing "KubeCon 2026"; they need "how are people building agents". Publishers organise around how users think, not around the printing press. Cross-source synthesis is the thing no single conference channel can offer. | DISTRIBUTION.md (M1) |
| D2 | **Five selection properties for any collection:** licence-feasible · impossible to consume manually · evergreen (a subject, not a schedule) · naturally expandable · cross-source (≥3 distinct events/channels). | The old inclusion criteria were about individual videos; these are about whether the *collection* is worth existing. "If someone could watch it in an afternoon, don't build it." | DISTRIBUTION.md (M1) |
| D3 | **Two publication tiers.** `full` = complete §4b session document (near-complete derivative) → **CC-BY only, no exceptions**. `reference` = index entry with deep links and timestamps, **no derived text** → any publicly available video, zero inference cost. | Strict CC-BY-only would have made most topics uncoverable, killing D2's "impossible to consume manually". The escape hatch was already inside §4b: *indexes cite, never restate*. A citation is not a reproduction. | ARCHITECTURE §4b v1.1 (M1) |
| D4 | **CC-BY only** for `full` — `NC` (we are commercial), `ND` (we publish a derivative) and `SA` (share-alike conflicts with the CC-BY-4.0 we publish under) all disqualify. Standard-YouTube-licence material is `reference` at best. | A YouTube-ToS or copyright fight inside a company selling compliance to DPOs is existential, not a nit (CLAUDE.md rule 8). Conservative default, **explicitly under legal review** — see Q1. | DISTRIBUTION.md (M1) |
| D5 | **mdreel.com is the destination; GitHub is an export target.** | Decisive argument: **GitHub cannot be instrumented** — no Umami, no `collection_convert_click`, no funnel — and distribution (A5) is the top risk. Making the primary surface the unmeasurable one is a straightforward error. GitHub earns its place as *portability proof*: clone it, open it in Cursor/Claude Code, no walled garden. | PLAN.md + DISTRIBUTION.md (M4/M5) |
| D6 | **🏭 Two factories. Factory A (`video → session document`) is the product. Factory B (`many documents → editorial artifact`) is an experiment.** | They differ in every dimension: A is deterministic, contract-frozen, cost-measured, gated; B is editorial, its cost is **founder-hours** (the scarcest input, metered nowhere), it has no quality gate and its legality is open. Strategy conversations drift toward B because it is more exciting. One sentence prevents months of elegant drift. Note the ordering is also enforced by dependency: **B's raw material is A's output.** | PLAN.md STATUS (M1) |
| D7 | **The value claim is "checkable answers", never "correct" ones.** The repository is the artifact; checkable answers are what it gives you. | "Evidence-backed" implies correctness, and correctness is A4 — open, with the weakest measured category on the ICP's own content type. "Checkable" says *don't trust me, here's where I got it*, which is what engineers, lawyers and researchers actually want. | BUSINESS_MODEL §4/§7 — **one sentence, not a rewrite** (M1) |
| D8 | **The artifact post is redefined** from "plain transcript vs mdreel Markdown" to an **answer-shaped artifact**: *"The State of &lt;topic&gt; &lt;year&gt; — based on N hours from M public events"*, every claim timestamp-linked, repository offered as the portable evidence. The format demo becomes a section inside it. | People wake up wanting answers, not repositories. "The State of MCP 2026" beats "MCP Knowledge Pack" and it isn't close. Same asset, better framing — costs nothing, and it was already a planned Phase 5 item. | PLAN.md Phase 5 item 4 (M7) |
| D9 | **Factory B authorship is a three-stage editorial workflow**, not "the model writes it" vs "the founder writes it": model extracts candidate observations → claims are verified → model writes prose from **verified observations only**. Invariant: the prose may not contain a claim absent from the verified list. | The binary framing was wrong. The analyst pattern — model as research assistant, founder as editor, never as typist — collapses the cost without giving up the check. | M7 workflow (run output) |
| D10 | **Split Factory B claims by verification technology, not confidence score.** *Quantitative* (counts, coverage, first-appearance, timeline shifts) verify **programmatically against the §4b corpus** — ~zero cost, full coverage, legally safest (facts, not expression). *Interpretive* (themes, disagreements) cost founder minutes and assert an opinion in mdreel's name. **Lead with quantitative.** | Most of what makes such an artifact compelling — *"14 sessions, up from 3"; "nobody mentioned Y after March"* — is the quantitative class, and it is **only possible because the corpus is structured**. That is a stronger moat than editorial voice: a competitor with a transcript pile cannot produce it. | M7 workflow (run output) |
| D11 | 🚨 **Never verify only the interesting claims.** Verify everything intended for publication **plus a random sample of the discards**, and report the observed error rate. | Surprising and fabricated correlate — a hallucinated trend is more interesting than a real one, because reality is usually boring. Checking only the standouts selects for exactly the claims most likely to be wrong, and never yields a **base error rate** — the number that decides whether Factory B is viable at all. | M7 workflow (run output) |
| D12 | **Naming: `collection` is frozen in code, contract and events.** The *published artifact* may carry a different marketing noun, to be settled on real traffic via the existing A/B instrument. | The manifest (`visibility: "public-collection"`), routes and Umami events (`collection_session_view`, `collection_convert_click`) form a measurement series whose continuity was deliberately preserved during the pivot; a rename destroys it and buys nothing. | — (guard only) |

---

## Rejected, with reasons — do not re-propose

| Idea | Why not |
|---|---|
| **Conference-first collections** ("Microsoft Build 2026") | Superseded by D1. Conferences are provenance, not products. |
| **Company-first / "everything about X" collections** (Cloudflare's whole archive, every Anthropic keynote) | Almost entirely standard-YouTube-licence. Great products, unlicensable ones. Available only as `reference` tier (D3). |
| **"Dossier"** as the artifact noun | In European usage, a dossier is a file kept **on a person**. The buyer is a DPO. Worst possible collision with the positioning. |
| **"Auditable knowledge artifacts"** as the value claim | Abstract vendor register with no reader in it, and "auditable" implies a formal audit trail we do not provide — a near-miss claim standing next to a real compliance claim. D7 stands. |
| **Publishing as the business model** (sponsorship / audience revenue) | Publishing is a **distribution** model here, not a revenue one. The subscription buys access to the factory, never access to the published collections. Public artifacts demonstrate; private repositories monetize — GitHub/Terraform pattern. |
| **"Content compounds" as a plan** | True over years, and every compounding content brand (Stripe, Cloudflare, Fowler) had a salary or a team underneath it. This company has a **calendar kill deadline** instead of a burn clock. Compounding is upside if we survive; it must never justify shipping slower or softening the A5 kill criterion. It is unfalsifiable on the timescale the decision gets made. |
| **Reframing the living docs toward "mdreel is a publisher"** | Three reframings in six days, nothing shipped between them. The identity question is already answered (it publishes to earn trust, like Stripe's docs). The genuinely open question is *cadence and intensity*, and it needs evidence a shipped collection has not yet produced. |

---

## The asymmetry (product philosophy, adopted)

> **The public collection shows the shape, and the shape is only complete on video you own.**

Public artifacts **demonstrate**; private repositories **monetize**. mdreel is a *manufacturing
system for AI-ready knowledge artifacts*: the SaaS is the factory, the customer's own video is the
raw material, the subscription buys **access to the factory**. This is compatible with
BUSINESS_MODEL §7 as written — it sharpens it, it does not replace it.

---

## Open questions

**Q1 — the legal ceiling on everything above (→ the queued Polish-lawyer review).** Which of these
may we publish from a video we do not own and that is not CC-licensed: (1) a generated summary;
(2) extracted structural metadata; (3) transformed knowledge — an answer synthesized across many
talks, citing each; (4) a near-complete timestamped transcript including verbatim on-screen text
(= a §4b `full` document)? Today we publish (4) for CC-BY only and (2) for anything public.
**And separately: may we *process* a non-CC public video for internal analysis, publishing no
derived text from it, only a synthesis that cites it — or does processing itself require a
licence?** That last question decides whether Factory B can ever scale beyond CC-BY material.
Also: does any answer change between public collections and a customer's **private** repository of
their own footage?

**Q2 — the licence funnel, unmeasured.** How many candidate sessions survive to `reference`, and how
many to `full`? If the ratio is severe, the public-collection strategy is **corpus-bound rather than
budget-bound**, which is a different company. The next run measures it.

**Q3 — Factory B's base error rate.** Unknown until D11 is executed once.

**Q4 — publishing cadence and intensity.** Deferred to a founder session *after* a real collection
has shipped and drawn traffic.

---

## The filter that produced these decisions

Every attractive idea in this session was put through four questions:

1. **Is it legally possible?**
2. **Can we ship it before the current milestone?**
3. **Can we measure whether it worked?**
4. **Does it increase trust, or merely sound impressive?**

Known blind spot, accepted: the filter **rewards the legible and rejects the unmeasurable-at-n=1**,
which hits A5 — distribution — hardest, since the highest-value moves there (writing that lands, a
community that adopts you) fail question 3 by construction at the start. The amendment: a move that
fails Q3 may still be **taken** if it is cheap and reversible; it may not be **scaled** on unmeasured
belief. Measurement gates commitment, not attempts.
