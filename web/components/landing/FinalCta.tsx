import Link from 'next/link';

export function FinalCta() {
  return (
    <section id="waitlist" className="section cta-final">
      <div className="wrap cta-box">
        <h2>Turn your video library into something your agents can read.</h2>
        <p className="lead">We&apos;re onboarding design partners now. Get 2 hours free on your own footage.</p>
        <div className="cta-row" style={{ justifyContent: 'center' }}>
          <Link className="btn btn-primary" href="/signup">
            Get early access
          </Link>
        </div>
        <p className="micro">No spam, unsubscribe anytime.</p>
      </div>
    </section>
  );
}
