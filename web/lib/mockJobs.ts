// Stateless mock job status. No server-side store: Cloud Run can run multiple instances / scale
// to zero between polls, so an in-memory Map would intermittently 404 a job created on a
// different instance. Instead the job id IS the state: a base64url-encoded token, and status is a
// pure function of elapsed time since the token's createdAtMs against fixed stage-duration tables.

export type JobKind = 'youtube' | 'upload';
export type JobStage = 'A' | 'B' | 'C' | 'D';
export type JobState = 'queued' | 'processing' | 'done' | 'failed';

export interface JobToken {
  kind: JobKind;
  ref: string; // youtube: video_id; upload: the picked filename (label only, no bytes stored)
  createdAtMs: number;
  fail?: boolean;
  meta?: { filename?: string; durationSec?: number };
}

export interface JobStatus {
  kind: JobKind;
  status: JobState;
  stage?: JobStage;
  progress: number; // 0-100
  cost_cents?: number;
  duration_sec?: number;
  wall_clock_sec?: number;
}

interface StageStep {
  stage: JobStage | 'queued';
  ms: number;
}

// Upload path runs Stage A (ffmpeg probe/prepare); the YouTube path never does — no local bytes,
// no ffmpeg, no per-segment static detection (ARCHITECTURE §1's asymmetric-paths table).
const STAGE_MS_UPLOAD: StageStep[] = [
  { stage: 'queued', ms: 1500 },
  { stage: 'A', ms: 2500 },
  { stage: 'B', ms: 5000 },
  { stage: 'C', ms: 2000 },
  { stage: 'D', ms: 1000 },
];

const STAGE_MS_YOUTUBE: StageStep[] = [
  { stage: 'queued', ms: 1500 },
  { stage: 'B', ms: 4500 },
  { stage: 'C', ms: 2000 },
  { stage: 'D', ms: 1000 },
];

function stagesFor(kind: JobKind): StageStep[] {
  return kind === 'youtube' ? STAGE_MS_YOUTUBE : STAGE_MS_UPLOAD;
}

export function encodeJobToken(token: JobToken): string {
  return Buffer.from(JSON.stringify(token), 'utf-8').toString('base64url');
}

export function decodeJobToken(id: string): JobToken | null {
  try {
    const json = Buffer.from(id, 'base64url').toString('utf-8');
    const token = JSON.parse(json) as Partial<JobToken>;
    if (!token.kind || !token.ref || !token.createdAtMs) return null;
    return token as JobToken;
  } catch {
    return null;
  }
}

export function computeStatus(token: JobToken): JobStatus {
  const elapsed = Date.now() - token.createdAtMs;
  const stages = stagesFor(token.kind);
  const total = stages.reduce((sum, s) => sum + s.ms, 0);

  if (elapsed >= total) {
    if (token.fail) return { kind: token.kind, status: 'failed', progress: 100 };
    return {
      kind: token.kind,
      status: 'done',
      progress: 100,
      cost_cents: token.kind === 'youtube' ? 4 : 38,
      duration_sec: token.kind === 'youtube' ? 600 : token.meta?.durationSec ?? 2832,
      wall_clock_sec: Math.round(total / 1000),
    };
  }

  let acc = 0;
  for (const step of stages) {
    if (elapsed < acc + step.ms) {
      const progress = Math.round((elapsed / total) * 100);
      return {
        kind: token.kind,
        status: step.stage === 'queued' ? 'queued' : 'processing',
        stage: step.stage === 'queued' ? undefined : step.stage,
        progress,
      };
    }
    acc += step.ms;
  }

  return { kind: token.kind, status: 'processing', progress: 99 };
}
