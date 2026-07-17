namespace MdReel.Infrastructure.Vertex;

/// <summary>
/// Vertex AI connection + model settings. EU regions only (CLAUDE.md rule 2). Defaults match
/// INFRA.md / ARCHITECTURE §2: <c>gemini-2.5-flash</c> in <c>europe-central2</c>, with
/// <c>europe-west3</c> as the only EU fallback that also serves the model.
/// </summary>
public sealed class VertexOptions
{
    public const string SectionName = "Vertex";

    public string Project { get; set; } = "tensile-runway-442915-j6";

    public string Region { get; set; } = "europe-central2";

    public string FallbackRegion { get; set; } = "europe-west3";

    public string Model { get; set; } = "gemini-2.5-flash";

    /// <summary>API version segment of the generateContent URL.</summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// How many times to retry the <b>same</b> region on <c>429 RESOURCE_EXHAUSTED</c> before
    /// falling back to <see cref="FallbackRegion"/>. Stage B→C back-to-back trips 429 on the primary
    /// region under load (INFRA.md); one retry, then fall back, keeps the pipeline moving. EU-only.
    /// </summary>
    public int MaxRetriesPerRegion { get; set; } = 1;

    /// <summary>
    /// Back-off before retrying the same region on 429 (the server's <c>Retry-After</c> header wins
    /// when present). Tests set this to zero.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>Sampling temperature. Extraction wants determinism, not creativity.</summary>
    public double Temperature { get; set; } = 0.2;

    /// <summary>
    /// Fusion (Stage C) is text-only; it may use more output than a single segment. A separate,
    /// larger cap than Stage B keeps the fused document from truncating.
    /// </summary>
    public int FusionMaxOutputTokens { get; set; } = 32_000;

    /// <summary>Bounded thinking budget for the fusion call (CLAUDE.md rule 9 applies here too).</summary>
    public int FusionThinkingBudget { get; set; } = 8_192;

    /// <summary>Wall-clock timeout for the fusion call.</summary>
    public TimeSpan FusionTimeout { get; set; } = TimeSpan.FromSeconds(180);
}
