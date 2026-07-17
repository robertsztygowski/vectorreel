using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MdReel.Api.Features.Instrumentation;
using Microsoft.AspNetCore.Identity;

namespace MdReel.Api.Features.Auth;

/// <summary>
/// Credentials for our tenant-provisioning registration endpoint. The optional survey fields feed
/// the same first-party instrumentation the anonymous <c>signup</c> event captures (N20 archive/
/// monthly hours + first-touch attribution).
/// </summary>
public sealed record SignupCredentials(
    [property: EmailAddress] string Email,
    string Password,
    double? ArchiveHours,
    double? MonthlyHours,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmTerm,
    string? FirstReferrer,
    string? AbArm);

public static class AuthEndpoints
{
    public const string RateLimitPolicy = "auth";

    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/auth").RequireRateLimiting(RateLimitPolicy);

        // Framework surface: /login (cookie mode via ?useCookies=true), /refresh, /manage/*,
        // /forgotPassword, /resetPassword, /confirmEmail. Its bare /register is intentionally
        // NOT used by our UI — registration goes through /signup below so tenant creation stays a
        // single source of truth (ITenantStore); a user created any other way is backfilled on /me.
        group.MapIdentityApi<AppUser>();

        // Registration WITH tenant provisioning + N33 trial credit (METRICS.md N33), reusing the
        // exact ITenantStore.UpsertSignupAsync path the anonymous signup event uses.
        group.MapPost("/signup", async (
            SignupCredentials body,
            UserManager<AppUser> users,
            SignInManager<AppUser> signInManager,
            ITenantStore tenants,
            AppIdentityDbContext db,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["credentials"] = ["Email and password are required."],
                });
            }

            await IdentitySchema.EnsureAsync(db, cancellationToken);

            var email = body.Email.Trim();
            var user = new AppUser { UserName = email, Email = email };
            var created = await users.CreateAsync(user, body.Password);
            if (!created.Succeeded)
            {
                return Results.ValidationProblem(created.Errors.ToDictionary(
                    static e => e.Code,
                    static e => new[] { e.Description }));
            }

            var signup = await tenants.UpsertSignupAsync(new SignupDraft(
                email,
                body.ArchiveHours,
                body.MonthlyHours,
                body.UtmSource,
                body.UtmMedium,
                body.UtmCampaign,
                body.UtmTerm,
                body.FirstReferrer,
                body.AbArm), cancellationToken);

            user.TenantId = signup.Tenant.Id;
            await users.UpdateAsync(user);

            await signInManager.SignInAsync(user, isPersistent: true);

            return Results.Ok(new
            {
                email = user.Email,
                tenant_id = signup.Tenant.Id,
                trial_credit_hours = signup.Tenant.TrialCreditHours,
            });
        });

        group.MapPost("/logout", async (SignInManager<AppUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok(new { status = "signed_out" });
        }).RequireAuthorization();

        // Session probe for the web nav/dashboard. Backfills the tenant for any user that reached
        // an authenticated state without one (e.g. via the framework /register), keeping tenant
        // creation flowing through ITenantStore.
        group.MapGet("/me", async (
            ClaimsPrincipal principal,
            UserManager<AppUser> users,
            ITenantStore tenants,
            AppIdentityDbContext db,
            CancellationToken cancellationToken) =>
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var user = await users.GetUserAsync(principal);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(user.TenantId) && !string.IsNullOrWhiteSpace(user.Email))
            {
                await IdentitySchema.EnsureAsync(db, cancellationToken);
                var signup = await tenants.UpsertSignupAsync(
                    new SignupDraft(user.Email, null, null, null, null, null, null, null, null),
                    cancellationToken);
                user.TenantId = signup.Tenant.Id;
                await users.UpdateAsync(user);
            }

            return Results.Ok(new { email = user.Email, tenant_id = user.TenantId });
        }).RequireAuthorization();

        return routes;
    }
}
