using System.Xml.Linq;
using Dapper;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace NHIGIA.Modern.Infrastructure;

public sealed class SqlDataProtectionKeyRepository : IXmlRepository
{
    private readonly IConfiguration _configuration;
    public SqlDataProtectionKeyRepository(IConfiguration configuration) => _configuration = configuration;

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var db = DatabaseConfiguration.OpenConnection(_configuration);
        EnsureTable(db);
        return db.Query<string>("SELECT Xml FROM dbo.HrmDataProtectionKey ORDER BY Id")
            .Select(XElement.Parse).ToList().AsReadOnly();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        using var db = DatabaseConfiguration.OpenConnection(_configuration);
        EnsureTable(db);
        db.Execute(@"INSERT dbo.HrmDataProtectionKey(FriendlyName,Xml)
            VALUES(@FriendlyName,@Xml)", new
        {
            FriendlyName = string.IsNullOrWhiteSpace(friendlyName) ? null : friendlyName,
            Xml = element.ToString(SaveOptions.DisableFormatting)
        });
    }

    private static void EnsureTable(Microsoft.Data.SqlClient.SqlConnection db) => db.Execute(@"
        IF OBJECT_ID('dbo.HrmDataProtectionKey','U') IS NULL
        BEGIN
            CREATE TABLE dbo.HrmDataProtectionKey(
                Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmDataProtectionKey PRIMARY KEY,
                FriendlyName NVARCHAR(200) NULL,
                Xml NVARCHAR(MAX) NOT NULL,
                CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmDataProtectionKey_CreatedAt DEFAULT(SYSUTCDATETIME())
            );
        END");
}
