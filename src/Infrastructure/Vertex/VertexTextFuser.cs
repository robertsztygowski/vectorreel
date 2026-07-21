using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MdReel.Core;
using MdReel.Core.Domain;
using MdReel.Core.Output;
using MdReel.Core.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MdReel.Infrastructure.Vertex;

/// <summary>
/// Stage C — fusion of the ordered Stage B segment analyses into one <see cref="OutputDocument"/>
/// via a text-only Vertex call. The model chooses topic boundaries, headings, title, summary and
/// tags; this class then <b>sanitizes</b> the result so it satisfies the frozen output contract
/// (<c>tests/fixtures/contracts/output.schema.json</c>) regardless of model sloppiness.
/// </summary>
public sealed partial class VertexTextFuser(
    HttpClient httpClient,
    IAccessTokenProvider tokenProvider,
    IOptions<VertexOptions> options,
    ICostLedger ledger,
    ILogger<VertexTextFuser> logger) : ITextFuser
{
    private static readonly JsonSerializerOptions PayloadJson = new() { PropertyNameCaseInsensitive = true };

    private readonly VertexOptions _options = options.Value;

    public async Task<OutputDocument> FuseAsync(
        IReadOnlyList<SegmentAnalysis> segments,
        FusionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(request);

        if (segments.Count == 0)
        {
            throw new InvalidOperationException("Stage C cannot fuse zero segments.");
        }

        var (payload, region, usage) = await CallModelAsync(segments, cancellationToken);

        // The fusion call bills too (CLAUDE.md rule 6); record which EU region served it after any
        // 429 fallback (INFRA.md). Stage B is metered by the runner; this is the Stage C counterpart.
        var tokenUsage = VertexUsage.ToTokenUsage(usage);
        ledger.Record(new CostEntry(
            JobId: request.JobId,
            Kind: CostKind.Llm,
            Step: "stage_c.fuse",
            Quantity: 1,
            Unit: "calls",
            Cents: tokenUsage is null ? null : LlmPricing.Cents(tokenUsage),
            Region: region,
            Microcents: tokenUsage is null ? null : LlmPricing.Microcents(tokenUsage)));
        RecordTokenMetrics(region, usage);

        var language = ResolveLanguage(payload.Language, segments);
        return Assemble(payload, request, language);
    }

    private async Task<(FusionPayload Payload, string Region, VertexUsageMetadata? Usage)> CallModelAsync(
        IReadOnlyList<SegmentAnalysis> segments,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(segments);
        var body = new GenerateContentRequest(
            [new VertexContent("user", [new VertexPart { Text = prompt }])],
            new VertexGenerationConfig
            {
                Temperature = _options.Temperature,
                MaxOutputTokens = _options.FusionMaxOutputTokens,
                ThinkingConfig = new VertexThinkingConfig { ThinkingBudget = _options.FusionThinkingBudget },
                ResponseMimeType = "application/json",
                ResponseSchema = FusionResponseSchema,
            });

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.FusionTimeout);

        var result = await VertexRegionInvoker.SendAsync(
            httpClient, _options, tokenProvider, BuildUrl, body, logger, timeout.Token);

        var candidate = result.Body?.Candidates is { Count: > 0 } c ? c[0] : null;
        var text = candidate?.Content?.Parts is { Count: > 0 } parts
            ? string.Concat(parts.Select(p => p.Text ?? string.Empty))
            : null;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException(
                $"Stage C fusion produced no text (finish={candidate?.FinishReason}).");
        }

        var payload = JsonSerializer.Deserialize<FusionPayload>(text, PayloadJson)
            ?? throw new InvalidOperationException("Stage C fusion returned unparseable JSON.");

        if (payload.Sections is not { Count: > 0 })
        {
            throw new InvalidOperationException("Stage C fusion returned no sections.");
        }

        return (payload, result.Region, result.Body?.UsageMetadata);
    }

    private static void RecordTokenMetrics(string region, VertexUsageMetadata? usage)
    {
        if (usage is null)
        {
            return;
        }

        PipelineDiagnostics.AddLlmTokens("input", region, usage.PromptTokenCount);
        PipelineDiagnostics.AddLlmTokens("output", region, usage.CandidatesTokenCount);
        PipelineDiagnostics.AddLlmTokens("thinking", region, usage.ThoughtsTokenCount);
    }

    private string BuildUrl(string region) =>
        $"https://{region}-aiplatform.googleapis.com/{_options.ApiVersion}/projects/{_options.Project}"
        + $"/locations/{region}/publishers/google/models/{_options.Model}:generateContent";

    private static OutputDocument Assemble(FusionPayload payload, FusionRequest request, string language)
    {
        var frontmatter = new OutputFrontmatter(
            Title: Fallback(payload.Title, request.Source),
            Source: request.Source,
            Duration: VertexStageBPrompt.FormatHhmmss(request.Duration),
            Language: language,
            ProcessedAt: DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            Generator: request.Generator,
            Summary: Fallback(payload.Summary, "Automatically generated summary."),
            Tags: SanitizeTags(payload.Tags));

        var sections = new List<OutputSection>(payload.Sections!.Count);
        foreach (var section in payload.Sections!)
        {
            var blocks = SanitizeBlocks(section.Blocks);
            if (blocks.Count == 0)
            {
                continue;
            }

            sections.Add(new OutputSection(
                Timestamp: NormalizeTimestamp(section.Timestamp),
                Heading: Fallback(section.Heading, "Section"),
                Blocks: blocks));
        }

        var ordered = OrderAndMergeSections(sections);
        if (ordered.Count == 0)
        {
            throw new InvalidOperationException("Stage C fusion yielded no usable sections after sanitization.");
        }

        return new OutputDocument(frontmatter, ordered, Fallback(request.Provenance, "Generated by mdreel."));
    }

    /// <summary>
    /// Enforces the §4 rule that section timestamps are strictly ascending.
    ///
    /// Two things make this necessary rather than defensive, and both were observed on the first
    /// real corpus run: the model does not reliably emit sections in chronological order, and
    /// <b>adjacent segments overlap by design</b>, so two segments legitimately describe the same
    /// moment and fuse to the same timestamp. The timestamp is ground truth (Stage B normalized it
    /// against the fetched duration); the ordering is presentation, so sorting is safe.
    ///
    /// Colliding sections are <b>merged, not dropped</b> — the overlap exists to avoid losing
    /// content at a boundary, and discarding the second section would throw away exactly what the
    /// overlap was bought to keep.
    /// </summary>
    private static List<OutputSection> OrderAndMergeSections(List<OutputSection> sections)
    {
        List<OutputSection> merged = [];

        foreach (var group in sections
            .GroupBy(s => s.Timestamp, StringComparer.Ordinal)
            .OrderBy(g => ParseTimestamp(g.Key)))
        {
            var members = group.ToList();
            if (members.Count == 1)
            {
                merged.Add(members[0]);
                continue;
            }

            // First writer wins per label, matching SanitizeBlocks' own rule, so a merge is
            // deterministic regardless of how many segments collided.
            var byLabel = new Dictionary<OutputBlockLabel, OutputBlock>();
            foreach (var block in members.SelectMany(m => m.Blocks))
            {
                byLabel.TryAdd(block.Label, block);
            }

            merged.Add(new OutputSection(
                Timestamp: group.Key,
                Heading: members[0].Heading,
                Blocks: [.. byLabel.OrderBy(kv => kv.Key).Select(kv => kv.Value)]));
        }

        return merged;
    }

    private static TimeSpan ParseTimestamp(string timestamp) =>
        TimeSpan.TryParseExact(timestamp, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : TimeSpan.Zero;

    private static List<OutputBlock> SanitizeBlocks(IReadOnlyList<FusionBlock>? blocks)
    {
        if (blocks is null)
        {
            return [];
        }

        // The contract allows each label at most once, in the order spoken, on_screen, visual.
        var byLabel = new Dictionary<OutputBlockLabel, string>();
        foreach (var block in blocks)
        {
            if (!TryParseLabel(block.Label, out var label))
            {
                continue;
            }

            var text = NormalizeText(block.Text);
            if (text.Length == 0 || byLabel.ContainsKey(label))
            {
                continue;
            }

            byLabel[label] = text;
        }

        var ordered = new List<OutputBlock>(3);
        foreach (var label in new[] { OutputBlockLabel.Spoken, OutputBlockLabel.OnScreen, OutputBlockLabel.Visual })
        {
            if (byLabel.TryGetValue(label, out var text))
            {
                ordered.Add(new OutputBlock(label, text));
            }
        }

        return ordered;
    }

    private static bool TryParseLabel(string? raw, out OutputBlockLabel label)
    {
        switch (raw?.Trim().ToLowerInvariant())
        {
            case "spoken":
                label = OutputBlockLabel.Spoken;
                return true;
            case "on_screen":
            case "on screen":
            case "onscreen":
                label = OutputBlockLabel.OnScreen;
                return true;
            case "visual":
                label = OutputBlockLabel.Visual;
                return true;
            default:
                label = OutputBlockLabel.Spoken;
                return false;
        }
    }

    private static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // The contract forbids carriage returns and blank lines; multi-line uses \n.
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(l => l.TrimEnd())
            .Where(l => l.Length > 0);

        return string.Join('\n', lines).Trim();
    }

    private static List<string> SanitizeTags(IReadOnlyList<string>? tags)
    {
        var cleaned = new List<string>();
        foreach (var raw in tags ?? [])
        {
            var tag = TagInvalidChars().Replace(raw.Trim().ToLowerInvariant(), "-").Trim('-', ' ');
            tag = MultiDash().Replace(tag, "-");
            if (tag.Length > 0 && !cleaned.Contains(tag))
            {
                cleaned.Add(tag);
            }
        }

        return cleaned.Count > 0 ? cleaned : ["video"];
    }

    private string ResolveLanguage(string? modelLanguage, IReadOnlyList<SegmentAnalysis> segments)
    {
        foreach (var candidate in new[] { modelLanguage }.Concat(segments.Select(s => s.Language)))
        {
            if (candidate is not null && LanguageTag().IsMatch(candidate.Trim()))
            {
                return candidate.Trim();
            }
        }

        logger.LogDebug("Stage C could not resolve a valid language tag; defaulting to 'en'.");
        return "en";
    }

    private static string NormalizeTimestamp(string? raw)
    {
        if (raw is not null && TimestampTag().IsMatch(raw.Trim()))
        {
            return raw.Trim();
        }

        var parts = (raw ?? string.Empty).Split(':');
        var seconds = 0;
        foreach (var part in parts)
        {
            seconds = seconds * 60 + (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0);
        }

        return VertexStageBPrompt.FormatHhmmss(TimeSpan.FromSeconds(seconds));
    }

    private static string Fallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string BuildPrompt(IReadOnlyList<SegmentAnalysis> segments)
    {
        var builder = new StringBuilder();
        builder.Append("""
            You are fusing the per-segment analysis of a video into one clean, structured document
            for a searchable knowledge base. Merge duplicate content that repeats across overlapping
            segments. Group the blocks into topic sections (NOT one section per segment): a section
            begins where the subject genuinely changes.

            For each section produce:
            - "timestamp": start of the section as hh:mm:ss (from the block times below).
            - "heading": a short, specific title for the section.
            - "blocks": 1-3 blocks, each with "label" in {spoken, on_screen, visual} used at most once,
              in that order. "spoken" = the cleaned narration; "on_screen" = verbatim on-screen text;
              "visual" = what is shown. Omit a label if there is nothing for it.

            Also produce: "title" (document title), "summary" (2-4 sentences), "language" (BCP-47 code
            like "en"), and "tags" (3-8 short lowercase keywords).

            Here are the ordered segment analyses (timestamps are global video time):

            """);

        foreach (var segment in segments)
        {
            foreach (var block in segment.Blocks)
            {
                builder.Append('[').Append(VertexStageBPrompt.FormatHhmmss(block.At)).Append("] ")
                    .Append(block.Kind);
                if (!string.IsNullOrWhiteSpace(block.Speaker))
                {
                    builder.Append(" (").Append(block.Speaker).Append(')');
                }

                builder.Append('\n');
                Append(builder, "spoken", block.Spoken);
                Append(builder, "on_screen", block.OnScreenText);
                Append(builder, "visual", block.Visual);
            }
        }

        return builder.ToString();

        static void Append(StringBuilder builder, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                builder.Append("  ").Append(label).Append(": ").Append(value.Trim()).Append('\n');
            }
        }
    }

    private static object FusionResponseSchema { get; } = new
    {
        type = "OBJECT",
        properties = new
        {
            title = new { type = "STRING" },
            summary = new { type = "STRING" },
            language = new { type = "STRING" },
            tags = new { type = "ARRAY", items = new { type = "STRING" } },
            sections = new
            {
                type = "ARRAY",
                items = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        timestamp = new { type = "STRING" },
                        heading = new { type = "STRING" },
                        blocks = new
                        {
                            type = "ARRAY",
                            items = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    label = new { type = "STRING", @enum = new[] { "spoken", "on_screen", "visual" } },
                                    text = new { type = "STRING" },
                                },
                                required = new[] { "label", "text" },
                            },
                        },
                    },
                    required = new[] { "timestamp", "heading", "blocks" },
                },
            },
        },
        required = new[] { "title", "summary", "language", "tags", "sections" },
    };

    [GeneratedRegex("^[a-z]{2}(-[A-Za-z]{2,4})?$")]
    private static partial Regex LanguageTag();

    [GeneratedRegex("^[0-9]{2}:[0-5][0-9]:[0-5][0-9]$")]
    private static partial Regex TimestampTag();

    [GeneratedRegex("[^a-z0-9 -]")]
    private static partial Regex TagInvalidChars();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultiDash();
}

internal sealed record FusionPayload(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("language")] string? Language,
    [property: JsonPropertyName("tags")] IReadOnlyList<string>? Tags,
    [property: JsonPropertyName("sections")] IReadOnlyList<FusionSection>? Sections);

internal sealed record FusionSection(
    [property: JsonPropertyName("timestamp")] string? Timestamp,
    [property: JsonPropertyName("heading")] string? Heading,
    [property: JsonPropertyName("blocks")] IReadOnlyList<FusionBlock>? Blocks);

internal sealed record FusionBlock(
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("text")] string? Text);
