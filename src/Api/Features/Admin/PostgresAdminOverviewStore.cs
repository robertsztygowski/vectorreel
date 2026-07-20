using Npgsql;

namespace MdReel.Api.Features.Admin;

public sealed class PostgresAdminOverviewStore(NpgsqlDataSource dataSource) : IAdminOverviewStore
{
    public async Task<AdminOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken)
    {
        await Instrumentation.PostgresSchema.EnsureAsync(dataSource, cancellationToken);

        return new AdminOverviewResponse(
            await GetFunnelAsync(cancellationToken),
            await GetUsageAsync(cancellationToken),
            await GetRetentionAsync(cancellationToken),
            await GetSourcesAsync(cancellationToken));
    }

    private async Task<IReadOnlyList<AdminFunnelWindow>> GetFunnelAsync(CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("""
            with windows(label, start_at) as (
                values
                    ('today', date_trunc('day', now())),
                    ('7d', now() - interval '7 days'),
                    ('30d', now() - interval '30 days')
            )
            select w.label,
                   count(distinct coalesce(e.session_id, e.id)) filter (where e.name = 'page_view')::int as page_view,
                   count(distinct coalesce(e.session_id, e.id)) filter (where e.name = 'signup_view')::int as signup_view,
                   count(distinct coalesce(e.session_id, e.id)) filter (where e.name = 'signup')::int as signup,
                   count(distinct coalesce(e.session_id, e.id)) filter (where e.name = 'upload_started')::int as upload_started,
                   count(distinct coalesce(e.session_id, e.id)) filter (where e.name = 'job_completed')::int as job_completed,
                   count(distinct coalesce(e.session_id, e.id)) filter (where e.name = 'checkout_clicked')::int as checkout_clicked,
                   count(distinct coalesce(e.session_id, e.id)) filter (where e.name = 'payment_succeeded')::int as payment_succeeded
            from windows w
            left join events e on e.occurred_at >= w.start_at
            group by w.label
            order by case w.label when 'today' then 1 when '7d' then 2 else 3 end
            """);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<AdminFunnelWindow>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new AdminFunnelWindow(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7)));
        }

        return rows;
    }

    private async Task<AdminUsageOverview> GetUsageAsync(CancellationToken cancellationToken) => new(
        await GetUsageWindowAsync("date_trunc('day', now())", cancellationToken),
        await GetUsageWindowAsync("now() - interval '7 days'", cancellationToken));

    private async Task<AdminUsageWindow> GetUsageWindowAsync(string startExpression, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"""
            select count(*)::int,
                   coalesce(sum(coalesce(
                       (payload_json->>'hours')::double precision * 60,
                       (payload_json->>'duration_sec')::double precision / 60,
                       (payload_json->>'duration_seconds')::double precision / 60
                   )), 0)::double precision
            from events
            where name = 'job_completed'
              and occurred_at >= {startExpression}
            """);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AdminUsageWindow(0, 0);
        }

        return new AdminUsageWindow(reader.GetInt32(0), reader.GetDouble(1));
    }

    private async Task<AdminRetentionOverview> GetRetentionAsync(CancellationToken cancellationToken)
    {
        int new7;
        int new30;
        int returning;
        int inactive;
        await using (var command = dataSource.CreateCommand("""
            select
                count(*) filter (where created_at >= now() - interval '7 days')::int as new_7d,
                count(*) filter (where created_at >= now() - interval '30 days')::int as new_30d,
                count(*) filter (
                    where created_at < date_trunc('week', now())
                      and exists (
                        select 1 from events e
                        where e.tenant_id = tenants.id
                          and e.occurred_at >= date_trunc('week', now())
                      )
                )::int as returning_this_week,
                count(*) filter (
                    where not exists (
                        select 1 from events e
                        where e.tenant_id = tenants.id
                          and e.occurred_at >= now() - interval '30 days'
                    )
                )::int as inactive_30d
            from tenants
            """))
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            new7 = reader.GetInt32(0);
            new30 = reader.GetInt32(1);
            returning = reader.GetInt32(2);
            inactive = reader.GetInt32(3);
        }

        var weeks = new List<AdminSignupWeek>();
        await using (var command = dataSource.CreateCommand("""
            with weeks as (
                select generate_series(date_trunc('week', now()) - interval '7 weeks', date_trunc('week', now()), interval '1 week') as week_start
            )
            select w.week_start::date,
                   count(t.id)::int
            from weeks w
            left join tenants t
              on date_trunc('week', t.created_at) = w.week_start
            group by w.week_start
            order by w.week_start
            """))
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                weeks.Add(new AdminSignupWeek(reader.GetFieldValue<DateOnly>(0), reader.GetInt32(1)));
            }
        }

        return new AdminRetentionOverview(new7, new30, returning, inactive, weeks);
    }

    private async Task<IReadOnlyList<AdminSourceOverview>> GetSourcesAsync(CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("""
            with tenant_sources as (
                select id, first_utm_source, first_utm_medium, first_utm_campaign
                from tenants
            ),
            payments_by_tenant as (
                select tenant_id, sum(amount_cents)::int as revenue_cents
                from payments
                group by tenant_id
            ),
            source_groups as (
                select ts.first_utm_source,
                       ts.first_utm_medium,
                       ts.first_utm_campaign,
                       count(ts.id)::int as tenant_count,
                       count(p.tenant_id)::int as paying_tenant_count,
                       coalesce(sum(p.revenue_cents), 0)::int as revenue_cents
                from tenant_sources ts
                left join payments_by_tenant p on p.tenant_id = ts.id
                group by ts.first_utm_source, ts.first_utm_medium, ts.first_utm_campaign
            ),
            spend_by_campaign as (
                select campaign, sum(cost_cents)::int as ad_spend_cents
                from ad_spend
                group by campaign
            )
            select sg.first_utm_source,
                   sg.first_utm_medium,
                   sg.first_utm_campaign,
                   sg.tenant_count,
                   sg.paying_tenant_count,
                   sg.revenue_cents,
                   coalesce(sb.ad_spend_cents, 0) as ad_spend_cents,
                   case when sg.paying_tenant_count = 0 then null
                        else coalesce(sb.ad_spend_cents, 0)::double precision / sg.paying_tenant_count
                   end as cac_cents
            from source_groups sg
            left join spend_by_campaign sb on sb.campaign is not distinct from sg.first_utm_campaign
            order by sg.tenant_count desc, sg.first_utm_source nulls last, sg.first_utm_campaign nulls last
            """);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<AdminSourceOverview>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new AdminSourceOverview(
                reader.IsDBNull(0) ? null : reader.GetString(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetDouble(7)));
        }

        return rows;
    }
}
