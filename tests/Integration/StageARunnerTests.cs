using Microsoft.Extensions.Logging.Abstractions;
using VectorReel.Core.Domain;
using VectorReel.Core.Media;
using VectorReel.Core.Pipeline.StageA;

namespace VectorReel.Tests.Integration;

/// <summary>
/// Stage A driven end to end, exactly as the Worker will drive it: a real file in, real ffmpeg, a
/// segment plan out. This is the flow the definition of done asks to be exercised — and it needs no
/// cloud, because Stage A is pure local compute.
/// </summary>
public sealed class StageARunnerTests
{
    private readonly InMemoryCostLedger _ledger = new();
    private readonly StageARunner _stageA;

    public StageARunnerTests()
    {
        var tools = new MediaToolOptions();
        _stageA = new StageARunner(
            new FfprobeMediaProbe(tools),
            new FfmpegMediaScanner(tools),
            _ledger,
            NullLogger<StageARunner>.Instance);
    }

    [Fact]
    public async Task Prepares_a_screen_recording_for_analysis()
    {
        var prepared = await _stageA.PrepareAsync(
            "job-1", Fixtures.Screencast, StageAOptions.Default, CancellationToken.None);

        Assert.Equal(90, prepared.Probe.DurationSeconds);
        Assert.NotEmpty(prepared.Timeline.StaticRuns);
        Assert.NotEmpty(prepared.Timeline.Cues);

        var segment = Assert.Single(prepared.Segments);
        Assert.Equal(TimeSpan.Zero, segment.Start);
        Assert.Equal(prepared.Probe.Duration, segment.End);
        Assert.NotEmpty(segment.Cues);
    }

    [Fact]
    public async Task Every_compute_step_lands_in_the_cost_ledger()
    {
        // CLAUDE.md rule 6. ffmpeg is ~a third of true COGS and has been an estimate in every phase so
        // far (METRICS.md N5) — this is the step that closes it, so it must be metered where it happens.
        await _stageA.PrepareAsync("job-7", Fixtures.SlideTalk, StageAOptions.Default, CancellationToken.None);

        Assert.Collection(
            _ledger.Entries,
            e => Assert.Equal("stage_a.probe", e.Step),
            e => Assert.Equal("stage_a.scan", e.Step));

        Assert.All(_ledger.Entries, e =>
        {
            Assert.Equal("job-7", e.JobId);
            Assert.Equal(CostKind.Compute, e.Kind);
            Assert.Equal("seconds", e.Unit);
            Assert.True(e.Quantity > 0, "a compute step that took no time was not really measured");

            // Seconds are knowable from a laptop; euros are not. An invented rate would be worse than
            // an admitted gap — the ledger is only worth having because the numbers in it are real.
            Assert.Null(e.Cents);
        });
    }

    [Fact]
    public async Task A_segment_that_is_mostly_frozen_picture_is_routed_to_the_cheap_config()
    {
        // The slide talk holds one slide for its whole 90 seconds, so it should route cheap. This is the
        // cost lever working end to end.
        var prepared = await _stageA.PrepareAsync(
            "job-2", Fixtures.SlideTalk, StageAOptions.Default, CancellationToken.None);

        Assert.Equal(MediaResolution.Low, Assert.Single(prepared.Segments).Sampling.MediaResolution);
    }

    [Fact]
    public async Task A_corrupt_source_is_rejected_before_anything_is_decoded()
    {
        await Assert.ThrowsAsync<CorruptSourceException>(() => _stageA.PrepareAsync(
            "job-3",
            Path.Combine(Fixtures.VideoDirectory, "README.md"),
            StageAOptions.Default,
            CancellationToken.None));
    }
}
