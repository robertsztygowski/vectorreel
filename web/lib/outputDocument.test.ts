import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { parseDocument, renderDocument, OutputContractViolation } from './outputDocument';

const SAMPLE = readFileSync(join(process.cwd(), 'fixtures', 'output', 'private_sample.md'), 'utf-8');

function assertViolation(raw: string, why: string) {
  assert.throws(() => parseDocument(raw), OutputContractViolation, why);
}

test('canonical document round-trips byte-identically', () => {
  const doc = parseDocument(SAMPLE);
  assert.equal(renderDocument(doc), SAMPLE);
});

// Every case below was legal under the pre-Phase-2.5 tolerant parser and appeared in the real
// Phase-0.2 corpus. The contract freeze turned each into a violation — these tests are the freeze.

test('CRLF line endings are rejected (contract is LF-only)', () => {
  assertViolation(SAMPLE.replace(/\n/g, '\r\n'), 'CRLF');
});

test('unbracketed section timestamps are rejected', () => {
  assertViolation(SAMPLE.replace('## [00:03:40] ', '## 00:03:40 '), '5si4zkAngpA-style heading');
});

test('H1 differing from frontmatter title is rejected', () => {
  assertViolation(SAMPLE.replace('# Q3 Platform Demo — Billing Module', '# Some Other Title'), 'title/H1 split');
});

test('inline on-screen text is rejected (must be blockquote lines)', () => {
  assertViolation(
    SAMPLE.replace('**On screen:**\n> mdreel — Q3 Platform Demo', '**On screen:** mdreel — Q3 Platform Demo'),
    'KL7WBjAuTMg-style inline on-screen',
  );
});

test('blocks out of canonical order are rejected', () => {
  const section = /## \[00:09:12\] Wrap-up\n\n(\*\*Spoken:\*\* [^\n]+)\n\n(\*\*On screen:\*\*\n> [^\n]+)/;
  const m = section.exec(SAMPLE)!;
  assert.ok(m, 'test precondition: wrap-up section found');
  assertViolation(SAMPLE.replace(m[0], m[0].replace(m[1], '@@1').replace(m[2], m[1]).replace('@@1', m[2])), 'gRFaow12xo0-style order');
});

test('missing provenance section is rejected', () => {
  assertViolation(SAMPLE.slice(0, SAMPLE.lastIndexOf('\n---\n')) + '\n', 'no Source & licence');
});

test('legacy source_filename frontmatter key is rejected', () => {
  assertViolation(SAMPLE.replace('source: "demo-billing.mp4"', 'source_filename: "demo-billing.mp4"'), 'renamed at the freeze');
});

test('legacy generator values are rejected', () => {
  assertViolation(SAMPLE.replace('generator: "mdreel@0.0.0-fixture"', 'generator: "vectorreel-experiments/001@phase0.2"'), 'mdreel@<version> only');
});

test('renderDocument refuses non-canonical content it could not round-trip', () => {
  const doc = parseDocument(SAMPLE);
  const bad = structuredClone(doc);
  bad.sections[0].blocks[0].text = 'line one\n> looks like a quote';
  assert.throws(() => renderDocument(bad), OutputContractViolation);
});
