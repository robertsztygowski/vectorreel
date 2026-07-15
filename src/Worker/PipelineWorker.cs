namespace MdReel.Worker;

/// <summary>
/// Queue-driven pipeline worker. A shell until Phase 2: the stage handlers stay thin
/// (dequeue → call Core → persist) and the pipeline itself lives in Core (DEVELOPMENT.md §2).
/// </summary>
public sealed class PipelineWorker(ILogger<PipelineWorker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker started; no queue is wired yet (Phase 2).");
        return Task.CompletedTask;
    }
}
