-- OpenCV trial results do not change FaceVerificationStatus or attendance approval.
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceMatchStatus') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceMatchStatus VARCHAR(30) NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceMatchScore') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceMatchScore FLOAT NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceMatchThreshold') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceMatchThreshold FLOAT NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceMatchModelVersion') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceMatchModelVersion VARCHAR(128) NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceComparedAt') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceComparedAt DATETIME2 NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceMatchConsentAt') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceMatchConsentAt DATETIME2 NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceMatchConsentVersion') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceMatchConsentVersion VARCHAR(40) NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceMatchAttemptId') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceMatchAttemptId UNIQUEIDENTIFIER NULL;
GO
IF OBJECT_ID('dbo.HrmRemotePunch','U') IS NOT NULL AND COL_LENGTH('dbo.HrmRemotePunch','FaceMatchStartedAt') IS NULL
 ALTER TABLE dbo.HrmRemotePunch ADD FaceMatchStartedAt DATETIME2 NULL;
GO
