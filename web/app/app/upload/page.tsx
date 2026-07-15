'use client';

import { useRef, useState, type ChangeEvent } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { trackUploadStarted } from '@/lib/events';
import { TRIAL_CREDIT_HOURS } from '@/lib/pricing';

function formatMinutes(totalSeconds: number): string {
  const m = Math.floor(totalSeconds / 60);
  const s = Math.round(totalSeconds % 60);
  return `${m}m ${s}s`;
}

export default function UploadPage() {
  const router = useRouter();
  const [file, setFile] = useState<File | null>(null);
  const [durationSec, setDurationSec] = useState<number | null>(null);
  const [simulateFail, setSimulateFail] = useState(false);
  const [starting, setStarting] = useState(false);
  const videoRef = useRef<HTMLVideoElement | null>(null);

  function handleFile(e: ChangeEvent<HTMLInputElement>) {
    const picked = e.target.files?.[0] ?? null;
    setFile(picked);
    setDurationSec(null);
    if (picked && videoRef.current) {
      videoRef.current.src = URL.createObjectURL(picked);
    }
  }

  function handleLoadedMetadata() {
    const video = videoRef.current;
    if (!video) return;
    setDurationSec(video.duration);
    if (video.src) URL.revokeObjectURL(video.src);
  }

  async function handleStart() {
    if (!file) return;
    setStarting(true);
    try {
      trackUploadStarted({ duration_sec: Math.round(durationSec ?? 0) });

      const uploadRes = await fetch('/api/uploads', { method: 'POST' });
      const { uploadId } = (await uploadRes.json()) as { uploadId: string };

      const jobRes = await fetch('/api/jobs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          uploadId,
          options: { fail: simulateFail, filename: file.name, durationSec: Math.round(durationSec ?? 0) },
        }),
      });
      const { jobId } = (await jobRes.json()) as { jobId: string };
      router.push(`/app/jobs/${jobId}`);
    } finally {
      setStarting(false);
    }
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>Process a recording</h1>
          <p className="lead">
            Uses your {TRIAL_CREDIT_HOURS}-hour trial credit. Your file stays on your device this phase — nothing is
            actually uploaded (no product backend yet).
          </p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap page-narrow">
          <div className="field">
            <label htmlFor="file">Video file</label>
            <input id="file" type="file" accept="video/*" onChange={handleFile} />
            {durationSec !== null && <span className="hint">Detected duration: {formatMinutes(durationSec)}</span>}
          </div>
          {/* local metadata probe only, never played */}
          <video ref={videoRef} onLoadedMetadata={handleLoadedMetadata} style={{ display: 'none' }} muted />

          <label
            style={{
              display: 'flex',
              gap: 8,
              alignItems: 'center',
              fontSize: 13.5,
              color: 'var(--ink-faint)',
              margin: '4px 0 24px',
            }}
          >
            <input type="checkbox" checked={simulateFail} onChange={(e) => setSimulateFail(e.target.checked)} />
            Simulate failure (QA)
          </label>

          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
            <button className="btn btn-primary" type="button" onClick={handleStart} disabled={!file || starting}>
              {starting ? 'Starting…' : 'Start processing'}
            </button>
            <Link className="btn btn-ghost" href="/app">
              Back to library
            </Link>
          </div>
        </div>
      </div>
    </>
  );
}
