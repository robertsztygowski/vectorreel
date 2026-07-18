import type { Metadata } from 'next';
import Link from 'next/link';
import { LegalLayout } from '@/components/LegalLayout/LegalLayout';
import { LEGAL_ENTITY, legalPage } from '@/lib/legal';

const page = legalPage('privacy');

export const metadata: Metadata = { title: `${page.title} — mdreel`, description: page.description };

export default function PrivacyPage() {
  return (
    <LegalLayout
      title={page.title}
      lead="How mdreel handles personal data under the GDPR — what we collect, why, on what legal basis, for how long, who processes it, your rights, and our cookieless, EU-only analytics."
      toc={[
        { id: 'controller', label: '1. who we are' },
        { id: 'roles', label: '2. controller vs processor' },
        { id: 'data', label: '3. data & purposes' },
        { id: 'content', label: '4. uploaded content' },
        { id: 'analytics', label: '5. analytics & cookies' },
        { id: 'recipients', label: '6. recipients' },
        { id: 'transfers', label: '7. EU processing' },
        { id: 'retention', label: '8. retention' },
        { id: 'rights', label: '9. your rights' },
        { id: 'security', label: '10. security' },
        { id: 'changes', label: '11. changes' },
      ]}
    >
      <section id="controller">
        <h2>1. Who we are</h2>
        <p>
          The controller for the personal data described here is <strong>{LEGAL_ENTITY.name}</strong>,{' '}
          {LEGAL_ENTITY.address}, VAT {LEGAL_ENTITY.vat}. For any privacy or data-protection matter,
          contact us at <a href={`mailto:${LEGAL_ENTITY.email}`}>{LEGAL_ENTITY.email}</a>. We are a
          small business and are not required to appoint a Data Protection Officer; the address above
          reaches the person responsible for data protection.
        </p>
      </section>

      <section id="roles">
        <h2>2. Controller vs. processor</h2>
        <p>
          We act in two different roles, and this Policy is about the first:
        </p>
        <ul>
          <li>
            <strong>As controller</strong> — for your account, billing and analytics data, where we
            decide the purposes and means of processing. This Policy governs that data.
          </li>
          <li>
            <strong>As processor</strong> — for the recordings and outputs you upload and generate
            (&ldquo;Customer Content&rdquo;), which we process only on your instructions. There, your
            organisation is the controller and processing is governed by our{' '}
            <Link href="/legal/dpa">Data Processing Agreement</Link>, not this Policy (see section 4).
          </li>
        </ul>
      </section>

      <section id="data">
        <h2>3. What we process and why</h2>
        <table>
          <thead>
            <tr>
              <th>Data</th>
              <th>Purpose</th>
              <th>Legal basis (GDPR Art. 6)</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>Account data — email, password (hashed), your organisation, volume answers you give</td>
              <td>Create and operate your account; provide the Service</td>
              <td>6(1)(b) — performance of our contract with you</td>
            </tr>
            <tr>
              <td>Billing data — plan, VAT/NIP, payment and invoice records</td>
              <td>Take payment, issue invoices, meet tax and accounting duties</td>
              <td>6(1)(b) contract; 6(1)(c) legal obligation</td>
            </tr>
            <tr>
              <td>Usage &amp; product events — actions in the app, job metadata</td>
              <td>Operate, secure and understand the Service</td>
              <td>6(1)(f) — our legitimate interest in running and improving the Service</td>
            </tr>
            <tr>
              <td>Support &amp; communications — messages you send us</td>
              <td>Respond to requests, handle complaints</td>
              <td>6(1)(b) contract; 6(1)(f) legitimate interest</td>
            </tr>
          </tbody>
        </table>
        <p>
          Providing account and billing data is necessary to enter into and perform the contract;
          without it we cannot provide the Service.
        </p>
      </section>

      <section id="content">
        <h2>4. Recordings you upload</h2>
        <p>
          The video and audio you upload may contain personal data of people who appear or speak in it,
          and of people mentioned or shown on screen. For that content we act as your{' '}
          <strong>processor</strong>: we process it only to deliver the Service to you, on your
          instructions, under the <Link href="/legal/dpa">DPA</Link>. Your organisation, as
          controller, is responsible for having a lawful basis and the necessary notices or consents
          for that content.
        </p>
        <p>
          Where individuals in a recording are not our own customers, we generally cannot identify or
          contact them, so providing information to each of them directly would involve
          disproportionate effort. In line with Article 14(5)(b) GDPR, we make this Policy publicly
          available as the means of informing them, and we support the controller in honouring their
          rights.
        </p>
      </section>

      <section id="analytics">
        <h2>5. Analytics and cookies</h2>
        <p>
          We use <strong>Umami</strong>, a privacy-friendly analytics tool that we{' '}
          <strong>self-host on our own EU infrastructure</strong>. It is{' '}
          <strong>cookieless</strong> and does not track you across sites or build advertising
          profiles; it produces aggregate usage statistics only. Because it sets no cookies and
          performs no cross-site tracking, no cookie-consent banner is required.
        </p>
        <p>
          We do <strong>not</strong> use Google Analytics or any other US-based analytics, tracking
          pixels, session-replay or heat-mapping tools. Nothing on our site sends your browsing data
          to a third party outside the EU.
        </p>
      </section>

      <section id="recipients">
        <h2>6. Who receives your data</h2>
        <p>
          We share personal data only with the subprocessors needed to run the Service — our cloud
          infrastructure and AI provider, our payment provider, and our email provider — each bound by
          a data-processing agreement and each processing in the EU. The current list, with each
          provider&apos;s role and location, is on our{' '}
          <Link href="/legal/subprocessors">Subprocessors</Link> page. We do not sell personal data.
        </p>
      </section>

      <section id="transfers">
        <h2>7. EU processing — no international transfers</h2>
        <p>
          All personal data is processed exclusively within the European Economic Area (EEA). We do
          not transfer personal data to third countries outside the EEA as part of the Service, so no
          Chapter V transfer mechanism is required. If this ever changes, we will update this Policy
          and put an appropriate safeguard in place before any such transfer.
        </p>
        <p className="legal-note">
          Honest note: our infrastructure runs on Google Cloud in EU regions. This is EU data
          residency, not full EU sovereignty — Google is a US-headquartered company. We say so plainly
          and publish our sovereignty roadmap rather than overclaim.
        </p>
      </section>

      <section id="retention">
        <h2>8. How long we keep data</h2>
        <ul>
          <li>
            <strong>Uploaded source recordings</strong> are deleted after processing by default. You
            may choose a short configurable retention window per job; a storage lifecycle rule acts as
            a backstop.
          </li>
          <li>
            <strong>Generated outputs</strong> are retained while your account is active so you can
            access them, and are deleted when you delete them or close your account.
          </li>
          <li>
            <strong>Account data</strong> is kept for the life of your account.
          </li>
          <li>
            <strong>Billing and invoice records</strong> are kept as long as tax and accounting law
            requires.
          </li>
        </ul>
      </section>

      <section id="rights">
        <h2>9. Your rights</h2>
        <p>Under the GDPR you have the right to:</p>
        <ul>
          <li>access your data (Art. 15) and receive a copy;</li>
          <li>rectify inaccurate data (Art. 16);</li>
          <li>erase data (Art. 17) — the app also offers direct deletion of jobs and outputs;</li>
          <li>restrict or object to processing (Arts. 18, 21), including processing based on our legitimate interests;</li>
          <li>data portability (Art. 20) — our outputs are portable Markdown by design;</li>
          <li>withdraw any consent you have given, without affecting prior processing.</li>
        </ul>
        <p>
          To exercise a right, email <a href={`mailto:${LEGAL_ENTITY.email}`}>{LEGAL_ENTITY.email}</a>.
          We do not carry out automated decision-making that produces legal or similarly significant
          effects on you.
        </p>
        <p>
          You also have the right to lodge a complaint with the Polish supervisory authority:{' '}
          <strong>Prezes Urzędu Ochrony Danych Osobowych (UODO)</strong>, ul. Stawki 2, 00-193
          Warszawa, <a href="https://uodo.gov.pl" rel="noopener noreferrer">uodo.gov.pl</a>.
        </p>
      </section>

      <section id="security">
        <h2>10. Security</h2>
        <p>
          We apply appropriate technical and organisational measures (Art. 32 GDPR): EU-region
          processing, encryption in transit and at rest, least-privilege access, secret management,
          and audit logging of data access and deletion. Our security measures are summarised in the{' '}
          <Link href="/legal/dpa">DPA</Link>.
        </p>
      </section>

      <section id="changes">
        <h2>11. Changes to this Policy</h2>
        <p>
          We may update this Policy; the effective date and version at the top reflect the current
          text. For material changes affecting you as a customer we will give reasonable notice.
        </p>
      </section>
    </LegalLayout>
  );
}
