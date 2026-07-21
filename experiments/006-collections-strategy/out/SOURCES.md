# M1a — Collection #1 source list and licence audit trail

> **Status: point-in-time record, 2026-07-21. NOT authoritative** (CLAUDE.md — `experiments/**`
> never is). This is the measured result of one licence-audit run against the YouTube Data API v3.
> Licence facts were true on the date of the API call recorded in
> [`corpus.json`](./corpus.json) (`licence_verified_at`) and can change at any time — a publisher
> can flip a video off CC-BY tomorrow. **Re-verify before manufacturing, and again before
> publishing.** If this file contradicts a living doc, the living doc wins.
>
> Companion files: [`corpus.json`](./corpus.json) (the audit trail),
> [`../curation.json`](../curation.json) (the human selection layer),
> [`../scripts/yt_licence_audit.py`](../scripts/yt_licence_audit.py) (re-runnable),
> [`candidates.json`](./candidates.json) + [`raw_search.json`](./raw_search.json) (raw evidence).
> Decisions this run executes: DECISIONS.md **D1–D4**. Question it answers: **Q2**.

---

## 1. The topic scope for collection #1

**AI engineering: getting language-model systems into production and keeping them there.** The
scope is the engineering practice, not the models — retrieval-augmented generation and its failure
modes, agent architectures and their operational reality, evaluation as a discipline (offline
harnesses, LLM-as-judge, regression suites), serving and inference economics, vector and hybrid
search as infrastructure, and the observability/LLM-ops layer that tells you when any of it has
quietly stopped working. It deliberately excludes model research, prompt-tip content, and
"what is an AI agent" explainers. The reader it is built for is the engineer who already shipped
something and is now discovering that shipping was the easy part.

**Against the five selection properties (D2):**

| Property | Evidence |
|---|---|
| **Licence-feasible** | 25 sessions independently verified `status.license == "creativeCommon"` via `videos.list`, all public and embeddable. Feasible — but only just, and only because the search was licence-first (see §2). |
| **Impossible to consume manually** | 15.6 hours at `full` tier + 22.6 hours at `reference` = **38 hours across 60 sessions and 31 channels**. Nobody watches that in an afternoon, and nobody watches it *cross-referenced*, which is the actual product. |
| **Evergreen** | The unit is a subject — "how do you evaluate a RAG system" — not a schedule. The accepted `full` corpus already spans 2023→2026 (3/6/11/5 sessions per year); the same questions recur with different answers, which is exactly what makes a timeline worth having. |
| **Naturally expandable** | Every conference in the corpus runs again next year; the topic screen in the script is a keyword set, not a hand list. Adjacent topics (MCP/tooling, agent security, GPU scheduling) already appear in the discarded CC-BY pool and can seed collections #2 and #3. |
| **Cross-source** | `full` tier alone spans **12 distinct channels / 13 distinct events**; no single channel contributes more than 4 of 25. Adding `reference` takes it to 31 channels. Requirement was ≥3. |

---

## 2. 🔢 The licence funnel, measured (answers **Q2**)

### The headline number

> **3.6%.** Of 276 relevance-ranked YouTube results for AI-engineering *conference-talk* queries,
> issued with **no licence filter at all**, exactly **10 were CC-BY**. Six of those ten came from
> one channel (CNCF).

That is the unbiased base rate of the topic's public supply, and it is the number that matters.
It is recorded in `corpus.json` under `licence_base_rate`.

⚠️ **Do not quote `funnel.licence_passed` (936/1202 = 78%) as a base rate.** It is inflated *by
construction*: 27 of the 33 `search.list` calls carried `videoLicense=creativeCommon`, so of
course most results came back CC. That field measures how well the licence-first search strategy
worked, not what the world looks like.

### The full funnel

| Stage | N | Notes |
|---|---:|---|
| Candidates considered (resolved via `videos.list`) | **1,202** | union of 33 searches, deduplicated |
| Licence-passed (`status.license == creativeCommon`) | 936 | **biased — see warning above** |
| Session-shaped (public + embeddable + 8–120 min) | 822 | of which CC-BY: 651 |
| Topic-passed (session-shaped + AI-engineering title screen) | 566 | of which CC-BY: **412** |
| **Accepted — `full` tier** | **25** | 15.62 h, 12 channels, 13 events |
| **Accepted — `reference` tier** | **35** | 22.6 h, 19 channels, metadata only |
| Rejected (recorded with reason in `corpus.json`) | 1,143 | |

**`full : reference` = 25 : 35 = 0.71.**

### What the ratio means — read this part

The published ratio (0.71) looks reassuring and **is not the interesting number**, because both
sides were hand-capped: 25 was the brief's target and 35 was a curatorial judgement about how much
index makes the collection read as covering its subject. Either side could have been larger.

The load-bearing finding is the pair of numbers underneath:

- **412 CC-BY topic-passing candidates existed** — reachable in under 4,000 quota units. For *this*
  topic, at *this* size, the strategy is **not corpus-bound**. There is roughly 15–20× more
  eligible CC-BY material than collection #1 consumes, which is several collections of runway.
- **But they are 3.6% of the supply, and they are not the talks people cite.** Filtered to CC-BY,
  the corpus is overwhelmingly *infrastructure* content — Kubernetes, serving, GPUs, vector
  databases — because the CC-BY-rich publishers are foundations (CNCF, Linux Foundation) whose
  subject is infrastructure. The canonical **application-layer** talks — Chip Huyen on LLM
  applications, Jerry Liu on RAG pain points, Barry Zhang on effective agents, Hamel Husain and
  Shreya Shankar on evals, the whole AI Engineer conference — are **standard licence, every one.**

So the honest statement of Q2 is: **the strategy is not corpus-bound, it is canon-bound.** We can
manufacture as much as we can afford; we cannot manufacture the specific sessions a practitioner
would name if asked what to watch. The `reference` tier (D3) is therefore not a nice-to-have that
softens a licence constraint — **it is what makes the collection recognisable to its own ICP.**
Removing it would leave a technically impressive corpus about GPU scheduling that no AI engineer
would recognise as a map of their field.

The visible symptom, in one line: `agents` is the topic the field is loudest about right now, and
it is the **weakest tag in the `full` tier (4 of 25) and the strongest in `reference` (9 of 35).**

---

## 3. CC-BY-rich vs barren — reusable for every future collection

### Rich (verified CC-BY, per-video, this run)

| Channel | Verdict |
|---|---|
| **CNCF [Cloud Native Computing Foundation]** | The single richest source found. ~70 CC-BY AI/LLM sessions in the topic window; the only publisher that appeared organically in *unfiltered* search results (6 of the 10). Infrastructure-angled. |
| **The Linux Foundation** | Second richest, ~53 in-window CC-BY sessions. Same infrastructure bias; heavy on serving, KServe/vLLM, observability. |
| **DevConf** (US / CZ / IN) | Consistently CC-BY, genuinely engineering-level, and covers evals and inference — the best *content-to-licence* ratio in the set. Underrated. |
| **EuroPython Conference** | Reliably CC-BY and the closest thing in the CC pool to application-layer content (RAG limits, prototype→production, semantic search). Highest-value channel for this specific topic. |
| **Southern California Linux Expo (SCaLE)** | CC-BY, long-form (45–60 min), practical. |
| **Snorkel AI** | Vendor channel but CC-BY — and unusually, its material is *evaluation*, the topic the CC pool is otherwise thinnest on. |
| **Plain Schwarz** (Berlin Buzzwords) | CC-BY, strong on search/embeddings/eval. |
| Apache Airflow · The ASF · FOSSASIA · Postgres Conference · Python India · Bulgarian JUG · Cloud Native Rejekts · R Consortium · Red Hat Developer | All confirmed CC-BY, in small numbers. Useful for topping up cross-source diversity. |

### Barren (checked directly, zero CC-BY)

| Channel | Result |
|---|---|
| **GOTO Conferences** | 6/6 standard licence |
| **NDC Conferences** | 10/10 standard licence |
| **PyData** | 31/31 standard licence |
| **Devoxx** | 42/42 standard licence |
| **AI Engineer** (the topic's flagship event) | 0 CC-BY across 26 verified videos |
| **MLOps.community** | 0 CC-BY |
| Vendor channels (Google Cloud Tech, Microsoft Developer, IBM Technology, Databricks, MongoDB, Cohere, Anyscale, LlamaIndex) | 0 CC-BY, as expected. `reference` at best. |

**Correction to the brief:** GOTO, NDC, PyData and Devoxx were listed as CC-BY-friendly leads.
**They are not** — all four are standard YouTube licence on every video checked. The lead list is
folklore; the API field is the evidence. This is exactly the failure mode D4 warns about.

### 🚩 FOSDEM — the finding worth remembering

Two separate problems, both of which would have silently produced a wrong answer:

1. **The channel id in the brief (`UCLTNKXNBmnKiCr-yYICcXJA`) does not exist.** `channels.list`
   returns an empty `items` array for it. `search.list` with a dead `channelId` does not error —
   it returns **zero results with HTTP 200**. A typo and a barren channel are indistinguishable
   unless you check. The real id is **`UC9NuJImUbaSNKiwF2bdSfAw`**, now in the script with a
   comment. *Any channel id used in future runs must be validated against `channels.list` first.*
2. **FOSDEM's YouTube archive stops in 2020.** All 117 verified FOSDEM videos are CC-BY (the
   licence reputation is fully deserved) but they are dated 2018–2020 plus one 2009 outlier —
   **zero from 2023 or later.** Recent editions are self-hosted on `video.fosdem.org`, not YouTube.
   FOSDEM therefore contributed **nothing** to collection #1 despite being the best-licensed
   archive in open source. Its modern AI content is reachable only via an ingestion path we do
   not have, since rule 8 confines us to `fileData.fileUri` for YouTube. **Flag for the
   coordinator:** a non-YouTube ingestion path unlocks the highest-quality CC-BY archive in Europe.

---

## 4. Topics abandoned for lack of an eligible `full` core

- **"The State of AI Agents"** — the obvious, most saleable cut, and the first one attempted. It
  fails property 1 outright: the canonical agent sessions (Anthropic's *How We Build Effective
  Agents*, *12-Factor Agents*, the MCP workshop, Karpathy) are standard licence without exception,
  and CC-BY agent material is thin (4 sessions) and skewed to observability and Kubernetes
  orchestration rather than agent design. A `full`-tier agents collection would have been a
  collection about *running* agents pretending to be one about *building* them. Folded in as a
  topic tag instead; revisit if Q1's legal review widens what may be published.
- **"Model Context Protocol"** — genuinely evergreen-adjacent and high interest, but the CC-BY
  pool is ~6 sessions, all from 2026, all infrastructure-flavoured (MCP authorization, MCP
  security, MCP sprawl). Fails property 2 (an afternoon's viewing) *and* property 3 (it is a
  schedule right now, not yet a subject). Strong candidate for collection #3 in two conference
  cycles.
- **Company/vendor-first cuts** — already rejected in DECISIONS.md; this run confirms the reason
  empirically at 0 CC-BY across every vendor channel checked.

---

## 5. Size of the M3 Vertex batch

**`full` tier: 25 sessions, 15.62 video-hours** (56,232 s). Shortest 21 min, longest 61 min; median
around 31 min. **No session exceeds the 90-minute cost-flag threshold**, so there is no long-tail
cost risk in this batch — the `cost_flag` field in `corpus.json` is empty for all 25.

`reference` tier: 35 sessions, 22.6 hours, **zero inference cost by construction** — index entries
only, no derived text (D3).

---

## 6. Re-running this

```bash
cd experiments/006-collections-strategy/scripts
python yt_licence_audit.py discover   # cached in out/raw_search.json; --refresh to re-search
python yt_licence_audit.py verify     # videos.list — this is the licence evidence step
python yt_licence_audit.py build      # curation.json + candidates.json -> out/corpus.json
```

Quota spent this run: **~4,100 of 10,000 units** (33 `search.list` calls @100 + ~30 `videos.list`
@1 + probes). Well inside budget; no exhaustion, no truncated results. Discovery is cached, so a
re-verify of all 1,202 licences costs **25 units** — cheap enough to re-run immediately before
manufacturing and again before publishing, which is the recommended practice given that licences
can change under us.
