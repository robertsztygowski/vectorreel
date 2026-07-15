'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import type { LibraryItem } from '@/lib/mockLibrary';
import { trackOutputDownloaded } from '@/lib/events';

const DELETED_KEY = 'mdreel_deleted_videos';

function formatDuration(totalSeconds: number): string {
  const m = Math.round(totalSeconds / 60);
  return `${m} min`;
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' });
}

function readDeleted(): Set<string> {
  if (typeof window === 'undefined') return new Set();
  try {
    return new Set(JSON.parse(window.localStorage.getItem(DELETED_KEY) ?? '[]') as string[]);
  } catch {
    return new Set();
  }
}

export function LibraryTable({ items, categoryLabels }: { items: LibraryItem[]; categoryLabels: Record<string, string> }) {
  const router = useRouter();
  const [deleted, setDeleted] = useState<Set<string>>(new Set());

  // Mock deletion has no backend: persist a client-side set so a deleted item stays gone across
  // navigations this phase. DELETE /jobs/{id} (GDPR erasure) is wired for real in Phase 4.
  useEffect(() => {
    setDeleted(readDeleted());
  }, []);

  function remove(id: string) {
    const next = new Set(deleted);
    next.add(id);
    setDeleted(next);
    window.localStorage.setItem(DELETED_KEY, JSON.stringify([...next]));
  }

  function download(id: string) {
    trackOutputDownloaded({ job_id: id });
    window.location.href = `/api/videos/${id}/output.md`;
  }

  const visible = items.filter((i) => !deleted.has(i.id));

  if (visible.length === 0) {
    return (
      <div className="empty-state">
        <p>No processed videos yet.</p>
        <Link className="btn btn-primary" href="/app/upload">
          Process a video
        </Link>
      </div>
    );
  }

  return (
    <div className="lib-table" role="table" aria-label="Processed videos">
      <div className="lib-row lib-head" role="row">
        <span role="columnheader">Video</span>
        <span role="columnheader">Type</span>
        <span role="columnheader">Length</span>
        <span role="columnheader">Processed</span>
        <span role="columnheader">Status</span>
        <span role="columnheader" className="lib-actions-head">
          Actions
        </span>
      </div>
      {visible.map((item) => (
        <div className="lib-row" role="row" key={item.id}>
          <span role="cell" className="lib-title">
            <Link href={`/app/videos/${item.id}`}>{item.title}</Link>
            <small>{item.channel}</small>
          </span>
          <span role="cell">{categoryLabels[item.category]}</span>
          <span role="cell">{formatDuration(item.durationSec)}</span>
          <span role="cell">{formatDate(item.processedAt)}</span>
          <span role="cell">
            <span className="badge badge-done">Done</span>
          </span>
          <span role="cell" className="lib-actions">
            <Link href={`/app/videos/${item.id}`}>View</Link>
            <button type="button" onClick={() => download(item.id)}>
              Download
            </button>
            <button
              type="button"
              className="lib-delete"
              onClick={() => {
                if (window.confirm(`Delete "${item.title}"? This erases its output (mock).`)) remove(item.id);
                else router.refresh();
              }}
            >
              Delete
            </button>
          </span>
        </div>
      ))}
    </div>
  );
}
