namespace MdReel.Core.Domain;

/// <summary>
/// Turns provider-reported token usage into money, so the cost ledger carries euros and not just
/// call counts (CLAUDE.md rule 6).
///
/// 🚨 <b>Input is priced per modality, not blended.</b> Audio costs 3.3× video per token, and at
/// low media resolution — the setting internal production runs on — video tokens collapse while
/// audio tokens do not, so audio becomes the dominant input cost. A blended input rate would
/// misprice exactly the configuration we actually use, which is worse than not measuring at all:
/// it produces a confident wrong number. (ARCHITECTURE.md §8; the method is the one Phase 0 used
/// to measure METRICS.md N4a/N4b.)
///
/// Thinking tokens are billed as output. Hitting the thinking budget is not a failure
/// (PLAN.md Phase 0.1), but it is not free either.
/// </summary>
public static class LlmPricing
{
    /// <summary>USD per 1M tokens — Vertex <c>gemini-2.5-flash</c>, standard tier, verified 2026-07-14.</summary>
    private const decimal TextInPerMillionUsd = 0.30m;
    private const decimal VideoInPerMillionUsd = 0.30m;
    private const decimal AudioInPerMillionUsd = 1.00m;
    private const decimal OutputPerMillionUsd = 2.50m;

    /// <summary>USD → EUR. The ledger is denominated in euros because the P&amp;L is.</summary>
    private const decimal UsdToEur = 0.86m;

    /// <summary>
    /// Cost of one model call, in whole cents, rounded away from zero. A call that costs less than
    /// half a cent still returns 0 — see <see cref="Microcents"/> when precision matters, which it
    /// does when summing a batch of small calls.
    /// </summary>
    public static int Cents(LlmTokenUsage usage) =>
        (int)Math.Round(Microcents(usage) / 10_000m, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Cost in hundredths of a cent. Stage B fires many small calls per video; rounding each one to
    /// a whole cent first would quietly lose most of the bill, so the ledger sums micro-units.
    /// </summary>
    public static long Microcents(LlmTokenUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage);

        var usd =
            (usage.TextInputTokens * TextInPerMillionUsd
             + usage.VideoInputTokens * VideoInPerMillionUsd
             + usage.AudioInputTokens * AudioInPerMillionUsd
             + (usage.OutputTokens + usage.ThinkingTokens) * OutputPerMillionUsd)
            / 1_000_000m;

        return (long)Math.Round(usd * UsdToEur * 1_000_000m, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// Token usage for one model call, split the way the provider bills it.
/// </summary>
/// <remarks>
/// <c>TextInputTokens</c> folds in image and document modalities: they share the text price.
/// When a provider reports only a total prompt count with no modality breakdown, put it in
/// <c>TextInputTokens</c> — that under-prices audio-heavy calls, so treat such a reading as a
/// floor rather than a measurement.
/// </remarks>
public sealed record LlmTokenUsage(
    int TextInputTokens = 0,
    int VideoInputTokens = 0,
    int AudioInputTokens = 0,
    int OutputTokens = 0,
    int ThinkingTokens = 0);
