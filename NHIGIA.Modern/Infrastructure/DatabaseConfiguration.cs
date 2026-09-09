using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NHIGIA.Modern.Infrastructure;

public static class DatabaseConfiguration
{
    private static readonly object LocalDbLock = new();
    private static string _localDbInstance;
    private static string _localDbPipe;

    public static string Resolve(IConfiguration configuration)
        => Resolve(configuration, false);

    public static SqlConnection OpenConnection(IConfiguration configuration)
    {
        var connection = new SqlConnection(Resolve(configuration));
        try
        {
            connection.Open();
            return connection;
        }
        catch (SqlException) when (IsLocalDb(configuration))
        {
            connection.Dispose();
            SqlConnection.ClearAllPools();
            connection = new SqlConnection(Resolve(configuration, true));
            connection.Open();
            return connection;
        }
    }

    private static string Resolve(IConfiguration configuration, bool refreshLocalDb)
    {
        var raw = GetRawConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(raw) || raw.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Configure HRM_CONNECTION_STRING or ConnectionStrings__MainConnectionString with the SQL Server connection string.");

        var connection = new SqlConnectionStringBuilder(raw);
        if (!OperatingSystem.IsWindows() && (connection.IntegratedSecurity || connection.DataSource.Contains("localdb", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Render requires a reachable SQL Server host and SQL authentication; LocalDB/Windows authentication is unavailable.");

        var localDb = Regex.Match(connection.DataSource ?? string.Empty, @"^\(localdb\)\\(?<name>[A-Za-z0-9_-]+)$", RegexOptions.IgnoreCase);
        if (localDb.Success)
            connection.DataSource = ResolveLocalDbPipe(localDb.Groups["name"].Value, refreshLocalDb);

        return connection.ConnectionString;
    }

    private static string GetRawConnectionString(IConfiguration configuration)
        => new[] { configuration["HRM_CONNECTION_STRING"], configuration.GetConnectionString("MainConnectionString") }
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static bool IsLocalDb(IConfiguration configuration)
    {
        var raw = GetRawConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var builder = new SqlConnectionStringBuilder(raw);
        return Regex.IsMatch(builder.DataSource ?? string.Empty, @"^\(localdb\)\\[A-Za-z0-9_-]+$", RegexOptions.IgnoreCase);
    }

    private static string ResolveLocalDbPipe(string instanceName, bool refresh)
    {
        lock (LocalDbLock)
        {
            if (!refresh && string.Equals(_localDbInstance, instanceName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_localDbPipe))
                return _localDbPipe;

            RunLocalDb($"start \"{instanceName}\"");
            for (var attempt = 0; attempt < 30; attempt++)
            {
                var info = RunLocalDb($"info \"{instanceName}\"");
                var pipe = Regex.Match(info, @"np:[^\r\n]+", RegexOptions.IgnoreCase);
                if (pipe.Success)
                {
                    _localDbInstance = instanceName;
                    _localDbPipe = pipe.Value.Trim();
                    return _localDbPipe;
                }
                Thread.Sleep(200);
            }

            throw new InvalidOperationException($"SQL LocalDB instance {instanceName} không cung cấp named pipe sau khi khởi động.");
        }
    }

    private static string RunLocalDb(string arguments)
    {
        var executable = FindLocalDbExecutable();
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }) ?? throw new InvalidOperationException($"Không thể chạy {executable}.");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        if (!process.WaitForExit(15000))
        {
            process.Kill(true);
            throw new InvalidOperationException("SQL LocalDB không phản hồi trong 15 giây.");
        }
        if (process.ExitCode != 0)
            throw new InvalidOperationException("Không thể điều khiển SQL LocalDB: " + error.Trim());
        return output;
    }

    private static string FindLocalDbExecutable()
    {
        var roots = new[]
        {
            Environment.GetEnvironmentVariable("ProgramW6432"),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        };
        foreach (var root in roots.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
        foreach (var version in new[] { "170", "160", "150", "140", "130" })
        {
            var candidate = Path.Combine(root, "Microsoft SQL Server", version, "Tools", "Binn", "SqlLocalDB.exe");
            if (File.Exists(candidate)) return candidate;
        }
        return "sqllocaldb.exe";
    }
}
