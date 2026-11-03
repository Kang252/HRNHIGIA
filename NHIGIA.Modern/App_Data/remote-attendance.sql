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
IF OBJECT_ID('dbo.HrmRemoteWorkLocation','U') IS NULL
BEGIN
 CREATE TABLE dbo.HrmRemoteWorkLocation (
  Id BIGINT IDENTITY PRIMARY KEY,
  PlanId BIGINT NOT NULL REFERENCES dbo.HrmRemoteWorkPlan(Id),
  Name NVARCHAR(200) NOT NULL,
  Latitude FLOAT NOT NULL, Longitude FLOAT NOT NULL,
  RadiusMeters INT NOT NULL,
  SortOrder INT NOT NULL DEFAULT 0,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
  CHECK(Latitude BETWEEN -90 AND 90), CHECK(Longitude BETWEEN -180 AND 180),
  CHECK(RadiusMeters BETWEEN 50 AND 2000)
 );
 CREATE INDEX IX_HrmRemoteWorkLocation_Plan ON dbo.HrmRemoteWorkLocation(PlanId,SortOrder,Id);
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
  MatchedLocationId BIGINT NULL REFERENCES dbo.HrmRemoteWorkLocation(Id), MatchedLocationName NVARCHAR(200) NULL,
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
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','MatchedLocationId') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD MatchedLocationId BIGINT NULL REFERENCES dbo.HrmRemoteWorkLocation(Id);
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','MatchedLocationName') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD MatchedLocationName NVARCHAR(200) NULL;
GO
INSERT dbo.HrmRemoteWorkLocation(PlanId,Name,Latitude,Longitude,RadiusMeters,SortOrder)
SELECT p.Id,p.PlaceName,p.Latitude,p.Longitude,
  CASE WHEN p.RadiusMeters<50 THEN 50 WHEN p.RadiusMeters>2000 THEN 2000 ELSE p.RadiusMeters END,0
FROM dbo.HrmRemoteWorkPlan p
WHERE p.Latitude IS NOT NULL AND p.Longitude IS NOT NULL
  AND NOT EXISTS(SELECT 1 FROM dbo.HrmRemoteWorkLocation x WHERE x.PlanId=p.Id);
GO
