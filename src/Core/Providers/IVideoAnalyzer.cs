using MdReel.Core.Domain;

namespace MdReel.Core.Providers;

/// <summary>
/// Stage B — segment analysis against a multimodal model (Vertex Gemini today, Mistral or a
/// self-hosted model later; that portability is the whole reason this interface exists,
/// DEVELOPMENT.md §3).
///
/// 🚧 <b>Not implemented in Phase 1.</b> The seam is declared so Stage A can be built and tested
/// against it; the implementation and its mandatory guards land in Phase 2:
/// <list type="bullet">
///   <item><description>a hard <c>maxOutputTokens</c>, a bounded <c>thinkingBudget</c>, and a wall-clock timeout — ~8% of benchmark calls ran away without them (ARCHITECTURE.md §3);</description></item>
///   <item><description>on <c>MAX_TOKENS</c>, <b>split the segment</b> — the overflow is deterministic, so retrying it unchanged just bills twice (<c>SegmentSplitPolicy</c> is already here for exactly this);</description></item>
///   <item><description>normalize timestamps <b>per timestamp</b> — the model mixes <c>mm:ss:cs</c> and <c>hh:mm:ss</c> inside one response;</description></item>
///   <item><description>guard coverage on <b>both</b> sides — a citation to a timestamp that does not exist is worse than no citation.</description></item>
/// </list>
/// </summary>
public interface IVideoAnalyzer
{
    /// <summary>Analyse one segment. <paramref name="segment"/> carries the sampling plan and the forced block cues.</summary>
    Task<StageBModelResponse> AnalyzeAsync(
        string sourceUri,
        Segment segment,
        StageBCallOptions callOptions,
        CancellationToken cancellationToken);
}

/// <summary>Required guards on every Stage B model call (CLAUDE.md rule 9).</summary>
public sealed record StageBCallOptions(int MaxOutputTokens, int ThinkingBudget, TimeSpan Timeout)
{
    public static StageBCallOptions Default { get; } = new(
        MaxOutputTokens: 12_000,
        ThinkingBudget: 4_096,
        Timeout: TimeSpan.FromSeconds(90));
}

/// <summary>How the model ended a Stage B call.</summary>
public enum StageBFinishReason
{
    Stop,
    MaxTokens,
    Timeout,
    InvalidJson,
    Error,
}

/// <summary>Raw model block before timestamp normalization.</summary>
public sealed record StageBModelBlock(
    string Timestamp,
    string? Spoken,
    string? Speaker,
    string? OnScreenText,
    string? Visual,
    string Kind);

/// <summary>Structured Stage B payload as emitted by the model.</summary>
public sealed record StageBModelOutput(
    string SegmentStart,
    string? Language,
    IReadOnlyList<StageBModelBlock> Blocks,
    string? Summary);

/// <summary>Model response plus execution metadata needed by Stage B guards.</summary>
/// <param name="FinishReason">How the model ended the call.</param>
/// <param name="Output">The parsed structured output, or <c>null</c> on a non-Stop finish.</param>
/// <param name="FetchedDuration">Fetched video duration recovered from the bill, when available.</param>
/// <param name="Region">
/// Vertex region the call actually ran in (after any <c>429</c> fallback), or <c>null</c> when no
/// real model call was made (fake/replay) or the call never reached the model (transport/timeout).
/// The Stage B runner meters an LLM ledger entry iff this is non-null.
/// </param>
public sealed record StageBModelResponse(
    StageBFinishReason FinishReason,
    StageBModelOutput? Output,
    TimeSpan? FetchedDuration,
    string? Region = null);

/// <summary>Stage B's validated output for one segment.</summary>
public sealed record SegmentAnalysis(
    int SegmentIndex,
    TimeSpan SegmentStart,
    string? Language,
    IReadOnlyList<AnalyzedBlock> Blocks,
    string? Summary);

/// <summary>What one validated block of a segment says.</summary>
public sealed record AnalyzedBlock(
    TimeSpan At,
    string? Spoken,
    string? Speaker,
    string? OnScreenText,
    string? Visual,
    string Kind);
