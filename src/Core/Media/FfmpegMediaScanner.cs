using System.Globalization;
using System.Text.RegularExpressions;
using MdReel.Core.Domain;
using MdReel.Core.Pipeline.StageA;
using MdReel.Core.Providers;

namespace MdReel.Core.Media;

/// <summary>
/// Extracts both of Stage A's signals from a source file in <b>one</b> ffmpeg pass: the downscaled
/// frame timeline on stdout, and the pauses in the narration on stderr.
///
/// The two signals answer different questions and neither substitutes for the other:
/// <list type="bullet">
///   <item><description><b>Stillness</b> (frames) routes <em>cost</em> — a frozen picture can be analysed at low media resolution.</description></item>
///   <item><description><b>Silence</b> (audio) bounds <em>blocks</em> — a boundary belongs where the narration pauses.</description></item>
/// </list>
/// Conflating them is the trap: on the ICP's own footage the screen is frozen for minutes while the
/// presenter keeps talking, so "nothing is moving" emphatically does not mean "nothing is happening".
/// </summary>
public sealed partial class FfmpegMediaScanner(MediaToolOptions options) : IMediaScanner
{
    /// <summary>Scan a source file that is known to have an audio track.</summary>
    public Task<MediaScan> ScanAsync(string path, CancellationToken cancellationToken) =>
        ScanAsync(path, hasAudioStream: true, cancellationToken);

    /// <inheritdoc />
    public async Task<MediaScan> ScanAsync(string path, bool hasAudioStream, CancellationToken cancellationToken)
    {
        var stageA = StageAOptions.Default;

        List<string> arguments =
        [
            "-v", "info",
            "-i", path,

            // Video: exactly the filter chain the Phase-0 experiment measured the cost lever with.
            // These four numbers are not free parameters — see StageAOptions.
            "-map", "0:v:0",
            "-vf", $"fps={MediaScan.SamplesPerSecond},scale={MediaScan.FrameWidth}:{MediaScan.FrameHeight}",
            "-pix_fmt", "gray",
            "-f", "rawvideo", "pipe:1",
        ];

        if (hasAudioStream)
        {
            arguments.AddRange([
                "-map", "0:a:0",
                "-af", FormattableString.Invariant(
                    $"silencedetect=noise={stageA.SilenceNoiseFloorDb}dB:d={stageA.SilenceMinDurationSeconds}"),
                "-f", "null", "-",
            ]);
        }

        var result = await ProcessRunner.RunAsync(options.FfmpegPath, arguments, cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new MediaToolException(options.FfmpegPath, result.ExitCode, ProcessRunner.Tail(result.StandardError));
        }

        var frames = ReadFrames(result.StandardOutput, path);
        var silences = ParseSilences(result.StandardError);

        return new MediaScan(frames, silences);
    }

    internal static List<GrayFrame> ReadFrames(byte[] stdout, string path)
    {
        if (stdout.Length % MediaScan.FrameBytes != 0)
        {
            throw new CorruptSourceException(
                $"ffmpeg produced a truncated frame stream for {Path.GetFileName(path)}: "
                + $"{stdout.Length} bytes is not a whole number of {MediaScan.FrameBytes}-byte frames");
        }

        var count = stdout.Length / MediaScan.FrameBytes;
        var frames = new List<GrayFrame>(count);
        for (var i = 0; i < count; i++)
        {
            frames.Add(new GrayFrame(i, stdout.AsSpan(i * MediaScan.FrameBytes, MediaScan.FrameBytes).ToArray()));
        }

        return frames;
    }

    /// <summary>
    /// Pull the silencedetect report out of ffmpeg's stderr. A trailing <c>silence_start</c> with no
    /// matching end means the video ends in silence — that interval is left open and closed by the caller.
    /// </summary>
    internal static List<SilenceInterval> ParseSilences(string stderr)
    {
        var silences = new List<SilenceInterval>();
        double? openStart = null;

        foreach (Match match in SilenceEvent().Matches(stderr))
        {
            var seconds = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            if (match.Groups[1].Value == "start")
            {
                openStart = seconds;
            }
            else if (openStart is { } start)
            {
                silences.Add(new SilenceInterval(TimeSpan.FromSeconds(start), TimeSpan.FromSeconds(seconds)));
                openStart = null;
            }
        }

        if (openStart is { } trailing)
        {
            silences.Add(new SilenceInterval(TimeSpan.FromSeconds(trailing), TimeSpan.MaxValue));
        }

        return silences;
    }

    [GeneratedRegex(@"silence_(start|end): (-?[\d.]+)")]
    private static partial Regex SilenceEvent();
}
