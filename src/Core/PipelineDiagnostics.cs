using System.Diagnostics;

namespace MdReel.Core;

/// <summary>
/// The one ActivitySource for pipeline work (DEVELOPMENT.md §3: OTel traces across
/// API → queue → worker). Stage runners and job processors start activities here so a
/// trace can be found by jobId in the local dashboard (TESTING.md runbook).
/// </summary>
public static class PipelineDiagnostics
{
    public const string SourceName = "MdReel.Pipeline";

    public static readonly ActivitySource Source = new(SourceName);
}
