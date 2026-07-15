import { test } from 'node:test';
import assert from 'node:assert/strict';
import { loadCorpusIndex, loadCorpusMarkdownRaw, parseMarkdown, findCorpusEntry } from './corpus';

const EXPECTED_FRONTMATTER_KEYS = [
  'title',
  'source_filename',
  'duration',
  'language',
  'processed_at',
  'generator',
  'summary',
  'tags',
];

const VIDEO_IDS = ['5si4zkAngpA', 'JvbBFwlqxeI', 'KL7WBjAuTMg', 'gRFaow12xo0', 'rAl-9HwD858'];

test('corpus index has all 5 accepted entries', () => {
  assert.equal(loadCorpusIndex().length, 5);
});

for (const videoId of VIDEO_IDS) {
  test(`${videoId}: parses without throwing and matches expected shape`, () => {
    const entry = findCorpusEntry(videoId);
    assert.ok(entry, 'corpus.json entry exists');

    const raw = loadCorpusMarkdownRaw(videoId);
    const parsed = parseMarkdown(raw);

    for (const key of EXPECTED_FRONTMATTER_KEYS) {
      assert.ok(key in parsed.frontmatter, `frontmatter has "${key}"`);
    }

    assert.ok(parsed.sections.length > 0, 'has at least one timestamped section');

    // Order/format tolerance check: labels can appear in any order and inline-or-next-line —
    // gRFaow12xo0 reverses to On screen -> Visual -> Spoken; 5si4zkAngpA omits heading brackets.
    const labelsSeen = new Set(parsed.sections.flatMap((s) => s.blocks.map((b) => b.label)));
    assert.ok(labelsSeen.has('spoken'), 'has a spoken block somewhere in the file');
    assert.ok(labelsSeen.has('on_screen'), 'has an on_screen block somewhere in the file');
    assert.ok(labelsSeen.has('visual'), 'has a visual block somewhere in the file');

    // No block text should still carry a raw blockquote marker after normalization.
    for (const section of parsed.sections) {
      for (const block of section.blocks) {
        assert.ok(!block.text.startsWith('> '), `block text for ${videoId} strips blockquote markers`);
      }
    }

    assert.ok(entry!.attribution.length > 0, 'corpus.json attribution is non-empty');
  });
}
