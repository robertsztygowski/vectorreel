import type { Metadata } from 'next';
import Link from 'next/link';
import { CodeCard } from '@/components/CodeCard/CodeCard';

export const metadata: Metadata = { title: 'Docs — mdreel API, webhooks & MCP' };

const ENDPOINTS: { method: string; path: string; purpose: string }[] = [
  { method: 'POST', path: '/uploads', purpose: 'Get a signed GCS URL + uploadId for a resumable upload.' },
  { method: 'POST', path: '/jobs', purpose: 'Start a job from an uploadId. Returns 202 { jobId }.' },
  { method: 'GET', path: '/jobs/{id}', purpose: 'Poll status: queued · processing (stage, progress) · done · failed.' },
  { method: 'GET', path: '/jobs/{id}/output.md', purpose: 'Download the Markdown (or output.json for structured blocks).' },
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
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>API, webhooks &amp; MCP</h1>
          <p className="lead">
            mdreel is API-first. Everything the UI does is a call you can make yourself — REST for pipelines, webhooks
            for completion, and an MCP server so an assistant can process a video from inside your IDE. All three ship
            in the MVP.
          </p>
        </div>
      </div>

      <div className="page-body">
        <div className="wrap page-narrow docs">
          <section id="rest">
            <h2>REST API</h2>
            <p>
              Base URL <code>/api/v1</code>. Authenticate with a bearer token:{' '}
              <code>Authorization: Bearer &lt;API_KEY&gt;</code>. Errors are RFC 7807 <code>problem+json</code>; rate
              limits are per key.
            </p>
            <div className="endpoint-table">
              {ENDPOINTS.map((e) => (
                <div className="endpoint-row" key={`${e.method} ${e.path}`}>
                  <span className={`method method-${e.method.toLowerCase()}`}>{e.method}</span>
                  <code>{e.path}</code>
                  <span className="endpoint-purpose">{e.purpose}</span>
                </div>
              ))}
            </div>
            <CodeCard title="create-job.sh" content={CREATE_JOB} />
          </section>

          <section id="webhooks">
            <h2>Webhooks</h2>
            <p>
              Pass a <code>webhook_url</code> when you create a job and we POST it when the job finishes. Every payload
              is signed with HMAC-SHA256 over the raw body in an <code>X-Mdreel-Signature</code> header — verify it
              before trusting the event.
            </p>
            <CodeCard title="job.completed" content={WEBHOOK} />
          </section>

          <section id="mcp">
            <h2>MCP server</h2>
            <p>
              The mdreel MCP server is a thin layer over the same REST API — no separate business logic — so an AI
              assistant can process a recording and cite its Markdown without leaving the editor. Point your MCP client
              at it:
            </p>
            <CodeCard title=".mcp.json" content={MCP} />
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
