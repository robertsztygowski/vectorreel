using System.Text.Json.Serialization;

namespace MdReel.Core.Output;

/// <summary>Publication tier — ARCHITECTURE.md §4b v1.1.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<InclusionTier>))]
public enum InclusionTier
{
    /// <summary>A complete §4 session document exists. CC-BY sources only in a public collection.</summary>
    [JsonStringEnumMemberName("full")]
    Full,

    /// <summary>Index entry only: no derived text, nothing processed, zero inference cost.</summary>
    [JsonStringEnumMemberName("reference")]
    Reference,
}

/// <summary>
/// One input to the repository renderer. A <c>Full</c> entry carries its produced document; a
/// <c>Reference</c> entry carries curated citations instead and must never carry a document.
/// </summary>
public sealed record RepositoryInput
{
    public required InclusionTier Inclusion { get; init; }

    public required string Title { get; init; }

    public required string Source { get; init; }

    /// <summary>Recording/publication date, <c>yyyy-MM-dd</c>. Drives the slug and the timeline.</summary>
    public required string RecordedAt { get; init; }

    public string? Event { get; init; }

    public int? Year { get; init; }

    public required string Licence { get; init; }

    public required string LicenceVerifiedVia { get; init; }

    public required string Attribution { get; init; }

    /// <summary>Curation metadata, never model output (§4b). Empty means no speaker index.</summary>
    public IReadOnlyList<string> Speakers { get; init; } = [];

    /// <summary>Full tier only.</summary>
    public OutputDocument? Document { get; init; }

    /// <summary>Reference tier only: hand-curated deep links into the original.</summary>
    public IReadOnlyList<RepositoryCitation> Citations { get; init; } = [];

    /// <summary>Reference tier only — a full entry's tags come from its document's frontmatter.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record RepositoryCitation(string Timestamp, string Label);

public sealed record RepositoryOptions
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Generator { get; init; }

    public required string GeneratedAt { get; init; }

    public bool Public { get; init; } = true;

    public string LicenceNote { get; init; } =
        "Every source is CC-licensed and attributed in its session document and in this manifest. "
        + "Derived content is published under CC BY 4.0.";

    /// <summary>Curation-time synonym merges, applied to topic tags (§4b).</summary>
    public IReadOnlyDictionary<string, string> TopicMerges { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>A rendered repository: relative path → file contents. Written verbatim.</summary>
public sealed record RenderedRepository(IReadOnlyDictionary<string, string> Files);
