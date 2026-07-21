using System.Text.Json;
using MdReel.Core.Output;

namespace MdReel.Tests.Unit.Output;

/// <summary>
/// The §4b repository renderer. The properties under test are the ones a reader (human or agent)
/// relies on without being told: the tiers never blur, indexes cite rather than restate, and the
/// same input always renders the same bytes.
/// </summary>
public sealed class RepositoryRendererTests
{
    [Fact]
    public void Renders_the_full_contract_layout()
    {
        var repo = RepositoryRenderer.Render([FullInput("Rust lifetimes", "2020-04-22")], Options());

        Assert.Contains("README.md", repo.Files.Keys);
        Assert.Contains("metadata/manifest.json", repo.Files.Keys);
        Assert.Contains("timeline/index.md", repo.Files.Keys);
        Assert.Contains("sessions/2020-04-22-rust-lifetimes.md", repo.Files.Keys);
        Assert.Contains(repo.Files.Keys, k => k.StartsWith("topics/", StringComparison.Ordinal));
        Assert.Contains(repo.Files.Keys, k => k.StartsWith("speakers/", StringComparison.Ordinal));
    }

    [Fact]
    public void A_reference_entry_gets_no_session_document()
    {
        var repo = RepositoryRenderer.Render(
            [FullInput("Kept", "2024-01-01"), ReferenceInput("Not ours", "2025-02-02")],
            Options());

        Assert.Single(repo.Files.Keys, k => k.StartsWith("sessions/", StringComparison.Ordinal));
        Assert.DoesNotContain("sessions/2025-02-02-not-ours.md", repo.Files.Keys);
    }

    [Fact]
    public void Reference_citations_link_out_and_never_into_sessions()
    {
        var repo = RepositoryRenderer.Render(
            [FullInput("Kept", "2024-01-01"), ReferenceInput("Not ours", "2025-02-02")],
            Options());

        var timeline = repo.Files["timeline/index.md"];
        Assert.Contains("(https://www.youtube.com/watch?v=ref) · reference", timeline, StringComparison.Ordinal);

        var topic = repo.Files.First(f => f.Key.StartsWith("topics/", StringComparison.Ordinal) && f.Value.Contains("Not ours", StringComparison.Ordinal)).Value;
        foreach (var line in topic.Split('\n').Where(l => l.StartsWith("- ", StringComparison.Ordinal) && l.Contains("reference", StringComparison.Ordinal)))
        {
            Assert.DoesNotContain("../sessions/", line, StringComparison.Ordinal);
            Assert.Contains("t=", line, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Deep_links_convert_the_timestamp_to_seconds()
    {
        Assert.Equal("https://x/v?a=1&t=264s", RepositoryRenderer.DeepLink("https://x/v?a=1", "00:04:24"));
        Assert.Equal("https://x/v?t=3661s", RepositoryRenderer.DeepLink("https://x/v", "01:01:01"));
    }

    [Fact]
    public void Index_citations_use_the_github_anchor_and_the_section_heading_verbatim()
    {
        var repo = RepositoryRenderer.Render([FullInput("Talk", "2024-03-04")], Options());
        var topic = repo.Files.First(f => f.Key.StartsWith("topics/", StringComparison.Ordinal)).Value;

        // Indexes cite, never restate: the visible text is the section heading itself, and the
        // anchor is derived from it rather than invented.
        Assert.Contains(
            "- [00:00:00](../sessions/2024-03-04-talk.md#000000-introduction) — Introduction",
            topic,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Github_anchors_keep_existing_hyphens()
    {
        // Regression: GitHub strips punctuation but KEEPS hyphens. Dropping them produced anchors
        // that silently failed to resolve, and the canonical fixture had no hyphenated heading, so
        // nothing caught it until the oracle was pointed at a real generated repository.
        Assert.Equal(
            "000357-building-blocks-of-an-ai-infused-application",
            RepositorySlug.GithubAnchor("00:03:57", "Building Blocks of an AI-Infused Application"));
        Assert.Equal(
            "000901-vllm-high-performance-inference-engine",
            RepositorySlug.GithubAnchor("00:09:01", "vLLM: High-Performance Inference Engine"));
    }

    [Fact]
    public void Colliding_slugs_are_suffixed_deterministically()
    {
        var repo = RepositoryRenderer.Render(
            [FullInput("Same title", "2024-01-01"), FullInput("Same title", "2024-01-01")],
            Options());

        Assert.Contains("sessions/2024-01-01-same-title.md", repo.Files.Keys);
        Assert.Contains("sessions/2024-01-01-same-title-2.md", repo.Files.Keys);
    }

    [Fact]
    public void Rendering_is_deterministic()
    {
        var inputs = new[] { FullInput("A", "2024-01-01"), ReferenceInput("B", "2023-01-01") };
        var first = RepositoryRenderer.Render(inputs, Options());
        var second = RepositoryRenderer.Render(inputs, Options());

        Assert.Equal(first.Files.Count, second.Files.Count);
        foreach (var (path, content) in first.Files)
        {
            Assert.Equal(content, second.Files[path]);
        }
    }

    [Fact]
    public void Every_file_is_LF_with_exactly_one_trailing_newline()
    {
        var repo = RepositoryRenderer.Render(
            [FullInput("A", "2024-01-01"), ReferenceInput("B", "2023-01-01")],
            Options());

        foreach (var (path, content) in repo.Files)
        {
            Assert.DoesNotContain('\r', content);
            Assert.EndsWith("\n", content, StringComparison.Ordinal);
            Assert.False(content.EndsWith("\n\n", StringComparison.Ordinal), $"{path}: one trailing newline");
        }
    }

    [Fact]
    public void The_timeline_is_chronological_and_covers_every_entry()
    {
        var repo = RepositoryRenderer.Render(
            [FullInput("Later", "2025-06-01"), ReferenceInput("Earlier", "2021-02-03")],
            Options());

        var timeline = repo.Files["timeline/index.md"];
        Assert.True(
            timeline.IndexOf("2021-02-03", StringComparison.Ordinal)
            < timeline.IndexOf("2025-06-01", StringComparison.Ordinal));
        Assert.Contains("## 2021", timeline, StringComparison.Ordinal);
        Assert.Contains("## 2025", timeline, StringComparison.Ordinal);
    }

    [Fact]
    public void The_manifest_marks_the_tier_on_every_entry()
    {
        var repo = RepositoryRenderer.Render(
            [FullInput("A", "2024-01-01"), ReferenceInput("B", "2023-01-01")],
            Options());

        using var manifest = JsonDocument.Parse(repo.Files["metadata/manifest.json"]);
        var sessions = manifest.RootElement.GetProperty("sessions").EnumerateArray().ToList();

        Assert.Equal(2, sessions.Count);
        Assert.All(sessions, s => Assert.True(s.TryGetProperty("inclusion", out _)));
        Assert.Equal("1.1", manifest.RootElement.GetProperty("repository").GetProperty("contract_version").GetString());

        var reference = sessions.Single(s => s.GetProperty("inclusion").GetString() == "reference");
        Assert.False(reference.TryGetProperty("file", out _), "a reference entry claims no document");
        Assert.False(reference.TryGetProperty("duration", out _));
        Assert.True(reference.TryGetProperty("citations", out _));
    }

    [Fact]
    public void A_full_entry_without_a_document_is_refused()
    {
        var broken = FullInput("A", "2024-01-01") with { Document = null };
        var ex = Assert.Throws<InvalidOperationException>(() => RepositoryRenderer.Render([broken], Options()));
        Assert.Contains("full tier but carries no document", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_reference_entry_carrying_a_document_is_refused()
    {
        var broken = ReferenceInput("B", "2024-01-01") with { Document = Document("B") };
        var ex = Assert.Throws<InvalidOperationException>(() => RepositoryRenderer.Render([broken], Options()));
        Assert.Contains("metadata only", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_reference_entry_that_cites_nothing_is_refused()
    {
        var broken = ReferenceInput("B", "2024-01-01") with { Citations = [] };
        Assert.Throws<InvalidOperationException>(() => RepositoryRenderer.Render([broken], Options()));
    }

    [Fact]
    public void A_public_collection_entry_without_attribution_is_refused()
    {
        var broken = FullInput("A", "2024-01-01") with { Attribution = "" };
        var ex = Assert.Throws<InvalidOperationException>(() => RepositoryRenderer.Render([broken], Options()));
        Assert.Contains("no attribution line", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_private_export_needs_no_public_attribution()
    {
        // One contract, both surfaces (§4b): the licence discipline is a public-collection concern.
        var input = FullInput("A", "2024-01-01") with { Attribution = "" };
        var repo = RepositoryRenderer.Render([input], Options() with { Public = false });

        using var manifest = JsonDocument.Parse(repo.Files["metadata/manifest.json"]);
        Assert.Equal("private", manifest.RootElement.GetProperty("repository").GetProperty("visibility").GetString());
    }

    [Fact]
    public void Topic_merges_fold_synonyms_and_are_recorded()
    {
        var options = Options() with
        {
            TopicMerges = new Dictionary<string, string>(StringComparer.Ordinal) { ["llmops"] = "llm-ops" },
        };
        var input = FullInput("A", "2024-01-01") with { Document = Document("A", ["llmops", "llm-ops"]) };

        var repo = RepositoryRenderer.Render([input], options);

        Assert.Contains("topics/llm-ops.md", repo.Files.Keys);
        Assert.DoesNotContain("topics/llmops.md", repo.Files.Keys);

        using var manifest = JsonDocument.Parse(repo.Files["metadata/manifest.json"]);
        var topic = manifest.RootElement.GetProperty("topics").EnumerateArray()
            .Single(t => t.GetProperty("slug").GetString() == "llm-ops");
        Assert.Equal("llmops", topic.GetProperty("merged_tags").EnumerateArray().Single().GetString());
    }

    [Fact]
    public void Speakers_without_curation_metadata_get_no_index()
    {
        var input = FullInput("A", "2024-01-01") with { Speakers = [] };
        var repo = RepositoryRenderer.Render([input], Options());

        // §4b: unknown speakers stay "Speaker N" inside documents and get NO index file. Inventing
        // one would be the model naming a real person.
        Assert.DoesNotContain(repo.Files.Keys, k => k.StartsWith("speakers/", StringComparison.Ordinal));
    }

    private static RepositoryOptions Options() => new()
    {
        Name = "Test collection",
        Description = "A collection used by the renderer tests.",
        Generator = "mdreel@0.0.0-test",
        GeneratedAt = "2026-07-21T00:00:00Z",
    };

    private static RepositoryInput FullInput(string title, string date) => new()
    {
        Inclusion = InclusionTier.Full,
        Title = title,
        Source = "https://www.youtube.com/watch?v=abc",
        RecordedAt = date,
        Event = "Test Event",
        Year = int.Parse(date[..4], System.Globalization.CultureInfo.InvariantCulture),
        Licence = "creativeCommon",
        LicenceVerifiedVia = "youtube.data.api.v3 videos.list status.license",
        Attribution = $"\"{title}\" by Channel (https://www.youtube.com/watch?v=abc), CC BY 3.0",
        Speakers = ["Jon Gjengset"],
        Document = Document(title),
    };

    private static RepositoryInput ReferenceInput(string title, string date) => new()
    {
        Inclusion = InclusionTier.Reference,
        Title = title,
        Source = "https://www.youtube.com/watch?v=ref",
        RecordedAt = date,
        Event = "Other Event",
        Year = int.Parse(date[..4], System.Globalization.CultureInfo.InvariantCulture),
        Licence = "youtube",
        LicenceVerifiedVia = "youtube.data.api.v3 videos.list status.license",
        Attribution = $"\"{title}\" by Channel — standard YouTube licence, index entry only",
        Tags = ["agents"],
        Citations = [new RepositoryCitation("00:04:24", "Talk start")],
    };

    private static OutputDocument Document(string title, IReadOnlyList<string>? tags = null) => new(
        new OutputFrontmatter(
            Title: title,
            Source: "https://www.youtube.com/watch?v=abc",
            Duration: "00:30:00",
            Language: "en",
            ProcessedAt: "2026-07-21T00:00:00Z",
            Generator: "mdreel@0.0.0-test",
            Summary: "A summary.",
            Tags: tags ?? ["rag"]),
        [
            new OutputSection("00:00:00", "Introduction", [new OutputBlock(OutputBlockLabel.Spoken, "Hello.")]),
            new OutputSection("00:10:00", "The middle", [new OutputBlock(OutputBlockLabel.Spoken, "More.")]),
        ],
        "Generated by mdreel.");
}
