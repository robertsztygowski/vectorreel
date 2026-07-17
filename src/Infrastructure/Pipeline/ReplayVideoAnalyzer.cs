using MdReel.Core.Domain;
using MdReel.Core.Providers;

namespace MdReel.Infrastructure.Pipeline;

/// <summary>
/// Replays a committed Stage B response for the segment window from the fixture store — no Vertex
/// call. A missing fixture throws (see <see cref="FixtureStore.Read{T}"/>) so drift is loud, not
/// silent. Used in <see cref="PipelineModelMode.Replay"/>.
/// </summary>
public sealed class ReplayVideoAnalyzer(FixtureStore fixtures) : IVideoAnalyzer
{
    public Task<StageBModelResponse> AnalyzeAsync(
        string sourceUri,
        Segment segment,
        StageBCallOptions callOptions,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = fixtures.Read<StageBModelResponse>(FixtureKey.ForStageB(segment));
        return Task.FromResult(response);
    }
}

/// <summary>
/// Calls the real Vertex analyzer, then writes the response to the fixture store keyed by the
/// segment window. Used in <see cref="PipelineModelMode.Record"/> to refresh <c>tests/fixtures/llm</c>.
/// </summary>
public sealed class RecordingVideoAnalyzer(IVideoAnalyzer inner, FixtureStore fixtures) : IVideoAnalyzer
{
    public async Task<StageBModelResponse> AnalyzeAsync(
        string sourceUri,
        Segment segment,
        StageBCallOptions callOptions,
        CancellationToken cancellationToken)
    {
        var response = await inner.AnalyzeAsync(sourceUri, segment, callOptions, cancellationToken);
        fixtures.Write(FixtureKey.ForStageB(segment), response);
        return response;
    }
}
