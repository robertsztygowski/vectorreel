import { getVideoMeta } from './corpus';
import { buildSampleOutput } from './sampleOutput';
import type { JobToken } from './mockJobs';

export interface JobOutput {
  raw: string;
  filename: string;
}

// Resolves a *finished* job token to its raw markdown + suggested filename. YouTube jobs return
// the real matched corpus fixture verbatim; upload jobs return the canonical sample output with
// the user's actual filename/duration filled in (never a CC BY corpus file — see sampleOutput.ts).
export function resolveJobOutput(token: JobToken): JobOutput | null {
  if (token.kind === 'youtube') {
    const meta = getVideoMeta(token.ref);
    if (!meta) return null;
    return { raw: meta.raw, filename: `${token.ref}.md` };
  }

  const filename = token.meta?.filename ?? 'upload.mp4';
  const durationSec = token.meta?.durationSec ?? 2832;
  const raw = buildSampleOutput({
    sourceFilename: filename,
    durationSec,
    processedAt: new Date(token.createdAtMs).toISOString(),
  });
  return { raw, filename: `${filename.replace(/\.[^./]+$/, '')}.md` };
}
