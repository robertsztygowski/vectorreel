using System.Diagnostics;
using MdReel.Core.Domain;
using MdReel.Core.Providers;
using Microsoft.Extensions.Logging;

namespace MdReel.Core.Pipeline.StageA;

/// <summary>What Stage A hands to Stage B.</summary>
/// <param name="Probe">What the file is.</param>
/// <param name="Timeline">What is in it — static runs (cost) and block boundaries (citations).</param>
/// <param name="Segments">The units of work, each with its sampling plan and its boundaries.</param>
public sealed record PreparedVideo(
    VideoProbe Probe,
    ContentTimeline Timeline,
    IReadOnlyList<Segment> Segments);

/// <summary>
/// Stage A end to end: probe the file, scan it once, work out where the cost can be cut and where the
/// blocks must begin, and plan the segments (ARCHITECTURE.md §3).
///
/// It is pure local compute — ffmpeg and arithmetic. No cloud, no credentials, nothing billable.
/// </summary>
public sealed class StageARunner(
    IMediaProbe probe,
    IMediaScanner scanner,
    ICostLedger ledger,
    ILogger<StageARunner> logger)
{
    /// <summary>Prepare a local source file for analysis.</summary>
    public async Task<PreparedVideo> PrepareAsync(
        string jobId,
        string path,
        StageAOptions options,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("job {JobId}", jobId);

        var probed = await Meter(
            jobId, "stage_a.probe", () => probe.ProbeAsync(path, cancellationToken));

        var scan = await Meter(
            jobId, "stage_a.scan", () => scanner.ScanAsync(path, probed.HasAudioStream, cancellationToken));

        var staticRuns = StaticRunDetector.Detect(scan.Frames, options);
        var cues = CueDetector.Detect(scan.Frames, scan.Silences, options);

        var timeline = new ContentTimeline(
            probed.Duration,
            staticRuns,
            cues,
            StaticRunDetector.StaticFraction(staticRuns, scan.SampledSeconds));

        var segments = Segmenter.Plan(timeline, options);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Stage A: {Duration:F0}s, {StaticPercent:P0} static, {Cues} block boundaries "
                + "({PerTenMinutes:F1}/10min, worst block {Worst:F0}s), {Segments} segments, {Cheap} at low resolution",
                probed.Duration.TotalSeconds,
                timeline.StaticFractionInRuns,
                cues.Count,
                timeline.CuesPerTenMinutes,
                timeline.LongestUncuedSpan.TotalSeconds,
                segments.Count,
                segments.Count(s => s.Sampling.MediaResolution == MediaResolution.Low));
        }

        return new PreparedVideo(probed, timeline, segments);
    }

    /// <summary>
    /// Time a compute step and put it in the ledger.
    ///
    /// CLAUDE.md rule 6 covers every compute step, not just LLM calls — and this is the step it was
    /// written for. ffmpeg is roughly a third of true COGS and has been an <em>estimate</em> in every
    /// phase so far (METRICS.md N5), which makes every cost figure we hold optimistic by about that much.
    /// Counting it where it happens is the only way that gets fixed; retrofitting the measurement later
    /// means threading a stopwatch back through code already declared done.
    ///
    /// The entry carries seconds and no price. Turning seconds into euros needs a Cloud Run rate that is
    /// not knowable from a laptop, and a fabricated rate in the ledger is worse than an admitted gap —
    /// the ledger's whole value is that the numbers in it are real. Phase 2 prices these.
    /// </summary>
    private async Task<T> Meter<T>(string jobId, string step, Func<Task<T>> work)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            return await work();
        }
        finally
        {
            stopwatch.Stop();

            ledger.Record(new CostEntry(
                JobId: jobId,
                Kind: CostKind.Compute,
                Step: step,
                Quantity: stopwatch.Elapsed.TotalSeconds,
                Unit: "seconds",
                Cents: null));
        }
    }
}
