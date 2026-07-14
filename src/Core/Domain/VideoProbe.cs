namespace VectorReel.Core.Domain;

/// <summary>
/// What ffprobe tells us about a source file before any analysis happens (ARCHITECTURE.md §3, Stage A step 1).
/// </summary>
/// <param name="Duration">Container duration.</param>
/// <param name="Width">Video width in pixels.</param>
/// <param name="Height">Video height in pixels.</param>
/// <param name="FrameRate">
/// Average frame rate. ffprobe reports this as a rational (<c>19001/317</c> on one of the committed
/// fixtures), so it is parsed as a fraction and kept as a double — never as an int.
/// </param>
/// <param name="VideoCodec">e.g. <c>h264</c>.</param>
/// <param name="AudioCodec">
/// <c>null</c> when the file carries no audio stream at all. An audio stream that is *present but
/// silent* is not the same thing and is not detectable here: it probes exactly like a normal one.
/// That case is real (an internal test asset has it) and it belongs to Stage B, which must return
/// no transcript rather than invent one.
/// </param>
public sealed record VideoProbe(
    TimeSpan Duration,
    int Width,
    int Height,
    double FrameRate,
    string VideoCodec,
    string? AudioCodec)
{
    /// <summary>True when the container has an audio stream — which says nothing about whether anyone speaks.</summary>
    public bool HasAudioStream => AudioCodec is not null;

    /// <summary>Whole seconds of content; the frame timeline is sampled at 1 fps, so this is its length.</summary>
    public int DurationSeconds => (int)Math.Floor(Duration.TotalSeconds);
}
