'use client';

import {
  Suspense,
  useEffect,
  useRef,
  useState,
  type ChangeEvent,
  type DragEvent,
} from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { trackUploadStarted } from '@/lib/events';
import { TRIAL_CREDIT_HOURS } from '@/lib/pricing';
import {
  DEFAULT_DURATION_SEC,
  buildJobOptions,
  getUploadPutTarget,
  isVideoFile,
  type RetentionDays,
  type UploadCreatedResponse,
  type UploadQuality,
} from '@/lib/uploadFlow';

type UploadPhase = 'idle' | 'creating' | 'uploading' | 'uploaded' | 'error';
type JobPhase = 'idle' | 'waitingForUpload' | 'creating' | 'error';

function formatMinutes(totalSeconds: number): string {
  const m = Math.floor(totalSeconds / 60);
  const s = Math.round(totalSeconds % 60);
  return `${m}m ${s}s`;
}

function formatClock(totalSeconds: number): string {
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = Math.round(totalSeconds % 60);
  return [hours, minutes, seconds].map((part) => String(part).padStart(2, '0')).join(':');
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === 'AbortError';
}

function uploadStatusText(phase: UploadPhase, progress: number, jobPhase: JobPhase): string {
  if (jobPhase === 'waitingForUpload') return 'start queued — job will be created when upload finishes';
  if (jobPhase === 'creating') return 'creating job…';
  if (phase === 'creating') return 'reserving upload…';
  if (phase === 'uploading') return `uploading ${progress}%`;
  if (phase === 'uploaded') return 'upload complete — fill options, then start processing';
  if (phase === 'error') return 'upload failed — retry or remove the file';
  return 'ready';
}

// useSearchParams() forces a CSR bailout during prerender; the Suspense wrapper is required
// for `next build` — same pattern as checkout/page.tsx.
export default function UploadPage() {
  return (
    <Suspense fallback={null}>
      <UploadInner />
    </Suspense>
  );
}

function UploadInner() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [file, setFile] = useState<File | null>(null);
  const [durationSec, setDurationSec] = useState<number | null>(null);
  const [simulateFail, setSimulateFail] = useState(false);
  const [quality, setQuality] = useState<UploadQuality>('standard');
  const [retentionDays, setRetentionDays] = useState<RetentionDays>(0);
  const [webhookUrl, setWebhookUrl] = useState('');
  const [uploadPhase, setUploadPhase] = useState<UploadPhase>('idle');
  const [jobPhase, setJobPhase] = useState<JobPhase>('idle');
  const [uploadProgress, setUploadProgress] = useState(0);
  const [dragActive, setDragActive] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const videoRef = useRef<HTMLVideoElement | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const xhrRef = useRef<XMLHttpRequest | null>(null);
  const createUploadAbortRef = useRef<AbortController | null>(null);
  const objectUrlRef = useRef<string | null>(null);
  const uploadTokenRef = useRef(0);
  const uploadIdRef = useRef<string | null>(null);
  const fileRef = useRef<File | null>(null);
  const durationSecRef = useRef<number | null>(null);
  const uploadPhaseRef = useRef<UploadPhase>('idle');
  const submitInFlightRef = useRef(false);
  const autoStartJobRef = useRef(false);
  const optionsRef = useRef({ quality, retentionDays, webhookUrl, simulateFail });

  const usage = searchParams.get('usage') ?? 'trial';
  const forcedState = searchParams.get('state');
  const selectedMock = !file && forcedState === 'selected';

  useEffect(() => {
    optionsRef.current = { quality, retentionDays, webhookUrl, simulateFail };
  }, [quality, retentionDays, webhookUrl, simulateFail]);

  useEffect(() => {
    return () => {
      createUploadAbortRef.current?.abort();
      xhrRef.current?.abort();
      if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
    };
  }, []);

  function commitUploadPhase(phase: UploadPhase) {
    uploadPhaseRef.current = phase;
    setUploadPhase(phase);
  }

  function resetUploadTransport() {
    createUploadAbortRef.current?.abort();
    xhrRef.current?.abort();
    createUploadAbortRef.current = null;
    xhrRef.current = null;
  }

  function setVideoPreview(picked: File) {
    if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
    const nextUrl = URL.createObjectURL(picked);
    objectUrlRef.current = nextUrl;
    if (videoRef.current) videoRef.current.src = nextUrl;
  }

  function rejectFile(message: string) {
    setError(message);
    setDragActive(false);
    if (fileInputRef.current) fileInputRef.current.value = '';
  }

  function putFile(upload: UploadCreatedResponse, picked: File, contentType: string, token: number) {
    return new Promise<void>((resolve, reject) => {
      const xhr = new XMLHttpRequest();
      xhrRef.current = xhr;
      xhr.open('PUT', getUploadPutTarget(upload));
      xhr.setRequestHeader('Content-Type', contentType);
      xhr.upload.onprogress = (event) => {
        if (token !== uploadTokenRef.current || !event.lengthComputable) return;
        setUploadProgress(Math.max(1, Math.min(99, Math.round((event.loaded / event.total) * 100))));
      };
      xhr.onload = () => {
        if (xhr.status === 200 || xhr.status === 204) {
          resolve();
        } else {
          reject(new Error(`upload bytes failed: ${xhr.status}`));
        }
      };
      xhr.onerror = () => reject(new Error('upload bytes failed: network error'));
      xhr.onabort = () => reject(new DOMException('upload aborted', 'AbortError'));
      xhr.send(picked);
    });
  }

  async function createJob() {
    const uploadId = uploadIdRef.current;
    const picked = fileRef.current;
    if (!uploadId || !picked) {
      submitInFlightRef.current = false;
      setJobPhase('idle');
      return;
    }

    setJobPhase('creating');
    setError(null);
    const duration = Math.round(durationSecRef.current ?? DEFAULT_DURATION_SEC);
    const options = optionsRef.current;

    try {
      const jobRes = await fetch('/api/v1/jobs', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          uploadId,
          options: buildJobOptions({
            quality: options.quality,
            retentionDays: options.retentionDays,
            webhookUrl: options.webhookUrl,
            fail: options.simulateFail,
            filename: picked.name,
            durationSec: duration,
          }),
        }),
      });
      if (jobRes.status !== 202) throw new Error(`create job failed: ${jobRes.status}`);
      const { jobId } = (await jobRes.json()) as { jobId: string };
      router.push(`/app/jobs/${jobId}`);
    } catch (err) {
      submitInFlightRef.current = false;
      autoStartJobRef.current = false;
      setJobPhase('error');
      setError(err instanceof Error ? err.message : 'create job failed');
    }
  }

  async function beginUpload(picked: File, startJobWhenDone = false) {
    resetUploadTransport();
    const token = uploadTokenRef.current + 1;
    uploadTokenRef.current = token;
    fileRef.current = picked;
    uploadIdRef.current = null;
    durationSecRef.current = null;
    autoStartJobRef.current = startJobWhenDone;
    submitInFlightRef.current = startJobWhenDone;

    setFile(picked);
    setDurationSec(null);
    setUploadProgress(0);
    setJobPhase(startJobWhenDone ? 'waitingForUpload' : 'idle');
    setError(null);
    commitUploadPhase('creating');
    setVideoPreview(picked);

    const contentType = picked.type || 'application/octet-stream';
    trackUploadStarted({ duration_sec: DEFAULT_DURATION_SEC });

    try {
      const controller = new AbortController();
      createUploadAbortRef.current = controller;
      const uploadRes = await fetch('/api/v1/uploads', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ contentType }),
        signal: controller.signal,
      });
      if (!uploadRes.ok) throw new Error(`create upload failed: ${uploadRes.status}`);
      const upload = (await uploadRes.json()) as UploadCreatedResponse;
      if (token !== uploadTokenRef.current) return;

      uploadIdRef.current = upload.uploadId;
      commitUploadPhase('uploading');
      await putFile(upload, picked, contentType, token);
      if (token !== uploadTokenRef.current) return;

      setUploadProgress(100);
      commitUploadPhase('uploaded');
      if (autoStartJobRef.current) void createJob();
    } catch (err) {
      if (token !== uploadTokenRef.current || isAbortError(err)) return;
      submitInFlightRef.current = false;
      autoStartJobRef.current = false;
      setJobPhase('idle');
      commitUploadPhase('error');
      setError(err instanceof Error ? err.message : 'upload failed');
    }
  }

  function handlePickedFile(picked: File | null) {
    if (!picked) return;
    if (!isVideoFile(picked)) {
      rejectFile('Choose a video file (mp4, mov, webm, or another video/* type).');
      return;
    }
    void beginUpload(picked);
  }

  function handleFile(e: ChangeEvent<HTMLInputElement>) {
    handlePickedFile(e.target.files?.[0] ?? null);
  }

  function handleLoadedMetadata() {
    const video = videoRef.current;
    if (!video) return;
    durationSecRef.current = video.duration;
    setDurationSec(video.duration);
  }

  function handleDrop(e: DragEvent<HTMLButtonElement>) {
    e.preventDefault();
    setDragActive(false);
    handlePickedFile(e.dataTransfer.files?.[0] ?? null);
  }

  function handleDragOver(e: DragEvent<HTMLButtonElement>) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'copy';
    setDragActive(true);
  }

  function handleDragLeave(e: DragEvent<HTMLButtonElement>) {
    if (!e.currentTarget.contains(e.relatedTarget as Node | null)) setDragActive(false);
  }

  function removeFile() {
    resetUploadTransport();
    uploadTokenRef.current += 1;
    uploadIdRef.current = null;
    fileRef.current = null;
    durationSecRef.current = null;
    submitInFlightRef.current = false;
    autoStartJobRef.current = false;
    if (objectUrlRef.current) {
      URL.revokeObjectURL(objectUrlRef.current);
      objectUrlRef.current = null;
    }
    if (fileInputRef.current) fileInputRef.current.value = '';
    setFile(null);
    setDurationSec(null);
    setUploadProgress(0);
    setJobPhase('idle');
    setError(null);
    commitUploadPhase('idle');
  }

  function handleStart() {
    if (submitInFlightRef.current || selectedMock) return;
    if (!fileRef.current) return;

    submitInFlightRef.current = true;
    setError(null);

    if (uploadPhaseRef.current === 'uploaded') {
      void createJob();
      return;
    }

    if (uploadPhaseRef.current === 'error') {
      void beginUpload(fileRef.current, true);
      return;
    }

    autoStartJobRef.current = true;
    setJobPhase('waitingForUpload');
  }

  const displayedName = file?.name ?? (selectedMock ? 'demo-billing.mp4' : '');
  const displayedSize = file ? `${(file.size / (1024 * 1024 * 1024)).toFixed(1)} GB` : '1.2 GB';
  const displayedDuration = durationSec ?? DEFAULT_DURATION_SEC;
  const canStart = Boolean(file) && jobPhase !== 'waitingForUpload' && jobPhase !== 'creating';
  const startLabel =
    jobPhase === 'waitingForUpload'
      ? 'waiting for upload…'
      : jobPhase === 'creating'
        ? 'creating job…'
        : uploadPhase === 'error'
          ? 'retry upload'
          : 'start processing';

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
          <div className="upload-stack">
            {error ? (
              <div className="error-panel upload-error" role="alert">
                <p className="headline">upload problem</p>
                <p>{error}</p>
                <button className="remove" type="button" onClick={() => setError(null)}>
                  dismiss
                </button>
              </div>
            ) : null}

            {!file && !selectedMock ? (
              <button
                className={`drop-zone${dragActive ? ' is-dragover' : ''}`}
                type="button"
                onClick={() => fileInputRef.current?.click()}
                onDragEnter={handleDragOver}
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
              >
                <span className="cta">
                  drop a video file or <b>browse</b>
                </span>
                <span className="note">upload starts immediately · processed in the EU</span>
              </button>
            ) : (
              <div className="file-row">
                <div className="file-row-head">
                  <span>
                    <span className="file">{displayedName}</span>
                    <span className="meta">
                      {' '}
                      · {displayedSize} · detected duration {formatMinutes(displayedDuration)}
                    </span>
                  </span>
                  <button className="remove" type="button" onClick={removeFile}>
                    remove
                  </button>
                </div>
                <p className="count">
                  will count {formatClock(displayedDuration)} against your {usage === 'plan' ? 'plan' : 'trial credit'}
                </p>
                <div className="upload-progress" role="status" aria-live="polite">
                  <div className="upload-progress-head">
                    <span>{selectedMock ? 'selected demo state' : uploadStatusText(uploadPhase, uploadProgress, jobPhase)}</span>
                    {!selectedMock ? <span>{uploadProgress}%</span> : null}
                  </div>
                  <div className="upload-progress-bar" aria-hidden="true">
                    <span style={{ width: `${selectedMock ? 100 : uploadProgress}%` }} />
                  </div>
                </div>
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

            <p className="eyebrow upload-section-label">## job options — sent as POST /jobs</p>
            <div className="options-table">
              <span className="k">language_hint:</span>
              <span>
                auto <span className="default">(default)</span>
              </span>

              <span className="k">quality:</span>
              <span>
                <span className="seg" role="group" aria-label="quality">
                  {(['standard', 'high'] as const).map((value) => (
                    <button
                      key={value}
                      className={quality === value ? 'on' : undefined}
                      type="button"
                      aria-pressed={quality === value}
                      onClick={() => setQuality(value)}
                    >
                      {value}
                    </button>
                  ))}
                </span>
              </span>

              <span className="k">retention_days:</span>
              <span>
                <span className="seg" role="group" aria-label="retention days">
                  <button
                    className={retentionDays === 0 ? 'on' : undefined}
                    type="button"
                    aria-pressed={retentionDays === 0}
                    onClick={() => setRetentionDays(0)}
                  >
                    0 · delete after processing
                  </button>
                  <button
                    className={retentionDays === 30 ? 'on' : undefined}
                    type="button"
                    aria-pressed={retentionDays === 30}
                    onClick={() => setRetentionDays(30)}
                  >
                    30 · keep source 30 days
                  </button>
                </span>
                <span className="hint">0 is the default — the source video is erased once your document exists</span>
              </span>

              <span className="k">webhook_url:</span>
              <span>
                <input
                  type="url"
                  placeholder="https://your.app/hooks/mdreel"
                  value={webhookUrl}
                  onChange={(e) => setWebhookUrl(e.target.value)}
                />
                <span className="hint">optional · job.completed events signed HMAC-SHA256 in X-Mdreel-Signature</span>
              </span>
            </div>

            <label className="micro upload-qa">
              <input type="checkbox" checked={simulateFail} onChange={(e) => setSimulateFail(e.target.checked)} />
              simulate failure (qa)
            </label>

            <div className="upload-actions">
              <button className="btn btn-primary btn-sm" type="button" onClick={handleStart} disabled={!canStart}>
                {startLabel}
              </button>
              <Link className="btn btn-ghost btn-sm" href="/app">
                back to library
              </Link>
            </div>

            <p className="micro upload-footnote">
              prefer the API? POST /uploads then POST /jobs — <Link href="/docs">do this via API →</Link>
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
