import { NextResponse } from 'next/server';
import { problem } from '@/lib/apiProblem';
import { computeStatus, decodeJobToken } from '@/lib/mockJobs';
import { resolveJobOutput } from '@/lib/jobOutput';

// Mirrors ARCHITECTURE §5 GET /jobs/{id}/output.md. For a YouTube-matched job this returns the
// real fixture file's bytes verbatim, so a download here is byte-identical to the committed
// fixture in web/fixtures/corpus_md/.
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

  return new NextResponse(output.raw, {
    headers: {
      'Content-Type': 'text/markdown; charset=utf-8',
      'Content-Disposition': `inline; filename="${output.filename}"`,
    },
  });
}
