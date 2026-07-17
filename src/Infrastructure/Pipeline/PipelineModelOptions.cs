namespace MdReel.Infrastructure.Pipeline;

/// <summary>How the Stage B / Stage C model seams are backed.</summary>
public enum PipelineModelMode
{
    /// <summary>
    /// Deterministic in-process stand-ins that synthesize a valid, schema-correct result from the
    /// real Stage A segments. Zero spend, no network — the default for local dev and the e2e compose
    /// profile (the E2E suite must never call Vertex, CLAUDE.md rule 4 tier).
    /// </summary>
    Fake,

    /// <summary>Real Vertex Gemini calls. Production, the gallery runner, and <c>tests/Live</c>.</summary>
    Live,

    /// <summary>
    /// Replay committed responses from <see cref="PipelineModelOptions.FixturesDirectory"/> — the
    /// record/replay harness so integration tests exercise real-shaped data offline
    /// (<c>tests/fixtures/llm</c>). A missing fixture fails loudly.
    /// </summary>
    Replay,

    /// <summary>Real Vertex calls that also write fixtures to <see cref="PipelineModelOptions.FixturesDirectory"/>.</summary>
    Record,
}

/// <summary>Selection + fixture location for the Stage B/C model seams.</summary>
public sealed class PipelineModelOptions
{
    public const string SectionName = "PipelineModel";

    /// <summary>Model backing. Safe default is <see cref="PipelineModelMode.Fake"/> (never spends).</summary>
    public PipelineModelMode Mode { get; set; } = PipelineModelMode.Fake;

    /// <summary>Directory for record/replay fixtures. Required for Replay and Record modes.</summary>
    public string? FixturesDirectory { get; set; }

    /// <summary>
    /// Whether the private path stages the uploaded raw bytes into the GCS <c>raw-videos-eu</c>
    /// bucket and hands Stage B the resulting <c>gs://</c> URI (then deletes the object after Stage D
    /// — ARCHITECTURE §3/§7 tenant-isolation + auto-deletion). Vertex can only fetch a <c>gs://</c>
    /// URI, so this is required in <see cref="PipelineModelMode.Live"/>/<see cref="PipelineModelMode.Record"/>
    /// and pointless in <see cref="PipelineModelMode.Fake"/> (the stand-in ignores the URI).
    /// <c>null</c> ⇒ derive from <see cref="Mode"/> (stage iff not Fake); set explicitly to exercise
    /// the write-then-erase path offline in tests.
    /// </summary>
    public bool? StageRawUploadsToObjectStorage { get; set; }
}
