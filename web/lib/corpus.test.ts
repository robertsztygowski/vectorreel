import { test } from 'node:test';
import assert from 'node:assert/strict';
import { loadCorpusIndex, loadCorpusMarkdownRaw, findCorpusEntry } from './corpus';
import { parseDocument } from './outputDocument';

const VIDEO_IDS = ['5si4zkAngpA', 'JvbBFwlqxeI', 'KL7WBjAuTMg', 'gRFaow12xo0', 'rAl-9HwD858'];

test('corpus index has all 5 accepted entries', () => {
  assert.equal(loadCorpusIndex().length, 5);
});

for (const videoId of VIDEO_IDS) {
  test(`${videoId}: is a canonical contract document consistent with corpus.json`, () => {
    const entry = findCorpusEntry(videoId);
    assert.ok(entry, 'corpus.json entry exists');

    // Strict parse — any deviation from the ratified grammar throws (Phase 2.5; the old
    // tolerant-parser cases are now contract-violation cases in outputDocument.test.ts).
    const parsed = parseDocument(loadCorpusMarkdownRaw(videoId));

    assert.ok(parsed.sections.length > 0, 'has at least one timestamped section');

    const labelsSeen = new Set(parsed.sections.flatMap((s) => s.blocks.map((b) => b.label)));
    assert.ok(labelsSeen.has('spoken'), 'has a spoken block somewhere in the file');
    assert.ok(labelsSeen.has('on_screen'), 'has an on_screen block somewhere in the file');
    assert.ok(labelsSeen.has('visual'), 'has a visual block somewhere in the file');

    // Cross-file invariants the gallery depends on: `source` is the canonical source URL on the
    // ingest path, and corpus.json's attribution string appears verbatim in the provenance
    // section (getLicenceBlock renders from corpus.json — the two must never diverge).
    assert.equal(parsed.frontmatter.source, entry!.url);
    assert.ok(parsed.provenance.includes(entry!.attribution), 'provenance carries the attribution');
  });
}
