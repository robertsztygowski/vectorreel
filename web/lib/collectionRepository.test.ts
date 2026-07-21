// The site's reader for an ARCHITECTURE.md §4b v1.1 repository. The pages are DERIVED from the
// contract, so these tests are about the rules the pages must not be able to break — above all
// that a `reference` entry never acquires a session page, because it has no document to show.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  chronological,
  countsOf,
  dateOf,
  deepLink,
  findSession,
  isFull,
  listCollections,
  loadCollection,
  loadSessionDocument,
  sessionsOf,
  tierOf,
  type ManifestSession,
} from './collectionRepository';

const collection = loadCollection('repository')!;

test('the canonical repository fixture loads', () => {
  assert.ok(collection, 'fixture collection resolves');
  assert.equal(collection.manifest.repository.visibility, 'public-collection');
  assert.deepEqual(listCollections(), ['repository']);
});

test('an unknown collection is null, not a throw', () => {
  assert.equal(loadCollection('does-not-exist'), null);
});

test('an absent inclusion field reads as the full tier', () => {
  const legacy = { slug: 'x', title: 't', source: 's', tags: [] } as unknown as ManifestSession;
  assert.equal(tierOf(legacy), 'full');
  assert.ok(isFull(legacy));
});

test('counts split the two tiers', () => {
  const counts = countsOf(collection);
  assert.ok(counts.full >= 1, 'at least one full session');
  assert.ok(counts.reference >= 1, 'at least one reference entry');
  assert.equal(counts.full + counts.reference, collection.manifest.sessions.length);
});

test('a reference entry has no session document — asking returns null, never a stub', () => {
  const reference = collection.manifest.sessions.find((s) => !isFull(s))!;
  assert.equal(loadSessionDocument('repository', reference), null);
  assert.equal(reference.file, undefined, 'the contract forbids a file on a reference entry');
});

test('a full entry resolves to a §4 document whose title matches the manifest', () => {
  const full = collection.manifest.sessions.find(isFull)!;
  const document = loadSessionDocument('repository', full);
  assert.ok(document);
  assert.equal(document!.frontmatter.title, full.title);
});

test('deepLink points into the original video at the cited second', () => {
  const reference = collection.manifest.sessions.find((s) => !isFull(s))!;
  const link = deepLink(reference, '00:04:24');
  assert.ok(link.startsWith(reference.source), 'links to the original, not to mdreel');
  assert.ok(link.includes('t=264s'), `4m24s is 264 seconds: ${link}`);
  assert.ok(!link.includes('/collections/'), 'a reference link never points back at mdreel');
});

test('deepLink respects an existing query string', () => {
  const withQuery = { source: 'https://example.com/watch?v=abc' } as ManifestSession;
  assert.equal(deepLink(withQuery, '00:01:00'), 'https://example.com/watch?v=abc&t=60s');
  const withoutQuery = { source: 'https://example.com/abc' } as ManifestSession;
  assert.equal(deepLink(withoutQuery, '00:01:00'), 'https://example.com/abc?t=60s');
});

test('chronological ordering uses the recorded date and keeps undated entries last', () => {
  const ordered = chronological(collection.manifest.sessions);
  const dates = ordered.map(dateOf).filter(Boolean) as string[];
  assert.deepEqual(dates, [...dates].sort(), 'ascending by date');

  const undated = { slug: 'u', title: 'u', source: 's', tags: [] } as unknown as ManifestSession;
  const mixed = chronological([undated, ...collection.manifest.sessions]);
  assert.equal(mixed.at(-1)!.slug, 'u', 'an undated entry sorts last, not to 1970');
});

test('sessionsOf resolves index slugs and silently drops danglers', () => {
  const topic = collection.manifest.topics[0];
  assert.equal(sessionsOf(collection, topic.sessions).length, topic.sessions.length);
  assert.deepEqual(sessionsOf(collection, ['no-such-session']), []);
});

test('findSession locates by slug', () => {
  const first = collection.manifest.sessions[0];
  assert.equal(findSession(collection, first.slug)?.slug, first.slug);
  assert.equal(findSession(collection, 'nope'), undefined);
});

test('every topic and speaker index resolves to at least one real session', () => {
  for (const index of [...collection.manifest.topics, ...collection.manifest.speakers]) {
    assert.ok(
      sessionsOf(collection, index.sessions).length >= 1,
      `${index.slug} resolves to real sessions`,
    );
  }
});
