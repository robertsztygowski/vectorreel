using MdReel.Core.Pipeline.StageB;

namespace MdReel.Tests.Unit.StageB;

public sealed class StageBTimestampNormalizerTests
{
    [Fact]
    public void Mixed_formats_are_normalized_per_timestamp()
    {
        var normalized = StageBTimestampNormalizer.Normalize(
            ["00:00:30", "00:14:87", "00:01:15"],
            TimeSpan.FromMinutes(3));

        Assert.Equal(
            [TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(14), TimeSpan.FromSeconds(75)],
            normalized);
    }

    [Fact]
    public void Impossible_hhmmss_fields_use_mmss_centiseconds_reading()
    {
        var normalized = StageBTimestampNormalizer.Normalize(
            ["00:99:10"],
            TimeSpan.FromMinutes(4));

        Assert.Equal(TimeSpan.FromSeconds(99), Assert.Single(normalized));
    }
}
