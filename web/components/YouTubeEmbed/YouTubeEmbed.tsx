// Privacy-enhanced (youtube-nocookie.com) iframe embed of the video we're attributing — content,
// not tracking. This is what ARCHITECTURE §1's public path exists for: distribution via the
// gallery, never a bytes download (CLAUDE.md rule 8).
export function YouTubeEmbed({ videoId, title }: { videoId: string; title: string }) {
  return (
    <div
      style={{
        position: 'relative',
        paddingBottom: '56.25%',
        height: 0,
        borderRadius: 'var(--radius)',
        overflow: 'hidden',
        background: '#000',
      }}
    >
      <iframe
        src={`https://www.youtube-nocookie.com/embed/${videoId}`}
        title={title}
        style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', border: 0 }}
        allow="accelerometer; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
        allowFullScreen
      />
    </div>
  );
}
