using Npgsql;

namespace MdReel.Api.Features.Instrumentation;

public sealed class PostgresEventStore(NpgsqlDataSource dataSource) : IEventStore
{
    public async Task<EventRecord> RecordAsync(EventDraft draft, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        var id = $"evt_{Guid.NewGuid():N}";
        await using var command = dataSource.CreateCommand("""
            insert into events (id, name, session_id, tenant_id, user_id, occurred_at, referrer, ab_arm, payload_json)
            values (@id, @name, @session_id, @tenant_id, @user_id, @occurred_at, @referrer, @ab_arm, @payload_json::jsonb)
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("name", draft.Name);
        command.Parameters.AddWithValue("session_id", (object?)draft.SessionId ?? DBNull.Value);
        command.Parameters.AddWithValue("tenant_id", (object?)draft.TenantId ?? DBNull.Value);
        command.Parameters.AddWithValue("user_id", (object?)draft.UserId ?? DBNull.Value);
        command.Parameters.AddWithValue("occurred_at", draft.OccurredAt);
        command.Parameters.AddWithValue("referrer", (object?)draft.Referrer ?? DBNull.Value);
        command.Parameters.AddWithValue("ab_arm", (object?)draft.AbArm ?? DBNull.Value);
        command.Parameters.AddWithValue("payload_json", draft.PayloadJson);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return new EventRecord(id, draft.Name, draft.SessionId, draft.TenantId, draft.UserId, draft.OccurredAt, draft.Referrer, draft.AbArm, draft.PayloadJson);
    }

    public async Task<IReadOnlyList<EventRecord>> ListAsync(CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            select id, name, session_id, tenant_id, user_id, occurred_at, referrer, ab_arm, payload_json::text
            from events
            order by occurred_at, id
            """);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<EventRecord>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new EventRecord(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetFieldValue<DateTimeOffset>(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetString(8)));
        }

        return rows;
    }
}
