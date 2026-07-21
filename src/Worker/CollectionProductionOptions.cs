namespace MdReel.Worker;

/// <summary>
/// Config for one collection-production batch (`scripts/produce-collection.sh`). Internal
/// production only — this is never reachable from an API surface (METRICS.md N10).
/// </summary>
public sealed record CollectionProductionOptions
{
    public bool Enabled { get; init; }

    /// <summary>Collection slug, e.g. <c>ai-engineering</c>. Used in job ids and the output prefix.</summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// Path to the licence audit trail produced by the source-list step. Only its <c>full</c> array
    /// is read: <c>reference</c> entries are metadata and must never be processed
    /// (ARCHITECTURE.md §4b v1.1).
    /// </summary>
    public string CorpusPath { get; init; } = string.Empty;

    public string OutputBucket { get; init; } = "outputs-eu";

    public string OutputPrefix { get; init; } = "collections";

    /// <summary>Where to write the machine-readable batch report (cost reconciliation input).</summary>
    public string ReportPath { get; init; } = string.Empty;

    public TimeSpan SegmentLength { get; init; } = TimeSpan.FromMinutes(10);

    public TimeSpan SegmentOverlap { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>METRICS.md N4b — internal production runs blanket-low. Do not raise without a reason.</summary>
    public bool LowMediaResolution { get; init; } = true;

    /// <summary>Produce at most this many, then stop. Calibrate before committing the batch.</summary>
    public int? CalibrationCount { get; init; }

    /// <summary>Per-session N4d guard. Breaching it aborts the batch.</summary>
    public int? AbortOverCentsPerVideoHour { get; init; }

    /// <summary>Attempts per session before it is reported as failed and the batch moves on.</summary>
    public int RetryAttempts { get; init; } = 3;

    /// <summary>Base backoff between attempts; multiplied by the attempt number.</summary>
    public TimeSpan RetryBackoff { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>Idle between sessions, to stay under the Vertex quota rather than retry past it.</summary>
    public TimeSpan PaceBetweenSessions { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>Process only these video ids (empty ⇒ all). Used to retry a single dropped session.</summary>
    public IReadOnlyList<string> OnlyVideoIds { get; init; } = [];

    /// <summary>Rule 9 guards. Mandatory on every Stage B call — there is no unguarded path.</summary>
    public YouTubeGalleryRunnerOptions.StageBCallOptionsDto StageB { get; init; } = new();
}
