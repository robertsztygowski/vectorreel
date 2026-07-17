using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MdReel.Worker;

public sealed class HealthListener(ILogger<HealthListener> logger) : BackgroundService
{
    private static readonly byte[] _okResponse = Encoding.ASCII.GetBytes(
        "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Length: 2\r\nConnection: close\r\n\r\nok");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var portValue = Environment.GetEnvironmentVariable("PORT");
        if (string.IsNullOrWhiteSpace(portValue))
        {
            logger.LogDebug("PORT is not set; health listener is disabled.");
            return;
        }

        if (!int.TryParse(portValue, NumberStyles.None, CultureInfo.InvariantCulture, out var port)
            || port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
        {
            logger.LogWarning("Invalid PORT value '{PortValue}'; health listener is disabled.", portValue);
            return;
        }

        var listener = new TcpListener(IPAddress.Any, port);
        try
        {
            listener.Start();
            logger.LogInformation("Health listener started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    logger.LogError(ex, "Health listener failed while accepting a connection.");
                    continue;
                }

                _ = HandleClientAsync(client, stoppingToken);
            }
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "Health listener could not bind to the configured PORT.");
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {
        using (client)
        {
            try
            {
                client.NoDelay = true;
                var stream = client.GetStream();
                var buffer = new byte[1024];
                using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                requestTimeout.CancelAfter(TimeSpan.FromSeconds(5));

                _ = await stream.ReadAsync(buffer, requestTimeout.Token);
                await stream.WriteAsync(_okResponse, stoppingToken);
                await stream.FlushAsync(stoppingToken);
                client.Client.Shutdown(SocketShutdown.Send);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (OperationCanceledException ex)
            {
                logger.LogDebug(ex, "Health check client did not send a request before the timeout.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.LogDebug(ex, "Health check client disconnected before the response was written.");
            }
            catch (IOException ex)
            {
                logger.LogDebug(ex, "Health check client disconnected before the response was written.");
            }
            catch (SocketException ex)
            {
                logger.LogDebug(ex, "Health check client disconnected before the response was written.");
            }
        }
    }
}
