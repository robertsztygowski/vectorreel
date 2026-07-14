var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Vertical slices land under Features/ from Phase 3 (DEVELOPMENT.md §2). Until then the API is a
// shell: Phase 1 is Stage A, which the Worker drives and which needs no HTTP surface of its own.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
