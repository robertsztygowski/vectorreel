using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using MdReel.Core.Domain;

namespace MdReel.Infrastructure.Vertex;

/// <summary>
/// The Stage B prompt and response schema (ARCHITECTURE §3). The prompt lists the segment's forced
/// cues as <b>mandatory block starts</b> — the lesson from the Phase 3 gate
/// (<c>experiments/001-*/out/GATE.md</c>): given the cues, the model places its blocks on them to
/// the second; without them it under-segments on static screen recordings (N7b).
/// </summary>
internal static class VertexStageBPrompt
{
    // Vertex response schema (OpenAPI subset). Mirrors experiments/001-*/common.py STAGE_B_SCHEMA.
    public static object ResponseSchema { get; } = new
    {
        type = "OBJECT",
        properties = new
        {
            segment_start = new { type = "STRING" },
            language = new { type = "STRING" },
            blocks = new
            {
                type = "ARRAY",
                items = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        t = new { type = "STRING" },
                        spoken = new { type = "STRING", nullable = true },
                        speaker = new { type = "STRING", nullable = true },
                        on_screen_text = new { type = "STRING", nullable = true },
                        visual = new { type = "STRING" },
                        kind = new
                        {
                            type = "STRING",
                            @enum = new[] { "slide", "demo", "talking_head", "screen_share", "whiteboard", "other" },
                        },
                    },
                    required = new[] { "t", "spoken", "speaker", "on_screen_text", "visual", "kind" },
                },
            },
            segment_summary = new { type = "STRING" },
        },
        required = new[] { "segment_start", "language", "blocks", "segment_summary" },
    };

    private const string BaseRules = """
        You are analyzing one segment of a longer video for a searchable knowledge base.
        This segment starts at {0} in the full video.

        Break the segment into blocks: start a new block whenever the content meaningfully changes
        (new slide, new screen, new topic, new demonstrated action). Typical block length 20-90 s.

        Rules:
        - "t": timestamp of the block start WITHIN THIS CLIP, as hh:mm:ss with two-digit fields,
          e.g. "00:03:45" (00:00:00 = clip start). For clips under one hour the hours field stays
          "00". Never use frames or fractions of a second.
        - "spoken": cleaned transcript of what is actually said during the block. If nothing is
          spoken, use an empty string. NEVER invent or paraphrase speech that is not audible.
        - "speaker": the speaker's name only if stated or visible; otherwise "Speaker 1",
          "Speaker 2", ... consistently; null if there is no speech.
        - "on_screen_text": the important text visible on screen, VERBATIM (UI labels, headings,
          code, slide text). Do not correct spelling. Empty string if none.
        - "visual": one or two sentences describing what is shown or demonstrated.
        - "kind": one of slide | demo | talking_head | screen_share | whiteboard | other.
        - "segment_start": echo the value "{0}".
        - "segment_summary": 2-4 sentences summarizing this segment.
        - COVERAGE IS MANDATORY: the blocks together must cover the ENTIRE clip from 00:00:00 to
          the very end, with no gaps. Do not stop after the first block. A 10-15 minute clip
          typically yields 10-25 blocks.
        """;

    private const string CueGuidance = """


        MANDATORY BLOCK BOUNDARIES: a pre-analysis of the narration has already located the points
        where the topic changes. You MUST start a new block at each of these timestamps (you may add
        finer blocks between them, but you may NOT merge across them):
        {0}
        """;

    /// <summary>Build the prompt for a segment, injecting its forced cues when present.</summary>
    public static string Build(Segment segment)
    {
        var segmentStart = FormatHhmmss(segment.Start);
        var prompt = BaseRules.Replace("{0}", segmentStart, StringComparison.Ordinal);

        if (segment.Cues.Count > 0)
        {
            var list = new StringBuilder();
            foreach (var cue in segment.Cues)
            {
                list.Append("  - ").Append(FormatHhmmss(cue)).Append('\n');
            }

            prompt += CueGuidance.Replace("{0}", list.ToString().TrimEnd('\n'), StringComparison.Ordinal);
        }

        return prompt;
    }

    public static string FormatHhmmss(TimeSpan value)
    {
        var total = (int)Math.Round(value.TotalSeconds, MidpointRounding.AwayFromZero);
        if (total < 0)
        {
            total = 0;
        }

        return string.Create(CultureInfo.InvariantCulture, $"{total / 3600:D2}:{total % 3600 / 60:D2}:{total % 60:D2}");
    }
}

/// <summary>Structured Stage B payload as emitted by the model (snake_case JSON).</summary>
internal sealed record VertexStageBPayload(
    [property: JsonPropertyName("segment_start")] string? SegmentStart,
    [property: JsonPropertyName("language")] string? Language,
    [property: JsonPropertyName("blocks")] IReadOnlyList<VertexStageBBlock>? Blocks,
    [property: JsonPropertyName("segment_summary")] string? SegmentSummary);

internal sealed record VertexStageBBlock(
    [property: JsonPropertyName("t")] string? T,
    [property: JsonPropertyName("spoken")] string? Spoken,
    [property: JsonPropertyName("speaker")] string? Speaker,
    [property: JsonPropertyName("on_screen_text")] string? OnScreenText,
    [property: JsonPropertyName("visual")] string? Visual,
    [property: JsonPropertyName("kind")] string? Kind);
