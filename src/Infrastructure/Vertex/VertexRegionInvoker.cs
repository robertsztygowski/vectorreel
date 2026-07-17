using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace MdReel.Infrastructure.Vertex;

/// <summary>The parsed model response together with the EU region that actually served it.</summary>
internal readonly record struct VertexRegionResponse(GenerateContentResponse? Body, string Region);

/// <summary>
/// Sends a Vertex <c>generateContent</c> call with a region fallback. Stage B→C back-to-back trips
/// <c>429 RESOURCE_EXHAUSTED</c> on the primary EU region under load (INFRA.md); this retries the
/// primary, then falls back to the only other EU region that serves <c>gemini-2.5-flash</c>
/// (<c>europe-west3</c>, ARCHITECTURE §2). <b>EU regions only</b> (CLAUDE.md rule 2) — the region set
/// is exactly <see cref="VertexOptions.Region"/> then <see cref="VertexOptions.FallbackRegion"/>,
/// both EU. Non-429 failures are surfaced immediately without a fallback.
/// </summary>
internal static class VertexRegionInvoker
{
    public static async Task<VertexRegionResponse> SendAsync(
        HttpClient httpClient,
        VertexOptions options,
        IAccessTokenProvider tokenProvider,
        Func<string, string> urlForRegion,
        GenerateContentRequest body,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        var regions = ResolveRegions(options);

        HttpStatusCode lastStatus = HttpStatusCode.TooManyRequests;
        for (var r = 0; r < regions.Count; r++)
        {
            var region = regions[r];
            var url = urlForRegion(region);

            for (var attempt = 0; attempt <= options.MaxRetriesPerRegion; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var message = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(body),
                };
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = await httpClient.SendAsync(message, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var parsed = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(cancellationToken);
                    return new VertexRegionResponse(parsed, region);
                }

                if (response.StatusCode != HttpStatusCode.TooManyRequests)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"Vertex generateContent returned {(int)response.StatusCode}: {Truncate(errorBody, 800)}");
                }

                lastStatus = response.StatusCode;
                var retryAfter = response.Headers.RetryAfter?.Delta ?? options.RetryDelay;
                var moreAttemptsHere = attempt < options.MaxRetriesPerRegion;
                var moreRegions = r < regions.Count - 1;

                logger.LogWarning(
                    "Vertex 429 RESOURCE_EXHAUSTED in {Region} (attempt {Attempt}/{Max}); {Next}",
                    region,
                    attempt + 1,
                    options.MaxRetriesPerRegion + 1,
                    moreAttemptsHere ? "retrying same region" : moreRegions ? "falling back to next EU region" : "no regions left");

                if (!moreAttemptsHere)
                {
                    break;
                }

                if (retryAfter > TimeSpan.Zero)
                {
                    await Task.Delay(retryAfter, cancellationToken);
                }
            }
        }

        throw new HttpRequestException(
            $"Vertex generateContent returned {(int)lastStatus} RESOURCE_EXHAUSTED in all EU regions "
            + $"({string.Join(", ", regions)}).");
    }

    // Primary then fallback, de-duplicated. Both are EU (CLAUDE.md rule 2); the fallback is only
    // added when it is set and distinct.
    private static List<string> ResolveRegions(VertexOptions options)
    {
        var regions = new List<string> { options.Region };
        if (!string.IsNullOrWhiteSpace(options.FallbackRegion)
            && !string.Equals(options.FallbackRegion, options.Region, StringComparison.OrdinalIgnoreCase))
        {
            regions.Add(options.FallbackRegion);
        }

        return regions;
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
