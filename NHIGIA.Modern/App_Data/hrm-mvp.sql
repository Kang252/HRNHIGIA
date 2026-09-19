SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;


IF OBJECT_ID('dbo.HrmDepartment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmDepartment (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmDepartment PRIMARY KEY,
        Code NVARCHAR(30) NOT NULL CONSTRAINT UQ_HrmDepartment_Code UNIQUE,
        Name NVARCHAR(150) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_HrmDepartment_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmDepartment_CreatedAt DEFAULT (SYSDATETIME())
    );
END;
GO

IF OBJECT_ID('dbo.HrmUserAccount', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmUserAccount (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmUserAccount PRIMARY KEY,
        Username NVARCHAR(80) NOT NULL CONSTRAINT UQ_HrmUserAccount_Username UNIQUE,
        PasswordHash NVARCHAR(100) NOT NULL,
        PasswordSalt NVARCHAR(100) NOT NULL,
        DisplayName NVARCHAR(150) NOT NULL,
        RoleCode NVARCHAR(30) NOT NULL,
        DepartmentId INT NULL,
        SupervisorUserId INT NULL,
        EmployeeInformationId INT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_HrmUserAccount_IsActive DEFAULT (1),
        LastLoginAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmUserAccount_CreatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmUserAccount_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.HrmDepartment(Id),
        CONSTRAINT FK_HrmUserAccount_Supervisor FOREIGN KEY (SupervisorUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
END;
GO

IF OBJECT_ID('dbo.HrmEmployeeProfile', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmEmployeeProfile (
        UserId INT NOT NULL CONSTRAINT PK_HrmEmployeeProfile PRIMARY KEY,
        EmployeeCode NVARCHAR(30) NOT NULL CONSTRAINT UQ_HrmEmployeeProfile_EmployeeCode UNIQUE,
        AvatarUrl NVARCHAR(500) NULL,
        AvatarContent VARBINARY(MAX) NULL,
        AvatarContentType NVARCHAR(100) NULL,
        Gender NVARCHAR(20) NULL,
        DateOfBirth DATE NULL,
        PlaceOfBirth NVARCHAR(250) NULL,
        Nationality NVARCHAR(100) NULL,
        Ethnicity NVARCHAR(100) NULL,
        Religion NVARCHAR(100) NULL,
        MaritalStatus NVARCHAR(50) NULL,
        MobilePhone NVARCHAR(30) NULL,
        OfficePhone NVARCHAR(30) NULL,
        HomePhone NVARCHAR(30) NULL,
        PersonalEmail NVARCHAR(150) NULL,
        CompanyEmail NVARCHAR(150) NULL,
        PermanentAddress NVARCHAR(500) NULL,
        CurrentAddress NVARCHAR(500) NULL,
        IdentityNumber NVARCHAR(30) NULL,
        IdentityIssuedDate DATE NULL,
        IdentityIssuedPlace NVARCHAR(250) NULL,
        IdentityExpiryDate DATE NULL,
        PassportNumber NVARCHAR(30) NULL,
        PassportIssuedDate DATE NULL,
        PassportIssuedPlace NVARCHAR(250) NULL,
        PassportExpiryDate DATE NULL,
        PersonalTaxCode NVARCHAR(30) NULL,
        JobTitle NVARCHAR(150) NULL,
        EmploymentStatus NVARCHAR(50) NULL,
        WorkLocation NVARCHAR(250) NULL,
        TimekeepingCode NVARCHAR(50) NULL,
        HireDate DATE NULL,
        ProbationDate DATE NULL,
        OfficialDate DATE NULL,
        ContractType NVARCHAR(100) NULL,
        ContractNumber NVARCHAR(50) NULL,
        ContractStartDate DATE NULL,
        ContractEndDate DATE NULL,
        AnnualLeaveDays DECIMAL(5,1) NULL,
        EducationLevel NVARCHAR(100) NULL,
        Degree NVARCHAR(150) NULL,
        SchoolName NVARCHAR(250) NULL,
        Faculty NVARCHAR(150) NULL,
        Major NVARCHAR(150) NULL,
        GraduationYear NVARCHAR(4) NULL,
        GraduationClassification NVARCHAR(100) NULL,
        BasicSalary NVARCHAR(50) NULL,
        BankAccountNumber NVARCHAR(50) NULL,
        BankName NVARCHAR(150) NULL,
        BankBranch NVARCHAR(150) NULL,
        SocialInsuranceNumber NVARCHAR(50) NULL,
        SocialInsuranceStartDate DATE NULL,
        HealthInsuranceNumber NVARCHAR(50) NULL,
        HealthInsuranceExpiryDate DATE NULL,
        RegisteredHealthFacility NVARCHAR(250) NULL,
        EmergencyContactName NVARCHAR(150) NULL,
        EmergencyContactRelationship NVARCHAR(100) NULL,
        EmergencyContactPhone NVARCHAR(30) NULL,
        EmergencyContactEmail NVARCHAR(150) NULL,
        EmergencyContactAddress NVARCHAR(500) NULL,
        Notes NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmEmployeeProfile_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_HrmEmployeeProfile_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
END;
GO

IF COL_LENGTH('dbo.HrmEmployeeProfile', 'AvatarContent') IS NULL
    ALTER TABLE dbo.HrmEmployeeProfile ADD AvatarContent VARBINARY(MAX) NULL;
GO

IF COL_LENGTH('dbo.HrmEmployeeProfile', 'AvatarContentType') IS NULL
    ALTER TABLE dbo.HrmEmployeeProfile ADD AvatarContentType NVARCHAR(100) NULL;
GO

IF OBJECT_ID('dbo.HrmShiftTemplate', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmShiftTemplate (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmShiftTemplate PRIMARY KEY,
        Code NVARCHAR(30) NOT NULL CONSTRAINT UQ_HrmShiftTemplate_Code UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        BreakMinutes INT NOT NULL CONSTRAINT DF_HrmShiftTemplate_Break DEFAULT (60),
        GraceMinutes INT NOT NULL CONSTRAINT DF_HrmShiftTemplate_Grace DEFAULT (5),
        IsOvernight BIT NOT NULL CONSTRAINT DF_HrmShiftTemplate_Overnight DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_HrmShiftTemplate_IsActive DEFAULT (1)
    );
END;
GO

IF OBJECT_ID('dbo.HrmEmployeeSchedule', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmEmployeeSchedule (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmEmployeeSchedule PRIMARY KEY,
        UserId INT NOT NULL,
        ShiftTemplateId INT NULL,
        ShiftName NVARCHAR(100) NOT NULL,
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        BreakMinutes INT NOT NULL CONSTRAINT DF_HrmEmployeeSchedule_Break DEFAULT (60),
        GraceMinutes INT NOT NULL CONSTRAINT DF_HrmEmployeeSchedule_Grace DEFAULT (5),
        WorkDaysMask INT NOT NULL CONSTRAINT DF_HrmEmployeeSchedule_WorkDaysMask DEFAULT (127),
        EffectiveFrom DATE NOT NULL,
        EffectiveTo DATE NULL,
        StatusCode NVARCHAR(30) NOT NULL CONSTRAINT DF_HrmEmployeeSchedule_Status DEFAULT ('ACTIVE'),
        CreatedByUserId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmEmployeeSchedule_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_HrmEmployeeSchedule_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmEmployeeSchedule_Shift FOREIGN KEY (ShiftTemplateId) REFERENCES dbo.HrmShiftTemplate(Id),
        CONSTRAINT FK_HrmEmployeeSchedule_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmEmployeeSchedule_UserDate ON dbo.HrmEmployeeSchedule(UserId, EffectiveFrom, EffectiveTo);
END;
GO

IF COL_LENGTH('dbo.HrmEmployeeSchedule', 'WorkDaysMask') IS NULL
    ALTER TABLE dbo.HrmEmployeeSchedule ADD WorkDaysMask INT NOT NULL
        CONSTRAINT DF_HrmEmployeeSchedule_WorkDaysMask DEFAULT (127) WITH VALUES;
GO

UPDATE dbo.HrmShiftTemplate SET GraceMinutes=0 WHERE GraceMinutes<>0;
UPDATE dbo.HrmEmployeeSchedule SET GraceMinutes=0 WHERE GraceMinutes<>0;
UPDATE dbo.HrmEmployeeSchedule SET ShiftName=N'Ca Thứ 7 buổi sáng'
WHERE WorkDaysMask=64 AND ShiftName=N'Ca nửa ngày Thứ 7';
GO

IF OBJECT_ID('dbo.HrmLeaveRequest', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmLeaveRequest (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmLeaveRequest PRIMARY KEY,
        RequestCode AS ('NP' + RIGHT('00000000' + CONVERT(VARCHAR(8), Id), 8)) PERSISTED,
        UserId INT NOT NULL,
        LeaveType NVARCHAR(80) NOT NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NOT NULL,
        SessionCode NVARCHAR(30) NOT NULL,
        HandoverTo NVARCHAR(150) NULL,
        Reason NVARCHAR(1000) NOT NULL,
        AttachmentName NVARCHAR(255) NULL,
        StatusCode NVARCHAR(30) NOT NULL CONSTRAINT DF_HrmLeaveRequest_Status DEFAULT ('PENDING_MANAGER'),
        ManagerNote NVARCHAR(1000) NULL,
        HrNote NVARCHAR(1000) NULL,
        ApprovedByManagerId INT NULL,
        ApprovedByHrId INT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmLeaveRequest_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT CK_HrmLeaveRequest_Dates CHECK (EndDate >= StartDate),
        CONSTRAINT FK_HrmLeaveRequest_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmLeaveRequest_Manager FOREIGN KEY (ApprovedByManagerId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmLeaveRequest_Hr FOREIGN KEY (ApprovedByHrId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmLeaveRequest_UserStatus ON dbo.HrmLeaveRequest(UserId, StatusCode, CreatedAt DESC);
END;
GO

IF COL_LENGTH('dbo.HrmLeaveRequest', 'AttachmentContentType') IS NULL
    ALTER TABLE dbo.HrmLeaveRequest ADD AttachmentContentType NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.HrmLeaveRequest', 'AttachmentContent') IS NULL
    ALTER TABLE dbo.HrmLeaveRequest ADD AttachmentContent VARBINARY(MAX) NULL;
GO

IF OBJECT_ID('dbo.HrmCommunication', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmCommunication (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmCommunication PRIMARY KEY,
        AuthorUserId INT NOT NULL,
        Category NVARCHAR(80) NOT NULL,
        ScopeCode NVARCHAR(30) NOT NULL,
        DepartmentId INT NULL,
        Title NVARCHAR(200) NOT NULL,
        Body NVARCHAR(MAX) NOT NULL,
        AttachmentName NVARCHAR(255) NULL,
        IsPinned BIT NOT NULL CONSTRAINT DF_HrmCommunication_Pinned DEFAULT (0),
        IsPublished BIT NOT NULL CONSTRAINT DF_HrmCommunication_Published DEFAULT (1),
        PublishedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmCommunication_PublishedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmCommunication_Author FOREIGN KEY (AuthorUserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmCommunication_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.HrmDepartment(Id)
    );
    CREATE INDEX IX_HrmCommunication_Published ON dbo.HrmCommunication(IsPublished, IsPinned DESC, PublishedAt DESC);
END;
GO

IF COL_LENGTH('dbo.HrmCommunication', 'AttachmentContentType') IS NULL
    ALTER TABLE dbo.HrmCommunication ADD AttachmentContentType NVARCHAR(100) NULL;
GO
IF COL_LENGTH('dbo.HrmCommunication', 'AttachmentContent') IS NULL
    ALTER TABLE dbo.HrmCommunication ADD AttachmentContent VARBINARY(MAX) NULL;
GO

IF COL_LENGTH('dbo.HrmCommunication', 'StatusCode') IS NULL
    ALTER TABLE dbo.HrmCommunication ADD StatusCode NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.HrmCommunication', 'SubmittedAt') IS NULL
    ALTER TABLE dbo.HrmCommunication ADD SubmittedAt DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.HrmCommunication', 'ApprovedByUserId') IS NULL
    ALTER TABLE dbo.HrmCommunication ADD ApprovedByUserId INT NULL;
GO
IF COL_LENGTH('dbo.HrmCommunication', 'ReviewedAt') IS NULL
    ALTER TABLE dbo.HrmCommunication ADD ReviewedAt DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.HrmCommunication', 'ReviewNote') IS NULL
    ALTER TABLE dbo.HrmCommunication ADD ReviewNote NVARCHAR(1000) NULL;
GO
UPDATE dbo.HrmCommunication SET StatusCode=CASE WHEN IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END WHERE StatusCode IS NULL;
UPDATE dbo.HrmCommunication SET SubmittedAt=PublishedAt WHERE SubmittedAt IS NULL;
GO

IF OBJECT_ID('dbo.HrmCommunicationReaction', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmCommunicationReaction (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmCommunicationReaction PRIMARY KEY,
        CommunicationId INT NOT NULL,
        UserId INT NOT NULL,
        ReactionCode NVARCHAR(20) NOT NULL CONSTRAINT DF_HrmCommunicationReaction_Code DEFAULT ('LIKE'),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmCommunicationReaction_Created DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmCommunicationReaction_Post FOREIGN KEY (CommunicationId) REFERENCES dbo.HrmCommunication(Id),
        CONSTRAINT FK_HrmCommunicationReaction_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT UQ_HrmCommunicationReaction_PostUser UNIQUE (CommunicationId, UserId)
    );
END;
GO

IF OBJECT_ID('dbo.HrmCommunicationComment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmCommunicationComment (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmCommunicationComment PRIMARY KEY,
        CommunicationId INT NOT NULL,
        AuthorUserId INT NOT NULL,
        Body NVARCHAR(1500) NOT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_HrmCommunicationComment_Deleted DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmCommunicationComment_Created DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmCommunicationComment_Post FOREIGN KEY (CommunicationId) REFERENCES dbo.HrmCommunication(Id),
        CONSTRAINT FK_HrmCommunicationComment_Author FOREIGN KEY (AuthorUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmCommunicationComment_Post ON dbo.HrmCommunicationComment(CommunicationId, CreatedAt);
END;
GO

IF OBJECT_ID('dbo.HrmHanetSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmHanetSettings (
        Id INT NOT NULL CONSTRAINT PK_HrmHanetSettings PRIMARY KEY CONSTRAINT CK_HrmHanetSettings_Single CHECK (Id = 1),
        ApiBaseUrl NVARCHAR(300) NULL,
        OAuthTokenUrl NVARCHAR(300) NULL,
        ClientId NVARCHAR(300) NULL,
        ProtectedClientSecret NVARCHAR(MAX) NULL,
        ProtectedAccessToken NVARCHAR(MAX) NULL,
        PlaceId NVARCHAR(100) NULL,
        WebhookSecret NVARCHAR(200) NULL,
        IsEnabled BIT NOT NULL CONSTRAINT DF_HrmHanetSettings_Enabled DEFAULT (0),
        LastSyncAt DATETIME2 NULL,
        LastSyncStatus NVARCHAR(30) NULL,
        LastSyncMessage NVARCHAR(1000) NULL,
        UpdatedByUserId INT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_HrmHanetSettings_User FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
END;
GO

IF OBJECT_ID('dbo.HrmHanetPersonMap', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmHanetPersonMap (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmHanetPersonMap PRIMARY KEY,
        UserId INT NOT NULL CONSTRAINT UQ_HrmHanetPersonMap_User UNIQUE,
        AliasId NVARCHAR(150) NULL,
        PersonId NVARCHAR(150) NULL,
        PlaceId NVARCHAR(100) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_HrmHanetPersonMap_Active DEFAULT (1),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmHanetPersonMap_UpdatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmHanetPersonMap_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE UNIQUE INDEX UX_HrmHanetPersonMap_Person ON dbo.HrmHanetPersonMap(PersonId) WHERE PersonId IS NOT NULL;
END;
GO

IF OBJECT_ID('dbo.HrmAttendanceEvent', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmAttendanceEvent (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmAttendanceEvent PRIMARY KEY,
        EventKey NVARCHAR(200) NOT NULL CONSTRAINT UQ_HrmAttendanceEvent_Key UNIQUE,
        UserId INT NULL,
        PersonId NVARCHAR(150) NULL,
        AliasId NVARCHAR(150) NULL,
        PlaceId NVARCHAR(100) NULL,
        DeviceId NVARCHAR(150) NULL,
        CheckTime DATETIME2 NOT NULL,
        EventType NVARCHAR(80) NULL,
        PayloadJson NVARCHAR(MAX) NOT NULL,
        ReceivedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmAttendanceEvent_ReceivedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmAttendanceEvent_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmAttendanceEvent_UserTime ON dbo.HrmAttendanceEvent(UserId, CheckTime);
END;
GO

IF OBJECT_ID('dbo.HrmAuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmAuditLog (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmAuditLog PRIMARY KEY,
        UserId INT NULL,
        ActionCode NVARCHAR(80) NOT NULL,
        EntityType NVARCHAR(80) NOT NULL,
        EntityId NVARCHAR(80) NULL,
        Detail NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(80) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmAuditLog_CreatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmAuditLog_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmAuditLog_CreatedAt ON dbo.HrmAuditLog(CreatedAt DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'BOD') INSERT dbo.HrmDepartment(Code, Name) VALUES ('BOD', N'Ban Giám đốc');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'HR') INSERT dbo.HrmDepartment(Code, Name) VALUES ('HR', N'Phòng Nhân sự');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'IT') INSERT dbo.HrmDepartment(Code, Name) VALUES ('IT', N'Phòng Công nghệ thông tin');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'QLTS') INSERT dbo.HrmDepartment(Code, Name) VALUES ('QLTS', N'Phòng Ban Quản Lý Tài Sản');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'TTNB') INSERT dbo.HrmDepartment(Code, Name) VALUES ('TTNB', N'Phòng Ban Truyền Thông Nội Bộ');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'BH_CU') INSERT dbo.HrmDepartment(Code, Name) VALUES ('BH_CU', N'Phòng Ban Bán Hàng và Cung Ứng');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'DA_TT') INSERT dbo.HrmDepartment(Code, Name) VALUES ('DA_TT', N'Phòng Dự Án Thông Tin');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'CSKH') INSERT dbo.HrmDepartment(Code, Name) VALUES ('CSKH', N'Phòng Ban Hỗ Trợ Khách Hàng');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HrmUserAccount WHERE Username = 'admin')
    INSERT dbo.HrmUserAccount(Username, PasswordHash, PasswordSalt, DisplayName, RoleCode, DepartmentId) VALUES ('admin', 'pk1cwHXQdt3ry5KAkgYRFHl0jZUaMjCZs0nNryLylQk=', '2MaORRli84/Q6iH0rfGqSg==', N'Quản trị hệ thống', 'ADMIN', (SELECT Id FROM dbo.HrmDepartment WHERE Code='IT'));
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HrmEmployeeProfile p INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId WHERE u.Username='admin')
    INSERT dbo.HrmEmployeeProfile(UserId, EmployeeCode, JobTitle, EmploymentStatus, CompanyEmail, TimekeepingCode, AnnualLeaveDays)
    SELECT Id, 'NG000', N'Quản trị hệ thống', N'Đang làm việc', 'admin@nhigia.local', 'NG000', 12 FROM dbo.HrmUserAccount WHERE Username='admin';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HrmShiftTemplate WHERE Code='HC') INSERT dbo.HrmShiftTemplate(Code, Name, StartTime, EndTime, BreakMinutes, GraceMinutes, IsOvernight) VALUES ('HC', N'Ca hành chính', '08:00', '17:30', 60, 0, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.HrmShiftTemplate WHERE Code='SANG') INSERT dbo.HrmShiftTemplate(Code, Name, StartTime, EndTime, BreakMinutes, GraceMinutes, IsOvernight) VALUES ('SANG', N'Ca Thứ 7 buổi sáng', '08:00', '12:00', 0, 0, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.HrmShiftTemplate WHERE Code='DEM') INSERT dbo.HrmShiftTemplate(Code, Name, StartTime, EndTime, BreakMinutes, GraceMinutes, IsOvernight) VALUES ('DEM', N'Ca Thứ 7 buổi chiều', '13:30', '17:30', 0, 0, 0);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HrmHanetSettings WHERE Id=1) INSERT dbo.HrmHanetSettings(Id, ApiBaseUrl, OAuthTokenUrl, IsEnabled, LastSyncStatus, LastSyncMessage) VALUES (1, 'https://partner.hanet.ai', 'https://oauth.hanet.com/token', 0, 'NOT_CONFIGURED', N'Chưa cấu hình thông tin ứng dụng HANET');
GO

-- Repair seed labels that were previously imported by sqlcmd with the wrong
-- input code page. Only known seed rows containing mojibake markers are changed.
UPDATE dbo.HrmDepartment
SET Name = CASE Code
    WHEN 'BOD' THEN N'Ban Giám đốc'
    WHEN 'HR' THEN N'Phòng Nhân sự'
    WHEN 'IT' THEN N'Phòng Công nghệ thông tin'
END
WHERE Code IN ('BOD', 'HR', 'IT')
  AND (Name LIKE N'%Ã%' OR Name LIKE N'%Æ%' OR Name LIKE N'%Ä%' OR Name LIKE N'%º%' OR Name LIKE N'%»%');

UPDATE dbo.HrmUserAccount
SET DisplayName = CASE Username
    WHEN 'admin' THEN N'Quản trị hệ thống'
    WHEN 'hradmin' THEN N'Quản trị nhân sự'
    WHEN 'thedt' THEN N'Giám đốc'
    WHEN 'huongtm' THEN N'Trưởng phòng'
    WHEN 'anhvt' THEN N'Nguyễn Văn A'
END
WHERE Username IN ('admin', 'hradmin', 'thedt', 'huongtm', 'anhvt')
  AND (DisplayName LIKE N'%Ã%' OR DisplayName LIKE N'%Æ%' OR DisplayName LIKE N'%Ä%' OR DisplayName LIKE N'%º%' OR DisplayName LIKE N'%»%');

UPDATE p
SET JobTitle = CASE u.Username
        WHEN 'admin' THEN N'Quản trị hệ thống'
        WHEN 'hradmin' THEN N'Chuyên viên nhân sự'
        WHEN 'thedt' THEN N'Giám đốc'
        WHEN 'huongtm' THEN N'Trưởng phòng'
        WHEN 'anhvt' THEN N'Nhân viên'
    END,
    EmploymentStatus = N'Đang làm việc'
FROM dbo.HrmEmployeeProfile p
INNER JOIN dbo.HrmUserAccount u ON u.Id = p.UserId
WHERE u.Username IN ('admin', 'hradmin', 'thedt', 'huongtm', 'anhvt')
  AND (p.JobTitle LIKE N'%Ã%' OR p.JobTitle LIKE N'%Æ%' OR p.JobTitle LIKE N'%Ä%' OR p.JobTitle LIKE N'%º%' OR p.JobTitle LIKE N'%»%'
       OR p.EmploymentStatus LIKE N'%Ã%' OR p.EmploymentStatus LIKE N'%Æ%' OR p.EmploymentStatus LIKE N'%Ä%' OR p.EmploymentStatus LIKE N'%º%' OR p.EmploymentStatus LIKE N'%»%');

UPDATE dbo.HrmShiftTemplate
SET Name = CASE Code
    WHEN 'HC' THEN N'Ca hành chính'
    WHEN 'SANG' THEN N'Ca Thứ 7 buổi sáng'
    WHEN 'DEM' THEN N'Ca Thứ 7 buổi chiều'
END,
StartTime = CASE Code WHEN 'HC' THEN '08:00' WHEN 'SANG' THEN '08:00' WHEN 'DEM' THEN '13:30' END,
EndTime = CASE Code WHEN 'HC' THEN '17:30' WHEN 'SANG' THEN '12:00' WHEN 'DEM' THEN '17:30' END,
BreakMinutes = CASE Code WHEN 'HC' THEN 60 ELSE 0 END,
GraceMinutes = 0,
IsOvernight = 0
WHERE Code IN ('HC', 'SANG', 'DEM');

UPDATE dbo.HrmHanetSettings
SET LastSyncMessage = N'Chưa cấu hình thông tin ứng dụng HANET'
WHERE Id = 1 AND LastSyncStatus = 'NOT_CONFIGURED'
  AND (LastSyncMessage LIKE N'%Ã%' OR LastSyncMessage LIKE N'%Æ%' OR LastSyncMessage LIKE N'%Ä%' OR LastSyncMessage LIKE N'%º%' OR LastSyncMessage LIKE N'%»%');
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID('dbo.HrmWorkItem', 'U') IS NULL
BEGIN
 CREATE TABLE dbo.HrmWorkItem (
  Id INT IDENTITY PRIMARY KEY,
  Kind NVARCHAR(20) NOT NULL CHECK (Kind IN ('kpi','payroll','recruitment','recruitment-plan','recruitment-round','talent-pool','walkin-profile','candidate-intake','training','overtime','resignation','transfer','transfer-decision','assets','helpdesk')),
  Title NVARCHAR(200) NOT NULL, Description NVARCHAR(2000) NULL,
  Category NVARCHAR(100) NULL, Reference NVARCHAR(100) NULL,
  WorkLocation NVARCHAR(250) NULL, JobLevel NVARCHAR(100) NULL, ExperienceRequired NVARCHAR(100) NULL,
  EducationRequired NVARCHAR(100) NULL, GenderRequirement NVARCHAR(50) NULL, AgeRange NVARCHAR(50) NULL,
  SalaryRange NVARCHAR(100) NULL, SkillRequirements NVARCHAR(2000) NULL, Benefits NVARCHAR(2000) NULL,
  RecruitmentProcess NVARCHAR(500) NULL,
  RecruitmentReason NVARCHAR(250) NULL, StartDate DATE NULL, ContractType NVARCHAR(100) NULL,
  ProbationPeriod NVARCHAR(100) NULL, RecruitmentChannel NVARCHAR(150) NULL, ContactEmail NVARCHAR(150) NULL,
  ContactPhone NVARCHAR(30) NULL, ContactName NVARCHAR(150) NULL, ContactAddress NVARCHAR(500) NULL, Keywords NVARCHAR(500) NULL,
  EmployeeId INT NULL REFERENCES dbo.HrmUserAccount(Id),
  DepartmentId INT NULL REFERENCES dbo.HrmDepartment(Id),
  DueDate DATE NULL, Target DECIMAL(19,4) NULL, Actual DECIMAL(19,4) NULL,
  Weight DECIMAL(5,2) NULL CHECK (Weight BETWEEN 1 AND 100),
  Priority NVARCHAR(20) NOT NULL DEFAULT 'NORMAL', Status NVARCHAR(30) NOT NULL,
  CreatedBy INT NOT NULL REFERENCES dbo.HrmUserAccount(Id),
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
 );
 CREATE INDEX IX_HrmWorkItem_Kind ON dbo.HrmWorkItem(Kind,CreatedAt);
 CREATE UNIQUE INDEX UX_HrmWorkItem_AssetReference ON dbo.HrmWorkItem(Reference) WHERE Kind='assets' AND Reference IS NOT NULL;
END;
GO

DECLARE @workKindConstraint SYSNAME;
DECLARE @dropWorkKindConstraintSql NVARCHAR(500);
SELECT TOP (1) @workKindConstraint = cc.name
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID('dbo.HrmWorkItem')
  AND cc.definition LIKE '%Kind%';
IF @workKindConstraint IS NOT NULL
BEGIN
    SET @dropWorkKindConstraintSql = N'ALTER TABLE dbo.HrmWorkItem DROP CONSTRAINT ' + QUOTENAME(@workKindConstraint);
    EXEC sys.sp_executesql @dropWorkKindConstraintSql;
END;
IF OBJECT_ID('dbo.CK_HrmWorkItem_Kind', 'C') IS NULL
    ALTER TABLE dbo.HrmWorkItem WITH CHECK ADD CONSTRAINT CK_HrmWorkItem_Kind
    CHECK (Kind IN ('kpi','payroll','payroll-allowance','payroll-deduction','payroll-advance','recruitment','recruitment-plan','recruitment-round','talent-pool','walkin-profile','candidate-intake','training','overtime','resignation','transfer','transfer-decision','assets','asset-handover','helpdesk','vehicle','meeting','business-trip','offboarding'));
GO

IF COL_LENGTH('dbo.HrmWorkItem', 'UpdatedAt') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD UpdatedAt DATETIME2 NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'LastActionNote') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD LastActionNote NVARCHAR(1000) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'WorkLocation') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD WorkLocation NVARCHAR(250) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'JobLevel') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD JobLevel NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'ExperienceRequired') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD ExperienceRequired NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'EducationRequired') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD EducationRequired NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'GenderRequirement') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD GenderRequirement NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'AgeRange') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD AgeRange NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'SalaryRange') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD SalaryRange NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'SkillRequirements') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD SkillRequirements NVARCHAR(2000) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'Benefits') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD Benefits NVARCHAR(2000) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'RecruitmentProcess') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD RecruitmentProcess NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'RecruitmentReason') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD RecruitmentReason NVARCHAR(250) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'StartDate') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD StartDate DATE NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'ContractType') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD ContractType NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'ProbationPeriod') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD ProbationPeriod NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'RecruitmentChannel') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD RecruitmentChannel NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'ContactEmail') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD ContactEmail NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'ContactPhone') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD ContactPhone NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'ContactName') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD ContactName NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'ContactAddress') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD ContactAddress NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'Keywords') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD Keywords NVARCHAR(MAX) NULL;
ELSE
    ALTER TABLE dbo.HrmWorkItem ALTER COLUMN Keywords NVARCHAR(MAX) NULL;
GO

IF COL_LENGTH('dbo.HrmWorkItem', 'StartAt') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD StartAt DATETIME2 NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'EndAt') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD EndAt DATETIME2 NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'Location') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD Location NVARCHAR(250) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'Destination') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD Destination NVARCHAR(250) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'AssetInUse') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD AssetInUse INT NOT NULL CONSTRAINT DF_HrmWorkItem_AssetInUse DEFAULT (0);
IF COL_LENGTH('dbo.HrmWorkItem', 'AssetMaintenance') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD AssetMaintenance INT NOT NULL CONSTRAINT DF_HrmWorkItem_AssetMaintenance DEFAULT (0);
IF COL_LENGTH('dbo.HrmWorkItem', 'AssetLost') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD AssetLost INT NOT NULL CONSTRAINT DF_HrmWorkItem_AssetLost DEFAULT (0);
IF COL_LENGTH('dbo.HrmWorkItem', 'AssetDisposed') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD AssetDisposed INT NOT NULL CONSTRAINT DF_HrmWorkItem_AssetDisposed DEFAULT (0);
IF COL_LENGTH('dbo.HrmWorkItem', 'AssetDamaged') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD AssetDamaged INT NOT NULL CONSTRAINT DF_HrmWorkItem_AssetDamaged DEFAULT (0);
IF COL_LENGTH('dbo.HrmWorkItem', 'KpiType') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD KpiType NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'Quarter') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD Quarter NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.HrmWorkItem', 'ProofNote') IS NULL
    ALTER TABLE dbo.HrmWorkItem ADD ProofNote NVARCHAR(2000) NULL;
GO

IF OBJECT_ID('dbo.HrmTrainingEnrollment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmTrainingEnrollment (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmTrainingEnrollment PRIMARY KEY,
        TrainingId INT NOT NULL,
        EmployeeId INT NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_HrmTrainingEnrollment_Status DEFAULT ('STUDYING'),
        ProgressPercent INT NOT NULL CONSTRAINT DF_HrmTrainingEnrollment_Progress DEFAULT (0),
        Score DECIMAL(5,2) NULL,
        EvaluationResult NVARCHAR(100) NULL,
        EvaluationNote NVARCHAR(1000) NULL,
        CertificateNumber NVARCHAR(100) NULL,
        CertificateIssuedAt DATETIME2 NULL,
        EnrolledAt DATETIME2 NOT NULL CONSTRAINT DF_HrmTrainingEnrollment_EnrolledAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_HrmTrainingEnrollment_TrainingEmployee UNIQUE (TrainingId, EmployeeId),
        CONSTRAINT CK_HrmTrainingEnrollment_Progress CHECK (ProgressPercent BETWEEN 0 AND 100),
        CONSTRAINT FK_HrmTrainingEnrollment_Training FOREIGN KEY (TrainingId) REFERENCES dbo.HrmWorkItem(Id) ON DELETE CASCADE,
        CONSTRAINT FK_HrmTrainingEnrollment_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmTrainingEnrollment_Employee ON dbo.HrmTrainingEnrollment(EmployeeId, Status, EnrolledAt DESC);
END;
GO

IF OBJECT_ID('dbo.HrmWorkItemParticipant', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmWorkItemParticipant (
        WorkItemId INT NOT NULL,
        UserId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmWorkItemParticipant_CreatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_HrmWorkItemParticipant PRIMARY KEY (WorkItemId, UserId),
        CONSTRAINT FK_HrmWorkItemParticipant_WorkItem FOREIGN KEY (WorkItemId) REFERENCES dbo.HrmWorkItem(Id) ON DELETE CASCADE,
        CONSTRAINT FK_HrmWorkItemParticipant_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmWorkItemParticipant_User ON dbo.HrmWorkItemParticipant(UserId, WorkItemId);
END;
GO

IF OBJECT_ID('dbo.HrmNotification', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmNotification (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmNotification PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        LinkUrl NVARCHAR(500) NULL,
        IsRead BIT NOT NULL CONSTRAINT DF_HrmNotification_IsRead DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmNotification_CreatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmNotification_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmNotification_User ON dbo.HrmNotification(UserId, IsRead, CreatedAt DESC);
END;
GO

-- HRM device inventory; does not change camera configuration on HANET.
IF OBJECT_ID('dbo.HrmHanetDevice','U') IS NULL
BEGIN
 CREATE TABLE dbo.HrmHanetDevice (
 Id INT IDENTITY PRIMARY KEY, DeviceId NVARCHAR(100) NOT NULL UNIQUE,
 Name NVARCHAR(150) NOT NULL, PlaceId NVARCHAR(100) NOT NULL,
 Location NVARCHAR(250) NULL, Notes NVARCHAR(1000) NULL,
 IsActive BIT NOT NULL DEFAULT 1, UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME());
END;
GO
