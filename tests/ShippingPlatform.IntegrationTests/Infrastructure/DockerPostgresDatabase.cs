using System.Diagnostics;
using System.Net.Sockets;
using Npgsql;

namespace ShippingPlatform.IntegrationTests.Infrastructure;

internal sealed class DockerPostgresDatabase : IAsyncDisposable
{
    private readonly string _containerName = $"shipping-platform-tests-{Guid.NewGuid():N}";
    private readonly int _hostPort = GetFreeTcpPort();
    private readonly string _databaseName = $"shipping_platform_tests_{Guid.NewGuid():N}";
    private bool _started;

    public string ConnectionString =>
        $"Host=127.0.0.1;Port={_hostPort};Database={_databaseName};Username=postgres;Password=postgres";

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await RunDockerCommandAsync(
            cancellationToken,
            "run",
            "--detach",
            "--rm",
            "--name",
            _containerName,
            "-e",
            $"POSTGRES_DB={_databaseName}",
            "-e",
            "POSTGRES_USER=postgres",
            "-e",
            "POSTGRES_PASSWORD=postgres",
            "-p",
            $"{_hostPort}:5432",
            "postgres:16");

        _started = true;

        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < TimeSpan.FromSeconds(60))
        {
            try
            {
                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync(cancellationToken);
                return;
            }
            catch when (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }

        throw new TimeoutException("PostgreSQL container did not become ready within 60 seconds.");
    }

    public async ValueTask DisposeAsync()
    {
        if (!_started)
        {
            return;
        }

        try
        {
            await RunDockerCommandAsync(CancellationToken.None, "rm", "-f", _containerName);
        }
        catch
        {
        }
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();

        try
        {
            return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static async Task RunDockerCommandAsync(
        CancellationToken cancellationToken,
        params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start docker process.");

        var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await standardOutput;
        var error = await standardError;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Docker command failed with exit code {process.ExitCode}.{Environment.NewLine}{output}{Environment.NewLine}{error}");
        }
    }
}
