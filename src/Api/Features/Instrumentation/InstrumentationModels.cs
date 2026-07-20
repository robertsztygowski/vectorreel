using MdReel.Core.Domain;

namespace MdReel.Api.Features.Instrumentation;

public static class EventNames
{
    public static readonly ISet<string> Stable = new HashSet<string>(StringComparer.Ordinal)
    {
        "page_view",
        "signup_view",
        "signup",
        "upload_started",
        "job_completed",
        "output_downloaded",
        "upload_repeat",
        "checkout_clicked",
        "checkout_abandoned",
        "payment_succeeded",
    };
}

public sealed record EventDraft(
    string Name,
    string? SessionId,
    string? TenantId,
    string? UserId,
    DateTimeOffset OccurredAt,
    string? Referrer,
    string? AbArm,
    string PayloadJson);

public sealed record EventRecord(
    string Id,
    string Name,
    string? SessionId,
    string? TenantId,
    string? UserId,
    DateTimeOffset OccurredAt,
    string? Referrer,
    string? AbArm,
    string PayloadJson);

public sealed record TenantRecord(
    string Id,
    string Email,
    string? FirstUtmSource,
    string? FirstUtmMedium,
    string? FirstUtmCampaign,
    string? FirstUtmTerm,
    string? FirstReferrer,
    string? AbArm,
    double? ArchiveHours,
    double? MonthlyHours,
    double TrialCreditHours,
    string? Plan);

public sealed record UserRecord(string Id, string TenantId, string Email);

public sealed record SignupResult(TenantRecord Tenant, UserRecord User, bool TrialGranted);

public sealed record SignupDraft(
    string Email,
    double? ArchiveHours,
    double? MonthlyHours,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmTerm,
    string? FirstReferrer,
    string? AbArm);

public sealed record TenantHoursByWeek(string TenantId, int WeekIndex, double Hours);

public sealed record AdSpendEntry(
    string Id,
    DateOnly Day,
    string Channel,
    string? Campaign,
    string? Keyword,
    int CostCents,
    int? Clicks,
    int? Impressions);

public interface IEventStore
{
    Task<EventRecord> RecordAsync(EventDraft draft, CancellationToken cancellationToken);

    Task<IReadOnlyList<EventRecord>> ListAsync(CancellationToken cancellationToken);
}

public interface ITenantStore
{
    Task<SignupResult> UpsertSignupAsync(SignupDraft draft, CancellationToken cancellationToken);

    Task<TenantRecord?> GetAsync(string tenantId, CancellationToken cancellationToken);

    Task<TenantRecord?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task SetPlanAsync(string tenantId, string plan, CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantRecord>> ListAsync(CancellationToken cancellationToken);
}

public interface IAdSpendLedger
{
    Task<AdSpendEntry> RecordAsync(AdSpendEntry entry, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdSpendEntry>> ListAsync(CancellationToken cancellationToken);
}

public interface ICohortAnalytics
{
    Task<IReadOnlyList<TenantHoursByWeek>> GetTenantHoursByWeekAsync(CancellationToken cancellationToken);
}

public sealed class PostgresCostLedger(Npgsql.NpgsqlDataSource dataSource) : ICostLedger
{
    public void Record(CostEntry entry)
    {
        PostgresSchema.EnsureAsync(dataSource, CancellationToken.None).GetAwaiter().GetResult();
        using var command = dataSource.CreateCommand("""
            insert into usage_ledger (id, job_id, kind, step, quantity, unit, cents)
            values (@id, @job_id, @kind, @step, @quantity, @unit, @cents)
            """);
        command.Parameters.AddWithValue("id", $"use_{Guid.NewGuid():N}");
        command.Parameters.AddWithValue("job_id", entry.JobId);
        command.Parameters.AddWithValue("kind", entry.Kind.ToString().ToLowerInvariant());
        command.Parameters.AddWithValue("step", entry.Step);
        command.Parameters.AddWithValue("quantity", entry.Quantity);
        command.Parameters.AddWithValue("unit", entry.Unit);
        command.Parameters.AddWithValue("cents", (object?)entry.Cents ?? DBNull.Value);
        command.ExecuteNonQuery();
    }
}
