import { getSessionId } from './attribution';

export const MDREEL_SESSION_HEADER = 'X-Mdreel-Session';

export function withMdreelSessionHeader(headers?: HeadersInit): Headers {
  const next = new Headers(headers);
  next.set(MDREEL_SESSION_HEADER, getSessionId());
  return next;
}
