using MdReel.Core.Domain;

namespace MdReel.Core.Pipeline.StageB;

/// <summary>Fallback when Stage B ignores cue hints: split physically at cue boundaries.</summary>
public static class StageBCueFallbackSplitter
{
    public static IReadOnlyList<Segment> Split(
        Segment segment,
        TimeSpan minSegmentLength)
    {
        if (segment.Cues.Count == 0)
        {
            return [segment];
        }

        var boundaries = new List<TimeSpan> { TimeSpan.Zero };
        boundaries.AddRange(segment.Cues.Where(cue => cue > TimeSpan.Zero && cue < segment.Duration));
        boundaries.Add(segment.Duration);

        boundaries = [.. boundaries.Distinct().OrderBy(boundary => boundary)];
        var mergedBoundaries = new List<TimeSpan> { boundaries[0] };
        for (var i = 1; i < boundaries.Count; i++)
        {
            if (boundaries[i] - mergedBoundaries[^1] >= minSegmentLength || i == boundaries.Count - 1)
            {
                mergedBoundaries.Add(boundaries[i]);
            }
        }

        if (mergedBoundaries.Count <= 2)
        {
            return [segment];
        }

        var pieces = new List<Segment>(mergedBoundaries.Count - 1);
        for (var i = 0; i < mergedBoundaries.Count - 1; i++)
        {
            var startOffset = mergedBoundaries[i];
            var endOffset = mergedBoundaries[i + 1];
            var start = segment.Start + startOffset;
            var end = segment.Start + endOffset;

            pieces.Add(segment with
            {
                Start = start,
                End = end,
                OverlapBefore = TimeSpan.Zero,
                Cues = [.. segment.Cues
                    .Where(cue => cue > startOffset && cue < endOffset)
                    .Select(cue => cue - startOffset)],
            });
        }

        return pieces;
    }
}

