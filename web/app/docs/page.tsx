import type { Metadata } from 'next';
import Link from 'next/link';
import { CodeCard } from '@/components/CodeCard/CodeCard';

export const metadata: Metadata = { title: 'Docs — mdreel API, webhooks & MCP' };

const ENDPOINTS: { method: string; path: string; purpose: string }[] = [
  { method: 'POST', path: '/uploads', purpose: 'Get a signed GCS URL + uploadId for a resumable upload.' },
  { method: 'POST', path: '/jobs', purpose: 'Start a job from an uploadId. Returns 202 { jobId }.' },
  { method: 'GET', path: '/jobs', purpose: 'List your jobs, newest first — the same list the panel shows.' },
  { method: 'GET', path: '/jobs/{id}', purpose: 'Poll status: queued · processing (stage, progress) · done · failed.' },
  { method: 'GET', path: '/jobs/{id}/output.md', purpose: 'Download the Markdown (or output.json for the structured document).' },
  { method: 'DELETE', path: '/jobs/{id}', purpose: 'Right-to-erasure: deletes outputs + metadata, audit-logged.' },
  { method: 'GET', path: '/usage', purpose: 'Hours processed this period and remaining quota.' },
  { method: 'POST', path: '/webhooks/test', purpose: 'Verify a webhook endpoint (HMAC signature).' },
];

const CREATE_JOB = `curl -X POST https://api.mdreel.eu/api/v1/jobs \\
  -H "Authorization: Bearer $MDREEL_API_KEY" \\
  -H "Content-Type: application/json" \\
  -d '{
    "uploadId": "up_9f2c…",
    "options": {
      "language_hint": "en",
      "retention_days": 0,
      "webhook_url": "https://your.app/hooks/mdreel",
      "quality": "high"
    }
  }'
# → 202 { "jobId": "job_7a1b…" }`;

const WEBHOOK = `POST https://your.app/hooks/mdreel
X-Mdreel-Signature: sha256=<hmac>

{
  "event": "job.completed",
  "jobId": "job_7a1b…",
  "durationSec": 2832,
  "outputUrl": "https://api.mdreel.eu/api/v1/jobs/job_7a1b…/output.md"
}
# Verify: HMAC-SHA256(body, your signing secret) === header`;

const MCP = `{
  "mcpServers": {
    "mdreel": {
      "command": "npx",
      "args": ["-y", "@mdreel/mcp"],
      "env": { "MDREEL_API_KEY": "sk_live_…" }
    }
  }
}
# Tools: process_video(uploadId|url), get_job(jobId), get_output(jobId), list_jobs()`;

export default function DocsPage() {
  return (
    <>
      <div className="page-head" style={{ padding: 0 }}>
        <div className="wrap" style={{ padding: '64px 32px 48px' }}>
          <p className="kicker"># docs</p>
          <h1
            style={{
              fontSize: 'clamp(32px, 3.6vw, 46px)',
              lineHeight: 1.05,
              letterSpacing: '-0.014em',
              fontWeight: 500,
              fontVariationSettings: "'opsz' 64",
              margin: '0 0 18px',
            }}
          >
            API, webhooks &amp; MCP.
          </h1>
          <p className="lead" style={{ maxWidth: '56ch' }}>
            mdreel is API-first — everything the UI does is a call you can make yourself. REST for pipelines, webhooks
            for completion, MCP so an assistant can process a video from inside the IDE.
          </p>
        </div>
      </div>

      <div className="wrap docs-grid">
        <nav className="docs-toc" aria-label="On this page">
          <div className="docs-toc-inner">
            <span className="toc-label">contents</span>
            <a href="#rest">rest api</a>
            <a href="#webhooks">webhooks</a>
            <a href="#mcp">mcp server</a>
            <a href="#llms-txt">llms.txt</a>
            <a href="#gdpr-docs">data &amp; residency</a>
          </div>
        </nav>

        <div className="docs-main docs docs-body">
          <section id="rest">
            <h2>REST API</h2>
            <div className="doc-meta">
              <span className="k">base url</span>
              <span>
                https://api.mdreel.eu<b>/api/v1</b>
              </span>
              <span className="k">auth</span>
              <span>bearer token per key</span>
              <span className="k">errors</span>
              <span>RFC 7807 · application/problem+json</span>
              <span className="k">rate limits</span>
              <span>per API key</span>
            </div>
            <div className="endpoint-table">
              {ENDPOINTS.map((e) => (
                <div className="endpoint-row" key={`${e.method} ${e.path}`}>
                  <span className={`method method-${e.method.toLowerCase()}`}>{e.method}</span>
                  <span className="path">{e.path}</span>
                  <span className="purpose">{e.purpose}</span>
                </div>
              ))}
            </div>
            <CodeCard title="create a job" lang="bash" content={CREATE_JOB} />
          </section>

          <section id="webhooks">
            <h2>Webhooks</h2>
            <p>
              Pass a <code>webhook_url</code> when you create a job and we POST it when the job finishes. Every payload
              is signed with HMAC-SHA256 over the raw body in an <code>X-Mdreel-Signature</code> header — verify it
              before trusting the event.
            </p>
            <CodeCard title="job.completed" lang="http" content={WEBHOOK} />
          </section>

          <section id="mcp">
            <h2>MCP server</h2>
            <p>
              The mdreel MCP server is a thin layer over the same REST API — no separate business logic — so an AI
              assistant can process a recording and cite its Markdown without leaving the editor. Point your MCP client
              at it:
            </p>
            <CodeCard title=".mcp.json" lang="json" content={MCP} />
            <p style={{ marginTop: 18, fontFamily: 'var(--mono)', fontSize: 13 }}>
              <span style={{ color: 'var(--ink-faint)' }}>tools:</span> process_video(uploadId|url) · get_job(jobId) ·
              get_output(jobId) · list_jobs()
            </p>
          </section>

          <section id="llms-txt">
            <h2>llms.txt</h2>
            <p>
              We publish a <code>/llms.txt</code> so an agent can discover what mdreel does and how to call it. If you
              are building an assistant that recommends tools, this is the machine-readable summary to read first.
            </p>
          </section>

          <section id="gdpr-docs">
            <h2>Data &amp; residency</h2>
            <p>
              All processing runs in EU regions; source video is deleted after processing by default, and{' '}
              <code>DELETE /jobs/&#123;id&#125;</code> is a first-class right-to-erasure endpoint. The honest residency
              story — EU data residency on Google Cloud, not full sovereignty — is on the{' '}
              <Link href="/#eu">trust page</Link>.
            </p>
          </section>
        </div>
      </div>
    </>
  );
}
