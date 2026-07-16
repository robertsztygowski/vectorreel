import { NextResponse } from 'next/server';
import { problem } from '@/lib/apiProblem';
import { computeStatus, decodeJobToken } from '@/lib/mockJobs';
import { resolveJobOutput } from '@/lib/jobOutput';
import { buildSampleDocument } from '@/lib/sampleOutput';

// Mirrors ARCHITECTURE §5 GET /jobs/{id}/output.json — the structured form of the document
// (tests/fixtures/contracts/output.schema.json), nothing else: the Markdown bytes are what
// GET /jobs/{id}/output.md is for.
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

  return NextResponse.json(buildSampleDocument(output.args));
}
