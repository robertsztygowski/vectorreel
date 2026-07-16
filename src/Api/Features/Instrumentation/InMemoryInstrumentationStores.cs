using System.Collections.Concurrent;
using System.Text.Json;
using MdReel.Core.Domain;

namespace MdReel.Api.Features.Instrumentation;

public sealed class InMemoryEventStore : IEventStore
{
    private readonly List<EventRecord> _events = [];
    private readonly object _gate = new();

    public Task<EventRecord> RecordAsync(EventDraft draft, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var record = new EventRecord(
            $"evt_{Guid.NewGuid():N}",
            draft.Name,
            draft.SessionId,
            draft.TenantId,
            draft.UserId,
            draft.OccurredAt,
            draft.Referrer,
            draft.AbArm,
            draft.PayloadJson);

        lock (_gate)
        {
            _events.Add(record);
        }

        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<EventRecord>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<EventRecord>>(_events.ToArray());
        }
    }
}

public sealed class InMemoryTenantStore(ICostLedger costLedger) : ITenantStore
{
    private readonly ConcurrentDictionary<string, TenantRecord> _byId = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _emailToId = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, UserRecord> _userByEmail = new(StringComparer.OrdinalIgnoreCase);

    public Task<SignupResult> UpsertSignupAsync(SignupDraft draft, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var email = draft.Email.Trim();
        var trialGranted = false;
        var tenant = _emailToId.TryGetValue(email, out var existingId) && _byId.TryGetValue(existingId, out var existing)
            ? existing with
            {
                ArchiveHours = draft.ArchiveHours,
                MonthlyHours = draft.MonthlyHours,
            }
            : CreateTenant(draft, email, out trialGranted);

        if (!trialGranted && tenant.TrialCreditHours <= 0)
        {
            tenant = tenant with { TrialCreditHours = 1 };
            trialGranted = true;
        }

        _byId[tenant.Id] = tenant;
        _emailToId[email] = tenant.Id;

        var user = _userByEmail.GetOrAdd(email, _ => new UserRecord($"usr_{Guid.NewGuid():N}", tenant.Id, email));
        if (trialGranted)
        {
            costLedger.Record(new CostEntry(tenant.Id, CostKind.Compute, "trial_credit", 1, "hours", null));
        }

        return Task.FromResult(new SignupResult(tenant, user, trialGranted));
    }

    public Task<TenantRecord?> GetAsync(string tenantId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _byId.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant);
    }

    public Task<TenantRecord?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_emailToId.TryGetValue(email, out var tenantId) && _byId.TryGetValue(tenantId, out var tenant) ? tenant : null);
    }

    public Task SetPlanAsync(string tenantId, string plan, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_byId.TryGetValue(tenantId, out var tenant))
        {
            _byId[tenantId] = tenant with { Plan = plan };
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TenantRecord>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<TenantRecord>>(_byId.Values.ToArray());
    }

    private static TenantRecord CreateTenant(SignupDraft draft, string email, out bool trialGranted)
    {
        trialGranted = true;
        return new TenantRecord(
            $"ten_{Guid.NewGuid():N}",
            email,
            EmptyToNull(draft.UtmSource),
            EmptyToNull(draft.UtmMedium),
            EmptyToNull(draft.UtmCampaign),
            EmptyToNull(draft.UtmTerm),
            EmptyToNull(draft.FirstReferrer),
            EmptyToNull(draft.AbArm),
            draft.ArchiveHours,
            draft.MonthlyHours,
            1,
            null);
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}

public sealed class InMemoryAdSpendLedger : IAdSpendLedger
{
    private readonly List<AdSpendEntry> _entries = [];
    private readonly object _gate = new();

    public Task<AdSpendEntry> RecordAsync(AdSpendEntry entry, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stored = string.IsNullOrWhiteSpace(entry.Id) ? entry with { Id = $"ad_{Guid.NewGuid():N}" } : entry;
        lock (_gate)
        {
            _entries.Add(stored);
        }

        return Task.FromResult(stored);
    }

    public Task<IReadOnlyList<AdSpendEntry>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<AdSpendEntry>>(_entries.ToArray());
        }
    }
}

public sealed class InMemoryCohortAnalytics(IEventStore events, ITenantStore tenants) : ICohortAnalytics
{
    public async Task<IReadOnlyList<TenantHoursByWeek>> GetTenantHoursByWeekAsync(CancellationToken cancellationToken)
    {
        var tenantList = await tenants.ListAsync(cancellationToken);
        var byTenant = tenantList.ToDictionary(static x => x.Id, static _ => DateTimeOffset.MaxValue, StringComparer.Ordinal);
        var eventList = await events.ListAsync(cancellationToken);

        foreach (var signup in eventList.Where(static x => x.Name == "signup" && x.TenantId is not null))
        {
            if (byTenant.TryGetValue(signup.TenantId!, out var current) && signup.OccurredAt < current)
            {
                byTenant[signup.TenantId!] = signup.OccurredAt;
            }
        }

        return eventList
            .Where(static x => x.Name == "job_completed" && x.TenantId is not null)
            .Select(x => ToHoursByWeek(x, byTenant))
            .Where(static x => x is not null)
            .Select(static x => x!)
            .GroupBy(static x => new { x.TenantId, x.WeekIndex })
            .Select(static g => new TenantHoursByWeek(g.Key.TenantId, g.Key.WeekIndex, g.Sum(x => x.Hours)))
            .OrderBy(static x => x.TenantId, StringComparer.Ordinal)
            .ThenBy(static x => x.WeekIndex)
            .ToArray();
    }

    private static TenantHoursByWeek? ToHoursByWeek(EventRecord record, Dictionary<string, DateTimeOffset> signupByTenant)
    {
        if (record.TenantId is null || !signupByTenant.TryGetValue(record.TenantId, out var signupAt) || signupAt == DateTimeOffset.MaxValue)
        {
            return null;
        }

        using var payload = JsonDocument.Parse(record.PayloadJson);
        var root = payload.RootElement;
        var hours = TryGetDouble(root, "hours") ?? (TryGetDouble(root, "duration_sec") / 3600) ?? (TryGetDouble(root, "duration_seconds") / 3600);
        if (hours is null)
        {
            return null;
        }

        var weekIndex = Math.Max(0, (int)Math.Floor((record.OccurredAt - signupAt).TotalDays / 7));
        return new TenantHoursByWeek(record.TenantId, weekIndex, hours.Value);
    }

    private static double? TryGetDouble(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
            ? value
            : null;
}
