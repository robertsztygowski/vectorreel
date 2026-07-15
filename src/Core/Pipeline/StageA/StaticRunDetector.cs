using MdReel.Core.Domain;

namespace MdReel.Core.Pipeline.StageA;

/// <summary>
/// Finds the stretches where the picture is frozen. This is the <b>cost</b> lever and nothing else
/// (ARCHITECTURE.md §8): a static stretch is analysed at low media resolution, which is ~45% cheaper
/// and loses nothing on content that is not moving.
///
/// 🔒 <b>This is a faithful port of the Phase-0 experiment, and its numbers are not free parameters.</b>
/// The measured cost blend (METRICS.md N4) assumes that ~two thirds of a real demo recording lands in
/// static runs, and that share is an output of exactly this algorithm at exactly these thresholds.
/// Change the metric, the threshold, the strictness of the comparison, or the denominator below, and
/// the fraction moves — which silently makes a load-bearing business number wrong. A calibration test
/// asserts the port still reproduces the original measurement.
///
/// ⚠️ It deliberately says nothing about where blocks begin. Stillness is not silence: the presenter
/// talks over a frozen screen for minutes at a time. Boundaries are <c>CueDetector</c>'s job.
/// </summary>
public static class StaticRunDetector
{
    /// <summary>
    /// Per-second motion, as the mean absolute difference to the previous sampled frame.
    /// Index <c>i</c> compares second <c>i+1</c> against second <c>i</c>, so this timeline is one
    /// shorter than the frame timeline — as in the original.
    /// </summary>
    public static double[] MotionTimeline(IReadOnlyList<GrayFrame> frames)
    {
        if (frames.Count < 2)
        {
            return [];
        }

        var motion = new double[frames.Count - 1];
        for (var i = 0; i < motion.Length; i++)
        {
            motion[i] = FrameMath.MeanAbsoluteDifference(frames[i + 1], frames[i]);
        }

        return motion;
    }

    /// <summary>Find the static runs in a scanned video.</summary>
    public static IReadOnlyList<StaticRun> Detect(IReadOnlyList<GrayFrame> frames, StageAOptions options) =>
        DetectFromMotion(MotionTimeline(frames), options);

    /// <summary>Find the static runs given a precomputed motion timeline.</summary>
    public static IReadOnlyList<StaticRun> DetectFromMotion(double[] motion, StageAOptions options)
    {
        var runs = new List<StaticRun>();
        int? start = null;

        for (var i = 0; i < motion.Length; i++)
        {
            // Strictly less than, as in the original: a second differing by exactly the threshold is
            // motion, not stillness.
            var isStatic = motion[i] < options.StaticMeanAbsDiffThreshold;

            if (isStatic && start is null)
            {
                start = i;
            }
            else if (!isStatic && start is { } open)
            {
                if (i - open >= options.StaticMinRunSeconds)
                {
                    runs.Add(new StaticRun(open, i));
                }

                start = null;
            }
        }

        // A run still open at the end of the video is closed here. Most recordings end on a frozen
        // slide, so dropping this would quietly lose the cheap tail of nearly every video.
        if (start is { } trailing && motion.Length - trailing >= options.StaticMinRunSeconds)
        {
            runs.Add(new StaticRun(trailing, motion.Length));
        }

        return runs;
    }

    /// <summary>
    /// Share of the video sitting inside a static run — the quantity METRICS.md N4's blend is built on.
    ///
    /// The denominator is the length of the <em>motion</em> timeline (one less than the frame count),
    /// which is what the original divides by. On a 50-minute video the difference from dividing by the
    /// frame count is 0.03%, but keeping it identical is the point: this number is only trustworthy
    /// because it reproduces a measurement, and it only reproduces it if the arithmetic matches.
    /// </summary>
    public static double StaticFraction(IReadOnlyList<StaticRun> runs, int frameCount)
    {
        var motionSamples = frameCount - 1;
        return motionSamples <= 0 ? 0 : runs.Sum(r => r.Seconds) / (double)motionSamples;
    }
}
