SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- 1. BẢNG ĐỊNH NGHĨA QUY TRÌNH (WORKFLOW DEFINITION)
IF OBJECT_ID('dbo.HrmWorkflowDefinition', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmWorkflowDefinition (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmWorkflowDefinition PRIMARY KEY,
        Code NVARCHAR(50) NOT NULL CONSTRAINT UQ_HrmWorkflowDefinition_Code UNIQUE,
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NULL,
        Icon NVARCHAR(50) NULL CONSTRAINT DF_HrmWorkflowDefinition_Icon DEFAULT ('task'),
        IsActive BIT NOT NULL CONSTRAINT DF_HrmWorkflowDefinition_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmWorkflowDefinition_CreatedAt DEFAULT (SYSDATETIME())
    );
END;
GO

-- 2. BẢNG CÁC BƯỚC DUYỆT (WORKFLOW STEPS)
IF OBJECT_ID('dbo.HrmWorkflowStep', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmWorkflowStep (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmWorkflowStep PRIMARY KEY,
        WorkflowId INT NOT NULL,
        StepOrder INT NOT NULL,
        StepName NVARCHAR(100) NOT NULL,
        ApproverType NVARCHAR(50) NOT NULL, -- DIRECT_MANAGER, DEPARTMENT_HEAD, DIRECTOR, HR_ADMIN, SPECIFIC_USER
        SpecificUserId INT NULL,
        ThresholdAmount DECIMAL(18,2) NULL,
        SlaHours INT NOT NULL CONSTRAINT DF_HrmWorkflowStep_Sla DEFAULT (24),
        CanRejectToStart BIT NOT NULL CONSTRAINT DF_HrmWorkflowStep_Reject DEFAULT (1),
        CONSTRAINT FK_HrmWorkflowStep_Workflow FOREIGN KEY (WorkflowId) REFERENCES dbo.HrmWorkflowDefinition(Id) ON DELETE CASCADE,
        CONSTRAINT FK_HrmWorkflowStep_SpecificUser FOREIGN KEY (SpecificUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmWorkflowStep_WorkflowOrder ON dbo.HrmWorkflowStep(WorkflowId, StepOrder);
END;
GO

-- 3. BẢNG TIẾN TRÌNH XỬ LÝ ĐƠN (WORKFLOW INSTANCE)
IF OBJECT_ID('dbo.HrmWorkflowInstance', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmWorkflowInstance (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmWorkflowInstance PRIMARY KEY,
        InstanceCode AS ('WF' + RIGHT('00000000' + CONVERT(VARCHAR(8), Id), 8)) PERSISTED,
        WorkflowId INT NOT NULL,
        RequesterUserId INT NOT NULL,
        CurrentStepId INT NULL,
        CurrentApproverId INT NULL,
        StatusCode NVARCHAR(30) NOT NULL CONSTRAINT DF_HrmWorkflowInstance_Status DEFAULT ('PENDING'), -- PENDING, APPROVED, REJECTED, CANCELLED
        Title NVARCHAR(250) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Amount DECIMAL(18,2) NULL,
        TargetTable NVARCHAR(80) NULL,
        TargetRecordId BIGINT NULL,
        PayloadJson NVARCHAR(MAX) NULL,
        DigitalSignature NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmWorkflowInstance_CreatedAt DEFAULT (SYSDATETIME()),
        CompletedAt DATETIME2 NULL,
        CONSTRAINT FK_HrmWorkflowInstance_Workflow FOREIGN KEY (WorkflowId) REFERENCES dbo.HrmWorkflowDefinition(Id),
        CONSTRAINT FK_HrmWorkflowInstance_Requester FOREIGN KEY (RequesterUserId) REFERENCES dbo.HrmUserAccount(Id),
        CONSTRAINT FK_HrmWorkflowInstance_Step FOREIGN KEY (CurrentStepId) REFERENCES dbo.HrmWorkflowStep(Id),
        CONSTRAINT FK_HrmWorkflowInstance_Approver FOREIGN KEY (CurrentApproverId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmWorkflowInstance_Requester ON dbo.HrmWorkflowInstance(RequesterUserId, StatusCode, CreatedAt DESC);
    CREATE INDEX IX_HrmWorkflowInstance_Approver ON dbo.HrmWorkflowInstance(CurrentApproverId, StatusCode);
END;
GO

-- 4. BẢNG NHẬT KÝ PHÊ DUYỆT (WORKFLOW AUDIT LOG)
IF OBJECT_ID('dbo.HrmWorkflowLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmWorkflowLog (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmWorkflowLog PRIMARY KEY,
        InstanceId BIGINT NOT NULL,
        StepId INT NULL,
        ActorUserId INT NOT NULL,
        ActionCode NVARCHAR(50) NOT NULL, -- SUBMIT, APPROVE, REJECT, CANCEL, DELEGATE
        Comment NVARCHAR(1000) NULL,
        DigitalSignature NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(80) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmWorkflowLog_CreatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmWorkflowLog_Instance FOREIGN KEY (InstanceId) REFERENCES dbo.HrmWorkflowInstance(Id) ON DELETE CASCADE,
        CONSTRAINT FK_HrmWorkflowLog_Step FOREIGN KEY (StepId) REFERENCES dbo.HrmWorkflowStep(Id),
        CONSTRAINT FK_HrmWorkflowLog_Actor FOREIGN KEY (ActorUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmWorkflowLog_Instance ON dbo.HrmWorkflowLog(InstanceId, CreatedAt DESC);
END;
GO

-- 5. BẢNG CHẤM CÔNG GPS HIỆN TRƯỜNG (GPS ATTENDANCE LOG)
IF OBJECT_ID('dbo.HrmAttendanceGpsLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmAttendanceGpsLog (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmAttendanceGpsLog PRIMARY KEY,
        UserId INT NOT NULL,
        Latitude DECIMAL(10,7) NOT NULL,
        Longitude DECIMAL(10,7) NOT NULL,
        AccuracyMeters DECIMAL(6,2) NULL,
        LocationAddress NVARCHAR(500) NULL,
        CheckTime DATETIME2 NOT NULL CONSTRAINT DF_HrmAttendanceGpsLog_CheckTime DEFAULT (SYSDATETIME()),
        CheckType NVARCHAR(20) NOT NULL, -- IN, OUT
        SelfieImageUrl NVARCHAR(500) NULL,
        FaceMatchScore DECIMAL(5,2) NULL,
        IsMockLocation BIT NOT NULL CONSTRAINT DF_HrmAttendanceGpsLog_Mock DEFAULT (0),
        DeviceInfo NVARCHAR(250) NULL,
        IpAddress NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmAttendanceGpsLog_CreatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmAttendanceGpsLog_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmAttendanceGpsLog_UserTime ON dbo.HrmAttendanceGpsLog(UserId, CheckTime DESC);
END;
GO

-- 6. BẢNG DANH MỤC VĂN PHÒNG & ĐỊA ĐIỂM CÔNG TÁC (OFFICE LOCATIONS & GEOFENCE)
IF OBJECT_ID('dbo.HrmOfficeLocation', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmOfficeLocation (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmOfficeLocation PRIMARY KEY,
        Code NVARCHAR(50) NOT NULL CONSTRAINT UQ_HrmOfficeLocation_Code UNIQUE,
        Name NVARCHAR(150) NOT NULL,
        Address NVARCHAR(300) NOT NULL,
        Latitude DECIMAL(10,7) NOT NULL,
        Longitude DECIMAL(10,7) NOT NULL,
        RadiusMeters INT NOT NULL CONSTRAINT DF_HrmOfficeLocation_Radius DEFAULT (150),
        IsActive BIT NOT NULL CONSTRAINT DF_HrmOfficeLocation_Active DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmOfficeLocation_CreatedAt DEFAULT (SYSDATETIME())
    );
END;
GO

-- 7. BẢNG HỒ SƠ TÀI LIỆU SỐ & HỢP ĐỒNG (EMPLOYEE DIGITAL DOCUMENTS)
IF OBJECT_ID('dbo.HrmEmployeeDocument', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmEmployeeDocument (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmEmployeeDocument PRIMARY KEY,
        UserId INT NOT NULL,
        DocType NVARCHAR(50) NOT NULL, -- CONTRACT, ID_CARD, DIPLOMA, CERTIFICATE, DECISION, OTHER
        DocNumber NVARCHAR(100) NULL,
        DocTitle NVARCHAR(250) NOT NULL,
        FileUrl NVARCHAR(500) NOT NULL,
        FileSizeBytes BIGINT NOT NULL,
        ContentType NVARCHAR(100) NOT NULL,
        EffectiveDate DATE NULL,
        ExpiryDate DATE NULL,
        IsVerifiedByHr BIT NOT NULL CONSTRAINT DF_HrmEmployeeDocument_Verified DEFAULT (0),
        VerifiedAt DATETIME2 NULL,
        UploadedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmEmployeeDocument_UploadedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmEmployeeDocument_User FOREIGN KEY (UserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmEmployeeDocument_User ON dbo.HrmEmployeeDocument(UserId, DocType);
END;
GO

-- 8. BẢNG HÀNG ĐỢI THÔNG BÁO PUSH & ZALO (NOTIFICATION QUEUE)
IF OBJECT_ID('dbo.HrmNotificationQueue', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HrmNotificationQueue (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HrmNotificationQueue PRIMARY KEY,
        RecipientUserId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Body NVARCHAR(1000) NOT NULL,
        NotificationType NVARCHAR(50) NOT NULL, -- WORKFLOW_APPROVAL, PAYSLIP, ANNOUNCEMENT, SYSTEM
        TargetUrl NVARCHAR(500) NULL,
        Channels NVARCHAR(50) NOT NULL CONSTRAINT DF_HrmNotificationQueue_Channels DEFAULT ('PUSH'),
        IsSent BIT NOT NULL CONSTRAINT DF_HrmNotificationQueue_Sent DEFAULT (0),
        SentAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HrmNotificationQueue_CreatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_HrmNotificationQueue_User FOREIGN KEY (RecipientUserId) REFERENCES dbo.HrmUserAccount(Id)
    );
    CREATE INDEX IX_HrmNotificationQueue_Status ON dbo.HrmNotificationQueue(IsSent, CreatedAt);
END;
GO

-- KHỞI TẠO ĐỊA ĐIỂM VĂN PHÒNG NHỊ GIA GROUP
IF NOT EXISTS (SELECT 1 FROM dbo.HrmOfficeLocation WHERE Code = 'NHIGIA_HQ')
    INSERT dbo.HrmOfficeLocation(Code, Name, Address, Latitude, Longitude, RadiusMeters)
    VALUES ('NHIGIA_HQ', N'Trụ sở Nhị Gia Group - TP. Hồ Chí Minh', N'108-110 Nguyễn Văn Trỗi, Phường 8, Phú Nhuận, TP.HCM', 10.7937400, 106.6781200, 150);

IF NOT EXISTS (SELECT 1 FROM dbo.HrmOfficeLocation WHERE Code = 'NHIGIA_HN')
    INSERT dbo.HrmOfficeLocation(Code, Name, Address, Latitude, Longitude, RadiusMeters)
    VALUES ('NHIGIA_HN', N'Văn phòng Chi nhánh Hà Nội', N'Số 56, Ngõ 102 Trường Chinh, Đống Đa, Hà Nội', 21.0012000, 105.8398000, 150);
GO

-- KHỞI TẠO 6 QUY TRÌNH WORKFLOW CHUẨN
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowDefinition WHERE Code = 'LEAVE')
    INSERT dbo.HrmWorkflowDefinition(Code, Name, Description, Icon) VALUES ('LEAVE', N'Nghỉ phép', N'Quy trình xin nghỉ phép, nghỉ ốm, nghỉ không lương', 'calendar_today');

IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowDefinition WHERE Code = 'BUSINESS_TRIP')
    INSERT dbo.HrmWorkflowDefinition(Code, Name, Description, Icon) VALUES ('BUSINESS_TRIP', N'Công tác', N'Đề xuất đi công tác trong và ngoài nước', 'flight_takeoff');

IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowDefinition WHERE Code = 'PURCHASE')
    INSERT dbo.HrmWorkflowDefinition(Code, Name, Description, Icon) VALUES ('PURCHASE', N'Đề xuất mua sắm', N'Đề xuất mua văn phòng phẩm, trang thiết bị công ty', 'shopping_cart');

IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowDefinition WHERE Code = 'IT_ASSET')
    INSERT dbo.HrmWorkflowDefinition(Code, Name, Description, Icon) VALUES ('IT_ASSET', N'Cấp phát thiết bị IT', N'Yêu cầu cấp phát laptop, màn hình, tai nghe, quyền truy cập hệ thống', 'devices');

IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowDefinition WHERE Code = 'EXPENSE')
    INSERT dbo.HrmWorkflowDefinition(Code, Name, Description, Icon) VALUES ('EXPENSE', N'Duyệt chi phí', N'Thanh toán công tác phí, tiếp khách, hoàn ứng', 'receipt_long');

IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowDefinition WHERE Code = 'RECRUITMENT')
    INSERT dbo.HrmWorkflowDefinition(Code, Name, Description, Icon) VALUES ('RECRUITMENT', N'Đề xuất tuyển dụng', N'Đề xuất bổ sung nhân sự cho phòng ban', 'person_add');
GO

-- CẤU HÌNH CÁC BƯỚC MẪU CHO 6 QUY TRÌNH
DECLARE @wfLeaveId INT = (SELECT Id FROM dbo.HrmWorkflowDefinition WHERE Code = 'LEAVE');
IF @wfLeaveId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowStep WHERE WorkflowId = @wfLeaveId)
BEGIN
    INSERT dbo.HrmWorkflowStep(WorkflowId, StepOrder, StepName, ApproverType, SlaHours)
    VALUES (@wfLeaveId, 1, N'Quản lý trực tiếp phê duyệt', 'DIRECT_MANAGER', 24),
           (@wfLeaveId, 2, N'Phòng Nhân sự xác nhận', 'HR_ADMIN', 24);
END;

DECLARE @wfTripId INT = (SELECT Id FROM dbo.HrmWorkflowDefinition WHERE Code = 'BUSINESS_TRIP');
IF @wfTripId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowStep WHERE WorkflowId = @wfTripId)
BEGIN
    INSERT dbo.HrmWorkflowStep(WorkflowId, StepOrder, StepName, ApproverType, SlaHours)
    VALUES (@wfTripId, 1, N'Trưởng bộ phận phê duyệt', 'DIRECT_MANAGER', 24),
           (@wfTripId, 2, N'Ban Giám Đốc phê duyệt', 'DIRECTOR', 48);
END;

DECLARE @wfPurchaseId INT = (SELECT Id FROM dbo.HrmWorkflowDefinition WHERE Code = 'PURCHASE');
IF @wfPurchaseId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowStep WHERE WorkflowId = @wfPurchaseId)
BEGIN
    INSERT dbo.HrmWorkflowStep(WorkflowId, StepOrder, StepName, ApproverType, SlaHours)
    VALUES (@wfPurchaseId, 1, N'Trưởng bộ phận phê duyệt', 'DIRECT_MANAGER', 24),
           (@wfPurchaseId, 2, N'Ban Giám Đốc phê duyệt', 'DIRECTOR', 48);
END;

DECLARE @wfItId INT = (SELECT Id FROM dbo.HrmWorkflowDefinition WHERE Code = 'IT_ASSET');
IF @wfItId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowStep WHERE WorkflowId = @wfItId)
BEGIN
    INSERT dbo.HrmWorkflowStep(WorkflowId, StepOrder, StepName, ApproverType, SlaHours)
    VALUES (@wfItId, 1, N'Trưởng bộ phận xác nhận nhu cầu', 'DIRECT_MANAGER', 24),
           (@wfItId, 2, N'Phòng CNTT chuẩn bị & bàn giao', 'DEPARTMENT_HEAD', 48);
END;

DECLARE @wfExpenseId INT = (SELECT Id FROM dbo.HrmWorkflowDefinition WHERE Code = 'EXPENSE');
IF @wfExpenseId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowStep WHERE WorkflowId = @wfExpenseId)
BEGIN
    INSERT dbo.HrmWorkflowStep(WorkflowId, StepOrder, StepName, ApproverType, SlaHours)
    VALUES (@wfExpenseId, 1, N'Trưởng bộ phận phê duyệt', 'DIRECT_MANAGER', 24),
           (@wfExpenseId, 2, N'Kế toán thẩm tra chứng từ', 'HR_ADMIN', 24),
           (@wfExpenseId, 3, N'Ban Giám Đốc duyệt chi', 'DIRECTOR', 48);
END;

DECLARE @wfRecruitId INT = (SELECT Id FROM dbo.HrmWorkflowDefinition WHERE Code = 'RECRUITMENT');
IF @wfRecruitId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.HrmWorkflowStep WHERE WorkflowId = @wfRecruitId)
BEGIN
    INSERT dbo.HrmWorkflowStep(WorkflowId, StepOrder, StepName, ApproverType, SlaHours)
    VALUES (@wfRecruitId, 1, N'Phòng Nhân sự thẩm định ngân sách & định biên', 'HR_ADMIN', 48),
           (@wfRecruitId, 2, N'Ban Giám Đốc phê duyệt đợt tuyển', 'DIRECTOR', 48);
END;
GO

