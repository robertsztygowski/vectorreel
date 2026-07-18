// Single source of truth for the legal pack — entity identity, versioning, the page registry and
// the subprocessor list. Pages and tests both read from here so the entity name, effective date and
// version can never drift between the six pages. Facts are grounded in INFRA.md (subprocessors,
// regions, retention) and the founder-provided imprint data — never invented.

/** The operating entity (public-by-law imprint data — not a secret). */
export const LEGAL_ENTITY = {
  /** Full legal name of the sole proprietorship (JDG). */
  name: 'Royalcode Robert Sztygowski',
  owner: 'Robert Sztygowski',
  form: 'Jednoosobowa działalność gospodarcza (Polish sole proprietorship)',
  address: 'ul. Józefa Wybickiego 2/9, 05-820 Piastów, Poland',
  country: 'Poland',
  vat: 'PL 8222319203',
  /** One contact for legal notices, privacy/GDPR requests and support. */
  email: 'hello@mdreel.com',
  brand: 'mdreel',
  domain: 'mdreel.com',
  governingLaw: 'Polish law',
  venue: 'the Polish courts competent for the seller’s registered office',
} as const;

/** Bump on any material change; each page shows it alongside the effective date. */
export const LEGAL_VERSION = '1.0';

/** Effective date of this version of the pack (run date). ISO 8601. */
export const LEGAL_EFFECTIVE_DATE = '2026-07-18';

export interface LegalPage {
  slug: string;
  /** Page <h1> / document title. */
  title: string;
  /** Short label used in the footer + cross-links. */
  navLabel: string;
  /** One-line summary for the page lead + metadata description. */
  description: string;
}

/** The six routes, all under /legal/…, in reading order. */
export const LEGAL_PAGES: readonly LegalPage[] = [
  {
    slug: 'terms',
    title: 'Terms of Service',
    navLabel: 'Terms',
    description:
      'The business-to-business agreement governing your use of mdreel — accounts, fees, acceptable use, liability and Polish governing law.',
  },
  {
    slug: 'privacy',
    title: 'Privacy Policy',
    navLabel: 'Privacy',
    description:
      'What personal data mdreel processes, why, on what legal basis, for how long, who our subprocessors are, and your GDPR rights — including our cookieless analytics notice.',
  },
  {
    slug: 'imprint',
    title: 'Imprint',
    navLabel: 'Imprint',
    description:
      'The legally required service-provider identification for mdreel under the Polish Act on Providing Services by Electronic Means.',
  },
  {
    slug: 'dpa',
    title: 'Data Processing Agreement',
    navLabel: 'DPA',
    description:
      'The GDPR Article 28 controller–processor terms for customer content you upload to mdreel, incorporated by reference into the Terms of Service.',
  },
  {
    slug: 'subprocessors',
    title: 'Subprocessors',
    navLabel: 'Subprocessors',
    description:
      'The current list of subprocessors mdreel uses to deliver the service, all with EU-region processing, and how we notify you of changes.',
  },
  {
    slug: 'acceptable-use',
    title: 'Acceptable Use Policy',
    navLabel: 'Acceptable Use',
    description:
      'What you may and may not upload to or do with mdreel, and the grounds on which we may suspend a violating account.',
  },
] as const;

export function legalPage(slug: string): LegalPage {
  const page = LEGAL_PAGES.find((p) => p.slug === slug);
  if (!page) throw new Error(`Unknown legal page: ${slug}`);
  return page;
}

export interface Subprocessor {
  name: string;
  purpose: string;
  /** Where the processing physically happens — EU-only per CLAUDE.md rule 2. */
  location: string;
  /** Live/planned status — honest, matching INFRA.md reality. */
  status: string;
}

// Truth from INFRA.md — never imagination. Umami is self-hosted first-party and is deliberately
// NOT a subprocessor (it is a selling point); that is stated on the page, not encoded here.
export const SUBPROCESSORS: readonly Subprocessor[] = [
  {
    name: 'Google Cloud (Google Ireland Limited / Google Cloud EMEA)',
    purpose:
      'Cloud infrastructure hosting the whole service: compute (Cloud Run), the application database (Cloud SQL), object storage for uploads and outputs (Cloud Storage), secret storage (Secret Manager) and AI video/audio analysis (Vertex AI, no training on customer data).',
    location:
      'European Union — Cloud Run in europe-west1 (Belgium); Cloud SQL, Cloud Storage and Secret Manager in europe-central2 (Warsaw); Vertex AI pinned to europe-central2 / europe-west3 (Frankfurt).',
    status: 'Active',
  },
  {
    name: 'Stripe (Stripe Payments Europe, Ltd.)',
    purpose:
      'Payment processing and subscription billing. Card details are entered on Stripe and never touch mdreel servers; mdreel stores only billing identifiers.',
    location: 'European Union (Stripe Payments Europe, Ltd., Ireland).',
    status: 'Integrated (test mode) — not yet processing live payments.',
  },
  {
    name: 'Brevo (Sendinblue SAS)',
    purpose: 'Transactional email (account and service notifications).',
    location: 'European Union (France).',
    status: 'Planned — no email is sent at this time.',
  },
] as const;
