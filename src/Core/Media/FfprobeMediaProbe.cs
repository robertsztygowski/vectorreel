using System.Globalization;
using System.Text.Json;
using MdReel.Core.Domain;
using MdReel.Core.Providers;

namespace MdReel.Core.Media;

/// <summary>
/// Stage A step 1 (ARCHITECTURE.md §3): find out what the file is, and reject it here if we can't
/// process it. Every rejection at this point is a Vertex call we never paid for.
/// </summary>
public sealed class FfprobeMediaProbe(MediaToolOptions options) : IMediaProbe
{
    /// <inheritdoc />
    public async Task<VideoProbe> ProbeAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            throw new CorruptSourceException($"source file does not exist: {path}");
        }

        string[] arguments =
        [
            "-v", "error",
            "-show_entries", "format=duration",
            "-show_entries", "stream=codec_type,codec_name,width,height,avg_frame_rate",
            "-of", "json",
            path,
        ];

        var result = await ProcessRunner.RunAsync(options.FfprobePath, arguments, cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new CorruptSourceException(
                $"ffprobe rejected {Path.GetFileName(path)}: {ProcessRunner.Tail(result.StandardError, 500)}");
        }

        return Parse(path, System.Text.Encoding.UTF8.GetString(result.StandardOutput));
    }

    // Kept internal-but-static so the JSON shapes ffprobe actually emits can be pinned in unit
    // tests without running the binary.
    internal static VideoProbe Parse(string path, string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("streams", out var streams) || streams.GetArrayLength() == 0)
        {
            throw new CorruptSourceException($"no streams in {Path.GetFileName(path)} — not a media file");
        }

        JsonElement? video = null;
        JsonElement? audio = null;
        foreach (var stream in streams.EnumerateArray())
        {
            var kind = stream.TryGetProperty("codec_type", out var t) ? t.GetString() : null;
            if (kind == "video" && video is null)
            {
                video = stream;
            }
            else if (kind == "audio" && audio is null)
            {
                audio = stream;
            }
        }

        if (video is not { } videoStream)
        {
            throw new CorruptSourceException($"no video stream in {Path.GetFileName(path)}");
        }

        if (!root.TryGetProperty("format", out var format)
            || !format.TryGetProperty("duration", out var durationText)
            || !double.TryParse(durationText.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            || seconds <= 0)
        {
            throw new CorruptSourceException($"no usable duration in {Path.GetFileName(path)}");
        }

        return new VideoProbe(
            Duration: TimeSpan.FromSeconds(seconds),
            Width: videoStream.GetProperty("width").GetInt32(),
            Height: videoStream.GetProperty("height").GetInt32(),
            FrameRate: ParseFrameRate(videoStream),
            VideoCodec: videoStream.GetProperty("codec_name").GetString() ?? "unknown",
            AudioCodec: audio?.GetProperty("codec_name").GetString());
    }

    /// <summary>
    /// ffprobe reports frame rate as a rational string. It is <em>not</em> always a whole number:
    /// one of the committed fixtures is <c>19001/317</c> (≈59.94 fps), so parsing this as an int
    /// throws away the file, and parsing it as a decimal fails outright.
    /// </summary>
    internal static double ParseFrameRate(JsonElement stream)
    {
        var raw = stream.TryGetProperty("avg_frame_rate", out var value) ? value.GetString() : null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0;
        }

        var parts = raw.Split('/');
        if (parts.Length != 2
            || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var numerator)
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var denominator)
            || denominator == 0)
        {
            return 0;
        }

        return numerator / denominator;
    }
}
