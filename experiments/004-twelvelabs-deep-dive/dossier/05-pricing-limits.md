# 05 — TwelveLabs: Pricing & Limits

> 🧊 Point-in-time (2026-07-20), never authoritative. Documented values first (M2), then observed
> values (M4). Grades: **Q** primary, **T** third-party. USD→EUR conversion, when applied, is
> stated explicitly (002 used 0.90).

## 1. Plans & pricing ladder (public pricing page)

TwelveLabs operates under three main tiers. Prices are metered by usage (no monthly base fee for the Developer plan).

### Marengo Pricing (Embed / Search)
- **Video Indexing (One-time)**:
  - **Free Plan**: Free (up to 10 hours)
  - **Developer PAYG Plan**: **$0.042 per minute** (equivalent to **$2.52 per hour**)
  - **Enterprise Plan**: Custom / Committed-use
- **Infrastructure Services (Monthly / Retention)**:
  - **Free Plan**: Free
  - **Developer PAYG Plan**: **$0.0015 per minute** (equivalent to **$0.09 per hour**)
  - **Enterprise Plan**: Custom
- **Search API Usage**:
  - **Free Plan**: Free
  - **Developer PAYG Plan**: **$4.00 per 1,000 queries**
  - **Enterprise Plan**: Custom

### Embed API v2 Pricing
- **Embed API by input file type (One-time)**:
  - **Video**: Free (Free plan) | **$0.042 per minute** (Developer PAYG)
  - **Audio**: Free (Free plan) | **$0.0083 per minute** (Developer PAYG)
  - **Image**: Free (Free plan) | **$0.10 per 1,000 requests** (Developer PAYG)
  - **Text**: Free (Free plan) | **$0.07 per 1,000 requests** (Developer PAYG)

### Pegasus Pricing (Analyze / Generate)
- **Input Video (Analysis/Segmentation)**:
  - **Free Plan**: Free
  - **Developer PAYG Plan**: **$0.0292 per minute** (equivalent to **$1.752 per hour**)
    - *Segmentation billing*: Paid segmentation is charged per segment definition. Total cost = billable video duration × number of segment definitions (effective May 7, 2026).
  - **Enterprise Plan**: Custom
- **Output Text Generation**:
  - **Free Plan**: Free
  - **Developer PAYG Plan**: **$0.0075 per 1,000 tokens** (equivalent to **$7.50 per 1M tokens**)
  - **Enterprise Plan**: Custom

---

## 2. Free-tier limits (documented)

New accounts are assigned the **Free Plan** by default without requiring a credit card:
- **Combined Video Hours Limit**: A single **10-hour limit (600 minutes)** is shared across video indexing, video analysis, and segmentation (effective May 7, 2026).
  - This limit is cumulative and does not refresh. Deleting indexes or uploaded assets does not restore used minutes.
- **Index Retention**: Stored indexes are kept for **90 days** from creation. Upgrading to a Developer plan grants unlimited retention for active indexes that have not yet expired.
- **Index Scope limits**:
  - **Duration per index**: Maximum 10 hours.
  - **Volume per index**: Maximum 100 videos.
  - **Concurrent indexing tasks**: Maximum 5 concurrent tasks.

---

## 3. Documented rate limits / quotas / video constraints

### Multi-Dimensional Rate Limits
Rate limits are applied across multiple dimensions: DPD (Duration Per Day in minutes), DPH (Duration Per Hour in minutes), RPD (Requests Per Day), RPM (Requests Per Minute), TPD (Tokens Per Day in thousands), and TPM (Tokens Per Minute in thousands).

#### Free Plan & Developer Tier 1 (Default Developer Plan)
- **Index Category**: DPD: 3,000 min | DPH: 600 min | RPD: 3,000 | RPM: 60
- **Upload Category**: RPD: 3,000 | RPM: 60
- **Search Category**: RPD: 3,000 | RPM: 600
- **Analyze Category**: DPD: 3,000 min | DPH: 600 min | RPD: 1,000 | RPM: 60 | TPD: 500k tokens | TPM: 30k tokens
- **Embed Video / Audio Categories**: DPD: 3,000 min | DPH: 600 min | RPD: 3,000 | RPM: 25
- **Embed Text / Image / Text_Image Categories**: RPD: 3,000 | RPM: 600

#### Developer Tier 2 (Spend ≥ $200/month)
- **Index Category**: DPD: 6,000 min | DPH: 1,200 min | RPD: 6,000 | RPM: 120
- **Upload Category**: RPD: 6,000 | RPM: 120
- **Search Category**: RPD: 6,000 | RPM: 1,200
- **Analyze Category**: DPD: 6,000 min | DPH: 1,200 min | RPD: 2,000 | RPM: 120 | TPD: 1,000k tokens | TPM: 60k tokens
- **Embed Video / Audio Categories**: DPD: 6,000 min | DPH: 1,200 min | RPD: 6,000 | RPM: 50
- **Embed Text / Image / Text_Image Categories**: RPD: 12,000 | RPM: 1,200

#### Developer Tier 3 (Spend ≥ $400/month)
- **Index Category**: DPD: 12,000 min | DPH: 2,400 min | RPD: 12,000 | RPM: 240
- **Upload Category**: RPD: 12,000 | RPM: 240
- **Search Category**: RPD: 12,000 | RPM: 2,400
- **Analyze Category**: DPD: 12,000 min | DPH: 2,400 min | RPD: 3,000 | RPM: 240 | TPD: 1,500k tokens | TPM: 120k tokens
- **Embed Video / Audio Categories**: DPD: 12,000 min | DPH: 2,400 min | RPD: 9,000 | RPM: 75
- **Embed Text / Image / Text_Image Categories**: RPD: 30,000 | RPM: 2,400

### Video Constraints
- **Marengo 3.0**:
  - **File Duration**: 4 seconds to 4 hours.
  - **File Size**: ≤ 4 GB (6 GB via S3/Amazon Bedrock).
  - **Resolution**: 360x360 up to 5184x2160.
- **Pegasus 1.5**:
  - **File Duration**: 4 seconds to 2 hours.
  - **File Size**: ≤ 2 GB.
- **Format Requirements**: FFmpeg supported containers. video hosting URLs (e.g. YouTube) are explicitly unsupported.

---

## 4. Observed limits & latency (from M4 hands-on)

_TBD (M4) — indexing latency, real quota decrements, discrepancies vs documented._

## 5. Effective €/video-hour (cite METRICS.md names, never restate figures)

_TBD (M5-adjacent)_

## Evidence log

| # | Claim | Source URL / capture | Checked | Grade |
|---|---|---|---|---|
| 1 | Metered Pricing: Marengo indexing $0.042/min, infra $0.0015/min, search $4/1k, Pegasus analyze $0.0292/min, output $0.0075/1k tokens | https://www.twelvelabs.io/pricing | 2026-07-20 | Q |
| 2 | Free tier features: 10 hours combined indexing+analysis limit, 90-day index access, max 10h per index, 100 videos volume, 5 concurrent indexing tasks | https://www.twelvelabs.io/pricing | 2026-07-20 | Q |
| 3 | Shared 10-hour Free plan limit announced May 7, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 4 | Segment billing rules for paid plans (duration * segment definitions) | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 5 | Multi-dimensional rate limit dimensions: DPD, DPH, RPD, RPM, TPD, TPM | https://docs.twelvelabs.io/docs/get-started/rate-limits.md | 2026-07-20 | Q |
| 6 | Free Plan and Developer Tier 1 rate limit tables | https://docs.twelvelabs.io/docs/get-started/rate-limits.md | 2026-07-20 | Q |
| 7 | Developer Tier 2 and Tier 3 spending qualifications ($200/mo and $400/mo) and rate limit tables | https://docs.twelvelabs.io/docs/get-started/rate-limits.md | 2026-07-20 | Q |
| 8 | Marengo 3.0 input requirements (4s-4h, <=4GB size, 360p-2160p resolution) | https://docs.twelvelabs.io/docs/concepts/models/marengo.md | 2026-07-20 | Q |
| 9 | Pegasus 1.5 input requirements (4s-2h, <=2GB size) | https://docs.twelvelabs.io/docs/concepts/models/pegasus.md | 2026-07-20 | Q |
| 10 | Direct links required, YouTube links unsupported | https://docs.twelvelabs.io/docs/concepts/upload-methods.md | 2026-07-20 | Q |
