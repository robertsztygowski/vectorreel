import { buildSampleOutput } from './sampleOutput';
import type { JobToken } from './mockJobs';

export interface JobOutput {
  raw: string;
  filename: string;
}

// Resolves a *finished* upload job token to its raw markdown + suggested filename. Returns the
// canonical sample output with the user's actual filename/duration filled in (never a CC BY corpus
// file — see sampleOutput.ts). The public YouTube path was dropped in Phase 2R.
export function resolveJobOutput(token: JobToken): JobOutput | null {
  const filename = token.meta?.filename ?? 'upload.mp4';
  const durationSec = token.meta?.durationSec ?? 2832;
  const raw = buildSampleOutput({
    sourceFilename: filename,
    durationSec,
    processedAt: new Date(token.createdAtMs).toISOString(),
  });
  return { raw, filename: `${filename.replace(/\.[^./]+$/, '')}.md` };
}
