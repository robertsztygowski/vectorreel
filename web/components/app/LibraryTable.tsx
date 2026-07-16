'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import type { LibraryItem } from '@/lib/mockLibrary';
import { trackOutputDownloaded } from '@/lib/events';

const DELETED_KEY = 'mdreel_deleted_videos';

function formatDuration(totalSeconds: number): string {
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = Math.round(totalSeconds % 60);
  if (hours > 0) return `${hours}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  return `${minutes}:${String(seconds).padStart(2, '0')}`;
}

function formatDate(iso: string): string {
  return new Date(iso).toISOString().slice(0, 10);
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

  useEffect(() => {
    setDeleted(readDeleted());
  }, []);

  // GDPR erasure goes through DELETE /jobs/{id} (ARCHITECTURE §5). The mock route has no store,
  // so a client-side set is still what keeps the item gone across navigations this phase — but it
  // is only written after the route confirms, so the erasure contract is exercised for real.
  async function remove(id: string, jobId: string) {
    const res = await fetch(`/api/jobs/${jobId}`, { method: 'DELETE' });
    if (!res.ok && res.status !== 404) return;
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
        <p>No documents yet.</p>
        <Link className="btn btn-primary" href="/app/upload">
          upload a video
        </Link>
      </div>
    );
  }

  return (
    <div className="lib-table" role="table" aria-label="Processed videos">
      <div className="lib-row lib-head" role="row">
        <span role="columnheader">document</span>
        <span role="columnheader">type</span>
        <span role="columnheader">length</span>
        <span role="columnheader">processed</span>
        <span role="columnheader">status</span>
        <span role="columnheader" className="lib-actions-head">
          actions
        </span>
      </div>
      {visible.map((item) => (
        <div className="lib-row" role="row" key={item.id}>
          <span role="cell" className="lib-title">
            {item.status === 'done' ? <Link href={`/app/videos/${item.id}`}>{item.title}</Link> : <span>{item.title}</span>}
            <small>
              {item.id}.md · {item.retentionLine}
            </small>
          </span>
          <span role="cell" className="lib-cell">
            {categoryLabels[item.category]}
          </span>
          <span role="cell" className="lib-cell">
            {formatDuration(item.durationSec)}
          </span>
          <span role="cell" className="lib-cell">
            {item.status === 'processing' ? 'now' : item.status === 'queued' || item.status === 'failed' ? '—' : formatDate(item.processedAt)}
          </span>
          <span role="cell">
            <span className={`badge badge-${item.status}`}>{item.status}</span>
          </span>
          <span role="cell" className="lib-actions">
            {item.status === 'done' && (
              <>
                <Link className="act-view" href={`/app/videos/${item.id}`}>
                  view
                </Link>
                <button className="act-view" type="button" onClick={() => download(item.id)}>
                  download
                </button>
                <button
                  type="button"
                  className="act-delete"
                  onClick={() => {
                    if (window.confirm(`Delete "${item.title}"? This erases its output (mock).`)) void remove(item.id, item.jobId);
                    else router.refresh();
                  }}
                >
                  delete
                </button>
              </>
            )}
            {(item.status === 'queued' || item.status === 'processing') && (
              <button
                type="button"
                className="act-delete"
                onClick={() => {
                  if (window.confirm(`Cancel "${item.title}"?`)) void remove(item.id, item.jobId);
                  else router.refresh();
                }}
              >
                cancel
              </button>
            )}
            {item.status === 'failed' && (
              <>
                <Link className="act-view" href={`/app/jobs/${item.jobId}?state=failed`}>
                  details
                </Link>
                <button
                  type="button"
                  className="act-delete"
                  onClick={() => {
                    if (window.confirm(`Delete "${item.title}"? This erases its output (mock).`)) void remove(item.id, item.jobId);
                    else router.refresh();
                  }}
                >
                  delete
                </button>
              </>
            )}
          </span>
        </div>
      ))}
    </div>
  );
}
