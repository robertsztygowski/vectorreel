import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { Ajv2020 } from 'ajv/dist/2020';
import addFormats from 'ajv-formats';
import { Client } from 'pg';

export const API_BASE = process.env.E2E_API_BASE ?? 'http://localhost:8080';
export const WEB_BASE = process.env.E2E_WEB_BASE ?? 'http://localhost:3000';
export const PG_CONNECTION =
  process.env.E2E_PG_CONNECTION ?? 'postgres://dev:dev@localhost:5432/vectorreel';

export const REPO_ROOT = join(__dirname, '..', '..');
export const CONTRACTS_DIR = join(REPO_ROOT, 'tests', 'fixtures', 'contracts');
export const SAMPLE_VIDEO = join(
  REPO_ROOT,
  'tests',
  'fixtures',
  'videos',
  'talking_head_nasa_bolten.mp4',
);

/** Compiled validators for the frozen Phase-2.5 contracts (ARCHITECTURE §5). */
export function contractValidator(schemaFile: string) {
  const ajv = new Ajv2020({ allErrors: true, strict: false });
  addFormats(ajv);
  const schema = JSON.parse(readFileSync(join(CONTRACTS_DIR, schemaFile), 'utf-8'));
  return ajv.compile(schema);
}

export function assertValid(
  validate: ReturnType<typeof contractValidator>,
  payload: unknown,
  label: string,
): void {
  if (!validate(payload)) {
    throw new Error(`${label} violates its frozen contract:\n${JSON.stringify(validate.errors, null, 2)}`);
  }
}

/** One-shot Postgres query against the compose database (the runbook check, as code). */
export async function pgQuery<T extends Record<string, unknown>>(
  sql: string,
  params: unknown[] = [],
): Promise<T[]> {
  const client = new Client({ connectionString: PG_CONNECTION });
  await client.connect();
  try {
    const result = await client.query(sql, params as (string | number | null)[]);
    return result.rows as T[];
  } finally {
    await client.end();
  }
}

export function uniqueEmail(prefix: string): string {
  return `${prefix}+${Date.now()}${Math.random().toString(36).slice(2, 8)}@example.test`;
}
