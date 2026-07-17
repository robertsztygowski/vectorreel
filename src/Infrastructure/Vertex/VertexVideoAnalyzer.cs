using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using MdReel.Core.Domain;
using MdReel.Core.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MdReel.Infrastructure.Vertex;

/// <summary>
/// Stage B against Vertex Gemini (native video) over REST. Structured output, the mandatory guard
/// options from <see cref="StageBCallOptions"/> (CLAUDE.md rule 9), cue injection (the Phase 3 gate
/// lesson), and per-timestamp coverage recovered from the bill.
/// </summary>
public sealed class VertexVideoAnalyzer(
    HttpClient httpClient,
    IAccessTokenProvider tokenProvider,
    IOptions<VertexOptions> options,
    ILogger<VertexVideoAnalyzer> logger) : IVideoAnalyzer
{
    // Vertex tokenizes video at a fixed rate, independent of content (experiments/001, YOUTUBE.md).
    // This is what lets us recover the TRUE fetched duration off the bill — mandatory on the YouTube
    // path where Vertex clamps the window to the end of the video.
    private const double VideoTokensPerSecondDefault = 258.0;
    private const double VideoTokensPerSecondLow = 66.0;

    private static readonly JsonSerializerOptions PayloadJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly VertexOptions _options = options.Value;

    public async Task<StageBModelResponse> AnalyzeAsync(
        string sourceUri,
        Segment segment,
        StageBCallOptions callOptions,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceUri);
        ArgumentNullException.ThrowIfNull(segment);
        ArgumentNullException.ThrowIfNull(callOptions);

        var mediaResolution = segment.Sampling.MediaResolution == MediaResolution.Low
            ? "MEDIA_RESOLUTION_LOW"
            : null;

        var request = BuildRequest(sourceUri, segment, callOptions, mediaResolution);

        GenerateContentResponse? response;
        try
        {
            response = await PostAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The only cancellation the runner arms is the per-call wall-clock timeout (rule 9).
            logger.LogWarning("Stage B call for segment {Index} timed out or was cancelled", segment.Index);
            return new StageBModelResponse(StageBFinishReason.Timeout, null, null);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Stage B call for segment {Index} failed at transport level", segment.Index);
            return new StageBModelResponse(StageBFinishReason.Error, null, null);
        }

        return Interpret(response, mediaResolution, segment);
    }

    private GenerateContentRequest BuildRequest(
        string sourceUri,
        Segment segment,
        StageBCallOptions callOptions,
        string? mediaResolution)
    {
        var videoPart = new VertexPart
        {
            // Vertex rejects fileData without a mimeType (400), including for YouTube watch URLs.
            FileData = new VertexFileData("video/mp4", sourceUri),
            VideoMetadata = new VertexVideoMetadata
            {
                StartOffset = Offset(segment.Start),
                EndOffset = Offset(segment.End),
            },
        };

        var textPart = new VertexPart { Text = VertexStageBPrompt.Build(segment) };

        var generationConfig = new VertexGenerationConfig
        {
            Temperature = _options.Temperature,
            MaxOutputTokens = callOptions.MaxOutputTokens,
            ThinkingConfig = new VertexThinkingConfig { ThinkingBudget = callOptions.ThinkingBudget },
            ResponseMimeType = "application/json",
            ResponseSchema = VertexStageBPrompt.ResponseSchema,
            MediaResolution = mediaResolution,
        };

        return new GenerateContentRequest(
            [new VertexContent("user", [videoPart, textPart])],
            generationConfig);
    }

    private StageBModelResponse Interpret(
        GenerateContentResponse? response,
        string? mediaResolution,
        Segment segment)
    {
        var candidate = response?.Candidates is { Count: > 0 } candidates ? candidates[0] : null;
        var finish = candidate?.FinishReason;

        // A MAX_TOKENS overflow is deterministic (ARCHITECTURE §3): report it so the runner halves
        // the segment rather than parsing truncated JSON as an "invalid" failure and retrying.
        if (string.Equals(finish, "MAX_TOKENS", StringComparison.OrdinalIgnoreCase))
        {
            return new StageBModelResponse(StageBFinishReason.MaxTokens, null, null);
        }

        var text = ExtractText(candidate);
        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogWarning("Stage B segment {Index} returned no text (finish={Finish})", segment.Index, finish);
            return new StageBModelResponse(StageBFinishReason.Error, null, null);
        }

        VertexStageBPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<VertexStageBPayload>(text, PayloadJson);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Stage B segment {Index} returned invalid JSON (finish={Finish})", segment.Index, finish);
            return new StageBModelResponse(StageBFinishReason.InvalidJson, null, null);
        }

        if (payload?.Blocks is null)
        {
            return new StageBModelResponse(StageBFinishReason.InvalidJson, null, null);
        }

        var output = new StageBModelOutput(
            SegmentStart: payload.SegmentStart ?? VertexStageBPrompt.FormatHhmmss(segment.Start),
            Language: payload.Language,
            Blocks: [.. payload.Blocks.Select(ToModelBlock)],
            Summary: payload.SegmentSummary);

        var fetched = FetchedDuration(response?.UsageMetadata, mediaResolution);
        return new StageBModelResponse(StageBFinishReason.Stop, output, fetched);
    }

    private async Task<GenerateContentResponse?> PostAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        var url = BuildUrl(_options.Region);

        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request),
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var httpResponse = await httpClient.SendAsync(message, cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Vertex generateContent returned {(int)httpResponse.StatusCode}: {Truncate(body, 800)}");
        }

        return await httpResponse.Content.ReadFromJsonAsync<GenerateContentResponse>(cancellationToken);
    }

    private string BuildUrl(string region) =>
        $"https://{region}-aiplatform.googleapis.com/{_options.ApiVersion}/projects/{_options.Project}"
        + $"/locations/{region}/publishers/google/models/{_options.Model}:generateContent";

    private static StageBModelBlock ToModelBlock(VertexStageBBlock block) => new(
        Timestamp: block.T ?? "00:00:00",
        Spoken: block.Spoken,
        Speaker: block.Speaker,
        OnScreenText: block.OnScreenText,
        Visual: block.Visual,
        Kind: block.Kind ?? "other");

    private static string? ExtractText(VertexCandidate? candidate)
    {
        if (candidate?.Content?.Parts is not { Count: > 0 } parts)
        {
            return null;
        }

        return string.Concat(parts.Select(p => p.Text ?? string.Empty));
    }

    private static TimeSpan? FetchedDuration(VertexUsageMetadata? usage, string? mediaResolution)
    {
        if (usage?.PromptTokensDetails is not { Count: > 0 } details)
        {
            return null;
        }

        var videoTokens = details
            .Where(d => string.Equals(d.Modality, "VIDEO", StringComparison.OrdinalIgnoreCase))
            .Sum(d => d.TokenCount);

        if (videoTokens <= 0)
        {
            return null;
        }

        var rate = string.Equals(mediaResolution, "MEDIA_RESOLUTION_LOW", StringComparison.OrdinalIgnoreCase)
            ? VideoTokensPerSecondLow
            : VideoTokensPerSecondDefault;

        return TimeSpan.FromSeconds(videoTokens / rate);
    }

    private static string Offset(TimeSpan value)
    {
        var seconds = Math.Max(0, (int)Math.Round(value.TotalSeconds, MidpointRounding.AwayFromZero));
        return string.Create(CultureInfo.InvariantCulture, $"{seconds}s");
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
