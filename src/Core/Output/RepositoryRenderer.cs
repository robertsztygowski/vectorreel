using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MdReel.Core.Output;

/// <summary>
/// Renders a set of §4 documents plus curation metadata into an ARCHITECTURE.md §4b repository.
///
/// This is a **rendering step**, the same category as Stage D's Markdown renderer — no model calls,
/// no compute, no I/O. It is a pure function on purpose: §4b promises that the same documents and
/// the same curation metadata produce the same repository bytes, which is what lets a regenerated
/// collection be diffed rather than re-reviewed.
///
/// The rule that shapes everything here: <b>sessions are the only content; indexes cite, never
/// restate.</b> No index file may contain prose that is not in a session document — an index that
/// copies content is an index that can contradict it.
/// </summary>
public static class RepositoryRenderer
{
    private static readonly JsonSerializerOptions ManifestJson = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static RenderedRepository Render(
        IReadOnlyList<RepositoryInput> inputs,
        RepositoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(options);

        var entries = BuildEntries(inputs, options);
        var files = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var entry in entries.Where(e => e.Input.Inclusion == InclusionTier.Full))
        {
            files[$"sessions/{entry.Slug}.md"] = OutputMarkdownRenderer.Render(entry.Input.Document!);
        }

        var topics = BuildTopics(entries, options);
        foreach (var topic in topics)
        {
            files[$"topics/{topic.Slug}.md"] = RenderIndex(
                $"Topic: {topic.Label}",
                $"Sessions that touch **{topic.Label}** — cited, never restated (ARCHITECTURE.md §4b).",
                topic.Entries);
        }

        var speakers = BuildSpeakers(entries);
        foreach (var speaker in speakers)
        {
            files[$"speakers/{speaker.Slug}.md"] = RenderIndex(
                $"Speaker: {speaker.Name}",
                $"Sessions featuring **{speaker.Name}** (curation metadata — ARCHITECTURE.md §4b).",
                speaker.Entries);
        }

        files["timeline/index.md"] = RenderTimeline(entries);
        files["README.md"] = RenderReadme(entries, topics, speakers, options);
        files["metadata/manifest.json"] = RenderManifest(entries, topics, speakers, options);

        return new RenderedRepository(files);
    }

    private static List<Entry> BuildEntries(IReadOnlyList<RepositoryInput> inputs, RepositoryOptions options)
    {
        var taken = new HashSet<string>(StringComparer.Ordinal);
        List<Entry> entries = [];

        foreach (var input in inputs)
        {
            if (input.Inclusion == InclusionTier.Full && input.Document is null)
            {
                throw new InvalidOperationException(
                    $"'{input.Title}' is full tier but carries no document. A full entry IS its document.");
            }

            if (input.Inclusion == InclusionTier.Reference)
            {
                if (input.Document is not null)
                {
                    throw new InvalidOperationException(
                        $"'{input.Title}' is reference tier but carries a document. Reference entries are "
                        + "metadata only — nothing about them was processed.");
                }

                if (input.Citations.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"'{input.Title}' is reference tier with no citations. A reference entry that cites "
                        + "nothing is a link, not an index entry.");
                }
            }

            if (options.Public && string.IsNullOrWhiteSpace(input.Attribution))
            {
                throw new InvalidOperationException(
                    $"'{input.Title}' has no attribution line and this is a public collection.");
            }

            var slug = RepositorySlug.Deduplicate(
                RepositorySlug.ForSession(input.Title, input.RecordedAt),
                taken);

            entries.Add(new Entry(slug, input, ResolveTags(input, options)));
        }

        return entries;
    }

    // A full entry's topics come from its document's frontmatter — the document is the source of
    // truth about its own content. A reference entry has no document, so its tags are curation.
    private static IReadOnlyList<string> ResolveTags(RepositoryInput input, RepositoryOptions options)
    {
        var raw = input.Inclusion == InclusionTier.Full
            ? input.Document!.Frontmatter.Tags
            : input.Tags;

        return [.. raw
            .Select(RepositorySlug.Slugify)
            .Select(tag => options.TopicMerges.TryGetValue(tag, out var merged) ? merged : tag)
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.Ordinal)];
    }

    private static List<TopicIndex> BuildTopics(List<Entry> entries, RepositoryOptions options)
    {
        var byTag = new SortedDictionary<string, List<Entry>>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            foreach (var tag in entry.Tags)
            {
                if (!byTag.TryGetValue(tag, out var list))
                {
                    byTag[tag] = list = [];
                }

                list.Add(entry);
            }
        }

        return [.. byTag.Select(kv => new TopicIndex(
            kv.Key,
            Humanize(kv.Key),
            [.. options.TopicMerges.Where(m => m.Value == kv.Key).Select(m => m.Key).OrderBy(x => x, StringComparer.Ordinal)],
            kv.Value))];
    }

    private static List<SpeakerIndex> BuildSpeakers(List<Entry> entries)
    {
        var bySlug = new SortedDictionary<string, (string Name, List<Entry> Entries)>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            foreach (var name in entry.Input.Speakers)
            {
                var slug = RepositorySlug.Slugify(name);
                if (slug.Length == 0)
                {
                    continue;
                }

                if (!bySlug.TryGetValue(slug, out var existing))
                {
                    bySlug[slug] = existing = (name, []);
                }

                existing.Entries.Add(entry);
            }
        }

        return [.. bySlug.Select(kv => new SpeakerIndex(kv.Key, kv.Value.Name, kv.Value.Entries))];
    }

    /// <summary>
    /// One index file. Full entries cite into their session document; reference entries cite out to
    /// the original with a <c>t=</c> offset and are marked, so a reader can tell the tiers apart on
    /// every line.
    /// </summary>
    private static string RenderIndex(string title, string blurb, IReadOnlyList<Entry> entries)
    {
        var builder = new StringBuilder();
        builder.Append("# ").Append(title).Append("\n\n");
        builder.Append(blurb).Append("\n\n");

        foreach (var entry in Chronological(entries))
        {
            if (entry.Input.Inclusion == InclusionTier.Full)
            {
                builder.Append("## [").Append(entry.Input.Title)
                    .Append("](../sessions/").Append(entry.Slug).Append(".md)\n\n");

                foreach (var section in entry.Input.Document!.Sections)
                {
                    builder.Append("- [").Append(section.Timestamp).Append("](../sessions/")
                        .Append(entry.Slug).Append(".md#")
                        .Append(RepositorySlug.GithubAnchor(section.Timestamp, section.Heading))
                        .Append(") — ").Append(section.Heading).Append('\n');
                }
            }
            else
            {
                builder.Append("## [").Append(entry.Input.Title).Append("](")
                    .Append(entry.Input.Source).Append(") · reference\n\n");

                foreach (var citation in entry.Input.Citations)
                {
                    builder.Append("- [").Append(citation.Timestamp).Append("](")
                        .Append(DeepLink(entry.Input.Source, citation.Timestamp))
                        .Append(") — ").Append(citation.Label).Append(" · reference\n");
                }
            }

            builder.Append('\n');
        }

        return Normalize(builder.ToString());
    }

    private static string RenderTimeline(IReadOnlyList<Entry> entries)
    {
        var builder = new StringBuilder("# Timeline\n\nAll sessions in chronological order (recording/publication date).\n\n");

        foreach (var year in Chronological(entries)
            .GroupBy(e => e.Input.RecordedAt[..4])
            .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            builder.Append("## ").Append(year.Key).Append("\n\n");
            foreach (var entry in year)
            {
                builder.Append("- ").Append(entry.Input.RecordedAt).Append(" — ");
                if (entry.Input.Inclusion == InclusionTier.Full)
                {
                    builder.Append('[').Append(entry.Input.Title).Append("](../sessions/")
                        .Append(entry.Slug).Append(".md) · `")
                        .Append(entry.Input.Document!.Frontmatter.Duration).Append('`');
                }
                else
                {
                    builder.Append('[').Append(entry.Input.Title).Append("](")
                        .Append(entry.Input.Source).Append(") · reference");
                }

                if (!string.IsNullOrWhiteSpace(entry.Input.Event))
                {
                    builder.Append(" · ").Append(entry.Input.Event);
                }

                builder.Append('\n');
            }

            builder.Append('\n');
        }

        return Normalize(builder.ToString());
    }

    private static string RenderReadme(
        List<Entry> entries,
        List<TopicIndex> topics,
        List<SpeakerIndex> speakers,
        RepositoryOptions options)
    {
        var full = entries.Count(e => e.Input.Inclusion == InclusionTier.Full);
        var reference = entries.Count - full;

        var builder = new StringBuilder();
        builder.Append("# ").Append(options.Name).Append("\n\n");
        builder.Append(options.Description).Append("\n\n");
        builder.Append("This repository contains **").Append(full)
            .Append(full == 1 ? " session** of processed video knowledge" : " sessions** of processed video knowledge")
            .Append(" — structured, timestamped Markdown an AI assistant can explore and cite — plus **")
            .Append(reference).Append(reference == 1 ? " reference entry**.\n\n" : " reference entries**.\n\n");

        builder.Append("## Two publication tiers\n\n");
        builder.Append("| Tier | What it is | Where it appears |\n|---|---|---|\n");
        builder.Append("| **`full`** | A complete mdreel session document: structured, timestamped Markdown with verbatim on-screen text. A near-complete derivative, so **CC BY sources only**. | `sessions/` + every index |\n");
        builder.Append("| **`reference`** | An index entry only — title, speaker, event, year, tags and deep links into the original. **No derived text, nothing processed.** | indexes only, always linking out |\n\n");
        builder.Append("An index entry linking to `sessions/…` is `full`; one linking to the original video and marked `· reference` was never processed. The distinction is visible on every line on purpose.\n\n");

        builder.Append("## How to navigate\n\n");
        builder.Append("| Start here | If you are |\n|---|---|\n");
        builder.Append("| [`metadata/manifest.json`](metadata/manifest.json) | an agent that parses JSON — the full structured index |\n");
        builder.Append("| [`sessions/`](sessions/) | reading a single talk end-to-end |\n");
        builder.Append("| [`topics/`](topics/) | looking for every session that touches a subject |\n");
        builder.Append("| [`speakers/`](speakers/) | following one person across sessions |\n");
        builder.Append("| [`timeline/index.md`](timeline/index.md) | browsing chronologically |\n\n");

        builder.Append("**").Append(topics.Count).Append("** topics · **")
            .Append(speakers.Count).Append("** speakers.\n\n");

        builder.Append("## How to cite\n\n");
        builder.Append("Every session section carries a `[hh:mm:ss]` timestamp. Cite as\n");
        builder.Append("`<session file> · [hh:mm:ss] <section heading>` — the timestamp is verifiable against the\n");
        builder.Append("original video linked in each session's *Source & licence* section. Reference entries carry\n");
        builder.Append("timestamps into the original directly.\n\n");

        builder.Append("## Licence\n\n").Append(options.LicenceNote).Append('\n');

        return Normalize(builder.ToString());
    }

    private static string RenderManifest(
        List<Entry> entries,
        List<TopicIndex> topics,
        List<SpeakerIndex> speakers,
        RepositoryOptions options)
    {
        var manifest = new
        {
            repository = new
            {
                name = options.Name,
                description = options.Description,
                generator = options.Generator,
                generated_at = options.GeneratedAt,
                visibility = options.Public ? "public-collection" : "private",
                contract_version = "1.1",
                licence_note = options.Public ? options.LicenceNote : null,
            },
            sessions = entries.Select(entry => entry.Input.Inclusion == InclusionTier.Full
                ? (object)new
                {
                    slug = entry.Slug,
                    inclusion = "full",
                    file = $"sessions/{entry.Slug}.md",
                    title = entry.Input.Document!.Frontmatter.Title,
                    duration = entry.Input.Document.Frontmatter.Duration,
                    language = entry.Input.Document.Frontmatter.Language,
                    processed_at = entry.Input.Document.Frontmatter.ProcessedAt,
                    recorded_at = entry.Input.RecordedAt,
                    source = entry.Input.Source,
                    @event = entry.Input.Event,
                    year = entry.Input.Year,
                    licence = entry.Input.Licence,
                    licence_verified_via = entry.Input.LicenceVerifiedVia,
                    tags = entry.Tags,
                    speakers = entry.Input.Speakers.Select(RepositorySlug.Slugify).ToArray(),
                    attribution = entry.Input.Attribution,
                }
                : new
                {
                    slug = entry.Slug,
                    inclusion = "reference",
                    title = entry.Input.Title,
                    recorded_at = entry.Input.RecordedAt,
                    source = entry.Input.Source,
                    @event = entry.Input.Event,
                    year = entry.Input.Year,
                    licence = entry.Input.Licence,
                    licence_verified_via = entry.Input.LicenceVerifiedVia,
                    citations = entry.Input.Citations.Select(c => new { timestamp = c.Timestamp, label = c.Label }),
                    tags = entry.Tags,
                    speakers = entry.Input.Speakers.Select(RepositorySlug.Slugify).ToArray(),
                    attribution = entry.Input.Attribution,
                }),
            topics = topics.Select(t => new
            {
                slug = t.Slug,
                file = $"topics/{t.Slug}.md",
                label = t.Label,
                merged_tags = t.MergedTags.Count > 0 ? t.MergedTags : null,
                sessions = Chronological(t.Entries).Select(e => e.Slug),
            }),
            speakers = speakers.Select(s => new
            {
                slug = s.Slug,
                file = $"speakers/{s.Slug}.md",
                name = s.Name,
                sessions = Chronological(s.Entries).Select(e => e.Slug),
            }),
        };

        // The manifest is JSON, but it lives in a git repository beside Markdown — same LF and
        // trailing-newline rules, so a regenerated repository diffs cleanly on every platform.
        return Normalize(JsonSerializer.Serialize(manifest, ManifestJson));
    }

    private static IReadOnlyList<Entry> Chronological(IReadOnlyList<Entry> entries) =>
        [.. entries
            .OrderBy(e => e.Input.RecordedAt, StringComparer.Ordinal)
            .ThenBy(e => e.Slug, StringComparer.Ordinal)];

    /// <summary>
    /// A deep link into an original video at a timestamp — how the reference tier cites material we
    /// may not render. Part of the §4b citation grammar, so it is public: the site renders the same
    /// links and must derive them identically.
    /// </summary>
    public static string DeepLink(string source, string timestamp)
    {
        var parts = timestamp.Split(':');
        var seconds = (int.Parse(parts[0], CultureInfo.InvariantCulture) * 3600)
            + (int.Parse(parts[1], CultureInfo.InvariantCulture) * 60)
            + int.Parse(parts[2], CultureInfo.InvariantCulture);
        var separator = source.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{source}{separator}t={seconds}s";
    }

    private static string Humanize(string slug) =>
        slug.Length == 0 ? slug : char.ToUpperInvariant(slug[0]) + slug[1..].Replace('-', ' ');

    // §4b portability: LF only, exactly one trailing newline. Enforced here rather than trusted, so
    // the repository renders identically on GitHub, Obsidian, VS Code and a bare file tree.
    private static string Normalize(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n') + "\n";

    private sealed record Entry(string Slug, RepositoryInput Input, IReadOnlyList<string> Tags);

    private sealed record TopicIndex(
        string Slug,
        string Label,
        IReadOnlyList<string> MergedTags,
        IReadOnlyList<Entry> Entries);

    private sealed record SpeakerIndex(string Slug, string Name, IReadOnlyList<Entry> Entries);
}
