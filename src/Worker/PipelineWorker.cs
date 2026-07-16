using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;
using Microsoft.Extensions.Options;

namespace MdReel.Worker;

/// <summary>Internal runner for gallery production jobs (YouTube fileUri path).</summary>
public sealed class PipelineWorker(
    YouTubeInternalGalleryRunner runner,
    IOptions<YouTubeGalleryRunnerOptions> options,
    ILogger<PipelineWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = options.Value;
        if (!cfg.Enabled)
        {
            logger.LogInformation("Worker started; YouTubeGalleryRunner is disabled.");
            return;
        }

        if (cfg.Videos.Count == 0)
        {
            logger.LogWarning("YouTubeGalleryRunner is enabled but no videos are configured.");
            return;
        }

        var stageBOptions = StageBOptions.Default with
        {
            CallOptions = new StageBCallOptions(
                cfg.StageB.MaxOutputTokens,
                cfg.StageB.ThinkingBudget,
                TimeSpan.FromSeconds(cfg.StageB.TimeoutSeconds)),
        };

        foreach (var video in cfg.Videos)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var request = new YouTubeInternalGalleryRunRequest(
                JobId: $"gallery_{video.VideoId}_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
                VideoId: video.VideoId,
                SourceUri: video.SourceUri,
                Duration: TimeSpan.FromSeconds(video.DurationSeconds),
                OutputBucket: cfg.OutputBucket,
                OutputPrefix: cfg.OutputPrefix,
                SegmentLength: cfg.SegmentLength,
                SegmentOverlap: cfg.SegmentOverlap);

            await runner.RunAsync(request, stageBOptions, stoppingToken);
        }
    }
}
