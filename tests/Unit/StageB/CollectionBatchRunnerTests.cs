using System.Text;
using MdReel.Core.Domain;
using MdReel.Core.Output;
using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;
using Microsoft.Extensions.Logging.Abstractions;

namespace MdReel.Tests.Unit.StageB;

/// <summary>
/// The batch driver's job is to spend money carefully. Each test here corresponds to one way a
/// batch can waste it: re-paying for work already done, committing before calibrating, or paying
/// for the METRICS.md N4d over-segmentation failure mode twenty-five times in a row.
/// </summary>
public sealed class CollectionBatchRunnerTests
{
    [Fact]
    public async Task Skips_sources_that_were_already_produced()
    {
        var harness = new Harness();
        harness.Storage.Writes[("outputs-eu", "collections/ai/vid-2/output.md")] = "already here";

        var result = await harness.RunAsync(Harness.Request(Sources(3)));

        Assert.Equal(["vid-1", "vid-3"], result.Produced.Select(s => s.VideoId));
        Assert.Equal(["vid-2"], result.SkippedSessions.Select(s => s.VideoId));
        // The point of skipping is that it costs nothing — not that it is merely quiet.
        Assert.DoesNotContain("vid-2", harness.Analyzer.AnalyzedSources.Select(u => u));
    }

    [Fact]
    public async Task Stops_at_the_calibration_count_so_the_batch_is_measured_before_it_is_committed()
    {
        var harness = new Harness();

        var result = await harness.RunAsync(Harness.Request(Sources(5)) with { CalibrationCount = 2 });

        Assert.Equal(2, result.Produced.Count());
        Assert.Equal(["vid-1", "vid-2"], result.Produced.Select(s => s.VideoId));
    }

    [Fact]
    public async Task Skipped_sources_do_not_consume_the_calibration_budget()
    {
        var harness = new Harness();
        harness.Storage.Writes[("outputs-eu", "collections/ai/vid-1/output.md")] = "already here";

        var result = await harness.RunAsync(Harness.Request(Sources(4)) with { CalibrationCount = 2 });

        // Calibration exists to measure two *fresh* productions. If an already-present session ate
        // one of the two slots, the extrapolation would be built on a single data point.
        Assert.Equal(["vid-2", "vid-3"], result.Produced.Select(s => s.VideoId));
    }

    [Fact]
    public async Task Runs_at_low_media_resolution_by_default()
    {
        var harness = new Harness();

        await harness.RunAsync(Harness.Request(Sources(1)));

        Assert.NotEmpty(harness.Analyzer.Resolutions);
        Assert.All(harness.Analyzer.Resolutions, r => Assert.Equal(MediaResolution.Low, r));
    }

    [Fact]
    public async Task Aborts_the_batch_when_a_session_breaches_the_cost_ceiling()
    {
        var harness = new Harness { CentsPerSession = 500 };
        var request = Harness.Request(Sources(3)) with { AbortOverCentsPerVideoHour = 200 };

        var ex = await Assert.ThrowsAsync<CollectionBatchAbortedException>(
            () => harness.RunAsync(request));

        Assert.Contains("N4d", ex.Message, StringComparison.Ordinal);
        // Stopped on the first breach — it did not work through the rest of the corpus paying the
        // same inflated rate.
        Assert.Single(harness.Analyzer.ProducedVideoIds.Distinct());
    }

    [Fact]
    public async Task Reports_measured_cost_per_video_hour_and_extrapolates()
    {
        var harness = new Harness { CentsPerSession = 20 };

        // Two sources of exactly 30 minutes each: 1.0 video-hour, 40 cents.
        var result = await harness.RunAsync(Harness.Request(Sources(2)));

        Assert.Equal(40, result.TotalCents);
        Assert.Equal(1.0, result.VideoHours, 3);
        Assert.Equal(40, result.CentsPerVideoHour);
        Assert.Equal(400, result.ProjectedCentsFor(10));
    }

    [Fact]
    public async Task Per_source_segment_length_overrides_the_batch_default()
    {
        var harness = new Harness();
        var dense = new CollectionSource(
            "vid-dense",
            "https://www.youtube.com/watch?v=vid-dense",
            TimeSpan.FromMinutes(30),
            Attribution("vid-dense"),
            SegmentLength: TimeSpan.FromMinutes(5));

        await harness.RunAsync(Harness.Request([dense]));

        // 30 minutes at a 5-minute segment target is 6 calls, not the 3 the batch default gives.
        Assert.Equal(6, harness.Analyzer.Resolutions.Count);
    }

    private static IReadOnlyList<CollectionSource> Sources(int count) =>
        [.. Enumerable.Range(1, count).Select(i => new CollectionSource(
            $"vid-{i}",
            $"https://www.youtube.com/watch?v=vid-{i}",
            TimeSpan.FromMinutes(30),
            Attribution($"vid-{i}")))];

    private static GalleryAttribution Attribution(string id) =>
        new($"Title {id}", "Channel", "CC BY", $"https://www.youtube.com/watch?v={id}");

    private sealed class Harness
    {
        public int CentsPerSession { get; init; } = 10;

        public RecordingAnalyzer Analyzer { get; } = new();

        public InMemoryStorage Storage { get; } = new();

        public InMemoryCostLedger Ledger { get; } = new();

        public static CollectionBatchRequest Request(IReadOnlyList<CollectionSource> sources) =>
            new(
                Collection: "ai",
                Sources: sources,
                OutputBucket: "outputs-eu",
                OutputPrefix: "collections/ai",
                StageBOptions: StageBOptions.Default with
                {
                    ForceCueBoundarySegmentation = false,
                    DenseCueThresholdPerTenMinutes = 999,
                },
                SegmentLength: TimeSpan.FromMinutes(10),
                SegmentOverlap: TimeSpan.FromSeconds(20));

        public Task<CollectionBatchResult> RunAsync(CollectionBatchRequest request)
        {
            var runner = new YouTubeInternalGalleryRunner(
                new StageBRunner(Analyzer),
                new MeteringFuser(Ledger, CentsPerSession),
                Storage,
                Ledger,
                NullLogger<YouTubeInternalGalleryRunner>.Instance);

            return new CollectionBatchRunner(runner, Storage, Ledger, NullLogger<CollectionBatchRunner>.Instance)
                .RunAsync(request, CancellationToken.None);
        }
    }

    private sealed class RecordingAnalyzer : IVideoAnalyzer
    {
        public List<string> AnalyzedSources { get; } = [];

        public List<MediaResolution> Resolutions { get; } = [];

        public List<string> ProducedVideoIds { get; } = [];

        public Task<StageBModelResponse> AnalyzeAsync(
            string sourceUri,
            Segment segment,
            StageBCallOptions callOptions,
            CancellationToken cancellationToken)
        {
            AnalyzedSources.Add(sourceUri);
            Resolutions.Add(segment.Sampling.MediaResolution);
            ProducedVideoIds.Add(sourceUri.Split('=')[^1]);

            // Stage B reports offsets within the segment, not absolute video positions.
            static string Stamp(TimeSpan t) => $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
            var third = TimeSpan.FromSeconds(segment.Duration.TotalSeconds / 3);
            var output = new StageBModelOutput(
                SegmentStart: Stamp(segment.Start),
                Language: "en",
                Blocks:
                [
                    new StageBModelBlock(Stamp(TimeSpan.Zero), "a", "Speaker", "screen a", "visual a", "talk"),
                    new StageBModelBlock(Stamp(third), "b", "Speaker", "screen b", "visual b", "demo"),
                    new StageBModelBlock(Stamp(third * 2), "c", "Speaker", "screen c", "visual c", "talk"),
                ],
                Summary: "summary");

            return Task.FromResult(new StageBModelResponse(StageBFinishReason.Stop, output, segment.Duration));
        }
    }

    // Stands in for the real Vertex fuser's rule-6 metering, so the batch has a cost to reconcile.
    private sealed class MeteringFuser(ICostLedger ledger, int cents) : ITextFuser
    {
        public Task<OutputDocument> FuseAsync(
            IReadOnlyList<SegmentAnalysis> segments,
            FusionRequest request,
            CancellationToken cancellationToken)
        {
            ledger.Record(new CostEntry(request.JobId, CostKind.Llm, "stage_c.fuse", 1, "calls", cents));

            var document = new OutputDocument(
                Frontmatter: new OutputFrontmatter(
                    Title: "Fused",
                    Source: request.Source,
                    Duration: "00:30:00",
                    Language: "en",
                    ProcessedAt: "2026-01-01T00:00:00Z",
                    Generator: request.Generator,
                    Summary: $"Segments: {segments.Count}",
                    Tags: ["demo"]),
                Sections:
                [
                    new OutputSection(
                        Timestamp: "00:00:00",
                        Heading: "Intro",
                        Blocks: [new OutputBlock(OutputBlockLabel.Spoken, "Fused body.")]),
                ],
                Provenance: request.Provenance);

            return Task.FromResult(document);
        }
    }

    private sealed class InMemoryStorage : IObjectStorage
    {
        public Dictionary<(string Bucket, string ObjectName), string> Writes { get; } = [];

        public Task<Stream> OpenReadAsync(string bucket, string objectName, CancellationToken cancellationToken)
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(Writes[(bucket, objectName)]));
            return Task.FromResult(stream);
        }

        public Task<bool> ExistsAsync(string bucket, string objectName, CancellationToken cancellationToken) =>
            Task.FromResult(Writes.ContainsKey((bucket, objectName)));

        public async Task WriteAsync(string bucket, string objectName, Stream content, CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(content, Encoding.UTF8, leaveOpen: true);
            Writes[(bucket, objectName)] = await reader.ReadToEndAsync(cancellationToken);
        }

        public Task DeleteAsync(string bucket, string objectName, CancellationToken cancellationToken)
        {
            Writes.Remove((bucket, objectName));
            return Task.CompletedTask;
        }
    }
}
