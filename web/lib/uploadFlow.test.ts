import { test } from 'node:test';
import assert from 'node:assert/strict';
import { buildJobOptions, getUploadPutTarget, isVideoFile } from './uploadFlow';

test('isVideoFile accepts browser video types and common extension fallbacks', () => {
  assert.equal(isVideoFile({ name: 'demo.mp4', type: 'video/mp4' }), true);
  assert.equal(isVideoFile({ name: 'camera.MOV', type: '' }), true);
  assert.equal(isVideoFile({ name: 'notes.txt', type: 'text/plain' }), false);
});

test('getUploadPutTarget preserves gcs URLs and same-origin api paths', () => {
  assert.equal(
    getUploadPutTarget({
      storage: 'gcs',
      uploadUrl: 'https://storage.googleapis.com/bucket/object?signature=abc',
    }),
    'https://storage.googleapis.com/bucket/object?signature=abc',
  );

  assert.equal(
    getUploadPutTarget({
      storage: 'api',
      uploadUrl: 'https://api.mdreel.test/api/v1/uploads/up_123/bytes?token=abc',
    }),
    '/api/v1/uploads/up_123/bytes?token=abc',
  );
});

test('buildJobOptions maps the upload form to POST /jobs options', () => {
  assert.deepEqual(
    buildJobOptions({
      quality: 'high',
      retentionDays: 30,
      webhookUrl: '  https://example.test/hooks/mdreel  ',
      fail: true,
      filename: 'demo.mp4',
      durationSec: 89.6,
    }),
    {
      language_hint: 'auto',
      quality: 'high',
      retention_days: 30,
      webhook_url: 'https://example.test/hooks/mdreel',
      fail: true,
      filename: 'demo.mp4',
      durationSec: 90,
    },
  );

  assert.equal(
    buildJobOptions({
      quality: 'standard',
      retentionDays: 0,
      webhookUrl: '   ',
      fail: false,
      filename: 'demo.mp4',
      durationSec: 1,
    }).webhook_url,
    null,
  );
});
