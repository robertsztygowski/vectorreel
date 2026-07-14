namespace VectorReel.Core.Domain;

/// <summary>
/// A grayscale frame sampled from the source video, downscaled hard. One frame per second.
/// </summary>
/// <param name="Second">Second offset from the start of the video; also the frame's index.</param>
/// <param name="Pixels">Row-major 8-bit gray, <see cref="MediaScan.FrameWidth"/> × <see cref="MediaScan.FrameHeight"/>.</param>
public sealed record GrayFrame(int Second, byte[] Pixels);

/// <summary>
/// A stretch of the audio track quiet enough to count as a pause in the narration.
///
/// This is the signal that decides where blocks may begin. Stage A's other signal — stillness —
/// says nothing about it: on the ICP's own footage the screen is frozen for minutes at a time
/// while the presenter keeps talking.
/// </summary>
public sealed record SilenceInterval(TimeSpan Start, TimeSpan End)
{
    /// <summary>How long the quiet lasts.</summary>
    public TimeSpan Duration => End - Start;

    /// <summary>The midpoint — where a block boundary is snapped to, so it lands between sentences.</summary>
    public TimeSpan Midpoint => Start + (Duration / 2);

    /// <summary>True when <paramref name="at"/> falls inside this pause.</summary>
    public bool Contains(TimeSpan at) => at >= Start && at <= End;
}

/// <summary>
/// Everything one ffmpeg pass extracts from a source file: the downscaled frame timeline and the
/// pauses in the narration. Both modalities come from a single invocation — see
/// <c>Media.FfmpegMediaScanner</c>.
/// </summary>
public sealed record MediaScan(IReadOnlyList<GrayFrame> Frames, IReadOnlyList<SilenceInterval> Silences)
{
    /// <summary>Analysis frame width. Identical to the Phase-0 experiment — see <c>StageAOptions</c>.</summary>
    public const int FrameWidth = 160;

    /// <summary>Analysis frame height. Identical to the Phase-0 experiment.</summary>
    public const int FrameHeight = 90;

    /// <summary>Bytes in one analysis frame.</summary>
    public const int FrameBytes = FrameWidth * FrameHeight;

    /// <summary>Number of sampled seconds.</summary>
    public int SampledSeconds => Frames.Count;
}
