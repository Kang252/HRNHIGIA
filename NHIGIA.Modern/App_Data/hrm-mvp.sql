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
IF NOT EXISTS (SELECT 1 FROM dbo.HrmUserAccount WHERE Username = 'hradmin')
    INSERT dbo.HrmUserAccount(Username, PasswordHash, PasswordSalt, DisplayName, RoleCode, DepartmentId) VALUES ('hradmin', 'rsCoYHfrFJwzq9nE/4NOv7VN8z+hM8NpXCDpN4BKmA4=', 'Z8oXIpjIL5i+i3j4aeEcdw==', N'Quản trị nhân sự', 'HR', (SELECT Id FROM dbo.HrmDepartment WHERE Code='HR'));
IF NOT EXISTS (SELECT 1 FROM dbo.HrmUserAccount WHERE Username = 'thedt')
    INSERT dbo.HrmUserAccount(Username, PasswordHash, PasswordSalt, DisplayName, RoleCode, DepartmentId) VALUES ('thedt', 'QqYJ+5QanjYAHMbdMG2SgW6HWya/pOS7jSPMcc9pUSQ=', 'xWTA480bRGcjhIzLPN3gNg==', N'Giám đốc', 'DIRECTOR', (SELECT Id FROM dbo.HrmDepartment WHERE Code='BOD'));
IF NOT EXISTS (SELECT 1 FROM dbo.HrmUserAccount WHERE Username = 'huongtm')
    INSERT dbo.HrmUserAccount(Username, PasswordHash, PasswordSalt, DisplayName, RoleCode, DepartmentId) VALUES ('huongtm', 'dwjzkd1+uqTVlCYCrgRvfPhtl+9LJ8JLuJR2Z+GLodc=', 'SLgp7cdbMeklGBDGrauqbg==', N'Trưởng phòng', 'MANAGER', (SELECT Id FROM dbo.HrmDepartment WHERE Code='IT'));
IF NOT EXISTS (SELECT 1 FROM dbo.HrmUserAccount WHERE Username = 'anhvt')
    INSERT dbo.HrmUserAccount(Username, PasswordHash, PasswordSalt, DisplayName, RoleCode, DepartmentId, SupervisorUserId) VALUES ('anhvt', '1NVAHjhZ1bhmA5ZfU2t/qbNk4dyyl5/g9M7ybLNea18=', 'jl5wrk1kwMsd8JVs2oxcGQ==', N'Nguyễn Văn A', 'EMPLOYEE', (SELECT Id FROM dbo.HrmDepartment WHERE Code='IT'), (SELECT Id FROM dbo.HrmUserAccount WHERE Username='huongtm'));
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HrmEmployeeProfile p INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId WHERE u.Username='admin')
    INSERT dbo.HrmEmployeeProfile(UserId, EmployeeCode, JobTitle, EmploymentStatus, CompanyEmail, TimekeepingCode, AnnualLeaveDays)
    SELECT Id, 'NG000', N'Quản trị hệ thống', N'Đang làm việc', 'admin@nhigia.local', 'NG000', 12 FROM dbo.HrmUserAccount WHERE Username='admin';
IF NOT EXISTS (SELECT 1 FROM dbo.HrmEmployeeProfile p INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId WHERE u.Username='hradmin')
    INSERT dbo.HrmEmployeeProfile(UserId, EmployeeCode, JobTitle, EmploymentStatus, CompanyEmail, TimekeepingCode, AnnualLeaveDays)
    SELECT Id, 'NG001', N'Chuyên viên nhân sự', N'Đang làm việc', 'hradmin@nhigia.local', 'NG001', 12 FROM dbo.HrmUserAccount WHERE Username='hradmin';
IF NOT EXISTS (SELECT 1 FROM dbo.HrmEmployeeProfile p INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId WHERE u.Username='thedt')
    INSERT dbo.HrmEmployeeProfile(UserId, EmployeeCode, JobTitle, EmploymentStatus, CompanyEmail, TimekeepingCode, AnnualLeaveDays)
    SELECT Id, 'NG002', N'Giám đốc', N'Đang làm việc', 'thedt@nhigia.local', 'NG002', 12 FROM dbo.HrmUserAccount WHERE Username='thedt';
IF NOT EXISTS (SELECT 1 FROM dbo.HrmEmployeeProfile p INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId WHERE u.Username='huongtm')
    INSERT dbo.HrmEmployeeProfile(UserId, EmployeeCode, JobTitle, EmploymentStatus, CompanyEmail, TimekeepingCode, AnnualLeaveDays)
    SELECT Id, 'NG003', N'Trưởng phòng', N'Đang làm việc', 'huongtm@nhigia.local', 'NG003', 12 FROM dbo.HrmUserAccount WHERE Username='huongtm';
IF NOT EXISTS (SELECT 1 FROM dbo.HrmEmployeeProfile p INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId WHERE u.Username='anhvt')
    INSERT dbo.HrmEmployeeProfile(UserId, EmployeeCode, JobTitle, EmploymentStatus, CompanyEmail, TimekeepingCode, AnnualLeaveDays)
    SELECT Id, 'NG004', N'Nhân viên', N'Đang làm việc', 'anhvt@nhigia.local', 'NG004', 12 FROM dbo.HrmUserAccount WHERE Username='anhvt';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HrmShiftTemplate WHERE Code='HC') INSERT dbo.HrmShiftTemplate(Code, Name, StartTime, EndTime, BreakMinutes, GraceMinutes, IsOvernight) VALUES ('HC', N'Ca hành chính', '08:00', '17:00', 60, 5, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.HrmShiftTemplate WHERE Code='SANG') INSERT dbo.HrmShiftTemplate(Code, Name, StartTime, EndTime, BreakMinutes, GraceMinutes, IsOvernight) VALUES ('SANG', N'Ca sáng', '06:00', '14:00', 30, 5, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.HrmShiftTemplate WHERE Code='DEM') INSERT dbo.HrmShiftTemplate(Code, Name, StartTime, EndTime, BreakMinutes, GraceMinutes, IsOvernight) VALUES ('DEM', N'Ca qua đêm', '20:00', '05:00', 60, 10, 1);
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
    WHEN 'SANG' THEN N'Ca sáng'
    WHEN 'DEM' THEN N'Ca qua đêm'
END
WHERE Code IN ('HC', 'SANG', 'DEM')
  AND (Name LIKE N'%Ã%' OR Name LIKE N'%Æ%' OR Name LIKE N'%Ä%' OR Name LIKE N'%º%' OR Name LIKE N'%»%');

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

-- Twenty complete demo employee profiles for UI, search, scheduling and HANET mapping tests.
-- DEMO-prefixed identifiers keep these records separate from operational employee data.
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'SALES')
    INSERT dbo.HrmDepartment(Code, Name) VALUES ('SALES', N'Phòng Kinh doanh');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'ACC')
    INSERT dbo.HrmDepartment(Code, Name) VALUES ('ACC', N'Phòng Kế toán');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'MKT')
    INSERT dbo.HrmDepartment(Code, Name) VALUES ('MKT', N'Phòng Marketing');
IF NOT EXISTS (SELECT 1 FROM dbo.HrmDepartment WHERE Code = 'OPS')
    INSERT dbo.HrmDepartment(Code, Name) VALUES ('OPS', N'Phòng Vận hành');
GO

DECLARE @DemoEmployees TABLE
(
    Seq INT NOT NULL,
    Username NVARCHAR(80) NOT NULL,
    EmployeeCode NVARCHAR(30) NOT NULL,
    DisplayName NVARCHAR(150) NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    DateOfBirth DATE NOT NULL,
    PlaceOfBirth NVARCHAR(250) NOT NULL,
    DepartmentCode NVARCHAR(30) NOT NULL,
    JobTitle NVARCHAR(150) NOT NULL,
    MobilePhone NVARCHAR(30) NOT NULL,
    IdentityNumber NVARCHAR(30) NOT NULL,
    HireDate DATE NOT NULL,
    BasicSalary NVARCHAR(50) NOT NULL,
    EmergencyContactName NVARCHAR(150) NOT NULL
);

INSERT @DemoEmployees
    (Seq, Username, EmployeeCode, DisplayName, Gender, DateOfBirth, PlaceOfBirth, DepartmentCode,
     JobTitle, MobilePhone, IdentityNumber, HireDate, BasicSalary, EmergencyContactName)
VALUES
    (1,  'demo01', 'DEMO001', N'Nguyễn Minh Anh',  N'Nữ',  '1995-03-12', N'Hà Nội',         'HR',    N'Chuyên viên tuyển dụng',       '0901000001', '001095000001', '2021-04-05', '15000000', N'Nguyễn Văn Hùng'),
    (2,  'demo02', 'DEMO002', N'Trần Quốc Bảo',    N'Nam', '1992-07-21', N'Hải Phòng',      'IT',    N'Kỹ sư phần mềm',               '0901000002', '001092000002', '2020-08-10', '22000000', N'Trần Thị Hạnh'),
    (3,  'demo03', 'DEMO003', N'Lê Thu Hà',        N'Nữ',  '1996-11-08', N'Nam Định',       'ACC',   N'Kế toán viên',                 '0901000003', '001096000003', '2022-02-14', '16000000', N'Lê Văn Thành'),
    (4,  'demo04', 'DEMO004', N'Phạm Hoàng Long',  N'Nam', '1990-05-19', N'Thanh Hóa',      'SALES', N'Chuyên viên kinh doanh',       '0901000004', '001090000004', '2019-06-03', '18000000', N'Phạm Thị Lan'),
    (5,  'demo05', 'DEMO005', N'Võ Ngọc Mai',      N'Nữ',  '1997-01-25', N'Đà Nẵng',        'MKT',   N'Chuyên viên nội dung',          '0901000005', '001097000005', '2023-01-09', '14500000', N'Võ Minh Tuấn'),
    (6,  'demo06', 'DEMO006', N'Đặng Tuấn Kiệt',   N'Nam', '1993-09-14', N'Bắc Ninh',       'OPS',   N'Điều phối vận hành',            '0901000006', '001093000006', '2020-11-16', '17500000', N'Đặng Thu Hương'),
    (7,  'demo07', 'DEMO007', N'Bùi Khánh Linh',   N'Nữ',  '1998-04-30', N'Ninh Bình',      'HR',    N'Chuyên viên C&B',               '0901000007', '001098000007', '2023-07-03', '15500000', N'Bùi Văn Quang'),
    (8,  'demo08', 'DEMO008', N'Hoàng Đức Nam',    N'Nam', '1991-12-02', N'Hà Nội',         'IT',    N'Quản trị hệ thống',             '0901000008', '001091000008', '2018-09-17', '23000000', N'Hoàng Thị Mai'),
    (9,  'demo09', 'DEMO009', N'Đỗ Thanh Thảo',    N'Nữ',  '1994-06-17', N'Thái Bình',      'ACC',   N'Kế toán tổng hợp',              '0901000009', '001094000009', '2021-03-22', '19000000', N'Đỗ Quốc Trung'),
    (10, 'demo10', 'DEMO010', N'Hồ Gia Huy',       N'Nam', '1996-08-09', N'TP. Hồ Chí Minh','SALES', N'Chuyên viên phát triển đối tác','0901000010', '001096000010', '2022-05-09', '18500000', N'Hồ Ngọc Yến'),
    (11, 'demo11', 'DEMO011', N'Ngô Phương Uyên',  N'Nữ',  '1999-02-11', N'Huế',            'MKT',   N'Chuyên viên truyền thông',      '0901000011', '001099000011', '2024-01-08', '14000000', N'Ngô Văn Bình'),
    (12, 'demo12', 'DEMO012', N'Dương Thành Đạt',  N'Nam', '1992-10-27', N'Quảng Ninh',     'OPS',   N'Chuyên viên mua hàng',          '0901000012', '001092000012', '2020-02-03', '18000000', N'Dương Thị Nga'),
    (13, 'demo13', 'DEMO013', N'Lý Quỳnh Anh',     N'Nữ',  '1995-07-06', N'Lào Cai',        'HR',    N'Chuyên viên đào tạo',           '0901000013', '001095000013', '2021-10-11', '16000000', N'Lý Mạnh Cường'),
    (14, 'demo14', 'DEMO014', N'Mai Quốc Khánh',   N'Nam', '1989-03-29', N'Hà Nam',         'IT',    N'Kỹ sư hạ tầng',                '0901000014', '001089000014', '2017-05-15', '25000000', N'Mai Thu Hà'),
    (15, 'demo15', 'DEMO015', N'Tạ Minh Châu',     N'Nữ',  '1997-12-18', N'Hưng Yên',       'ACC',   N'Chuyên viên thanh toán',        '0901000015', '001097000015', '2022-08-01', '15500000', N'Tạ Văn Nam'),
    (16, 'demo16', 'DEMO016', N'Trịnh Anh Khoa',   N'Nam', '1994-09-03', N'Nghệ An',        'SALES', N'Chuyên viên chăm sóc khách hàng','0901000016','001094000016', '2021-01-18', '17000000', N'Trịnh Thị Hoa'),
    (17, 'demo17', 'DEMO017', N'Cao Mỹ Linh',      N'Nữ',  '1998-05-22', N'Hải Dương',      'MKT',   N'Chuyên viên thiết kế',          '0901000017', '001098000017', '2023-03-13', '16500000', N'Cao Đức Thắng'),
    (18, 'demo18', 'DEMO018', N'Huỳnh Nhật Minh',  N'Nam', '1993-01-16', N'Quảng Nam',      'OPS',   N'Chuyên viên quản lý kho',       '0901000018', '001093000018', '2019-12-02', '17500000', N'Huỳnh Ngọc Anh'),
    (19, 'demo19', 'DEMO019', N'Phan Bảo Ngọc',    N'Nữ',  '1996-04-07', N'Cần Thơ',        'SALES', N'Chuyên viên bán hàng',          '0901000019', '001096000019', '2022-06-20', '17500000', N'Phan Văn Đức'),
    (20, 'demo20', 'DEMO020', N'Vũ Tiến Dũng',     N'Nam', '1991-08-31', N'Phú Thọ',        'IT',    N'Kỹ sư kiểm thử phần mềm',       '0901000020', '001091000020', '2019-04-08', '21000000', N'Vũ Thị Hồng');

INSERT dbo.HrmUserAccount
    (Username, PasswordHash, PasswordSalt, DisplayName, RoleCode, DepartmentId, SupervisorUserId, IsActive)
SELECT e.Username,
       'hLuLygwPf9f5WzML0MqEg/KswxrX3KlHypL5SHQXMV8=',
       '1YksFXARmslLCwyDjUWWVw==',
       e.DisplayName,
       'EMPLOYEE',
       d.Id,
       CASE e.DepartmentCode
           WHEN 'IT' THEN (SELECT Id FROM dbo.HrmUserAccount WHERE Username = 'huongtm')
           WHEN 'HR' THEN (SELECT Id FROM dbo.HrmUserAccount WHERE Username = 'hradmin')
           ELSE (SELECT Id FROM dbo.HrmUserAccount WHERE Username = 'thedt')
       END,
       1
FROM @DemoEmployees e
INNER JOIN dbo.HrmDepartment d ON d.Code = e.DepartmentCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.HrmUserAccount u WHERE u.Username = e.Username);

UPDATE u
SET DisplayName = e.DisplayName,
    RoleCode = 'EMPLOYEE',
    DepartmentId = d.Id,
    SupervisorUserId = CASE e.DepartmentCode
        WHEN 'IT' THEN (SELECT Id FROM dbo.HrmUserAccount WHERE Username = 'huongtm')
        WHEN 'HR' THEN (SELECT Id FROM dbo.HrmUserAccount WHERE Username = 'hradmin')
        ELSE (SELECT Id FROM dbo.HrmUserAccount WHERE Username = 'thedt')
    END,
    IsActive = 1
FROM dbo.HrmUserAccount u
INNER JOIN @DemoEmployees e ON e.Username = u.Username
INNER JOIN dbo.HrmDepartment d ON d.Code = e.DepartmentCode;

MERGE dbo.HrmEmployeeProfile AS target
USING
(
    SELECT u.Id AS UserId, e.*
    FROM @DemoEmployees e
    INNER JOIN dbo.HrmUserAccount u ON u.Username = e.Username
) AS source
ON target.UserId = source.UserId
WHEN MATCHED THEN UPDATE SET
    EmployeeCode = source.EmployeeCode,
    AvatarUrl = '/images/nhigia-logo.png',
    Gender = source.Gender,
    DateOfBirth = source.DateOfBirth,
    PlaceOfBirth = source.PlaceOfBirth,
    Nationality = N'Việt Nam',
    Ethnicity = N'Kinh',
    Religion = N'Không',
    MaritalStatus = CASE WHEN source.Seq % 3 = 0 THEN N'Đã kết hôn' ELSE N'Độc thân' END,
    MobilePhone = source.MobilePhone,
    OfficePhone = CONCAT('0247300', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
    HomePhone = CONCAT('0243800', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
    PersonalEmail = CONCAT(source.Username, '@gmail.com'),
    CompanyEmail = CONCAT(source.Username, '@nhigia.vn'),
    PermanentAddress = CONCAT(N'Số ', source.Seq + 10, N', đường Nguyễn Trãi, ', source.PlaceOfBirth),
    CurrentAddress = CONCAT(N'Căn hộ ', source.Seq, N', Hà Nội'),
    IdentityNumber = source.IdentityNumber,
    IdentityIssuedDate = DATEADD(YEAR, -3, source.HireDate),
    IdentityIssuedPlace = N'Cục Cảnh sát QLHC về TTXH',
    IdentityExpiryDate = DATEADD(YEAR, 10, DATEADD(YEAR, -3, source.HireDate)),
    PassportNumber = CONCAT('DEMO', RIGHT('000000' + CONVERT(VARCHAR(6), source.Seq), 6)),
    PassportIssuedDate = source.HireDate,
    PassportIssuedPlace = N'Cục Quản lý xuất nhập cảnh',
    PassportExpiryDate = DATEADD(YEAR, 10, source.HireDate),
    PersonalTaxCode = CONCAT('010900', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
    JobTitle = source.JobTitle,
    EmploymentStatus = N'Đang làm việc',
    WorkLocation = N'Văn phòng Nhị Gia - Hà Nội',
    TimekeepingCode = source.EmployeeCode,
    HireDate = source.HireDate,
    ProbationDate = source.HireDate,
    OfficialDate = DATEADD(MONTH, 2, source.HireDate),
    ContractType = N'Hợp đồng lao động xác định thời hạn',
    ContractNumber = CONCAT('HDLD/DEMO/', RIGHT('00' + CONVERT(VARCHAR(2), source.Seq), 2)),
    ContractStartDate = DATEADD(MONTH, 2, source.HireDate),
    ContractEndDate = DATEADD(YEAR, 3, DATEADD(MONTH, 2, source.HireDate)),
    AnnualLeaveDays = 12,
    EducationLevel = N'Đại học',
    Degree = N'Cử nhân',
    SchoolName = CASE WHEN source.Seq % 2 = 0 THEN N'Đại học Bách khoa Hà Nội' ELSE N'Đại học Kinh tế Quốc dân' END,
    Faculty = CASE WHEN source.DepartmentCode = 'IT' THEN N'Công nghệ thông tin' ELSE N'Quản trị kinh doanh' END,
    Major = source.JobTitle,
    GraduationYear = CONVERT(NVARCHAR(4), YEAR(source.HireDate) - 1),
    GraduationClassification = CASE WHEN source.Seq % 4 = 0 THEN N'Giỏi' ELSE N'Khá' END,
    BasicSalary = source.BasicSalary,
    BankAccountNumber = CONCAT('1903600', RIGHT('000000' + CONVERT(VARCHAR(6), source.Seq), 6)),
    BankName = N'Ngân hàng TMCP Kỹ thương Việt Nam',
    BankBranch = N'Chi nhánh Hà Nội',
    SocialInsuranceNumber = CONCAT('BHXHDEMO', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
    SocialInsuranceStartDate = DATEADD(MONTH, 2, source.HireDate),
    HealthInsuranceNumber = CONCAT('DN40101DEMO', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
    HealthInsuranceExpiryDate = '2027-12-31',
    RegisteredHealthFacility = N'Bệnh viện Đa khoa Hà Đông',
    EmergencyContactName = source.EmergencyContactName,
    EmergencyContactRelationship = CASE WHEN source.Gender = N'Nam' THEN N'Mẹ' ELSE N'Bố' END,
    EmergencyContactPhone = CONCAT('091200', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
    EmergencyContactEmail = CONCAT('lienhe.', source.Username, '@gmail.com'),
    EmergencyContactAddress = CONCAT(N'Số ', source.Seq + 10, N', đường Nguyễn Trãi, ', source.PlaceOfBirth),
    Notes = N'Dữ liệu nhân viên thử nghiệm đầy đủ; mã DEMO dùng để nhận diện và lọc dữ liệu.',
    UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT
    (UserId, EmployeeCode, AvatarUrl, Gender, DateOfBirth, PlaceOfBirth, Nationality, Ethnicity, Religion,
     MaritalStatus, MobilePhone, OfficePhone, HomePhone, PersonalEmail, CompanyEmail, PermanentAddress,
     CurrentAddress, IdentityNumber, IdentityIssuedDate, IdentityIssuedPlace, IdentityExpiryDate,
     PassportNumber, PassportIssuedDate, PassportIssuedPlace, PassportExpiryDate, PersonalTaxCode,
     JobTitle, EmploymentStatus, WorkLocation, TimekeepingCode, HireDate, ProbationDate, OfficialDate,
     ContractType, ContractNumber, ContractStartDate, ContractEndDate, AnnualLeaveDays, EducationLevel,
     Degree, SchoolName, Faculty, Major, GraduationYear, GraduationClassification, BasicSalary,
     BankAccountNumber, BankName, BankBranch, SocialInsuranceNumber, SocialInsuranceStartDate,
     HealthInsuranceNumber, HealthInsuranceExpiryDate, RegisteredHealthFacility, EmergencyContactName,
     EmergencyContactRelationship, EmergencyContactPhone, EmergencyContactEmail, EmergencyContactAddress, Notes)
VALUES
    (source.UserId, source.EmployeeCode, '/images/nhigia-logo.png', source.Gender, source.DateOfBirth,
     source.PlaceOfBirth, N'Việt Nam', N'Kinh', N'Không',
     CASE WHEN source.Seq % 3 = 0 THEN N'Đã kết hôn' ELSE N'Độc thân' END,
     source.MobilePhone,
     CONCAT('0247300', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
     CONCAT('0243800', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
     CONCAT(source.Username, '@gmail.com'), CONCAT(source.Username, '@nhigia.vn'),
     CONCAT(N'Số ', source.Seq + 10, N', đường Nguyễn Trãi, ', source.PlaceOfBirth),
     CONCAT(N'Căn hộ ', source.Seq, N', Hà Nội'), source.IdentityNumber,
     DATEADD(YEAR, -3, source.HireDate), N'Cục Cảnh sát QLHC về TTXH',
     DATEADD(YEAR, 10, DATEADD(YEAR, -3, source.HireDate)),
     CONCAT('DEMO', RIGHT('000000' + CONVERT(VARCHAR(6), source.Seq), 6)), source.HireDate,
     N'Cục Quản lý xuất nhập cảnh', DATEADD(YEAR, 10, source.HireDate),
     CONCAT('010900', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)), source.JobTitle,
     N'Đang làm việc', N'Văn phòng Nhị Gia - Hà Nội', source.EmployeeCode, source.HireDate,
     source.HireDate, DATEADD(MONTH, 2, source.HireDate), N'Hợp đồng lao động xác định thời hạn',
     CONCAT('HDLD/DEMO/', RIGHT('00' + CONVERT(VARCHAR(2), source.Seq), 2)),
     DATEADD(MONTH, 2, source.HireDate), DATEADD(YEAR, 3, DATEADD(MONTH, 2, source.HireDate)), 12,
     N'Đại học', N'Cử nhân',
     CASE WHEN source.Seq % 2 = 0 THEN N'Đại học Bách khoa Hà Nội' ELSE N'Đại học Kinh tế Quốc dân' END,
     CASE WHEN source.DepartmentCode = 'IT' THEN N'Công nghệ thông tin' ELSE N'Quản trị kinh doanh' END,
     source.JobTitle, CONVERT(NVARCHAR(4), YEAR(source.HireDate) - 1),
     CASE WHEN source.Seq % 4 = 0 THEN N'Giỏi' ELSE N'Khá' END, source.BasicSalary,
     CONCAT('1903600', RIGHT('000000' + CONVERT(VARCHAR(6), source.Seq), 6)),
     N'Ngân hàng TMCP Kỹ thương Việt Nam', N'Chi nhánh Hà Nội',
     CONCAT('BHXHDEMO', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
     DATEADD(MONTH, 2, source.HireDate),
     CONCAT('DN40101DEMO', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)), '2027-12-31',
     N'Bệnh viện Đa khoa Hà Đông', source.EmergencyContactName,
     CASE WHEN source.Gender = N'Nam' THEN N'Mẹ' ELSE N'Bố' END,
     CONCAT('091200', RIGHT('0000' + CONVERT(VARCHAR(4), source.Seq), 4)),
     CONCAT('lienhe.', source.Username, '@gmail.com'),
     CONCAT(N'Số ', source.Seq + 10, N', đường Nguyễn Trãi, ', source.PlaceOfBirth),
     N'Dữ liệu nhân viên thử nghiệm đầy đủ; mã DEMO dùng để nhận diện và lọc dữ liệu.');
GO
