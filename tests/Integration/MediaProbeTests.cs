using MdReel.Core.Media;

namespace MdReel.Tests.Integration;

/// <summary>Real ffprobe against the committed fixtures. No cloud, no spend.</summary>
public sealed class MediaProbeTests
{
    private readonly FfprobeMediaProbe _probe = new(new MediaToolOptions());

    [Theory]
    [MemberData(nameof(Fixtures.All), MemberType = typeof(Fixtures))]
    public async Task Probes_a_committed_fixture(string path)
    {
        var probe = await _probe.ProbeAsync(path, CancellationToken.None);

        Assert.Equal(90, probe.DurationSeconds);
        Assert.Equal(1280, probe.Width);
        Assert.Equal(720, probe.Height);
        Assert.Equal("h264", probe.VideoCodec);
        Assert.True(probe.HasAudioStream);
    }

    // ffprobe reports frame rate as a rational, and this fixture's is 19001/317 — not a whole number.
    // Parsing it as an int silently truncates; parsing it as a decimal throws. Neither is hypothetical.
    [Fact]
    public async Task Reads_a_non_integer_frame_rate_as_a_fraction()
    {
        var probe = await _probe.ProbeAsync(Fixtures.TalkingHead, CancellationToken.None);

        Assert.Equal(59.94, probe.FrameRate, 1);
    }

    [Fact]
    public async Task Rejects_a_file_that_is_not_a_video()
    {
        var notAVideo = Path.Combine(Fixtures.VideoDirectory, "README.md");

        await Assert.ThrowsAsync<CorruptSourceException>(
            () => _probe.ProbeAsync(notAVideo, CancellationToken.None));
    }

    [Fact]
    public async Task Rejects_a_missing_file()
    {
        await Assert.ThrowsAsync<CorruptSourceException>(
            () => _probe.ProbeAsync(Path.Combine(Fixtures.VideoDirectory, "nope.mp4"), CancellationToken.None));
    }

    [Fact]
    public async Task Rejects_a_truncated_video()
    {
        // A real failure mode: an upload that died halfway. Rejecting it at the probe costs nothing;
        // discovering it inside Stage B means we have already paid Vertex.
        var truncated = Path.Combine(Path.GetTempPath(), $"vectorreel-truncated-{Guid.NewGuid():N}.mp4");
        var head = new byte[4096];
        await using (var source = File.OpenRead(Fixtures.SlideTalk))
        {
            _ = await source.ReadAsync(head, CancellationToken.None);
        }

        await File.WriteAllBytesAsync(truncated, head, CancellationToken.None);
        try
        {
            await Assert.ThrowsAsync<CorruptSourceException>(
                () => _probe.ProbeAsync(truncated, CancellationToken.None));
        }
        finally
        {
            File.Delete(truncated);
        }
    }
}
