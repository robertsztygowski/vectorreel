using MdReel.Core.Providers;

namespace MdReel.Core.Pipeline.StageB;

/// <summary>Tunables for Stage B guard rails.</summary>
public sealed record StageBOptions
{
    public static StageBOptions Default { get; } = new();

    /// <summary>Primary call guard settings (mandatory on every call).</summary>
    public StageBCallOptions CallOptions { get; init; } = StageBCallOptions.Default;

    /// <summary>Tighter budget for one retry on degenerate output (not MAX_TOKENS).</summary>
    public StageBCallOptions RetryCallOptions { get; init; } = new(
        MaxOutputTokens: 8_000,
        ThinkingBudget: 1_024,
        Timeout: TimeSpan.FromSeconds(60));

    /// <summary>At most one unchanged retry, and never for MAX_TOKENS.</summary>
    public int MaxRetries { get; init; } = 1;

    /// <summary>Coverage lower bound for long segments.</summary>
    public double CoverageMin { get; init; } = 0.60;

    /// <summary>Coverage upper bound. Guards impossible timestamps.</summary>
    public double CoverageMax { get; init; } = 1.05;

    /// <summary>Minimum expected blocks in long segments.</summary>
    public int MinBlocksForLongSegments { get; init; } = 3;

    /// <summary>Dense segments are shortened up front to avoid naive N4d amplification.</summary>
    public TimeSpan DenseSegmentTarget { get; init; } = TimeSpan.FromMinutes(4);

    /// <summary>Cues per ten minutes that marks a segment as dense.</summary>
    public double DenseCueThresholdPerTenMinutes { get; init; } = 13.0;

    /// <summary>
    /// Known fallback: Stage B can ignore cue hints. Split real segments at cues to enforce
    /// boundaries physically.
    /// </summary>
    public bool ForceCueBoundarySegmentation { get; init; } = true;
}

