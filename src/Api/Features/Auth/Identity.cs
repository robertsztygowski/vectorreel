using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace MdReel.Api.Features.Auth;

/// <summary>
/// The authentication user. Extends the framework <see cref="IdentityUser"/> with the product
/// <see cref="TenantId"/> so a signed-in principal carries its tenant. EF Core owns ONLY the
/// identity tables; all product data (tenants, events, payments, …) stays raw Npgsql
/// (<c>PostgresSchema</c>), per the settled auth decision.
/// </summary>
public sealed class AppUser : IdentityUser
{
    public string? TenantId { get; set; }
}

public sealed class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<AppUser>(options);

/// <summary>
/// Adds the <c>tenant_id</c> claim to the signed-in principal so downstream code can read the
/// tenant without a database round-trip.
/// </summary>
public sealed class AppUserClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<AppUser>(userManager, optionsAccessor)
{
    public const string TenantClaimType = "tenant_id";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrWhiteSpace(user.TenantId))
        {
            identity.AddClaim(new Claim(TenantClaimType, user.TenantId));
        }

        return identity;
    }
}

/// <summary>
/// Lazily creates the EF-owned identity tables in the shared Postgres database, mirroring the
/// <c>PostgresSchema.EnsureAsync</c> pattern. Because the database already holds the raw-Npgsql
/// product tables, <c>EnsureCreated</c> would be a no-op, so on the relational provider we create
/// only the identity tables when <c>AspNetUsers</c> is absent (identity tables are always created
/// together, so that one probe is sufficient). On the EF InMemory provider (no-Postgres dev/test)
/// we simply <c>EnsureCreated</c>.
/// </summary>
public static class IdentitySchema
{
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static bool _ensured;

    public static async Task EnsureAsync(AppIdentityDbContext db, CancellationToken cancellationToken)
    {
        if (_ensured)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_ensured)
            {
                return;
            }

            if (db.Database.IsRelational())
            {
                var creator = (RelationalDatabaseCreator)db.GetService<IRelationalDatabaseCreator>();
                if (!await creator.ExistsAsync(cancellationToken))
                {
                    await creator.CreateAsync(cancellationToken);
                }

                if (!await AspNetUsersExistsAsync(db, cancellationToken))
                {
                    await creator.CreateTablesAsync(cancellationToken);
                }
            }
            else
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
            }

            _ensured = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Resets the once-per-process guard. Test-only hook for fresh throwaway databases.</summary>
    public static void ResetForTests() => _ensured = false;

    private static async Task<bool> AspNetUsersExistsAsync(AppIdentityDbContext db, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        var mustClose = connection.State != ConnectionState.Open;
        if (mustClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "select to_regclass('public.\"AspNetUsers\"') is not null";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is bool exists && exists;
        }
        finally
        {
            if (mustClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
