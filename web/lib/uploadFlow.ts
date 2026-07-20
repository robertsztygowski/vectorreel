export type UploadStorage = 'gcs' | 'api';
export type UploadQuality = 'standard' | 'high';
export type RetentionDays = 0 | 30;

export const DEFAULT_DURATION_SEC = 47 * 60 + 12;

const VIDEO_EXTENSIONS = new Set(['mp4', 'mov', 'm4v', 'webm', 'avi', 'mkv']);

export interface UploadCreatedResponse {
  uploadId: string;
  uploadUrl: string;
  storage: UploadStorage;
}

export function isVideoFile(file: Pick<File, 'name' | 'type'>): boolean {
  if (file.type.startsWith('video/')) return true;
  if (file.type) return false;

  const extension = file.name.split('.').pop()?.toLowerCase();
  return Boolean(extension && VIDEO_EXTENSIONS.has(extension));
}

export function getUploadPutTarget(upload: Pick<UploadCreatedResponse, 'uploadUrl' | 'storage'>): string {
  if (upload.storage === 'gcs') return upload.uploadUrl;

  const url = new URL(upload.uploadUrl);
  return `${url.pathname}${url.search}`;
}

export function buildJobOptions(args: {
  quality: UploadQuality;
  retentionDays: RetentionDays;
  webhookUrl: string;
  fail: boolean;
  filename: string;
  durationSec: number;
}) {
  const webhookUrl = args.webhookUrl.trim();
  return {
    language_hint: 'auto',
    quality: args.quality,
    retention_days: args.retentionDays,
    webhook_url: webhookUrl.length > 0 ? webhookUrl : null,
    fail: args.fail,
    filename: args.filename,
    durationSec: Math.round(args.durationSec),
  };
}
