// Stateless mock job status. No server-side store: Cloud Run can run multiple instances / scale
// to zero between polls, so an in-memory Map would intermittently 404 a job created on a
// different instance. Instead the job id IS the state: a base64url-encoded token, and status is a
// pure function of elapsed time since the token's createdAtMs against a fixed stage-duration table.
//
// Only the private-upload path exists (Phase 2R): the free YouTube tool was dropped — no public,
// visitor-triggered processing endpoint exists anywhere (PLAN.md Phase 2 review #1, METRICS.md N10).

export type JobStage = 'A' | 'B' | 'C' | 'D';
export type JobState = 'queued' | 'processing' | 'done' | 'failed';

export interface JobToken {
  ref: string; // the picked filename (label only, no bytes stored)
  createdAtMs: number;
  fail?: boolean;
  meta?: { filename?: string; durationSec?: number };
}

export interface JobStatus {
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

// The upload path runs Stage A (ffmpeg probe/prepare) through Stage D (ARCHITECTURE §1).
const STAGE_MS: StageStep[] = [
  { stage: 'queued', ms: 1500 },
  { stage: 'A', ms: 2500 },
  { stage: 'B', ms: 5000 },
  { stage: 'C', ms: 2000 },
  { stage: 'D', ms: 1000 },
];

export function encodeJobToken(token: JobToken): string {
  return Buffer.from(JSON.stringify(token), 'utf-8').toString('base64url');
}

export function decodeJobToken(id: string): JobToken | null {
  try {
    const json = Buffer.from(id, 'base64url').toString('utf-8');
    const token = JSON.parse(json) as Partial<JobToken>;
    if (!token.ref || !token.createdAtMs) return null;
    return token as JobToken;
  } catch {
    return null;
  }
}

export function computeStatus(token: JobToken): JobStatus {
  const elapsed = Date.now() - token.createdAtMs;
  const total = STAGE_MS.reduce((sum, s) => sum + s.ms, 0);

  if (elapsed >= total) {
    if (token.fail) return { status: 'failed', progress: 100 };
    return {
      status: 'done',
      progress: 100,
      cost_cents: 38,
      duration_sec: token.meta?.durationSec ?? 2832,
      wall_clock_sec: Math.round(total / 1000),
    };
  }

  let acc = 0;
  for (const step of STAGE_MS) {
    if (elapsed < acc + step.ms) {
      const progress = Math.round((elapsed / total) * 100);
      return {
        status: step.stage === 'queued' ? 'queued' : 'processing',
        stage: step.stage === 'queued' ? undefined : step.stage,
        progress,
      };
    }
    acc += step.ms;
  }

  return { status: 'processing', progress: 99 };
}
