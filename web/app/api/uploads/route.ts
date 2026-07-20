import { NextResponse } from 'next/server';

// Mirrors ARCHITECTURE §5 POST /uploads. No file is ever actually stored; the companion PUT
// accepts bytes so local mock flows can exercise the same API-storage branch as production tests.
export async function POST() {
  const uploadId = crypto.randomUUID();
  return NextResponse.json(
    {
      uploadId,
      uploadUrl: `http://mock.local/api/uploads?uploadId=${uploadId}`,
      storage: 'api',
    },
    { status: 201 },
  );
}

export async function PUT() {
  return new Response(null, { status: 204 });
}
