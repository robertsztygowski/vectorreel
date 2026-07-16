using Npgsql;

namespace MdReel.Api.Features.Instrumentation;

public sealed class PostgresAdSpendLedger(NpgsqlDataSource dataSource) : IAdSpendLedger
{
    public async Task<AdSpendEntry> RecordAsync(AdSpendEntry entry, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        var stored = string.IsNullOrWhiteSpace(entry.Id) ? entry with { Id = $"ad_{Guid.NewGuid():N}" } : entry;
        await using var command = dataSource.CreateCommand("""
            insert into ad_spend (id, day, channel, campaign, keyword, cost_cents, clicks, impressions)
            values (@id, @day, @channel, @campaign, @keyword, @cost_cents, @clicks, @impressions)
            on conflict (id) do update set day = excluded.day, channel = excluded.channel, campaign = excluded.campaign, keyword = excluded.keyword, cost_cents = excluded.cost_cents, clicks = excluded.clicks, impressions = excluded.impressions
            """);
        command.Parameters.AddWithValue("id", stored.Id);
        command.Parameters.AddWithValue("day", stored.Day);
        command.Parameters.AddWithValue("channel", stored.Channel);
        command.Parameters.AddWithValue("campaign", (object?)stored.Campaign ?? DBNull.Value);
        command.Parameters.AddWithValue("keyword", (object?)stored.Keyword ?? DBNull.Value);
        command.Parameters.AddWithValue("cost_cents", stored.CostCents);
        command.Parameters.AddWithValue("clicks", (object?)stored.Clicks ?? DBNull.Value);
        command.Parameters.AddWithValue("impressions", (object?)stored.Impressions ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return stored;
    }

    public async Task<IReadOnlyList<AdSpendEntry>> ListAsync(CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("select id, day, channel, campaign, keyword, cost_cents, clicks, impressions from ad_spend order by day, id");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<AdSpendEntry>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new AdSpendEntry(
                reader.GetString(0),
                reader.GetFieldValue<DateOnly>(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(5),
                reader.IsDBNull(6) ? null : reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetInt32(7)));
        }

        return rows;
    }
}

public sealed class PostgresCohortAnalytics(NpgsqlDataSource dataSource) : ICohortAnalytics
{
    public async Task<IReadOnlyList<TenantHoursByWeek>> GetTenantHoursByWeekAsync(CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            with signup as (
                select tenant_id, min(occurred_at) as signup_at
                from events
                where name = 'signup' and tenant_id is not null
                group by tenant_id
            )
            select e.tenant_id,
                   greatest(0, floor(extract(epoch from (e.occurred_at - s.signup_at)) / 604800))::int as week_index,
                   sum(coalesce((e.payload_json->>'hours')::double precision, (e.payload_json->>'duration_sec')::double precision / 3600, (e.payload_json->>'duration_seconds')::double precision / 3600)) as hours
            from events e
            join signup s on s.tenant_id = e.tenant_id
            where e.name = 'job_completed' and e.tenant_id is not null
            group by e.tenant_id, week_index
            order by e.tenant_id, week_index
            """);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<TenantHoursByWeek>();
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(2))
            {
                rows.Add(new TenantHoursByWeek(reader.GetString(0), reader.GetInt32(1), reader.GetDouble(2)));
            }
        }

        return rows;
    }
}
