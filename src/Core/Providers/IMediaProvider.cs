using VectorReel.Core.Domain;

namespace VectorReel.Core.Providers;

/// <summary>
/// Reads a source file's shape without decoding it. Backed by ffprobe.
/// </summary>
public interface IMediaProbe
{
    /// <summary>
    /// Probe a local file. Throws <c>CorruptSourceException</c> if the file is not a video we can
    /// process — rejecting it early is cheaper than failing inside Stage B.
    /// </summary>
    Task<VideoProbe> ProbeAsync(string path, CancellationToken cancellationToken);
}

/// <summary>
/// The one place ffmpeg is invoked. Extracts both of Stage A's signals in a single pass: the
/// downscaled frame timeline (stdout) and the pauses in the narration (stderr).
///
/// This is a seam so that every algorithm above it is a pure function over frames and silences,
/// unit-testable with synthesized input and no ffmpeg anywhere near the test.
/// </summary>
public interface IMediaScanner
{
    /// <summary>
    /// Decode a source file down to the frames and silences Stage A reasons about.
    /// <paramref name="hasAudioStream"/> comes from the probe: asking ffmpeg to map an audio stream that
    /// does not exist makes it fail outright, and a video with no audio track is a case we actually have.
    /// </summary>
    Task<MediaScan> ScanAsync(string path, bool hasAudioStream, CancellationToken cancellationToken);
}
