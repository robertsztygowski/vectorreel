using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MdReel.Core.Providers;
using MdReel.Infrastructure;
using MdReel.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MdReel.Tests.Integration;

public sealed class UploadUrlModeTests
{
    [Theory]
    [InlineData("local", null, typeof(DisabledUploadUrlSigner))]
    [InlineData("gcs", "http://fake-gcs-server:4443", typeof(DisabledUploadUrlSigner))]
    [InlineData("gcs", null, typeof(GcsV4UploadUrlSigner))]
    public void Infrastructure_selects_direct_upload_signer_only_for_real_gcs(
        string storageMode,
        string? emulatorHost,
        Type expectedSignerType)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Storage:Mode"] = storageMode,
            ["Gcs:EmulatorHost"] = emulatorHost,
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection();

        services.AddPipelineInfrastructure(
            configuration,
            Path.Combine(Environment.CurrentDirectory, ".local-state", "object-storage-tests"));

        using var provider = services.BuildServiceProvider();

        Assert.IsType(expectedSignerType, provider.GetRequiredService<IUploadUrlSigner>());
    }

    [Fact]
    public async Task Real_gcs_mode_returns_signed_gcs_put_url_bound_to_content_type()
    {
        var signer = new CapturingUploadUrlSigner();
        await using var factory = new UploadModeApiFactory(
            new Dictionary<string, string?>
            {
                ["Storage:Mode"] = "gcs",
            },
            services =>
            {
                services.RemoveAll<IUploadUrlSigner>();
                services.RemoveAll<IObjectStorage>();
                services.AddSingleton<IUploadUrlSigner>(signer);
                services.AddSingleton<IObjectStorage, NoopObjectStorage>();
            });
        using var client = CreateAuthedClient(factory);

        using var response = await client.PostAsJsonAsync("/api/v1/uploads", new { contentType = "video/mp4" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        var root = payload.RootElement;
        Assert.Equal("gcs", root.GetProperty("storage").GetString());
        Assert.Equal(signer.ReturnedUrl, root.GetProperty("uploadUrl").GetString());
        Assert.Equal("raw-videos-eu", signer.Bucket);
        Assert.StartsWith("uploads/up_", signer.ObjectName);
        Assert.Equal("video/mp4", signer.ContentType);
        Assert.Equal(TimeSpan.FromHours(2), signer.ExpiresAfter);
    }

    [Fact]
    public async Task Gcs_emulator_mode_keeps_api_proxied_uploads()
    {
        await using var factory = new UploadModeApiFactory(
            new Dictionary<string, string?>
            {
                ["Storage:Mode"] = "gcs",
                ["Gcs:EmulatorHost"] = "http://fake-gcs-server:4443",
            },
            services =>
            {
                services.RemoveAll<IObjectStorage>();
                services.AddSingleton<IObjectStorage, NoopObjectStorage>();
            });
        using var client = CreateAuthedClient(factory);

        using var response = await client.PostAsync("/api/v1/uploads", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        Assert.Equal("api", payload.RootElement.GetProperty("storage").GetString());
        Assert.Contains("/api/v1/uploads/", payload.RootElement.GetProperty("uploadUrl").GetString(), StringComparison.Ordinal);
    }

    private static HttpClient CreateAuthedClient(WebApplicationFactory<MdReel.Api.Program> factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        return client;
    }

    private sealed class UploadModeApiFactory(
        Dictionary<string, string?> settings,
        Action<IServiceCollection> configureServices) : WebApplicationFactory<MdReel.Api.Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(settings));
            builder.ConfigureServices(configureServices);
            return base.CreateHost(builder);
        }
    }

    private sealed class CapturingUploadUrlSigner : IUploadUrlSigner
    {
        public string ReturnedUrl { get; } = "https://storage.googleapis.com/raw-videos-eu/uploads/up_fake?X-Goog-Signature=fake";

        public bool DirectUploadsEnabled => true;

        public string? Bucket { get; private set; }

        public string? ObjectName { get; private set; }

        public string? ContentType { get; private set; }

        public TimeSpan? ExpiresAfter { get; private set; }

        public Task<string> SignPutUrlAsync(
            string bucket,
            string objectName,
            string contentType,
            TimeSpan expiresAfter,
            CancellationToken cancellationToken)
        {
            Bucket = bucket;
            ObjectName = objectName;
            ContentType = contentType;
            ExpiresAfter = expiresAfter;
            return Task.FromResult(ReturnedUrl);
        }
    }

    private sealed class NoopObjectStorage : IObjectStorage
    {
        public Task<Stream> OpenReadAsync(string bucket, string objectName, CancellationToken cancellationToken) =>
            Task.FromResult<Stream>(new MemoryStream());

        public Task WriteAsync(string bucket, string objectName, Stream content, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteAsync(string bucket, string objectName, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
