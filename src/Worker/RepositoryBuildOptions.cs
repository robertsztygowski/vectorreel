namespace MdReel.Worker;

/// <summary>
/// Config for generating a §4b repository from a licence-verified corpus plus whatever the batch
/// has actually produced (`scripts/generate-collection-repo.sh`).
/// </summary>
public sealed record RepositoryBuildOptions
{
    public bool Enabled { get; init; }

    public string Collection { get; init; } = string.Empty;

    /// <summary>Human-facing repository name, e.g. "AI Engineering".</summary>
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    /// <summary>The licence audit trail. Its `full` tier is matched against produced documents.</summary>
    public string CorpusPath { get; init; } = string.Empty;

    /// <summary>Root the batch wrote to; `<c>{bucket}/{prefix}/{collection}/{videoId}/output.json</c>` beneath it.</summary>
    public string ProducedRoot { get; init; } = string.Empty;

    public string OutputBucket { get; init; } = "outputs-eu";

    public string OutputPrefix { get; init; } = "collections";

    /// <summary>Directory the repository is written to (a git working tree).</summary>
    public string TargetDirectory { get; init; } = string.Empty;

    public string Generator { get; init; } = "mdreel@0.1.0";

    /// <summary>
    /// Scaffold mode: render the repository with no sessions at all. Used to stand up a collection
    /// before its first batch, so the conventions and issue forms exist from day one.
    /// </summary>
    public bool ScaffoldOnly { get; init; }
}
