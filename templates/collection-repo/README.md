# Collection repo templates

Reusable files copied into every public collection repository
(`mdreel/<collection-slug>`). The operating model that governs them lives in
**DISTRIBUTION.md → "GitHub distribution — the operating model"**; the content
contract each collection repo must satisfy is **ARCHITECTURE.md §4b**
(validated by `web/lib/repository.test.ts`).

Contents:

- `.github/ISSUE_TEMPLATE/correction.yml` — "this line doesn't match the video"
- `.github/ISSUE_TEMPLATE/request.yml` — session / collection requests
- `.github/ISSUE_TEMPLATE/config.yml` — blank issues off, product contact link

These are templates, not live config for *this* repo — this repo's own issue
templates (if any) live in the top-level `.github/`.
