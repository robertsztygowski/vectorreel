namespace MdReel.Core.Pipeline.StageB;

/// <summary>Normalize mixed timestamp formats per timestamp (ARCHITECTURE.md §3).</summary>
public static class StageBTimestampNormalizer
{
    public static IReadOnlyList<TimeSpan> Normalize(
        IReadOnlyList<string> timestamps,
        TimeSpan? clipDuration)
    {
        var normalized = new List<TimeSpan>(timestamps.Count);
        var previous = TimeSpan.Zero;
        var limit = clipDuration is { } d ? TimeSpan.FromSeconds(d.TotalSeconds * 1.05) : TimeSpan.MaxValue;

        foreach (var timestamp in timestamps)
        {
            var candidates = ParseCandidates(timestamp);
            if (candidates.Count == 0)
            {
                candidates.Add(ParseAsHhMmSs(timestamp));
            }

            var chosen = candidates
                .Where(candidate => candidate >= previous && candidate <= limit)
                .Cast<TimeSpan?>()
                .FirstOrDefault();

            if (chosen is null)
            {
                chosen = candidates.OrderBy(candidate => (candidate - previous).Duration()).FirstOrDefault();
            }

            normalized.Add(chosen ?? TimeSpan.Zero);
            previous = chosen ?? TimeSpan.Zero;
        }

        return normalized;
    }

    private static List<TimeSpan> ParseCandidates(string timestamp)
    {
        var fields = timestamp.Trim().Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length is < 2 or > 3)
        {
            return [];
        }

        if (!fields.All(field => int.TryParse(field, out _)))
        {
            return [];
        }

        var numbers = fields.Select(int.Parse).ToArray();
        List<TimeSpan> candidates = [];

        if (numbers.Skip(1).All(value => value <= 59))
        {
            candidates.Add(ParseAsHhMmSs(timestamp));
        }

        if (numbers.Length == 3)
        {
            // mm:ss:centiseconds -> drop centiseconds.
            candidates.Add(TimeSpan.FromSeconds((numbers[0] * 60) + numbers[1]));
        }

        if (numbers.Length == 2 && numbers[1] <= 59)
        {
            candidates.Add(TimeSpan.FromSeconds((numbers[0] * 60) + numbers[1]));
        }

        return [.. candidates.Distinct()];
    }

    private static TimeSpan ParseAsHhMmSs(string timestamp)
    {
        var numbers = timestamp
            .Trim()
            .Split(':', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => int.TryParse(part, out var parsed) ? parsed : 0)
            .ToList();

        while (numbers.Count < 3)
        {
            numbers.Insert(0, 0);
        }

        return TimeSpan.FromSeconds((numbers[0] * 3600) + (numbers[1] * 60) + numbers[2]);
    }
}
