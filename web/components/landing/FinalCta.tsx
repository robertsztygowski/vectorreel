import Link from 'next/link';

export function FinalCta() {
  return (
    <section id="waitlist" className="section">
      <div className="wrap cta-final">
        <h2>See it working, then build your own.</h2>
        <p className="micro">public collections are real output — every line verifiable against the original video</p>
        <Link className="btn btn-primary" href="/gallery">
          explore the public collections
        </Link>
        <p className="micro micro-foot">
          or <Link href="/signup">start your own repository</Link> — first hour free · no credit card · EU-only · source
          deleted after processing
        </p>
      </div>
    </section>
  );
}
