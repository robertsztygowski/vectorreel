using MdReel.Core.Domain;

namespace MdReel.Core.Pipeline.StageA;

/// <summary>
/// Cuts the video into the windows Stage B analyses (ARCHITECTURE.md §3, step 3), routes each to a
/// media resolution, and attaches the block boundaries that fall inside it.
///
/// ⚠️ <b>Segment length does not bound Stage B's output — on-screen text density does</b>, and that is
/// not knowable before the call. A dense slide deck overflows the output cap even at a 10-minute
/// window. So the sizing here is a heuristic, and <see cref="SegmentSplitPolicy"/> is the backstop
/// that has to exist regardless.
/// </summary>
public static class Segmenter
{
    /// <summary>Plan the segments for an analysed video.</summary>
    public static IReadOnlyList<Segment> Plan(ContentTimeline timeline, StageAOptions options)
    {
        if (timeline.Duration <= TimeSpan.Zero)
        {
            return [];
        }

        var boundaries = PlanBoundaries(timeline, options);
        var segments = new List<Segment>(boundaries.Count - 1);

        for (var i = 0; i < boundaries.Count - 1; i++)
        {
            // Every segment after the first replays the tail of its predecessor. Overlap stops context
            // being lost at a cut; Stage C merges the duplicated blocks back out.
            var overlap = i == 0 ? TimeSpan.Zero : options.SegmentOverlap;
            var start = boundaries[i] - overlap;
            var end = boundaries[i + 1];

            segments.Add(new Segment(
                Index: i,
                Start: start,
                End: end,
                OverlapBefore: overlap,
                Sampling: PlanSampling(timeline, start, end, options),
                Cues: [.. timeline.Cues
                    .Where(c => c.At > start && c.At < end)
                    .Select(c => c.At - start)]));
        }

        return segments;
    }

    /// <summary>
    /// Where the cuts fall. Walk forward by the target length, and snap each cut to a nearby block
    /// boundary when there is one — a cut that lands on a scene change or a pause in speech costs less
    /// context than one that lands mid-sentence.
    /// </summary>
    private static List<TimeSpan> PlanBoundaries(ContentTimeline timeline, StageAOptions options)
    {
        List<TimeSpan> boundaries = [TimeSpan.Zero];
        var cursor = TimeSpan.Zero;

        while (timeline.Duration - cursor > options.SegmentTarget)
        {
            var target = cursor + options.SegmentTarget;

            var snapped = timeline.Cues
                .Select(c => c.At)
                .Where(at => (at - target).Duration() <= options.SegmentSnapWindow && at > cursor)
                .OrderBy(at => (at - target).Duration())
                .Cast<TimeSpan?>()
                .FirstOrDefault() ?? target;

            // A trailing sliver is worse than one slightly long segment: it costs a whole extra Vertex
            // call to analyse a few seconds of video.
            if (timeline.Duration - snapped < options.MinSegmentLength)
            {
                break;
            }

            boundaries.Add(snapped);
            cursor = snapped;
        }

        boundaries.Add(timeline.Duration);
        return boundaries;
    }

    /// <summary>
    /// Route the segment to a media resolution. A window that is mostly frozen picture can be analysed
    /// at low resolution for ~45% less, and loses nothing — there is no detail moving to lose. This is
    /// the lever that produces the measured cost blend (METRICS.md N4).
    /// </summary>
    private static SamplingPlan PlanSampling(
        ContentTimeline timeline,
        TimeSpan start,
        TimeSpan end,
        StageAOptions options)
    {
        var from = (int)start.TotalSeconds;
        var to = (int)end.TotalSeconds;
        var seconds = Math.Max(1, to - from);

        var staticSeconds = timeline.StaticRuns.Sum(run =>
            Math.Max(0, Math.Min(run.EndSecondExclusive, to) - Math.Max(run.StartSecond, from)));

        var fraction = staticSeconds / (double)seconds;

        return new SamplingPlan(
            fraction >= options.LowResolutionStaticFraction ? MediaResolution.Low : MediaResolution.Default,
            fraction);
    }
}
