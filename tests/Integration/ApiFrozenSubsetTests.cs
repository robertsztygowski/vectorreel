using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MdReel.Tests.Integration;

public sealed class ApiFrozenSubsetTests
{
    [Fact]
    public async Task Post_uploads_defaults_to_api_storage_and_returns_frozen_contract_shape()
    {
        await using var factory = new ApiFactory();
        using var client = CreateAuthedClient(factory);

        using var response = await client.PostAsJsonAsync("/api/v1/uploads", new { contentType = "video/mp4" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        var root = payload.RootElement;
        Assert.StartsWith("up_", root.GetProperty("uploadId").GetString());
        Assert.Equal("api", root.GetProperty("storage").GetString());
        var uploadUrl = root.GetProperty("uploadUrl").GetString();
        Assert.NotNull(uploadUrl);
        Assert.StartsWith("http://localhost/api/v1/uploads/", uploadUrl);
        Assert.Contains("/content?sig=", uploadUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_jobs_returns_frozen_contract_shape_newest_first()
    {
        await using var factory = new ApiFactory();
        using var client = CreateAuthedClient(factory);

        _ = await CreateJobAsync(client, "a.mp4");
        _ = await CreateJobAsync(client, "b.mp4");

        using var response = await client.GetAsync("/api/v1/jobs");
        response.EnsureSuccessStatusCode();

        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        var root = payload.RootElement;

        Assert.True(root.TryGetProperty("jobs", out var jobs));
        Assert.Equal(JsonValueKind.Array, jobs.ValueKind);
        Assert.NotEmpty(jobs.EnumerateArray());

        DateTimeOffset? previous = null;
        foreach (var item in jobs.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("jobId", out _));
            Assert.True(item.TryGetProperty("status", out var status));
            Assert.True(item.TryGetProperty("progress", out _));
            Assert.True(item.TryGetProperty("source", out _));
            Assert.True(item.TryGetProperty("created_at", out var createdAt));

            var createdAtValue = DateTimeOffset.Parse(createdAt.GetString()!, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
            if (previous is not null)
            {
                Assert.True(previous >= createdAtValue, "jobs must be returned newest first");
            }

            previous = createdAtValue;

            var hasStage = item.TryGetProperty("stage", out var stageValue);
            var statusValue = status.GetString();
            if (statusValue == "processing")
            {
                Assert.True(hasStage);
                Assert.Equal(JsonValueKind.String, stageValue.ValueKind);
            }
            else
            {
                Assert.False(hasStage);
            }
        }
    }

    [Fact]
    public async Task Delete_jobs_performs_erasure_and_returns_problem_when_missing()
    {
        await using var factory = new ApiFactory();
        using var client = CreateAuthedClient(factory);

        var jobId = await CreateJobAsync(client, "delete-me.mp4");

        using var listResponse = await client.GetAsync("/api/v1/jobs");
        listResponse.EnsureSuccessStatusCode();
        using var listPayload = await listResponse.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(listPayload);
        Assert.Contains(
            listPayload.RootElement.GetProperty("jobs").EnumerateArray(),
            item => item.GetProperty("jobId").GetString() == jobId);

        using var deleteResponse = await client.DeleteAsync($"/api/v1/jobs/{jobId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Empty(await deleteResponse.Content.ReadAsByteArrayAsync());

        using var listAfterDelete = await client.GetAsync("/api/v1/jobs");
        listAfterDelete.EnsureSuccessStatusCode();
        using var listAfterDeletePayload = await listAfterDelete.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(listAfterDeletePayload);
        Assert.DoesNotContain(
            listAfterDeletePayload.RootElement.GetProperty("jobs").EnumerateArray(),
            item => item.GetProperty("jobId").GetString() == jobId);

        using var missingDelete = await client.DeleteAsync($"/api/v1/jobs/{jobId}");
        Assert.Equal(HttpStatusCode.NotFound, missingDelete.StatusCode);
        var missingDeleteMediaType = missingDelete.Content.Headers.ContentType?.MediaType;
        Assert.Equal("application/problem+json", missingDeleteMediaType);
        using var problem = await missingDelete.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Job not found", problem.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Non_success_routes_return_rfc_7807_problem_details()
    {
        await using var factory = new ApiFactory();
        using var client = CreateAuthedClient(factory);

        using var response = await client.GetAsync("/api/v1/missing-route");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        Assert.Equal("application/problem+json", mediaType);

        using var problem = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("title").GetString()));
    }

    [Fact]
    public void Api_has_no_public_youtube_file_uri_routes()
    {
        using var factory = new ApiFactory();
        var routes = factory.Services
            .GetServices<EndpointDataSource>()
            .SelectMany(static source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(static endpoint => endpoint.RoutePattern.RawText ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(routes, route =>
            route.Contains("youtube", StringComparison.OrdinalIgnoreCase)
            || route.Contains("fileuri", StringComparison.OrdinalIgnoreCase));
    }

    private static HttpClient CreateAuthedClient(ApiFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        return client;
    }

    private static async Task<string> CreateJobAsync(HttpClient client, string filename)
    {
        using var uploadResponse = await client.PostAsync("/api/v1/uploads", content: null);
        uploadResponse.EnsureSuccessStatusCode();
        using var uploadPayload = await uploadResponse.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(uploadPayload);

        var uploadId = uploadPayload.RootElement.GetProperty("uploadId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(uploadId));

        using var createJobResponse = await client.PostAsJsonAsync(
            "/api/v1/jobs",
            new
            {
                uploadId,
                options = new
                {
                    filename,
                    durationSec = 90,
                },
            });
        createJobResponse.EnsureSuccessStatusCode();
        using var createJobPayload = await createJobResponse.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(createJobPayload);

        var jobId = createJobPayload.RootElement.GetProperty("jobId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(jobId));
        return jobId!;
    }

    private sealed class ApiFactory : WebApplicationFactory<MdReel.Api.Program>;
}
