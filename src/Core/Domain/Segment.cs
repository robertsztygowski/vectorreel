namespace MdReel.Core.Domain;

/// <summary>
/// How much of the frame Vertex is asked to look at. The measured cost lever (ARCHITECTURE.md §8):
/// low resolution is ~45% cheaper and holds quality on static content.
///
/// ⚠️ Reducing <em>fps</em> is not on this enum on purpose. Phase 0 tested it and rejected it — it
/// destabilises timestamps and coverage. Media resolution is the lever; sampling rate is not.
/// </summary>
public enum MediaResolution
{
    /// <summary>Vertex default. Used where the picture is moving and detail is being lost otherwise.</summary>
    Default,

    /// <summary><c>MEDIA_RESOLUTION_LOW</c>. Routed to static stretches, which is where the saving comes from.</summary>
    Low,
}

/// <summary>What Stage B should be told about how to sample this segment.</summary>
/// <param name="MediaResolution">Resolution for the Vertex call.</param>
/// <param name="StaticFraction">Share of the segment that is frozen picture — the reason for the choice above.</param>
public sealed record SamplingPlan(MediaResolution MediaResolution, double StaticFraction);

/// <summary>
/// One unit of work for Stage B: a window of the source, the sampling config to analyse it with,
/// and the block boundaries Stage A already knows about inside it.
/// </summary>
/// <param name="Index">Position in the job. Half of the Stage B idempotency key (jobId + index).</param>
/// <param name="Start">Start of the window, inclusive.</param>
/// <param name="End">End of the window, exclusive.</param>
/// <param name="OverlapBefore">
/// How much of this segment repeats the tail of the previous one. Overlap prevents context loss at
/// a boundary; Stage C merges the duplicates back out.
/// </param>
/// <param name="Sampling">Per-segment cost routing.</param>
/// <param name="Cues">
/// The forced block boundaries inside this window, as offsets from the <em>segment</em> start —
/// which is the frame Stage B's prompt speaks in.
/// </param>
public sealed record Segment(
    int Index,
    TimeSpan Start,
    TimeSpan End,
    TimeSpan OverlapBefore,
    SamplingPlan Sampling,
    IReadOnlyList<TimeSpan> Cues)
{
    /// <summary>Length of the window.</summary>
    public TimeSpan Duration => End - Start;
}
