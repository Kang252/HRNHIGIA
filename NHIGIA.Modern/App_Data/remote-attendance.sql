-- Additive migration; HRM dates are Vietnam local time, CapturedAt preserves the device offset.
IF OBJECT_ID('dbo.HrmRemoteWorkPlan','U') IS NULL
BEGIN
 CREATE TABLE dbo.HrmRemoteWorkPlan (
  Id BIGINT IDENTITY PRIMARY KEY, UserId INT NOT NULL REFERENCES dbo.HrmUserAccount(Id),
  Mode VARCHAR(10) NOT NULL CHECK(Mode IN ('FIELD','HOME')), PlaceName NVARCHAR(200) NOT NULL,
  FromDate DATE NOT NULL, ToDate DATE NOT NULL, WorkDaysMask INT NOT NULL,
  WindowStart TIME NOT NULL, WindowEnd TIME NOT NULL, IsFlexible BIT NOT NULL,
  RequiredMinutes INT NOT NULL, BreakMinutes INT NOT NULL,
  Latitude FLOAT NULL, Longitude FLOAT NULL, RadiusMeters INT NOT NULL,
  LeaveRequestId INT NULL REFERENCES dbo.HrmLeaveRequest(Id), Note NVARCHAR(2000) NULL,
  StatusCode VARCHAR(20) NOT NULL DEFAULT 'PENDING' CHECK(StatusCode IN ('PENDING','APPROVED','REJECTED','CANCELLED')),
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(), ReviewedAt DATETIME2 NULL,
  ReviewedBy INT NULL REFERENCES dbo.HrmUserAccount(Id), ReviewNote NVARCHAR(1000) NULL,
  CHECK(ToDate>=FromDate), CHECK(WorkDaysMask BETWEEN 1 AND 127)
 );
 CREATE INDEX IX_HrmRemoteWorkPlan_UserDate ON dbo.HrmRemoteWorkPlan(UserId,FromDate,ToDate);
END;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NULL
BEGIN
 CREATE TABLE dbo.HrmRemotePunch (
  Id BIGINT IDENTITY PRIMARY KEY, ClientId UNIQUEIDENTIFIER NOT NULL,
  UserId INT NOT NULL REFERENCES dbo.HrmUserAccount(Id), PlanId BIGINT NOT NULL REFERENCES dbo.HrmRemoteWorkPlan(Id),
  Kind VARCHAR(10) NOT NULL CHECK(Kind IN ('IN','OUT','VISIT')), CapturedAt DATETIMEOFFSET NOT NULL,
  CheckTime DATETIME2 NOT NULL, ReceivedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(), WasOffline BIT NOT NULL,
  Latitude FLOAT NULL, Longitude FLOAT NULL, AccuracyMeters FLOAT NULL, DistanceMeters FLOAT NULL,
  PlaceName NVARCHAR(200) NOT NULL, Note NVARCHAR(2000) NULL, PhotoContent VARBINARY(MAX) NOT NULL,
  PhotoContentType VARCHAR(30) NOT NULL,
  StatusCode VARCHAR(20) NOT NULL CHECK(StatusCode IN ('PENDING','APPROVED','REJECTED')),
  ReviewReason NVARCHAR(1000) NULL, ReviewNote NVARCHAR(1000) NULL,
  ReviewedBy INT NULL REFERENCES dbo.HrmUserAccount(Id), ReviewedAt DATETIME2 NULL,
  IncludedAt DATETIME2 NULL,
  CONSTRAINT UQ_HrmRemotePunch_Client UNIQUE(UserId,ClientId)
 );
 CREATE INDEX IX_HrmRemotePunch_UserTime ON dbo.HrmRemotePunch(UserId,CheckTime) INCLUDE(StatusCode,Kind,PlanId,IncludedAt);
END;
GO
