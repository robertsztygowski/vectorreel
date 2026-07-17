using System.Text.Json;

namespace MdReel.Infrastructure.Pipeline;

/// <summary>
/// Reads and writes JSON fixtures under a root directory, one file per key
/// (<c>&lt;root&gt;/&lt;key&gt;.json</c>). Backs the record/replay harness.
/// </summary>
public sealed class FixtureStore(string rootDirectory)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    public string RootDirectory { get; } = rootDirectory;

    public bool TryRead<T>(string key, out T? value)
    {
        var path = PathFor(key);
        if (!File.Exists(path))
        {
            value = default;
            return false;
        }

        using var stream = File.OpenRead(path);
        value = JsonSerializer.Deserialize<T>(stream, Json);
        return value is not null;
    }

    public T Read<T>(string key)
    {
        if (!TryRead<T>(key, out var value) || value is null)
        {
            throw new FileNotFoundException(
                $"Missing LLM fixture '{key}'. Record it with PipelineModel:Mode=Record (see TESTING.md), "
                + $"expected at {PathFor(key)}.");
        }

        return value;
    }

    public void Write<T>(string key, T value)
    {
        var path = PathFor(key);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, value, Json);
    }

    private string PathFor(string key)
    {
        var relative = key.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(RootDirectory, relative + ".json");
    }
}
