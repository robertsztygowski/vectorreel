using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace MdReel.Core.Output;

public static partial class OutputMarkdownRenderer
{
    private static readonly Regex TimestampRegex = TimestampRegexFactory();
    private static readonly Regex ProcessedAtRegex = ProcessedAtRegexFactory();
    private static readonly Regex GeneratorRegex = GeneratorRegexFactory();
    private static readonly Regex TagRegex = TagRegexFactory();

    private const string SpokenMarker = "**Spoken:**";
    private const string OnScreenMarker = "**On screen:**";
    private const string VisualMarker = "**Visual:**";
    private const string ProvenanceHeading = "## Source & licence";
    private static readonly JsonSerializerOptions QuoteOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Render(OutputDocument document)
    {
        ValidateDocument(document);

        var output = new List<string>(capacity: 64)
        {
            "---",
            $"title: {Quote(document.Frontmatter.Title)}",
            $"source: {Quote(document.Frontmatter.Source)}",
            $"duration: {Quote(document.Frontmatter.Duration)}",
            $"language: {Quote(document.Frontmatter.Language)}",
            $"processed_at: {Quote(document.Frontmatter.ProcessedAt)}",
            $"generator: {Quote(document.Frontmatter.Generator)}",
            $"summary: {Quote(document.Frontmatter.Summary)}",
            $"tags: [{string.Join(", ", document.Frontmatter.Tags)}]",
            "---",
            string.Empty,
            $"# {document.Frontmatter.Title}",
        };

        foreach (var section in document.Sections)
        {
            output.Add(string.Empty);
            output.Add($"## [{section.Timestamp}] {section.Heading}");

            foreach (var block in section.Blocks)
            {
                output.Add(string.Empty);
                AppendBlock(output, block);
            }
        }

        output.Add(string.Empty);
        output.Add("---");
        output.Add(string.Empty);
        output.Add(ProvenanceHeading);
        output.Add(string.Empty);
        output.Add(document.Provenance);
        output.Add(string.Empty);
        return string.Join('\n', output);
    }

    private static void AppendBlock(List<string> output, OutputBlock block)
    {
        var lines = block.Text.Split('\n');
        switch (block.Label)
        {
            case OutputBlockLabel.Spoken:
                output.Add($"{SpokenMarker} {lines[0]}");
                output.AddRange(lines[1..]);
                break;
            case OutputBlockLabel.OnScreen:
                output.Add(OnScreenMarker);
                output.AddRange(lines.Select(line => $"> {line}"));
                break;
            case OutputBlockLabel.Visual:
                output.Add($"{VisualMarker} {lines[0]}");
                output.AddRange(lines[1..]);
                break;
            default:
                throw new OutputContractViolationException($"Unknown block label '{block.Label}'.");
        }
    }

    private static void ValidateDocument(OutputDocument document)
    {
        ValidateFrontmatter(document.Frontmatter);
        ValidateSections(document.Sections);
        ValidateProvenance(document.Provenance);
    }

    private static void ValidateFrontmatter(OutputFrontmatter frontmatter)
    {
        EnsureNonEmpty(frontmatter.Title, "frontmatter title");
        EnsureNonEmpty(frontmatter.Source, "frontmatter source");
        EnsureNonEmpty(frontmatter.Duration, "frontmatter duration");
        EnsureNonEmpty(frontmatter.Language, "frontmatter language");
        EnsureNonEmpty(frontmatter.ProcessedAt, "frontmatter processed_at");
        EnsureNonEmpty(frontmatter.Generator, "frontmatter generator");
        EnsureNonEmpty(frontmatter.Summary, "frontmatter summary");

        if (!TimestampRegex.IsMatch(frontmatter.Duration))
        {
            throw new OutputContractViolationException($"frontmatter duration '{frontmatter.Duration}' is not hh:mm:ss.");
        }

        if (!ProcessedAtRegex.IsMatch(frontmatter.ProcessedAt))
        {
            throw new OutputContractViolationException(
                $"frontmatter processed_at '{frontmatter.ProcessedAt}' is not UTC ISO-8601.");
        }

        if (!GeneratorRegex.IsMatch(frontmatter.Generator))
        {
            throw new OutputContractViolationException(
                $"frontmatter generator '{frontmatter.Generator}' does not match mdreel@<version>.");
        }

        if (frontmatter.Tags.Count == 0)
        {
            throw new OutputContractViolationException("frontmatter tags must contain at least one tag.");
        }

        foreach (var tag in frontmatter.Tags)
        {
            if (string.IsNullOrEmpty(tag) || !TagRegex.IsMatch(tag))
            {
                throw new OutputContractViolationException($"tag '{tag}' is not a lowercase slug.");
            }
        }
    }

    private static void ValidateSections(IReadOnlyList<OutputSection> sections)
    {
        if (sections.Count == 0)
        {
            throw new OutputContractViolationException("document has no timestamped sections.");
        }

        var previousSeconds = -1;
        foreach (var section in sections)
        {
            EnsureNonEmpty(section.Timestamp, "section timestamp");
            if (!TimestampRegex.IsMatch(section.Timestamp))
            {
                throw new OutputContractViolationException(
                    $"section timestamp '{section.Timestamp}' is not hh:mm:ss.");
            }

            var seconds = ToSeconds(section.Timestamp);
            if (seconds <= previousSeconds)
            {
                throw new OutputContractViolationException(
                    $"section timestamps are not strictly ascending at '{section.Timestamp}'.");
            }

            previousSeconds = seconds;
            EnsureNonEmpty(section.Heading, $"section [{section.Timestamp}] heading");

            if (!string.Equals(section.Heading, section.Heading.Trim(), StringComparison.Ordinal))
            {
                throw new OutputContractViolationException(
                    $"section [{section.Timestamp}] heading has surrounding whitespace.");
            }

            if (section.Blocks.Count == 0)
            {
                throw new OutputContractViolationException(
                    $"section [{section.Timestamp}] has no blocks.");
            }

            var previousLabelOrder = -1;
            foreach (var block in section.Blocks)
            {
                var labelOrder = LabelOrder(block.Label);
                if (labelOrder <= previousLabelOrder)
                {
                    throw new OutputContractViolationException(
                        $"section [{section.Timestamp}] blocks must be ordered as spoken, on_screen, visual.");
                }

                previousLabelOrder = labelOrder;
                ValidateBlockText(block);
            }
        }
    }

    private static void ValidateProvenance(string provenance)
    {
        EnsureNonEmpty(provenance, "provenance");
        if (provenance.Contains('\r', StringComparison.Ordinal))
        {
            throw new OutputContractViolationException("provenance contains a carriage return.");
        }

        if (!string.Equals(provenance, provenance.Trim(), StringComparison.Ordinal))
        {
            throw new OutputContractViolationException("provenance has surrounding whitespace.");
        }
    }

    private static void ValidateBlockText(OutputBlock block)
    {
        EnsureNonEmpty(block.Text, $"{block.Label} block text");
        if (block.Text.Contains('\r', StringComparison.Ordinal))
        {
            throw new OutputContractViolationException($"{block.Label} block contains a carriage return.");
        }

        var lines = block.Text.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().Length == 0)
            {
                throw new OutputContractViolationException($"{block.Label} block contains a blank line.");
            }

            if (block.Label is OutputBlockLabel.OnScreen)
            {
                continue;
            }

            if (line.StartsWith('>') || line.StartsWith('#') || line.Equals("---", StringComparison.Ordinal))
            {
                throw new OutputContractViolationException(
                    $"{block.Label} block line starts with reserved marker: '{line}'.");
            }

            if (line.StartsWith(SpokenMarker, StringComparison.Ordinal) ||
                line.StartsWith(OnScreenMarker, StringComparison.Ordinal) ||
                line.StartsWith(VisualMarker, StringComparison.Ordinal))
            {
                throw new OutputContractViolationException(
                    $"{block.Label} block line starts with a block label marker.");
            }
        }
    }

    private static int LabelOrder(OutputBlockLabel label) =>
        label switch
        {
            OutputBlockLabel.Spoken => 0,
            OutputBlockLabel.OnScreen => 1,
            OutputBlockLabel.Visual => 2,
            _ => throw new OutputContractViolationException($"Unknown block label '{label}'."),
        };

    private static int ToSeconds(string timestamp)
    {
        var parts = timestamp.Split(':');
        return (int.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture) * 3600) +
               (int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture) * 60) +
               int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string Quote(string value) => JsonSerializer.Serialize(value, QuoteOptions);

    private static void EnsureNonEmpty(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new OutputContractViolationException($"{name} must be a non-empty string.");
        }
    }

    [GeneratedRegex(@"^\d{2}:[0-5]\d:[0-5]\d$")]
    private static partial Regex TimestampRegexFactory();

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d{1,3})?Z$")]
    private static partial Regex ProcessedAtRegexFactory();

    [GeneratedRegex(@"^mdreel@[0-9A-Za-z.+-]+$")]
    private static partial Regex GeneratorRegexFactory();

    [GeneratedRegex(@"^[a-z0-9][a-z0-9 -]*$")]
    private static partial Regex TagRegexFactory();
}

public sealed class OutputContractViolationException(string message) : Exception(message);
