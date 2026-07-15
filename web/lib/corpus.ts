import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';
import matter from 'gray-matter';

// Server-only: reads web/fixtures/ (a committed mirror of experiments/001-*/out/, see
// scripts/sync-fixtures.mjs). Never import this module from a Client Component.

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

export interface Frontmatter {
  title: string;
  source_filename: string;
  duration: string;
  language: string;
  processed_at: string;
  generator: string;
  summary: string;
  tags: string[];
}

export type BlockLabel = 'spoken' | 'on_screen' | 'visual';

export interface Block {
  label: BlockLabel;
  text: string;
}

export interface Section {
  timestamp: string;
  heading: string;
  blocks: Block[];
}

export interface ParsedMarkdown {
  frontmatter: Frontmatter;
  // Verbatim from the file's own `# ` heading — kept separate from corpus.title because they
  // disagree on 3 of 5 files (frontmatter.title / corpus.title are editorially rewritten).
  // Do NOT reconcile the two; corpus.title is the display title everywhere in the app.
  h1: string;
  sections: Section[];
}

export interface VideoMeta {
  corpus: CorpusEntry;
  parsed: ParsedMarkdown;
  raw: string;
}

export interface LicenceBlock {
  attribution: string;
  originalVideoUrl: string;
  licenceLine: string;
}

const FIXTURES_DIR = join(process.cwd(), 'fixtures');
const CORPUS_JSON_PATH = join(FIXTURES_DIR, 'corpus.json');
const CORPUS_MD_DIR = join(FIXTURES_DIR, 'corpus_md');

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
  const files = readdirSync(CORPUS_MD_DIR);
  const match = files.find((f) => f.startsWith(`${videoId}_`) && f.endsWith('.md'));
  if (!match) throw new Error(`No corpus markdown fixture for video ${videoId}`);
  return match;
}

export function loadCorpusMarkdownRaw(videoId: string): string {
  const filename = resolveMarkdownFilename(videoId);
  return readFileSync(join(CORPUS_MD_DIR, filename), 'utf-8');
}

// Tolerant on purpose: the 5 real fixture files disagree on heading-bracket style, block-label
// order, and inline-vs-next-line content. See corpus.test.ts for the cases this must survive.
const SECTION_HEADING_RE = /^##\s+\[?(\d{2}:\d{2}:\d{2})\]?\s+(.+)$/;
const H1_RE = /^#\s+(.+)$/m;

const LABEL_PATTERNS: { label: BlockLabel; re: RegExp }[] = [
  { label: 'spoken', re: /^\*\*Spoken:\*\*\s?(.*)$/ },
  { label: 'on_screen', re: /^\*\*On screen:\*\*\s?(.*)$/ },
  { label: 'visual', re: /^\*\*Visual:\*\*\s?(.*)$/ },
];

function splitIntoHeadingChunks(content: string): { heading: string; body: string }[] {
  const lines = content.split('\n');
  const chunks: { heading: string; body: string }[] = [];
  let heading: string | null = null;
  let body: string[] = [];

  for (const line of lines) {
    if (line.startsWith('## ')) {
      if (heading !== null) chunks.push({ heading, body: body.join('\n') });
      heading = line;
      body = [];
    } else if (heading !== null) {
      body.push(line);
    }
  }
  if (heading !== null) chunks.push({ heading, body: body.join('\n') });
  return chunks;
}

function finalizeBlockText(text: string): string {
  return text
    .split('\n')
    .map((line) => line.replace(/^>\s?/, ''))
    .join('\n')
    .trim();
}

function parseSectionBody(body: string): Block[] {
  const lines = body.split('\n');
  const blocks: Block[] = [];
  let current: { label: BlockLabel; text: string } | null = null;

  for (const line of lines) {
    const hit = LABEL_PATTERNS.find(({ re }) => re.test(line));
    if (hit) {
      if (current) blocks.push({ label: current.label, text: finalizeBlockText(current.text) });
      const m = hit.re.exec(line)!;
      current = { label: hit.label, text: m[1] ?? '' };
      continue;
    }
    if (current) {
      current.text += `\n${line}`;
    }
  }
  if (current) blocks.push({ label: current.label, text: finalizeBlockText(current.text) });
  return blocks;
}

export function parseMarkdown(raw: string): ParsedMarkdown {
  // The committed fixtures use CRLF line endings; normalize before any line-based parsing (JS
  // regex `.`/`$` treat \r as a line terminator, so an un-normalized `\r` before the newline
  // silently breaks every heading/label match).
  const { data, content } = matter(raw.replace(/\r\n/g, '\n'));
  const frontmatter = data as Frontmatter;
  const h1Match = H1_RE.exec(content);
  const h1 = h1Match ? h1Match[1].trim() : frontmatter.title;

  const sections: Section[] = [];
  for (const { heading, body } of splitIntoHeadingChunks(content)) {
    const m = SECTION_HEADING_RE.exec(heading);
    if (!m) continue; // e.g. "## Source & licence" — not a timestamped section
    sections.push({ timestamp: m[1], heading: m[2].trim(), blocks: parseSectionBody(body) });
  }

  return { frontmatter, h1, sections };
}

export function getVideoMeta(videoId: string): VideoMeta | undefined {
  const corpus = findCorpusEntry(videoId);
  if (!corpus) return undefined;
  const raw = loadCorpusMarkdownRaw(videoId);
  return { corpus, parsed: parseMarkdown(raw), raw };
}

// corpus.json's attribution string is verified identical (corpus.test.ts) to each file's own
// "## Source & licence" blockquote — corpus.json is the source of truth, so the licence block is
// rendered from it rather than re-parsed out of each markdown body.
export function getLicenceBlock(entry: CorpusEntry): LicenceBlock {
  return {
    attribution: entry.attribution,
    originalVideoUrl: entry.url,
    licenceLine: 'Creative Commons Attribution (CC BY) — https://creativecommons.org/licenses/by/3.0/',
  };
}
