namespace MdReel.Core.Media;

/// <summary>Where the ffmpeg binaries are. Both variables appear in <c>.env.example</c> (DEVELOPMENT.md §6).</summary>
public sealed record MediaToolOptions
{
    /// <summary>Path to the ffmpeg binary, or just <c>ffmpeg</c> to find it on PATH.</summary>
    public string FfmpegPath { get; init; } = "ffmpeg";

    /// <summary>Path to the ffprobe binary, or just <c>ffprobe</c> to find it on PATH.</summary>
    public string FfprobePath { get; init; } = "ffprobe";
}
