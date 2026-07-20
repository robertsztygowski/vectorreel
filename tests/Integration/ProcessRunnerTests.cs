using System.Diagnostics;
using MdReel.Core.Media;

namespace MdReel.Tests.Integration;

public sealed class ProcessRunnerTests
{
    [Fact]
    public async Task RunToFileAsync_writes_stdout_to_target_file()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"mdreel-processrunner-{Guid.NewGuid():N}.bin");
        try
        {
            var result = await ProcessRunner.RunToFileAsync(
                "ffmpeg",
                ["-v", "error", "-f", "lavfi", "-i", "anullsrc=r=16000:cl=mono", "-t", "1", "-f", "wav", "pipe:1"],
                outputPath,
                CancellationToken.None);

            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
            Assert.True(string.IsNullOrWhiteSpace(result.StandardError));
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task RunToFileAsync_returns_exit_code_and_stderr_for_failure()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"mdreel-processrunner-{Guid.NewGuid():N}.bin");
        try
        {
            var result = await ProcessRunner.RunToFileAsync(
                "ffmpeg",
                ["-v", "error", "-i", "definitely-missing-input-file.mp4", "-f", "null", "-"],
                outputPath,
                CancellationToken.None);

            Assert.NotEqual(0, result.ExitCode);
            Assert.False(string.IsNullOrWhiteSpace(result.StandardError));
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task RunToFileAsync_cancellation_kills_process_promptly()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"mdreel-processrunner-{Guid.NewGuid():N}.bin");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        var stopwatch = Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ProcessRunner.RunToFileAsync(
                "ffmpeg",
                ["-v", "error", "-f", "lavfi", "-i", "anullsrc=r=16000:cl=mono", "-f", "wav", "pipe:1"],
                outputPath,
                cancellation.Token));

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(30), $"cancellation took {stopwatch.Elapsed}");
        File.Delete(outputPath);
    }
}
