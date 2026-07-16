import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';
import { parseDocument, type OutputDocument } from './outputDocument';

// Server-only: reads web/fixtures/output/ (a committed mirror of tests/fixtures/output/, see
// scripts/sync-fixtures.mjs). Never import this module from a Client Component.
//
// Parsing is STRICT (Phase 2.5): the fixtures are canonical contract documents, so any parse
// failure here means the fixtures drifted from the contract — that must fail loudly, not be
// tolerated. lib/outputDocument.ts owns the grammar.

export type { OutputDocument, OutputFrontmatter, OutputSection, OutputBlock, BlockLabel } from './outputDocument';

export interface CorpusEntry {
  video_id: string;
  category: 'slide_talk' | 'talking_head' | 'screencast';
  url: string;
  title: string;
  channel: string;
  published_at: string;
  duration_s: number;
  licence: string;
  privacy: string;
  embeddable: boolean;
  attribution: string;
}

interface CorpusFile {
  accepted: CorpusEntry[];
  rejected: CorpusEntry[];
}

export interface VideoMeta {
  corpus: CorpusEntry;
  // parsed.frontmatter.title is the document's own (Stage-C editorial) title and equals the H1 by
  // contract. corpus.title is the YouTube title — a gallery display concern; do NOT reconcile.
  parsed: OutputDocument;
  raw: string;
}

export interface LicenceBlock {
  attribution: string;
  originalVideoUrl: string;
  licenceLine: string;
}

const FIXTURES_DIR = join(process.cwd(), 'fixtures', 'output');
const CORPUS_JSON_PATH = join(FIXTURES_DIR, 'corpus.json');

let corpusCache: CorpusFile | null = null;

function loadCorpusFile(): CorpusFile {
  if (!corpusCache) {
    corpusCache = JSON.parse(readFileSync(CORPUS_JSON_PATH, 'utf-8')) as CorpusFile;
  }
  return corpusCache;
}

export function loadCorpusIndex(): CorpusEntry[] {
  return loadCorpusFile().accepted;
}

export function findCorpusEntry(videoId: string): CorpusEntry | undefined {
  return loadCorpusIndex().find((entry) => entry.video_id === videoId);
}

function resolveMarkdownFilename(videoId: string): string {
  const files = readdirSync(FIXTURES_DIR);
  const match = files.find((f) => f.startsWith(`${videoId}_`) && f.endsWith('.md'));
  if (!match) throw new Error(`No corpus markdown fixture for video ${videoId}`);
  return match;
}

export function loadCorpusMarkdownRaw(videoId: string): string {
  const filename = resolveMarkdownFilename(videoId);
  return readFileSync(join(FIXTURES_DIR, filename), 'utf-8');
}

export function getVideoMeta(videoId: string): VideoMeta | undefined {
  const corpus = findCorpusEntry(videoId);
  if (!corpus) return undefined;
  const raw = loadCorpusMarkdownRaw(videoId);
  return { corpus, parsed: parseDocument(raw), raw };
}

// corpus.json's attribution string is verified identical (contracts.test.ts) to each file's own
// "## Source & licence" blockquote — corpus.json is the source of truth, so the licence block is
// rendered from it rather than re-parsed out of each markdown body.
export function getLicenceBlock(entry: CorpusEntry): LicenceBlock {
  return {
    attribution: entry.attribution,
    originalVideoUrl: entry.url,
    licenceLine: 'Creative Commons Attribution (CC BY) — https://creativecommons.org/licenses/by/3.0/',
  };
}
