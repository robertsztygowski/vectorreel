export function EuBand() {
  return (
    <section id="eu" className="section">
      <div className="wrap eu-band">
        <div className="eu-copy">
          <p className="eyebrow">EU &amp; GDPR</p>
          <h2>Feed your videos to your agents without leaving the EU.</h2>
          <p className="lead">
            Internal recordings contain voices, faces and customer data. mdreel is built EU-first so your DPO doesn&apos;t
            have to fight your AI roadmap — with region pinning, retention by design and no training on your data, ever.
          </p>
          <ul className="ticks">
            <li>EU-regional processing &amp; storage</li>
            <li>Source deleted after processing (configurable)</li>
            <li>No training on customer data — contractually</li>
            <li>DPA, subprocessor list &amp; data-flow diagram published</li>
          </ul>
        </div>
        <div className="eu-note">
          <p className="honesty-title">Our honesty rule</p>
          <p>
            Today&apos;s stack is <strong>EU data residency on Google Cloud</strong> (EU regions, no-training terms) —
            not full EU sovereignty. EU-owned infrastructure and a self-hosted edition are on the public roadmap.
            We&apos;d rather tell you that than have your DPO catch it.
          </p>
        </div>
      </div>
    </section>
  );
}
