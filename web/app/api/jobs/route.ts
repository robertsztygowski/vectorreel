import { NextResponse } from 'next/server';
import { problem } from '@/lib/apiProblem';
import { encodeJobToken } from '@/lib/mockJobs';

interface CreateJobBody {
  uploadId?: string;
  options?: { fail?: boolean; filename?: string; durationSec?: number };
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
