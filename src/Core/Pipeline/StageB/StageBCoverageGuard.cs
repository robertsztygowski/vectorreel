namespace MdReel.Core.Pipeline.StageB;

public sealed record StageBCoverageVerdict(bool IsValid, double Coverage, string? Reason);

/// <summary>Two-sided guard against under and over coverage.</summary>
public static class StageBCoverageGuard
{
    public static StageBCoverageVerdict Evaluate(
        IReadOnlyList<TimeSpan> blockOffsets,
        TimeSpan segmentDuration,
        StageBOptions options)
    {
        if (segmentDuration <= TimeSpan.Zero)
        {
            return new StageBCoverageVerdict(false, 0, "segment duration must be positive");
        }

        if (blockOffsets.Count == 0)
        {
            return new StageBCoverageVerdict(false, 0, "model returned zero blocks");
        }

        var lastOffset = blockOffsets[^1];
        var coverage = lastOffset.TotalSeconds / segmentDuration.TotalSeconds;

        if (segmentDuration.TotalSeconds > 120 && blockOffsets.Count < options.MinBlocksForLongSegments)
        {
            return new StageBCoverageVerdict(false, coverage, "too few blocks for long segment");
        }

        if (coverage < options.CoverageMin)
        {
            return new StageBCoverageVerdict(false, coverage, "under-coverage");
        }

        if (coverage > options.CoverageMax)
        {
            return new StageBCoverageVerdict(false, coverage, "over-coverage");
        }

        return new StageBCoverageVerdict(true, coverage, null);
    }
}

