using MdReel.Core.Domain;
using MdReel.Core.Pipeline.StageB;
using MdReel.Infrastructure;
using MdReel.Infrastructure.Telemetry;
using MdReel.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddMdreelGoogleCloudConsole(builder.Configuration);
builder.Services.AddMdreelOpenTelemetry(builder.Configuration, "mdreel-worker");
builder.Services.Configure<YouTubeGalleryRunnerOptions>(builder.Configuration.GetSection("YouTubeGalleryRunner"));
builder.Services.AddSingleton<ICostLedger, InMemoryCostLedger>();
builder.Services.AddSingleton<StageBRunner>();
builder.Services.AddSingleton<YouTubeInternalGalleryRunner>();
builder.Services.AddPipelineInfrastructure(
    builder.Configuration,
    Path.Combine(builder.Environment.ContentRootPath, ".local-state", "internal-object-storage"));
builder.Services.AddHostedService<HealthListener>();
builder.Services.AddHostedService<PipelineWorker>();

var host = builder.Build();
host.Run();
