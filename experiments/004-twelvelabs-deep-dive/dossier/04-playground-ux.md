# 04 — TwelveLabs: Playground UX Walkthrough

> 🧊 Point-in-time (2026-07-20), never authoritative. Every surface is backed by a dated screenshot
> in `assets/` named `<surface>-YYYY-MM-DD.png`. **No screenshot shows the full API key** — the key
> was captured only into the gitignored `.env.local` and the UI masks it as `tlk_0832********`.
> Hands-on session: logged in once to the founder's real free-tier account (2026-07-20).

## 0. Account state (recorded for the run)
- **Plan:** Free Plan. Two product lines shown separately in Billing:
  - **Jockey** (agent layer): Free Plan, **$0/month**, Knowledge store **0 / 5 GB**.
  - **Marengo & Pegasus**: Free plan, **Pay-as-you-go**, but gated by free-tier caps (below).
- **Free-tier caps (Marengo/Pegasus, from Billing page):** Video-hours usage **0 / 10 hr**
  (shared across Indexing + Analyze & Segment), Max duration per index **0 / 10 hr**, Max videos
  per index **0 / 100 videos**. `assets/billing-2026-07-20.png`.
- **Billing period:** Jul 20 – Aug 1, 2026; charge date Aug 2, 2026; **$0**; no payment method
  registered (never registered — contract §2). Index expires **90 days** after creation on Free.
- **API keys generated this run:** 1 — name `mdreel-research-2026-07`, 3-month expiry (exp
  07/20/2027), status Active. Value stored only in `.env.local`.
- **Note (mdreel rule 10 relevance):** the playground itself loads **Google Analytics**
  (`G-END0TB2RFD`, `region1.analytics.google.com`) and **HubSpot** livechat + collected-forms +
  a HubSpot visitor token — i.e. TwelveLabs ships US analytics/tracking on its own app. mdreel
  prohibits exactly this (CLAUDE.md rule 10). `api-captures/playground-network-2026-07-20.txt`.

## 1. Login & dashboard (`assets/home-2026-07-20.png`)
- Auth is a hosted Auth0-style flow at `auth.twelvelabs.io` (email → password, or "Continue with
  Google"). Login sidebar carries an NVIDIA testimonial (Sid Siddek).
- Home ("Take the next step") = big **drag-and-drop upload** target with constraints printed on it:
  **Duration 4 sec–4 hr · Resolution 360p–4k · Ratio 1:1–1:2.4 · File size ≤4 GB per video**.
- Left rail nav: get-started · Home · **indexes · assets · entities · search · analyze · segment ·
  embed** · examples; bottom: api-keys, billing. Header shows live **"Used 0 hr / 10 hr"** meter.
- A **"$100M Series B / Video Superintelligence"** banner and a **Jockey** ("agent layer for your
  video corpus") hero card dominate the home — Jockey/agent is the promoted headline surface.
- Pre-seeded **SAMPLE indexes** for exploration: "Mix" (161 videos, 8h35m), "Ads" (27, 47m),
  "E Learning" (24, 2h41m), "Social Media", "Sports" — plus an empty "My Index (Default)".

## 2. Index creation (both model types)
- A default empty index is auto-created on signup ("My Index (Default)", created Jul 20 2026).
- Free-plan notice on the index list: *"You are currently on the Free Plan, which means that your
  index will expire 90 days after it was created."* `assets/home-2026-07-20.png`.
- [SPECULATION] Model-family choice (Marengo vs Pegasus embedding engines) is selected at index
  creation via the "+" control; not screenshotted this run (default index sufficed).

## 3. Upload flow
- Home drop-zone (`Choose File` / "Drop videos or browse files") with the 4 sec–4 hr / 360p–4k /
  ≤4 GB constraints. Backed by a signing flow — six `POST /api/sign` calls fire on load (S3
  multipart pre-sign). `api-captures/playground-network-2026-07-20.txt`.

## 4. Search UI (`assets/search-2026-07-20.png`)
The Search page is a **live API-form builder** that mirrors `POST /v1.3/search` 1:1:
- `index` (required, select), `query_text` (string; placeholder "Search actions, objects, sounds
  and logos"), plus **Image** and **Entity** query modes (present but **disabled** on this
  surface/plan).
- `search_options` (array, checkboxes): **Visual, Audio, Transcription** (all checked by default).
- `transcription_options` (array): **Lexical, Semantic** (both checked).
- Advanced: `operator` enum **OR**/AND · `group_by` enum **Clip**/(video) · `page_limit` int (10) ·
  `include_user_metadata` bool · `filter` object.
- Curated example prompts frame the ICP: viral-clip retrieval, sports highlights, superhero movie
  moments, object counting, **logo/on-screen-text detection**, sound search — media/sports/brand.

## 5. Analyze / generate UI (`assets/analyze-2026-07-20.png`)
- Inputs: `video` (required) + a free-text prompt. Advanced Settings expose **temperature** (default
  **0.2**) and **max tokens** with range **512 – 98304** (confirms Pegasus's ~98k response ceiling,
  dossier 02).
- Header: *"Generate summaries, chapters, highlights and more insights"*. Example prompts: police
  report with exact timestamps, content recommendation, content moderation, visual-component Q&A,
  video description — open-ended prompt → generated text (no fixed Markdown export; you define the
  ask). Maps to the open-ended Pegasus `/analyze` endpoint (dossier 03).

## 6. Segment & Embed UIs
- `/segment` (Pegasus 1.5 video segmentation) and `/embed` (Embed v2) exist as first-class left-rail
  surfaces. Not deep-captured this run (empty default index; core Search/Analyze sufficed to map the
  API-form pattern). Both follow the same form-builder-over-REST pattern.

## 7. API-key page (`assets/api-keys-empty-2026-07-20.png`, `assets/api-keys-created-2026-07-20.png`)
- Table: Name · API key · Status · Expires by · Date created. Empty state: *"No API Keys. Create API
  keys to access TwelveLabs API"*.
- **Create dialog:** Name + Expiration presets (3 / 6 / 12 months · Custom · Never expire).
- On creation a one-time **"Save your API key"** dialog shows the full key with the warning *"you
  won't be able to view it again"*; the list thereafter shows only the masked prefix `tlk_0832********`.
  (The full key was extracted programmatically into `.env.local`, never screenshotted.)

## 8. Usage / quota / billing page
- **Billing** (`assets/billing-2026-07-20.png`): plan cards for Jockey (Free, 5 GB store) and
  Marengo & Pegasus (PAYG), the free-tier caps (§0), payment-info block ("Register payment method"),
  and an empty "Billing history" table.
- **Rate limits** (`assets/rate-limits-2026-07-20.png`, "Tier: free"), captured values:

  | Endpoint | Requests/day | Duration/day | Output tokens/day |
  |---|---|---|---|
  | Index | 3K | 180K sec | — |
  | Upload | 3K | — | — |
  | Search | 3K | — | — |
  | Analyze | 1K | 180K sec | 500K |
  | Summarize | 1K | 180K sec | 500K |
  | Embed – Video | 3K | 180K sec | — |
  | Embed – Audio | 3K | 180K sec | — |
  | Embed – Text / Image / Text_Image | 3K | — | — |

## 9. Settings, docs links, empty/error states
- Settings sub-nav: **Organization · API keys · Billing & plan · Usage · Rate limits · Webhooks ·
  Profile** (`/dashboard/*`). A dedicated **Webhooks** settings surface confirms async callbacks.
- Resources cards recur across pages: **API Quickstart** (github.com/twelvelabs-io/
  twelvelabs-developer-experience), **Sample Apps** (twelvelabs.io/sample-apps), **Jockey MCP**
  (docs → model-context-protocol).
- **Get-started is MCP-first** (`assets/get-started-2026-07-20.png`): "Get started with Jockey" —
  connect the **Jockey MCP** (`https://mcp.twelvelabs.io/jockey/mcp`) to **Claude / Claude Code /
  ChatGPT**, then run prompts (search videos, corpus overview, extract entities, assemble highlight
  reels). The onboarding funnel leads with agent/MCP, not raw REST.

## 10. Internal API shape (from `browser_network_requests`)
`api-captures/playground-network-2026-07-20.txt`. The playground is a Next.js BFF: it mints
short-lived tokens via `playground.twelvelabs.io/api/access-token` then calls the public/edge API:
- **Public API base:** `https://api.twelvelabs.io/v1.3/` — e.g. `GET /v1.3/indexes`,
  `GET /v1.3/indexes/{id}`.
- **Internal/private surfaces:** `/tl/iam/organization-accounts`, `/tl/billing/usage`,
  `/tl/playground/samples/v1.3/indexes/{id}`, `/v1.3/private/indexes/{id}/indexed-assets/tasks/counts`.
- Sample indexes return **403** on the plain `/v1.3/indexes/{id}` path but load via the
  `/tl/playground/samples/...` path — i.e. samples are a curated playground-only surface, not part
  of the customer's own quota.
- Control-plane / feature flags via `control.twelvelabs.io/api/frontend`.

## Screenshot index

| File | Surface | Captured |
|---|---|---|
| `home-2026-07-20.png` | Home / dashboard + upload + sample indexes | 2026-07-20 |
| `api-keys-empty-2026-07-20.png` | API-keys page (empty state) | 2026-07-20 |
| `api-keys-created-2026-07-20.png` | API-keys page (1 key, masked) | 2026-07-20 |
| `billing-2026-07-20.png` | Billing & plan (free-tier caps) | 2026-07-20 |
| `rate-limits-2026-07-20.png` | Rate limits (Tier free) | 2026-07-20 |
| `search-2026-07-20.png` | Search API-form builder | 2026-07-20 |
| `analyze-2026-07-20.png` | Analyze / generate | 2026-07-20 |
| `get-started-2026-07-20.png` | MCP-first onboarding (Jockey) | 2026-07-20 |
