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
/// <item><c>RetryAttempts</c> / <c>RetryBackoff</c> — Vertex returns <c>429 RESOURCE_EXHAUSTED</c>
/// in <em>both</em> EU regions under contention, so an unattended batch will meet it. A quota blip
/// must not end the run or silently shrink the collection. <b>Keep the backoff short</b>: a 429
/// arrives in well under a second and costs nothing, so a long sleep buys nothing and forfeits
/// throughput.</item>
/// <item><c>MaxConcurrentSegments</c> — 🚩 the single biggest throughput lever on this path.
/// Measured 2026-07-21: the 429 success rate is a flat ~25% at every concurrency level, because the
/// contention is not ours (we run at ~1% of our own limit). Throughput therefore scales with
/// attempts in flight — ~0.8 successful calls/min sequential vs ~8.0 at 24-way concurrency.</item>
/// <item><c>PaceBetweenSessions</c> — deliberate idle between sessions. Now default OFF: it was
/// added on the assumption we were provoking the throttling, and the measurement says we are not.</item>
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
    int? AbortOverCentsPerVideoHour = null,
    int RetryAttempts = 3,
    TimeSpan? RetryBackoff = null,
    TimeSpan? PaceBetweenSessions = null,
    int MaxConcurrentSegments = 6);

/// <summary>
/// What happened to one source. <see cref="Failed"/> is deliberately distinct from a source that
/// was never attempted: a transport or quota failure is <b>not</b> a reason to drop a session from
/// the collection — it is a reason to retry it later with <c>--only</c>. Quality failures are the
/// ones that get dropped (DISTRIBUTION.md publishing threshold), and they are a different decision
/// made by a human looking at the output.
/// </summary>
public enum CollectionSessionOutcome
{
    Produced,
    AlreadyPresent,
    Failed,
}

public sealed record CollectionSessionResult(
    string VideoId,
    CollectionSessionOutcome Outcome,
    long Microcents,
    TimeSpan VideoDuration,
    TimeSpan WallClock,
    int StageBSegments,
    string? OutputMarkdownObject,
    string? FailureReason = null)
{
    public bool WasProduced => Outcome == CollectionSessionOutcome.Produced;

    public static CollectionSessionResult Skipped(CollectionSource source) =>
        new(source.VideoId, CollectionSessionOutcome.AlreadyPresent, 0, source.Duration, TimeSpan.Zero, 0, null);

    public static CollectionSessionResult Failed(CollectionSource source, string reason, long microcents) =>
        new(
            source.VideoId,
            CollectionSessionOutcome.Failed,
            microcents,
            source.Duration,
            TimeSpan.Zero,
            0,
            null,
            reason);

    public static CollectionSessionResult Produced(
        CollectionSource source,
        YouTubeInternalGalleryRunResult run,
        long microcents,
        TimeSpan wallClock) =>
        new(
            source.VideoId,
            CollectionSessionOutcome.Produced,
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
    public IEnumerable<CollectionSessionResult> Produced =>
        Sessions.Where(static s => s.Outcome == CollectionSessionOutcome.Produced);

    public IEnumerable<CollectionSessionResult> SkippedSessions =>
        Sessions.Where(static s => s.Outcome == CollectionSessionOutcome.AlreadyPresent);

    /// <summary>Sources worth retrying — pass these to <c>--only</c>, do not silently lose them.</summary>
    public IEnumerable<CollectionSessionResult> FailedSessions =>
        Sessions.Where(static s => s.Outcome == CollectionSessionOutcome.Failed);

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
