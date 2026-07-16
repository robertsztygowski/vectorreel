# Canonical output-contract fixtures (frozen 2026-07-16, Phase 2.5)

Each `.md`/`.json` pair is one output document in the ratified contract form — the `.md` is the
Markdown contract (ARCHITECTURE.md §4), the `.json` its structured twin
(`../contracts/output.schema.json`). `web/lib/outputDocument.ts` is the executable grammar:
`render(parse(md))` is byte-identical to every `.md` here, enforced by `web/lib/contracts.test.ts`.

- The five video pairs are the Phase-0.2 CC BY corpus outputs
  (`experiments/001-gemini-video-benchmark/out/corpus_md/`, a frozen point-in-time record that is
  never rewritten), normalized into the contract by `web/scripts/normalize-fixtures.mts` —
  format-only changes; the content is untouched. Attribution ships inside each file's
  `## Source & licence` section and must stay identical to `corpus.json`'s `attribution` field.
- `private_sample.*` is the frozen private-upload sample; `web/lib/sampleOutput.ts` must
  reproduce it byte-for-byte for the fixed reference arguments.
- `corpus.json` is the gallery metadata index (licence audit trail from Phase 0.2).
- `generator: "mdreel@0.0.0-fixture"` is a reserved version — fixture bytes can never masquerade
  as real pipeline output.

`web/fixtures/` is a committed mirror of this directory (`web/scripts/sync-fixtures.mjs`) because
`gcloud run deploy --source web` only packages `web/`. Change fixtures only via the contract:
update ARCHITECTURE §4 + the schema + `outputDocument.ts` together, regenerate, re-sync.
