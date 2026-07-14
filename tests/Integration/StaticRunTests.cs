using VectorReel.Core.Media;
using VectorReel.Core.Pipeline.StageA;

namespace VectorReel.Tests.Integration;

/// <summary>Static-run detection against real video. The cost lever, measured end to end.</summary>
public sealed class StaticRunTests
{
    private readonly FfmpegMediaScanner _scanner = new(new MediaToolOptions());

    private async Task<double> StaticFractionOf(string path)
    {
        var scan = await _scanner.ScanAsync(path, CancellationToken.None);
        var runs = StaticRunDetector.Detect(scan.Frames, StageAOptions.Default);
        return StaticRunDetector.StaticFraction(runs, scan.SampledSeconds);
    }

    // Bands, not exact values: a one-frame shift from a different ffmpeg build must not turn the
    // suite red, but a change that moves a category's cost profile must.
    [Theory]
    [InlineData("slide_talk_fosdem_curl.mp4", 0.90, 1.00)]      // one slide, held for 90 s — nearly all static
    [InlineData("screencast_blender_lesson.mp4", 0.40, 0.65)]   // a UI that is worked in, then left alone
    [InlineData("talking_head_nasa_bolten.mp4", 0.05, 0.20)]    // a moving head against a fixed background
    public async Task Static_fraction_of_each_category_sits_in_its_measured_band(string file, double low, double high)
    {
        var fraction = await StaticFractionOf(Path.Combine(Fixtures.VideoDirectory, file));

        Assert.InRange(fraction, low, high);
    }

    // 🚩 This pins a KNOWN GAP so nobody closes it by accident.
    //
    // A talking head has nothing on screen worth reading, so it is the most obvious candidate for the
    // cheap config — yet it routes expensive, because the head moves and this detector measures motion.
    // The tempting fix is to raise StaticMeanAbsDiffThreshold to 5.0. Doing so also inflates the static
    // share of a real demo recording well past what Phase 0 measured, which silently invalidates the
    // cost blend in METRICS.md N4. If you want the cheap win, N4 must be re-measured in the same commit.
    [Fact]
    public async Task A_talking_head_is_not_routed_cheap_and_that_is_a_known_gap_not_a_bug()
    {
        Assert.True(await StaticFractionOf(Fixtures.TalkingHead) < 0.20);
    }
}
