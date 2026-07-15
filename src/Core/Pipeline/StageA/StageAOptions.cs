namespace MdReel.Core.Pipeline.StageA;

/// <summary>
/// Every tunable in Stage A, in one place, each labelled with where it came from.
///
/// The distinction matters more than the values:
/// <list type="bullet">
///   <item><description>
///     <b>Ported</b> — copied from the Phase-0 experiment and <b>not free to change</b>. The measured
///     cost blend (METRICS.md N4) was produced with these exact numbers; moving them silently
///     invalidates it. A calibration test asserts the port still reproduces the original measurement.
///   </description></item>
///   <item><description>
///     <b>Assumed</b> — chosen by calibrating against one private 50-minute demo (n = 1). They are
///     honest first guesses, not measurements, and they will move once Stage B can be run against them.
///   </description></item>
/// </list>
/// </summary>
public sealed record StageAOptions
{
    /// <summary>The shipped configuration.</summary>
    public static StageAOptions Default { get; } = new();

    // ---- Static-content detection: the cost lever. PORTED — do not tune. ----

    /// <summary>
    /// A second is "static" when the mean absolute pixel difference to the previous sampled frame is
    /// below this (0–255 scale). <b>Ported, load-bearing.</b> Raising it to 5.0 would route talking
    /// heads to the cheap config — tempting — but it also inflates the measured static share of a real
    /// demo recording, and that share is exactly what METRICS.md N4's blended cost rests on. Any change
    /// here requires re-measuring N4 in the same commit.
    /// </summary>
    public double StaticMeanAbsDiffThreshold { get; init; } = 2.0;

    /// <summary>
    /// Shortest run of static seconds worth routing to the cheap config. Below this the switch costs
    /// more than it saves. <b>Ported.</b>
    /// </summary>
    public int StaticMinRunSeconds { get; init; } = 10;

    /// <summary>Share of a segment that must be static before the whole segment is analysed at low media resolution. Assumed.</summary>
    public double LowResolutionStaticFraction { get; init; } = 0.5;

    // ---- Block cues: the fix for under-segmentation (METRICS.md N7b). ASSUMED, n = 1. ----

    /// <summary>
    /// How far the picture must drift from the last cue's frame before it counts as a new scene
    /// (mean absolute difference, 0–255). Assumed. In practice the value barely matters: a sweep from
    /// 4 to 10 changes the cue count by a few percent, because on real footage the picture is either
    /// plainly changing or plainly not.
    /// </summary>
    public double CueDriftThreshold { get; init; } = 6.0;

    /// <summary>
    /// Never place two boundaries closer than this. Stops cue spam on high-motion footage, and is
    /// the effective block length while the picture is changing continuously. Assumed.
    /// </summary>
    public int CueMinSpacingSeconds { get; init; } = 20;

    /// <summary>
    /// 🚨 <b>The number that fixes N7b.</b> If the picture has not changed for this long but the
    /// presenter is still talking, force a boundary anyway.
    ///
    /// This exists because the pixels give no signal at all in the case that matters. The benchmark's
    /// worst window is 94% static — a frozen IDE with a man talking over it for three minutes at a
    /// stretch — so a boundary rule that waits for the picture to move never fires, and Stage B runs
    /// the whole thing together into ~86-second blocks. Forcing on elapsed narration, not on motion,
    /// is what takes that window from 7 blocks per 10 minutes to 17.
    /// </summary>
    public int CueMaxBlockSeconds { get; init; } = 45;

    /// <summary>
    /// A forced boundary is nudged to the nearest pause in speech within this window, so it lands
    /// between sentences rather than on an arbitrary tick. Assumed.
    /// </summary>
    public int CueSnapWindowSeconds { get; init; } = 12;

    /// <summary>
    /// Silence this long means nobody is talking, so there is nothing to cite and no boundary is
    /// forced. <b>This — not stillness — is the correct suppression rule.</b> Assumed.
    /// </summary>
    public int DeadAirSeconds { get; init; } = 20;

    // ---- Audio ----

    /// <summary>Noise floor for pause detection, in negative dBFS. Assumed.</summary>
    public int SilenceNoiseFloorDb { get; init; } = -30;

    /// <summary>Shortest gap in speech that counts as a pause, in seconds. Assumed.</summary>
    public double SilenceMinDurationSeconds { get; init; } = 0.4;

    // ---- Segmentation (ARCHITECTURE.md §3, step 3) ----

    /// <summary>
    /// Target segment length. 10–15 minutes per ARCHITECTURE.md §3; 12 sits in the middle.
    ///
    /// ⚠️ Segment length does <b>not</b> bound Stage B's output — on-screen text density does, and a
    /// dense slide can overflow the output cap even at 10 minutes. Sizing here is a heuristic, and
    /// <c>SegmentSplitPolicy</c> is the backstop that has to exist regardless.
    /// </summary>
    public TimeSpan SegmentTarget { get; init; } = TimeSpan.FromMinutes(12);

    /// <summary>Overlap between consecutive segments, so context is not lost at a boundary. Stage C merges the duplicates out.</summary>
    public TimeSpan SegmentOverlap { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>A segment boundary snaps to a cue if one falls within this of the target. Keeps segments from cutting mid-sentence.</summary>
    public TimeSpan SegmentSnapWindow { get; init; } = TimeSpan.FromSeconds(45);

    /// <summary>
    /// Refuse to split below this. Guarantees the Stage B overflow loop terminates instead of
    /// halving forever on a segment that will never fit.
    /// </summary>
    public TimeSpan MinSegmentLength { get; init; } = TimeSpan.FromSeconds(30);
}
