using VectorReel.Core.Media;
using VectorReel.Core.Pipeline.StageA;

namespace VectorReel.Tests.Integration;

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
}
