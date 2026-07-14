using VectorReel.Core.Domain;

namespace VectorReel.Tests.Unit.Domain;

public sealed class ContentTimelineTests
{
    private static ContentTimeline Timeline(int durationSeconds, params int[] cueSeconds) =>
        new(
            TimeSpan.FromSeconds(durationSeconds),
            [],
            [.. cueSeconds.Select(s => new BlockCue(TimeSpan.FromSeconds(s), CueKind.Interval))],
            StaticFractionInRuns: 0);

    [Fact]
    public void CuesPerTenMinutes_is_the_quantity_N7b_thresholds()
    {
        // 7 cues in a 10-minute segment is the measured failure (METRICS.md N7b: below 10).
        var timeline = Timeline(600, 60, 120, 180, 240, 300, 360, 420);

        Assert.Equal(7, timeline.CuesPerTenMinutes, 3);
    }

    // The symptom of N7b was not "few cues" in the abstract — it was a 162-second block that a
    // citation had to point somewhere inside. This is the number that says whether that is fixed.
    [Fact]
    public void LongestUncuedSpan_finds_the_worst_citation_in_the_video()
    {
        var timeline = Timeline(600, 30, 60, 500);

        Assert.Equal(TimeSpan.FromSeconds(440), timeline.LongestUncuedSpan);
    }

    [Fact]
    public void LongestUncuedSpan_counts_the_tail_after_the_last_cue()
    {
        var timeline = Timeline(600, 10, 20);

        Assert.Equal(TimeSpan.FromSeconds(580), timeline.LongestUncuedSpan);
    }

    [Fact]
    public void LongestUncuedSpan_of_an_uncued_video_is_the_whole_video()
    {
        Assert.Equal(TimeSpan.FromSeconds(600), Timeline(600).LongestUncuedSpan);
    }
}
