using MdReel.Core.Domain;
using MdReel.Core.Pipeline.StageA;
using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;

namespace MdReel.Tests.Unit.StageB;

public sealed class StageBRunnerTests
{
    [Fact]
    public async Task Every_call_receives_mandatory_guard_options()
    {
        var fake = new RecordingAnalyzer(static (_, _) => ValidResponse(TimeSpan.FromSeconds(180)));
        var runner = new StageBRunner(fake);
        var options = StageBOptions.Default;

        await runner.AnalyzeAsync("gs://x", Segment(0, 180), StageAOptions.Default, options, CancellationToken.None);

        Assert.NotEmpty(fake.Calls);
        Assert.All(fake.Calls, call =>
        {
            Assert.Equal(options.CallOptions.MaxOutputTokens, call.MaxOutputTokens);
            Assert.Equal(options.CallOptions.ThinkingBudget, call.ThinkingBudget);
            Assert.Equal(options.CallOptions.Timeout, call.Timeout);
        });
    }

    [Fact]
    public async Task Max_tokens_splits_instead_of_retrying_the_same_segment()
    {
        var attemptsByWindow = new Dictionary<string, int>();
        var fake = new RecordingAnalyzer((segment, _) =>
        {
            var key = $"{segment.Start.TotalSeconds}-{segment.End.TotalSeconds}";
            attemptsByWindow[key] = attemptsByWindow.GetValueOrDefault(key) + 1;

            if (segment.Duration.TotalSeconds >= 120)
            {
                return new StageBModelResponse(StageBFinishReason.MaxTokens, null, segment.Duration);
            }

            return ValidResponse(segment.Duration);
        });

        var runner = new StageBRunner(fake);
        var result = await runner.AnalyzeAsync(
            "gs://x",
            Segment(0, 240, 60, 120, 180),
            StageAOptions.Default,
            StageBOptions.Default with { ForceCueBoundarySegmentation = false },
            CancellationToken.None);

        Assert.True(result.Count > 1);
        Assert.All(attemptsByWindow.Values, attempts => Assert.Equal(1, attempts));
    }

    [Fact]
    public async Task Cue_fallback_physically_splits_segment_before_calling_model()
    {
        var fake = new RecordingAnalyzer(static (_, _) => ValidResponse(TimeSpan.FromSeconds(60)));
        var runner = new StageBRunner(fake);

        await runner.AnalyzeAsync(
            "gs://x",
            Segment(0, 240, 30, 90, 150, 210),
            StageAOptions.Default,
            StageBOptions.Default with { DenseCueThresholdPerTenMinutes = 999 },
            CancellationToken.None);

        Assert.True(fake.Calls.Count >= 2);
    }

    private static Segment Segment(int startSeconds, int endSeconds, params int[] cueSeconds) =>
        new(
            Index: 1,
            Start: TimeSpan.FromSeconds(startSeconds),
            End: TimeSpan.FromSeconds(endSeconds),
            OverlapBefore: TimeSpan.Zero,
            Sampling: new SamplingPlan(MediaResolution.Default, 0),
            Cues: [.. cueSeconds.Select(static second => TimeSpan.FromSeconds(second))]);

    private static StageBModelResponse ValidResponse(TimeSpan duration) =>
        new(
            StageBFinishReason.Stop,
            new StageBModelOutput(
                SegmentStart: "00:00:00",
                Language: "en",
                Blocks:
                [
                    new StageBModelBlock("00:00:10", "a", "Speaker 1", "", "v1", "demo"),
                    new StageBModelBlock("00:01:10", "b", "Speaker 1", "", "v2", "demo"),
                    new StageBModelBlock($"00:{Math.Min(59, (int)duration.TotalSeconds / 60):00}:{Math.Min(59, (int)duration.TotalSeconds % 60):00}", "c", "Speaker 1", "", "v3", "demo"),
                ],
                Summary: "sum"),
            duration);

    private sealed class RecordingAnalyzer(
        Func<Segment, StageBCallOptions, StageBModelResponse> responder) : IVideoAnalyzer
    {
        public List<StageBCallOptions> Calls { get; } = [];

        public Task<StageBModelResponse> AnalyzeAsync(
            string sourceUri,
            Segment segment,
            StageBCallOptions callOptions,
            CancellationToken cancellationToken)
        {
            Calls.Add(callOptions);
            return Task.FromResult(responder(segment, callOptions));
        }
    }
}
