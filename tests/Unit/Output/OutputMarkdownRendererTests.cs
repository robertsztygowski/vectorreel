using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using MdReel.Core.Output;

namespace MdReel.Tests.Unit.Output;

public sealed class OutputMarkdownRendererTests
{
    private static readonly string RepoRoot = FindRepositoryRoot();
    private static readonly string FixturesDir = Path.Combine(RepoRoot, "tests", "fixtures", "output");
    private static readonly string SchemaPath = Path.Combine(RepoRoot, "tests", "fixtures", "contracts", "output.schema.json");
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    private static readonly JsonSchema OutputSchema = JsonSchema.FromText(File.ReadAllText(SchemaPath));

    [Fact]
    public void StageD_renderer_is_byte_compatible_with_canonical_markdown_fixtures()
    {
        foreach (var jsonPath in GetOutputFixtureJsonPaths())
        {
            var fixtureName = Path.GetFileNameWithoutExtension(jsonPath);
            var markdownPath = Path.Combine(FixturesDir, $"{fixtureName}.md");

            var json = File.ReadAllText(jsonPath);
            var expectedMarkdown = File.ReadAllText(markdownPath);

            var document = JsonSerializer.Deserialize<OutputDocument>(json, SerializerOptions);
            Assert.NotNull(document);

            var rendered = OutputMarkdownRenderer.Render(document);
            Assert.Equal(expectedMarkdown, rendered);
        }
    }

    [Fact]
    public void Output_json_mapping_and_fixtures_validate_against_frozen_schema()
    {
        foreach (var jsonPath in GetOutputFixtureJsonPaths())
        {
            var fixtureName = Path.GetFileName(jsonPath);
            var json = File.ReadAllText(jsonPath);
            var fixtureNode = JsonNode.Parse(json);
            Assert.NotNull(fixtureNode);

            var fixtureValidation = OutputSchema.Evaluate(fixtureNode);
            Assert.True(fixtureValidation.IsValid, $"Fixture {fixtureName} failed output.schema.json validation.");

            var document = JsonSerializer.Deserialize<OutputDocument>(json, SerializerOptions);
            Assert.NotNull(document);

            var mappedNode = JsonSerializer.SerializeToNode(document, SerializerOptions);
            Assert.NotNull(mappedNode);
            var mappedValidation = OutputSchema.Evaluate(mappedNode);
            Assert.True(mappedValidation.IsValid, $"Mapped document for {fixtureName} failed output.schema.json validation.");
        }
    }

    private static IEnumerable<string> GetOutputFixtureJsonPaths()
    {
        return Directory
            .EnumerateFiles(FixturesDir, "*.json", SearchOption.TopDirectoryOnly)
            .Where(path => !path.EndsWith("corpus.json", StringComparison.OrdinalIgnoreCase));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PLAN.md")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root not found from test base directory.");
    }
}
