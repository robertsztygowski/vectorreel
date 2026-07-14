using VectorReel.Core.Domain;

namespace VectorReel.Tests.Unit.StageA;

/// <summary>
/// Synthesized frames. Every Stage A algorithm is a pure function over a frame sequence, so its
/// behaviour can be pinned without ffmpeg, without a video, and without a fixture — which means the
/// unit tests specify the algorithm rather than describing three particular clips.
/// </summary>
public static class Frames
{
    /// <summary>A frame of one uniform grey value.</summary>
    public static GrayFrame Flat(int second, byte value)
    {
        var pixels = new byte[MediaScan.FrameBytes];
        Array.Fill(pixels, value);
        return new GrayFrame(second, pixels);
    }

    /// <summary>A run of identical frames — a frozen picture.</summary>
    public static IEnumerable<GrayFrame> Frozen(int fromSecond, int count, byte value = 100) =>
        Enumerable.Range(fromSecond, count).Select(s => Flat(s, value));

    /// <summary>
    /// A frame where <paramref name="changedFraction"/> of the pixels sit at <paramref name="value"/>
    /// and the rest at <paramref name="background"/>. Lets a test dial in an exact mean-abs-diff.
    /// </summary>
    public static GrayFrame Partial(int second, double changedFraction, byte value, byte background = 100)
    {
        var pixels = new byte[MediaScan.FrameBytes];
        Array.Fill(pixels, background);
        var changed = (int)(MediaScan.FrameBytes * changedFraction);
        Array.Fill(pixels, value, 0, changed);
        return new GrayFrame(second, pixels);
    }
}
