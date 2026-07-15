namespace VectorReel.Tests.Integration;

/// <summary>
/// The private calibration assets — the ICP's own content type, and the only footage that reproduces
/// the under-segmentation failure Stage A exists to fix.
///
/// 🚨 <b>These are not in the repo and never will be.</b> They are company-confidential and ~300 MB.
/// Tests that need them skip when they are absent, so a fresh clone stays green — but a skip is a test
/// that did not run, and these are the two that matter most:
/// <list type="bullet">
///   <item><description>the demo proves the cost lever still measures what METRICS.md N4 says it measures;</description></item>
///   <item><description>seg2 is the 12.8-minute window where Stage B returned ≤3 blocks in 4 runs out of 4 — the failure itself.</description></item>
/// </list>
/// Both currently live outside the repo — a Downloads folder and an experiment's scratch directory —
/// and neither will survive a cleanup. Preserving them somewhere durable is worth doing before Phase 2
/// needs something to regress against.
/// </summary>
public static class CalibrationFixtures
{
    /// <summary>
    /// The full ~50-minute internal demo: a real screen recording, the ICP's content type.
    /// After <c>scripts/fetch-calibration.sh</c> it lives in <c>.calibration/</c>; the env var and the
    /// original Downloads path are kept as overrides for whoever has it there already.
    /// </summary>
    public static string? Demo { get; } = FirstExisting(
        Environment.GetEnvironmentVariable("VECTORREEL_CALIBRATION_DEMO"),
        Path.Combine(Fixtures.RepoRoot, ".calibration", "isolation-demo.mp4"),
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            "Isolation Component V2 demo.mp4"));

    /// <summary>
    /// The 12.8-minute window that produced the under-segmentation failure in Phase 0.
    /// <c>scripts/fetch-calibration.sh</c> regenerates it into <c>.calibration/</c> from the demo — it is
    /// a window of the master, so it is never stored separately.
    /// </summary>
    public static string? Seg2 { get; } = FirstExisting(
        Environment.GetEnvironmentVariable("VECTORREEL_CALIBRATION_SEG2"),
        Path.Combine(Fixtures.RepoRoot, ".calibration", "seg2_720p.mp4"),
        Path.Combine(Fixtures.RepoRoot, "experiments", "001-gemini-video-benchmark", "work", "seg2_720p.mp4"));

    private static string? FirstExisting(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}

/// <summary>Which private recording a calibration test needs.</summary>
public enum CalibrationAsset
{
    /// <summary>The full internal demo.</summary>
    Demo,

    /// <summary>The 12.8-minute window that reproduces the under-segmentation failure.</summary>
    Seg2,
}

/// <summary>
/// A test that needs a private recording. It skips — visibly, with a reason — when the file is not on
/// this machine, so a fresh clone is green without pretending the assertion ran.
/// </summary>
public sealed class CalibrationFactAttribute : FactAttribute
{
    /// <summary>Declare which private recording this test needs.</summary>
    public CalibrationFactAttribute(CalibrationAsset asset)
    {
        Path = asset switch
        {
            CalibrationAsset.Demo => CalibrationFixtures.Demo,
            CalibrationAsset.Seg2 => CalibrationFixtures.Seg2,
            _ => null,
        };

        if (Path is null)
        {
            Skip = $"calibration asset '{asset}' is not on this machine. It is confidential and cannot be "
                + "committed; set VECTORREEL_CALIBRATION_DEMO / VECTORREEL_CALIBRATION_SEG2 to run this test.";
        }
    }

    /// <summary>The resolved path to the recording, or null when it is absent (in which case the test is skipped).</summary>
    public string? Path { get; }
}
