using System.Security.Claims;
using MdReel.Api.Features.Instrumentation;

namespace MdReel.Api.Features.Admin;

public static class AdminEndpoints
{
    public static void MapAdmin(this IEndpointRouteBuilder routes)
    {
        var admin = routes.MapGroup("/admin");

        admin.MapGet("/overview", async (
            HttpContext httpContext,
            IConfiguration configuration,
            IServiceProvider services,
            ITenantStore tenants,
            CancellationToken cancellationToken) =>
        {
            if (!await IsAdminAsync(httpContext, configuration, tenants, cancellationToken))
            {
                return Results.NotFound();
            }

            var overview = services.GetService<IAdminOverviewStore>();
            if (overview is null)
            {
                return Results.Problem(
                    title: "Admin overview unavailable",
                    detail: "Postgres is required for the admin overview.",
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    type: "about:blank");
            }

            return Results.Json(await overview.GetOverviewAsync(cancellationToken));
        });

        admin.MapPost("/ad-spend", async (
            HttpContext httpContext,
            IConfiguration configuration,
            ITenantStore tenants,
            IAdSpendLedger ledger,
            AdminAdSpendRequest request,
            CancellationToken cancellationToken) =>
        {
            if (!await IsAdminAsync(httpContext, configuration, tenants, cancellationToken))
            {
                return Results.NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.Source)
                || string.IsNullOrWhiteSpace(request.Campaign)
                || request.AmountCents <= 0
                || !string.Equals(request.Currency, "EUR", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Problem(
                    title: "Invalid ad spend",
                    detail: "source, campaign, positive amount_cents and currency EUR are required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "about:blank");
            }

            var stored = await ledger.RecordAsync(new AdSpendEntry(
                string.Empty,
                request.SpentOn,
                request.Source.Trim(),
                request.Campaign.Trim(),
                null,
                request.AmountCents,
                null,
                null), cancellationToken);

            return Results.Json(new
            {
                id = stored.Id,
                source = stored.Channel,
                campaign = stored.Campaign,
                amount_cents = stored.CostCents,
                currency = "EUR",
                spent_on = stored.Day,
            }, statusCode: StatusCodes.Status201Created);
        });
    }

    private static async Task<bool> IsAdminAsync(
        HttpContext httpContext,
        IConfiguration configuration,
        ITenantStore tenants,
        CancellationToken cancellationToken)
    {
        var allowlist = ParseAllowlist(configuration["Admin:Emails"] ?? configuration["Admin__Emails"]);
        if (allowlist.Count == 0)
        {
            return false;
        }

        var email = await ResolveEmailAsync(httpContext, tenants, cancellationToken);
        return email is not null && allowlist.Contains(email);
    }

    private static HashSet<string> ParseAllowlist(string? raw) =>
        (raw ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static async Task<string?> ResolveEmailAsync(
        HttpContext httpContext,
        ITenantStore tenants,
        CancellationToken cancellationToken)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var claimEmail = httpContext.User.FindFirstValue(ClaimTypes.Email)
                ?? httpContext.User.FindFirstValue("email")
                ?? httpContext.User.Identity.Name;
            if (!string.IsNullOrWhiteSpace(claimEmail))
            {
                return claimEmail.Trim();
            }
        }

        var tenantId = ReadBearer(httpContext);
        if (tenantId is null)
        {
            return null;
        }

        var tenant = await tenants.GetAsync(tenantId, cancellationToken);
        return tenant?.Email;
    }

    private static string? ReadBearer(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var values))
        {
            return null;
        }

        var raw = values.ToString();
        const string prefix = "Bearer ";
        return raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? raw[prefix.Length..].Trim()
            : null;
    }
}
