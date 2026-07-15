namespace MdReel.Core.Media;

/// <summary>
/// The source file is not something we can process — not a video, truncated, or unreadable.
///
/// Stage A rejects these at the probe (ARCHITECTURE.md §3, step 1). Failing here is cheap; failing
/// inside Stage B means we have already paid Vertex for the privilege.
/// </summary>
public sealed class CorruptSourceException(string message) : Exception(message);

/// <summary>An ffmpeg or ffprobe invocation failed. Carries the tail of stderr, which is where the reason is.</summary>
public sealed class MediaToolException(string tool, int exitCode, string stderrTail)
    : Exception($"{tool} exited with code {exitCode}: {stderrTail}")
{
    /// <summary>Which binary failed.</summary>
    public string Tool { get; } = tool;

    /// <summary>Its exit code.</summary>
    public int ExitCode { get; } = exitCode;
}
