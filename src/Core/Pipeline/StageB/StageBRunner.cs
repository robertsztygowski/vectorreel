using MdReel.Core.Domain;
using MdReel.Core.Pipeline.StageA;
using MdReel.Core.Providers;

namespace MdReel.Core.Pipeline.StageB;

/// <summary>Stage B orchestration with mandatory guards and deterministic overflow splitting.</summary>
public sealed class StageBRunner(IVideoAnalyzer analyzer)
{
    public async Task<IReadOnlyList<SegmentAnalysis>> AnalyzeAsync(
        string sourceUri,
        Segment segment,
        StageAOptions stageAOptions,
        StageBOptions options,
        CancellationToken cancellationToken)
    {
        var planned = PreSize(segment, stageAOptions, options);
        List<SegmentAnalysis> analyses = [];
        foreach (var plannedSegment in planned)
        {
            var analyzed = await AnalyzeSegmentAsync(
                sourceUri,
                plannedSegment,
                stageAOptions,
                options,
                attempt: 0,
                cancellationToken);

            analyses.AddRange(analyzed);
        }

        return analyses;
    }

    private async Task<IReadOnlyList<SegmentAnalysis>> AnalyzeSegmentAsync(
        string sourceUri,
        Segment segment,
        StageAOptions stageAOptions,
        StageBOptions options,
        int attempt,
        CancellationToken cancellationToken)
    {
        var callOptions = attempt == 0 ? options.CallOptions : options.RetryCallOptions;
        var response = await CallModelAsync(sourceUri, segment, callOptions, cancellationToken);

        if (response.FinishReason == StageBFinishReason.MaxTokens)
        {
            var split = SegmentSplitPolicy.Halve(segment, stageAOptions);
            var first = await AnalyzeSegmentAsync(sourceUri, split.First, stageAOptions, options, attempt: 0, cancellationToken);
            var second = await AnalyzeSegmentAsync(sourceUri, split.Second, stageAOptions, options, attempt: 0, cancellationToken);
            return [.. first, .. second];
        }

        var validation = Validate(response, segment, options);
        if (validation.IsValid)
        {
            return [validation.Analysis!];
        }

        if (attempt < options.MaxRetries)
        {
            return await AnalyzeSegmentAsync(sourceUri, segment, stageAOptions, options, attempt + 1, cancellationToken);
        }

        throw new InvalidOperationException(
            $"Stage B produced invalid output for segment {segment.Index}: {validation.FailureReason}");
    }

    private async Task<StageBModelResponse> CallModelAsync(
        string sourceUri,
        Segment segment,
        StageBCallOptions callOptions,
        CancellationToken cancellationToken)
    {
        if (callOptions.MaxOutputTokens <= 0 || callOptions.ThinkingBudget < 0 || callOptions.Timeout <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Stage B guard options are invalid or unbounded.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(callOptions.Timeout);
        return await analyzer.AnalyzeAsync(sourceUri, segment, callOptions, timeout.Token);
    }

    private static List<Segment> PreSize(
        Segment segment,
        StageAOptions stageAOptions,
        StageBOptions options)
    {
        var seeded = options.ForceCueBoundarySegmentation
            ? StageBCueFallbackSplitter.Split(segment, stageAOptions.MinSegmentLength)
            : new[] { segment };

        List<Segment> output = [];
        foreach (var seed in seeded)
        {
            SplitDense(seed, stageAOptions, options, output);
        }

        return output;
    }

    private static void SplitDense(
        Segment segment,
        StageAOptions stageAOptions,
        StageBOptions options,
        List<Segment> output)
    {
        if (!ShouldSplitDense(segment, stageAOptions, options))
        {
            output.Add(segment);
            return;
        }

        var halves = SegmentSplitPolicy.Halve(segment, stageAOptions);
        SplitDense(halves.First, stageAOptions, options, output);
        SplitDense(halves.Second, stageAOptions, options, output);
    }

    private static bool ShouldSplitDense(
        Segment segment,
        StageAOptions stageAOptions,
        StageBOptions options)
    {
        if (segment.Duration <= options.DenseSegmentTarget)
        {
            return false;
        }

        if (segment.Duration < stageAOptions.MinSegmentLength * 2)
        {
            return false;
        }

        var cuesPerTenMinutes = segment.Cues.Count / (segment.Duration.TotalSeconds / 600.0);
        return cuesPerTenMinutes >= options.DenseCueThresholdPerTenMinutes;
    }

    private static StageBValidationResult Validate(
        StageBModelResponse response,
        Segment segment,
        StageBOptions options)
    {
        if (response.Output is null)
        {
            return StageBValidationResult.Fail("model output was null");
        }

        if (response.Output.Blocks.Count == 0)
        {
            return StageBValidationResult.Fail("model output had no blocks");
        }

        var normalizedOffsets = StageBTimestampNormalizer.Normalize(
            [.. response.Output.Blocks.Select(block => block.Timestamp)],
            response.FetchedDuration ?? segment.Duration);

        var coverage = StageBCoverageGuard.Evaluate(
            normalizedOffsets,
            response.FetchedDuration ?? segment.Duration,
            options);

        if (!coverage.IsValid)
        {
            return StageBValidationResult.Fail(coverage.Reason ?? "coverage guard failed");
        }

        var blocks = response.Output.Blocks
            .Zip(normalizedOffsets, (block, offset) => new AnalyzedBlock(
                At: offset,
                Spoken: block.Spoken,
                Speaker: block.Speaker,
                OnScreenText: block.OnScreenText,
                Visual: block.Visual,
                Kind: block.Kind))
            .ToArray();

        return StageBValidationResult.Pass(new SegmentAnalysis(
            SegmentIndex: segment.Index,
            SegmentStart: segment.Start,
            Language: response.Output.Language,
            Blocks: blocks,
            Summary: response.Output.Summary));
    }

    private sealed record StageBValidationResult(
        bool IsValid,
        SegmentAnalysis? Analysis,
        string? FailureReason)
    {
        public static StageBValidationResult Pass(SegmentAnalysis analysis) => new(true, analysis, null);

        public static StageBValidationResult Fail(string reason) => new(false, null, reason);
    }
}
