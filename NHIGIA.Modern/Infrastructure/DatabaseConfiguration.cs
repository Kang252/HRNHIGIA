using Microsoft.Data.SqlClient;
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
}
