import { NextResponse, type NextRequest } from 'next/server';

// Same-origin proxy so the ASP.NET Core Identity auth cookie is first-party (no cross-origin
// cookie / CORS credentials dance). API_ORIGIN points at the Cloud Run API in deployed
// environments; unset locally means the mock app/api routes serve instead. Only /api/v1/* is
// rewritten, so the web's own mock routes (/api/jobs, /api/uploads, /api/videos) are untouched.
//
// This runs at request time (unlike next.config `rewrites()`, which is baked at build time and so
// cannot read a runtime env var), so the stats/api origin is a single runtime env change.
export function middleware(request: NextRequest) {
  // Indirect read so Next does not statically inline the value at build time.
  const key = 'API_ORIGIN';
  const apiOrigin = process.env[key]?.replace(/\/+$/, '');
  if (!apiOrigin) return NextResponse.next();

  const target = new URL(`${request.nextUrl.pathname}${request.nextUrl.search}`, apiOrigin);
  return NextResponse.rewrite(target);
}

export const config = {
  matcher: '/api/v1/:path*',
};
