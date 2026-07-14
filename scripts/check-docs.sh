#!/usr/bin/env bash
# check-docs.sh — enforce "one fact, one home" across the living docs.
#
# METRICS.md is the sole home of every load-bearing number. Any other living doc may *reference*
# a number, but only on a line that cites METRICS.md — so the reader can always reach the
# authoritative value in one hop, and a stale copy cannot hide.
#
# Why this exists: on 2026-07-14 a superseded memo contradicted four living docs because the same
# figures had been copied into all of them and only some were updated. A number that lives in five
# files has five chances to be wrong and one chance to be right.
#
# Run: scripts/check-docs.sh   (part of the definition of done — CLAUDE.md rule 4)

set -uo pipefail
cd "$(dirname "$0")/.." || exit 2

OWNER="METRICS.md"
DOCS=(PLAN.md ARCHITECTURE.md BUSINESS_MODEL.md DISTRIBUTION.md CLAUDE.md DEVELOPMENT.md INFRA.md)

# Canonical values owned by METRICS.md. Extend this list whenever a new number becomes load-bearing.
PATTERNS=(
  '0\.65'          # N6  all-in COGS / video-hour
  '0\.011'         # N6  all-in COGS / video-minute
  '0\.45'          # N4  LLM subtotal   /  N4a public-path Stage B, default media resolution
  '0\.0075'        # N4  LLM subtotal / video-minute
  '0\.28'          # N4b public-path Stage B at MEDIA_RESOLUTION_LOW
  '0\.38'          # N4c public-path LLM subtotal — sizes N10
  '3\.80'          # N4d dense-content full-video cost — the ~13x blow-up
  '86 s'           # N7b under-segmentation on continuous screen recordings
  '258'            # §1.2b video tokenization rate; the coverage-guard denominator
  '63 retained'    # N1b job-replacement accounts
  '€131'           # N0  contribution per account
  '€390'           # N23 CAC ceiling
  '€4.73'          # N25 max viable CPC (upper)
  '2–3 retained'   # N1a survival accounts
  '400–1,000'      # N14 visitors per paying customer
  '9 months'       # T   the time-box deadline
  '2,000'          # N15 visitors to a verdict / N1a
  '20,000'         # N15b cumulative visitors to N1b
  '27,000'         # N16 TAM accounts
  '€65M'           # N16 TAM
  '2–5%'           # N12 visitor -> trial
  '5–15%'          # N13 trial -> paid
)

fail=0
for pat in "${PATTERNS[@]}"; do
  for doc in "${DOCS[@]}"; do
    [[ -f "$doc" ]] || continue
    # A hit is only OK if the same line cites the owner doc.
    while IFS=: read -r line_no line; do
      [[ -z "$line_no" ]] && continue
      if [[ "$line" != *"$OWNER"* ]]; then
        printf '%s:%s: restates a METRICS.md value (/%s/) without citing it\n    %s\n' \
          "$doc" "$line_no" "$pat" "$(echo "$line" | sed 's/^[[:space:]]*//' | cut -c1-100)"
        fail=1
      fi
    done < <(grep -nE "$pat" "$doc" 2>/dev/null)
  done
done

if [[ $fail -ne 0 ]]; then
  cat <<'EOF'

✗ One fact, one home (CLAUDE.md).
  Move the value into METRICS.md and reference it by name here — e.g.
  "sits well below break-even (METRICS.md N6)" — rather than copying the figure.
EOF
  exit 1
fi

echo "✓ docs consistent — no METRICS.md value restated elsewhere"
