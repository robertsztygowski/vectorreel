# mdreel Worker on Cloud Run (listens on Cloud Run PORT).
# ffmpeg/ffprobe are runtime dependencies: Stage A is pure local ffmpeg (ARCHITECTURE §3).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props global.json .editorconfig ./
COPY src/Core/Core.csproj src/Core/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Worker/Worker.csproj src/Worker/
RUN dotnet restore src/Worker/Worker.csproj
COPY src/Core/ src/Core/
COPY src/Infrastructure/ src/Infrastructure/
COPY src/Worker/ src/Worker/
RUN dotnet publish src/Worker/Worker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MdReel.Worker.dll"]
