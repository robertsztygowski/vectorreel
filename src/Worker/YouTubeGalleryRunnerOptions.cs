namespace MdReel.Worker;

public sealed record YouTubeGalleryRunnerOptions
{
    public bool Enabled { get; init; }

    public string OutputBucket { get; init; } = "outputs-eu";

    public string OutputPrefix { get; init; } = "gallery";

    public TimeSpan SegmentLength { get; init; } = TimeSpan.FromMinutes(10);

    public TimeSpan SegmentOverlap { get; init; } = TimeSpan.FromSeconds(20);

    public StageBCallOptionsDto StageB { get; init; } = new();

    public IReadOnlyList<YouTubeGalleryVideoOptions> Videos { get; init; } = [];

    public sealed record StageBCallOptionsDto
    {
        public int MaxOutputTokens { get; init; } = 12_000;

        public int ThinkingBudget { get; init; } = 4_096;

        public int TimeoutSeconds { get; init; } = 90;
    }
}

public sealed record YouTubeGalleryVideoOptions
{
    public string VideoId { get; init; } = string.Empty;

    public string SourceUri { get; init; } = string.Empty;

    public double DurationSeconds { get; init; }

    // Attribution for the "## Source & licence" section (CLAUDE.md rule 8). Curated + CC-licensed +
    // attributed is non-negotiable for gallery output.
    public string Title { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string Licence { get; init; } = string.Empty;

    public string SourceUrl { get; init; } = string.Empty;

    public MdReel.Core.Pipeline.StageB.GalleryAttribution? ToAttribution() =>
        string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Licence)
            ? null
            : new MdReel.Core.Pipeline.StageB.GalleryAttribution(
                Title,
                Author,
                Licence,
                string.IsNullOrWhiteSpace(SourceUrl) ? SourceUri : SourceUrl);
}
