'use client';

import { useRouter } from 'next/navigation';

const DELETED_KEY = 'mdreel_deleted_videos';

// Mock GDPR erasure (DELETE /jobs/{id} is wired for real in Phase 4). Marks the item deleted in a
// client-side set the library reads, then returns to the library.
export function DeleteVideoButton({ videoId, title }: { videoId: string; title: string }) {
  const router = useRouter();

  function remove() {
    if (!window.confirm(`Delete "${title}"? This erases its output (mock).`)) return;
    let current: string[] = [];
    try {
      current = JSON.parse(window.localStorage.getItem(DELETED_KEY) ?? '[]') as string[];
    } catch {
      current = [];
    }
    if (!current.includes(videoId)) current.push(videoId);
    window.localStorage.setItem(DELETED_KEY, JSON.stringify(current));
    router.push('/app');
  }

  return (
    <button type="button" className="btn btn-ghost" onClick={remove}>
      Delete
    </button>
  );
}
