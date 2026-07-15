namespace MdReel.Core.Domain;

/// <summary>What kind of resource a ledger entry accounts for.</summary>
public enum CostKind
{
    /// <summary>A Vertex call. Phase 2.</summary>
    Llm,

    /// <summary>
    /// A compute step — ffmpeg transcode/analysis. Metered from Phase 1 because it is not small:
    /// it is roughly a third of true COGS and has been an estimate, not a measurement, in every
    /// phase so far (METRICS.md N5). CLAUDE.md rule 6 covers compute, not just LLM calls.
    /// </summary>
    Compute,
}

/// <summary>
/// One metered step of a job. A product feature, not ops telemetry — the customer sees this
/// (ARCHITECTURE.md §8).
/// </summary>
/// <param name="JobId">The job this step belongs to.</param>
/// <param name="Kind">LLM or compute.</param>
/// <param name="Step">Stable identifier, e.g. <c>stage_a.scan</c>.</param>
/// <param name="Quantity">How much was consumed, in <paramref name="Unit"/>.</param>
/// <param name="Unit">e.g. <c>seconds</c>, <c>tokens</c>.</param>
/// <param name="Cents">
/// Cost in euro cents, or <c>null</c> when it is not yet knowable. Stage A leaves this null on
/// purpose: turning ffmpeg seconds into euros needs a Cloud Run rate we do not have when running
/// locally, and a fabricated rate is worse than an honest gap — the whole point of the ledger is
/// that the numbers in it are real. Phase 2 prices these entries; Phase 1 only counts them truthfully.
/// </param>
public sealed record CostEntry(
    string JobId,
    CostKind Kind,
    string Step,
    double Quantity,
    string Unit,
    int? Cents = null);

/// <summary>
/// Where metered steps go. Persistence is Phase 2 (ARCHITECTURE.md §6, <c>job_segments</c> /
/// <c>usage_ledger</c>); Phase 1 needs only the seam and an in-memory sink, so that the *measurement*
/// is in place at the point the compute happens rather than retrofitted around it later.
/// </summary>
public interface ICostLedger
{
    /// <summary>Record one metered step.</summary>
    void Record(CostEntry entry);
}

/// <summary>Collects entries in memory. Used by the worker until Phase 2 persists them, and by tests.</summary>
public sealed class InMemoryCostLedger : ICostLedger
{
    private readonly List<CostEntry> _entries = [];

    /// <summary>Everything recorded so far, in order.</summary>
    public IReadOnlyList<CostEntry> Entries => _entries;

    /// <inheritdoc />
    public void Record(CostEntry entry) => _entries.Add(entry);
}
