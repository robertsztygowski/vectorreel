using System.Globalization;
using MdReel.Core.Domain;
using MdReel.Core.Providers;

namespace MdReel.Infrastructure.Pipeline;

/// <summary>
/// Deterministic stand-in for <see cref="IVideoAnalyzer"/>. It cannot see the video, so it
/// synthesizes a valid Stage B response from the segment window alone — enough blocks, ascending
/// offsets, coverage near the end so the Stage B guards accept it. Used in <see
/// cref="PipelineModelMode.Fake"/> to keep local dev and the E2E suite free and offline.
/// </summary>
public sealed class FakeVideoAnalyzer : IVideoAnalyzer
{
    public Task<StageBModelResponse> AnalyzeAsync(
        string sourceUri,
        Segment segment,
        StageBCallOptions callOptions,
        CancellationToken cancellationToken)
    {
        var duration = segment.Duration <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : segment.Duration;

        // Five ascending offsets spanning ~0..0.92 of the window keep the two-sided coverage guard
        // happy (>= MinBlocksForLongSegments, coverage in [CoverageMin, CoverageMax]).
        var fractions = new[] { 0.0, 0.25, 0.5, 0.75, 0.92 };
        var blocks = fractions
            .Select((f, i) => new StageBModelBlock(
                Timestamp: FormatOffset(duration * f),
                Spoken: $"Placeholder narration for segment {segment.Index}, part {i + 1}.",
                Speaker: "Presenter",
                OnScreenText: i == 0 ? $"Segment {segment.Index}" : null,
                Visual: $"Placeholder description of what is shown at part {i + 1}.",
                Kind: "other"))
            .ToArray();

        var output = new StageBModelOutput(
            SegmentStart: FormatOffset(segment.Start),
            Language: "en",
            Blocks: blocks,
            Summary: $"Placeholder summary of segment {segment.Index}.");

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new StageBModelResponse(StageBFinishReason.Stop, output, duration));
    }

    private static string FormatOffset(TimeSpan value)
    {
        var total = Math.Max(0, (int)value.TotalSeconds);
        return string.Create(CultureInfo.InvariantCulture, $"{total / 3600:D2}:{total % 3600 / 60:D2}:{total % 60:D2}");
    }
}
