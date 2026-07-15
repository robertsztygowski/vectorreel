using MdReel.Core.Domain;
using MdReel.Core.Media;
using MdReel.Core.Pipeline.StageA;

namespace MdReel.Tests.Integration;

/// <summary>
/// Cue detection against the committed CC clips. Unlike the calibration suite these run on any clone,
/// so they are the reproducible half of the evidence.
///
/// They assert <b>bands and invariants</b>, never exact timestamps: a different ffmpeg build can shift
/// a boundary by a second, and a suite that goes red for that would be retuned until it stopped meaning
/// anything. What must not shift is the behaviour per category.
/// </summary>
public sealed class CueTests
{
    private readonly FfmpegMediaScanner _scanner = new(new MediaToolOptions());

    private async Task<ContentTimeline> Analyze(string path)
    {
        var scan = await _scanner.ScanAsync(path, CancellationToken.None);
        var runs = StaticRunDetector.Detect(scan.Frames, StageAOptions.Default);

        return new ContentTimeline(
            TimeSpan.FromSeconds(scan.SampledSeconds),
            runs,
            CueDetector.Detect(scan.Frames, scan.Silences, StageAOptions.Default),
            StaticRunDetector.StaticFraction(runs, scan.SampledSeconds));
    }

    /// <summary>
    /// The public analogue of the ICP's content, and the category the product is weakest on. This is the
    /// fixture that has to work.
    /// </summary>
    [Fact]
    public async Task The_screen_recording_is_cued_far_more_finely_than_the_model_manages_alone()
    {
        var timeline = await Analyze(Fixtures.Screencast);

        // Stage B produces ~7 blocks per 10 minutes here unaided (METRICS.md N7b), and N7b fails below 10.
        Assert.True(timeline.CuesPerTenMinutes >= 13, $"only {timeline.CuesPerTenMinutes:F1} per 10 min");
        Assert.True(
            timeline.LongestUncuedSpan <= TimeSpan.FromSeconds(StageAOptions.Default.CueMaxBlockSeconds),
            $"worst block is {timeline.LongestUncuedSpan.TotalSeconds:F0}s");
    }

    /// <summary>
    /// The strong category must not be made worse. A slide talk already gets ~25-second blocks and scores
    /// 99% (METRICS.md N30), so cues are there as a floor, not to shred it.
    /// </summary>
    [Fact]
    public async Task The_slide_talk_is_not_shredded()
    {
        var timeline = await Analyze(Fixtures.SlideTalk);

        Assert.InRange(timeline.CuesPerTenMinutes, 0, 30);
        Assert.All(
            timeline.Cues,
            c => Assert.InRange(c.At, TimeSpan.Zero, timeline.Duration));
    }

    [Theory]
    [MemberData(nameof(Fixtures.All), MemberType = typeof(Fixtures))]
    public async Task Boundaries_are_ordered_spaced_and_inside_the_video(string path)
    {
        var timeline = await Analyze(path);
        var seconds = timeline.Cues.Select(c => c.At.TotalSeconds).ToList();

        Assert.All(timeline.Cues, c => Assert.InRange(c.At, TimeSpan.Zero, timeline.Duration));
        Assert.Equal(seconds.Order(), seconds);
        Assert.All(
            seconds.Zip(seconds.Skip(1), (a, b) => b - a),
            gap => Assert.True(gap >= StageAOptions.Default.CueMinSpacingSeconds, $"boundaries {gap}s apart"));
    }

    [Theory]
    [MemberData(nameof(Fixtures.All), MemberType = typeof(Fixtures))]
    public async Task No_category_is_left_with_a_block_as_coarse_as_the_failure_we_are_fixing(string path)
    {
        var timeline = await Analyze(path);

        // The measured failure was an 86-second average with 162-second spans. A narrated video must
        // never leave a span that coarse — a static, silent one legitimately may, which is why this is
        // asserted against the fixtures (all narrated) rather than as a universal law.
        Assert.True(
            timeline.LongestUncuedSpan < TimeSpan.FromSeconds(86),
            $"{Path.GetFileName(path)} has an {timeline.LongestUncuedSpan.TotalSeconds:F0}s block");
    }
}
