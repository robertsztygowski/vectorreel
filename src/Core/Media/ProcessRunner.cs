using System.Diagnostics;

namespace MdReel.Core.Media;

/// <summary>What a finished child process produced.</summary>
public sealed record ProcessResult(int ExitCode, byte[] StandardOutput, string StandardError);
public sealed record ProcessFileResult(int ExitCode, string StandardError);

/// <summary>
/// The only place in the codebase that starts a process. ffmpeg and ffprobe are the sole callers.
/// </summary>
public static class ProcessRunner
{
    /// <summary>
    /// Run a child process to completion, capturing stdout as bytes and stderr as text.
    ///
    /// 🚨 <b>stdout and stderr are drained concurrently, and that is load-bearing.</b> The scan
    /// invocation writes raw frames to stdout <em>and</em> a silence report to stderr from the same
    /// process. A pipe buffer is only a few KB, so reading one to completion before touching the
    /// other deadlocks ffmpeg the moment the unread pipe fills — on a long video, always.
    /// </summary>
    public static async Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            throw new MediaToolException(fileName, -1, $"could not start '{fileName}': {ex.Message}");
        }

        // Kill the whole tree on cancellation: ffmpeg ignores a closed stdout and would otherwise
        // keep decoding a 50-minute video long after the job that wanted it has gone away.
        await using var kill = cancellationToken.Register(static state =>
        {
            try
            {
                ((Process)state!).Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // Already exited — nothing to kill.
            }
        }, process);

        using var stdout = new MemoryStream();
        var stdoutTask = process.StandardOutput.BaseStream.CopyToAsync(stdout, cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await Task.WhenAll(stdoutTask, stderrTask);
        await process.WaitForExitAsync(cancellationToken);

        return new ProcessResult(process.ExitCode, stdout.ToArray(), await stderrTask);
    }

    /// <summary>
    /// Run a child process to completion, streaming stdout to <paramref name="outputPath"/> and
    /// capturing stderr as text.
    /// </summary>
    public static async Task<ProcessFileResult> RunToFileAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var startInfo = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            throw new MediaToolException(fileName, -1, $"could not start '{fileName}': {ex.Message}");
        }

        await using var kill = cancellationToken.Register(static state =>
        {
            try
            {
                ((Process)state!).Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // Already exited — nothing to kill.
            }
        }, process);

        await using var stdout = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        var stdoutTask = process.StandardOutput.BaseStream.CopyToAsync(stdout, cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await Task.WhenAll(stdoutTask, stderrTask);
        await process.WaitForExitAsync(cancellationToken);

        return new ProcessFileResult(process.ExitCode, await stderrTask);
    }

    /// <summary>The last <paramref name="maxChars"/> of stderr — where the reason for a failure actually is.</summary>
    public static string Tail(string text, int maxChars = 2000) =>
        text.Length <= maxChars ? text : text[^maxChars..];
}
