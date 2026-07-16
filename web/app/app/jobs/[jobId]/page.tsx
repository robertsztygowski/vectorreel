'use client';

import { useEffect, useRef, useState } from 'react';
import { useParams, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { JobStepper, type JobStageKey, type JobStatusState } from '@/components/JobStepper/JobStepper';
import { MarkdownOutputCard } from '@/components/MarkdownOutputCard/MarkdownOutputCard';
import { trackJobCompleted, trackOutputDownloaded, trackUploadRepeat } from '@/lib/events';
import type { ParsedMarkdown } from '@/lib/corpus';

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

interface OutputJson extends ParsedMarkdown {
  raw: string;
  filename: string;
}

export default function JobStatusPage() {
  const params = useParams<{ jobId: string }>();
  const searchParams = useSearchParams();
  const jobId = params.jobId;
  const [status, setStatus] = useState<JobStatusResponse | null>(null);
  const [output, setOutput] = useState<OutputJson | null>(null);
  const [notFound, setNotFound] = useState(false);
  const trackedDoneRef = useRef(false);
  const forcedState = searchParams.get('state') as JobStatusState | null;

  useEffect(() => {
    let cancelled = false;
    async function poll() {
      const res = await fetch(`/api/jobs/${jobId}`);
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
      const res = await fetch(`/api/jobs/${jobId}/output.json`);
      if (!res.ok) return;
      const data = (await res.json()) as OutputJson;
      setOutput(data);

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
      <div className="page-body">
        <div className="wrap page-narrow">
          <h1>Job not found</h1>
          <p className="lead">This job id doesn&apos;t exist — it may have expired or was never created.</p>
          <Link className="btn btn-ghost" href="/app">
            Back to library
          </Link>
        </div>
      </div>
    );
  }

  const done = status?.status === 'done';
  const failed = status?.status === 'failed';

  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>{done ? 'Your Markdown is ready' : failed ? 'Processing failed' : 'Processing your recording'}</h1>
          <p className="lead">
            {done
              ? 'Done — download it below, or find it any time in your library.'
              : failed
                ? 'Simulated failure for QA — nothing was charged.'
                : 'This mirrors the real pipeline stages, but every step here is simulated.'}
          </p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap page-narrow">
          {status && !output && (
            <JobStepper stages={STAGES} status={status.status} activeStage={status.stage} progress={status.progress} />
          )}

          {failed && <p style={{ color: 'var(--err)', marginTop: 16 }}>Job failed — simulated failure for QA.</p>}

          {output && (
            <div style={{ marginTop: 28 }}>
              <MarkdownOutputCard
                h1={output.h1}
                frontmatter={output.frontmatter}
                sections={output.sections}
                raw={output.raw}
                filename={output.filename}
                onDownload={() => trackOutputDownloaded({ job_id: jobId })}
              />
              <p className="micro" style={{ marginTop: 20 }}>
                <Link href="/app">← Back to your library</Link>
              </p>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
