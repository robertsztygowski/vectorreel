'use client';

import { useEffect, useMemo, useRef, useState } from 'react';
import { useParams, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { JobStepper, type JobStageKey, type JobStatusState } from '@/components/JobStepper/JobStepper';
import { MarkdownOutputCard } from '@/components/MarkdownOutputCard/MarkdownOutputCard';
import { withMdreelSessionHeader } from '@/lib/apiHeaders';
import { trackJobCompleted, trackOutputDownloaded, trackUploadRepeat } from '@/lib/events';
import type { OutputDocument } from '@/lib/outputDocument';

const STAGES: JobStageKey[] = ['A', 'B', 'C', 'D'];
const UPLOAD_COUNT_KEY = 'mdreel_upload_count';
const UPLOAD_FIRST_KEY = 'mdreel_upload_first_at';

interface JobStatusResponse {
  status: JobStatusState;
  stage?: JobStageKey;
  progress: number;
  cost_cents?: number;
  duration_sec?: number;
  wall_clock_sec?: number;
}

// GET /jobs/{id}/output.json returns the bare contract document (output.schema.json); the raw
// Markdown bytes come from GET /jobs/{id}/output.md — fetched alongside for the raw tab/download.
interface JobOutputView {
  document: OutputDocument;
  raw: string;
}

function decodeJobSummary(jobId: string): { filename?: string; durationSec?: number } {
  try {
    const normalized = jobId.replace(/-/g, '+').replace(/_/g, '/');
    const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=');
    const json = globalThis.atob(padded);
    const token = JSON.parse(json) as { meta?: { filename?: string; durationSec?: number } };
    return { filename: token.meta?.filename, durationSec: token.meta?.durationSec };
  } catch {
    return {};
  }
}

function formatClock(totalSeconds: number): string {
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = Math.round(totalSeconds % 60);
  return [hours, minutes, seconds].map((part) => String(part).padStart(2, '0')).join(':');
}

export default function JobStatusPage() {
  const params = useParams<{ jobId: string }>();
  const searchParams = useSearchParams();
  const jobId = params.jobId;
  const jobSummary = useMemo(() => decodeJobSummary(jobId), [jobId]);
  const [status, setStatus] = useState<JobStatusResponse | null>(null);
  const [output, setOutput] = useState<JobOutputView | null>(null);
  const [notFound, setNotFound] = useState(false);
  const trackedDoneRef = useRef(false);
  const forcedState = searchParams.get('state') as JobStatusState | null;

  useEffect(() => {
    let cancelled = false;
    async function poll() {
      const res = await fetch(`/api/v1/jobs/${jobId}`, { credentials: 'include', headers: withMdreelSessionHeader() });
      if (cancelled) return;
      if (!res.ok) {
        setNotFound(true);
        return;
      }
      const next = (await res.json()) as JobStatusResponse;
      if (forcedState) {
        if (forcedState === 'queued') setStatus({ status: 'queued', progress: 0 });
        else if (forcedState === 'processing') setStatus({ status: 'processing', stage: 'C', progress: 64 });
        else if (forcedState === 'failed') setStatus({ status: 'failed', progress: 100 });
        else setStatus({ ...next, status: 'done', progress: 100, stage: 'D' });
        return;
      }
      setStatus(next);
    }
    poll();
    const timer = setInterval(() => {
      if (status?.status === 'done' || status?.status === 'failed') return;
      poll();
    }, 900);
    return () => {
      cancelled = true;
      clearInterval(timer);
    };
  }, [forcedState, jobId, status?.status]);

  useEffect(() => {
    if (status?.status !== 'done' || trackedDoneRef.current) return;
    trackedDoneRef.current = true;

    (async () => {
      const [jsonRes, mdRes] = await Promise.all([
        fetch(`/api/v1/jobs/${jobId}/output.json`, { credentials: 'include', headers: withMdreelSessionHeader() }),
        fetch(`/api/v1/jobs/${jobId}/output.md`, { credentials: 'include', headers: withMdreelSessionHeader() }),
      ]);
      if (!jsonRes.ok || !mdRes.ok) return;
      const document = (await jsonRes.json()) as OutputDocument;
      const raw = await mdRes.text();
      setOutput({ document, raw });

      trackJobCompleted({
        job_id: jobId,
        duration_sec: status.duration_sec ?? 0,
        cost_cents: status.cost_cents ?? 0,
        wall_clock_sec: status.wall_clock_sec ?? 0,
      });

      const count = Number(window.localStorage.getItem(UPLOAD_COUNT_KEY) ?? '0') + 1;
      window.localStorage.setItem(UPLOAD_COUNT_KEY, String(count));
      if (count === 1) {
        window.localStorage.setItem(UPLOAD_FIRST_KEY, new Date().toISOString());
      } else {
        const firstAt = window.localStorage.getItem(UPLOAD_FIRST_KEY);
        const daysSince = firstAt ? Math.round((Date.now() - new Date(firstAt).getTime()) / 86_400_000) : 0;
        trackUploadRepeat({ n_th: count, days_since_first: daysSince });
      }
    })();
  }, [status, jobId]);

  if (notFound) {
    return (
      <div className="app-page">
        <div className="wrap page-narrow">
          <h1>Job not found</h1>
          <p className="lead">This job id doesn&apos;t exist — it may have expired or was never created.</p>
          <Link className="btn btn-ghost" href="/app">
            back to library
          </Link>
        </div>
      </div>
    );
  }

  const done = status?.status === 'done';
  const failed = status?.status === 'failed';
  const sourceFilename = jobSummary.filename ?? (done && output ? output.document.frontmatter.source : 'Processing your recording');
  // Real job ids are opaque (no embedded token), so fall back to the source recorded in the
  // finished document's frontmatter for the download name.
  const outputBaseName = (jobSummary.filename ?? output?.document.frontmatter.source ?? 'output.mp4').replace(/\.[^./]+$/, '');
  const outputFilename = `${outputBaseName}.md`;

  return (
    <div className="app-page">
      <div className="wrap">
        <div className="app-head-row">
          <div>
            <h1>{sourceFilename}</h1>
            <p className="lead">
              {done
                ? 'Done — download it below, or find it any time in your library.'
                : failed
                  ? 'Simulated failure for QA — nothing was charged.'
                  : 'Your recording is running through the real pipeline stages, in the EU.'}
            </p>
          </div>
          {status && <span className={`badge badge-${status.status}`}>{status.status}</span>}
        </div>

        {status && <JobStepper stages={STAGES} status={status.status} activeStage={status.stage} progress={status.progress} />}

        {failed && <p style={{ color: 'var(--err)', marginTop: 16 }}>Job failed — simulated failure for QA.</p>}

        {output && (
          <div style={{ marginTop: 28 }}>
            <div className="ledger" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 20, flexWrap: 'wrap', marginBottom: 14 }}>
              <p style={{ margin: 0 }}>
                <span className="k">ledger:</span> counts {formatClock(status?.duration_sec ?? 0)} against your trial credit · wall-clock{' '}
                {formatClock(status?.wall_clock_sec ?? 0)}
              </p>
              <p className="retention-ok" style={{ margin: 0 }}>
                ✓ source video deleted after processing
              </p>
            </div>
            <MarkdownOutputCard
              frontmatter={output.document.frontmatter}
              sections={output.document.sections}
              raw={output.raw}
              filename={outputFilename}
              onDownload={() => trackOutputDownloaded({ job_id: jobId })}
            />
            <p className="micro" style={{ marginTop: 20 }}>
              <Link href="/app">← back to library</Link>
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
