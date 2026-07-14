using VectorReel.Core.Domain;
using VectorReel.Core.Pipeline.StageA;

namespace VectorReel.Tests.Unit.StageA;

/// <summary>
/// The specification of the fix for METRICS.md N7b.
///
/// The failure being fixed: on continuous screen recordings — the ICP's own content type — Stage B
/// emits ~86-second blocks (7 per 10-minute segment against ~25 s on slide talks), so citations land
/// on vague spans exactly where the paying customers are. The private path holds the bytes, so it can
/// compute the boundaries and hand them over rather than hoping the model finds them.
///
/// The insight these tests encode, and the one that took a measurement to see: <b>the benchmark's
/// worst window is 94% static.</b> It is a man talking over a frozen IDE for three minutes at a
/// stretch. So a boundary rule that waits for the picture to change never fires there — which is
/// precisely why the model has nothing to segment on either. Boundaries must be forced on elapsed
/// <em>narration</em>, and suppressed on <em>silence</em>, never on stillness.
/// </summary>
public sealed class CueDetectorTests
{
    private static readonly StageAOptions _options = StageAOptions.Default;

    private static IReadOnlyList<BlockCue> Detect(
        IReadOnlyList<GrayFrame> frames,
        IReadOnlyList<SilenceInterval>? silences = null) =>
        CueDetector.Detect(frames, silences ?? [], _options);

    private static IEnumerable<int> SecondsOf(IEnumerable<BlockCue> cues) =>
        cues.Select(c => (int)c.At.TotalSeconds);

    // ---- The N7b case itself ----

    /// <summary>
    /// 🚨 The whole point of the phase. A frozen screen with someone talking over it gets boundaries.
    /// This is the case where every pixel-based rule produces nothing at all.
    /// </summary>
    [Fact]
    public void A_frozen_screen_with_someone_talking_over_it_still_gets_boundaries()
    {
        var frames = Frames.Frozen(0, 200).ToList();

        var cues = Detect(frames);

        Assert.NotEmpty(cues);
        Assert.All(cues, c => Assert.Equal(CueKind.Interval, c.Kind));
        Assert.True(
            cues.Count >= 3,
            $"200 s of narration over a frozen screen produced only {cues.Count} boundaries — this is N7b");
    }

    [Fact]
    public void Forced_boundaries_never_leave_a_span_longer_than_the_maximum_block()
    {
        var frames = Frames.Frozen(0, 300).ToList();

        var timeline = new ContentTimeline(
            TimeSpan.FromSeconds(300), [], Detect(frames), StaticFractionInRuns: 1);

        // The symptom of N7b was a 162-second block. Nothing may exceed the configured ceiling.
        Assert.True(
            timeline.LongestUncuedSpan <= TimeSpan.FromSeconds(_options.CueMaxBlockSeconds),
            $"longest uncued span was {timeline.LongestUncuedSpan}");
    }

    // ---- Suppression: on silence, not on stillness ----

    /// <summary>
    /// The correct suppression rule. Nothing is said, so there is nothing to cite — one block is right,
    /// and forcing boundaries into dead air would only manufacture empty citations.
    /// </summary>
    [Fact]
    public void A_frozen_screen_with_nobody_talking_gets_no_forced_boundaries()
    {
        var frames = Frames.Frozen(0, 200).ToList();
        List<SilenceInterval> deadAir = [new(TimeSpan.Zero, TimeSpan.FromSeconds(200))];

        Assert.Empty(Detect(frames, deadAir));
    }

    /// <summary>
    /// The rule that was tempting and wrong: suppressing forced boundaries inside a static run. It reads
    /// as sensible ("a slide held for five minutes is one block") and it is right about slides — but the
    /// paying customer's footage is 94% static <em>while narrated</em>, so the rule switches the fix off
    /// exactly where it is needed and leaves 160-second blocks behind.
    /// </summary>
    [Fact]
    public void Stillness_alone_does_not_suppress_a_boundary()
    {
        var frames = Frames.Frozen(0, 200).ToList();

        // Short pauses between sentences — the speaker is talking throughout.
        List<SilenceInterval> pauses =
        [
            new(TimeSpan.FromSeconds(44), TimeSpan.FromSeconds(45)),
            new(TimeSpan.FromSeconds(89), TimeSpan.FromSeconds(90)),
        ];

        Assert.NotEmpty(Detect(frames, pauses));
    }

    // ---- Snapping ----

    [Fact]
    public void A_forced_boundary_lands_on_a_pause_in_speech_when_one_is_near()
    {
        var frames = Frames.Frozen(0, 120).ToList();
        List<SilenceInterval> pauses = [new(TimeSpan.FromSeconds(39), TimeSpan.FromSeconds(41))];

        var cues = Detect(frames, pauses);

        // Without the pause the first forced boundary would fall at CueMaxBlockSeconds (45). The pause
        // at 40 is inside the snap window, so the boundary moves there — between two sentences rather
        // than through the middle of one.
        Assert.Equal(40, (int)cues[0].At.TotalSeconds);
    }

    [Fact]
    public void A_forced_boundary_falls_on_the_raw_tick_when_no_pause_is_near()
    {
        var frames = Frames.Frozen(0, 120).ToList();

        Assert.Equal(_options.CueMaxBlockSeconds, (int)Detect(frames)[0].At.TotalSeconds);
    }

    // ---- Visual change ----

    [Fact]
    public void A_hard_cut_is_a_boundary()
    {
        List<GrayFrame> frames = [.. Frames.Frozen(0, 30, 20), .. Frames.Frozen(30, 30, 220)];

        var cues = Detect(frames);

        Assert.Equal(CueKind.VisualChange, cues[0].Kind);
        Assert.Equal(30, (int)cues[0].At.TotalSeconds);
    }

    [Fact]
    public void Drift_is_measured_from_the_anchor_so_slow_continuous_change_is_eventually_a_boundary()
    {
        // Each second the picture changes by only ~1/255 — far below any scene-cut threshold, so
        // frame-to-frame differencing would never fire. Against the anchor it accumulates. This is the
        // typing-and-scrolling case, and it is why the anchor exists.
        var frames = Enumerable.Range(0, 60).Select(s => Frames.Flat(s, (byte)(20 + s))).ToList();

        var cues = Detect(frames);

        Assert.Contains(cues, c => c.Kind == CueKind.VisualChange);
    }

    [Fact]
    public void A_moving_head_against_a_fixed_background_is_not_a_scene_change()
    {
        // The head oscillates within a bounded envelope; it never becomes a different scene. Firing a
        // VisualChange here would shred a talking head into meaningless blocks.
        var frames = Enumerable.Range(0, 60)
            .Select(s => Frames.Partial(s, changedFraction: 0.02, value: (byte)(s % 2 == 0 ? 40 : 60)))
            .ToList();

        Assert.DoesNotContain(Detect(frames), c => c.Kind == CueKind.VisualChange);
    }

    // ---- Invariants ----

    [Fact]
    public void Boundaries_are_strictly_increasing_and_inside_the_video()
    {
        List<GrayFrame> frames = [.. Frames.Frozen(0, 60, 20), .. Frames.Frozen(60, 60, 220)];

        var cues = Detect(frames);

        Assert.Equal(SecondsOf(cues).Order(), SecondsOf(cues));
        Assert.Equal(SecondsOf(cues).Distinct(), SecondsOf(cues));
        Assert.All(cues, c => Assert.InRange(c.At.TotalSeconds, 0, frames.Count));
    }

    [Fact]
    public void Boundaries_are_never_closer_together_than_the_minimum_spacing()
    {
        // A strobing picture: every second is a scene change. Without a floor on spacing this would
        // emit a boundary per second and citations would be noise.
        var frames = Enumerable.Range(0, 120).Select(s => Frames.Flat(s, (byte)(s % 2 == 0 ? 10 : 240))).ToList();

        var seconds = SecondsOf(Detect(frames)).ToList();

        Assert.All(
            seconds.Zip(seconds.Skip(1), (a, b) => b - a),
            gap => Assert.True(gap >= _options.CueMinSpacingSeconds, $"boundaries {gap}s apart"));
    }

    [Fact]
    public void There_is_never_a_boundary_at_zero_because_the_first_block_starts_there_anyway()
    {
        List<GrayFrame> frames = [.. Frames.Frozen(0, 60)];

        Assert.DoesNotContain(Detect(frames), c => c.At == TimeSpan.Zero);
    }

    [Fact]
    public void A_video_too_short_to_need_a_boundary_gets_none()
    {
        Assert.Empty(Detect([.. Frames.Frozen(0, 10)]));
        Assert.Empty(Detect([]));
    }
}
