// The repository-contract proof (ARCHITECTURE.md §4b): the committed canonical example under
// tests/fixtures/repository/ validates against the manifest schema, every session is a
// grammar-valid §4 document whose frontmatter agrees with the manifest, and every index entry
// cites a real session section with the correct GitHub anchor. If any of this fails, either the
// contract changed (update §4b + schema + fixture together) or the fixture drifted — never
// loosen a test to make it pass.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { join } from 'node:path';
import { Ajv2020 } from 'ajv/dist/2020';
import addFormats from 'ajv-formats';
import { parseDocument } from './outputDocument';

// Defaults to the canonical fixture. Point MDREEL_REPOSITORY_DIR at a GENERATED repository to run
// this same oracle against real output — the contract is only worth having if the thing we actually
// ship is held to it, not just the example.
const REPO_DIR = process.env.MDREEL_REPOSITORY_DIR
  ?? join(process.cwd(), '..', 'tests', 'fixtures', 'repository');
const CONTRACTS_DIR = join(process.cwd(), '..', 'tests', 'fixtures', 'contracts');

const ajv = new Ajv2020({ allErrors: true, strict: true });
addFormats(ajv);
const validateManifest = ajv.compile(
  JSON.parse(readFileSync(join(CONTRACTS_DIR, 'repository-manifest.schema.json'), 'utf-8')),
);

interface ManifestSession {
  slug: string;
  inclusion?: 'full' | 'reference';
  file: string;
  title: string;
  duration: string;
  language: string;
  processed_at: string;
  recorded_at?: string;
  source: string;
  event?: string;
  year?: number;
  licence?: string;
  licence_verified_via?: string;
  citations?: { timestamp: string; label: string }[];
  tags: string[];
  speakers?: string[];
  attribution?: string;
}
interface ManifestIndex {
  slug: string;
  file: string;
  sessions: string[];
}
interface Manifest {
  repository: { visibility: string; contract_version: string };
  sessions: ManifestSession[];
  topics: ManifestIndex[];
  speakers: ManifestIndex[];
}

const manifest: Manifest = JSON.parse(
  readFileSync(join(REPO_DIR, 'metadata', 'manifest.json'), 'utf-8'),
);

// §4b v1.1 publication tier. Absent ⇒ "full" (v1 manifests keep their meaning).
const tierOf = (s: ManifestSession) => s.inclusion ?? 'full';
const fullSessions = manifest.sessions.filter((s) => tierOf(s) === 'full');
const referenceSessions = manifest.sessions.filter((s) => tierOf(s) === 'reference');

// §4b: slugification of a frontmatter tag or heading fragment.
function slugify(text: string): string {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9 -]/g, '')
    .trim()
    .replace(/ +/g, '-');
}

// GitHub auto-anchor of a §4 section heading `## [hh:mm:ss] Topic`.
function githubAnchor(timestamp: string, heading: string): string {
  return slugify(`[${timestamp}] ${heading}`);
}

test('manifest validates against repository-manifest.schema.json', () => {
  assert.ok(
    validateManifest(manifest),
    JSON.stringify(validateManifest.errors, null, 2),
  );
});

test('layout: root README and every contract directory exist', () => {
  for (const p of ['README.md', 'sessions', 'topics', 'speakers', join('timeline', 'index.md'), join('metadata', 'manifest.json')]) {
    assert.ok(existsSync(join(REPO_DIR, p)), `${p} exists`);
  }
});

const sessionDocs = new Map(
  fullSessions.map((s) => [s.slug, parseDocument(readFileSync(join(REPO_DIR, s.file), 'utf-8'))]),
);

for (const s of fullSessions) {
  test(`session ${s.slug}: §4-valid document whose frontmatter agrees with the manifest`, () => {
    const doc = sessionDocs.get(s.slug)!;
    assert.equal(doc.frontmatter.title, s.title);
    assert.equal(doc.frontmatter.duration, s.duration);
    assert.equal(doc.frontmatter.language, s.language);
    assert.equal(doc.frontmatter.processed_at, s.processed_at);
    assert.equal(doc.frontmatter.source, s.source);
    assert.deepEqual(doc.frontmatter.tags.map(slugify), s.tags, 'manifest tags are the slugified frontmatter tags');
    const datePrefix = (s.recorded_at ?? s.processed_at.slice(0, 10)) + '-';
    assert.ok(s.slug.startsWith(datePrefix), `slug carries the §4b date prefix ${datePrefix}`);
    assert.equal(s.file, `sessions/${s.slug}.md`);
  });
}

test('sessions directory contains exactly the full-tier sessions', () => {
  assert.deepEqual(
    readdirSync(join(REPO_DIR, 'sessions')).sort(),
    fullSessions.map((s) => `${s.slug}.md`).sort(),
    'reference entries never get a session document — that is the whole point of the tier',
  );
});

// §4b citation grammar, full tier: `- [hh:mm:ss](../sessions/<slug>.md#<anchor>) — <section heading>`
const CITATION = /^- \[(\d{2}:\d{2}:\d{2})\]\(\.\.\/sessions\/([a-z0-9-]+)\.md#([a-z0-9-]+)\) — (.+)$/;
// §4b v1.1 citation grammar, reference tier: links OUT to the original, marked `· reference`.
// It can never point at ../sessions/ because no session document exists to point at.
const REF_CITATION = /^- \[(\d{2}:\d{2}:\d{2})\]\((https:\/\/[^)\s]+)\) — (.+) · reference$/;

const referenceBySlug = new Map(referenceSessions.map((s) => [s.slug, s]));

function assertIndexFile(entry: ManifestIndex, kind: string) {
  const raw = readFileSync(join(REPO_DIR, entry.file), 'utf-8');
  assert.ok(!raw.includes('\r'), `${entry.file}: LF only`);
  assert.ok(raw.endsWith('\n') && !raw.endsWith('\n\n'), `${entry.file}: one trailing newline`);

  const citations = raw.split('\n').filter((line) => line.startsWith('- '));
  assert.ok(citations.length >= 1, `${entry.file}: at least one citation`);
  const citedSlugs = new Set<string>();
  const expectedRefs = entry.sessions.filter((slug) => referenceBySlug.has(slug));
  const seenRefs = new Set<string>();

  for (const line of citations) {
    const ref = line.match(REF_CITATION);
    if (ref) {
      const [, timestamp, url, label] = ref;
      const target = referenceSessions.find((s) => url.startsWith(s.source));
      assert.ok(target, `${entry.file}: reference citation links to a manifest reference entry: ${line}`);
      seenRefs.add(target!.slug);
      citedSlugs.add(target!.slug);
      const cited = target!.citations!.find((c) => c.timestamp === timestamp);
      assert.ok(cited, `${entry.file}: [${timestamp}] is a curated citation on ${target!.slug}`);
      assert.equal(label, cited!.label, `${entry.file}: the label is the manifest label, not new prose`);
      continue;
    }
    const m = line.match(CITATION);
    assert.ok(m, `${entry.file}: citation matches the §4b grammar: ${line}`);
    const [, timestamp, slug, anchor, heading] = m!;
    citedSlugs.add(slug);
    const doc = sessionDocs.get(slug);
    assert.ok(doc, `${entry.file}: cited session ${slug} is in the manifest`);
    const section = doc!.sections.find((sec) => sec.timestamp === timestamp);
    assert.ok(section, `${entry.file}: [${timestamp}] exists in ${slug}`);
    assert.equal(heading, section!.heading, `${entry.file}: cited heading is the section heading, not a restatement`);
    assert.equal(anchor, githubAnchor(timestamp, section!.heading), `${entry.file}: anchor is the GitHub auto-anchor`);
  }
  assert.deepEqual(
    [...seenRefs].sort(),
    [...expectedRefs].sort(),
    `${entry.file}: every reference entry is cited in the out-linking form`,
  );
  assert.deepEqual(
    [...citedSlugs].sort(),
    [...entry.sessions].sort(),
    `${entry.file}: cites exactly the manifest's ${kind} sessions`,
  );
}

for (const topic of manifest.topics) {
  test(`topic index ${topic.slug}: citations resolve, tag appears in a session`, () => {
    assertIndexFile(topic, 'topic');
    assert.ok(
      manifest.sessions.some((s) => s.tags.includes(topic.slug)),
      `topic ${topic.slug} is a session tag`,
    );
  });
}

for (const speaker of manifest.speakers) {
  test(`speaker index ${speaker.slug}: citations resolve, sessions cross-reference back`, () => {
    assertIndexFile(speaker, 'speaker');
    for (const slug of speaker.sessions) {
      const s = manifest.sessions.find((x) => x.slug === slug)!;
      assert.ok(s.speakers?.includes(speaker.slug), `session ${slug} lists speaker ${speaker.slug}`);
    }
  });
}

test('every session speaker and tag has its index file', () => {
  const topicSlugs = new Set(manifest.topics.map((t) => t.slug));
  const speakerSlugs = new Set(manifest.speakers.map((sp) => sp.slug));
  for (const s of manifest.sessions) {
    for (const tag of s.tags) assert.ok(topicSlugs.has(tag), `tag ${tag} has a topic index`);
    for (const sp of s.speakers ?? []) assert.ok(speakerSlugs.has(sp), `speaker ${sp} has an index`);
  }
});

test('timeline: links every session, in chronological order', () => {
  const raw = readFileSync(join(REPO_DIR, 'timeline', 'index.md'), 'utf-8');
  const dates: string[] = [];
  for (const s of fullSessions) {
    assert.ok(raw.includes(`../sessions/${s.slug}.md`), `timeline links ${s.slug}`);
  }
  for (const s of referenceSessions) {
    assert.ok(raw.includes(s.source), `timeline links reference ${s.slug} to its original`);
    assert.ok(!raw.includes(`../sessions/${s.slug}.md`), `timeline never fakes a session page for ${s.slug}`);
  }
  for (const line of raw.split('\n')) {
    const m = line.match(/^- (\d{4}-\d{2}-\d{2}) — /);
    if (m) dates.push(m[1]);
  }
  assert.equal(dates.length, manifest.sessions.length, 'one dated entry per session');
  assert.deepEqual(dates, [...dates].sort(), 'chronological order');
});

// ── §4b v1.1: the two publication tiers ────────────────────────────────────────
// These are gates, not descriptions. A collection that blurs the tiers is worse than one that
// publishes less: `full` is a near-complete derivative of the talk (CC-BY only), `reference` is a
// citation of material we may not render. Never loosen these to make a corpus pass.

const clone = (): Manifest => JSON.parse(JSON.stringify(manifest));
const refIndex = manifest.sessions.findIndex((s) => tierOf(s) === 'reference');
const fullIndex = manifest.sessions.findIndex((s) => tierOf(s) === 'full');

test('the fixture exercises both tiers', () => {
  assert.ok(fullSessions.length >= 1, 'at least one full session');
  assert.ok(referenceSessions.length >= 1, 'at least one reference entry');
  assert.equal(manifest.repository.contract_version, '1.1');
});

test('a reference entry may not claim a session document', () => {
  for (const field of ['file', 'duration', 'language', 'processed_at'] as const) {
    const m = clone();
    // Each of these asserts that the video was processed and rendered — exactly what the
    // reference tier promises did not happen.
    (m.sessions[refIndex] as Record<string, unknown>)[field] =
      field === 'file' ? `sessions/${m.sessions[refIndex].slug}.md`
      : field === 'duration' ? '00:30:00'
      : field === 'language' ? 'en'
      : '2026-07-21T00:00:00Z';
    assert.equal(validateManifest(m), false, `reference entry carrying ${field} must fail validation`);
  }
});

test('a reference entry must carry provenance and at least one deep link', () => {
  for (const field of ['event', 'year', 'citations'] as const) {
    const m = clone();
    delete (m.sessions[refIndex] as Record<string, unknown>)[field];
    assert.equal(validateManifest(m), false, `reference entry without ${field} must fail validation`);
  }
});

test('a reference citation label may not grow into prose', () => {
  const m = clone();
  m.sessions[refIndex].citations![0].label = 'x'.repeat(121);
  assert.equal(validateManifest(m), false, 'an over-long label is derived text wearing a label’s clothes');
});

test('a full entry may not carry curated citations', () => {
  const m = clone();
  (m.sessions[fullIndex] as Record<string, unknown>).citations = [
    { timestamp: '00:00:00', label: 'Start' },
  ];
  assert.equal(validateManifest(m), false, 'a full session’s citations live in its document, not the manifest');
});

test('a public-collection entry without licence evidence + attribution fails', () => {
  for (const field of ['licence', 'licence_verified_via', 'attribution'] as const) {
    for (const idx of [fullIndex, refIndex]) {
      const m = clone();
      delete (m.sessions[idx] as Record<string, unknown>)[field];
      assert.equal(
        validateManifest(m),
        false,
        `public collection: session ${m.sessions[idx].slug} without ${field} must fail validation`,
      );
    }
  }
});

test('a private repository is not required to carry public licence evidence', () => {
  const m = clone();
  m.repository.visibility = 'private';
  delete (m.repository as Record<string, unknown>).licence_note;
  for (const s of m.sessions) {
    delete (s as Record<string, unknown>).licence;
    delete (s as Record<string, unknown>).licence_verified_via;
    delete (s as Record<string, unknown>).attribution;
  }
  assert.ok(validateManifest(m), JSON.stringify(validateManifest.errors, null, 2));
});

test('a v1 manifest stays valid and means full tier', () => {
  const m = clone();
  m.repository.contract_version = '1';
  m.sessions = m.sessions.filter((s) => tierOf(s) === 'full');
  for (const s of m.sessions) delete (s as Record<string, unknown>).inclusion;
  m.topics = m.topics.filter((t) => t.sessions.every((slug) => m.sessions.some((s) => s.slug === slug)));
  m.speakers = m.speakers.filter((sp) => sp.sessions.every((slug) => m.sessions.some((s) => s.slug === slug)));
  assert.ok(validateManifest(m), JSON.stringify(validateManifest.errors, null, 2));
  assert.ok(m.sessions.every((s) => tierOf(s) === 'full'), 'absent inclusion reads as full');
});
