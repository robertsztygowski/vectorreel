using Npgsql;

namespace MdReel.Api.Features.Instrumentation;

public sealed class PostgresTenantStore(NpgsqlDataSource dataSource) : ITenantStore
{
    public async Task<SignupResult> UpsertSignupAsync(SignupDraft draft, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        var email = draft.Email.Trim();
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);
        var existing = await ReadTenantByEmailAsync(connection, email, cancellationToken);
        var trialGranted = existing is null || existing.TrialCreditHours <= 0;
        var tenant = existing is null
            ? new TenantRecord($"ten_{Guid.NewGuid():N}", email, EmptyToNull(draft.UtmSource), EmptyToNull(draft.UtmMedium), EmptyToNull(draft.UtmCampaign), EmptyToNull(draft.UtmTerm), EmptyToNull(draft.FirstReferrer), EmptyToNull(draft.AbArm), draft.ArchiveHours, draft.MonthlyHours, 1, null)
            : existing with { ArchiveHours = draft.ArchiveHours, MonthlyHours = draft.MonthlyHours, TrialCreditHours = existing.TrialCreditHours <= 0 ? 1 : existing.TrialCreditHours };

        await using (var upsert = new NpgsqlCommand("""
            insert into tenants (id, email, first_utm_source, first_utm_medium, first_utm_campaign, first_utm_term, first_referrer, ab_arm, archive_hours, monthly_hours, trial_credit_hours)
            values (@id, @email, @source, @medium, @campaign, @term, @referrer, @ab_arm, @archive_hours, @monthly_hours, @trial_credit_hours)
            on conflict (email) do update set archive_hours = excluded.archive_hours, monthly_hours = excluded.monthly_hours, trial_credit_hours = greatest(tenants.trial_credit_hours, excluded.trial_credit_hours)
            """, connection, tx))
        {
            AddTenantParams(upsert, tenant);
            await upsert.ExecuteNonQueryAsync(cancellationToken);
        }

        var user = await UpsertUserAsync(connection, tx, tenant.Id, email, cancellationToken);
        if (trialGranted)
        {
            await using var credit = new NpgsqlCommand("""
                insert into usage_ledger (id, tenant_id, kind, step, quantity, unit)
                values (@id, @tenant_id, 'credit', 'trial_credit', 1, 'hours')
                """, connection, tx);
            credit.Parameters.AddWithValue("id", $"use_{Guid.NewGuid():N}");
            credit.Parameters.AddWithValue("tenant_id", tenant.Id);
            await credit.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return new SignupResult(tenant, user, trialGranted);
    }

    public async Task<TenantRecord?> GetAsync(string tenantId, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await ReadTenantAsync(connection, tenantId, cancellationToken);
    }

    public async Task<TenantRecord?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await ReadTenantByEmailAsync(connection, email, cancellationToken);
    }

    public async Task SetPlanAsync(string tenantId, string plan, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("update tenants set plan = @plan where id = @id");
        command.Parameters.AddWithValue("plan", plan);
        command.Parameters.AddWithValue("id", tenantId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantRecord>> ListAsync(CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            select id, email, first_utm_source, first_utm_medium, first_utm_campaign, first_utm_term, first_referrer, ab_arm, archive_hours, monthly_hours, trial_credit_hours, plan
            from tenants
            order by created_at, id
            """);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var tenants = new List<TenantRecord>();
        while (await reader.ReadAsync(cancellationToken))
        {
            tenants.Add(ReadTenant(reader));
        }

        return tenants;
    }

    private static async Task<TenantRecord?> ReadTenantAsync(NpgsqlConnection connection, string tenantId, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("""
            select id, email, first_utm_source, first_utm_medium, first_utm_campaign, first_utm_term, first_referrer, ab_arm, archive_hours, monthly_hours, trial_credit_hours, plan
            from tenants where id = @id
            """, connection);
        command.Parameters.AddWithValue("id", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadTenant(reader) : null;
    }

    private static async Task<TenantRecord?> ReadTenantByEmailAsync(NpgsqlConnection connection, string email, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("""
            select id, email, first_utm_source, first_utm_medium, first_utm_campaign, first_utm_term, first_referrer, ab_arm, archive_hours, monthly_hours, trial_credit_hours, plan
            from tenants where email = @email
            """, connection);
        command.Parameters.AddWithValue("email", email);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadTenant(reader) : null;
    }

    private static async Task<UserRecord> UpsertUserAsync(NpgsqlConnection connection, NpgsqlTransaction tx, string tenantId, string email, CancellationToken cancellationToken)
    {
        var id = $"usr_{Guid.NewGuid():N}";
        await using var command = new NpgsqlCommand("""
            insert into users (id, tenant_id, email) values (@id, @tenant_id, @email)
            on conflict (email) do update set tenant_id = excluded.tenant_id
            returning id, tenant_id, email
            """, connection, tx);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("email", email);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new UserRecord(reader.GetString(0), reader.GetString(1), reader.GetString(2));
    }

    private static TenantRecord ReadTenant(NpgsqlDataReader reader) => new(
        reader.GetString(0),
        reader.GetString(1),
        reader.IsDBNull(2) ? null : reader.GetString(2),
        reader.IsDBNull(3) ? null : reader.GetString(3),
        reader.IsDBNull(4) ? null : reader.GetString(4),
        reader.IsDBNull(5) ? null : reader.GetString(5),
        reader.IsDBNull(6) ? null : reader.GetString(6),
        reader.IsDBNull(7) ? null : reader.GetString(7),
        reader.IsDBNull(8) ? null : reader.GetDouble(8),
        reader.IsDBNull(9) ? null : reader.GetDouble(9),
        reader.GetDouble(10),
        reader.IsDBNull(11) ? null : reader.GetString(11));

    private static void AddTenantParams(NpgsqlCommand command, TenantRecord tenant)
    {
        command.Parameters.AddWithValue("id", tenant.Id);
        command.Parameters.AddWithValue("email", tenant.Email);
        command.Parameters.AddWithValue("source", (object?)tenant.FirstUtmSource ?? DBNull.Value);
        command.Parameters.AddWithValue("medium", (object?)tenant.FirstUtmMedium ?? DBNull.Value);
        command.Parameters.AddWithValue("campaign", (object?)tenant.FirstUtmCampaign ?? DBNull.Value);
        command.Parameters.AddWithValue("term", (object?)tenant.FirstUtmTerm ?? DBNull.Value);
        command.Parameters.AddWithValue("referrer", (object?)tenant.FirstReferrer ?? DBNull.Value);
        command.Parameters.AddWithValue("ab_arm", (object?)tenant.AbArm ?? DBNull.Value);
        command.Parameters.AddWithValue("archive_hours", (object?)tenant.ArchiveHours ?? DBNull.Value);
        command.Parameters.AddWithValue("monthly_hours", (object?)tenant.MonthlyHours ?? DBNull.Value);
        command.Parameters.AddWithValue("trial_credit_hours", tenant.TrialCreditHours);
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
