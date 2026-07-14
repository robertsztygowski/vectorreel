using VectorReel.Core.Domain;

namespace VectorReel.Core.Pipeline.StageA;

/// <summary>Pixel arithmetic. Pure, hot, and the base of both Stage A signals.</summary>
public static class FrameMath
{
    /// <summary>
    /// Mean absolute difference between two frames, on the 0–255 scale.
    ///
    /// This one function is used against two different reference frames, and the choice of reference
    /// is the whole idea:
    /// <list type="bullet">
    ///   <item><description>against the <b>previous</b> frame it asks <i>is the picture moving?</i> — which routes cost;</description></item>
    ///   <item><description>against the <b>anchor</b> frame it asks <i>has this become a different scene?</i> — which places boundaries.</description></item>
    /// </list>
    /// A frame-to-frame comparison can never answer the second question on a screen recording: typing
    /// and scrolling move too little from one second to the next to ever cross a threshold, while
    /// drifting arbitrarily far from where the scene started.
    /// </summary>
    public static double MeanAbsoluteDifference(GrayFrame a, GrayFrame b)
    {
        if (a.Pixels.Length != b.Pixels.Length)
        {
            throw new ArgumentException(
                $"frames differ in size ({a.Pixels.Length} vs {b.Pixels.Length}) — they are not from the same scan");
        }

        long total = 0;
        var left = a.Pixels;
        var right = b.Pixels;
        for (var i = 0; i < left.Length; i++)
        {
            total += Math.Abs(left[i] - right[i]);
        }

        return (double)total / left.Length;
    }
}
