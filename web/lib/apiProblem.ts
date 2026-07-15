import { NextResponse } from 'next/server';

// RFC 7807 problem+json — the error shape ARCHITECTURE §5 specifies for the real API.
export function problem(status: number, title: string, detail: string) {
  return NextResponse.json(
    { type: 'about:blank', title, status, detail },
    { status, headers: { 'Content-Type': 'application/problem+json' } },
  );
}
