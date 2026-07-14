using VectorReel.Core.Domain;
using VectorReel.Core.Pipeline.StageA;

namespace VectorReel.Tests.Unit.StageA;

/// <summary>
/// This is a <b>port-fidelity</b> suite, not a design suite.
///
/// The static lever is the mechanism behind the measured cost blend (METRICS.md N4): Phase 0 found
/// that ~two thirds of a real demo recording sits in static runs, and routed that share to the cheap
/// config. These tests pin the port's edges — strict comparison, minimum run length, a run still open
/// at the end of the video, the denominator — because a subtle difference from the original changes
/// that fraction, and changing it silently invalidates a number the business plan rests on.
///
/// Note the indexing throughout: the motion timeline compares each second to the one before it, so it
/// is one sample shorter than the frame timeline. Runs are expressed in that space, exactly as in the
/// original.
/// </summary>
public sealed class StaticRunDetectorTests
{
    private static readonly StageAOptions _options = StageAOptions.Default;

    [Fact]
    public void A_frozen_picture_is_one_static_run_covering_the_whole_motion_timeline()
    {
        var runs = StaticRunDetector.Detect([.. Frames.Frozen(0, 30)], _options);

        var run = Assert.Single(runs);
        Assert.Equal(0, run.StartSecond);
        Assert.Equal(29, run.EndSecondExclusive);
        Assert.Equal(1.0, StaticRunDetector.StaticFraction(runs, 30), 3);
    }

    [Fact]
    public void A_run_shorter_than_the_minimum_is_not_worth_switching_config_for()
    {
        // Below StaticMinRunSeconds (10) the config switch costs more than it saves, so the original
        // discards the run — and so must we.
        List<GrayFrame> frames = [.. Frames.Frozen(0, 9)];
        frames.AddRange(Enumerable.Range(9, 10).Select(s => Frames.Flat(s, (byte)(s % 2 == 0 ? 10 : 200))));

        Assert.Empty(StaticRunDetector.Detect(frames, _options));
    }

    [Fact]
    public void A_run_exactly_at_the_minimum_is_kept()
    {
        List<GrayFrame> frames = [Frames.Flat(0, 10)];
        frames.AddRange(Frames.Frozen(1, 11, 200));

        var run = Assert.Single(StaticRunDetector.Detect(frames, _options));
        Assert.Equal(10, run.Seconds);
    }

    [Fact]
    public void A_run_still_open_at_the_end_of_the_video_is_closed_and_kept()
    {
        // Most recordings end on a frozen slide, so dropping the trailing run would quietly lose the
        // cheap tail of nearly every video.
        List<GrayFrame> frames = [Frames.Flat(0, 10), Frames.Flat(1, 200)];
        frames.AddRange(Frames.Frozen(2, 20, 200));

        var run = Assert.Single(StaticRunDetector.Detect(frames, _options));
        Assert.Equal(frames.Count - 1, run.EndSecondExclusive);
    }

    [Fact]
    public void The_comparison_is_strict_so_a_difference_exactly_at_the_threshold_is_motion()
    {
        // The original tests `diff < threshold`. A second differing by exactly 2.0 is therefore NOT
        // static. Flipping this one character moves the measured static share of every video.
        List<GrayFrame> frames = [Frames.Flat(0, 100)];
        for (var second = 1; second < 30; second++)
        {
            frames.Add(Frames.Flat(second, (byte)(second % 2 == 0 ? 100 : 102)));
        }

        Assert.Equal(2.0, StaticRunDetector.MotionTimeline(frames)[0], 6);
        Assert.Empty(StaticRunDetector.Detect(frames, _options));
    }

    [Fact]
    public void Static_fraction_divides_by_the_motion_timeline_not_the_frame_count()
    {
        // 20 frozen frames then 20 alternating ones: 19 static motion samples out of 39.
        List<GrayFrame> frames = [.. Frames.Frozen(0, 20)];
        frames.AddRange(Enumerable.Range(20, 20).Select(s => Frames.Flat(s, (byte)(s % 2 == 0 ? 10 : 200))));

        var runs = StaticRunDetector.Detect(frames, _options);

        Assert.Equal(19, Assert.Single(runs).Seconds);
        Assert.Equal(19.0 / 39.0, StaticRunDetector.StaticFraction(runs, frames.Count), 6);
    }

    [Fact]
    public void An_empty_or_single_frame_video_has_no_runs()
    {
        Assert.Empty(StaticRunDetector.Detect([], _options));
        Assert.Empty(StaticRunDetector.Detect([Frames.Flat(0, 10)], _options));
    }
}
