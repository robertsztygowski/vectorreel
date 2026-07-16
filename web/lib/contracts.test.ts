// The Phase-2.5 freeze proof: every committed fixture validates against the normative schemas,
// the Markdown and JSON twins are the same document, the canonical renderer reproduces the
// Markdown byte-for-byte, and the mock API routes emit exactly the frozen response shapes.
// If any of this fails, either the contract changed (update ARCHITECTURE §4/§5 + schemas +
// regenerate fixtures together) or something drifted — never loosen a test to make it pass.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';
import { Ajv2020, type ValidateFunction } from 'ajv/dist/2020';
import addFormats from 'ajv-formats';
import { parseDocument, renderDocument } from './outputDocument';
import { buildSampleOutput } from './sampleOutput';
import { computeStatus, encodeJobToken, type JobToken } from './mockJobs';

const FIXTURES_DIR = join(process.cwd(), 'fixtures', 'output');
const CONTRACTS_DIR = join(process.cwd(), '..', 'tests', 'fixtures', 'contracts');

const ajv = new Ajv2020({ allErrors: true, strict: true });
addFormats(ajv);

function loadSchema(name: string): ValidateFunction {
  return ajv.compile(JSON.parse(readFileSync(join(CONTRACTS_DIR, `${name}.schema.json`), 'utf-8')));
}

const validateOutput = loadSchema('output');
const validateUploadCreated = loadSchema('upload-created');
const validateJobCreated = loadSchema('job-created');
const validateJobStatus = loadSchema('job-status');
const validateJobList = loadSchema('job-list');
const validateProblem = loadSchema('problem');

function assertValid(validate: ValidateFunction, data: unknown, what: string) {
  assert.ok(validate(data), `${what}: ${JSON.stringify(validate.errors, null, 2)}`);
}

// --- fixture pairs: schema-valid, twins agree, renderer reproduces the bytes ---

const mdFiles = readdirSync(FIXTURES_DIR).filter((f) => f.endsWith('.md'));

test('fixture directory has the expected shape', () => {
  assert.equal(mdFiles.length, 6, 'five corpus documents + the private sample');
  for (const md of mdFiles) {
    assert.ok(
      readdirSync(FIXTURES_DIR).includes(md.replace(/\.md$/, '.json')),
      `${md} has a .json twin`,
    );
  }
});

for (const md of mdFiles) {
  test(`${md}: schema-valid, twins agree, byte round-trip`, () => {
    const raw = readFileSync(join(FIXTURES_DIR, md), 'utf-8');
    const twin = JSON.parse(readFileSync(join(FIXTURES_DIR, md.replace(/\.md$/, '.json')), 'utf-8'));

    assertValid(validateOutput, twin, `${md} json twin`);

    const parsed = parseDocument(raw);
    assert.deepEqual(parsed, twin, 'parse(md) equals the committed .json twin');
    assert.equal(renderDocument(parsed), raw, 'render(parse(md)) is byte-identical to the .md');
  });
}

test('web/fixtures/output/ mirrors tests/fixtures/output/ byte-for-byte', () => {
  const canonicalDir = join(process.cwd(), '..', 'tests', 'fixtures', 'output');
  const canonical = readdirSync(canonicalDir).filter((f) => f !== 'README.md');
  const mirrored = readdirSync(FIXTURES_DIR);
  assert.deepEqual(mirrored.sort(), canonical.sort(), 'same file set (run: npm run sync-fixtures)');
  for (const file of canonical) {
    assert.ok(
      readFileSync(join(canonicalDir, file)).equals(readFileSync(join(FIXTURES_DIR, file))),
      `${file} is in sync (run: npm run sync-fixtures)`,
    );
  }
});

// --- the runtime mock is frozen to the fixture ---

test('buildSampleOutput(reference args) is byte-identical to private_sample.md', () => {
  const rendered = buildSampleOutput({
    sourceFilename: 'demo-billing.mp4',
    durationSec: 2832,
    processedAt: '2026-07-14T10:22:00Z',
  });
  assert.equal(rendered, readFileSync(join(FIXTURES_DIR, 'private_sample.md'), 'utf-8'));
});

// --- mock API routes emit the frozen response shapes ---

function token(overrides: Partial<JobToken> = {}): JobToken {
  return { ref: 'up_test', createdAtMs: Date.now(), meta: { filename: 'demo.mp4', durationSec: 90 }, ...overrides };
}

test('computeStatus emits job-status shapes across the whole lifecycle', () => {
  const now = Date.now();
  assertValid(validateJobStatus, computeStatus(token({ createdAtMs: now })), 'queued');
  assertValid(validateJobStatus, computeStatus(token({ createdAtMs: now - 5000 })), 'processing');
  assertValid(validateJobStatus, computeStatus(token({ createdAtMs: now - 60_000 })), 'done');
  assertValid(validateJobStatus, computeStatus(token({ createdAtMs: now - 60_000, fail: true })), 'failed');
});

test('POST /uploads response matches upload-created', async () => {
  const { POST } = await import('../app/api/uploads/route');
  const res = await POST();
  assert.equal(res.status, 201);
  assertValid(validateUploadCreated, await res.json(), 'upload-created');
});

test('POST /jobs responses match job-created and problem', async () => {
  const { POST } = await import('../app/api/jobs/route');
  const created = await POST(new Request('http://mock/api/jobs', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ uploadId: 'up_test', options: { filename: 'demo.mp4', durationSec: 90 } }),
  }));
  assert.equal(created.status, 202);
  assertValid(validateJobCreated, await created.json(), 'job-created');

  const rejected = await POST(new Request('http://mock/api/jobs', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({}),
  }));
  assert.equal(rejected.status, 400);
  assert.equal(rejected.headers.get('Content-Type'), 'application/problem+json');
  assertValid(validateProblem, await rejected.json(), 'problem');
});

test('GET /jobs response matches job-list', async () => {
  const { GET } = await import('../app/api/jobs/route');
  const res = await GET();
  assert.equal(res.status, 200);
  const body = await res.json();
  assertValid(validateJobList, body, 'job-list');
  assert.equal(body.jobs.length, 5, 'the mock library backs the list');
});

test('GET /jobs/{id} + DELETE /jobs/{id} match job-status / 204 / problem', async () => {
  const { GET, DELETE } = await import('../app/api/jobs/[id]/route');
  const id = encodeJobToken(token());
  const params = (value: string) => ({ params: Promise.resolve({ id: value }) });

  const status = await GET(new Request('http://mock'), params(id));
  assert.equal(status.status, 200);
  assertValid(validateJobStatus, await status.json(), 'job-status');

  const erased = await DELETE(new Request('http://mock'), params(id));
  assert.equal(erased.status, 204);
  assert.equal(await erased.text(), '', '204 has no body');

  const missing = await DELETE(new Request('http://mock'), params('not-a-job'));
  assert.equal(missing.status, 404);
  assertValid(validateProblem, await missing.json(), 'problem');
});

test('GET /jobs/{id}/output.json returns a schema-valid contract document', async () => {
  const { GET } = await import('../app/api/jobs/[id]/output.json/route');
  const id = encodeJobToken(token({ createdAtMs: Date.now() - 60_000 }));
  const res = await GET(new Request('http://mock'), { params: Promise.resolve({ id }) });
  assert.equal(res.status, 200);
  const doc = await res.json();
  assertValid(validateOutput, doc, 'output document');
  assert.equal(doc.frontmatter.source, 'demo.mp4', 'source carries the upload filename');
});
