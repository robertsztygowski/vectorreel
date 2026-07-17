using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MdReel.Core.Domain;
using MdReel.Core.Providers;

namespace MdReel.Infrastructure.Pipeline;

/// <summary>
/// Deterministic fixture keys for the record/replay harness. Keys deliberately depend ONLY on facts
/// that are stable across machines and ffmpeg builds — segment window and resolution for Stage B,
/// the sequence of segment starts + block counts for Stage C — never on volatile inputs like the
/// job id, the GCS URI, or scene-detected cue positions. That keeps a fixture recorded on one host
/// replayable in the e2e container built on another.
/// </summary>
internal static class FixtureKey
{
    public static string ForStageB(Segment segment)
    {
        var res = segment.Sampling.MediaResolution == MediaResolution.Low ? "low" : "def";
        var start = (int)Math.Round(segment.Start.TotalSeconds);
        var end = (int)Math.Round(segment.End.TotalSeconds);
        return $"stage-b/seg-{start:D6}-{end:D6}-{res}";
    }

    public static string ForStageC(IReadOnlyList<SegmentAnalysis> segments)
    {
        var canonical = new StringBuilder();
        foreach (var segment in segments)
        {
            canonical.Append(CultureInfo.InvariantCulture, $"{(int)Math.Round(segment.SegmentStart.TotalSeconds)}:{segment.Blocks.Count};");
        }

        return $"stage-c/fuse-{ShortHash(canonical.ToString())}";
    }

    private static string ShortHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }
}
