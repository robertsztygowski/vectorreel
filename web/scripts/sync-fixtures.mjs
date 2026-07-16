#!/usr/bin/env node
// Mirrors tests/fixtures/output/ (the canonical Phase-2.5 contract fixtures) into
// web/fixtures/output/.
//
// Why this exists: `gcloud run deploy --source web` only packages the `web/` directory into the
// Docker build context. `tests/` is a sibling directory and is invisible to that build, so the
// fixtures the app reads at request time must live inside `web/`. tests/fixtures/output/ is the
// canonical copy (regenerated only via scripts/normalize-fixtures.mts when the contract changes);
// this script keeps web/fixtures/ a byte-identical mirror. Run manually after the canonical
// fixtures change — not part of the Docker build.
//
//   node scripts/sync-fixtures.mjs           # copy
//   node scripts/sync-fixtures.mjs --check   # verify the mirror, exit 1 on drift
import { existsSync, mkdirSync, readdirSync, readFileSync, copyFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const webRoot = join(__dirname, '..');
const repoRoot = join(webRoot, '..');
const sourceDir = join(repoRoot, 'tests', 'fixtures', 'output');
const destDir = join(webRoot, 'fixtures', 'output');
const checkMode = process.argv.includes('--check');

if (!existsSync(sourceDir)) {
  console.error(`Canonical fixtures not found under ${sourceDir}`);
  process.exit(1);
}

const files = readdirSync(sourceDir).filter(
  (file) => (file.endsWith('.md') || file.endsWith('.json')) && file !== 'README.md',
);

if (checkMode) {
  let drift = 0;
  for (const file of files) {
    const dest = join(destDir, file);
    if (!existsSync(dest) || !readFileSync(join(sourceDir, file)).equals(readFileSync(dest))) {
      console.error(`drift: fixtures/output/${file}`);
      drift += 1;
    }
  }
  const extra = existsSync(destDir) ? readdirSync(destDir).filter((f) => !files.includes(f)) : [];
  for (const file of extra) {
    console.error(`drift: fixtures/output/${file} has no canonical source`);
    drift += 1;
  }
  if (drift > 0) {
    console.error(`\n${drift} file(s) out of sync — run: node scripts/sync-fixtures.mjs`);
    process.exit(1);
  }
  console.log(`web/fixtures/output/ matches tests/fixtures/output/ (${files.length} files)`);
  process.exit(0);
}

mkdirSync(destDir, { recursive: true });
for (const file of files) {
  copyFileSync(join(sourceDir, file), join(destDir, file));
  console.log(`copied output/${file}`);
}
console.log('Fixtures synced. Commit web/fixtures/ alongside this change.');
