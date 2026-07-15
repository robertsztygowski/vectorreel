using System.Diagnostics;
using MdReel.Core.Domain;
using MdReel.Core.Media;

namespace MdReel.Tests.Integration;

/// <summary>Real ffmpeg against the committed fixtures. No cloud, no spend.</summary>
public sealed class MediaScannerTests
{
    private readonly FfmpegMediaScanner _scanner = new(new MediaToolOptions());

    [Theory]
    [MemberData(nameof(Fixtures.All), MemberType = typeof(Fixtures))]
    public async Task Yields_one_frame_per_second_of_video(string path)
    {
        var scan = await _scanner.ScanAsync(path, CancellationToken.None);

        Assert.Equal(90, scan.SampledSeconds);
        Assert.All(scan.Frames, f => Assert.Equal(MediaScan.FrameBytes, f.Pixels.Length));
        Assert.Equal(Enumerable.Range(0, 90), scan.Frames.Select(f => f.Second));
    }

    // 🚨 The regression this file exists for. ffmpeg writes frames to stdout and the silence report to
    // stderr from the SAME process. Draining one before the other fills the unread pipe (a few KB) and
    // ffmpeg blocks forever. It does not reproduce on a two-second clip — it reproduces on real videos,
    // in production. A 90-second fixture already writes enough to stderr to catch it.
    [Theory]
    [MemberData(nameof(Fixtures.All), MemberType = typeof(Fixtures))]
    public async Task Does_not_deadlock_when_both_pipes_carry_data(string path)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        var scan = await _scanner.ScanAsync(path, timeout.Token);

        Assert.NotEmpty(scan.Frames);
        Assert.NotEmpty(scan.Silences);
    }

    [Fact]
    public async Task Finds_the_pauses_in_the_narration()
    {
        // Silence is what bounds blocks — stillness only routes cost. If this signal ever goes empty,
        // forced boundaries stop landing between sentences and start landing on arbitrary ticks.
        var scan = await _scanner.ScanAsync(Fixtures.Screencast, CancellationToken.None);

        Assert.NotEmpty(scan.Silences);
        Assert.All(scan.Silences, s => Assert.True(s.End > s.Start, $"silence {s} is not ordered"));
    }

    [Fact]
    public async Task Scans_a_video_with_no_audio_stream()
    {
        // Mapping 0:a:0 on a file with no audio makes ffmpeg fail outright, so the scanner must be
        // told. A video with no audio track is a real case, and it must still yield a frame timeline.
        var silent = Path.Combine(Path.GetTempPath(), $"vectorreel-silent-{Guid.NewGuid():N}.mp4");
        await StripAudio(Fixtures.SlideTalk, silent);
        try
        {
            var scan = await _scanner.ScanAsync(silent, hasAudioStream: false, CancellationToken.None);

            Assert.Equal(90, scan.SampledSeconds);
            Assert.Empty(scan.Silences);
        }
        finally
        {
            File.Delete(silent);
        }
    }

    [Fact]
    public async Task Cancellation_kills_ffmpeg_promptly()
    {
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        var stopwatch = Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _scanner.ScanAsync(Fixtures.SlideTalk, cancelled.Token));

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"cancellation took {stopwatch.Elapsed}");
    }

    private static async Task StripAudio(string source, string destination)
    {
        var result = await ProcessRunner.RunAsync(
            "ffmpeg",
            ["-y", "-v", "error", "-i", source, "-map", "0:v:0", "-c", "copy", destination],
            CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
    }
}
