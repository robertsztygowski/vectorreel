// The output document contract — reference implementation (frozen 2026-07-16, Phase 2.5).
//
// ARCHITECTURE §4 owns the human-readable contract; tests/fixtures/contracts/output.schema.json
// is the normative schema for the JSON form. This module is the executable grammar for the
// Markdown form: parseDocument() accepts ONLY canonical documents (it throws on any deviation, so
// fixture drift fails loudly), and renderDocument() emits the one canonical byte form —
// render(parse(md)) must be byte-identical to md. The Phase 3 Stage D renderer must reproduce
// renderDocument()'s output exactly.

export class OutputContractViolation extends Error {
  constructor(message: string) {
    super(`Output contract violation: ${message}`);
    this.name = 'OutputContractViolation';
  }
}

export interface OutputFrontmatter {
  title: string;
  /** Original upload filename (private path) or canonical source URL (internal ingest). */
  source: string;
  /** Video duration as "hh:mm:ss". */
  duration: string;
  language: string;
  /** UTC ISO-8601, e.g. "2026-07-14T10:22:00Z". */
  processed_at: string;
  /** "mdreel@<version>"; committed fixtures use "mdreel@0.0.0-fixture". */
  generator: string;
  summary: string;
  tags: string[];
}

export type BlockLabel = 'spoken' | 'on_screen' | 'visual';

export interface OutputBlock {
  label: BlockLabel;
  /**
   * Multi-line text uses "\n" (dialogue turns, one on-screen capture per line). Never contains
   * blank lines, "\r", or a line starting with ">", "#", or a block label marker.
   */
  text: string;
}

export interface OutputSection {
  /** "hh:mm:ss", strictly ascending across sections. */
  timestamp: string;
  heading: string;
  /** 1–3 blocks, each label at most once, in the order spoken → on_screen → visual. */
  blocks: OutputBlock[];
}

export interface OutputDocument {
  frontmatter: OutputFrontmatter;
  sections: OutputSection[];
  /** Markdown body of the final "## Source & licence" section (attribution or retention note). */
  provenance: string;
}

const FRONTMATTER_KEYS = [
  'title',
  'source',
  'duration',
  'language',
  'processed_at',
  'generator',
  'summary',
  'tags',
] as const;

const TIMESTAMP_RE = /^\d{2}:[0-5]\d:[0-5]\d$/;
const PROCESSED_AT_RE = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d{1,3})?Z$/;
const GENERATOR_RE = /^mdreel@[0-9A-Za-z.+-]+$/;
const TAG_RE = /^[a-z0-9][a-z0-9 -]*$/;
const SECTION_HEADING_RE = /^## \[(\d{2}:\d{2}:\d{2})\] (\S.*)$/;
const PROVENANCE_HEADING = '## Source & licence';

const LABEL_MARKERS: { label: BlockLabel; marker: string; inline: boolean }[] = [
  { label: 'spoken', marker: '**Spoken:**', inline: true },
  { label: 'on_screen', marker: '**On screen:**', inline: false },
  { label: 'visual', marker: '**Visual:**', inline: true },
];

function fail(message: string): never {
  throw new OutputContractViolation(message);
}

function timestampToSeconds(ts: string): number {
  const [h, m, s] = ts.split(':').map(Number);
  return h * 3600 + m * 60 + s;
}

export function validateFrontmatter(fm: unknown): asserts fm is OutputFrontmatter {
  if (typeof fm !== 'object' || fm === null) fail('frontmatter is not an object');
  const obj = fm as Record<string, unknown>;
  const keys = Object.keys(obj);
  for (const key of FRONTMATTER_KEYS) {
    if (!(key in obj)) fail(`frontmatter is missing "${key}"`);
  }
  for (const key of keys) {
    if (!(FRONTMATTER_KEYS as readonly string[]).includes(key)) {
      fail(`frontmatter has unknown key "${key}"`);
    }
  }
  for (const key of FRONTMATTER_KEYS) {
    if (key === 'tags') continue;
    const value = obj[key];
    if (typeof value !== 'string' || value.length === 0) {
      fail(`frontmatter "${key}" must be a non-empty string`);
    }
  }
  if (!TIMESTAMP_RE.test(obj.duration as string)) {
    fail(`frontmatter duration "${obj.duration}" is not "hh:mm:ss"`);
  }
  if (!PROCESSED_AT_RE.test(obj.processed_at as string)) {
    fail(`frontmatter processed_at "${obj.processed_at}" is not UTC ISO-8601`);
  }
  if (!GENERATOR_RE.test(obj.generator as string)) {
    fail(`frontmatter generator "${obj.generator}" does not match "mdreel@<version>"`);
  }
  const tags = obj.tags;
  if (!Array.isArray(tags) || tags.length === 0) fail('frontmatter tags must be a non-empty array');
  for (const tag of tags) {
    if (typeof tag !== 'string' || !TAG_RE.test(tag)) {
      fail(`tag ${JSON.stringify(tag)} is not a lowercase slug`);
    }
  }
}

function validateBlockText(label: BlockLabel, text: string): string[] {
  if (text.length === 0) fail(`empty "${label}" block`);
  if (text.includes('\r')) fail(`"${label}" block contains a carriage return`);
  const inline = label !== 'on_screen';
  const lines = text.split('\n');
  for (const line of lines) {
    if (line.trim().length === 0) fail(`"${label}" block contains a blank line`);
    // on_screen is verbatim screen content and renders blockquote-escaped ("> " prefix), so any
    // content is unambiguous there. Inline blocks (spoken/visual) render bare lines, where these
    // prefixes would collide with the document grammar itself.
    if (inline) {
      if (line.startsWith('>') || line.startsWith('#') || line === '---') {
        fail(`"${label}" block line starts with reserved marker: ${JSON.stringify(line.slice(0, 20))}`);
      }
      if (LABEL_MARKERS.some(({ marker }) => line.startsWith(marker))) {
        fail(`"${label}" block line starts with a block label marker`);
      }
    }
  }
  return lines;
}

function validateSections(sections: OutputSection[]): void {
  if (sections.length === 0) fail('document has no timestamped sections');
  let prevSeconds = -1;
  for (const section of sections) {
    if (!TIMESTAMP_RE.test(section.timestamp)) {
      fail(`section timestamp "${section.timestamp}" is not "hh:mm:ss"`);
    }
    const seconds = timestampToSeconds(section.timestamp);
    if (seconds <= prevSeconds) {
      fail(`section timestamps not strictly ascending at [${section.timestamp}]`);
    }
    prevSeconds = seconds;
    if (section.heading.trim().length === 0) {
      fail(`section [${section.timestamp}] has an empty heading`);
    }
    if (section.heading !== section.heading.trim()) {
      fail(`section [${section.timestamp}] heading has surrounding whitespace`);
    }
    if (section.blocks.length === 0) fail(`section [${section.timestamp}] has no blocks`);
    let prevLabelIndex = -1;
    for (const block of section.blocks) {
      const labelIndex = LABEL_MARKERS.findIndex(({ label }) => label === block.label);
      if (labelIndex < 0) fail(`unknown block label "${block.label}"`);
      if (labelIndex <= prevLabelIndex) {
        fail(
          `section [${section.timestamp}] blocks out of order — each label at most once, ` +
            'in the order spoken, on_screen, visual',
        );
      }
      prevLabelIndex = labelIndex;
      validateBlockText(block.label, block.text);
    }
  }
}

function parseFrontmatterYaml(lines: string[]): OutputFrontmatter {
  // The canonical frontmatter is a fixed set of double-quoted scalars plus one flow list, so it
  // is parsed directly rather than through a YAML library — anything a general parser would
  // accept beyond this grammar is a contract violation anyway.
  const obj: Record<string, unknown> = {};
  for (const line of lines) {
    const m = /^([a-z_]+): (.+)$/.exec(line);
    if (!m) fail(`malformed frontmatter line: ${JSON.stringify(line)}`);
    const [, key, value] = m;
    if (key in obj) fail(`duplicate frontmatter key "${key}"`);
    if (key === 'tags') {
      const list = /^\[(.+)\]$/.exec(value);
      if (!list) fail('frontmatter tags must be a flow list: [a, b, c]');
      obj.tags = list[1].split(', ');
    } else {
      if (!value.startsWith('"') || !value.endsWith('"')) {
        fail(`frontmatter "${key}" must be a double-quoted string`);
      }
      try {
        obj[key] = JSON.parse(value);
      } catch {
        fail(`frontmatter "${key}" is not a valid quoted string`);
      }
    }
  }
  validateFrontmatter(obj);
  return obj;
}

function quote(value: string): string {
  return JSON.stringify(value);
}

function renderFrontmatter(fm: OutputFrontmatter): string[] {
  return [
    '---',
    `title: ${quote(fm.title)}`,
    `source: ${quote(fm.source)}`,
    `duration: ${quote(fm.duration)}`,
    `language: ${quote(fm.language)}`,
    `processed_at: ${quote(fm.processed_at)}`,
    `generator: ${quote(fm.generator)}`,
    `summary: ${quote(fm.summary)}`,
    `tags: [${fm.tags.join(', ')}]`,
    '---',
  ];
}

export function renderDocument(doc: OutputDocument): string {
  validateFrontmatter(doc.frontmatter);
  validateSections(doc.sections);
  const provenance = doc.provenance;
  if (provenance.trim().length === 0) fail('provenance section is empty');
  if (provenance !== provenance.trim()) fail('provenance has surrounding whitespace');
  if (provenance.includes('\r')) fail('provenance contains a carriage return');

  const out: string[] = [...renderFrontmatter(doc.frontmatter), ''];
  out.push(`# ${doc.frontmatter.title}`);

  for (const section of doc.sections) {
    out.push('', `## [${section.timestamp}] ${section.heading}`);
    for (const block of section.blocks) {
      out.push('');
      const lines = block.text.split('\n');
      const marker = LABEL_MARKERS.find(({ label }) => label === block.label)!;
      if (marker.inline) {
        out.push(`${marker.marker} ${lines[0]}`, ...lines.slice(1));
      } else {
        out.push(marker.marker, ...lines.map((line) => `> ${line}`));
      }
    }
  }

  out.push('', '---', '', PROVENANCE_HEADING, '', provenance, '');
  return out.join('\n');
}

export function parseDocument(raw: string): OutputDocument {
  if (raw.includes('\r')) fail('document contains CRLF line endings — the contract is LF-only');
  if (!raw.endsWith('\n')) fail('document does not end with a newline');
  const lines = raw.split('\n');
  let i = 0;

  const expect = (want: string, what: string) => {
    if (lines[i] !== want) {
      fail(`expected ${what} at line ${i + 1}, got ${JSON.stringify(lines[i] ?? '<eof>')}`);
    }
    i += 1;
  };

  expect('---', 'frontmatter opening "---"');
  const fmStart = i;
  while (i < lines.length && lines[i] !== '---') i += 1;
  if (i >= lines.length) fail('frontmatter closing "---" not found');
  const frontmatter = parseFrontmatterYaml(lines.slice(fmStart, i));
  i += 1; // past closing ---

  expect('', 'blank line after frontmatter');
  expect(`# ${frontmatter.title}`, 'H1 equal to frontmatter title');

  const sections: OutputSection[] = [];
  let provenance: string | null = null;

  while (provenance === null) {
    expect('', 'blank line before section, "---", or end');
    const line = lines[i];
    if (line === undefined) fail('document ends without a "## Source & licence" section');

    if (line === '---') {
      i += 1;
      expect('', 'blank line after provenance "---"');
      expect(PROVENANCE_HEADING, `"${PROVENANCE_HEADING}" heading`);
      expect('', 'blank line after provenance heading');
      provenance = lines.slice(i).join('\n').replace(/\n$/, '');
      if (provenance.trim().length === 0) fail('provenance section is empty');
      if (provenance !== provenance.trim()) fail('extra blank lines around the provenance section');
      break;
    }

    const headingMatch = SECTION_HEADING_RE.exec(line);
    if (!headingMatch) {
      fail(`expected "## [hh:mm:ss] Heading" or "---" at line ${i + 1}, got ${JSON.stringify(line)}`);
    }
    i += 1;
    const section: OutputSection = {
      timestamp: headingMatch[1],
      heading: headingMatch[2],
      blocks: [],
    };

    // Blocks: each preceded by exactly one blank line; the section ends at the blank line that
    // precedes the next "## " heading or the provenance "---".
    while (lines[i] === '' && lines[i + 1] !== undefined && lines[i + 1] !== '---' && !lines[i + 1].startsWith('## ')) {
      i += 1;
      const labelLine = lines[i];
      const marker = LABEL_MARKERS.find(({ marker: m }) => labelLine.startsWith(m));
      if (!marker) {
        fail(`expected a block label at line ${i + 1}, got ${JSON.stringify(labelLine)}`);
      }
      const textLines: string[] = [];
      if (marker.inline) {
        if (!labelLine.startsWith(`${marker.marker} `) || labelLine.length <= marker.marker.length + 1) {
          fail(`"${marker.marker}" must be followed by text on the same line (line ${i + 1})`);
        }
        textLines.push(labelLine.slice(marker.marker.length + 1));
        i += 1;
        while (i < lines.length && lines[i] !== '') {
          textLines.push(lines[i]);
          i += 1;
        }
      } else {
        if (labelLine !== marker.marker) {
          fail(`"${marker.marker}" must stand alone on its line (line ${i + 1})`);
        }
        i += 1;
        while (i < lines.length && lines[i] !== '') {
          if (!lines[i].startsWith('> ')) {
            fail(`on_screen content must be "> " blockquote lines (line ${i + 1})`);
          }
          textLines.push(lines[i].slice(2));
          i += 1;
        }
      }
      section.blocks.push({ label: marker.label, text: textLines.join('\n') });
    }

    sections.push(section);
  }

  validateSections(sections);
  return { frontmatter, sections, provenance };
}
