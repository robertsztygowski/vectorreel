import type { Metadata } from 'next';
import Link from 'next/link';
import { LegalLayout } from '@/components/LegalLayout/LegalLayout';
import { LEGAL_ENTITY, legalPage } from '@/lib/legal';

const page = legalPage('acceptable-use');

export const metadata: Metadata = { title: `${page.title} — mdreel`, description: page.description };

export default function AcceptableUsePage() {
  return (
    <LegalLayout
      title={page.title}
      lead="What you may and may not upload to or do with mdreel. This policy is incorporated into the Terms of Service; breaching it is a material breach of those Terms."
      toc={[
        { id: 'intro', label: 'scope' },
        { id: 'content-rights', label: '1. content & rights' },
        { id: 'illegal', label: '2. illegal content' },
        { id: 'privacy', label: '3. privacy & surveillance' },
        { id: 'ip', label: '4. intellectual property' },
        { id: 'platform', label: '5. platform integrity' },
        { id: 'sanctions', label: '6. sanctions & export' },
        { id: 'enforcement', label: '7. enforcement' },
        { id: 'reporting', label: '8. reporting abuse' },
      ]}
    >
      <section id="intro">
        <h2>Scope</h2>
        <p>
          This Acceptable Use Policy (<strong>&ldquo;AUP&rdquo;</strong>) applies to everyone who uses
          mdreel and to all content submitted to the Service. It is incorporated by reference into the{' '}
          <Link href="/legal/terms">Terms of Service</Link>. Capitalised terms have the meaning given
          there.
        </p>
      </section>

      <section id="content-rights">
        <h2>1. You must have the rights to your content</h2>
        <p>
          You warrant that all recordings and other content you submit have been lawfully created and
          that you hold every right, licence and consent needed to upload and process them, including
          the consent of individuals appearing in a recording where applicable law requires it. You
          must not upload content you are not authorised to process.
        </p>
      </section>

      <section id="illegal">
        <h2>2. No illegal or harmful content</h2>
        <p>You must not upload, submit or process content that:</p>
        <ul>
          <li>is unlawful under applicable law or the law of Poland;</li>
          <li>
            constitutes, depicts, promotes or facilitates child sexual abuse material, terrorism, or
            incitement to violence;
          </li>
          <li>is defamatory, harassing, threatening, or unlawfully discriminatory; or</li>
          <li>violates a court order, injunction or regulatory prohibition.</li>
        </ul>
      </section>

      <section id="privacy">
        <h2>3. No unlawful surveillance or biometric misuse</h2>
        <ul>
          <li>
            Do not upload recordings made without the knowledge or consent of identifiable individuals
            where the law requires such consent.
          </li>
          <li>Do not use the Service for stalking, unlawful monitoring, or other covert surveillance.</li>
          <li>
            Do not process special-category personal data (Article 9 GDPR) — including biometric or
            health data — without an appropriate lawful basis.
          </li>
          <li>
            Do not create non-consensual deepfakes or deceptive synthetic media of real people.
          </li>
        </ul>
      </section>

      <section id="ip">
        <h2>4. Respect intellectual property</h2>
        <ul>
          <li>Do not submit content that infringes copyright, trademark, database or other IP rights.</li>
          <li>Do not reverse engineer the Service or its underlying models.</li>
          <li>Do not use the Service or its outputs to train a competing AI model or service.</li>
        </ul>
      </section>

      <section id="platform">
        <h2>5. Protect platform integrity</h2>
        <ul>
          <li>Do not circumvent rate limits, quotas, access controls or security measures.</li>
          <li>Do not scrape, crawl or bulk-extract beyond the documented API limits.</li>
          <li>Do not use the Service to probe, scan or attack any system.</li>
          <li>Do not resell, sublicense or redistribute the Service without our written permission.</li>
        </ul>
      </section>

      <section id="sanctions">
        <h2>6. Sanctions and export control</h2>
        <p>
          You must not use the Service in violation of applicable sanctions or export-control laws, and
          you confirm you are not a person or entity subject to EU or UN sanctions.
        </p>
      </section>

      <section id="enforcement">
        <h2>7. Enforcement</h2>
        <p>
          A breach of this AUP is a material breach of the Terms of Service. Depending on severity we
          may:
        </p>
        <table>
          <thead>
            <tr>
              <th>Severity</th>
              <th>Response</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>Serious — illegal content, security threat, sanctions breach</td>
              <td>Immediate suspension without prior notice; termination at our discretion</td>
            </tr>
            <tr>
              <td>Material — IP infringement, systematic unauthorised use</td>
              <td>Written notice and an opportunity to cure; termination if not cured</td>
            </tr>
            <tr>
              <td>Technical — minor or accidental breach</td>
              <td>Written notice and an opportunity to cure; suspension if not cured</td>
            </tr>
          </tbody>
        </table>
        <p>
          You agree to indemnify mdreel against third-party claims arising from your breach of this
          AUP, including claims relating to content you submitted.
        </p>
      </section>

      <section id="reporting">
        <h2>8. Reporting abuse</h2>
        <p>
          To report content or use that violates this AUP, email{' '}
          <a href={`mailto:${LEGAL_ENTITY.email}?subject=Abuse%20report`}>{LEGAL_ENTITY.email}</a> with
          enough detail for us to locate and assess it. We review reports and take appropriate action.
        </p>
      </section>
    </LegalLayout>
  );
}
