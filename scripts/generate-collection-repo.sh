#!/usr/bin/env bash
# Render a §4b repository from the licence-verified corpus plus whatever the batch has produced.
#
# Builds from the INTERSECTION of the two on purpose: the corpus lists what we intended to produce,
# the bucket holds what we did. A collection that lists sessions it does not contain is worse than a
# smaller one, so missing sources are reported and left out rather than half-linked.
#
#   scripts/generate-collection-repo.sh ai-engineering "AI Engineering" "Production RAG, agents, ..."
#   scripts/generate-collection-repo.sh dotnet ".NET" "..." --scaffold
#
# Nothing here pushes. Publishing is a separate, deliberate step.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

COLLECTION="${1:-}"; NAME="${2:-}"; DESCRIPTION="${3:-}"
if [[ -z "$COLLECTION" || -z "$NAME" ]]; then
  echo "usage: scripts/generate-collection-repo.sh <slug> <name> [description] [--scaffold] [--corpus PATH]" >&2
  exit 2
fi
shift 3 2>/dev/null || shift $#

SCAFFOLD=false
CORPUS="experiments/006-collections-strategy/out/corpus.json"
TARGET=".local-state/repos/$COLLECTION"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --scaffold) SCAFFOLD=true; shift ;;
    --corpus)   CORPUS="$2"; shift 2 ;;
    --target)   TARGET="$2"; shift 2 ;;
    *) echo "unknown flag: $1" >&2; exit 2 ;;
  esac
done

to_native() { command -v cygpath >/dev/null 2>&1 && cygpath -w "$1" || printf '%s' "$1"; }
mkdir -p "$REPO_ROOT/$TARGET"

export RepositoryBuild__Enabled=true
export RepositoryBuild__Collection="$COLLECTION"
export RepositoryBuild__Name="$NAME"
export RepositoryBuild__Description="$DESCRIPTION"
export RepositoryBuild__CorpusPath="$(to_native "$REPO_ROOT/$CORPUS")"
export RepositoryBuild__ProducedRoot="$(to_native "$REPO_ROOT/.local-state/collections")"
export RepositoryBuild__TargetDirectory="$(to_native "$REPO_ROOT/$TARGET")"
export RepositoryBuild__ScaffoldOnly="$SCAFFOLD"
# Rendering makes no model calls and no network calls — keep the pipeline seams inert.
export PipelineModel__Mode=fake

echo "collection : $COLLECTION ($NAME)"
echo "corpus     : $CORPUS"
echo "target     : $TARGET"
echo "scaffold   : $SCAFFOLD"
echo

# Separate build output so this can run while a production batch is mid-flight: the running Worker
# holds a lock on the default bin/, and generating a repository from what has landed so far is
# exactly something you want to do without stopping the batch.
dotnet run --project src/Worker --no-launch-profile \
  --property:BaseOutputPath="$(to_native "$REPO_ROOT/.local-state/build/repo-builder/")"

# The repo conventions that are not generated: issue forms, licence, and the CHANGELOG that acts as
# freshness proof (DISTRIBUTION.md "GitHub distribution").
mkdir -p "$REPO_ROOT/$TARGET/.github/ISSUE_TEMPLATE"
if [[ -d "$REPO_ROOT/templates/collection-repo" ]]; then
  cp -r "$REPO_ROOT/templates/collection-repo/." "$REPO_ROOT/$TARGET/.github/ISSUE_TEMPLATE/" 2>/dev/null || true
fi
# §4b portability is LF-only. The renderer guarantees that in the bytes it writes, but a clone on a
# machine with core.autocrlf=true would rewrite them on checkout — so the repository states the rule
# itself rather than depending on every reader's git config.
cat > "$REPO_ROOT/$TARGET/.gitattributes" <<'EOF'
# ARCHITECTURE.md §4b: LF, always, everywhere. The repository must render identically on GitHub,
# Obsidian, VS Code and a bare file tree, on any platform.
* text eol=lf
EOF

[[ -f "$REPO_ROOT/$TARGET/CHANGELOG.md" ]] || cat > "$REPO_ROOT/$TARGET/CHANGELOG.md" <<'EOF'
# Changelog

Every batch that lands in this collection is recorded here. The changelog is the freshness proof —
a reader can tell at a glance whether this repository is maintained or abandoned.
EOF

echo
echo "written to $TARGET"
