using System.Text.Json;
using MdReel.Core.Domain;
using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;
using Microsoft.Extensions.Options;

namespace MdReel.Worker;

/// <summary>
/// One-shot host for a collection-production batch: load the licence-verified source list, produce
/// what has not been produced yet, write the reconciliation report, stop.
/// </summary>
public sealed class CollectionProductionWorker(
    CollectionBatchRunner batch,
    IOptions<CollectionProductionOptions> options,
    IHostApplicationLifetime lifetime,
    ILogger<CollectionProductionWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions _reportJson = new() { WriteIndented = true };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = options.Value;
        if (!cfg.Enabled)
        {
            return;
        }

        try
        {
            var sources = CollectionCorpus.Load(cfg.CorpusPath, cfg.OnlyVideoIds);
            var totalHours = sources.Sum(s => s.Duration.TotalHours);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Collection '{Collection}': {Count} CC-BY sources, {Hours:F2} video-hours. Calibration limit: {Calibration}.",
                    cfg.Collection,
                    sources.Count,
                    totalHours,
                    cfg.CalibrationCount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none (full batch)");
            }

            var result = await batch.RunAsync(
                new CollectionBatchRequest(
                    Collection: cfg.Collection,
                    Sources: sources,
                    OutputBucket: cfg.OutputBucket,
                    OutputPrefix: $"{cfg.OutputPrefix.Trim('/')}/{cfg.Collection}",
                    StageBOptions: StageBOptions.Default with
                    {
                        CallOptions = new StageBCallOptions(
                            cfg.StageB.MaxOutputTokens,
                            cfg.StageB.ThinkingBudget,
                            TimeSpan.FromSeconds(cfg.StageB.TimeoutSeconds)),
                    },
                    SegmentLength: cfg.SegmentLength,
                    SegmentOverlap: cfg.SegmentOverlap,
                    MediaResolution: cfg.LowMediaResolution ? MediaResolution.Low : MediaResolution.Default,
                    CalibrationCount: cfg.CalibrationCount,
                    AbortOverCentsPerVideoHour: cfg.AbortOverCentsPerVideoHour,
                    RetryAttempts: cfg.RetryAttempts,
                    RetryBackoff: cfg.RetryBackoff,
                    PaceBetweenSessions: cfg.PaceBetweenSessions),
                stoppingToken);

            var remainingHours = totalHours - result.Produced.Sum(s => s.VideoDuration.TotalHours)
                - result.SkippedSessions.Sum(s => s.VideoDuration.TotalHours);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Batch done: {Produced} produced, {Skipped} already present, {Failed} failed, "
                    + "{Cents} cents total, {Rate} cents/video-hour measured. Projected cost for the "
                    + "remaining {Remaining:F2} video-hours at that rate: {Projection} cents. "
                    + "Reconcile against METRICS.md N4c. Retry failures with: --only {RetryList}",
                    result.Produced.Count(),
                    result.SkippedSessions.Count(),
                    result.FailedSessions.Count(),
                    result.TotalCents,
                    result.CentsPerVideoHour,
                    remainingHours,
                    result.ProjectedCentsFor(remainingHours),
                    string.Join(',', result.FailedSessions.Select(s => s.VideoId)));
            }

            await WriteReportAsync(cfg, result, totalHours, remainingHours, stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Collection production failed for '{Collection}'.", cfg.Collection);
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }

    private async Task WriteReportAsync(
        CollectionProductionOptions cfg,
        CollectionBatchResult result,
        double totalHours,
        double remainingHours,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cfg.ReportPath))
        {
            return;
        }

        var directory = Path.GetDirectoryName(cfg.ReportPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var report = new
        {
            collection = result.Collection,
            generated_at = DateTimeOffset.UtcNow,
            media_resolution = cfg.LowMediaResolution ? "low" : "default",
            segment_length_seconds = cfg.SegmentLength.TotalSeconds,
            corpus_video_hours = Math.Round(totalHours, 3),
            produced = result.Produced.Count(),
            skipped_already_present = result.SkippedSessions.Count(),
            failed = result.FailedSessions.Count(),
            retry_command = result.FailedSessions.Any()
                ? $"scripts/produce-collection.sh {result.Collection} --only {string.Join(',', result.FailedSessions.Select(s => s.VideoId))}"
                : null,
            total_cents = result.TotalCents,
            produced_video_hours = Math.Round(result.VideoHours, 3),
            cents_per_video_hour = result.CentsPerVideoHour,
            remaining_video_hours = Math.Round(remainingHours, 3),
            projected_remaining_cents = result.ProjectedCentsFor(remainingHours),
            sessions = result.Sessions,
        };

        await File.WriteAllTextAsync(
            cfg.ReportPath,
            JsonSerializer.Serialize(report, _reportJson),
            cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Batch report written to {Path}", cfg.ReportPath);
        }
    }
}
