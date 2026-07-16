using System.Text;
using MdReel.Core.Domain;
using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;
using Microsoft.Extensions.Logging.Abstractions;

namespace MdReel.Tests.Unit.StageB;

public sealed class YouTubeInternalGalleryRunnerTests
{
    [Fact]
    public async Task Runs_B_to_C_to_D_for_youtube_file_uri_and_persists_outputs()
    {
        var analyzer = new RecordingAnalyzer();
        var stageB = new StageBRunner(analyzer);
        var storage = new InMemoryStorage();
        var fuser = new FakeFuser();
        var runner = new YouTubeInternalGalleryRunner(stageB, fuser, storage, NullLogger<YouTubeInternalGalleryRunner>.Instance);

        var request = new YouTubeInternalGalleryRunRequest(
            JobId: "job-gallery-1",
            VideoId: "rAl-9HwD858",
            SourceUri: "https://www.youtube.com/watch?v=rAl-9HwD858",
            Duration: TimeSpan.FromSeconds(1500),
            OutputBucket: "outputs-eu",
            OutputPrefix: "gallery",
            SegmentLength: TimeSpan.FromMinutes(10),
            SegmentOverlap: TimeSpan.FromSeconds(20));

        var options = StageBOptions.Default with
        {
            CallOptions = new StageBCallOptions(11_000, 2_000, TimeSpan.FromSeconds(45)),
            ForceCueBoundarySegmentation = false,
            DenseCueThresholdPerTenMinutes = 999,
        };

        var result = await runner.RunAsync(request, options, CancellationToken.None);

        Assert.Equal("gallery/rAl-9HwD858/output.md", result.OutputMarkdownObject);
        Assert.Equal("gallery/rAl-9HwD858/stage-b.json", result.StageBRawObject);
        Assert.Equal(analyzer.Calls.Count, result.StageBSegments);

        Assert.All(analyzer.Calls, call =>
        {
            Assert.Equal(request.SourceUri, call.SourceUri);
            Assert.Equal(options.CallOptions.MaxOutputTokens, call.CallOptions.MaxOutputTokens);
            Assert.Equal(options.CallOptions.ThinkingBudget, call.CallOptions.ThinkingBudget);
            Assert.Equal(options.CallOptions.Timeout, call.CallOptions.Timeout);
        });

        Assert.True(storage.Writes.ContainsKey(("outputs-eu", "gallery/rAl-9HwD858/output.md")));
        Assert.True(storage.Writes.ContainsKey(("outputs-eu", "gallery/rAl-9HwD858/stage-b.json")));
    }

    [Fact]
    public async Task Rejects_non_youtube_sources()
    {
        var runner = new YouTubeInternalGalleryRunner(
            new StageBRunner(new RecordingAnalyzer()),
            new FakeFuser(),
            new InMemoryStorage(),
            NullLogger<YouTubeInternalGalleryRunner>.Instance);

        var request = new YouTubeInternalGalleryRunRequest(
            JobId: "job-2",
            VideoId: "demo",
            SourceUri: "https://example.com/video.mp4",
            Duration: TimeSpan.FromSeconds(600),
            OutputBucket: "outputs-eu",
            OutputPrefix: "gallery",
            SegmentLength: TimeSpan.FromMinutes(10),
            SegmentOverlap: TimeSpan.FromSeconds(20));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            runner.RunAsync(request, StageBOptions.Default, CancellationToken.None));
    }

    private sealed class RecordingAnalyzer : IVideoAnalyzer
    {
        public List<(string SourceUri, Segment Segment, StageBCallOptions CallOptions)> Calls { get; } = [];

        public Task<StageBModelResponse> AnalyzeAsync(
            string sourceUri,
            Segment segment,
            StageBCallOptions callOptions,
            CancellationToken cancellationToken)
        {
            Calls.Add((sourceUri, segment, callOptions));

            var blockEnd = TimeSpan.FromSeconds(Math.Max(10, segment.Duration.TotalSeconds - 5));
            var output = new StageBModelOutput(
                SegmentStart: "00:00:00",
                Language: "en",
                Blocks:
                [
                    new StageBModelBlock("00:00:10", "a", "Speaker", "screen a", "visual a", "demo"),
                    new StageBModelBlock("00:00:40", "mid", "Speaker", "screen mid", "visual mid", "demo"),
                    new StageBModelBlock($"00:{blockEnd.Minutes:00}:{blockEnd.Seconds:00}", "b", "Speaker", "screen b", "visual b", "demo"),
                ],
                Summary: "summary");

            return Task.FromResult(new StageBModelResponse(StageBFinishReason.Stop, output, segment.Duration));
        }
    }

    private sealed class FakeFuser : ITextFuser
    {
        public Task<string> FuseAsync(IReadOnlyList<SegmentAnalysis> segments, CancellationToken cancellationToken)
        {
            var text = $"# Fused\n\nSegments: {segments.Count}\n";
            return Task.FromResult(text);
        }
    }

    private sealed class InMemoryStorage : IObjectStorage
    {
        public Dictionary<(string Bucket, string ObjectName), string> Writes { get; } = [];

        public Task<Stream> OpenReadAsync(string bucket, string objectName, CancellationToken cancellationToken)
        {
            var content = Writes[(bucket, objectName)];
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return Task.FromResult(stream);
        }

        public async Task WriteAsync(string bucket, string objectName, Stream content, CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(content, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync(cancellationToken);
            Writes[(bucket, objectName)] = body;
        }

        public Task DeleteAsync(string bucket, string objectName, CancellationToken cancellationToken)
        {
            Writes.Remove((bucket, objectName));
            return Task.CompletedTask;
        }
    }
}
