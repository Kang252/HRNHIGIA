using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NHIGIA.Modern.Infrastructure;

public static class DatabaseConfiguration
{
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
            StartLocalDb(instanceName);
            SqlConnection.ClearAllPools();
            connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }

    private static bool TryGetLocalDbInstance(string connectionString, out string instanceName)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var match = Regex.Match(builder.DataSource ?? string.Empty, @"^\(localdb\)\\(?<name>[A-Za-z0-9_-]+)$", RegexOptions.IgnoreCase);
        instanceName = match.Success ? match.Groups["name"].Value : null;
        return match.Success;
    }

    private static void StartLocalDb(string instanceName)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "sqllocaldb.exe",
            Arguments = $"start \"{instanceName}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }) ?? throw new InvalidOperationException("Không thể chạy SqlLocalDB.exe.");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        if (!process.WaitForExit(15000))
        {
            process.Kill(true);
            throw new InvalidOperationException("SQL LocalDB không phản hồi trong 15 giây.");
        }
        if (process.ExitCode != 0)
            throw new InvalidOperationException("Không thể khởi động SQL LocalDB: " + (error + output).Trim());
    }
}
