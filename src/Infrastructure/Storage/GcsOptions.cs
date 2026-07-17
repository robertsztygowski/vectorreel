namespace MdReel.Infrastructure.Storage;

/// <summary>
/// Object-storage settings. Real GCS in production (ADC / the Cloud Run service account); the
/// fake-gcs-server emulator locally and in the e2e compose profile via <see cref="EmulatorHost"/>
/// (docker-compose.yml). EU buckets only (CLAUDE.md rule 2).
/// </summary>
public sealed class GcsOptions
{
    public const string SectionName = "Gcs";

    /// <summary>Bucket for uploaded source videos, deleted after processing (ARCHITECTURE.md §3).</summary>
    public string RawBucket { get; set; } = "raw-videos-eu";

    /// <summary>Bucket for the rendered output.md / output.json (ARCHITECTURE.md §3–§4).</summary>
    public string OutputBucket { get; set; } = "outputs-eu";

    /// <summary>
    /// Base URL of a fake-gcs-server emulator (e.g. <c>http://localhost:4443</c>). When set, the
    /// client runs unauthenticated against it and auto-creates buckets on first use. Unset in
    /// production ⇒ real GCS with ADC.
    /// </summary>
    public string? EmulatorHost { get; set; }

    /// <summary>GCP project that owns the buckets (only needed to create them on the emulator).</summary>
    public string Project { get; set; } = "tensile-runway-442915-j6";
}
