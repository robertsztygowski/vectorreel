using VectorReel.Core.Domain;
using VectorReel.Core.Pipeline.StageA;

namespace VectorReel.Tests.Unit.StageA;

public sealed class SegmenterTests
{
    private static readonly StageAOptions _options = StageAOptions.Default;

    private static ContentTimeline Timeline(
        int durationSeconds,
        IEnumerable<int>? cueSeconds = null,
        IEnumerable<StaticRun>? staticRuns = null) =>
        new(
            TimeSpan.FromSeconds(durationSeconds),
            [.. staticRuns ?? []],
            [.. (cueSeconds ?? []).Select(s => new BlockCue(TimeSpan.FromSeconds(s), CueKind.Interval))],
            StaticFractionInRuns: 0);

    [Fact]
    public void A_video_shorter_than_the_target_is_one_segment_with_no_overlap()
    {
        var segment = Assert.Single(Segmenter.Plan(Timeline(300), _options));

        Assert.Equal(TimeSpan.Zero, segment.Start);
        Assert.Equal(TimeSpan.FromSeconds(300), segment.End);
        Assert.Equal(TimeSpan.Zero, segment.OverlapBefore);
    }

    [Fact]
    public void Segments_cover_the_whole_video_with_no_gaps()
    {
        var timeline = Timeline(3600);

        var segments = Segmenter.Plan(timeline, _options);

        Assert.Equal(TimeSpan.Zero, segments[0].Start);
        Assert.Equal(timeline.Duration, segments[^1].End);
        foreach (var (previous, next) in segments.Zip(segments.Skip(1)))
        {
            // The next segment starts before the previous one ends — that IS the overlap. What must never
            // happen is a gap, which would silently drop that stretch of video from the output.
            Assert.True(next.Start <= previous.End, $"gap between segment {previous.Index} and {next.Index}");
            Assert.Equal(previous.End - next.Start, next.OverlapBefore);
        }
    }

    [Fact]
    public void Every_segment_after_the_first_replays_the_tail_of_its_predecessor()
    {
        var segments = Segmenter.Plan(Timeline(3600), _options);

        Assert.Equal(TimeSpan.Zero, segments[0].OverlapBefore);
        Assert.All(segments.Skip(1), s => Assert.Equal(_options.SegmentOverlap, s.OverlapBefore));
    }

    [Fact]
    public void A_cut_snaps_to_a_nearby_block_boundary()
    {
        // The target cut is at 12:00 (720 s). A boundary at 700 s is inside the snap window, so the cut
        // moves there — landing on a scene change or a pause rather than through the middle of a sentence.
        var segments = Segmenter.Plan(Timeline(1800, cueSeconds: [700]), _options);

        Assert.Equal(TimeSpan.FromSeconds(700), segments[0].End);
    }

    [Fact]
    public void A_cut_with_no_boundary_nearby_falls_on_the_target()
    {
        var segments = Segmenter.Plan(Timeline(1800, cueSeconds: [100]), _options);

        Assert.Equal(_options.SegmentTarget, segments[0].End);
    }

    [Fact]
    public void A_trailing_sliver_is_absorbed_rather_than_billed_as_its_own_call()
    {
        // 12 minutes and 5 seconds. Cutting at 12:00 would leave a 5-second segment — a whole extra
        // Vertex call to analyse five seconds of video.
        var segment = Assert.Single(Segmenter.Plan(Timeline(725), _options));

        Assert.Equal(TimeSpan.FromSeconds(725), segment.End);
    }

    [Fact]
    public void Cues_are_carried_into_the_segment_that_contains_them_relative_to_its_start()
    {
        // Stage B's prompt speaks in segment time, not video time, so the boundaries must be rebased.
        var segments = Segmenter.Plan(Timeline(1800, cueSeconds: [60, 900]), _options);

        Assert.Equal([TimeSpan.FromSeconds(60)], segments[0].Cues);

        var second = segments[1];
        Assert.Equal([TimeSpan.FromSeconds(900) - second.Start], second.Cues);
    }

    [Fact]
    public void A_mostly_static_segment_is_routed_to_the_cheap_config()
    {
        var timeline = Timeline(600, staticRuns: [new StaticRun(0, 500)]);

        var segment = Assert.Single(Segmenter.Plan(timeline, _options));

        Assert.Equal(MediaResolution.Low, segment.Sampling.MediaResolution);
    }

    [Fact]
    public void A_moving_segment_keeps_the_default_resolution()
    {
        var timeline = Timeline(600, staticRuns: [new StaticRun(0, 60)]);

        var segment = Assert.Single(Segmenter.Plan(timeline, _options));

        Assert.Equal(MediaResolution.Default, segment.Sampling.MediaResolution);
    }

    [Fact]
    public void An_empty_video_plans_nothing()
    {
        Assert.Empty(Segmenter.Plan(Timeline(0), _options));
    }
}
