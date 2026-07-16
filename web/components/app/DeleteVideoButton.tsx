'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';

const DELETED_KEY = 'mdreel_deleted_videos';

// GDPR erasure via DELETE /jobs/{id} (ARCHITECTURE §5). The mock route has no store to cascade,
// so the library's client-side deleted-set is still what hides the item this phase — but it is
// only written after the route returns 204, so the erasure contract is exercised for real.
export function DeleteVideoButton({ videoId, jobId, title }: { videoId: string; jobId: string; title: string }) {
  const router = useRouter();
  const [busy, setBusy] = useState(false);

  async function remove() {
    if (!window.confirm(`Delete "${title}"? This erases its output (mock).`)) return;
    setBusy(true);
    try {
      const res = await fetch(`/api/jobs/${jobId}`, { method: 'DELETE' });
      if (!res.ok && res.status !== 404) return;
      let current: string[] = [];
      try {
        current = JSON.parse(window.localStorage.getItem(DELETED_KEY) ?? '[]') as string[];
      } catch {
        current = [];
      }
      if (!current.includes(videoId)) current.push(videoId);
      window.localStorage.setItem(DELETED_KEY, JSON.stringify(current));
      router.push('/app');
    } finally {
      setBusy(false);
    }
  }

  return (
    <button type="button" className="btn btn-ghost btn-sm" onClick={remove} disabled={busy}>
      {busy ? 'deleting…' : 'delete'}
    </button>
  );
}
