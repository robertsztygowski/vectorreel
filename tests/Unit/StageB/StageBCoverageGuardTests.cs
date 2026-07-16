using MdReel.Core.Pipeline.StageB;

namespace MdReel.Tests.Unit.StageB;

public sealed class StageBCoverageGuardTests
{
    [Fact]
    public void Under_coverage_is_rejected()
    {
        var verdict = StageBCoverageGuard.Evaluate(
            [TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(50)],
            TimeSpan.FromSeconds(300),
            StageBOptions.Default);

        Assert.False(verdict.IsValid);
        Assert.Equal("under-coverage", verdict.Reason);
    }

    [Fact]
    public void Over_coverage_is_rejected()
    {
        var verdict = StageBCoverageGuard.Evaluate(
            [TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(320)],
            TimeSpan.FromSeconds(300),
            StageBOptions.Default);

        Assert.False(verdict.IsValid);
        Assert.Equal("over-coverage", verdict.Reason);
    }

    [Fact]
    public void Coverage_inside_bounds_passes()
    {
        var verdict = StageBCoverageGuard.Evaluate(
            [TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(270)],
            TimeSpan.FromSeconds(300),
            StageBOptions.Default);

        Assert.True(verdict.IsValid);
    }
}
