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

const REPO_DIR = join(process.cwd(), '..', 'tests', 'fixtures', 'repository');
const CONTRACTS_DIR = join(process.cwd(), '..', 'tests', 'fixtures', 'contracts');

const ajv = new Ajv2020({ allErrors: true, strict: true });
addFormats(ajv);
const validateManifest = ajv.compile(
  JSON.parse(readFileSync(join(CONTRACTS_DIR, 'repository-manifest.schema.json'), 'utf-8')),
);

interface ManifestSession {
  slug: string;
  file: string;
  title: string;
  duration: string;
  language: string;
  processed_at: string;
  recorded_at?: string;
  source: string;
  tags: string[];
  speakers?: string[];
}
interface ManifestIndex {
  slug: string;
  file: string;
  sessions: string[];
}
interface Manifest {
  repository: { visibility: string };
  sessions: ManifestSession[];
  topics: ManifestIndex[];
  speakers: ManifestIndex[];
}

const manifest: Manifest = JSON.parse(
  readFileSync(join(REPO_DIR, 'metadata', 'manifest.json'), 'utf-8'),
);

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
  manifest.sessions.map((s) => [s.slug, parseDocument(readFileSync(join(REPO_DIR, s.file), 'utf-8'))]),
);

for (const s of manifest.sessions) {
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

test('sessions directory contains exactly the manifest sessions', () => {
  assert.deepEqual(
    readdirSync(join(REPO_DIR, 'sessions')).sort(),
    manifest.sessions.map((s) => `${s.slug}.md`).sort(),
  );
});

// §4b citation grammar: `- [hh:mm:ss](../sessions/<slug>.md#<anchor>) — <section heading>`
const CITATION = /^- \[(\d{2}:\d{2}:\d{2})\]\(\.\.\/sessions\/([a-z0-9-]+)\.md#([a-z0-9-]+)\) — (.+)$/;

function assertIndexFile(entry: ManifestIndex, kind: string) {
  const raw = readFileSync(join(REPO_DIR, entry.file), 'utf-8');
  assert.ok(!raw.includes('\r'), `${entry.file}: LF only`);
  assert.ok(raw.endsWith('\n') && !raw.endsWith('\n\n'), `${entry.file}: one trailing newline`);

  const citations = raw.split('\n').filter((line) => line.startsWith('- '));
  assert.ok(citations.length >= 1, `${entry.file}: at least one citation`);
  const citedSlugs = new Set<string>();
  for (const line of citations) {
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
  for (const s of manifest.sessions) {
    assert.ok(raw.includes(`../sessions/${s.slug}.md`), `timeline links ${s.slug}`);
  }
  for (const line of raw.split('\n')) {
    const m = line.match(/^- (\d{4}-\d{2}-\d{2}) — /);
    if (m) dates.push(m[1]);
  }
  assert.equal(dates.length, manifest.sessions.length, 'one dated entry per session');
  assert.deepEqual(dates, [...dates].sort(), 'chronological order');
});
