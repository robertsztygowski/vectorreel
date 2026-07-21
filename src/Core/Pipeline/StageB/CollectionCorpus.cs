using System.Text.Json;
using System.Text.Json.Serialization;

namespace MdReel.Core.Pipeline.StageB;

/// <summary>
/// Reads the licence audit trail (the <c>corpus.json</c> pattern) into producible sources.
///
/// 🚨 Two invariants are enforced here rather than trusted, because they are the two ways this
/// company could get itself into a copyright fight: only the <c>full</c> array is read, and every
/// entry in it must actually be verified CC-BY. A corpus file that has drifted fails loudly at the
/// start of a batch instead of quietly producing something unpublishable (CLAUDE.md rule 8,
/// DISTRIBUTION.md licence gate).
/// </summary>
public static class CollectionCorpus
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static IReadOnlyList<CollectionSource> Load(string path, IReadOnlyList<string> onlyVideoIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var corpus = JsonSerializer.Deserialize<CorpusFile>(File.ReadAllText(path), Json)
            ?? throw new InvalidOperationException($"Corpus file {path} is empty or unparseable.");

        var entries = corpus.Full ?? [];
        if (entries.Count == 0)
        {
            throw new InvalidOperationException($"Corpus file {path} has no `full` entries to produce.");
        }

        List<CollectionSource> sources = [];
        foreach (var entry in entries)
        {
            if (!string.Equals(entry.Licence, "creativeCommon", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{entry.VideoId} is `full` tier with licence '{entry.Licence}'. Only CC-BY may be "
                    + "rendered as a full session document — this is a hard stop, not a warning.");
            }

            if (string.IsNullOrWhiteSpace(entry.LicenceVerifiedVia)
                || string.IsNullOrWhiteSpace(entry.LicenceVerifiedAt)
                || string.IsNullOrWhiteSpace(entry.Attribution))
            {
                throw new InvalidOperationException(
                    $"{entry.VideoId} is missing licence verification evidence or an attribution line. "
                    + "Nothing is processed 'pending a check'.");
            }

            if (entry.DurationS <= 0)
            {
                throw new InvalidOperationException($"{entry.VideoId} has no usable duration.");
            }

            if (onlyVideoIds.Count > 0 && !onlyVideoIds.Contains(entry.VideoId, StringComparer.Ordinal))
            {
                continue;
            }

            sources.Add(new CollectionSource(
                VideoId: entry.VideoId,
                SourceUri: entry.Url,
                Duration: TimeSpan.FromSeconds(entry.DurationS),
                Attribution: new GalleryAttribution(
                    Title: entry.Title,
                    Author: entry.Channel,
                    Licence: "CC BY",
                    SourceUrl: entry.Url)));
        }

        return sources;
    }

    private sealed record CorpusFile
    {
        [JsonPropertyName("full")]
        public IReadOnlyList<CorpusEntry>? Full { get; init; }
    }

    private sealed record CorpusEntry
    {
        public string VideoId { get; init; } = string.Empty;

        public string Url { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Channel { get; init; } = string.Empty;

        public double DurationS { get; init; }

        public string Licence { get; init; } = string.Empty;

        public string? LicenceVerifiedVia { get; init; }

        public string? LicenceVerifiedAt { get; init; }

        public string? Attribution { get; init; }
    }
}
