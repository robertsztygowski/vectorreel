using MdReel.Core.Domain;
using MdReel.Core.Media;
using MdReel.Core.Pipeline.StageA;

namespace MdReel.Tests.Integration;

/// <summary>
/// The tests that run against the ICP's own footage. They are the reason to trust — or distrust —
/// what Stage A claims. They skip when the private assets are absent (see <see cref="CalibrationFixtures"/>).
/// </summary>
public sealed class CalibrationTests
{
    private readonly FfmpegMediaScanner _scanner = new(new MediaToolOptions());

    /// <summary>
    /// 🔒 <b>The fidelity gate on the cost model.</b>
    ///
    /// Phase 0 measured the static share of this exact recording with a small Python script, and
    /// METRICS.md N4's blended cost is built on the answer. This asserts the C# port still produces the
    /// same number, runs and all. If it ever goes red, the port has drifted and N4 is quietly wrong —
    /// which is a far worse outcome than a failing test, because nothing else would ever tell us.
    /// </summary>
    [CalibrationFact(CalibrationAsset.Demo)]
    public async Task The_port_reproduces_the_static_share_Phase_0_measured()
    {
        var scan = await _scanner.ScanAsync(CalibrationFixtures.Demo!, CancellationToken.None);
        var runs = StaticRunDetector.Detect(scan.Frames, StageAOptions.Default);

        Assert.Equal(0.667, StaticRunDetector.StaticFraction(runs, scan.SampledSeconds), 2);
        Assert.Equal(46, runs.Count);
    }

    /// <summary>
    /// 🚨 <b>The acceptance gate for the whole phase.</b>
    ///
    /// seg2 is the 12.8-minute window where Stage B returned ≤3 blocks in 4 runs out of 4, and where the
    /// corpus run later produced 7 blocks with spans of 162 and 140 seconds. It is 94% static — a man
    /// talking over a frozen IDE — which is exactly why no pixel-based rule can find a boundary in it.
    ///
    /// N7b fails below 10 boundaries per 10 minutes. If this test ever drops under that, Stage A has
    /// stopped doing the one job this phase existed to do.
    /// </summary>
    [CalibrationFact(CalibrationAsset.Seg2)]
    public async Task The_window_that_produced_the_under_segmentation_failure_is_now_densely_cued()
    {
        var timeline = await Analyze(CalibrationFixtures.Seg2!);

        Assert.True(
            timeline.CuesPerTenMinutes >= 13,
            $"only {timeline.CuesPerTenMinutes:F1} boundaries per 10 min — N7b fails below 10");

        // The symptom was a 162-second block. Nothing may be that coarse again.
        Assert.True(
            timeline.LongestUncuedSpan <= TimeSpan.FromSeconds(60),
            $"worst block is {timeline.LongestUncuedSpan.TotalSeconds:F0}s — the failure was 162s");
    }

    /// <summary>
    /// The same, across a whole 50-minute recording rather than its worst window.
    ///
    /// The two are worth asserting separately: the full-video average is comfortably healthy even when
    /// the worst window is broken, which is precisely how the failure hid for three phases (METRICS.md
    /// N7 makes the same point — an average reassures exactly where the product is weakest).
    /// </summary>
    [CalibrationFact(CalibrationAsset.Demo)]
    public async Task A_full_length_screen_recording_is_densely_cued()
    {
        var timeline = await Analyze(CalibrationFixtures.Demo!);

        Assert.True(timeline.CuesPerTenMinutes >= 13, $"only {timeline.CuesPerTenMinutes:F1} per 10 min");
    }

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
}
