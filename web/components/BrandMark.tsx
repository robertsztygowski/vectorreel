// The Phase 2R wordmark: monospace "mdreel" with a blinking-cursor block after it, per the design
// comps. `md` is brand-blue, the rest ink, and the trailing block is the blue caret. Rendered inline
// (no SVG) so it inherits the header/footer sizing via the .brand class.
export function BrandMark() {
  return (
    <>
      <span style={{ color: 'var(--brand)' }}>md</span>
      <span>reel</span>
      <span
        aria-hidden="true"
        style={{
          display: 'inline-block',
          width: '7px',
          height: '15px',
          background: 'var(--brand)',
          marginLeft: '3px',
          transform: 'translateY(1px)',
        }}
      />
    </>
  );
}
