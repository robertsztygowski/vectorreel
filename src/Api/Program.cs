using MdReel.Api.Features.PrivateProcessing;
using MdReel.Core.Domain;
using MdReel.Core.Media;
using MdReel.Core.Pipeline.StageA;
using MdReel.Core.Providers;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace MdReel.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();
        ConfigurePipeline(app);
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddProblemDetails();

        services.AddSingleton(new MediaToolOptions
        {
            FfmpegPath = configuration["MEDIA_FFMPEG_PATH"] ?? "ffmpeg",
            FfprobePath = configuration["MEDIA_FFPROBE_PATH"] ?? "ffprobe",
        });
        services.AddSingleton<IMediaProbe, FfprobeMediaProbe>();
        services.AddSingleton<IMediaScanner, FfmpegMediaScanner>();
        services.AddSingleton<ICostLedger, InMemoryCostLedger>();
        services.AddSingleton<StageARunner>();
        services.AddSingleton<PrivatePipelineService>();
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;
            if (response.StatusCode is < StatusCodes.Status400BadRequest or > 599)
            {
                return;
            }

            if (!string.IsNullOrEmpty(response.ContentType) || response.ContentLength is > 0)
            {
                return;
            }

            var result = Results.Problem(
                title: ReasonPhrases.GetReasonPhrase(response.StatusCode),
                statusCode: response.StatusCode,
                instance: context.HttpContext.Request.Path);

            await result.ExecuteAsync(context.HttpContext);
        });

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        app.Use(async (context, next) =>
        {
            if (!context.Request.Path.StartsWithSegments("/api/v1"))
            {
                await next(context);
                return;
            }

            var isSignedUploadPut =
                HttpMethods.IsPut(context.Request.Method)
                && context.Request.Path.Value?.StartsWith("/api/v1/uploads/", StringComparison.Ordinal) == true
                && context.Request.Path.Value?.EndsWith("/content", StringComparison.Ordinal) == true;

            if (isSignedUploadPut)
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
            {
                await context.WriteProblemAsync(401, "Unauthorized", "Missing Authorization header.");
                return;
            }

            await next(context);
        });

        var api = app.MapGroup("/api/v1");

        api.MapPost("/uploads", (HttpContext httpContext, PrivatePipelineService pipeline) =>
        {
            var created = pipeline.CreateUpload(httpContext.Request);
            return Results.Json(created, statusCode: StatusCodes.Status201Created);
        });

        api.MapPut("/uploads/{uploadId}/content", async (
            string uploadId,
            HttpContext httpContext,
            PrivatePipelineService pipeline,
            CancellationToken cancellationToken) =>
        {
            var sig = httpContext.Request.Query["sig"].ToString();
            var stored = await pipeline.StoreUploadAsync(uploadId, sig, httpContext.Request, cancellationToken);
            if (!stored)
            {
                return Results.Problem(
                    title: "Upload not found",
                    detail: "Unknown uploadId or invalid signature.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            return Results.NoContent();
        });

        api.MapPost("/jobs", async (
            HttpContext httpContext,
            PrivatePipelineService pipeline,
            CancellationToken cancellationToken) =>
        {
            var request = await httpContext.Request.ReadFromJsonAsync<CreateJobRequest>(cancellationToken: cancellationToken);
            if (request is null)
            {
                return Results.Problem(
                    title: "Invalid request body",
                    detail: "Expected a JSON object.",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "about:blank");
            }

            if (string.IsNullOrWhiteSpace(request.UploadId))
            {
                return Results.Problem(
                    title: "Missing job source",
                    detail: "Provide an uploadId from POST /uploads.",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "about:blank");
            }

            var created = pipeline.CreateJob(request.UploadId, request.Options);
            if (created is null)
            {
                return Results.Problem(
                    title: "Upload not found",
                    detail: "Unknown uploadId.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            return Results.Json(new { jobId = created.JobId }, statusCode: StatusCodes.Status202Accepted);
        });

        MapFrozenJobSubset(api);

        api.MapGet("/jobs/{id}", (string id, PrivatePipelineService pipeline) =>
        {
            var status = pipeline.GetJobStatus(id);
            if (status is null)
            {
                return Results.Problem(
                    title: "Job not found",
                    detail: "Unknown job id.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            return Results.Json(status);
        });

        api.MapGet("/jobs/{id}/output.json", (string id, PrivatePipelineService pipeline) =>
        {
            if (!pipeline.HasJob(id))
            {
                return Results.Problem(
                    title: "Job not found",
                    detail: "Unknown job id.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            var output = pipeline.GetOutput(id);
            if (output is null)
            {
                return Results.Problem(
                    title: "Job not finished",
                    detail: "Output is available only for finished jobs.",
                    statusCode: StatusCodes.Status409Conflict,
                    type: "about:blank");
            }

            return Results.Json(output.Document);
        });

        api.MapGet("/jobs/{id}/output.md", (string id, PrivatePipelineService pipeline) =>
        {
            if (!pipeline.HasJob(id))
            {
                return Results.Problem(
                    title: "Job not found",
                    detail: "Unknown job id.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            var output = pipeline.GetOutput(id);
            if (output is null)
            {
                return Results.Problem(
                    title: "Job not finished",
                    detail: "Output is available only for finished jobs.",
                    statusCode: StatusCodes.Status409Conflict,
                    type: "about:blank");
            }

            return Results.Content(output.Markdown, "text/markdown; charset=utf-8", Encoding.UTF8);
        });

    }

    private static void MapFrozenJobSubset(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/jobs", (PrivatePipelineService pipeline) =>
        {
            var jobs = pipeline.ListJobs();
            return Results.Json(new { jobs });
        });

        routes.MapDelete("/jobs/{id}", (string id, PrivatePipelineService pipeline) =>
        {
            var deleted = pipeline.DeleteJob(id);
            if (!deleted)
            {
                return Results.Problem(
                    title: "Job not found",
                    detail: "Unknown job id.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            return Results.NoContent();
        });
    }
}
