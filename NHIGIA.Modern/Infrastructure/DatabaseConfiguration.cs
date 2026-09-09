using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NHIGIA.Modern.Infrastructure;

public static class DatabaseConfiguration
{
    private static readonly object LocalDbStartLock = new();
    private static readonly int[] LocalDbRetryDelays = [250, 500, 1000, 1500, 2000];

    public static string Resolve(IConfiguration configuration)
    {
        var raw = new[] { configuration["HRM_CONNECTION_STRING"], configuration.GetConnectionString("MainConnectionString") }
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        if (string.IsNullOrWhiteSpace(raw) || raw.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Configure HRM_CONNECTION_STRING or ConnectionStrings__MainConnectionString with the SQL Server connection string.");

        var connection = new SqlConnectionStringBuilder(raw);
        if (!OperatingSystem.IsWindows() && (connection.IntegratedSecurity || connection.DataSource.Contains("localdb", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Render requires a reachable SQL Server host and SQL authentication; LocalDB/Windows authentication is unavailable.");
        return connection.ConnectionString;
    }

    public static SqlConnection OpenConnection(IConfiguration configuration)
    {
        var connectionString = Resolve(configuration);
        var connection = new SqlConnection(connectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch (SqlException) when (TryGetLocalDbInstance(connectionString, out var instanceName))
        {
            connection.Dispose();

            lock (LocalDbStartLock)
            {
                TryStartLocalDb(instanceName);

                SqlException lastError = null;
                foreach (var delay in LocalDbRetryDelays)
                {
                    Thread.Sleep(delay);
                    SqlConnection.ClearAllPools();
                    connection = new SqlConnection(connectionString);
                    try
                    {
                        connection.Open();
                        return connection;
                    }
                    catch (SqlException error)
                    {
                        lastError = error;
                        connection.Dispose();
                    }
                }

                throw new InvalidOperationException(
                    $"Không thể kết nối SQL LocalDB instance {instanceName} sau nhiều lần thử. " +
                    "Hãy kiểm tra dịch vụ LocalDB và cơ sở dữ liệu DEV_NHIGIA.",
                    lastError);
            }
        }
    }

    private static bool TryGetLocalDbInstance(string connectionString, out string instanceName)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var match = Regex.Match(builder.DataSource ?? string.Empty, @"^\(localdb\)\\(?<name>[A-Za-z0-9_-]+)$", RegexOptions.IgnoreCase);
        instanceName = match.Success ? match.Groups["name"].Value : null;
        return match.Success;
    }

    private static void TryStartLocalDb(string instanceName)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sqllocaldb.exe",
                Arguments = $"start \"{instanceName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process is null)
                return;

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (!process.WaitForExit(15000))
                process.Kill(true);
        }
        catch
        {
            // The connection retries below provide the authoritative result. The
            // LocalDB command can fail transiently while another request starts it.
        }
    }
}
