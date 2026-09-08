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
    INSERT dbo.HrmUserAccount(Username, PasswordHash, PasswordSalt, DisplayName, RoleCode, DepartmentId, SupervisorUserId) VALUES ('anhvt', '1NVAHjhZ1bhmA5ZfU2t/qbNk4dyyl5/g9M7ybLNea18=', 'jl5wrk1kwMsd8JVs2oxcGQ==', N'Nhân viên', 'EMPLOYEE', (SELECT Id FROM dbo.HrmDepartment WHERE Code='IT'), (SELECT Id FROM dbo.HrmUserAccount WHERE Username='huongtm'));
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
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID('dbo.HrmWorkItem', 'U') IS NULL
BEGIN
 CREATE TABLE dbo.HrmWorkItem (
  Id INT IDENTITY PRIMARY KEY,
  Kind NVARCHAR(20) NOT NULL CHECK (Kind IN ('kpi','payroll','recruitment','training','overtime','resignation','transfer','assets','helpdesk')),
  Title NVARCHAR(200) NOT NULL, Description NVARCHAR(2000) NULL,
  Category NVARCHAR(100) NULL, Reference NVARCHAR(100) NULL,
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
SELECT TOP (1) @workKindConstraint = cc.name
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID('dbo.HrmWorkItem')
  AND cc.definition LIKE '%Kind%';
IF @workKindConstraint IS NOT NULL AND @workKindConstraint <> 'CK_HrmWorkItem_Kind'
    EXEC('ALTER TABLE dbo.HrmWorkItem DROP CONSTRAINT ' + QUOTENAME(@workKindConstraint));
IF OBJECT_ID('dbo.CK_HrmWorkItem_Kind', 'C') IS NULL
    ALTER TABLE dbo.HrmWorkItem WITH CHECK ADD CONSTRAINT CK_HrmWorkItem_Kind
    CHECK (Kind IN ('kpi','payroll','recruitment','training','overtime','resignation','transfer','assets','helpdesk'));
GO
