using System.Text.Json.Serialization;

namespace MdReel.Infrastructure.Vertex;

// Minimal Vertex generateContent wire model. Only the fields the pipeline sets or reads —
// deliberately not a full Vertex SDK surface (DEVELOPMENT.md §2: keep dependencies thin).

internal sealed record GenerateContentRequest(
    [property: JsonPropertyName("contents")] IReadOnlyList<VertexContent> Contents,
    [property: JsonPropertyName("generationConfig")] VertexGenerationConfig GenerationConfig);

internal sealed record VertexContent(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("parts")] IReadOnlyList<VertexPart> Parts);

internal sealed record VertexPart
{
    [property: JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; init; }

    [property: JsonPropertyName("fileData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VertexFileData? FileData { get; init; }

    [property: JsonPropertyName("videoMetadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VertexVideoMetadata? VideoMetadata { get; init; }
}

internal sealed record VertexFileData(
    [property: JsonPropertyName("mimeType")] string MimeType,
    [property: JsonPropertyName("fileUri")] string FileUri);

internal sealed record VertexVideoMetadata
{
    [property: JsonPropertyName("startOffset")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StartOffset { get; init; }

    [property: JsonPropertyName("endOffset")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EndOffset { get; init; }
}

internal sealed record VertexGenerationConfig
{
    [property: JsonPropertyName("temperature")]
    public double Temperature { get; init; }

    [property: JsonPropertyName("maxOutputTokens")]
    public int MaxOutputTokens { get; init; }

    [property: JsonPropertyName("thinkingConfig")]
    public VertexThinkingConfig ThinkingConfig { get; init; } = new();

    [property: JsonPropertyName("responseMimeType")]
    public string ResponseMimeType { get; init; } = "application/json";

    [property: JsonPropertyName("responseSchema")]
    public object? ResponseSchema { get; init; }

    [property: JsonPropertyName("mediaResolution")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MediaResolution { get; init; }
}

internal sealed record VertexThinkingConfig
{
    [property: JsonPropertyName("thinkingBudget")]
    public int ThinkingBudget { get; init; }
}

// --- response ---

internal sealed record GenerateContentResponse(
    [property: JsonPropertyName("candidates")] IReadOnlyList<VertexCandidate>? Candidates,
    [property: JsonPropertyName("usageMetadata")] VertexUsageMetadata? UsageMetadata);

internal sealed record VertexCandidate(
    [property: JsonPropertyName("content")] VertexContent? Content,
    [property: JsonPropertyName("finishReason")] string? FinishReason);

internal sealed record VertexUsageMetadata(
    [property: JsonPropertyName("candidatesTokenCount")] int CandidatesTokenCount,
    [property: JsonPropertyName("thoughtsTokenCount")] int ThoughtsTokenCount,
    [property: JsonPropertyName("promptTokenCount")] int PromptTokenCount,
    [property: JsonPropertyName("promptTokensDetails")] IReadOnlyList<VertexModalityTokens>? PromptTokensDetails);

internal sealed record VertexModalityTokens(
    [property: JsonPropertyName("modality")] string? Modality,
    [property: JsonPropertyName("tokenCount")] int TokenCount);
