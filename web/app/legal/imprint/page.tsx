import type { Metadata } from 'next';
import { LegalLayout } from '@/components/LegalLayout/LegalLayout';
import { LEGAL_ENTITY, legalPage } from '@/lib/legal';

const page = legalPage('imprint');

export const metadata: Metadata = { title: `${page.title} — mdreel`, description: page.description };

export default function ImprintPage() {
  return (
    <LegalLayout
      title={page.title}
      lead="Service-provider identification for mdreel, published under Article 5 of the Polish Act on Providing Services by Electronic Means (ustawa o świadczeniu usług drogą elektroniczną)."
      toc={[
        { id: 'provider', label: 'service provider' },
        { id: 'contact', label: 'contact' },
        { id: 'vat', label: 'tax identification' },
        { id: 'register', label: 'business register' },
        { id: 'responsibility', label: 'editorial responsibility' },
      ]}
    >
      <section id="provider">
        <h2>Service provider</h2>
        <p>The service provider operating {LEGAL_ENTITY.brand} ({LEGAL_ENTITY.domain}) is:</p>
        <table>
          <tbody>
            <tr>
              <th scope="row">Business name</th>
              <td>{LEGAL_ENTITY.name}</td>
            </tr>
            <tr>
              <th scope="row">Legal form</th>
              <td>{LEGAL_ENTITY.form}</td>
            </tr>
            <tr>
              <th scope="row">Owner</th>
              <td>{LEGAL_ENTITY.owner}</td>
            </tr>
            <tr>
              <th scope="row">Registered address</th>
              <td>{LEGAL_ENTITY.address}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <section id="contact">
        <h2>Contact</h2>
        <p>
          For legal notices, privacy and data-protection requests, abuse reports and support, one
          address reaches us:
        </p>
        <table>
          <tbody>
            <tr>
              <th scope="row">Email</th>
              <td>
                <a href={`mailto:${LEGAL_ENTITY.email}`}>{LEGAL_ENTITY.email}</a>
              </td>
            </tr>
            <tr>
              <th scope="row">Website</th>
              <td>https://{LEGAL_ENTITY.domain}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <section id="vat">
        <h2>Tax identification</h2>
        <table>
          <tbody>
            <tr>
              <th scope="row">VAT / NIP</th>
              <td>{LEGAL_ENTITY.vat}</td>
            </tr>
          </tbody>
        </table>
        <p>
          The NIP is the public tax identification number of the entrepreneur, displayed in
          accordance with Article 20(3) of the Polish Entrepreneurs&apos; Law (Prawo Przedsiębiorców).
        </p>
      </section>

      <section id="register">
        <h2>Business register</h2>
        <p>
          {LEGAL_ENTITY.name} is a sole proprietorship registered in the Polish Central Registration
          and Information on Business (Centralna Ewidencja i Informacja o Działalności Gospodarczej —
          CEIDG). As a sole proprietorship it is not entered in the National Court Register (KRS) and
          has no KRS number; the NIP above is its public identifier. The CEIDG register is maintained
          by the minister responsible for the economy and is publicly searchable at{' '}
          <a href="https://www.biznes.gov.pl/en" rel="noopener noreferrer">
            biznes.gov.pl
          </a>
          .
        </p>
      </section>

      <section id="responsibility">
        <h2>Editorial responsibility</h2>
        <p>
          Responsible for the content of {LEGAL_ENTITY.domain}: {LEGAL_ENTITY.owner},{' '}
          {LEGAL_ENTITY.address}.
        </p>
      </section>
    </LegalLayout>
  );
}
