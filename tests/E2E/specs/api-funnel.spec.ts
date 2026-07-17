import { readFileSync } from 'node:fs';
import { test, expect, request as playwrightRequest, type APIRequestContext } from '@playwright/test';
import {
  API_BASE,
  SAMPLE_VIDEO,
  assertValid,
  contractValidator,
  pgQuery,
} from '../helpers';

// The frozen ARCHITECTURE §5 funnel against the real containerized API + Postgres:
// uploads → PUT bytes (committed CC clip, real Stage A ffmpeg in the container) → jobs →
// poll to done → output.md/.json (validated against tests/fixtures/contracts) → DELETE erasure.
// Deterministic: Stages B–D are the Phase-2R stubs; zero Vertex spend (CLAUDE.md rule 4 tier).

const uploadCreated = contractValidator('upload-created.schema.json');
const jobCreated = contractValidator('job-created.schema.json');
const jobStatus = contractValidator('job-status.schema.json');
const jobList = contractValidator('job-list.schema.json');
const outputSchema = contractValidator('output.schema.json');
const problemSchema = contractValidator('problem.schema.json');

async function apiContext(): Promise<APIRequestContext> {
  return playwrightRequest.newContext({
    baseURL: API_BASE,
    extraHTTPHeaders: { Authorization: 'Bearer e2e-test-token' },
  });
}

test('@smoke api health and web root respond', async ({ page }) => {
  const api = await apiContext();
  const health = await api.get('/health');
  expect(health.status()).toBe(200);
  expect(await health.json()).toEqual({ status: 'ok' });

  await page.goto('/');
  await expect(page.locator('body')).toContainText(/markdown/i);
  await api.dispose();
});

test('@smoke full job funnel: upload → job → output contracts → erasure', async () => {
  const api = await apiContext();

  // 1. Create upload (frozen upload-created contract).
  const uploadRes = await api.post('/api/v1/uploads');
  expect(uploadRes.status()).toBe(201);
  const upload = await uploadRes.json();
  assertValid(uploadCreated, upload, 'POST /uploads response');

  // 2. PUT the committed 90-second CC clip — the container runs real ffmpeg Stage A on it.
  const putRes = await api.put(new URL(upload.uploadUrl).pathname + new URL(upload.uploadUrl).search, {
    data: readFileSync(SAMPLE_VIDEO),
  });
  expect(putRes.status()).toBe(204);

  // 3. Create the job (frozen job-created contract).
  const jobRes = await api.post('/api/v1/jobs', {
    data: { uploadId: upload.uploadId, options: { filename: 'talking_head_nasa_bolten.mp4' } },
  });
  expect(jobRes.status()).toBe(202);
  const job = await jobRes.json();
  assertValid(jobCreated, job, 'POST /jobs response');

  // 4. Poll to done; every intermediate body must satisfy the frozen job-status contract.
  let status: { status: string; stage?: string } = { status: 'queued' };
  await expect
    .poll(
      async () => {
        const res = await api.get(`/api/v1/jobs/${job.jobId}`);
        expect(res.status()).toBe(200);
        status = await res.json();
        assertValid(jobStatus, status, `GET /jobs/${job.jobId} response`);
        return status.status;
      },
      { timeout: 90_000, intervals: [1_000] },
    )
    .toBe('done');

  // 5. The job appears in the frozen list contract.
  const listRes = await api.get('/api/v1/jobs');
  const list = await listRes.json();
  assertValid(jobList, list, 'GET /jobs response');
  expect(list.jobs.map((j: { jobId: string }) => j.jobId)).toContain(job.jobId);

  // 6. output.json validates against the frozen output contract; output.md round-trips the
  //    frontmatter shape (ARCHITECTURE §4).
  const outputJsonRes = await api.get(`/api/v1/jobs/${job.jobId}/output.json`);
  expect(outputJsonRes.status()).toBe(200);
  assertValid(outputSchema, await outputJsonRes.json(), 'output.json');

  const outputMdRes = await api.get(`/api/v1/jobs/${job.jobId}/output.md`);
  expect(outputMdRes.status()).toBe(200);
  const markdown = await outputMdRes.text();
  expect(markdown.startsWith('---\n')).toBe(true);
  expect(markdown).toContain('source: "talking_head_nasa_bolten.mp4"');
  expect(markdown).toContain('## Source & licence');

  // 7. Stage A metered the job in the Postgres cost ledger (CLAUDE.md rule 6 — every compute step).
  const ledger = await pgQuery<{ step: string }>(
    'select step from usage_ledger where job_id = $1 order by step',
    [job.jobId],
  );
  expect(ledger.map((r) => r.step)).toEqual(['stage_a.probe', 'stage_a.scan']);

  // 8. DELETE = GDPR erasure; a second DELETE must be a problem+json 404.
  const deleteRes = await api.delete(`/api/v1/jobs/${job.jobId}`);
  expect(deleteRes.status()).toBe(204);

  const goneRes = await api.delete(`/api/v1/jobs/${job.jobId}`);
  expect(goneRes.status()).toBe(404);
  assertValid(problemSchema, await goneRes.json(), 'DELETE 404 problem');

  await api.dispose();
});

test('failed jobs surface as failed, not fake-green', async () => {
  const api = await apiContext();

  const upload = await (await api.post('/api/v1/uploads')).json();
  await api.put(new URL(upload.uploadUrl).pathname + new URL(upload.uploadUrl).search, {
    data: readFileSync(SAMPLE_VIDEO),
  });
  const job = await (
    await api.post('/api/v1/jobs', {
      data: { uploadId: upload.uploadId, options: { filename: 'boom.mp4', fail: true } },
    })
  ).json();

  await expect
    .poll(
      async () => (await (await api.get(`/api/v1/jobs/${job.jobId}`)).json()).status,
      { timeout: 60_000, intervals: [1_000] },
    )
    .toBe('failed');

  const outputRes = await api.get(`/api/v1/jobs/${job.jobId}/output.md`);
  expect(outputRes.status()).toBe(409);
  assertValid(problemSchema, await outputRes.json(), 'output for unfinished job');
  await api.dispose();
});

test('api requires auth outside the documented exceptions', async () => {
  const anonymous = await playwrightRequest.newContext({ baseURL: API_BASE });
  const res = await anonymous.get('/api/v1/jobs');
  expect(res.status()).toBe(401);
  assertValid(problemSchema, await res.json(), '401 problem');
  await anonymous.dispose();
});
