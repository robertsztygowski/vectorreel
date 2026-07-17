using System.Net;
using System.Net.Http.Json;
using MdReel.Api.Features.Webhooks;
using MdReel.Core.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MdReel.Tests.Integration;

public sealed class WebhookDeliveryTests
{
    [Fact]
    public async Task Dispatcher_enqueues_through_inprocess_queue_and_marks_delivery_delivered()
    {
        var sender = new ControllableWebhookSender(new WebhookSendResult(true, 204, null));
        await using var factory = new ApiFactory(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IWebhookSender>();
            services.AddSingleton<IWebhookSender>(sender);
        }));

        var dispatcher = factory.Services.GetRequiredService<WebhookDeliveryDispatcher>();
        var record = await dispatcher.EnqueueAsync(
            "tenant_any",
            "job.completed",
            "https://customer.example/webhook",
            "secret",
            "{\"ok\":true}",
            CancellationToken.None);

        var stored = await factory.Services.GetRequiredService<IWebhookDeliveryStore>().GetAsync(record.Id, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal("delivered", stored.Status);
        Assert.Equal(204, stored.ResponseStatus);
        Assert.Single(sender.Sent);
    }

    [Fact]
    public async Task Failed_attempt_stays_pending_then_exhausts_to_failed()
    {
        var sender = new ControllableWebhookSender(new WebhookSendResult(false, 503, "HTTP 503"));
        await using var factory = new ApiFactory(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IWebhookSender>();
            services.AddSingleton<IWebhookSender>(sender);
        }));
        var store = factory.Services.GetRequiredService<IWebhookDeliveryStore>();
        var queue = factory.Services.GetRequiredService<ITaskQueue>();
        var created = await store.EnqueueAsync(new WebhookDeliveryRecord(
            string.Empty,
            "tenant_any",
            "job.completed",
            "https://customer.example/webhook",
            "{\"ok\":true}",
            "secret",
            MaxAttempts: 2), CancellationToken.None);

        await queue.EnqueueAsync("webhook-deliveries", created.Id, CancellationToken.None);

        var first = await store.GetAsync(created.Id, CancellationToken.None);
        Assert.NotNull(first);
        Assert.Equal("pending", first.Status);
        Assert.Equal(1, first.Attempts);
        Assert.True(first.NextAttemptAt > DateTimeOffset.UtcNow);
        Assert.Equal(503, first.ResponseStatus);

        await queue.EnqueueAsync("webhook-deliveries", created.Id, CancellationToken.None);

        var second = await store.GetAsync(created.Id, CancellationToken.None);
        Assert.NotNull(second);
        Assert.Equal("failed", second.Status);
        Assert.Equal(2, second.Attempts);
    }

    [Fact]
    public async Task Internal_attempt_endpoint_drives_one_delivery_attempt()
    {
        var sender = new ControllableWebhookSender(new WebhookSendResult(true, 200, null));
        await using var factory = new ApiFactory(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IWebhookSender>();
            services.AddSingleton<IWebhookSender>(sender);
        }));
        using var client = factory.CreateClient();
        var store = factory.Services.GetRequiredService<IWebhookDeliveryStore>();
        var created = await store.EnqueueAsync(new WebhookDeliveryRecord(
            string.Empty,
            "tenant_any",
            "job.completed",
            "https://customer.example/webhook",
            "{\"ok\":true}",
            "secret"), CancellationToken.None);

        using var response = await client.PostAsync($"/internal/webhook-deliveries/{created.Id}/attempt", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AttemptResponse>();
        Assert.NotNull(body);
        Assert.Equal("delivered", body.Status);
    }

    private sealed record AttemptResponse(string Id, string Status, int Attempts);

    private sealed class ControllableWebhookSender(WebhookSendResult result) : IWebhookSender
    {
        public List<WebhookDeliveryRecord> Sent { get; } = [];

        public Task<WebhookSendResult> SendAsync(WebhookDeliveryRecord delivery, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Sent.Add(delivery);
            return Task.FromResult(result);
        }
    }

    private sealed class ApiFactory(Action<IWebHostBuilder>? configure = null) : WebApplicationFactory<MdReel.Api.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            configure?.Invoke(builder);
        }
    }
}
