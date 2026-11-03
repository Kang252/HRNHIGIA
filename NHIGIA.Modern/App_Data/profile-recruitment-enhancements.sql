SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.HrmRecruitmentCandidate','DateOfBirth') IS NULL
    ALTER TABLE dbo.HrmRecruitmentCandidate ADD DateOfBirth DATE NULL;
IF COL_LENGTH('dbo.HrmRecruitmentCandidate','Address') IS NULL
    ALTER TABLE dbo.HrmRecruitmentCandidate ADD Address NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.HrmRecruitmentCandidate','CurrentPosition') IS NULL
    ALTER TABLE dbo.HrmRecruitmentCandidate ADD CurrentPosition NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.HrmRecruitmentCandidate','Education') IS NULL
    ALTER TABLE dbo.HrmRecruitmentCandidate ADD Education NVARCHAR(2000) NULL;
IF COL_LENGTH('dbo.HrmRecruitmentCandidate','ExperienceSummary') IS NULL
    ALTER TABLE dbo.HrmRecruitmentCandidate ADD ExperienceSummary NVARCHAR(4000) NULL;
IF COL_LENGTH('dbo.HrmRecruitmentCandidate','Skills') IS NULL
    ALTER TABLE dbo.HrmRecruitmentCandidate ADD Skills NVARCHAR(2000) NULL;
IF COL_LENGTH('dbo.HrmRecruitmentCandidate','ExpectedSalary') IS NULL
    ALTER TABLE dbo.HrmRecruitmentCandidate ADD ExpectedSalary NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HrmRecruitmentIntegration WHERE ProviderCode='MANUAL')
BEGIN
    INSERT dbo.HrmRecruitmentIntegration(ProviderCode,ProviderName,WebhookSecret,IsEnabled,LastStatus,LastMessage)
    VALUES('MANUAL',N'Nhập CV thủ công',LOWER(REPLACE(CONVERT(varchar(36),NEWID()),'-','')),1,'READY',N'Sẵn sàng nhận tệp Word .docx');
END;
GO

IF OBJECT_ID('dbo.HrmRecruitmentImportDraft','U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmRecruitmentImportDraft(
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HrmRecruitmentImportDraft PRIMARY KEY,
        CreatedByUserId INT NOT NULL,
        RecruitmentWorkItemId INT NULL,
        FullName NVARCHAR(200) NULL,
        Email NVARCHAR(200) NULL,
        Phone NVARCHAR(50) NULL,
        DateOfBirth DATE NULL,
        Address NVARCHAR(500) NULL,
        CurrentPosition NVARCHAR(200) NULL,
        Education NVARCHAR(2000) NULL,
        ExperienceSummary NVARCHAR(4000) NULL,
        Skills NVARCHAR(2000) NULL,
        ExpectedSalary NVARCHAR(100) NULL,
        CvFileName NVARCHAR(255) NOT NULL,
        CvContentType NVARCHAR(150) NULL,
        CvContent VARBINARY(MAX) NOT NULL,
        ExtractedTextPreview NVARCHAR(4000) NULL,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmRecruitmentImportDraft_CreatedAt DEFAULT(SYSDATETIME()),
        CONSTRAINT FK_HrmRecruitmentImportDraft_User FOREIGN KEY(CreatedByUserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmRecruitmentImportDraft_WorkItem FOREIGN KEY(RecruitmentWorkItemId) REFERENCES dbo.HrmWorkItem(Id) ON DELETE SET NULL
    );
    CREATE INDEX IX_HrmRecruitmentImportDraft_Expiry ON dbo.HrmRecruitmentImportDraft(ExpiresAt);
END;
GO

IF OBJECT_ID('dbo.HrmEmployeeUniform','U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmEmployeeUniform(
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmEmployeeUniform PRIMARY KEY,
        UserId INT NOT NULL,
        ItemName NVARCHAR(150) NOT NULL,
        Size NVARCHAR(50) NULL,
        Quantity INT NOT NULL CONSTRAINT DF_HrmEmployeeUniform_Quantity DEFAULT(1),
        IssuedDate DATE NOT NULL,
        ReturnedDate DATE NULL,
        StatusCode NVARCHAR(30) NOT NULL CONSTRAINT DF_HrmEmployeeUniform_Status DEFAULT('ISSUED'),
        Notes NVARCHAR(500) NULL,
        CreatedByUserId INT NULL,
        UpdatedByUserId INT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmEmployeeUniform_CreatedAt DEFAULT(SYSDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT CK_HrmEmployeeUniform_Quantity CHECK(Quantity BETWEEN 1 AND 1000),
        CONSTRAINT CK_HrmEmployeeUniform_Status CHECK(StatusCode IN('ISSUED','RETURNED','LOST')),
        CONSTRAINT FK_HrmEmployeeUniform_User FOREIGN KEY(UserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmEmployeeUniform_CreatedBy FOREIGN KEY(CreatedByUserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmEmployeeUniform_UpdatedBy FOREIGN KEY(UpdatedByUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmEmployeeUniform_User ON dbo.HrmEmployeeUniform(UserId,IssuedDate DESC);
END;
GO

IF OBJECT_ID('dbo.HrmEmployeeSoftwareAccount','U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmEmployeeSoftwareAccount(
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmEmployeeSoftwareAccount PRIMARY KEY,
        UserId INT NOT NULL,
        SoftwareName NVARCHAR(150) NOT NULL,
        LoginUrl NVARCHAR(500) NULL,
        LoginMethod NVARCHAR(100) NULL,
        Username NVARCHAR(200) NULL,
        ProtectedPassword NVARCHAR(MAX) NULL,
        PermissionScope NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_HrmEmployeeSoftwareAccount_Active DEFAULT(1),
        CreatedByUserId INT NULL,
        UpdatedByUserId INT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmEmployeeSoftwareAccount_CreatedAt DEFAULT(SYSDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_HrmEmployeeSoftwareAccount_User FOREIGN KEY(UserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmEmployeeSoftwareAccount_CreatedBy FOREIGN KEY(CreatedByUserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmEmployeeSoftwareAccount_UpdatedBy FOREIGN KEY(UpdatedByUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmEmployeeSoftwareAccount_User ON dbo.HrmEmployeeSoftwareAccount(UserId,IsActive DESC,SoftwareName);
END;
GO

UPDATE dbo.HrmEmployeeProfile SET EmploymentStatus = CASE
    WHEN EmploymentStatus IN (N'Đã nghỉ việc',N'Đã thôi việc') THEN N'Đã thôi việc'
    WHEN EmploymentStatus=N'Thử việc' THEN N'Thử việc'
    WHEN EmploymentStatus=N'Thực tập sinh' OR JobTitle LIKE N'%thực tập%' THEN N'Thực tập sinh'
    ELSE N'Nhân viên chính thức'
END
WHERE EmploymentStatus IS NULL OR EmploymentStatus NOT IN (N'Nhân viên chính thức',N'Thử việc',N'Thực tập sinh',N'Đã thôi việc');
GO

UPDATE u SET IsActive=0,UpdatedAt=SYSDATETIME()
FROM dbo.HrmUserAccount u
INNER JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
WHERE p.EmploymentStatus=N'Đã thôi việc' AND u.IsActive=1;
GO
