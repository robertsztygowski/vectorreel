using MdReel.Core.Output;
using MdReel.Core.Providers;

namespace MdReel.Infrastructure.Pipeline;

/// <summary>
/// Replays a committed fused <see cref="OutputDocument"/> for the given analyses from the fixture
/// store — no Vertex call. Missing fixtures throw. Used in <see cref="PipelineModelMode.Replay"/>.
/// </summary>
public sealed class ReplayTextFuser(FixtureStore fixtures) : ITextFuser
{
    public Task<OutputDocument> FuseAsync(
        IReadOnlyList<SegmentAnalysis> segments,
        FusionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var document = fixtures.Read<OutputDocument>(FixtureKey.ForStageC(segments));

        // The pipeline owns provenance/source/generator; overlay them so a replayed body is still
        // stamped with this job's real facts rather than the recording's.
        var overlaid = document with
        {
            Frontmatter = document.Frontmatter with
            {
                Source = request.Source,
                Generator = request.Generator,
            },
            Provenance = string.IsNullOrWhiteSpace(request.Provenance) ? document.Provenance : request.Provenance.Trim(),
        };

        return Task.FromResult(overlaid);
    }
}

/// <summary>
/// Calls the real Vertex fuser, then writes the fused document to the fixture store keyed by the
/// analyses. Used in <see cref="PipelineModelMode.Record"/>.
/// </summary>
public sealed class RecordingTextFuser(ITextFuser inner, FixtureStore fixtures) : ITextFuser
{
    public async Task<OutputDocument> FuseAsync(
        IReadOnlyList<SegmentAnalysis> segments,
        FusionRequest request,
        CancellationToken cancellationToken)
    {
        var document = await inner.FuseAsync(segments, request, cancellationToken);
        fixtures.Write(FixtureKey.ForStageC(segments), document);
        return document;
    }
}
