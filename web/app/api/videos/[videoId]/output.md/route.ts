import { NextResponse } from 'next/server';
import { problem } from '@/lib/apiProblem';
import { getLibraryMarkdown } from '@/lib/mockLibrary';

// Mock download for a processed library item (GET /jobs/{id}/output.md equivalent). Returns the
// committed corpus fixture bytes verbatim — a download here is byte-identical to the fixture in
// web/fixtures/corpus_md/.
export async function GET(_request: Request, { params }: { params: Promise<{ videoId: string }> }) {
  const { videoId } = await params;
  const output = getLibraryMarkdown(videoId);
  if (!output) return problem(404, 'Not found', 'No processed output for that video.');

  return new NextResponse(output.raw, {
    headers: {
      'Content-Type': 'text/markdown; charset=utf-8',
      'Content-Disposition': `attachment; filename="${output.filename}"`,
    },
  });
}
