using MdReel.Core.Domain;

namespace MdReel.Core.Pipeline.StageA;

/// <summary>
/// 🚨 <b>Computes the block boundaries Stage B is told to honour. This is Stage A's most valuable
/// output, and the only thing the private path can do that the public YouTube path structurally
/// cannot — because it is the only one that holds the bytes.</b>
///
/// <para><b>The problem.</b> On continuous screen recordings — internal demos, the ICP's own content
/// type — Stage B runs the footage together into ~86-second blocks (METRICS.md N7b, N32). Citations
/// then point at a vague minute-and-a-half instead of a moment, and they do it exactly where the
/// paying customers live. Slide talks, by contrast, get ~25-second blocks and score 99%.</para>
///
/// <para><b>Why the model fails there, and why the obvious fix also fails.</b> A slide talk hands the
/// model visual events to hang boundaries on. A screen recording does not — and not because it changes
/// too continuously, which was the intuition. It is because it does not change at all: the benchmark's
/// worst window is <b>94% static</b>, a man talking over a frozen IDE for three minutes at a stretch.
/// So any rule that waits for the picture to move — scene cuts, frame differencing, drift, anything —
/// produces nothing precisely where the fix is needed. It is the same emptiness the model is looking at.</para>
///
/// <para><b>The rule.</b> A block boundary is a boundary in the <b>narration</b>, not a pixel event:
/// <list type="bullet">
///   <item><description><b>Force on elapsed speech.</b> If the picture has not changed for
///   <c>CueMaxBlockSeconds</c> but the presenter is still talking, place a boundary anyway.</description></item>
///   <item><description><b>Suppress on silence, never on stillness.</b> Nobody talking means nothing to
///   cite, so a frozen slide in dead air stays one block — which is correct. A frozen slide being
///   <em>narrated</em> is a dozen blocks, which is the whole point.</description></item>
///   <item><description><b>Snap to a pause.</b> Nudge a forced boundary onto the nearest gap in speech,
///   so it lands between sentences rather than on an arbitrary tick.</description></item>
///   <item><description><b>Follow the picture when it does move</b> — drift from the anchor frame, so a
///   cut fires at once and slow accumulation (typing, scrolling) fires eventually.</description></item>
/// </list></para>
///
/// <para>On the window that produced the failure, this takes Stage A from 7 boundaries per 10 minutes
/// with 162-second spans to 17 with a worst case under 50 seconds.</para>
///
/// <para>⚠️ <b>Cues are a floor on granularity, not a ceiling.</b> Stage B may still emit finer blocks;
/// it may not emit coarser ones. And note what Phase 1 cannot prove: that the model actually
/// <em>obeys</em> them. That verdict needs a Stage B call, and it is the first thing Phase 2 should check.</para>
/// </summary>
public static class CueDetector
{
    /// <summary>Compute the forced block boundaries for a scanned video.</summary>
    public static IReadOnlyList<BlockCue> Detect(
        IReadOnlyList<GrayFrame> frames,
        IReadOnlyList<SilenceInterval> silences,
        StageAOptions options)
    {
        var raw = DetectRaw(frames, options);
        return Refine(raw, silences, options);
    }

    /// <summary>
    /// Place boundaries from the picture alone: drift from the anchor frame, plus a forced boundary when
    /// too much time has passed without one.
    ///
    /// The anchor — rather than the previous frame — is what makes slow continuous change detectable.
    /// Typing moves a handful of pixels per second and would never cross a frame-to-frame threshold, but
    /// it drifts arbitrarily far from where the scene started. The anchor resets at every boundary, so
    /// each block is measured against its own beginning.
    /// </summary>
    internal static List<BlockCue> DetectRaw(IReadOnlyList<GrayFrame> frames, StageAOptions options)
    {
        var cues = new List<BlockCue>();
        if (frames.Count == 0)
        {
            return cues;
        }

        var anchor = frames[0];
        var lastCue = 0;

        for (var second = 1; second < frames.Count; second++)
        {
            var sinceLastCue = second - lastCue;
            if (sinceLastCue < options.CueMinSpacingSeconds)
            {
                continue;
            }

            var drift = FrameMath.MeanAbsoluteDifference(frames[second], anchor);

            var kind = drift >= options.CueDriftThreshold ? CueKind.VisualChange
                : sinceLastCue >= options.CueMaxBlockSeconds ? CueKind.Interval
                : (CueKind?)null;

            if (kind is not { } cueKind)
            {
                continue;
            }

            cues.Add(new BlockCue(TimeSpan.FromSeconds(second), cueKind));
            anchor = frames[second];
            lastCue = second;
        }

        return cues;
    }

    /// <summary>
    /// Apply the audio. A forced boundary is snapped onto a nearby pause in speech, and dropped entirely
    /// if it lands in dead air — because a boundary where nothing is said is a citation to nothing.
    ///
    /// Only <see cref="CueKind.Interval"/> boundaries are touched. A visual change is an event in its own
    /// right: the slide changed, and that is where the new block begins whether or not anyone was speaking.
    /// </summary>
    internal static List<BlockCue> Refine(
        IReadOnlyList<BlockCue> raw,
        IReadOnlyList<SilenceInterval> silences,
        StageAOptions options)
    {
        var deadAir = silences.Where(s => s.Duration >= TimeSpan.FromSeconds(options.DeadAirSeconds)).ToList();
        var pauses = silences.Where(s => s.Duration < TimeSpan.FromSeconds(options.DeadAirSeconds)).ToList();
        var snapWindow = TimeSpan.FromSeconds(options.CueSnapWindowSeconds);
        var minSpacing = TimeSpan.FromSeconds(options.CueMinSpacingSeconds);

        var refined = new List<BlockCue>();
        var lastAt = TimeSpan.Zero;

        for (var i = 0; i < raw.Count; i++)
        {
            var cue = raw[i];
            var at = cue.At;

            if (cue.Kind == CueKind.Interval)
            {
                if (deadAir.Any(s => s.Contains(at)))
                {
                    continue;
                }

                // 🚨 A snap may not encroach on its neighbours. Nudging a boundary forward onto a pause
                // can leave the NEXT boundary — often a real visual change — inside the spacing floor, so
                // it gets dropped and the two blocks merge. That trades a prettier citation for a coarser
                // one, which is the failure this class exists to prevent, reintroduced by its own polish.
                //
                // Snapping is cosmetic; a boundary is not. When they conflict, the boundary wins.
                var earliest = lastAt + minSpacing;
                var latest = i + 1 < raw.Count ? raw[i + 1].At - minSpacing : TimeSpan.MaxValue;

                var nearest = pauses
                    .Select(p => TimeSpan.FromSeconds(Math.Round(p.Midpoint.TotalSeconds)))
                    .Where(m => (m - at).Duration() <= snapWindow && m >= earliest && m <= latest)
                    .OrderBy(m => (m - at).Duration())
                    .Cast<TimeSpan?>()
                    .FirstOrDefault();

                if (nearest is { } snapped)
                {
                    at = snapped;
                }
            }

            if (at - lastAt < minSpacing)
            {
                continue;
            }

            refined.Add(cue with { At = at });
            lastAt = at;
        }

        return refined;
    }
}
