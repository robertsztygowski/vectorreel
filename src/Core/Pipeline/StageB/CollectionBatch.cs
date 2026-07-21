using MdReel.Core.Domain;

namespace MdReel.Core.Pipeline.StageB;

/// <summary>
/// One curated, licence-verified source in a collection's <c>full</c> tier.
/// <c>reference</c>-tier entries never reach this type: they are metadata only, are never processed,
/// and cost no inference (ARCHITECTURE.md §4b v1.1).
/// </summary>
/// <remarks>
/// <c>SegmentLength</c> is a per-source override. Dense slide content is segmented shorter
/// <em>up front</em> — the Phase-0.2 lesson behind METRICS.md <b>N4d</b> is that reacting to an
/// overflow costs far more than preventing it.
/// </remarks>
public sealed record CollectionSource(
    string VideoId,
    string SourceUri,
    TimeSpan Duration,
    GalleryAttribution Attribution,
    TimeSpan? SegmentLength = null);

/// <remarks>
/// <list type="bullet">
/// <item><c>MediaResolution</c> — METRICS.md <b>N4b</b>: blanket low resolution is the
/// request-level cost lever, and it is what METRICS.md <b>N4c</b> assumes for internal production
/// runs. Low by default here on purpose.</item>
/// <item><c>CalibrationCount</c> — produce at most this many sources, then stop. The batch is
/// calibrated before it is committed.</item>
/// <item><c>AbortOverCentsPerVideoHour</c> — abort the whole batch if any single session exceeds
/// this. The <b>N4d</b> guard.</item>
/// </list>
/// </remarks>
public sealed record CollectionBatchRequest(
    string Collection,
    IReadOnlyList<CollectionSource> Sources,
    string OutputBucket,
    string OutputPrefix,
    StageBOptions StageBOptions,
    TimeSpan SegmentLength,
    TimeSpan SegmentOverlap,
    MediaResolution MediaResolution = MediaResolution.Low,
    int? CalibrationCount = null,
    int? AbortOverCentsPerVideoHour = null);

public sealed record CollectionSessionResult(
    string VideoId,
    bool WasProduced,
    long Microcents,
    TimeSpan VideoDuration,
    TimeSpan WallClock,
    int StageBSegments,
    string? OutputMarkdownObject)
{
    public static CollectionSessionResult Skipped(CollectionSource source) =>
        new(source.VideoId, false, 0, source.Duration, TimeSpan.Zero, 0, null);

    public static CollectionSessionResult Produced(
        CollectionSource source,
        YouTubeInternalGalleryRunResult run,
        long microcents,
        TimeSpan wallClock) =>
        new(
            source.VideoId,
            true,
            microcents,
            source.Duration,
            wallClock,
            run.StageBSegments,
            run.OutputMarkdownObject);

    public int Cents => (int)Math.Round(Microcents / 10_000m, MidpointRounding.AwayFromZero);

    /// <summary>The reconciliation unit: METRICS.md <b>N4c</b> is the target, <b>N4d</b> the alarm.</summary>
    public int CentsPerVideoHour =>
        VideoDuration <= TimeSpan.Zero
            ? 0
            : (int)Math.Round(Microcents / 10_000m / (decimal)VideoDuration.TotalHours, MidpointRounding.AwayFromZero);
}

public sealed record CollectionBatchResult(
    string Collection,
    IReadOnlyList<CollectionSessionResult> Sessions)
{
    public IEnumerable<CollectionSessionResult> Produced => Sessions.Where(static s => s.WasProduced);

    public IEnumerable<CollectionSessionResult> SkippedSessions => Sessions.Where(static s => !s.WasProduced);

    public long TotalMicrocents => Produced.Sum(static s => s.Microcents);

    public int TotalCents => (int)Math.Round(TotalMicrocents / 10_000m, MidpointRounding.AwayFromZero);

    public double VideoHours => Produced.Sum(static s => s.VideoDuration.TotalHours);

    /// <summary>Measured cost per video-hour for what this run actually produced.</summary>
    public int CentsPerVideoHour =>
        VideoHours <= 0
            ? 0
            : (int)Math.Round(TotalMicrocents / 10_000m / (decimal)VideoHours, MidpointRounding.AwayFromZero);

    /// <summary>
    /// What the remaining sources would cost at the rate just measured — the extrapolation the
    /// calibration step exists to produce.
    /// </summary>
    public int ProjectedCentsFor(double remainingVideoHours) =>
        (int)Math.Round(CentsPerVideoHour * remainingVideoHours);
}

/// <summary>Raised when a session breaches the per-session cost ceiling. Stops the batch.</summary>
public sealed class CollectionBatchAbortedException(string message) : Exception(message);
