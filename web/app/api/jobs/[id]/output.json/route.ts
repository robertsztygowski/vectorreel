import { NextResponse } from 'next/server';
import { problem } from '@/lib/apiProblem';
import { computeStatus, decodeJobToken } from '@/lib/mockJobs';
import { resolveJobOutput } from '@/lib/jobOutput';
import { parseMarkdown } from '@/lib/corpus';

// Mirrors ARCHITECTURE §5 GET /jobs/{id}/output.json — the structured blocks, parsed server-side
// so client components never need to import lib/corpus.ts's fs-using runtime.
export async function GET(_request: Request, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const token = decodeJobToken(id);
  if (!token) return problem(404, 'Job not found', 'Unknown or malformed job id.');

  const status = computeStatus(token);
  if (status.status !== 'done') {
    return problem(409, 'Job not finished', `Job is currently "${status.status}", not "done".`);
  }

  const output = resolveJobOutput(token);
  if (!output) return problem(404, 'Job not found', 'Unknown or malformed job id.');

  const parsed = parseMarkdown(output.raw);
  return NextResponse.json({ ...parsed, raw: output.raw, filename: output.filename });
}
