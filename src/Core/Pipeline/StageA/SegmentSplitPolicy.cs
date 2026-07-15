using MdReel.Core.Domain;

namespace MdReel.Core.Pipeline.StageA;

/// <summary>
/// What Stage B does when a segment overflows the model's output cap.
///
/// 🚨 <b>On <c>MAX_TOKENS</c>, halve the segment — never retry it unchanged.</b> An output-cap overflow
/// is <b>deterministic</b>: the same clip at the same config overflows again, identically. A retry buys
/// nothing and bills twice, and Phase 0.2 paid for exactly that on both slide talks (~€0.70 for zero
/// output). What the call needs is a smaller window, not another attempt.
///
/// <para>⚠️ And halving is a <b>cost amplifier</b>, because you pay for the failed parent call and both
/// halves. Driven naively it turned a 29-minute talk into 22 Stage B calls (METRICS.md N4d — ~13× the
/// normal cost, and the most uncomfortable number in that file). So halving is the <b>backstop</b>;
/// the primary defence is sizing dense content shorter up front, which Phase 2 owns. Shipping the
/// naive loop is shipping N4d.</para>
///
/// <para>This lives in Stage A because segmentation is Stage A's job — but it is Stage B that calls it,
/// on failure. It is pure, so the overflow path can be tested without a model.</para>
/// </summary>
public static class SegmentSplitPolicy
{
    /// <summary>
    /// Split a segment that overflowed into two halves.
    ///
    /// The halves keep the parent's overlap discipline and carry their share of the block boundaries, so
    /// the pieces are exactly what Stage C already knows how to fuse. Halving needs no content analysis
    /// and is guaranteed to terminate — which is the whole reason to prefer it over anything cleverer.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The segment is already at the floor. Without this the overflow loop would halve forever on a
    /// segment that will never fit, and bill for every attempt.
    /// </exception>
    public static (Segment First, Segment Second) Halve(Segment segment, StageAOptions options)
    {
        if (segment.Duration < options.MinSegmentLength * 2)
        {
            throw new InvalidOperationException(
                $"segment {segment.Index} is {segment.Duration.TotalSeconds:F0}s and cannot be halved below the "
                + $"{options.MinSegmentLength.TotalSeconds:F0}s floor. Splitting further would bill for attempts "
                + "that cannot succeed; the caller must fail this segment instead.");
        }

        var midpoint = segment.Start + (segment.Duration / 2);
        var overlap = options.SegmentOverlap;

        var first = segment with
        {
            End = midpoint,
            Cues = [.. segment.Cues.Where(c => segment.Start + c < midpoint)],
        };

        var secondStart = midpoint - overlap;
        var second = segment with
        {
            Start = secondStart,
            OverlapBefore = overlap,
            Cues = [.. segment.Cues
                .Where(c => segment.Start + c >= midpoint)
                .Select(c => segment.Start + c - secondStart)],
        };

        return (first, second);
    }
}
