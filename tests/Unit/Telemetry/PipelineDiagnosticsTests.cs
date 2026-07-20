using System.Diagnostics.Metrics;
using MdReel.Core;
using MdReel.Core.Domain;
using MdReel.Core.Pipeline.StageA;
using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;

namespace MdReel.Tests.Unit.Telemetry;

public sealed class PipelineDiagnosticsTests
{
    [Fact]
    public void Job_metrics_are_recorded_with_low_cardinality_tags()
    {
        using var listener = MeasurementCollector.Start();

        PipelineDiagnostics.RecordJobDuration(TimeSpan.FromSeconds(2), "completed");
        PipelineDiagnostics.AddJobVideoMinutes(TimeSpan.FromMinutes(3));

        Assert.Contains(listener.Measurements, m =>
            m.Name == "mdreel.job.duration"
            && m.Value == 2
            && m.Tags.TryGetValue("outcome", out var outcome)
            && outcome == "completed");
        Assert.Contains(listener.Measurements, m =>
            m.Name == "mdreel.job.video_minutes" && m.Value == 3);
    }

    [Fact]
    public async Task StageB_metrics_include_duration_runaway_and_tokens()
    {
        using var listener = MeasurementCollector.Start();
        var fake = new MaxTokensOnceAnalyzer();
        var runner = new StageBRunner(fake);

        await runner.AnalyzeAsync(
            "gs://x",
            Segment(0, 240),
            StageAOptions.Default,
            StageBOptions.Default with { ForceCueBoundarySegmentation = false },
            CancellationToken.None,
            "job_test");

        Assert.Contains(listener.Measurements, m =>
            m.Name == "mdreel.stage.duration"
            && m.Tags.TryGetValue("stage", out var stage)
            && stage == "B");
        Assert.Contains(listener.Measurements, m => m.Name == "mdreel.stageb.runaway" && m.Value == 1);
        Assert.Contains(listener.Measurements, m =>
            m.Name == "mdreel.llm.tokens"
            && m.Value == 11
            && m.Tags.TryGetValue("kind", out var kind)
            && kind == "input"
            && m.Tags.TryGetValue("region", out var region)
            && region == "europe-central2");
    }

    private static Segment Segment(int startSeconds, int endSeconds) =>
        new(
            Index: 1,
            Start: TimeSpan.FromSeconds(startSeconds),
            End: TimeSpan.FromSeconds(endSeconds),
            OverlapBefore: TimeSpan.Zero,
            Sampling: new SamplingPlan(MediaResolution.Default, 0),
            Cues: []);

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
            duration,
            "europe-central2",
            InputTokens: 11,
            OutputTokens: 7,
            ThinkingTokens: 3);

    private sealed class MaxTokensOnceAnalyzer : IVideoAnalyzer
    {
        public Task<StageBModelResponse> AnalyzeAsync(
            string sourceUri,
            Segment segment,
            StageBCallOptions callOptions,
            CancellationToken cancellationToken)
        {
            var response = segment.Duration.TotalSeconds >= 120
                ? new StageBModelResponse(
                    StageBFinishReason.MaxTokens,
                    null,
                    null,
                    "europe-central2",
                    InputTokens: 11,
                    OutputTokens: 7,
                    ThinkingTokens: 3)
                : ValidResponse(segment.Duration);

            return Task.FromResult(response);
        }
    }

    private sealed class MeasurementCollector : IDisposable
    {
        // Metrics are emitted process-wide: pipeline code running in parallel test classes also
        // hits this listener, so collection must be thread-safe and reads must snapshot.
        private readonly System.Collections.Concurrent.ConcurrentQueue<CollectedMeasurement> _measurements = new();
        private readonly MeterListener _listener = new();

        private MeasurementCollector()
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == PipelineDiagnostics.SourceName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };

            _listener.SetMeasurementEventCallback<double>(Record);
            _listener.SetMeasurementEventCallback<long>(Record);
        }

        public IReadOnlyList<CollectedMeasurement> Measurements => [.. _measurements];

        public static MeasurementCollector Start()
        {
            var collector = new MeasurementCollector();
            collector._listener.Start();
            return collector;
        }

        public void Dispose() => _listener.Dispose();

        private void Record<T>(
            Instrument instrument,
            T measurement,
            ReadOnlySpan<KeyValuePair<string, object?>> tags,
            object? state)
            where T : struct
        {
            var value = Convert.ToDouble(measurement);
            var tagMap = tags.ToArray().ToDictionary(
                static tag => tag.Key,
                static tag => tag.Value?.ToString() ?? string.Empty,
                StringComparer.Ordinal);
            _measurements.Enqueue(new CollectedMeasurement(instrument.Name, value, tagMap));
        }
    }

    private sealed record CollectedMeasurement(string Name, double Value, IReadOnlyDictionary<string, string> Tags);
}
