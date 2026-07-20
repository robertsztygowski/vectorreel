using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MdReel.Core;

/// <summary>
/// The one ActivitySource and Meter for pipeline work. Stage runners and job processors
/// emit through here so traces and metrics stay named consistently across API and worker.
/// </summary>
public static class PipelineDiagnostics
{
    public const string SourceName = "MdReel.Pipeline";

    public static readonly ActivitySource Source = new(SourceName);

    public static readonly Meter Meter = new(SourceName);

    private static readonly Histogram<double> _jobDuration = Meter.CreateHistogram<double>(
        "mdreel.job.duration",
        unit: "s",
        description: "Pipeline job wall-clock duration.");

    private static readonly Counter<double> _jobVideoMinutes = Meter.CreateCounter<double>(
        "mdreel.job.video_minutes",
        unit: "min",
        description: "Video minutes processed by completed jobs.");

    private static readonly Histogram<double> _stageDuration = Meter.CreateHistogram<double>(
        "mdreel.stage.duration",
        unit: "s",
        description: "Pipeline stage wall-clock duration.");

    private static readonly Counter<long> _stageBRunaway = Meter.CreateCounter<long>(
        "mdreel.stageb.runaway",
        unit: "{event}",
        description: "Stage B MAX_TOKENS/degenerate-output guard activations (METRICS.md N7).");

    private static readonly Counter<long> _llmTokens = Meter.CreateCounter<long>(
        "mdreel.llm.tokens",
        unit: "{token}",
        description: "LLM tokens reported by the provider bill metadata.");

    private static readonly Counter<long> _webhookDeliveryFailures = Meter.CreateCounter<long>(
        "mdreel.webhook.delivery_failures",
        unit: "{failure}",
        description: "Webhook delivery failures.");

    public static void RecordJobDuration(TimeSpan duration, string outcome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);
        _jobDuration.Record(Math.Max(0, duration.TotalSeconds), new KeyValuePair<string, object?>("outcome", outcome));
    }

    public static void AddJobVideoMinutes(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return;
        }

        _jobVideoMinutes.Add(duration.TotalMinutes);
    }

    public static void RecordStageDuration(string stage, TimeSpan duration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stage);
        _stageDuration.Record(Math.Max(0, duration.TotalSeconds), new KeyValuePair<string, object?>("stage", stage));
    }

    public static void AddStageBRunaway() => _stageBRunaway.Add(1);

    public static void AddLlmTokens(string kind, string region, int tokens)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(region);
        if (tokens <= 0)
        {
            return;
        }

        _llmTokens.Add(
            tokens,
            new KeyValuePair<string, object?>("kind", kind),
            new KeyValuePair<string, object?>("region", region));
    }

    public static void AddWebhookDeliveryFailure() => _webhookDeliveryFailures.Add(1);
}
