using MdReel.Core.Domain;
using MdReel.Core.Pipeline.StageA;

namespace MdReel.Tests.Unit.StageA;

/// <summary>
/// The overflow path. Stage B calls this when a segment blows the model's output cap — which is a
/// deterministic failure, so the segment must get smaller rather than get another attempt.
/// </summary>
public sealed class SegmentSplitPolicyTests
{
    private static readonly StageAOptions _options = StageAOptions.Default;

    private static Segment Segment(int startSeconds, int endSeconds, params int[] cueSeconds) =>
        new(
            Index: 3,
            Start: TimeSpan.FromSeconds(startSeconds),
            End: TimeSpan.FromSeconds(endSeconds),
            OverlapBefore: TimeSpan.Zero,
            Sampling: new SamplingPlan(MediaResolution.Default, 0),
            Cues: [.. cueSeconds.Select(s => TimeSpan.FromSeconds(s))]);

    [Fact]
    public void The_halves_cover_the_parent()
    {
        var (first, second) = SegmentSplitPolicy.Halve(Segment(0, 600), _options);

        Assert.Equal(TimeSpan.Zero, first.Start);
        Assert.Equal(TimeSpan.FromSeconds(300), first.End);
        Assert.Equal(TimeSpan.FromSeconds(600), second.End);
        Assert.True(second.Start <= first.End, "the halves must not leave a gap");
    }

    [Fact]
    public void The_second_half_replays_the_tail_of_the_first()
    {
        var (first, second) = SegmentSplitPolicy.Halve(Segment(0, 600), _options);

        Assert.Equal(_options.SegmentOverlap, second.OverlapBefore);
        Assert.Equal(first.End - _options.SegmentOverlap, second.Start);
    }

    [Fact]
    public void Cues_are_partitioned_and_rebased_onto_the_half_that_holds_them()
    {
        var (first, second) = SegmentSplitPolicy.Halve(Segment(0, 600, 100, 400), _options);

        Assert.Equal([TimeSpan.FromSeconds(100)], first.Cues);
        Assert.Equal([TimeSpan.FromSeconds(400) - second.Start], second.Cues);
    }

    [Fact]
    public void Splitting_is_deterministic()
    {
        var segment = Segment(120, 900, 200, 700);

        var (firstA, secondA) = SegmentSplitPolicy.Halve(segment, _options);
        var (firstB, secondB) = SegmentSplitPolicy.Halve(segment, _options);

        // Compared field by field: the records hold their cues in a list, which records compare by
        // reference, so two identical splits are not `Equal` to each other.
        Assert.Equal((firstA.Start, firstA.End), (firstB.Start, firstB.End));
        Assert.Equal((secondA.Start, secondA.End), (secondB.Start, secondB.End));
        Assert.Equal(firstA.Cues, firstB.Cues);
        Assert.Equal(secondA.Cues, secondB.Cues);
    }

    /// <summary>
    /// The termination guarantee. Without it, a segment whose output will never fit gets halved forever —
    /// and every attempt is billed. The overflow loop must be able to give up.
    /// </summary>
    [Fact]
    public void A_segment_at_the_floor_refuses_to_split_rather_than_bill_for_attempts_that_cannot_succeed()
    {
        Assert.Throws<InvalidOperationException>(
            () => SegmentSplitPolicy.Halve(Segment(0, 45), _options));
    }

    [Fact]
    public void Repeated_halving_terminates()
    {
        var segment = Segment(0, 600);
        var splits = 0;

        while (true)
        {
            try
            {
                (segment, _) = SegmentSplitPolicy.Halve(segment, _options);
                splits++;
            }
            catch (InvalidOperationException)
            {
                break;
            }

            Assert.True(splits < 20, "halving did not converge on the floor");
        }

        Assert.True(splits > 0);
        Assert.True(segment.Duration >= _options.MinSegmentLength);
    }
}
