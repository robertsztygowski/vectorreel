# experiments/ — the escape hatch

Prompt tuning, sampling-config benchmarks, cost measurements. Iteration speed over rigor.

Rules (DEVELOPMENT.md §9):

- **Any language.** Python notebooks welcome. No style, test, or strictness rules apply.
- **Never imported by product code.** Nothing under `src/` may reference this directory.
- **Secrets rules still apply** — no keys, `.env` stays gitignored. No escape hatch for that.
- When an experiment produces a decision (a prompt, a sampling config, a cost number),
  the result graduates into `src/` + `tests/fixtures/` + the relevant doc.
  The notebook may stay here as a record.
