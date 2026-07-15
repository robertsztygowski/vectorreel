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
    Task<SegmentAnalysis> AnalyzeAsync(string sourceUri, Segment segment, CancellationToken cancellationToken);
}

/// <summary>What one block of a segment says. Mirrors the response schema in ARCHITECTURE.md §3.</summary>
public sealed record AnalyzedBlock(
    TimeSpan At,
    string? Spoken,
    string? Speaker,
    string? OnScreenText,
    string? Visual,
    string Kind);

/// <summary>Stage B's structured output for one segment.</summary>
public sealed record SegmentAnalysis(
    int SegmentIndex,
    TimeSpan SegmentStart,
    string? Language,
    IReadOnlyList<AnalyzedBlock> Blocks,
    string? Summary);
