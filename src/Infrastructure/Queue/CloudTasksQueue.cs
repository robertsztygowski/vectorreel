using MdReel.Core.Providers;
using Microsoft.Extensions.Options;

namespace MdReel.Infrastructure.Queue;

public sealed record CloudTasksOptions
{
    public const string SectionName = "CloudTasks";

    public string ProjectId { get; init; } = string.Empty;

    public string Location { get; init; } = "europe-west1";

    public string QueueName { get; init; } = string.Empty;

    public string TargetBaseUrl { get; init; } = string.Empty;
}

public sealed record CloudTaskRequest(
    string QueuePath,
    Uri TargetUrl,
    string HttpMethod,
    string Body,
    IReadOnlyDictionary<string, string> Headers,
    TimeSpan DispatchDeadline);

public interface ICloudTasksTransport
{
    Task CreateTaskAsync(CloudTaskRequest request, CancellationToken cancellationToken);
}

public sealed class CloudTasksQueue(IOptions<CloudTasksOptions> options, ICloudTasksTransport transport) : ITaskQueue
{
    private static readonly TimeSpan DefaultDispatchDeadline = TimeSpan.FromMinutes(5);
    private readonly CloudTasksOptions _options = options.Value;

    public Task EnqueueAsync(string queue, string payload, CancellationToken cancellationToken)
    {
        var queueName = string.IsNullOrWhiteSpace(queue) ? _options.QueueName : queue;
        var request = new CloudTaskRequest(
            $"projects/{_options.ProjectId}/locations/{_options.Location}/queues/{queueName}",
            BuildTargetUrl(payload),
            "POST",
            payload,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Content-Type"] = "application/json",
            },
            DefaultDispatchDeadline);

        return transport.CreateTaskAsync(request, cancellationToken);
    }

    private Uri BuildTargetUrl(string payload)
    {
        var baseUrl = _options.TargetBaseUrl.TrimEnd('/');
        return new Uri($"{baseUrl}/internal/webhook-deliveries/{Uri.EscapeDataString(payload)}/attempt", UriKind.Absolute);
    }
}
