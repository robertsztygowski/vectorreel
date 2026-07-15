import Link from 'next/link';

export function FinalCta() {
  return (
    <section id="waitlist" className="section">
      <div className="wrap cta-final">
        <h2>Your first hour of video is free.</h2>
        <p className="micro">no credit card · no lock-in</p>
        <Link className="btn btn-primary" href="/signup">
          start free — 1 hour
        </Link>
        <p className="micro micro-foot">plain Markdown out · EU-only · source deleted after processing</p>
      </div>
    </section>
  );
}
