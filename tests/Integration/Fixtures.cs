namespace MdReel.Tests.Integration;

/// <summary>
/// The three committed CC clips (tests/fixtures/videos/README.md). They are the golden inputs for
/// Stage A: 90 seconds each, one per content category, and reproducible by anyone who clones the repo.
/// </summary>
public static class Fixtures
{
    /// <summary>Repo root, found by walking up from the test binary until the solution file appears.</summary>
    public static string RepoRoot { get; } = FindRepoRoot();

    /// <summary>Directory holding the committed sample videos.</summary>
    public static string VideoDirectory => Path.Combine(RepoRoot, "tests", "fixtures", "videos");

    /// <summary>Slide-heavy conference talk — dense projected text. The OCR case.</summary>
    public static string SlideTalk => Path.Combine(VideoDirectory, "slide_talk_fosdem_curl.mp4");

    /// <summary>Talking head, essentially no on-screen text. The anti-hallucination case. 59.94 fps.</summary>
    public static string TalkingHead => Path.Combine(VideoDirectory, "talking_head_nasa_bolten.mp4");

    /// <summary>Screen recording — the public analogue of the ICP's own content, and the weak category.</summary>
    public static string Screencast => Path.Combine(VideoDirectory, "screencast_blender_lesson.mp4");

    /// <summary>All three, for theories that should hold on every category.</summary>
    public static TheoryData<string> All => new(SlideTalk, TalkingHead, Screencast);

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "MdReel.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("could not locate the repo root from the test binary");
    }
}
