using System.Text.Json.Serialization;

namespace MdReel.Core.Output;

public sealed record OutputDocument(
    [property: JsonPropertyName("frontmatter")] OutputFrontmatter Frontmatter,
    [property: JsonPropertyName("sections")] IReadOnlyList<OutputSection> Sections,
    [property: JsonPropertyName("provenance")] string Provenance);

public sealed record OutputFrontmatter(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("duration")] string Duration,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("processed_at")] string ProcessedAt,
    [property: JsonPropertyName("generator")] string Generator,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("tags")] IReadOnlyList<string> Tags);

public sealed record OutputSection(
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("heading")] string Heading,
    [property: JsonPropertyName("blocks")] IReadOnlyList<OutputBlock> Blocks);

public sealed record OutputBlock(
    [property: JsonPropertyName("label")] OutputBlockLabel Label,
    [property: JsonPropertyName("text")] string Text);

[JsonConverter(typeof(JsonStringEnumConverter<OutputBlockLabel>))]
public enum OutputBlockLabel
{
    [JsonStringEnumMemberName("spoken")]
    Spoken,

    [JsonStringEnumMemberName("on_screen")]
    OnScreen,

    [JsonStringEnumMemberName("visual")]
    Visual,
}
