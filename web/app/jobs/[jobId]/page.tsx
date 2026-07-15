'use client';

import { useEffect, useRef, useState } from 'react';
import { useParams } from 'next/navigation';
import { JobStepper, type JobStageKey, type JobStatusState } from '@/components/JobStepper/JobStepper';
import { MarkdownOutputCard } from '@/components/MarkdownOutputCard/MarkdownOutputCard';
import { trackJobCompleted, trackOutputDownloaded, trackUploadRepeat, trackYtToolUsed } from '@/lib/events';
import type { ParsedMarkdown } from '@/lib/corpus';
import type { JobKind } from '@/lib/mockJobs';

const STAGES_BY_KIND: Record<JobKind, JobStageKey[]> = {
  youtube: ['B', 'C', 'D'],
  upload: ['A', 'B', 'C', 'D'],
};

const UPLOAD_COUNT_KEY = 'mdreel_upload_count';
const UPLOAD_FIRST_KEY = 'mdreel_upload_first_at';

interface JobStatusResponse {
  kind: JobKind;
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
  const jobId = params.jobId;
  const [status, setStatus] = useState<JobStatusResponse | null>(null);
  const [output, setOutput] = useState<OutputJson | null>(null);
  const [notFound, setNotFound] = useState(false);
  const trackedDoneRef = useRef(false);

  useEffect(() => {
    let cancelled = false;
    async function poll() {
      const res = await fetch(`/api/jobs/${jobId}`);
      if (cancelled) return;
      if (!res.ok) {
        setNotFound(true);
        return;
      }
      setStatus(await res.json());
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
  }, [jobId, status?.status]);

  useEffect(() => {
    if (status?.status !== 'done' || trackedDoneRef.current) return;
    trackedDoneRef.current = true;

    (async () => {
      const res = await fetch(`/api/jobs/${jobId}/output.json`);
      if (!res.ok) return;
      const data = (await res.json()) as OutputJson;
      setOutput(data);

      if (status.kind === 'youtube') {
        trackYtToolUsed({
          video_id: data.filename.replace(/\.md$/, ''),
          duration_sec: status.duration_sec ?? 0,
          cache_hit: false,
          cost_cents: status.cost_cents ?? 0,
        });
        return;
      }

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
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>{status?.kind === 'youtube' ? 'Processing your video' : 'Processing your recording'}</h1>
          <p className="lead">
            {status?.status === 'done'
              ? 'Done — your Markdown is ready below.'
              : 'This mirrors the real pipeline stages, but every step here is simulated.'}
          </p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap page-narrow">
          {status && !output && (
            <JobStepper
              stages={STAGES_BY_KIND[status.kind]}
              status={status.status}
              activeStage={status.stage}
              progress={status.progress}
            />
          )}

          {status?.status === 'failed' && (
            <p style={{ color: 'var(--err)', marginTop: 16 }}>Job failed — simulated failure for QA.</p>
          )}

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
            </div>
          )}
        </div>
      </div>
    </>
  );
}
