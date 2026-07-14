# Non-technical agent workflows for vectorreel

Chained AI-subagent workflows for the **business / legal / marketing** side of building
vectorreel (EU SaaS: video → structured, timestamped Markdown for AI knowledge bases).

These agents are **prompt files**, not code. To run one, open the `.md`, paste its body as
the system/role prompt into your LLM of choice, then feed it your vectorreel context
(EU-only, per-job LLM cost ledger, video→Markdown output). They live under `experiments/`
by design — no repo standards apply and `src/` never imports them.

## Two sources, two jobs

| Source | Folder | Strength | Use for |
|--------|--------|----------|---------|
| VoltAgent | `volt/` | Strategy / frameworks | **Decide** what to do |
| wshobson  | `wshobson/` | Execution / artifacts | **Produce** the deliverable |

> Rule of thumb: use a `volt/` agent to choose the direction, then a `wshobson/` agent to
> generate the concrete asset. The value is in **chaining output → next input**, not in
> running any single agent alone.

---

## How the workflows connect

```
W1 (de-risk) ──► W2 (validate demand) ──► W4 (go-to-market engine)
                      │
                      └► W3 (EU legal) runs in parallel, before first real user data
```

---

## Workflow 1 — "Should I build this at all?" (pre-code de-risking)

**Goal:** a go/no-go verdict + a ranked list of assumptions to test, before writing
pipeline (Stage A–D) code.

| Step | Agent | Input | Output |
|------|-------|-------|--------|
| 1 | `volt/assumption-mapping.md` | 1-paragraph idea + target user | Assumptions ranked by VUBF risk (Value / Usability / Business viability / Feasibility) |
| 2 | `wshobson/before-you-build/SKILL.md` | Assumption list from step 1 | Short pre-mortem verdict (build / reshape / don't) |
| 3 | `wshobson/startup-analyst.md` | Surviving idea + segment | TAM/SAM/SOM + unit economics — plug in real numbers from the LLM cost ledger (cost per video-minute vs price) |
| 4 | `volt/ux-researcher.md` | Top 2 "Value" assumptions | 5-interview script + lightweight experiment to test them |

**Deliverable:** a 1-page decision memo. Run this *before* any pipeline work.

---

## Workflow 2 — "Prove demand cheaply" (validation loop)

**Goal:** test the riskiest assumption with a landing page + outreach — no product yet.

| Step | Agent | Input | Output |
|------|-------|-------|--------|
| 1 | `volt/product-manager.md` | Idea + ICP notes | Sharp positioning statement + ideal customer profile |
| 2 | `wshobson/content-marketer.md` | Positioning from step 1 | Landing page copy (hero, value props, FAQ) |
| 3 | `wshobson/sales-automator.md` | ICP + landing page | 3 cold emails to reach ~20 target users |
| 4 | `volt/ux-researcher.md` | Replies from step 3 | Interview guide for respondents |

Loop weekly. Results feed **back** into `volt/assumption-mapping.md` (W1) to re-rank.

**Deliverable:** landing page + outreach kit + a validated-or-killed assumption.

---

## Workflow 3 — "EU legal baseline" (compliance, do once, early)

**Goal:** the mandatory GDPR paperwork for an EU SaaS that processes user video.
Runs in parallel with W2, **before** any real user data lands.

| Step | Agent | Input | Output |
|------|-------|-------|--------|
| 1 | `wshobson/legal-advisor.md` | Product + data-flow description | GDPR-compliant Privacy Policy, Terms of Service, Cookie Policy, Data Processing Agreement (you are a processor for customers' content) |
| 2 | `volt/legal-advisor.md` | Drafts from step 1 + sub-processors (Vertex/GCP) | Review of sub-processor list, IP, data-retention & deletion clauses |
| 3 | `volt/license-engineer.md` | Your .NET/OSS dependency list | Dependency-license audit; confirm nothing blocks commercial SaaS distribution |

**Deliverable:** committed legal docs + a sub-processor register. Feeds `BUSINESS_MODEL.md`
and `INFRA.md`.

> ⚠️ These agents **draft**, they do not replace a lawyer. Treat all output as a reviewed
> first draft, not legal advice.

---

## Workflow 4 — "Go-to-market engine" (once validated)

**Goal:** a repeatable acquisition system, not one-off posts.

| Step | Agent | Input | Output |
|------|-------|-------|--------|
| 1 | `volt/growth-loops.md` | Validated positioning + channel notes | One acquisition loop (likely content/SEO for a dev / knowledge-base audience) + its constraint |
| 2 | `wshobson/seo-content-planner.md` | Chosen loop + topic | Keyword clusters → content calendar |
| 3 | `wshobson/seo-content-writer.md` + `wshobson/content-marketer.md` | Calendar items | Articles (e.g. "video → RAG-ready Markdown" how-tos) |
| 4 | `volt/customer-success-manager.md` | First-user journey | Onboarding flow + activation metric (first successful video→Markdown job) |

**Deliverable:** a growth loop with one measured constraint + a content pipeline feeding it.

---

## Operating tips

- **Always inject vectorreel context.** Without it (EU-only, cost ledger, video→Markdown,
  RAG/knowledge-base users) the output stays generic and useless.
- **Chain, don't isolate.** Each step's output is the next step's input — that's the point.
- **`volt/` decides, `wshobson/` builds.** Pick strategy with VoltAgent, generate assets
  with wshobson.
- **Legal output is a draft, not advice.** Have a lawyer review W3 before publishing.

## Inventory

```
volt/      assumption-mapping, ux-researcher, product-manager,
           legal-advisor, license-engineer, growth-loops, customer-success-manager
wshobson/  startup-analyst, content-marketer, sales-automator, legal-advisor,
           seo-content-planner, seo-content-writer,
           before-you-build/ (SKILL.md + references/risk-checklist.md)
```

Upstream sources:
- <https://github.com/VoltAgent/awesome-claude-code-subagents> (`categories/08-business-product`)
- <https://github.com/wshobson/agents> (`plugins/*`)
