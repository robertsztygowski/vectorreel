import { NextResponse } from 'next/server';
import { problem } from '@/lib/apiProblem';
import { findCorpusEntry } from '@/lib/corpus';
import { encodeJobToken } from '@/lib/mockJobs';
import { extractYouTubeVideoId } from '@/lib/youtube';

interface CreateJobBody {
  uploadId?: string;
  sourceUrl?: string;
  options?: { fail?: boolean; filename?: string; durationSec?: number };
}

// Mirrors ARCHITECTURE §5 POST /jobs. Practices the real error shape (RFC 7807 problem+json).
export async function POST(request: Request) {
  const body = (await request.json().catch(() => null)) as CreateJobBody | null;
  if (!body || typeof body !== 'object') {
    return problem(400, 'Invalid request body', 'Expected a JSON object.');
  }

  const { uploadId, sourceUrl, options } = body;

  if (uploadId && sourceUrl) {
    return problem(400, 'Ambiguous job source', 'Provide exactly one of uploadId or sourceUrl, not both.');
  }

  if (sourceUrl) {
    const videoId = extractYouTubeVideoId(sourceUrl);
    const entry = videoId ? findCorpusEntry(videoId) : undefined;
    if (!entry) {
      return problem(
        422,
        'No mocked result for this video',
        'This phase only has mocked output for the curated corpus videos in the gallery.',
      );
    }
    const jobId = encodeJobToken({
      kind: 'youtube',
      ref: videoId!,
      createdAtMs: Date.now(),
      fail: Boolean(options?.fail),
    });
    return NextResponse.json({ jobId }, { status: 202 });
  }

  if (uploadId) {
    const jobId = encodeJobToken({
      kind: 'upload',
      ref: uploadId,
      createdAtMs: Date.now(),
      fail: Boolean(options?.fail),
      meta: { filename: options?.filename, durationSec: options?.durationSec },
    });
    return NextResponse.json({ jobId }, { status: 202 });
  }

  return problem(400, 'Missing job source', 'Provide either uploadId or sourceUrl.');
}
