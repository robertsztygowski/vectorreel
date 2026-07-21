#!/usr/bin/env bash
# Push a generated collection repository to the mdreel org, PRIVATE.
#
# 🚨 Visibility is never inferred and never flipped here. The repo is created private, and the
# script then ASSERTS it is private and fails loudly otherwise. Making a collection public is a
# founder decision taken after a licensing and attribution review — not a side effect of a script.
#
#   scripts/push-collection-repo.sh ai-engineering "AI Engineering — an mdreel collection"
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COLLECTION="${1:-}"
DESCRIPTION="${2:-An mdreel collection — AI-ready knowledge repository}"
ORG="${MDREEL_GITHUB_ORG:-mdreel}"
SOURCE_DIR="$REPO_ROOT/.local-state/repos/$COLLECTION"

if [[ -z "$COLLECTION" ]]; then
  echo "usage: scripts/push-collection-repo.sh <collection-slug> [description]" >&2
  exit 2
fi
if [[ ! -f "$SOURCE_DIR/metadata/manifest.json" ]]; then
  echo "no generated repository at $SOURCE_DIR — run scripts/generate-collection-repo.sh first" >&2
  exit 1
fi

# Derived content is CC BY 4.0 (DISTRIBUTION.md "GitHub distribution"); each source keeps its own
# licence, recorded per session in the manifest.
if [[ ! -f "$SOURCE_DIR/LICENSE" ]]; then
  cat > "$SOURCE_DIR/LICENSE" <<'EOF'
Content in this repository that mdreel generated (session documents, topic, speaker and timeline
indexes) is licensed under Creative Commons Attribution 4.0 International (CC BY 4.0).

Each source recording keeps its own licence and attribution, recorded per entry in
metadata/manifest.json and in each session document's "Source & licence" section. Reference-tier
entries are index metadata only: no content from those recordings is reproduced here.

https://creativecommons.org/licenses/by/4.0/
EOF
fi

cd "$SOURCE_DIR"
if [[ ! -d .git ]]; then
  git init -q -b main
fi
git add -A
git -c user.name="mdreel" -c user.email="hello@mdreel.com" commit -q -m "Regenerate collection from the §4b contract" || echo "nothing new to commit"

if gh repo view "$ORG/$COLLECTION" >/dev/null 2>&1; then
  echo "repo $ORG/$COLLECTION exists — pushing"
  git remote get-url origin >/dev/null 2>&1 || git remote add origin "https://github.com/$ORG/$COLLECTION.git"
  git push -u origin main --force-with-lease
else
  gh repo create "$ORG/$COLLECTION" --private --description "$DESCRIPTION" --source=. --push
fi

# The assertion. A repo that is not private at this point is a hard stop, not a warning.
VISIBILITY="$(gh repo view "$ORG/$COLLECTION" --json visibility --jq .visibility)"
if [[ "$VISIBILITY" != "PRIVATE" ]]; then
  echo "🚨 HARD STOP: $ORG/$COLLECTION is $VISIBILITY, expected PRIVATE." >&2
  exit 1
fi

echo "✓ $ORG/$COLLECTION pushed and verified PRIVATE"
