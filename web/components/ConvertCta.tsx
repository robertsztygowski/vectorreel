'use client';

import type { CSSProperties, ReactNode } from 'react';
import Link from 'next/link';
import { trackConvertClick } from '@/lib/umami';

// The consume→convert hop: a CTA that takes a visitor from browsing a public collection to
// starting their own repository. First-party event only (lib/umami — CLAUDE.md rule 10).
export function ConvertCta({
  from,
  videoId,
  className,
  style,
  children,
}: {
  from: string;
  videoId?: string;
  className?: string;
  style?: CSSProperties;
  children: ReactNode;
}) {
  return (
    <Link
      href="/signup"
      className={className}
      style={style}
      onClick={() => trackConvertClick(from, videoId)}
    >
      {children}
    </Link>
  );
}
