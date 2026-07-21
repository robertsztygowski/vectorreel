using System.Text;
using System.Text.Json;
using MdReel.Core.Output;
using Microsoft.Extensions.Options;

namespace MdReel.Worker;

/// <summary>
/// One-shot: read the licence audit trail plus whatever the batch actually produced, render the
/// §4b repository, write it to a directory, stop.
///
/// 🚨 The corpus lists what we *intended* to produce; the bucket holds what we *did*. Those differ
/// whenever a session failed, was dropped by a quality gate, or has not run yet — so the repository
/// is built from the intersection, and the difference is reported rather than silently smoothed
/// over. A collection that lists sessions it does not contain is worse than a smaller one.
/// </summary>
public sealed class RepositoryBuildWorker(
    IOptions<RepositoryBuildOptions> options,
    IHostApplicationLifetime lifetime,
    ILogger<RepositoryBuildWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions CorpusJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = options.Value;
        if (!cfg.Enabled)
        {
            return Task.CompletedTask;
        }

        try
        {
            var inputs = cfg.ScaffoldOnly ? [] : BuildInputs(cfg);
            var rendered = RepositoryRenderer.Render(inputs, new RepositoryOptions
            {
                Name = cfg.Name,
                Description = cfg.Description,
                Generator = cfg.Generator,
                GeneratedAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture),
                Public = true,
            });

            Write(cfg.TargetDirectory, rendered);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Wrote {Files} files for '{Collection}' to {Target} ({Full} full, {Reference} reference).",
                    rendered.Files.Count,
                    cfg.Collection,
                    cfg.TargetDirectory,
                    inputs.Count(i => i.Inclusion == InclusionTier.Full),
                    inputs.Count(i => i.Inclusion == InclusionTier.Reference));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Repository build failed for '{Collection}'.", cfg.Collection);
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }

        return Task.CompletedTask;
    }

    private List<RepositoryInput> BuildInputs(RepositoryBuildOptions cfg)
    {
        var corpus = JsonSerializer.Deserialize<CorpusFile>(File.ReadAllText(cfg.CorpusPath), CorpusJson)
            ?? throw new InvalidOperationException($"Corpus {cfg.CorpusPath} is unreadable.");

        List<RepositoryInput> inputs = [];
        List<string> missing = [];

        foreach (var entry in corpus.Full ?? [])
        {
            var documentPath = Path.Combine(
                cfg.ProducedRoot,
                cfg.OutputBucket,
                cfg.OutputPrefix.Replace('/', Path.DirectorySeparatorChar),
                cfg.Collection,
                entry.VideoId,
                "output.json");

            if (!File.Exists(documentPath))
            {
                missing.Add(entry.VideoId);
                continue;
            }

            var document = JsonSerializer.Deserialize<OutputDocument>(File.ReadAllText(documentPath))
                ?? throw new InvalidOperationException($"{documentPath} did not parse as a §4 document.");

            inputs.Add(new RepositoryInput
            {
                Inclusion = InclusionTier.Full,
                Title = entry.Title,
                Source = entry.Url,
                RecordedAt = (entry.PublishedAt ?? string.Empty).Length >= 10
                    ? entry.PublishedAt![..10]
                    : document.Frontmatter.ProcessedAt[..10],
                Event = entry.Event,
                Year = entry.Year,
                Licence = entry.Licence,
                LicenceVerifiedVia = entry.LicenceVerifiedVia ?? "youtube.data.api.v3 videos.list status.license",
                Attribution = entry.Attribution ?? string.Empty,
                Speakers = string.IsNullOrWhiteSpace(entry.Speaker) ? [] : [entry.Speaker],
                Document = document,
            });
        }

        foreach (var entry in corpus.Reference ?? [])
        {
            inputs.Add(new RepositoryInput
            {
                Inclusion = InclusionTier.Reference,
                Title = entry.Title,
                Source = entry.Url,
                RecordedAt = (entry.PublishedAt ?? string.Empty).Length >= 10 ? entry.PublishedAt![..10] : "1970-01-01",
                Event = entry.Event,
                Year = entry.Year,
                Licence = entry.Licence,
                LicenceVerifiedVia = entry.LicenceVerifiedVia ?? "youtube.data.api.v3 videos.list status.license",
                Attribution = entry.Attribution ?? string.Empty,
                Speakers = string.IsNullOrWhiteSpace(entry.Speaker) ? [] : [entry.Speaker],
                Tags = entry.TopicTags ?? [],
                // Reference entries are hand-curated. Until a curator adds real chapter marks, the
                // one citation we can make truthfully without processing the video is its start.
                Citations = [new RepositoryCitation("00:00:00", "Talk start")],
            });
        }

        if (missing.Count > 0 && logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "{Count} full-tier sources have no produced document yet and were left out: {Missing}",
                missing.Count,
                string.Join(", ", missing));
        }

        return inputs;
    }

    private static void Write(string targetDirectory, RenderedRepository rendered)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory);

        // Generated directories are rewritten wholesale so a removed session cannot linger as an
        // orphan file; anything else in the working tree (.git, LICENSE, .github/) is left alone.
        foreach (var generated in new[] { "sessions", "topics", "speakers", "timeline", "metadata" })
        {
            var path = Path.Combine(targetDirectory, generated);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        foreach (var (relative, content) in rendered.Files)
        {
            var path = Path.Combine(targetDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            // LF and a single trailing newline are already guaranteed by the renderer; write the
            // bytes as-is so no platform newline translation can undo that (§4b portability).
            File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }

    private sealed record CorpusFile
    {
        public IReadOnlyList<CorpusEntry>? Full { get; init; }

        public IReadOnlyList<CorpusEntry>? Reference { get; init; }
    }

    private sealed record CorpusEntry
    {
        public string VideoId { get; init; } = string.Empty;

        public string Url { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string? Event { get; init; }

        public int? Year { get; init; }

        public string? PublishedAt { get; init; }

        public string Licence { get; init; } = string.Empty;

        public string? LicenceVerifiedVia { get; init; }

        public string? Attribution { get; init; }

        public string? Speaker { get; init; }

        public IReadOnlyList<string>? TopicTags { get; init; }
    }
}
