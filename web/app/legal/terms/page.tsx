import type { Metadata } from 'next';
import Link from 'next/link';
import { LegalLayout } from '@/components/LegalLayout/LegalLayout';
import { LEGAL_ENTITY, LEGAL_VERSION, legalPage } from '@/lib/legal';

const page = legalPage('terms');

export const metadata: Metadata = { title: `${page.title} — mdreel`, description: page.description };

export default function TermsPage() {
  return (
    <LegalLayout
      title={page.title}
      lead="These Terms are the business-to-business agreement between you and Royalcode Robert Sztygowski for the use of mdreel. They also serve as the regulamin required by the Polish Act on Providing Services by Electronic Means."
      toc={[
        { id: 'parties', label: '1. parties & scope' },
        { id: 'b2b', label: '2. business use only' },
        { id: 'service', label: '3. the service' },
        { id: 'accounts', label: '4. accounts' },
        { id: 'fees', label: '5. fees & billing' },
        { id: 'aup', label: '6. acceptable use' },
        { id: 'content', label: '7. your content & IP' },
        { id: 'data', label: '8. data protection' },
        { id: 'availability', label: '9. availability' },
        { id: 'warranty', label: '10. warranties' },
        { id: 'liability', label: '11. liability' },
        { id: 'term', label: '12. term & termination' },
        { id: 'complaints', label: '13. complaints' },
        { id: 'changes', label: '14. changes to terms' },
        { id: 'law', label: '15. governing law' },
      ]}
    >
      <section id="parties">
        <h2>1. Parties and scope</h2>
        <p>
          These Terms of Service (the <strong>&ldquo;Terms&rdquo;</strong>, and, for the purposes of
          the Polish Act on Providing Services by Electronic Means, the{' '}
          <strong>&ldquo;Regulamin&rdquo;</strong>) govern the provision and use of the {LEGAL_ENTITY.brand}{' '}
          service available at {LEGAL_ENTITY.domain} (the <strong>&ldquo;Service&rdquo;</strong>).
        </p>
        <p>
          The Service is provided by <strong>{LEGAL_ENTITY.name}</strong>, {LEGAL_ENTITY.form},{' '}
          {LEGAL_ENTITY.address}, VAT {LEGAL_ENTITY.vat} (<strong>&ldquo;mdreel&rdquo;</strong>,{' '}
          &ldquo;we&rdquo;, &ldquo;us&rdquo;). Full provider details are on the{' '}
          <Link href="/legal/imprint">Imprint</Link>. By creating an account or otherwise using the
          Service, you (the <strong>&ldquo;Customer&rdquo;</strong>) accept these Terms. They are made
          available before you enter into the contract, and you can save and reproduce them from this
          page.
        </p>
      </section>

      <section id="b2b">
        <h2>2. Business use only</h2>
        <p>
          The Service is offered <strong>exclusively to entrepreneurs</strong> (przedsiębiorcy) within
          the meaning of Article 4(1) of the Polish Entrepreneurs&apos; Law, acting for purposes
          directly related to their business or professional activity. It is{' '}
          <strong>not offered to consumers</strong> (konsumenci within the meaning of Article 22¹ of
          the Polish Civil Code).
        </p>
        <p>
          By using the Service you warrant that you are acting in the course of your business and that
          this contract has a professional character for you. Consumer-protection rules that apply
          only to consumers — including any statutory right of withdrawal — do not apply to this
          agreement.
        </p>
      </section>

      <section id="service">
        <h2>3. The Service</h2>
        <p>
          mdreel converts video and audio recordings that you upload, or reference by URL where
          supported, into structured, timestamped Markdown and an equivalent structured document,
          for use in your own systems and AI knowledge bases. The Service is provided through a web
          application and an HTTP API, including webhooks.
        </p>
        <p>
          <strong>Technical requirements.</strong> Use of the Service requires a current web browser,
          an internet connection, and — for the API — the ability to make authenticated HTTPS
          requests. Supported input formats and limits are described in the product documentation,
          which forms part of these Terms.
        </p>
        <p>
          We may improve, change or discontinue features of the Service. We will not make a change
          that materially degrades a paid plan you are on during a paid period without offering you a
          remedy under section 14.
        </p>
      </section>

      <section id="accounts">
        <h2>4. Accounts</h2>
        <ul>
          <li>You must provide accurate registration details and keep your credentials confidential.</li>
          <li>
            You are responsible for all activity under your account and for your authorised users&apos;
            compliance with these Terms.
          </li>
          <li>
            We may require a valid VAT/NIP number to confirm your business status and for invoicing.
          </li>
          <li>
            You must notify us at <a href={`mailto:${LEGAL_ENTITY.email}`}>{LEGAL_ENTITY.email}</a> of
            any unauthorised use of your account.
          </li>
        </ul>
      </section>

      <section id="fees">
        <h2>5. Fees and billing</h2>
        <p>
          Paid plans are charged at the fees stated at the time of purchase, in the currency and
          billing cycle shown at checkout, exclusive of VAT unless stated otherwise. Applicable VAT is
          added at checkout. Payments are processed by our payment subprocessor; card details are
          entered with the payment provider and never reach mdreel servers.
        </p>
        <ul>
          <li>Subscriptions renew automatically for successive periods until cancelled.</li>
          <li>You can cancel at any time, effective at the end of the current paid period.</li>
          <li>
            Except where required by mandatory law or expressly stated, fees already paid are
            non-refundable; a cancelled subscription is not pro-rated.
          </li>
          <li>Any free trial credit is provided as-is and may be changed or withdrawn for future sign-ups.</li>
        </ul>
      </section>

      <section id="aup">
        <h2>6. Acceptable use</h2>
        <p>
          Your use of the Service must comply with our{' '}
          <Link href="/legal/acceptable-use">Acceptable Use Policy</Link>, which is incorporated into
          these Terms by reference. In particular, you must not provide any content that is unlawful.
          A breach of the Acceptable Use Policy is a material breach of these Terms and may lead to
          suspension or termination under section 12.
        </p>
      </section>

      <section id="content">
        <h2>7. Your content and intellectual property</h2>
        <p>
          <strong>Your content stays yours.</strong> You retain all rights in the recordings you
          upload (<strong>&ldquo;Customer Content&rdquo;</strong>) and in the Markdown and structured
          outputs generated from them. You grant us only the limited, non-exclusive rights needed to
          host and process Customer Content in order to provide the Service to you, and to comply with
          your instructions. We do not use Customer Content to train AI models.
        </p>
        <p>
          You warrant that you hold all rights, licences and consents necessary to upload and process
          the Customer Content, including the consent of any individuals appearing in it where the law
          requires it.
        </p>
        <p>
          <strong>Our intellectual property.</strong> The Service, its software, and its
          documentation remain our property or that of our licensors. We grant you a non-exclusive,
          non-transferable right to use the Service during the term. You may not reverse engineer the
          Service or use its outputs to build a competing service.
        </p>
      </section>

      <section id="data">
        <h2>8. Data protection</h2>
        <p>
          How we handle personal data is described in our{' '}
          <Link href="/legal/privacy">Privacy Policy</Link>. Where we process personal data contained
          in Customer Content on your behalf, we act as your <strong>processor</strong> and you as the{' '}
          <strong>controller</strong>; that processing is governed by our{' '}
          <Link href="/legal/dpa">Data Processing Agreement</Link> (the &ldquo;DPA&rdquo;), which is
          incorporated into these Terms by reference and which you accept by using the Service. All
          processing takes place within the European Economic Area.
        </p>
      </section>

      <section id="availability">
        <h2>9. Availability and support</h2>
        <p>
          We aim to keep the Service available but do not guarantee uninterrupted operation. The
          Service may be unavailable during maintenance or due to factors outside our control. Support
          is provided by email at <a href={`mailto:${LEGAL_ENTITY.email}`}>{LEGAL_ENTITY.email}</a>.
          No specific service level is promised unless separately agreed in writing.
        </p>
      </section>

      <section id="warranty">
        <h2>10. Warranties and disclaimers</h2>
        <p>
          The Service applies automated AI processing; outputs may contain errors, omissions or
          inaccuracies. You are responsible for reviewing outputs before relying on them. To the
          fullest extent permitted by law, and except as expressly stated in these Terms, the Service
          is provided <strong>&ldquo;as is&rdquo;</strong> and we disclaim all implied warranties,
          including fitness for a particular purpose and that outputs will be accurate or complete.
        </p>
      </section>

      <section id="liability">
        <h2>11. Limitation of liability</h2>
        <p>
          To the extent permitted by Polish law, our total aggregate liability arising out of or in
          connection with these Terms in any 12-month period is limited to the total fees you paid for
          the Service in that period. We are not liable for indirect or consequential loss, loss of
          profits, loss of data (beyond our obligation to maintain reasonable backups and security),
          or loss of goodwill.
        </p>
        <p className="legal-note">
          Nothing in these Terms excludes or limits liability that cannot be excluded or limited under
          mandatory law, including liability for damage caused intentionally (Article 473 § 2 of the
          Polish Civil Code) or for personal injury caused by our negligence.
        </p>
      </section>

      <section id="term">
        <h2>12. Term, suspension and termination</h2>
        <ul>
          <li>These Terms apply for as long as you have an account or use the Service.</li>
          <li>
            You may terminate by cancelling your subscription and closing your account; paid periods
            already started run to their end.
          </li>
          <li>
            We may suspend or terminate your access, with immediate effect where necessary, if you
            materially breach these Terms or the Acceptable Use Policy, if required by law, or to
            protect the Service or third parties. Where practicable and lawful we give notice and an
            opportunity to cure.
          </li>
          <li>
            On termination, your right to use the Service ends. Customer Content is handled as set out
            in the Privacy Policy and the DPA (including deletion).
          </li>
        </ul>
      </section>

      <section id="complaints">
        <h2>13. Complaints (reklamacje)</h2>
        <p>
          You may submit a complaint about the Service by email to{' '}
          <a href={`mailto:${LEGAL_ENTITY.email}`}>{LEGAL_ENTITY.email}</a>, describing the issue and
          your account. We will acknowledge and respond to complaints without undue delay, and in any
          event within 30 days of receipt, to the contact address associated with your account.
        </p>
      </section>

      <section id="changes">
        <h2>14. Changes to these Terms</h2>
        <p>
          We may amend these Terms for valid reasons — for example changes in law, new or changed
          features, or security requirements. We will give you at least <strong>14 days&apos;</strong>{' '}
          advance notice of material changes by email or in-product notice before they take effect. If
          you do not accept a material change, you may terminate the affected Service before the change
          takes effect; continued use after the effective date constitutes acceptance. The current
          version is {LEGAL_VERSION}, shown with its effective date at the top of this page.
        </p>
      </section>

      <section id="law">
        <h2>15. Governing law and jurisdiction</h2>
        <p>
          These Terms and any dispute arising out of them are governed by {LEGAL_ENTITY.governingLaw}.
          The competent courts are {LEGAL_ENTITY.venue}. If any provision is held invalid, the rest
          remain in force.
        </p>
      </section>
    </LegalLayout>
  );
}
