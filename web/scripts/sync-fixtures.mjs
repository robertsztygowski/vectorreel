#!/usr/bin/env node
// Mirrors experiments/001-gemini-video-benchmark/out/{corpus.json,corpus_md/*} into web/fixtures/.
//
// Why this exists: `gcloud run deploy --source web` only packages the `web/` directory into the
// Docker build context. `experiments/` is a sibling directory and is invisible to that build, so
// the fixtures the app reads at request time must live inside `web/`. `experiments/` stays the
// sole authorial source (PLAN.md: never rewritten in place); this script keeps `web/fixtures/` a
// byte-identical mirror. Run manually after the experiment output changes — not part of the
// Docker build.
import { existsSync, mkdirSync, readdirSync, copyFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const webRoot = join(__dirname, '..');
const repoRoot = join(webRoot, '..');
const sourceDir = join(repoRoot, 'experiments', '001-gemini-video-benchmark', 'out');
const sourceCorpusJson = join(sourceDir, 'corpus.json');
const sourceCorpusMd = join(sourceDir, 'corpus_md');
const destDir = join(webRoot, 'fixtures');
const destCorpusMd = join(destDir, 'corpus_md');

if (!existsSync(sourceCorpusJson) || !existsSync(sourceCorpusMd)) {
  console.error(`Source corpus not found under ${sourceDir}`);
  process.exit(1);
}

mkdirSync(destCorpusMd, { recursive: true });

copyFileSync(sourceCorpusJson, join(destDir, 'corpus.json'));
console.log('copied corpus.json');

for (const file of readdirSync(sourceCorpusMd)) {
  if (!file.endsWith('.md')) continue;
  copyFileSync(join(sourceCorpusMd, file), join(destCorpusMd, file));
  console.log(`copied corpus_md/${file}`);
}

console.log('Fixtures synced. Commit web/fixtures/ alongside this change.');
