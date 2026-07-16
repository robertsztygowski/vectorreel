import { NextResponse } from 'next/server';

// Mirrors ARCHITECTURE §5 POST /uploads. No file is ever actually stored — the uploadUrl is
// never PUT to; the client only reads the picked file's local .duration via a hidden <video>.
export async function POST() {
  const uploadId = crypto.randomUUID();
  return NextResponse.json(
    {
      uploadId,
      uploadUrl: `https://storage.invalid/mock-upload/${uploadId}`,
    },
    { status: 201 },
  );
}
