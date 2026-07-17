using MdReel.Infrastructure.Queue;
using Microsoft.Extensions.Options;

namespace MdReel.Tests.Unit.Queue;

public sealed class TaskQueueTests
{
    [Fact]
    public async Task InProcessQueue_invokes_registered_handler()
    {
        var calls = new List<(string Queue, string Payload)>();
        var queue = new InProcessQueue((name, payload, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            calls.Add((name, payload));
            return Task.CompletedTask;
        });

        await queue.EnqueueAsync("webhook-deliveries", "wd_123", CancellationToken.None);

        var call = Assert.Single(calls);
        Assert.Equal("webhook-deliveries", call.Queue);
        Assert.Equal("wd_123", call.Payload);
    }

    [Fact]
    public async Task CloudTasksQueue_builds_http_push_request()
    {
        var transport = new CapturingCloudTasksTransport();
        var options = Options.Create(new CloudTasksOptions
        {
            ProjectId = "mdreel-prod",
            Location = "europe-west1",
            QueueName = "unused-default",
            TargetBaseUrl = "https://api.mdreel.example/",
        });
        var queue = new CloudTasksQueue(options, transport);

        await queue.EnqueueAsync("webhook-deliveries", "wd_123", CancellationToken.None);

        Assert.NotNull(transport.Request);
        var request = transport.Request!;
        Assert.Equal("projects/mdreel-prod/locations/europe-west1/queues/webhook-deliveries", request.QueuePath);
        Assert.Equal(new Uri("https://api.mdreel.example/internal/webhook-deliveries/wd_123/attempt"), request.TargetUrl);
        Assert.Equal("POST", request.HttpMethod);
        Assert.Equal("wd_123", request.Body);
        Assert.Equal("application/json", request.Headers["Content-Type"]);
        Assert.Equal(TimeSpan.FromMinutes(5), request.DispatchDeadline);
    }

    private sealed class CapturingCloudTasksTransport : ICloudTasksTransport
    {
        public CloudTaskRequest? Request { get; private set; }

        public Task CreateTaskAsync(CloudTaskRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Request = request;
            return Task.CompletedTask;
        }
    }
}
