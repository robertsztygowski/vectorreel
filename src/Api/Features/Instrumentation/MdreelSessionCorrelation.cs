using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace MdReel.Api.Features.Instrumentation;

public static class MdreelSessionCorrelation
{
    public const string HeaderName = "X-Mdreel-Session";
    public const string ActivityAttributeName = "mdreel.session_id";

    public static string? GetValidSessionId(IHeaderDictionary headers)
    {
        return headers.TryGetValue(HeaderName, out var values) && values.Count == 1
            ? Sanitize(values)
            : null;
    }

    public static IApplicationBuilder UseMdreelSessionCorrelation(this IApplicationBuilder app) =>
        app.Use((context, next) =>
        {
            var sessionId = GetValidSessionId(context.Request.Headers);
            if (sessionId is not null)
            {
                Activity.Current?.SetTag(ActivityAttributeName, sessionId);
            }

            return next(context);
        });

    private static string? Sanitize(StringValues values)
    {
        var value = values.ToString();
        if (value.Length is 0 or > 64)
        {
            return null;
        }

        foreach (var c in value)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_')
            {
                return null;
            }
        }

        return value;
    }
}
