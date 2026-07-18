import type { Metadata } from 'next';
import Link from 'next/link';
import { LegalLayout } from '@/components/LegalLayout/LegalLayout';
import { LEGAL_ENTITY, SUBPROCESSORS, legalPage } from '@/lib/legal';

const page = legalPage('subprocessors');

export const metadata: Metadata = { title: `${page.title} — mdreel`, description: page.description };

export default function SubprocessorsPage() {
  return (
    <LegalLayout
      title={page.title}
      lead="The third parties mdreel engages to deliver the Service, each processing personal data in the EU under a data-processing agreement. This list is part of our GDPR Article 28 commitments."
      toc={[
        { id: 'list', label: 'current subprocessors' },
        { id: 'umami', label: 'analytics (first-party)' },
        { id: 'changes', label: 'changes & objections' },
      ]}
    >
      <section id="list">
        <h2>Current subprocessors</h2>
        <p>
          The following subprocessors may process personal data contained in Customer Content or in
          account and billing data. Each is bound by a data-processing agreement with obligations
          equivalent to our own (Article 28(4) GDPR), and each processes within the European Economic
          Area.
        </p>
        <table>
          <thead>
            <tr>
              <th>Subprocessor</th>
              <th>Purpose</th>
              <th>Location</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {SUBPROCESSORS.map((s) => (
              <tr key={s.name}>
                <td>{s.name}</td>
                <td>{s.purpose}</td>
                <td>{s.location}</td>
                <td>{s.status}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section id="umami">
        <h2>Analytics is first-party — not a subprocessor</h2>
        <p>
          Our website analytics uses <strong>Umami</strong>, which we{' '}
          <strong>self-host on our own EU infrastructure</strong>. Because we run it ourselves,
          cookieless, no analytics data is shared with a third-party provider — so Umami is
          deliberately <strong>not</strong> a subprocessor. There is no US analytics vendor, tracking
          pixel or session-replay tool anywhere on our properties. See the{' '}
          <Link href="/legal/privacy">Privacy Policy</Link> for details.
        </p>
      </section>

      <section id="changes">
        <h2>Changes and your right to object</h2>
        <p>
          We will give at least <strong>30 days&apos;</strong> advance notice before a new
          subprocessor begins processing, by updating this page and — on request — by email. To be
          notified of changes, email{' '}
          <a href={`mailto:${LEGAL_ENTITY.email}?subject=Subprocessor%20change%20notifications`}>
            {LEGAL_ENTITY.email}
          </a>{' '}
          and ask to be added to the subprocessor-change list.
        </p>
        <p>
          If you object to a new subprocessor on reasonable data-protection grounds during the notice
          period, contact us at <a href={`mailto:${LEGAL_ENTITY.email}`}>{LEGAL_ENTITY.email}</a>. If
          we cannot resolve your objection, you may terminate the affected part of the Service, as set
          out in the <Link href="/legal/dpa">Data Processing Agreement</Link>.
        </p>
      </section>
    </LegalLayout>
  );
}
