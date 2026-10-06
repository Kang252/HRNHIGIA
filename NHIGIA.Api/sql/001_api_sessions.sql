-- NHIGIA.Api additive migration. Apply once to the same SQL Server database
-- used by NHIGIA.Modern. This script does not alter existing accounts or data.

IF OBJECT_ID('dbo.HrmApiSession', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmApiSession (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmApiSession PRIMARY KEY,
        TokenHash CHAR(64) NOT NULL CONSTRAINT UQ_HrmApiSession_TokenHash UNIQUE,
        UserId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmApiSession_CreatedAt DEFAULT SYSUTCDATETIME(),
        LastSeenAt DATETIME2 NULL,
        ExpiresAt DATETIME2 NOT NULL,
        RevokedAt DATETIME2 NULL,
        IpAddress NVARCHAR(80) NULL,
        UserAgentHash CHAR(64) NULL,
        CONSTRAINT FK_HrmApiSession_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HrmApiSession_Active' AND object_id = OBJECT_ID('dbo.HrmApiSession'))
BEGIN
    CREATE INDEX IX_HrmApiSession_Active
        ON dbo.HrmApiSession(TokenHash, ExpiresAt)
        INCLUDE (UserId, RevokedAt);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HrmApiSession_User' AND object_id = OBJECT_ID('dbo.HrmApiSession'))
BEGIN
    CREATE INDEX IX_HrmApiSession_User
        ON dbo.HrmApiSession(UserId, ExpiresAt DESC);
END;
GO
