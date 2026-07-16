'use client';

import { useRef, useState, type ChangeEvent } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { trackUploadStarted } from '@/lib/events';
import { TRIAL_CREDIT_HOURS } from '@/lib/pricing';

function formatMinutes(totalSeconds: number): string {
  const m = Math.floor(totalSeconds / 60);
  const s = Math.round(totalSeconds % 60);
  return `${m}m ${s}s`;
}

export default function UploadPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [file, setFile] = useState<File | null>(null);
  const [durationSec, setDurationSec] = useState<number | null>(null);
  const [simulateFail, setSimulateFail] = useState(false);
  const [starting, setStarting] = useState(false);
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const usage = searchParams.get('usage') ?? 'trial';
  const forcedState = searchParams.get('state');
  const selectedMock = !file && forcedState === 'selected';

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
    if (!file && forcedState !== 'selected') return;
    setStarting(true);
    try {
      trackUploadStarted({ duration_sec: Math.round(durationSec ?? (47 * 60 + 12)) });

      const uploadRes = await fetch('/api/uploads', { method: 'POST' });
      const { uploadId } = (await uploadRes.json()) as { uploadId: string };

      const jobRes = await fetch('/api/jobs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          uploadId,
          options: {
            fail: simulateFail,
            filename: file?.name ?? 'demo-billing.mp4',
            durationSec: Math.round(durationSec ?? (47 * 60 + 12)),
          },
        }),
      });
      const { jobId } = (await jobRes.json()) as { jobId: string };
      router.push(`/app/jobs/${jobId}`);
    } finally {
      setStarting(false);
    }
  }

  return (
    <div className="app-page">
      <div className="wrap app-page-narrow">
        <h1 className="app-h1">Process a recording</h1>
        <p className="app-h1-sub">
          {usage === 'at-cap'
            ? 'plan cap reached · pro'
            : usage === 'plan'
              ? 'counts against your plan · 7.6 h left this cycle · pro'
              : `counts against your trial credit · ${TRIAL_CREDIT_HOURS.toFixed(1)} h left`}
        </p>

        {usage === 'at-cap' ? (
          <div className="cap-panel">
            <p className="stat">25 / 25 h — processing paused.</p>
            <p className="body">
              No overage will be billed. Your documents stay exactly where they are, and this form unlocks the moment
              your plan grows or the cycle resets.
            </p>
            <Link className="btn btn-primary btn-sm" href="/pricing">
              upgrade to continue
            </Link>
            <p className="coda">hard caps are the product — never surprise bills</p>
          </div>
        ) : (
          <>
            {!file && !selectedMock ? (
              <button className="drop-zone" type="button" onClick={() => fileInputRef.current?.click()}>
                <span className="cta">
                  drop a video file or <b>browse</b>
                </span>
                <span className="note">processed in the EU · source deleted after processing (default)</span>
              </button>
            ) : (
              <div className="file-row">
                <div className="file-row-head">
                  <span>
                    <span className="file">{file?.name ?? 'demo-billing.mp4'}</span>
                    <span className="meta">
                      {' '}
                      · {file ? `${(file.size / (1024 * 1024 * 1024)).toFixed(1)} GB` : '1.2 GB'} · detected duration{' '}
                      {formatMinutes(durationSec ?? (47 * 60 + 12))}
                    </span>
                  </span>
                  <button className="remove" type="button" onClick={() => setFile(null)}>
                    remove
                  </button>
                </div>
                <p className="count">
                  will count {formatMinutes(durationSec ?? (47 * 60 + 12)).replace('m ', ':').replace('s', '')} against your{' '}
                  {usage === 'plan' ? 'plan' : 'trial credit'}
                </p>
              </div>
            )}

            <input
              id="file"
              ref={fileInputRef}
              type="file"
              accept="video/*"
              onChange={handleFile}
              style={{ display: 'none' }}
            />
            <video ref={videoRef} onLoadedMetadata={handleLoadedMetadata} style={{ display: 'none' }} muted />

            <div className="aside-note">
              <p className="eyebrow">&gt; why is there no url box?</p>
              <p>
                mdreel processes private recordings — files you own. Public YouTube video is deliberately not a
                product input; the <Link href="/gallery">gallery</Link> is our only YouTube surface.
              </p>
            </div>

            <p className="eyebrow" style={{ marginTop: 36, marginBottom: 12 }}>
              ## job options — sent as POST /jobs
            </p>
            <div className="options-table">
              <span className="k">language_hint:</span>
              <span>
                auto <span className="default">(default)</span>
              </span>

              <span className="k">quality:</span>
              <span>
                <span className="seg">
                  <button className="on" type="button">
                    standard
                  </button>
                  <button type="button">high</button>
                </span>
              </span>

              <span className="k">retention_days:</span>
              <span>
                <span className="seg">
                  <button className="on" type="button">
                    0 · delete after processing
                  </button>
                  <button type="button">30 · keep source 30 days</button>
                </span>
                <span className="hint">0 is the default — the source video is erased once your document exists</span>
              </span>

              <span className="k">webhook_url:</span>
              <span>
                <input type="url" placeholder="https://your.app/hooks/mdreel" />
                <span className="hint">
                  optional · job.completed events signed HMAC-SHA256 in X-Mdreel-Signature
                </span>
              </span>
            </div>

            <label className="micro" style={{ display: 'flex', gap: 8, alignItems: 'center', margin: '20px 0 24px' }}>
              <input type="checkbox" checked={simulateFail} onChange={(e) => setSimulateFail(e.target.checked)} />
              simulate failure (qa)
            </label>

            <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
              <button className="btn btn-primary btn-sm" type="button" onClick={handleStart} disabled={(!file && !selectedMock) || starting}>
                {starting ? 'starting…' : 'start processing'}
              </button>
              <Link className="btn btn-ghost btn-sm" href="/app">
                back to library
              </Link>
            </div>

            <p className="micro" style={{ marginTop: 48 }}>
              prefer the API? POST /uploads then POST /jobs — <Link href="/docs">do this via API →</Link>
            </p>
          </>
        )}
      </div>
    </div>
  );
}
