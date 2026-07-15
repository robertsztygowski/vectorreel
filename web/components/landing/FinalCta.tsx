import Link from 'next/link';

export function FinalCta() {
  return (
    <section id="waitlist" className="section cta-final">
      <div className="wrap cta-box">
        <h2>Turn your video library into something your agents can cite.</h2>
        <p className="lead">Start with 1 hour of processing free, on your own footage. No credit card, no lock-in.</p>
        <div className="cta-row" style={{ justifyContent: 'center' }}>
          <Link className="btn btn-primary" href="/signup">
            Start free — 1 hour
          </Link>
        </div>
        <p className="micro">Plain Markdown out · EU-only · source deleted after processing.</p>
      </div>
    </section>
  );
}
