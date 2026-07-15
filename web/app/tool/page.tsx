'use client';

import { useState, type FormEvent } from 'react';
import { useRouter } from 'next/navigation';

const CORPUS_EXAMPLES = [
  'https://www.youtube.com/watch?v=5si4zkAngpA',
  'https://www.youtube.com/watch?v=JvbBFwlqxeI',
  'https://www.youtube.com/watch?v=KL7WBjAuTMg',
  'https://www.youtube.com/watch?v=gRFaow12xo0',
  'https://www.youtube.com/watch?v=rAl-9HwD858',
];

export default function ToolPage() {
  const router = useRouter();
  const [url, setUrl] = useState('');
  const [simulateFail, setSimulateFail] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      const res = await fetch('/api/jobs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sourceUrl: url, options: { fail: simulateFail } }),
      });
      if (!res.ok) {
        const problem = (await res.json()) as { detail?: string };
        setError(problem.detail ?? 'Something went wrong.');
        return;
      }
      const data = (await res.json()) as { jobId: string };
      router.push(`/jobs/${data.jobId}`);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <>
      <div className="page-head">
        <div className="wrap page-narrow">
          <h1>Free YouTube tool</h1>
          <p className="lead">Paste a public YouTube URL — see real Markdown in 60s. No signup, zero trust required.</p>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap page-narrow">
          <form onSubmit={handleSubmit} style={{ display: 'flex', gap: 10, flexWrap: 'wrap', marginBottom: 12 }}>
            <div className="field" style={{ flex: '1 1 320px', marginBottom: 0 }}>
              <label htmlFor="yt-url">YouTube URL</label>
              <input
                id="yt-url"
                type="text"
                placeholder="https://www.youtube.com/watch?v=..."
                value={url}
                onChange={(e) => setUrl(e.target.value)}
                required
              />
            </div>
            <button className="btn btn-primary" type="submit" disabled={submitting} style={{ alignSelf: 'flex-end', height: 46 }}>
              {submitting ? 'Starting…' : 'Process video'}
            </button>
          </form>

          <label
            style={{
              display: 'flex',
              gap: 8,
              alignItems: 'center',
              fontSize: 13.5,
              color: 'var(--ink-faint)',
              marginBottom: 28,
            }}
          >
            <input type="checkbox" checked={simulateFail} onChange={(e) => setSimulateFail(e.target.checked)} />
            Simulate failure (QA)
          </label>

          {error && (
            <div className="eu-note">
              <p className="honesty-title">No mocked result for that video</p>
              <p>{error}</p>
              <p>Try one of the 5 curated corpus videos instead:</p>
              <ul>
                {CORPUS_EXAMPLES.map((u) => (
                  <li key={u}>
                    <a href={u} target="_blank" rel="noopener noreferrer">
                      {u}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
