using MdReel.Core.Pipeline.StageB;

namespace MdReel.Tests.Unit.StageB;

/// <summary>
/// The licence gate, tested as a gate. Every case here is a way an unpublishable video could reach
/// the pipeline; all of them must fail loudly at load time rather than after twenty-five Vertex
/// calls have been paid for (CLAUDE.md rule 8, DISTRIBUTION.md licence gate).
/// </summary>
public sealed class CollectionCorpusTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"corpus-{Guid.NewGuid():N}.json");

    [Fact]
    public void Loads_full_tier_entries_with_their_attribution()
    {
        Write($$"""
        { "full": [ {{Entry("abc")}} ], "reference": [] }
        """);

        var sources = CollectionCorpus.Load(_path, []);

        var source = Assert.Single(sources);
        Assert.Equal("abc", source.VideoId);
        Assert.Equal(TimeSpan.FromSeconds(1800), source.Duration);
        Assert.Equal("CC BY", source.Attribution.Licence);
        Assert.Equal("https://www.youtube.com/watch?v=abc", source.Attribution.SourceUrl);
    }

    [Fact]
    public void Reference_tier_entries_are_never_produced()
    {
        // A reference entry is metadata. Processing one would be exactly the thing the tier exists
        // to avoid, and it would spend money doing it.
        Write($$"""
        {
          "full": [ {{Entry("abc")}} ],
          "reference": [ {{Entry("standard-licence-video", licence: "youtube")}} ]
        }
        """);

        var sources = CollectionCorpus.Load(_path, []);

        Assert.Equal(["abc"], sources.Select(s => s.VideoId));
    }

    [Fact]
    public void A_full_entry_that_is_not_CC_BY_is_a_hard_stop()
    {
        Write($$"""
        { "full": [ {{Entry("abc", licence: "youtube")}} ] }
        """);

        var ex = Assert.Throws<InvalidOperationException>(() => CollectionCorpus.Load(_path, []));
        Assert.Contains("hard stop", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("licence_verified_via")]
    [InlineData("licence_verified_at")]
    [InlineData("attribution")]
    public void A_full_entry_without_licence_evidence_is_refused(string missingField)
    {
        Write($$"""
        { "full": [ {{Entry("abc", omit: missingField)}} ] }
        """);

        var ex = Assert.Throws<InvalidOperationException>(() => CollectionCorpus.Load(_path, []));
        Assert.Contains("pending a check", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Only_video_ids_narrows_the_batch()
    {
        Write($$"""
        { "full": [ {{Entry("abc")}}, {{Entry("def")}}, {{Entry("ghi")}} ] }
        """);

        var sources = CollectionCorpus.Load(_path, ["def"]);

        Assert.Equal(["def"], sources.Select(s => s.VideoId));
    }

    [Fact]
    public void An_empty_full_tier_is_an_error_not_a_no_op()
    {
        Write("""{ "full": [], "reference": [] }""");

        Assert.Throws<InvalidOperationException>(() => CollectionCorpus.Load(_path, []));
    }

    private static string Entry(string id, string licence = "creativeCommon", string? omit = null)
    {
        var fields = new List<string>
        {
            $"\"video_id\": \"{id}\"",
            $"\"url\": \"https://www.youtube.com/watch?v={id}\"",
            $"\"title\": \"Title {id}\"",
            "\"channel\": \"Channel\"",
            "\"duration_s\": 1800",
            $"\"licence\": \"{licence}\"",
        };

        if (omit != "licence_verified_via") fields.Add("\"licence_verified_via\": \"youtube.data.api.v3\"");
        if (omit != "licence_verified_at") fields.Add("\"licence_verified_at\": \"2026-07-21T10:00:00+00:00\"");
        if (omit != "attribution") fields.Add($"\"attribution\": \"\\\"Title {id}\\\" by Channel, CC BY 3.0\"");

        return "{" + string.Join(", ", fields) + "}";
    }

    private void Write(string json) => File.WriteAllText(_path, json);

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
