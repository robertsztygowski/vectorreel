using MdReel.Api.Features.Instrumentation;
using Npgsql;

namespace MdReel.Api.Features.PrivateProcessing;

public static class UploadStorageModes
{
    public const string Api = "api";
    public const string Gcs = "gcs";
}

public sealed record UploadRecord(
    string Id,
    string StorageMode,
    string? ObjectName,
    string? LocalPath,
    string? Signature,
    string ContentType,
    DateTimeOffset CreatedAt,
    bool Stored);

public interface IUploadStore
{
    Task SaveAsync(UploadRecord upload, CancellationToken cancellationToken);

    Task<UploadRecord?> GetAsync(string id, CancellationToken cancellationToken);

    Task MarkStoredAsync(string id, CancellationToken cancellationToken);
}

public sealed class InMemoryUploadStore : IUploadStore
{
    private readonly Dictionary<string, UploadRecord> _uploads = [];
    private readonly object _gate = new();

    public Task SaveAsync(UploadRecord upload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _uploads[upload.Id] = upload;
        }

        return Task.CompletedTask;
    }

    public Task<UploadRecord?> GetAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            return Task.FromResult(_uploads.TryGetValue(id, out var upload) ? upload : null);
        }
    }

    public Task MarkStoredAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_uploads.TryGetValue(id, out var upload))
            {
                _uploads[id] = upload with { Stored = true };
            }
        }

        return Task.CompletedTask;
    }
}

public sealed class PostgresUploadStore(NpgsqlDataSource dataSource) : IUploadStore
{
    public async Task SaveAsync(UploadRecord upload, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            insert into private_uploads (
                id, storage_mode, object_name, local_path, signature, content_type, created_at, stored)
            values (
                @id, @storage_mode, @object_name, @local_path, @signature, @content_type, @created_at, @stored)
            """);
        AddParameters(command, upload);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<UploadRecord?> GetAsync(string id, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            select id, storage_mode, object_name, local_path, signature, content_type, created_at, stored
            from private_uploads
            where id = @id
            """);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadUpload(reader);
    }

    public async Task MarkStoredAsync(string id, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            update private_uploads
            set stored = true
            where id = @id
            """);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddParameters(NpgsqlCommand command, UploadRecord upload)
    {
        command.Parameters.AddWithValue("id", upload.Id);
        command.Parameters.AddWithValue("storage_mode", upload.StorageMode);
        command.Parameters.AddWithValue("object_name", (object?)upload.ObjectName ?? DBNull.Value);
        command.Parameters.AddWithValue("local_path", (object?)upload.LocalPath ?? DBNull.Value);
        command.Parameters.AddWithValue("signature", (object?)upload.Signature ?? DBNull.Value);
        command.Parameters.AddWithValue("content_type", upload.ContentType);
        command.Parameters.AddWithValue("created_at", upload.CreatedAt);
        command.Parameters.AddWithValue("stored", upload.Stored);
    }

    private static UploadRecord ReadUpload(NpgsqlDataReader reader) => new(
        reader.GetString(0),
        reader.GetString(1),
        reader.IsDBNull(2) ? null : reader.GetString(2),
        reader.IsDBNull(3) ? null : reader.GetString(3),
        reader.IsDBNull(4) ? null : reader.GetString(4),
        reader.GetString(5),
        reader.GetFieldValue<DateTimeOffset>(6),
        reader.GetBoolean(7));
}
