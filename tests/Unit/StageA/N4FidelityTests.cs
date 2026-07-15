using System.Text.Json;
using VectorReel.Core.Pipeline.StageA;

namespace VectorReel.Tests.Unit.StageA;

/// <summary>
/// 🔒 <b>The fidelity gate on the cost model — now reproducible on any clone, with no video.</b>
///
/// Phase 0 measured that ~two thirds of the internal demo sits in static runs, and METRICS.md N4's
/// blended cost is built on that number. The full gate against the confidential 293 MB source lives in
/// the Integration project and skips when the file is absent — which, on a fresh clone or in CI, is
/// always. This test closes that gap: it runs the exact same detector against a committed, ~38 KB
/// derived fixture (per-second change-rate scalars, nothing reconstructable) and asserts the same
/// answer. So the number the business plan rests on is pinned everywhere, forever, for free.
///
/// <para>It pins the <em>algorithm</em>. The Integration test still pins the <em>ffmpeg extraction</em>
/// that produces those scalars; regenerate the fixture (<c>VECTORREEL_WRITE_FIXTURES=1</c>) if that ever
/// legitimately changes.</para>
/// </summary>
public sealed class N4FidelityTests
{
    [Fact]
    public void The_committed_motion_timeline_reproduces_the_static_share_Phase_0_measured()
    {
        var fixture = Load();

        var runs = StaticRunDetector.DetectFromMotion(fixture.Motion, StageAOptions.Default);

        Assert.Equal(0.667, StaticRunDetector.StaticFraction(runs, fixture.SampledSeconds), 2);
        Assert.Equal(46, runs.Count);
    }

    private static MotionTimelineFixture Load()
    {
        var path = Path.Combine(RepoRoot(), "tests", "fixtures", "golden", "demo_motion.json");
        return JsonSerializer.Deserialize<MotionTimelineFixture>(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"could not read {path}");
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "VectorReel.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("could not locate the repo root");
    }

    private sealed record MotionTimelineFixture(string Source, string Note, int SampledSeconds, double[] Motion);
}
