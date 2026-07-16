import { NextResponse } from 'next/server';
import { problem } from '@/lib/apiProblem';
import { encodeJobToken } from '@/lib/mockJobs';
import { listLibrary } from '@/lib/mockLibrary';

interface CreateJobBody {
  uploadId?: string;
  options?: { fail?: boolean; filename?: string; durationSec?: number };
}

// Mirrors ARCHITECTURE §5 GET /jobs — the authenticated panel's job list
// (tests/fixtures/contracts/job-list.schema.json). The panel's server component reads
// listLibrary() directly (no self-fetch); this route is the contract surface, validated by
// contracts.test.ts.
export async function GET() {
  const jobs = listLibrary().map((item) => ({
    jobId: item.jobId,
    status: item.status,
    progress: item.status === 'queued' ? 0 : item.status === 'processing' ? 50 : 100,
    ...(item.status === 'processing' ? { stage: 'B' as const } : {}),
    source: `${item.id}.mp4`,
    duration_sec: item.durationSec,
    created_at: item.processedAt,
    ...(item.status === 'done' ? { finished_at: item.processedAt } : {}),
  }));
  return NextResponse.json({ jobs });
}

// Mirrors ARCHITECTURE §5 POST /jobs. Practices the real error shape (RFC 7807 problem+json).
// Phase 2R: only the authenticated upload path exists — there is no public sourceUrl processing.
export async function POST(request: Request) {
  const body = (await request.json().catch(() => null)) as CreateJobBody | null;
  if (!body || typeof body !== 'object') {
    return problem(400, 'Invalid request body', 'Expected a JSON object.');
  }

  const { uploadId, options } = body;

  if (!uploadId) {
    return problem(400, 'Missing job source', 'Provide an uploadId from POST /uploads.');
  }

  const jobId = encodeJobToken({
    ref: uploadId,
    createdAtMs: Date.now(),
    fail: Boolean(options?.fail),
    meta: { filename: options?.filename, durationSec: options?.durationSec },
  });
  return NextResponse.json({ jobId }, { status: 202 });
}
