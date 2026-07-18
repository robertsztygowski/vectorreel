import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  LEGAL_ENTITY,
  LEGAL_EFFECTIVE_DATE,
  LEGAL_VERSION,
  LEGAL_PAGES,
  SUBPROCESSORS,
  legalPage,
} from './legal';

// The legal pack's single source of truth. These invariants back the six pages: every page reads
// the entity name, effective date and version from here, so asserting them here is asserting them
// everywhere. The E2E spec proves they actually render.

test('there are exactly the six settled legal routes', () => {
  const slugs = LEGAL_PAGES.map((p) => p.slug).sort();
  assert.deepEqual(slugs, [
    'acceptable-use',
    'dpa',
    'imprint',
    'privacy',
    'subprocessors',
    'terms',
  ]);
});

test('every page has a title, nav label and description', () => {
  for (const page of LEGAL_PAGES) {
    assert.ok(page.slug.length > 0, 'slug');
    assert.ok(page.title.length > 0, `${page.slug} title`);
    assert.ok(page.navLabel.length > 0, `${page.slug} navLabel`);
    assert.ok(page.description.length > 20, `${page.slug} description`);
  }
});

test('legalPage resolves known slugs and throws on unknown', () => {
  assert.equal(legalPage('terms').title, 'Terms of Service');
  assert.throws(() => legalPage('nope'), /Unknown legal page/);
});

test('the entity is the founder-provided JDG with VAT and single contact', () => {
  assert.equal(LEGAL_ENTITY.name, 'Royalcode Robert Sztygowski');
  assert.equal(LEGAL_ENTITY.vat, 'PL 8222319203');
  assert.equal(LEGAL_ENTITY.email, 'hello@mdreel.com');
  assert.match(LEGAL_ENTITY.address, /Piastów, Poland$/);
  assert.match(LEGAL_ENTITY.governingLaw, /Polish/);
});

test('version and effective date are set (ISO date)', () => {
  assert.equal(LEGAL_VERSION, '1.0');
  assert.match(LEGAL_EFFECTIVE_DATE, /^\d{4}-\d{2}-\d{2}$/);
});

test('subprocessors are the INFRA.md truth, all EU, none named Umami', () => {
  const names = SUBPROCESSORS.map((s) => s.name).join(' ');
  assert.match(names, /Google Cloud/);
  assert.match(names, /Stripe/);
  assert.match(names, /Brevo/);
  // Umami is self-hosted first-party — never a subprocessor.
  assert.ok(!/umami/i.test(names), 'Umami must not be listed as a subprocessor');
  for (const s of SUBPROCESSORS) {
    assert.match(s.location, /EU|European Union|Ireland|Belgium|Warsaw|Frankfurt|France/, `${s.name} EU location`);
    assert.ok(s.purpose.length > 0, `${s.name} purpose`);
    assert.ok(s.status.length > 0, `${s.name} status`);
  }
});
