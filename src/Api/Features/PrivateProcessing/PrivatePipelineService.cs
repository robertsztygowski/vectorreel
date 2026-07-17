using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using MdReel.Core;
using MdReel.Core.Pipeline.StageA;

namespace MdReel.Api.Features.PrivateProcessing;

public sealed partial class PrivatePipelineService(
    StageARunner stageA,
    IWebHostEnvironment environment,
    ILogger<PrivatePipelineService> logger)
{
    private static readonly TimeSpan StageBDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan StageCDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan StageDDelay = TimeSpan.FromMilliseconds(500);

    private readonly ConcurrentDictionary<string, UploadState> _uploads = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, JobState> _jobs = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<ErasureAuditEntry> _erasureAudit = new();
    private readonly string _uploadRoot = Path.Combine(environment.ContentRootPath, ".local-state", "uploads");

    public UploadCreatedResponse CreateUpload(HttpRequest request)
    {
        Directory.CreateDirectory(_uploadRoot);

        var uploadId = $"up_{Guid.NewGuid():N}";
        var signature = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var targetPath = Path.Combine(_uploadRoot, $"{uploadId}.bin");

        _uploads[uploadId] = new UploadState(uploadId, signature, targetPath);

        var uploadUrl = $"{request.Scheme}://{request.Host}/api/v1/uploads/{uploadId}/content?sig={signature}";
        return new UploadCreatedResponse(uploadId, uploadUrl);
    }

    public async Task<bool> StoreUploadAsync(
        string uploadId,
        string signature,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!_uploads.TryGetValue(uploadId, out var upload))
        {
            return false;
        }

        if (!string.Equals(upload.Signature, signature, StringComparison.Ordinal))
        {
            return false;
        }

        Directory.CreateDirectory(_uploadRoot);
        await using var write = File.Create(upload.Path);
        await request.Body.CopyToAsync(write, cancellationToken);
        await write.FlushAsync(cancellationToken);
        upload.Stored = true;
        return true;
    }

    public JobCreatedResponse? CreateJob(string uploadId, CreateJobOptions? options)
    {
        if (!_uploads.TryGetValue(uploadId, out var upload))
        {
            return null;
        }

        var jobId = $"job_{Guid.NewGuid():N}";
        var source = options?.Filename ?? $"{uploadId}.mp4";
        var now = DateTimeOffset.UtcNow;
        var state = new JobState
        {
            Id = jobId,
            UploadId = uploadId,
            Status = "queued",
            Stage = null,
            Progress = 0,
            Source = source,
            DurationSec = options?.DurationSec,
            CreatedAt = now,
            FinishedAt = null,
            CostCents = null,
            WallClockSec = null,
            ShouldFail = options?.Fail ?? false,
        };

        _jobs[jobId] = state;
        _ = ProcessJobAsync(state, upload);
        return new JobCreatedResponse(jobId);
    }

    public IReadOnlyList<JobListItemResponse> ListJobs() =>
        _jobs.Values
            .OrderByDescending(static x => x.CreatedAt)
            .Select(
                static x => new JobListItemResponse(
                    x.Id,
                    x.Status,
                    x.Status == "processing" ? x.Stage : null,
                    x.Progress,
                    x.Source,
                    x.DurationSec,
                    TimestampFormatter.ToUtcTimestamp(x.CreatedAt),
                    x.FinishedAt is null ? null : TimestampFormatter.ToUtcTimestamp(x.FinishedAt.Value)))
            .ToArray();

    public JobStatusResponse? GetJobStatus(string id)
    {
        if (!_jobs.TryGetValue(id, out var job))
        {
            return null;
        }

        return new JobStatusResponse(
            job.Status,
            job.Status == "processing" ? job.Stage : null,
            job.Progress,
            job.Status == "done" ? job.CostCents : null,
            job.Status == "done" ? job.DurationSec : null,
            job.Status == "done" ? job.WallClockSec : null);
    }

    public bool HasJob(string id) => _jobs.ContainsKey(id);

    public JobOutputResponse? GetOutput(string id)
    {
        if (!_jobs.TryGetValue(id, out var job))
        {
            return null;
        }

        return job.Output;
    }

    public bool DeleteJob(string id)
    {
        var removed = _jobs.TryRemove(id, out var job);
        if (!removed || job is null)
        {
            return false;
        }

        if (_uploads.TryGetValue(job.UploadId, out var upload))
        {
            TryDeleteFile(upload.Path);
        }

        _erasureAudit.Enqueue(new ErasureAuditEntry(id, DateTimeOffset.UtcNow));
        return true;
    }

    private async Task ProcessJobAsync(JobState job, UploadState upload)
    {
        var started = DateTimeOffset.UtcNow;

        // Root span per job (no parent: the HTTP request span ends at 202). jobId is the tag the
        // agent runbook searches on (TESTING.md) — one query from a red test to this trace.
        using var jobActivity = PipelineDiagnostics.Source.StartActivity(
            "pipeline.job", ActivityKind.Internal, parentContext: default);
        jobActivity?.SetTag("mdreel.job_id", job.Id);
        jobActivity?.SetTag("mdreel.upload_id", job.UploadId);
        jobActivity?.SetTag("mdreel.source", job.Source);

        try
        {
            SetProcessing(job, "A", 15);
            using (StartStage(job, "A"))
            {
                await ExecuteStageAAsync(job, upload);
            }

            if (job.ShouldFail)
            {
                SetFailed(job);
                jobActivity?.SetStatus(ActivityStatusCode.Error, "simulated failure requested");
                logger.LogWarning("Job {JobId} failed (simulated failure requested)", job.Id);
                return;
            }

            SetProcessing(job, "B", 45);
            using (StartStage(job, "B"))
            {
                await Task.Delay(StageBDelay);
            }

            SetProcessing(job, "C", 75);
            using (StartStage(job, "C"))
            {
                await Task.Delay(StageCDelay);
            }

            SetProcessing(job, "D", 92);
            using (StartStage(job, "D"))
            {
                await Task.Delay(StageDDelay);
            }

            var finishedAt = DateTimeOffset.UtcNow;
            job.Status = "done";
            job.Stage = null;
            job.Progress = 100;
            job.CostCents = 38;
            job.WallClockSec = Math.Max(1, (finishedAt - started).TotalSeconds);
            job.FinishedAt = finishedAt;
            job.DurationSec ??= 2832;
            job.Output = SampleOutputBuilder.Build(job.Source, job.DurationSec.Value, finishedAt);

            TryDeleteFile(upload.Path);
            jobActivity?.SetTag("mdreel.cost_cents", job.CostCents);
            LogJobDone(job.Id, job.WallClockSec.Value, job.CostCents.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Private pipeline failed for job {JobId}", job.Id);
            jobActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            SetFailed(job);
        }
    }

    private Activity? StartStage(JobState job, string stage)
    {
        LogStageEnter(job.Id, stage);
        var activity = PipelineDiagnostics.Source.StartActivity($"pipeline.stage.{stage}");
        activity?.SetTag("mdreel.job_id", job.Id);
        activity?.SetTag("mdreel.stage", stage);
        return activity;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobId} entering stage {Stage}")]
    private partial void LogStageEnter(string jobId, string stage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobId} done in {WallClockSec}s (cost {CostCents}c)")]
    private partial void LogJobDone(string jobId, double wallClockSec, int costCents);

    private async Task ExecuteStageAAsync(JobState job, UploadState upload)
    {
        if (!upload.Stored || !File.Exists(upload.Path))
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return;
        }

        var prepared = await stageA.PrepareAsync(job.Id, upload.Path, StageAOptions.Default, CancellationToken.None);
        job.DurationSec ??= prepared.Probe.Duration.TotalSeconds;
    }

    private static void SetProcessing(JobState job, string stage, double progress)
    {
        job.Status = "processing";
        job.Stage = stage;
        job.Progress = progress;
    }

    private static void SetFailed(JobState job)
    {
        job.Status = "failed";
        job.Stage = null;
        job.Progress = 100;
        job.FinishedAt = DateTimeOffset.UtcNow;
    }

    private static void TryDeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}

public sealed record UploadCreatedResponse(
    [property: JsonPropertyName("uploadId")] string UploadId,
    [property: JsonPropertyName("uploadUrl")] string UploadUrl);

public sealed record JobCreatedResponse(
    [property: JsonPropertyName("jobId")] string JobId);

public sealed record JobListItemResponse(
    [property: JsonPropertyName("jobId")] string JobId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("stage"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Stage,
    [property: JsonPropertyName("progress")] double Progress,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("duration_sec"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? DurationSec,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("finished_at"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? FinishedAt);

public sealed record JobStatusResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("stage"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Stage,
    [property: JsonPropertyName("progress")] double Progress,
    [property: JsonPropertyName("cost_cents"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? CostCents,
    [property: JsonPropertyName("duration_sec"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? DurationSec,
    [property: JsonPropertyName("wall_clock_sec"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? WallClockSec);

public sealed record CreateJobRequest(
    [property: JsonPropertyName("uploadId")] string? UploadId,
    [property: JsonPropertyName("options")] CreateJobOptions? Options);

public sealed record CreateJobOptions(
    [property: JsonPropertyName("language_hint")] string? LanguageHint,
    [property: JsonPropertyName("retention_days")] int? RetentionDays,
    [property: JsonPropertyName("webhook_url")] string? WebhookUrl,
    [property: JsonPropertyName("quality")] string? Quality,
    [property: JsonPropertyName("fail")] bool? Fail,
    [property: JsonPropertyName("filename")] string? Filename,
    [property: JsonPropertyName("durationSec")] double? DurationSec);

public sealed record JobOutputResponse(
    string Filename,
    string Markdown,
    OutputDocumentResponse Document);

public sealed record OutputDocumentResponse(
    [property: JsonPropertyName("frontmatter")] OutputFrontmatterResponse Frontmatter,
    [property: JsonPropertyName("sections")] IReadOnlyList<OutputSectionResponse> Sections,
    [property: JsonPropertyName("provenance")] string Provenance);

public sealed record OutputFrontmatterResponse(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("duration")] string Duration,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("processed_at")] string ProcessedAt,
    [property: JsonPropertyName("generator")] string Generator,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("tags")] IReadOnlyList<string> Tags);

public sealed record OutputSectionResponse(
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("heading")] string Heading,
    [property: JsonPropertyName("blocks")] IReadOnlyList<OutputBlockResponse> Blocks);

public sealed record OutputBlockResponse(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("text")] string Text);

internal sealed record UploadState(string Id, string Signature, string Path)
{
    public bool Stored { get; set; }
}

internal sealed record ErasureAuditEntry(string JobId, DateTimeOffset ErasedAt);

internal sealed class JobState
{
    public required string Id { get; init; }

    public required string UploadId { get; init; }

    public required string Source { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public string Status { get; set; } = "queued";

    public string? Stage { get; set; }

    public double Progress { get; set; }

    public double? DurationSec { get; set; }

    public double? WallClockSec { get; set; }

    public int? CostCents { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public bool ShouldFail { get; set; }

    public JobOutputResponse? Output { get; set; }
}

internal static class SampleOutputBuilder
{
    public static JobOutputResponse Build(string sourceFilename, double durationSec, DateTimeOffset processedAt)
    {
        var processedAtUtc = TimestampFormatter.ToUtcTimestamp(processedAt);
        var frontmatter = new OutputFrontmatterResponse(
            "Q3 Platform Demo — Billing Module",
            sourceFilename,
            FormatDuration(durationSec),
            "en",
            processedAtUtc,
            "mdreel@0.0.0-fixture",
            "A walkthrough of the billing module's invoice workflow, covering proration and the admin panel.",
            ["demo", "billing", "azure"]);

        var sections = new[]
        {
            new OutputSectionResponse(
                "00:00:00",
                "Introduction",
                [
                    new OutputBlockResponse(
                        "spoken",
                        "Thanks for joining — today we'll walk through the billing module, focusing on the new invoice workflow and how proration is applied."),
                    new OutputBlockResponse("on_screen", "mdreel — Q3 Platform Demo"),
                    new OutputBlockResponse(
                        "visual",
                        "Title card, then a cut to the presenter's screen share of the admin dashboard."),
                ]),
            new OutputSectionResponse(
                "00:03:40",
                "Invoice workflow walkthrough",
                [
                    new OutputBlockResponse(
                        "spoken",
                        "We open the invoice editor and apply proration before sending. The service call underneath is a straightforward async create."),
                    new OutputBlockResponse(
                        "on_screen",
                        "Invoices → Create → \"Apply proration\" checkbox\ncode: InvoiceService.CreateAsync(...)"),
                    new OutputBlockResponse(
                        "visual",
                        "Presenter navigates the admin panel, opens the invoice editor, and toggles the proration checkbox before saving."),
                ]),
            new OutputSectionResponse(
                "00:09:12",
                "Wrap-up",
                [
                    new OutputBlockResponse(
                        "spoken",
                        "That's the core flow — happy to take questions on edge cases like mid-cycle plan changes."),
                    new OutputBlockResponse("on_screen", "Questions? billing-team@example.com"),
                    new OutputBlockResponse("visual", "Presenter returns to the title card."),
                ]),
        };

        var document = new OutputDocumentResponse(
            frontmatter,
            sections,
            "Generated by **mdreel** from your uploaded recording. The source video was deleted after processing per the default retention policy.");

        var markdown = RenderMarkdown(document);
        var outputFilename = $"{Path.GetFileNameWithoutExtension(sourceFilename)}.md";
        return new JobOutputResponse(outputFilename, markdown, document);
    }

    private static string RenderMarkdown(OutputDocumentResponse document)
    {
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"title: {Quote(document.Frontmatter.Title)}");
        sb.AppendLine($"source: {Quote(document.Frontmatter.Source)}");
        sb.AppendLine($"duration: {Quote(document.Frontmatter.Duration)}");
        sb.AppendLine($"language: {Quote(document.Frontmatter.Language)}");
        sb.AppendLine($"processed_at: {Quote(document.Frontmatter.ProcessedAt)}");
        sb.AppendLine($"generator: {Quote(document.Frontmatter.Generator)}");
        sb.AppendLine($"summary: {Quote(document.Frontmatter.Summary)}");
        sb.AppendLine($"tags: [{string.Join(", ", document.Frontmatter.Tags)}]");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"# {document.Frontmatter.Title}");

        foreach (var section in document.Sections)
        {
            sb.AppendLine();
            sb.AppendLine($"## [{section.Timestamp}] {section.Heading}");

            foreach (var block in section.Blocks)
            {
                sb.AppendLine();
                if (block.Label == "on_screen")
                {
                    sb.AppendLine("**On screen:**");
                    foreach (var line in block.Text.Split('\n'))
                    {
                        sb.AppendLine($"> {line}");
                    }
                }
                else
                {
                    var marker = block.Label == "spoken" ? "**Spoken:**" : "**Visual:**";
                    var lines = block.Text.Split('\n');
                    sb.AppendLine($"{marker} {lines[0]}");
                    foreach (var line in lines.Skip(1))
                    {
                        sb.AppendLine(line);
                    }
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## Source & licence");
        sb.AppendLine();
        sb.AppendLine(document.Provenance);
        sb.AppendLine();
        return sb.ToString();
    }

    private static string Quote(string value) => $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string FormatDuration(double totalSeconds)
    {
        var total = Math.Max(0, (int)Math.Round(totalSeconds, MidpointRounding.AwayFromZero));
        var h = total / 3600;
        var m = (total % 3600) / 60;
        var s = total % 60;
        return $"{h:00}:{m:00}:{s:00}";
    }
}

internal static class TimestampFormatter
{
    public static string ToUtcTimestamp(DateTimeOffset value) =>
        value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
}

internal static class ProblemResponseExtensions
{
    public static async Task WriteProblemAsync(
        this HttpContext context,
        int status,
        string title,
        string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(
            new
            {
                type = "about:blank",
                title,
                status,
                detail,
                instance = context.Request.Path.Value,
            });
    }
}
