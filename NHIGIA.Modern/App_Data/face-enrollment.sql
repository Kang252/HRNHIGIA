-- Enrollment photos are protected by ASP.NET Data Protection. All dates in this table are UTC.
-- ACTIVE records are HR-approved identity evidence, not an AI/liveness verification result.
IF OBJECT_ID('dbo.HrmFaceEnrollment','U') IS NULL
BEGIN
 CREATE TABLE dbo.HrmFaceEnrollment (
  Id BIGINT IDENTITY PRIMARY KEY,
  UserId INT NOT NULL REFERENCES dbo.HrmUserAccount(Id),
  StatusCode VARCHAR(20) NOT NULL DEFAULT 'PENDING'
    CHECK(StatusCode IN ('PENDING','ACTIVE','REJECTED','REVOKED')),
  PhotoProtected VARBINARY(MAX) NULL,
  ConsentVersion VARCHAR(40) NOT NULL,
  ConsentedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  ReviewedAt DATETIME2 NULL,
  ReviewedBy INT NULL REFERENCES dbo.HrmUserAccount(Id),
  ReviewNote NVARCHAR(1000) NULL,
  RevokedAt DATETIME2 NULL,
  RevokedBy INT NULL REFERENCES dbo.HrmUserAccount(Id),
  CONSTRAINT CK_HrmFaceEnrollment_Photo CHECK(
    (StatusCode IN ('PENDING','ACTIVE') AND PhotoProtected IS NOT NULL) OR
    (StatusCode IN ('REJECTED','REVOKED') AND PhotoProtected IS NULL)),
  CONSTRAINT CK_HrmFaceEnrollment_SelfReview CHECK(ReviewedBy IS NULL OR ReviewedBy<>UserId)
 );
 CREATE INDEX IX_HrmFaceEnrollment_UserDate ON dbo.HrmFaceEnrollment(UserId,CreatedAt DESC) INCLUDE(StatusCode);
 CREATE UNIQUE INDEX UX_HrmFaceEnrollment_Active ON dbo.HrmFaceEnrollment(UserId) WHERE StatusCode='ACTIVE';
 CREATE UNIQUE INDEX UX_HrmFaceEnrollment_Pending ON dbo.HrmFaceEnrollment(UserId) WHERE StatusCode='PENDING';
END;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceEnrollmentId') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceEnrollmentId BIGINT NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceVerificationStatus') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceVerificationStatus NVARCHAR(30) NULL;
GO
