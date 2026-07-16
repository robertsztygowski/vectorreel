import { NextResponse } from 'next/server';
import { problem } from '@/lib/apiProblem';
import { computeStatus, decodeJobToken } from '@/lib/mockJobs';

// Mirrors ARCHITECTURE §5 GET /jobs/{id}.
export async function GET(_request: Request, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const token = decodeJobToken(id);
  if (!token) return problem(404, 'Job not found', 'Unknown or malformed job id.');
  return NextResponse.json(computeStatus(token));
}

// Mirrors ARCHITECTURE §5 DELETE /jobs/{id} — GDPR erasure, 204 on success. The mock has no
// store to cascade; the panel keeps its client-side deleted-set as the stand-in for the jobs
// table, but it only updates it after this route confirms — the erasure *contract* is exercised.
export async function DELETE(_request: Request, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const token = decodeJobToken(id);
  if (!token) return problem(404, 'Job not found', 'Unknown or malformed job id.');
  return new NextResponse(null, { status: 204 });
}
