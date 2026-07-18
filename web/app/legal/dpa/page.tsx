import type { Metadata } from 'next';
import Link from 'next/link';
import { LegalLayout } from '@/components/LegalLayout/LegalLayout';
import { LEGAL_ENTITY, legalPage } from '@/lib/legal';

const page = legalPage('dpa');

export const metadata: Metadata = { title: `${page.title} — mdreel`, description: page.description };

export default function DpaPage() {
  return (
    <LegalLayout
      title={page.title}
      lead="The GDPR Article 28 terms under which mdreel processes personal data contained in the content you upload. This DPA is incorporated by reference into the Terms of Service and you accept it by using the Service."
      toc={[
        { id: 'roles', label: '1. roles' },
        { id: 'scope', label: '2. scope & instructions' },
        { id: 'confidentiality', label: '3. confidentiality' },
        { id: 'security', label: '4. security (Art. 32)' },
        { id: 'subprocessors', label: '5. subprocessors' },
        { id: 'assistance', label: '6. assistance' },
        { id: 'breach', label: '7. breach notification' },
        { id: 'deletion', label: '8. return & deletion' },
        { id: 'audits', label: '9. audits' },
        { id: 'transfers', label: '10. no non-EEA transfer' },
        { id: 'annex-a', label: 'annex A — processing' },
        { id: 'annex-b', label: 'annex B — TOMs' },
      ]}
    >
      <section id="roles">
        <h2>1. Roles of the parties</h2>
        <p>
          This Data Processing Agreement (<strong>&ldquo;DPA&rdquo;</strong>) applies where{' '}
          {LEGAL_ENTITY.name} (<strong>&ldquo;mdreel&rdquo;</strong>, the <strong>processor</strong>)
          processes personal data contained in Customer Content on behalf of the Customer (the{' '}
          <strong>controller</strong>) in providing the Service. It forms part of, and is governed by,
          the <Link href="/legal/terms">Terms of Service</Link>. Where this DPA and the Terms conflict
          on the processing of personal data, this DPA prevails.
        </p>
      </section>

      <section id="scope">
        <h2>2. Scope and documented instructions (Art. 28(3)(a))</h2>
        <p>
          mdreel processes personal data only on the Customer&apos;s documented instructions,
          including as set out in the Terms, this DPA, the product documentation, and the
          configuration choices the Customer makes in the Service. The subject-matter, duration,
          nature, purpose, data types and categories of data subject are described in{' '}
          <a href="#annex-a">Annex A</a>. mdreel will inform the Customer if, in its opinion, an
          instruction infringes the GDPR or other applicable data-protection law, and is not obliged
          to follow an instruction that would do so.
        </p>
      </section>

      <section id="confidentiality">
        <h2>3. Confidentiality (Art. 28(3)(b))</h2>
        <p>
          mdreel ensures that persons authorised to process the personal data are bound by an
          appropriate duty of confidentiality and process the data only as instructed.
        </p>
      </section>

      <section id="security">
        <h2>4. Security of processing (Art. 28(3)(c), Art. 32)</h2>
        <p>
          mdreel implements appropriate technical and organisational measures to ensure a level of
          security appropriate to the risk, taking account of the state of the art and the nature of
          the processing. A summary of those measures is in <a href="#annex-b">Annex B</a>.
        </p>
      </section>

      <section id="subprocessors">
        <h2>5. Subprocessors (Art. 28(2), (3)(d), (4))</h2>
        <p>
          The Customer gives general authorisation for mdreel to engage subprocessors to provide the
          Service. The current subprocessors are listed, with their role and location, on the{' '}
          <Link href="/legal/subprocessors">Subprocessors</Link> page. mdreel imposes on each
          subprocessor data-protection obligations equivalent to those in this DPA (flow-down) and
          remains fully liable to the Customer for a subprocessor&apos;s performance.
        </p>
        <p>
          mdreel will give at least <strong>30 days&apos;</strong> notice of any intended addition or
          replacement of a subprocessor (via the Subprocessors page and, on request, by email). The
          Customer may object on reasonable data-protection grounds within that period; if the
          objection cannot be resolved, the Customer may terminate the affected part of the Service.
        </p>
      </section>

      <section id="assistance">
        <h2>6. Assistance with data-subject rights and compliance (Art. 28(3)(e), (f))</h2>
        <p>
          Taking into account the nature of the processing, mdreel assists the Customer by appropriate
          technical and organisational measures, insofar as possible, to respond to requests from data
          subjects exercising their rights under Chapter III GDPR (Arts. 15–22). mdreel also assists
          the Customer in ensuring compliance with its obligations under Arts. 32–36 (security, breach
          notification, data-protection impact assessments and prior consultation), taking into
          account the information available to mdreel.
        </p>
      </section>

      <section id="breach">
        <h2>7. Personal data breach (Art. 33)</h2>
        <p>
          mdreel notifies the Customer without undue delay after becoming aware of a personal data
          breach affecting Customer Content, and provides the information the Customer reasonably needs
          to meet its own notification obligations.
        </p>
      </section>

      <section id="deletion">
        <h2>8. Return and deletion (Art. 28(3)(g))</h2>
        <p>
          At the Customer&apos;s choice, mdreel deletes or returns all personal data in Customer
          Content after the end of the provision of the Service, and deletes existing copies, unless
          storage is required by EU or Member-State law. By design, uploaded source recordings are
          deleted after processing, and outputs are deleted when the Customer deletes them or closes
          the account. The Service also provides a deletion endpoint for individual jobs.
        </p>
      </section>

      <section id="audits">
        <h2>9. Information and audits (Art. 28(3)(h))</h2>
        <p>
          mdreel makes available to the Customer the information necessary to demonstrate compliance
          with Art. 28, and allows for and contributes to audits, including inspections, conducted by
          the Customer or a mandated auditor, subject to reasonable notice, confidentiality, and
          frequency limits, and primarily by providing documentation of its measures.
        </p>
      </section>

      <section id="transfers">
        <h2>10. No transfers outside the EEA</h2>
        <p>
          By design, all processing under this DPA takes place within the European Economic Area. No
          personal data is transferred to a third country as part of the Service, so no Chapter V
          transfer mechanism (such as Standard Contractual Clauses) is required. If mdreel ever needs
          to process outside the EEA, it will notify the Customer and put an appropriate Chapter V
          safeguard in place beforehand.
        </p>
      </section>

      <section id="annex-a">
        <h2>Annex A — Details of processing</h2>
        <div className="legal-annex">
          <p>
            <strong>Subject-matter:</strong> processing of personal data contained in Customer Content
            to provide the mdreel video/audio-to-Markdown Service.
          </p>
          <p>
            <strong>Duration:</strong> for the term of the Terms of Service and until deletion or
            return of the data as described in section 8.
          </p>
          <p>
            <strong>Nature and purpose:</strong> upload, storage, automated AI analysis of video and
            audio, generation of timestamped Markdown and structured output, delivery to the Customer,
            and deletion.
          </p>
          <p>
            <strong>Categories of personal data:</strong> any personal data present in the recordings
            the Customer uploads — for example the voices, images and statements of speakers and other
            individuals appearing in or referenced by the recording, and text shown on screen. mdreel
            does not require or solicit special-category data; the Customer must not upload such data
            without an appropriate lawful basis.
          </p>
          <p>
            <strong>Categories of data subjects:</strong> individuals appearing, speaking, mentioned or
            shown in Customer Content — which may include the Customer&apos;s staff, its customers, and
            third parties, as determined by the Customer.
          </p>
          <p>
            <strong>Controller / processor:</strong> the Customer is the controller; mdreel is the
            processor.
          </p>
        </div>
      </section>

      <section id="annex-b">
        <h2>Annex B — Technical and organisational measures (Art. 32)</h2>
        <div className="legal-annex">
          <ul>
            <li>
              <strong>EU-region processing:</strong> all compute, storage and AI processing pinned to
              EU regions.
            </li>
            <li>
              <strong>Encryption:</strong> data encrypted in transit (TLS) and at rest.
            </li>
            <li>
              <strong>Access control:</strong> least-privilege access, no long-lived exported
              credentials, and centrally managed secrets.
            </li>
            <li>
              <strong>Data minimisation &amp; retention:</strong> source recordings deleted after
              processing by default; per-job deletion; storage-lifecycle backstop.
            </li>
            <li>
              <strong>No model training:</strong> Customer Content is not used to train AI models; the
              AI provider operates under no-training terms.
            </li>
            <li>
              <strong>Integrity &amp; auditability:</strong> signed webhooks, audit logging of data
              access and deletion, and scale-to-zero infrastructure.
            </li>
            <li>
              <strong>Resilience:</strong> managed, backed-up EU database and object storage.
            </li>
          </ul>
        </div>
      </section>
    </LegalLayout>
  );
}
