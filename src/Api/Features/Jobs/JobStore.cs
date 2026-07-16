using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json.Serialization;

namespace MdReel.Api.Features.Jobs;

public sealed class JobStore
{
    private readonly ConcurrentDictionary<string, JobRecord> _jobs = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _outputs = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<ErasureAuditEntry> _erasureAudit = new();

    public JobStore()
    {
        var now = DateTimeOffset.UtcNow;

        var seeded = new[]
        {
            new JobRecord(
                "job_recent_processing",
                "processing",
                "B",
                42,
                "incident-retro.mp4",
                null,
                now.AddMinutes(-3),
                null),
            new JobRecord(
                "job_previous_done",
                "done",
                null,
                100,
                "api-walkthrough.mp4",
                372,
                now.AddMinutes(-22),
                now.AddMinutes(-12)),
        };

        foreach (var job in seeded)
        {
            _jobs[job.Id] = job;
            _outputs[job.Id] = $"output:{job.Id}";
        }
    }

    public IReadOnlyList<JobRecord> List() =>
        _jobs.Values.OrderByDescending(static x => x.CreatedAt).ToArray();

    public bool Delete(string id)
    {
        if (!_jobs.TryRemove(id, out _))
        {
            return false;
        }

        _outputs.TryRemove(id, out _);
        _erasureAudit.Enqueue(new ErasureAuditEntry(id, DateTimeOffset.UtcNow));
        return true;
    }
}

public sealed record JobListResponse(
    [property: JsonPropertyName("jobs")] IReadOnlyList<JobListItemResponse> Jobs);

public sealed record JobListItemResponse(
    [property: JsonPropertyName("jobId")] string JobId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("stage")] string? Stage,
    [property: JsonPropertyName("progress")] double Progress,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("duration_sec")] double? DurationSec,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("finished_at")] string? FinishedAt);

public sealed record JobRecord(
    string Id,
    string Status,
    string? Stage,
    double Progress,
    string Source,
    double? DurationSec,
    DateTimeOffset CreatedAt,
    DateTimeOffset? FinishedAt);

public sealed record ErasureAuditEntry(string JobId, DateTimeOffset ErasedAt);

public static class TimestampFormatter
{
    public static string ToUtcTimestamp(DateTimeOffset value) =>
        value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
}
