using System.Text.Json;
using VectorReel.Core.Media;
using VectorReel.Core.Pipeline.StageA;

namespace VectorReel.Tests.Integration;

/// <summary>
/// Writes the committed, <b>non-confidential</b> derived fixture that lets the N4 fidelity gate run on
/// any clone with no video and no cloud.
///
/// It is not a normal test — it regenerates a checked-in artefact from the confidential source, so it
/// only runs when explicitly asked (<c>VECTORREEL_WRITE_FIXTURES=1</c>) and the demo is present.
///
/// <para><b>What it emits, and why it is safe to commit.</b> One scalar per second — the mean absolute
/// pixel difference between consecutive 160×90 gray frames, i.e. the <em>rate of visual change</em>.
/// Nothing on screen can be reconstructed from a sequence of change-rates; it reveals strictly less than
/// the video's duration, which is effectively public. What it <em>does</em> pin is the exact input the
/// static-run detector consumes, so the cost-lever fidelity gate (METRICS.md N4) survives even when the
/// 293 MB source does not.</para>
/// </summary>
public sealed class FixtureGenerator
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    [CalibrationFact(CalibrationAsset.Demo)]
    public async Task Write_the_demo_motion_timeline_fixture()
    {
        // A generator, not an assertion. Off by default so a normal run never rewrites a committed
        // artefact; set VECTORREEL_WRITE_FIXTURES=1 to regenerate it. (xunit v2 has no runtime skip, so
        // this returns rather than skips — it does nothing unless explicitly asked.)
        if (Environment.GetEnvironmentVariable("VECTORREEL_WRITE_FIXTURES") != "1")
        {
            return;
        }

        var scan = await new FfmpegMediaScanner(new MediaToolOptions())
            .ScanAsync(CalibrationFixtures.Demo!, CancellationToken.None);

        // Round to 4 decimals: far below the static threshold of 2.0, so it cannot flip a
        // classification, and it keeps the fixture small and its diffs readable.
        var timeline = StaticRunDetector.MotionTimeline(scan.Frames);
        var motion = new double[timeline.Length];
        for (var i = 0; i < timeline.Length; i++)
        {
            motion[i] = Math.Round(timeline[i], 4);
        }

        var fixture = new MotionTimelineFixture(
            Source: "Isolation Component V2 demo.mp4 (confidential — not committed; see CalibrationFixtures)",
            Note: "Per-second visual change rate (mean abs diff, 160x90 gray, 1 fps). Scalars only — nothing "
                + "reconstructable. Pins the METRICS.md N4 static-lever fidelity gate without the source video.",
            SampledSeconds: scan.SampledSeconds,
            Motion: motion);

        // Self-certify before writing: the rounded values must still reproduce the Phase-0 measurement,
        // or the fixture is not fit to stand in for the video.
        var runs = StaticRunDetector.DetectFromMotion(motion, StageAOptions.Default);
        Assert.Equal(0.667, StaticRunDetector.StaticFraction(runs, fixture.SampledSeconds), 2);
        Assert.Equal(46, runs.Count);

        var path = Path.Combine(Fixtures.RepoRoot, "tests", "fixtures", "golden", "demo_motion.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(fixture, _json), CancellationToken.None);
    }
}

/// <summary>The shape of <c>tests/fixtures/golden/demo_motion.json</c>.</summary>
public sealed record MotionTimelineFixture(string Source, string Note, int SampledSeconds, double[] Motion);
