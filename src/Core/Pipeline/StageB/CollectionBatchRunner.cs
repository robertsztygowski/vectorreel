using System.Diagnostics;
using MdReel.Core.Domain;
using MdReel.Core.Providers;
using Microsoft.Extensions.Logging;

namespace MdReel.Core.Pipeline.StageB;

/// <summary>
/// Produces a whole collection through the real pipeline: one licence-verified source list in, one
/// set of §4 documents out. Internal-only, same as <see cref="YouTubeInternalGalleryRunner"/> —
/// there is no public endpoint on this path (METRICS.md N10).
///
/// Three properties matter, and none is incidental:
/// <list type="bullet">
/// <item><b>Resumable.</b> A source whose <c>output.md</c> already exists is skipped, never
/// re-produced. A batch that dies half-way must not re-pay for what it already bought.</item>
/// <item><b>Calibrated before committed.</b> <see cref="CollectionBatchRequest.CalibrationCount"/>
/// produces the first N sources and stops, so measured €/video-hour can be reconciled against
/// METRICS.md <b>N4c</b> and the batch extrapolated before the rest of the budget is spent.</item>
/// <item><b>Bounded.</b> A per-session cost ceiling aborts the batch rather than paying for the
/// METRICS.md <b>N4d</b> over-segmentation failure mode across twenty-five videos.</item>
/// </list>
/// </summary>
public sealed class CollectionBatchRunner(
    YouTubeInternalGalleryRunner runner,
    IObjectStorage storage,
    ICostLedger ledger,
    ILogger<CollectionBatchRunner> logger)
{
    public async Task<CollectionBatchResult> RunAsync(
        CollectionBatchRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<CollectionSessionResult> results = [];
        var produced = 0;

        foreach (var source in request.Sources)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request.CalibrationCount is int limit && produced >= limit)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Calibration limit of {Limit} reached before {VideoId}. Reconcile measured cost against METRICS.md N4c before producing the rest.",
                        limit,
                        source.VideoId);
                }

                break;
            }

            var prefix = BuildPrefix(request.OutputPrefix, source.VideoId);
            if (await storage.ExistsAsync(request.OutputBucket, $"{prefix}/output.md", cancellationToken))
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Skipping {VideoId} — already produced.", source.VideoId);
                }

                results.Add(CollectionSessionResult.Skipped(source));
                continue;
            }

            if (produced > 0 && request.PaceBetweenSessions is { } pace && pace > TimeSpan.Zero)
            {
                // Slower is cheaper than retrying: a 429'd call buys nothing and still burns
                // wall-clock, so it is better not to provoke one.
                await Task.Delay(pace, cancellationToken);
            }

            var jobId = $"collection_{request.Collection}_{source.VideoId}";
            var before = LedgerMicrocentsFor(jobId);
            var result = await ProduceWithRetriesAsync(request, source, jobId, before, cancellationToken);
            results.Add(result);

            if (result.Outcome == CollectionSessionOutcome.Failed)
            {
                // Not a drop. The source stays in the corpus and is reported for a targeted retry;
                // only a *quality* failure removes a session from a collection, and that is a
                // human decision made while looking at the output.
                logger.LogWarning(
                    "Giving up on {VideoId} for now: {Reason}. It stays in the corpus — retry it with --only.",
                    source.VideoId,
                    result.FailureReason);
                continue;
            }

            produced++;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Produced {VideoId}: {Segments} Stage-B segments, {Cents} cents, {CentsPerHour} cents/video-hour, {Wall} wall-clock.",
                    source.VideoId,
                    result.StageBSegments,
                    result.Cents,
                    result.CentsPerVideoHour,
                    result.WallClock);
            }

            // Not a price ceiling — the signature of naive over-segmentation (METRICS.md N4d).
            // Paying for that across a whole batch is the specific failure this run must not repeat.
            if (request.AbortOverCentsPerVideoHour is int ceiling && result.CentsPerVideoHour > ceiling)
            {
                throw new CollectionBatchAbortedException(
                    $"{source.VideoId} cost {result.CentsPerVideoHour} cents per video-hour, over the "
                    + $"configured ceiling of {ceiling}. This is the METRICS.md N4d failure mode, not a "
                    + "price: shorten segments up front for this source, or drop it. Do not pay for "
                    + "overflow retries in bulk.");
            }
        }

        return new CollectionBatchResult(request.Collection, results);
    }

    private async Task<CollectionSessionResult> ProduceWithRetriesAsync(
        CollectionBatchRequest request,
        CollectionSource source,
        string jobId,
        long ledgerBefore,
        CancellationToken cancellationToken)
    {
        var backoff = request.RetryBackoff ?? TimeSpan.FromSeconds(60);
        string lastFailure = "unknown";

        for (var attempt = 1; attempt <= Math.Max(1, request.RetryAttempts); attempt++)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var run = await runner.RunAsync(
                    new YouTubeInternalGalleryRunRequest(
                        JobId: jobId,
                        VideoId: source.VideoId,
                        SourceUri: source.SourceUri,
                        Duration: source.Duration,
                        OutputBucket: request.OutputBucket,
                        OutputPrefix: request.OutputPrefix,
                        // Dense slide content is segmented shorter UP FRONT rather than halved and
                        // re-run after an overflow — the naive reactive fix is the expensive one.
                        SegmentLength: source.SegmentLength ?? request.SegmentLength,
                        SegmentOverlap: request.SegmentOverlap,
                        Attribution: source.Attribution,
                        MediaResolution: request.MediaResolution,
                        // A 429 storm can kill a video half-way through; without this, the retry
                        // re-buys every segment it already paid for.
                        CheckpointSegments: true),
                    request.StageBOptions,
                    cancellationToken);

                stopwatch.Stop();
                return CollectionSessionResult.Produced(
                    source,
                    run,
                    LedgerMicrocentsFor(jobId) - ledgerBefore,
                    stopwatch.Elapsed);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                stopwatch.Stop();
                lastFailure = ex.Message;

                if (attempt >= Math.Max(1, request.RetryAttempts))
                {
                    break;
                }

                // Backs off linearly on attempt number. Quota windows recover on a timescale of
                // minutes, so the useful move is to wait, not to hammer a second region.
                var wait = backoff * attempt;
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "{VideoId} attempt {Attempt}/{Total} failed ({Reason}); waiting {Wait} before retrying.",
                        source.VideoId,
                        attempt,
                        request.RetryAttempts,
                        ex.Message,
                        wait);
                }

                await Task.Delay(wait, cancellationToken);
            }
        }

        return CollectionSessionResult.Failed(source, lastFailure, LedgerMicrocentsFor(jobId) - ledgerBefore);
    }

    private static string BuildPrefix(string outputPrefix, string videoId)
    {
        var trimmed = outputPrefix.Trim().Trim('/');
        return string.IsNullOrEmpty(trimmed) ? videoId : $"{trimmed}/{videoId}";
    }

    // Collection production runs on the in-memory ledger (the worker has no DB). Rule 6 metering
    // itself happens inside Stage B / the fuser; this only reads it back for reconciliation.
    // Summed in microcents: a video is many small calls, and summing rounded cents loses the bill.
    private long LedgerMicrocentsFor(string jobId) =>
        ledger is InMemoryCostLedger inMemory
            ? inMemory.Entries
                .Where(e => string.Equals(e.JobId, jobId, StringComparison.Ordinal))
                .Sum(e => e.Microcents ?? (e.Cents ?? 0) * 10_000L)
            : 0;
}
