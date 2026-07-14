namespace VectorReel.Core.Domain;

/// <summary>Why Stage A placed a block boundary here. Carried through to Stage B, and worth logging.</summary>
public enum CueKind
{
    /// <summary>The picture became a different scene — a slide change, a window switch, a cut.</summary>
    VisualChange,

    /// <summary>
    /// Nothing changed on screen for too long, but the presenter kept talking. This is the cue kind
    /// that fixes under-segmentation on screen recordings (METRICS.md N7b) — and it exists because
    /// the pixels give no signal at all in exactly that case.
    /// </summary>
    Interval,
}

/// <summary>
/// A block boundary Stage A hands to Stage B. Cues are a <em>floor</em> on granularity, not a
/// ceiling: Stage B may still emit finer blocks, it may not emit coarser ones.
/// </summary>
public sealed record BlockCue(TimeSpan At, CueKind Kind);

/// <summary>
/// A stretch where the picture is essentially frozen (ARCHITECTURE.md §8, lever 1).
///
/// This drives <em>cost</em> and nothing else — it is the input to per-segment media-resolution
/// routing. It deliberately does not gate block boundaries: see <see cref="CueKind.Interval"/>.
/// </summary>
public sealed record StaticRun(int StartSecond, int EndSecondExclusive)
{
    /// <summary>Length of the run in seconds.</summary>
    public int Seconds => EndSecondExclusive - StartSecond;

    /// <summary>True when <paramref name="second"/> falls inside this run.</summary>
    public bool Contains(int second) => second >= StartSecond && second < EndSecondExclusive;
}

/// <summary>
/// What Stage A learned about the content of a video — the whole product of the analysis pass.
/// </summary>
/// <param name="Duration">Source duration, from ffprobe.</param>
/// <param name="StaticRuns">Frozen-picture runs. Routes cost.</param>
/// <param name="Cues">Forced block boundaries. Fixes citation granularity.</param>
/// <param name="StaticFractionInRuns">
/// Share of the video inside a static run. This is the number the measured cost blend rests on
/// (METRICS.md N4) — Phase 0 measured it on a real demo recording, and the port reproduces it.
/// </param>
public sealed record ContentTimeline(
    TimeSpan Duration,
    IReadOnlyList<StaticRun> StaticRuns,
    IReadOnlyList<BlockCue> Cues,
    double StaticFractionInRuns)
{
    /// <summary>Cues per ten minutes — the quantity METRICS.md N7b thresholds.</summary>
    public double CuesPerTenMinutes =>
        Duration > TimeSpan.Zero ? Cues.Count / (Duration.TotalSeconds / 600.0) : 0;

    /// <summary>
    /// The longest stretch with no boundary in it — i.e. the worst citation this video can produce.
    /// The headline symptom of N7b was a 162-second block, so this is the number to watch.
    /// </summary>
    public TimeSpan LongestUncuedSpan
    {
        get
        {
            var longest = TimeSpan.Zero;
            var previous = TimeSpan.Zero;
            foreach (var cue in Cues)
            {
                if (cue.At - previous > longest)
                {
                    longest = cue.At - previous;
                }

                previous = cue.At;
            }

            return Duration - previous > longest ? Duration - previous : longest;
        }
    }
}
