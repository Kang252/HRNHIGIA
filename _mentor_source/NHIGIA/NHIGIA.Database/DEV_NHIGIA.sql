USE [master]
GO
/****** Object:  Database [DEV_NHIGIA]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE DATABASE [DEV_NHIGIA]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DEV_NHIGIA', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\DEV_NHIGIA.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DEV_NHIGIA_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\DEV_NHIGIA_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [DEV_NHIGIA] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DEV_NHIGIA].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DEV_NHIGIA] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET ARITHABORT OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DEV_NHIGIA] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DEV_NHIGIA] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET  ENABLE_BROKER 
GO
ALTER DATABASE [DEV_NHIGIA] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DEV_NHIGIA] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET RECOVERY FULL 
GO
ALTER DATABASE [DEV_NHIGIA] SET  MULTI_USER 
GO
ALTER DATABASE [DEV_NHIGIA] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DEV_NHIGIA] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DEV_NHIGIA] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DEV_NHIGIA] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DEV_NHIGIA] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DEV_NHIGIA] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'DEV_NHIGIA', N'ON'
GO
ALTER DATABASE [DEV_NHIGIA] SET QUERY_STORE = OFF
GO
USE [DEV_NHIGIA]
GO
/****** Object:  UserDefinedTableType [dbo].[TypeAllowanceInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeAllowanceInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[AllowanceTypeId] [int] NULL,
	[Price] [nvarchar](500) NULL,
	[StartDate] [date] NULL,
	[ToDate] [date] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeAssetInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeAssetInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[AssetCode] [nvarchar](100) NULL,
	[AssetName] [nvarchar](100) NULL,
	[AssetTypeId] [int] NULL,
	[ReceivedDate] [date] NULL,
	[PayDay] [date] NULL,
	[AssetStatusId] [int] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeAttachment]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeAttachment] AS TABLE(
	[EmployeeId] [int] NULL,
	[FileName] [nvarchar](500) NULL,
	[FileType] [nvarchar](500) NULL,
	[FileSize] [bigint] NOT NULL,
	[FileContent] [varbinary](max) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeAttachmentInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeAttachmentInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[FileName] [nvarchar](500) NULL,
	[FileType] [nvarchar](500) NULL,
	[FileSize] [bigint] NOT NULL,
	[FileContent] [varbinary](max) NULL,
	[IsDownload] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeBonusInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeBonusInformation] AS TABLE(
	[Id] [int] NULL,
	[DecisionNumber] [nvarchar](100) NULL,
	[DecisionDate] [date] NULL,
	[ThePersonSignedTheDecisionId] [int] NULL,
	[BonusDay] [date] NULL,
	[RewardPlanId] [int] NULL,
	[BonusGrounds] [nvarchar](100) NULL,
	[RewardReasonId] [int] NULL,
	[CommendationFormId] [int] NULL,
	[BonusBudgetSourceId] [int] NULL,
	[TotalValue] [nvarchar](100) NULL,
	[StatusBonusId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeBonusInformationStaffWereCommended]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeBonusInformationStaffWereCommended] AS TABLE(
	[Id] [int] NULL,
	[BonusInformationId] [int] NULL,
	[EmployeeId] [int] NULL,
	[BonusValue] [nvarchar](100) NULL,
	[Status] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeCertificateInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeCertificateInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[CertificateGroupId] [int] NULL,
	[CertificateName] [nvarchar](100) NULL,
	[NumberOfCertificates] [nvarchar](100) NULL,
	[DegreeTrainingId] [int] NULL,
	[DateRange] [date] NULL,
	[ExpirationDate] [date] NULL,
	[IssuedBy] [nvarchar](100) NULL,
	[ClassificationId] [int] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeContractInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeContractInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[JobPositionId] [int] NULL,
	[SomeContracts] [nvarchar](100) NULL,
	[WorkUnitId] [int] NULL,
	[SignDay] [date] NULL,
	[ContractName] [nvarchar](100) NULL,
	[ContractTypeId] [int] NULL,
	[ContractTermId] [int] NULL,
	[TheFormOfWorkId] [int] NULL,
	[WageRate] [nvarchar](100) NULL,
	[EffectiveDate] [date] NULL,
	[ExpirationDate] [date] NULL,
	[Abstract] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeDegreeInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeDegreeInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[TrainingPlacesId] [int] NULL,
	[FromYear] [nvarchar](100) NULL,
	[ToYear] [nvarchar](100) NULL,
	[FacultyId] [int] NULL,
	[SpecializedId] [int] NULL,
	[DegreeTrainingId] [int] NULL,
	[FormsOfTrainingId] [int] NULL,
	[ClassificationId] [int] NULL,
	[Graduated] [bit] NULL,
	[DateReceived] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeEmployeesOnBusinessTrip]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeEmployeesOnBusinessTrip] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[EmployeeApprovedId] [int] NULL,
	[DayTo] [date] NULL,
	[ReturnDate] [date] NULL,
	[WorkingPlace] [nvarchar](100) NULL,
	[WorkingPurpose] [nvarchar](100) NULL,
	[RecommendedDate] [date] NULL,
	[Deadline] [date] NULL,
	[AmountProposedForAdvance] [nvarchar](100) NULL,
	[AmountOfAdvance] [nvarchar](100) NULL,
	[ReasonForAdvance] [nvarchar](100) NULL,
	[RequireToBeSupported] [nvarchar](max) NULL,
	[BrowsingStatusId] [int] NULL,
	[ReasonsForNotBrowsing] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeEmployeesOnBusinessTripAdvances]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeEmployeesOnBusinessTripAdvances] AS TABLE(
	[Id] [int] NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[Expenses] [nvarchar](100) NULL,
	[Unit] [nvarchar](100) NULL,
	[Amount] [nvarchar](100) NULL,
	[UnitPrice] [nvarchar](100) NULL,
	[Money] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeEmployeesOnBusinessTripAssignedStaff]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeEmployeesOnBusinessTripAssignedStaff] AS TABLE(
	[Id] [int] NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[EmployeeId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeEmployeesOnBusinessTripPayments]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeEmployeesOnBusinessTripPayments] AS TABLE(
	[Id] [int] NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[Expenses] [nvarchar](100) NULL,
	[VoucherNumber] [nvarchar](100) NULL,
	[DayVouchers] [date] NULL,
	[AmountSpent] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeEmployeesOnBusinessTripRevenueEstimates]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeEmployeesOnBusinessTripRevenueEstimates] AS TABLE(
	[Id] [int] NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[Revenue] [nvarchar](100) NULL,
	[AmountOfMoney] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeEvaluate]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeEvaluate] AS TABLE(
	[Id] [int] NULL,
	[NameOfAudit] [nvarchar](100) NULL,
	[EvaluationPeriodId] [int] NULL,
	[WorkUnitId] [int] NULL,
	[EvaluationStatusId] [int] NULL,
	[PersonInChargeId] [int] NULL,
	[Since] [date] NULL,
	[ToDate] [date] NULL,
	[EvaluationTerm] [date] NULL,
	[BriefDescription] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeEvaluateDetail]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeEvaluateDetail] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[EvaluateId] [int] NULL,
	[Result] [nvarchar](500) NULL,
	[EvaluateStatus] [nvarchar](500) NULL,
	[ResultJson] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeFamilyInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeFamilyInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[RelationshipId] [int] NULL,
	[FirstAndLastName] [nvarchar](100) NULL,
	[DateOfBirth] [date] NULL,
	[SexId] [int] NULL,
	[NationalityId] [int] NULL,
	[IdPassportNumber] [nvarchar](100) NULL,
	[Address] [nvarchar](100) NULL,
	[MobilePhone] [nvarchar](100) NULL,
	[HomePhone] [nvarchar](100) NULL,
	[Email] [nvarchar](100) NULL,
	[Job] [nvarchar](100) NULL,
	[PersonalTaxCode] [nvarchar](100) NULL,
	[Workplace] [nvarchar](100) NULL,
	[SameHouseholdRegistrationBook] [bit] NULL,
	[BeTheHeadOfTheHousehold] [bit] NULL,
	[IsADependent] [bit] NULL,
	[TimeToCalculateDeduction] [date] NULL,
	[TimeToEndTheDeduction] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[IsDead] [bit] NULL,
	[DeadDate] [date] NULL,
	[AsAnEmergencyContact] [bit] NULL,
	[Number] [nvarchar](100) NULL,
	[NumberBook] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeFilterDescriptor]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeFilterDescriptor] AS TABLE(
	[Member] [nvarchar](100) NULL,
	[Value] [nvarchar](500) NULL,
	[Operator] [nvarchar](50) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeListCategory]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeListCategory] AS TABLE(
	[Id] [int] NULL,
	[Code] [nvarchar](100) NULL,
	[Name] [nvarchar](100) NULL,
	[Address] [nvarchar](100) NULL,
	[ListCategoryTypeId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypePageInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypePageInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[NameOfPapers] [nvarchar](100) NULL,
	[IssuedBy] [nvarchar](100) NULL,
	[DateRange] [date] NULL,
	[ExpirationDate] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypePoliticsHealthMilitaryInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypePoliticsHealthMilitaryInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[IsAUnionMember] [bit] NULL,
	[DayToUnion] [date] NULL,
	[GroupPositionId] [int] NULL,
	[PlaceOfUnionAdmission] [nvarchar](100) NULL,
	[AsAPartyMember] [bit] NULL,
	[DayToParty] [date] NULL,
	[PartyPositionId] [int] NULL,
	[PlaceOfAdmissionToTheParty] [nvarchar](100) NULL,
	[BloodGroupId] [int] NULL,
	[Height] [nvarchar](100) NULL,
	[Weight] [nvarchar](100) NULL,
	[HealthStatus] [nvarchar](100) NULL,
	[Diseases] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[PeopleWithDisabilities] [bit] NULL,
	[AsASsoldier] [bit] NULL,
	[DateOfEnlistment] [date] NULL,
	[ArmyId] [int] NULL,
	[MilitaryUnit] [nvarchar](100) NULL,
	[MilitaryRankId] [int] NULL,
	[MilitaryPositionId] [int] NULL,
	[DateOfDemobilization] [date] NULL,
	[TheReason] [nvarchar](100) NULL,
	[AsWoundedSoldiersSickSoldiers] [bit] NULL,
	[DateToJoinRevolution] [date] NULL,
	[RankId] [int] NULL,
	[RateOfLaborDecline] [nvarchar](100) NULL,
	[EnjoyTheMode] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeProblemInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeProblemInformation] AS TABLE(
	[Id] [int] NULL,
	[ProblemName] [nvarchar](100) NULL,
	[TypeOfIncidentId] [int] NULL,
	[HappenDay] [date] NULL,
	[WhereHappened] [nvarchar](100) NULL,
	[Reason] [nvarchar](100) NULL,
	[DescriptionOfTheProblem] [nvarchar](100) NULL,
	[RelatedUnitId] [int] NULL,
	[TotalValueOfDamage] [nvarchar](100) NULL,
	[TotalCompensationValue] [nvarchar](100) NULL,
	[CompensationStatusId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeProblemInformationRelatedStaff]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeProblemInformationRelatedStaff] AS TABLE(
	[Id] [int] NULL,
	[ProblemInformationId] [int] NULL,
	[EmployeeId] [int] NULL,
	[DescribeTheRelationship] [nvarchar](100) NULL,
	[TotalNumberOfDaysOffDueToOccupationalAccidents] [nvarchar](100) NULL,
	[InjuryConditionId] [int] NULL,
	[ProcessingStatusId] [int] NULL,
	[HavePassedLaborSafetyTraining] [bit] NULL,
	[DecisionNumber] [nvarchar](100) NULL,
	[DecisionDate] [date] NULL,
	[KindOfDecisionId] [int] NULL,
	[EffectiveDate] [date] NULL,
	[FormsProcessingId] [int] NULL,
	[TheDecisionId] [int] NULL,
	[CitationOfContent] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeProblemInformationTrackEmployeeCompensation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeProblemInformationTrackEmployeeCompensation] AS TABLE(
	[Id] [int] NULL,
	[ProblemInformationId] [int] NULL,
	[EmployeeId] [int] NULL,
	[AmountMoney] [nvarchar](100) NULL,
	[PayDay] [date] NULL,
	[SourceCompensation] [nvarchar](100) NULL,
	[Type] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeProfile]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeProfile] AS TABLE(
	[Id] [int] NULL,
	[Image] [nvarchar](100) NULL,
	[EmployeeCode] [nvarchar](100) NULL,
	[EmployeeName] [nvarchar](100) NULL,
	[SexId] [int] NULL,
	[DateOfBirth] [datetime] NULL,
	[PersonalTaxCode] [nvarchar](100) NULL,
	[WorkUnitId] [int] NULL,
	[JobPositionId] [int] NULL,
	[NationId] [int] NULL,
	[ReligionId] [int] NULL,
	[NationalityId] [int] NULL,
	[IdentificationCardNumber] [nvarchar](100) NULL,
	[DateOfIssueOfIdentificationCard] [datetime] NULL,
	[PlaceOfIssueOfIdCard] [nvarchar](100) NULL,
	[IdentificationCardExpirationDate] [datetime] NULL,
	[PassportNumber] [nvarchar](100) NULL,
	[PassportDate] [datetime] NULL,
	[PlaceOfIssueOfPassport] [nvarchar](100) NULL,
	[PassportExpirationDate] [datetime] NULL,
	[EducationalLevelId] [int] NULL,
	[DegreeTrainingId] [int] NULL,
	[TrainingPlacesId] [int] NULL,
	[FacultyId] [int] NULL,
	[SpecializedId] [nvarchar](100) NULL,
	[GraduationYear] [nvarchar](100) NULL,
	[ClassificationId] [int] NULL,
	[MaritalStatusId] [int] NULL,
	[FamilyMemberId] [int] NULL,
	[IngredientsThemselvesId] [int] NULL,
	[EmployeeId] [int] NULL,
	[MobilePhone] [nvarchar](100) NULL,
	[OfficePhone] [nvarchar](100) NULL,
	[HomePhone] [nvarchar](100) NULL,
	[OtherPhone] [nvarchar](100) NULL,
	[PersonalEmail] [nvarchar](100) NULL,
	[CompanyEmail] [nvarchar](100) NULL,
	[OtherEmail] [nvarchar](100) NULL,
	[Skype] [nvarchar](100) NULL,
	[Facebook] [nvarchar](100) NULL,
	[Domicile] [nvarchar](100) NULL,
	[ProvinceCityId] [int] NULL,
	[PlaceBirth] [nvarchar](100) NULL,
	[ResidenceNationalityId] [int] NULL,
	[ResidenceProvinceCityId] [int] NULL,
	[ResidenceDistrictId] [int] NULL,
	[ResidenceWardsId] [int] NULL,
	[ResidenceHouseStreetVillageNumber] [nvarchar](100) NULL,
	[ResidenceAddress] [nvarchar](100) NULL,
	[ResidenceHouseholdRegistrationNumber] [nvarchar](100) NULL,
	[ResidenceHouseholdCode] [nvarchar](100) NULL,
	[ResidenceIsHeadHousehold] [bit] NULL,
	[CurrentNationalityId] [int] NULL,
	[CurrentProvinceCityId] [int] NULL,
	[CurrentDistrictId] [int] NULL,
	[CurrentWardsId] [int] NULL,
	[CurrentHouseStreetVillageNumber] [nvarchar](100) NULL,
	[CurrentAddress] [nvarchar](100) NULL,
	[UrgentContactFirstAndLastName] [nvarchar](100) NULL,
	[UrgentContactRelationshipId] [int] NULL,
	[UrgentContactMobilePhone] [nvarchar](100) NULL,
	[UrgentContactHomePhone] [nvarchar](100) NULL,
	[UrgentContactEmail] [nvarchar](100) NULL,
	[UrgentContactAddress] [nvarchar](100) NULL,
	[TimekeepingCode] [nvarchar](100) NULL,
	[WorkStatusId] [int] NULL,
	[DirectManagementId] [int] NULL,
	[IndirectManagementId] [int] NULL,
	[WorkLocationId] [int] NULL,
	[LaborManagementBookNumber] [nvarchar](100) NULL,
	[ContractTypeId] [int] NULL,
	[ApprenticeDay] [datetime] NULL,
	[ProbationDay] [datetime] NULL,
	[OfficialDate] [datetime] NULL,
	[NumberOfDaysOff] [datetime] NULL,
	[AutomaticallyIncreasesMagicAccordingToSeniority] [bit] NULL,
	[IncreaseLaterSpells] [nvarchar](100) NULL,
	[WageId] [int] NULL,
	[BasicSalary] [nvarchar](100) NULL,
	[InsurancePremiums] [nvarchar](100) NULL,
	[StandardPublicNumber] [nvarchar](100) NULL,
	[StandardPublicId] [int] NULL,
	[BankAccoun] [nvarchar](100) NULL,
	[BankId] [int] NULL,
	[JoinTheUnion] [bit] NULL,
	[DateOfInsurance] [datetime] NULL,
	[InsurancePremiumRate] [nvarchar](100) NULL,
	[SomeSocialInsuranceBooks] [nvarchar](100) NULL,
	[SocialInsuranceNumber] [nvarchar](100) NULL,
	[ProvinceCodeLevel] [nvarchar](100) NULL,
	[ProvinceNameLevelId] [int] NULL,
	[HealthInsuranceCardNumber] [nvarchar](100) NULL,
	[HealthInsuranceExpirationDate] [datetime] NULL,
	[PlaceOfRegistrationForMedicalExaminationAndTreatmentId] [int] NULL,
	[CodesOfMedicalExaminationAndTreatmentPlaces] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeQuitInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeQuitInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[NameOfProcedure] [nvarchar](100) NULL,
	[ProcedureGroupQuitId] [int] NULL,
	[Accomplished] [bit] NULL,
	[FinishDay] [date] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeReceiveInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeReceiveInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[NameOfProcedure] [nvarchar](100) NULL,
	[ProcedureGroupReceiveId] [int] NULL,
	[Accomplished] [bit] NULL,
	[FinishDay] [date] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeResignationProcedures]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeResignationProcedures] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[ExpectedResignationDate] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[DecisionNumber] [nvarchar](100) NULL,
	[DecisionDate] [date] NULL,
	[DayOff] [date] NULL,
	[ReviewerId] [int] NULL,
	[ReasonForRest] [nvarchar](100) NULL,
	[Comments] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeResignationProceduresEmployeeDebt]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeResignationProceduresEmployeeDebt] AS TABLE(
	[Id] [int] NULL,
	[ResignationProceduresId] [int] NULL,
	[NameOfTheDebtId] [int] NULL,
	[AmountOfMoney] [nvarchar](100) NULL,
	[FinishDay] [date] NULL,
	[Accomplished] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeSalaryHistoryInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeSalaryHistoryInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[JobPositionId] [int] NULL,
	[DateOfChange] [date] NULL,
	[BasicSalary] [nvarchar](100) NULL,
	[InsurancePremiums] [nvarchar](100) NULL,
	[JoinInsurance] [bit] NULL,
	[Explain] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeSkillInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeSkillInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[SkillName] [nvarchar](100) NULL,
	[SkillGroupId] [int] NULL,
	[SkillLevelId] [int] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeSkinInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeSkinInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[ShirtStringId] [int] NULL,
	[TrousersStringId] [int] NULL,
	[ZuypStringId] [int] NULL,
	[ProtectiveGearStringId] [int] NULL,
	[ShirtNumberId] [int] NULL,
	[TrousersNumberId] [int] NULL,
	[ZuypNumberId] [int] NULL,
	[ProtectiveGearNumberId] [int] NULL,
	[ShoulderWidth] [nvarchar](100) NULL,
	[LongSleeve] [nvarchar](100) NULL,
	[LongCoat] [nvarchar](100) NULL,
	[ChestRing] [nvarchar](100) NULL,
	[Waist] [nvarchar](100) NULL,
	[Buttocks] [nvarchar](100) NULL,
	[LongPants] [nvarchar](100) NULL,
	[LongSkirt] [nvarchar](100) NULL,
	[LapThigh] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeSortDescriptor]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeSortDescriptor] AS TABLE(
	[Member] [nvarchar](100) NULL,
	[Direction] [nvarchar](4) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeTrainingProcessInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeTrainingProcessInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[TrainingProcessCode] [nvarchar](100) NULL,
	[TrainingProcessName] [nvarchar](100) NULL,
	[StartDay] [date] NULL,
	[EndDate] [date] NULL,
	[Purpose] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeWorkExperienceInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeWorkExperienceInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[FromMonthAndYear] [date] NULL,
	[ByMonthAndYear] [date] NULL,
	[Workplace] [nvarchar](100) NULL,
	[JobPosition] [nvarchar](100) NULL,
	[Wage] [nvarchar](100) NULL,
	[JobDescription] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[FirstAndLastName] [nvarchar](100) NULL,
	[Title] [nvarchar](100) NULL,
	[Phone] [nvarchar](100) NULL,
	[Email] [nvarchar](100) NULL,
	[HaveCheckedCompared] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TypeWorkProgressInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
CREATE TYPE [dbo].[TypeWorkProgressInformation] AS TABLE(
	[Id] [int] NULL,
	[EmployeeId] [int] NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
	[JobPositionId] [int] NULL,
	[WorkUnitId] [int] NULL,
	[WorkStatusId] [int] NULL,
	[DirectManagementId] [int] NULL,
	[IndirectManagementId] [int] NULL,
	[DecisionNumber] [nvarchar](100) NULL,
	[DecisionDate] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_removeMark]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufn_removeMark] (@text nvarchar(max))
RETURNS nvarchar(max)
AS
BEGIN
	SET @text = LOWER(@text)
	DECLARE @textLen int = LEN(@text)
	IF @textLen > 0
	BEGIN
		DECLARE @index int = 1
		DECLARE @lPos int
		DECLARE @SIGN_CHARS nvarchar(100) = N'ăâđêôơưàảãạáằẳẵặắầẩẫậấèẻẽẹéềểễệếìỉĩịíòỏõọóồổỗộốờởỡợớùủũụúừửữựứỳỷỹỵýđð'
		DECLARE @UNSIGN_CHARS varchar(100) = 'aadeoouaaaaaaaaaaaaaaaeeeeeeeeeeiiiiiooooooooooooooouuuuuuuuuuyyyyydd'

		WHILE @index <= @textLen
		BEGIN
			SET @lPos = CHARINDEX(SUBSTRING(@text,@index,1),@SIGN_CHARS)
			IF @lPos > 0
			BEGIN
				SET @text = STUFF(@text,@index,1,SUBSTRING(@UNSIGN_CHARS,@lPos,1))
			END
			SET @index = @index + 1
		END
	END
	RETURN @text
END
GO
/****** Object:  Table [dbo].[AllowanceInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AllowanceInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[AllowanceTypeId] [int] NULL,
	[Price] [nvarchar](500) NULL,
	[StartDate] [date] NULL,
	[ToDate] [date] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_AllowanceInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AssetInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AssetInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[AssetCode] [nvarchar](100) NULL,
	[AssetName] [nvarchar](100) NULL,
	[AssetTypeId] [int] NULL,
	[ReceivedDate] [date] NULL,
	[PayDay] [date] NULL,
	[AssetStatusId] [int] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_AssetInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AttachmentInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AttachmentInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[EmployeesOnBusinessTripId] [int] NOT NULL,
	[FileName] [nvarchar](500) NULL,
	[FileType] [nvarchar](500) NULL,
	[FileSize] [bigint] NOT NULL,
	[FileContent] [varbinary](max) NULL,
	[IsDownload] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_AttachmentInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BonusInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BonusInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DecisionNumber] [nvarchar](100) NULL,
	[DecisionDate] [date] NULL,
	[ThePersonSignedTheDecisionId] [int] NULL,
	[BonusDay] [date] NULL,
	[RewardPlanId] [int] NULL,
	[BonusGrounds] [nvarchar](100) NULL,
	[RewardReasonId] [int] NULL,
	[CommendationFormId] [int] NULL,
	[BonusBudgetSourceId] [int] NULL,
	[TotalValue] [nvarchar](100) NULL,
	[StatusBonusId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_BonusInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BonusInformationStaffWereCommended]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BonusInformationStaffWereCommended](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BonusInformationId] [int] NULL,
	[EmployeeId] [int] NULL,
	[BonusValue] [nvarchar](100) NULL,
	[Status] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_BonusInformationStaffWereCommended] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CertificateInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CertificateInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[CertificateGroupId] [int] NULL,
	[CertificateName] [nvarchar](100) NULL,
	[NumberOfCertificates] [nvarchar](100) NULL,
	[DegreeTrainingId] [int] NULL,
	[DateRange] [date] NULL,
	[ExpirationDate] [date] NULL,
	[IssuedBy] [nvarchar](100) NULL,
	[ClassificationId] [int] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_CertificateInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ContactInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContactInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[MobilePhone] [nvarchar](50) NULL,
	[OfficePhone] [nvarchar](50) NULL,
	[HomePhone] [nvarchar](50) NULL,
	[OtherPhone] [nvarchar](50) NULL,
	[PersonalEmail] [nvarchar](50) NULL,
	[CompanyEmail] [nvarchar](50) NULL,
	[OtherEmail] [nvarchar](50) NULL,
	[Skype] [nvarchar](50) NULL,
	[Facebook] [nvarchar](50) NULL,
	[Domicile] [nvarchar](50) NULL,
	[ProvinceCityId] [int] NULL,
	[PlaceBirth] [nvarchar](50) NULL,
	[ResidenceNationalityId] [int] NULL,
	[ResidenceProvinceCityId] [int] NULL,
	[ResidenceDistrictId] [int] NULL,
	[ResidenceWardsId] [int] NULL,
	[ResidenceHouseStreetVillageNumber] [nvarchar](50) NULL,
	[ResidenceAddress] [nvarchar](max) NULL,
	[ResidenceHouseholdRegistrationNumber] [nvarchar](50) NULL,
	[ResidenceHouseholdCode] [nvarchar](50) NULL,
	[ResidenceIsHeadHousehold] [bit] NULL,
	[CurrentNationalityId] [int] NULL,
	[CurrentProvinceCityId] [int] NULL,
	[CurrentDistrictId] [int] NULL,
	[CurrentWardsId] [int] NULL,
	[CurrentHouseStreetVillageNumber] [nvarchar](50) NULL,
	[CurrentAddress] [nvarchar](max) NULL,
	[UrgentContactFirstAndLastName] [nvarchar](50) NULL,
	[UrgentContactRelationshipId] [int] NULL,
	[UrgentContactMobilePhone] [nvarchar](50) NULL,
	[UrgentContactHomePhone] [nvarchar](50) NULL,
	[UrgentContactEmail] [nvarchar](50) NULL,
	[UrgentContactAddress] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ContactInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ContractInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[JobPositionId] [int] NULL,
	[SomeContracts] [nvarchar](100) NULL,
	[WorkUnitId] [int] NULL,
	[SignDay] [date] NULL,
	[ContractName] [nvarchar](100) NULL,
	[ContractTypeId] [int] NULL,
	[ContractTermId] [int] NULL,
	[TheFormOfWorkId] [int] NULL,
	[WageRate] [nvarchar](100) NULL,
	[EffectiveDate] [date] NULL,
	[ExpirationDate] [date] NULL,
	[Abstract] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ContractInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DegreeInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DegreeInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[TrainingPlacesId] [int] NULL,
	[FromYear] [nvarchar](100) NULL,
	[ToYear] [nvarchar](100) NULL,
	[FacultyId] [int] NULL,
	[SpecializedId] [int] NULL,
	[DegreeTrainingId] [int] NULL,
	[FormsOfTrainingId] [int] NULL,
	[ClassificationId] [int] NULL,
	[Graduated] [bit] NULL,
	[DateReceived] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_DegreeInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[District]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[District](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProvinceCityId] [int] NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_District] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Image] [nvarchar](max) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[EmployeeName] [nvarchar](50) NULL,
	[SexId] [int] NULL,
	[DateOfBirth] [date] NULL,
	[PersonalTaxCode] [nvarchar](50) NULL,
	[WorkUnitId] [int] NULL,
	[JobPositionId] [int] NULL,
	[NationId] [int] NULL,
	[ReligionId] [int] NULL,
	[NationalityId] [int] NULL,
	[IdentificationCardNumber] [nvarchar](50) NULL,
	[DateOfIssueOfIdentificationCard] [date] NULL,
	[PlaceOfIssueOfIdCard] [nvarchar](50) NULL,
	[IdentificationCardExpirationDate] [date] NULL,
	[PassportNumber] [nvarchar](50) NULL,
	[PassportDate] [date] NULL,
	[PlaceOfIssueOfPassport] [nvarchar](50) NULL,
	[PassportExpirationDate] [date] NULL,
	[EducationalLevelId] [int] NULL,
	[DegreeTrainingId] [int] NULL,
	[TrainingPlacesId] [int] NULL,
	[FacultyId] [int] NULL,
	[SpecializedId] [int] NULL,
	[GraduationYear] [nvarchar](50) NULL,
	[ClassificationId] [int] NULL,
	[MaritalStatusId] [int] NULL,
	[FamilyMemberId] [int] NULL,
	[IngredientsThemselvesId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_EmployeeInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeesOnBusinessTrip]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeesOnBusinessTrip](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[EmployeeApprovedId] [int] NULL,
	[DayTo] [date] NULL,
	[ReturnDate] [date] NULL,
	[WorkingPlace] [nvarchar](100) NULL,
	[WorkingPurpose] [nvarchar](100) NULL,
	[RecommendedDate] [date] NULL,
	[Deadline] [date] NULL,
	[AmountProposedForAdvance] [nvarchar](100) NULL,
	[AmountOfAdvance] [nvarchar](100) NULL,
	[ReasonForAdvance] [nvarchar](100) NULL,
	[RequireToBeSupported] [nvarchar](max) NULL,
	[BrowsingStatusId] [int] NULL,
	[ReasonsForNotBrowsing] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_EmployeesOnBusinessTrip] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeesOnBusinessTripAdvances]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeesOnBusinessTripAdvances](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[Expenses] [nvarchar](100) NULL,
	[Unit] [nvarchar](100) NULL,
	[Amount] [nvarchar](100) NULL,
	[UnitPrice] [nvarchar](100) NULL,
	[Money] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_EmployeesOnBusinessTripAdvances] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeesOnBusinessTripAssignedStaff]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeesOnBusinessTripAssignedStaff](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[EmployeeId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_EmployeesOnBusinessTripAssignedStaff] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeesOnBusinessTripPayments]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeesOnBusinessTripPayments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[Expenses] [nvarchar](100) NULL,
	[VoucherNumber] [nvarchar](100) NULL,
	[DayVouchers] [date] NULL,
	[AmountSpent] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_EmployeesOnBusinessTripPayments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeesOnBusinessTripRevenueEstimates]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeesOnBusinessTripRevenueEstimates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeesOnBusinessTripId] [int] NULL,
	[Revenue] [nvarchar](100) NULL,
	[AmountOfMoney] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_EmployeesOnBusinessTripRevenueEstimates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Evaluate]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Evaluate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NameOfAudit] [nvarchar](100) NULL,
	[EvaluationPeriodId] [int] NULL,
	[WorkUnitId] [int] NULL,
	[EvaluationStatusId] [int] NULL,
	[PersonInChargeId] [int] NULL,
	[Since] [date] NULL,
	[ToDate] [date] NULL,
	[EvaluationTerm] [date] NULL,
	[BriefDescription] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_Evaluate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EvaluateDetail]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EvaluateDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[EvaluateId] [int] NULL,
	[Result] [nvarchar](500) NULL,
	[EvaluateStatus] [nvarchar](500) NULL,
	[ResultJson] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_EvaluateDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FamilyInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FamilyInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[RelationshipId] [int] NULL,
	[FirstAndLastName] [nvarchar](100) NULL,
	[DateOfBirth] [date] NULL,
	[SexId] [int] NULL,
	[NationalityId] [int] NULL,
	[IdPassportNumber] [nvarchar](100) NULL,
	[Address] [nvarchar](100) NULL,
	[MobilePhone] [nvarchar](100) NULL,
	[HomePhone] [nvarchar](100) NULL,
	[Email] [nvarchar](100) NULL,
	[Job] [nvarchar](100) NULL,
	[PersonalTaxCode] [nvarchar](100) NULL,
	[Workplace] [nvarchar](100) NULL,
	[SameHouseholdRegistrationBook] [bit] NULL,
	[BeTheHeadOfTheHousehold] [bit] NULL,
	[IsADependent] [bit] NULL,
	[TimeToCalculateDeduction] [date] NULL,
	[TimeToEndTheDeduction] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[IsDead] [bit] NULL,
	[DeadDate] [date] NULL,
	[AsAnEmergencyContact] [bit] NULL,
	[Number] [nvarchar](100) NULL,
	[NumberBook] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_FamilyInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Functions]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Functions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](50) NOT NULL,
	[Caption] [nvarchar](200) NULL,
	[ParentId] [bigint] NULL,
	[Enable] [bit] NULL,
	[NodePath] [varchar](50) NULL,
	[Url] [varchar](500) NULL,
	[Order] [int] NULL,
	[IsApi] [bit] NULL,
	[Icon] [varchar](50) NULL,
	[IsShow] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_Functions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JobInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[TimekeepingCode] [nvarchar](100) NULL,
	[WorkStatusId] [int] NULL,
	[DirectManagementId] [int] NULL,
	[IndirectManagementId] [int] NULL,
	[WorkLocationId] [int] NULL,
	[LaborManagementBookNumber] [nvarchar](100) NULL,
	[ContractTypeId] [int] NULL,
	[ApprenticeDay] [date] NULL,
	[ProbationDay] [date] NULL,
	[OfficialDate] [date] NULL,
	[NumberOfDaysOff] [nvarchar](100) NULL,
	[AutomaticallyIncreasesMagicAccordingToSeniority] [bit] NULL,
	[IncreaseLaterSpells] [nvarchar](100) NULL,
	[WageId] [int] NULL,
	[BasicSalary] [nvarchar](100) NULL,
	[InsurancePremiums] [nvarchar](100) NULL,
	[StandardPublicNumber] [nvarchar](100) NULL,
	[StandardPublicId] [int] NULL,
	[BankAccoun] [nvarchar](100) NULL,
	[BankId] [int] NULL,
	[JoinTheUnion] [bit] NULL,
	[DateOfInsurance] [date] NULL,
	[InsurancePremiumRate] [nvarchar](100) NULL,
	[SomeSocialInsuranceBooks] [nvarchar](100) NULL,
	[SocialInsuranceNumber] [nvarchar](100) NULL,
	[ProvinceCodeLevel] [nvarchar](100) NULL,
	[ProvinceNameLevelId] [int] NULL,
	[HealthInsuranceCardNumber] [nvarchar](100) NULL,
	[HealthInsuranceExpirationDate] [date] NULL,
	[PlaceOfRegistrationForMedicalExaminationAndTreatmentId] [int] NULL,
	[CodesOfMedicalExaminationAndTreatmentPlaces] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_JobInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ListCategory]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ListCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](100) NULL,
	[Name] [nvarchar](100) NULL,
	[Address] [nvarchar](100) NULL,
	[ListCategoryTypeId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ListCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ListCategoryType]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ListCategoryType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](100) NULL,
	[Name] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ListCategoryType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ListStatus]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ListStatus](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Type] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ListStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Nationality]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Nationality](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_Nationality] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PageInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PageInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[NameOfPapers] [nvarchar](100) NULL,
	[IssuedBy] [nvarchar](100) NULL,
	[DateRange] [date] NULL,
	[ExpirationDate] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_PageInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PoliticsHealthMilitaryInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PoliticsHealthMilitaryInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[IsAUnionMember] [bit] NULL,
	[DayToUnion] [date] NULL,
	[GroupPositionId] [int] NULL,
	[PlaceOfUnionAdmission] [nvarchar](100) NULL,
	[AsAPartyMember] [bit] NULL,
	[DayToParty] [date] NULL,
	[PartyPositionId] [int] NULL,
	[PlaceOfAdmissionToTheParty] [nvarchar](100) NULL,
	[BloodGroupId] [int] NULL,
	[Height] [nvarchar](100) NULL,
	[Weight] [nvarchar](100) NULL,
	[HealthStatus] [nvarchar](100) NULL,
	[Diseases] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[PeopleWithDisabilities] [bit] NULL,
	[AsASsoldier] [bit] NULL,
	[DateOfEnlistment] [date] NULL,
	[ArmyId] [int] NULL,
	[MilitaryUnit] [nvarchar](100) NULL,
	[MilitaryRankId] [int] NULL,
	[MilitaryPositionId] [int] NULL,
	[DateOfDemobilization] [date] NULL,
	[TheReason] [nvarchar](100) NULL,
	[AsWoundedSoldiersSickSoldiers] [bit] NULL,
	[DateToJoinRevolution] [date] NULL,
	[RankId] [int] NULL,
	[RateOfLaborDecline] [nvarchar](100) NULL,
	[EnjoyTheMode] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_PoliticsHealthMilitaryInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProblemName] [nvarchar](100) NULL,
	[TypeOfIncidentId] [int] NULL,
	[HappenDay] [date] NULL,
	[WhereHappened] [nvarchar](100) NULL,
	[Reason] [nvarchar](100) NULL,
	[DescriptionOfTheProblem] [nvarchar](100) NULL,
	[RelatedUnitId] [int] NULL,
	[TotalValueOfDamage] [nvarchar](100) NULL,
	[TotalCompensationValue] [nvarchar](100) NULL,
	[CompensationStatusId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ProblemInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemInformationRelatedStaff]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemInformationRelatedStaff](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProblemInformationId] [int] NULL,
	[EmployeeId] [int] NULL,
	[DescribeTheRelationship] [nvarchar](100) NULL,
	[TotalNumberOfDaysOffDueToOccupationalAccidents] [nvarchar](100) NULL,
	[InjuryConditionId] [int] NULL,
	[ProcessingStatusId] [int] NULL,
	[HavePassedLaborSafetyTraining] [bit] NULL,
	[DecisionNumber] [nvarchar](100) NULL,
	[DecisionDate] [date] NULL,
	[KindOfDecisionId] [int] NULL,
	[EffectiveDate] [date] NULL,
	[FormsProcessingId] [int] NULL,
	[TheDecisionId] [int] NULL,
	[CitationOfContent] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ProblemInformationRelatedStaff] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemInformationTrackEmployeeCompensation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemInformationTrackEmployeeCompensation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProblemInformationId] [int] NULL,
	[EmployeeId] [int] NULL,
	[AmountMoney] [nvarchar](100) NULL,
	[PayDay] [date] NULL,
	[SourceCompensation] [nvarchar](100) NULL,
	[Type] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ProblemInformationTrackEmployeeCompensation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProvinceCity]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProvinceCity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](100) NULL,
	[NationalityId] [int] NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_ProvinceCity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuitInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuitInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[NameOfProcedure] [nvarchar](100) NULL,
	[ProcedureGroupQuitId] [int] NULL,
	[Accomplished] [bit] NULL,
	[FinishDay] [date] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_QuitInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReceiveInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReceiveInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[NameOfProcedure] [nvarchar](100) NULL,
	[ProcedureGroupReceiveId] [int] NULL,
	[Accomplished] [bit] NULL,
	[FinishDay] [date] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ReceiveInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResignationProcedures]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResignationProcedures](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[ExpectedResignationDate] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[DecisionNumber] [nvarchar](100) NULL,
	[DecisionDate] [date] NULL,
	[DayOff] [date] NULL,
	[ReviewerId] [int] NULL,
	[ReasonForRest] [nvarchar](100) NULL,
	[Comments] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ResignationProcedures] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResignationProceduresEmployeeDebt]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResignationProceduresEmployeeDebt](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResignationProceduresId] [int] NULL,
	[NameOfTheDebtId] [int] NULL,
	[AmountOfMoney] [nvarchar](100) NULL,
	[FinishDay] [date] NULL,
	[Accomplished] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_ResignationProceduresEmployeeDebt] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SalaryHistoryInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SalaryHistoryInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[JobPositionId] [int] NULL,
	[DateOfChange] [date] NULL,
	[BasicSalary] [nvarchar](100) NULL,
	[InsurancePremiums] [nvarchar](100) NULL,
	[JoinInsurance] [bit] NULL,
	[Explain] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_SalaryHistoryInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SkillInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SkillInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[SkillName] [nvarchar](100) NULL,
	[SkillGroupId] [int] NULL,
	[SkillLevelId] [int] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_SkillInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SkinInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SkinInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[ShirtStringId] [int] NULL,
	[TrousersStringId] [int] NULL,
	[ZuypStringId] [int] NULL,
	[ProtectiveGearStringId] [int] NULL,
	[ShirtNumberId] [int] NULL,
	[TrousersNumberId] [int] NULL,
	[ZuypNumberId] [int] NULL,
	[ProtectiveGearNumberId] [int] NULL,
	[ShoulderWidth] [nvarchar](100) NULL,
	[LongSleeve] [nvarchar](100) NULL,
	[LongCoat] [nvarchar](100) NULL,
	[ChestRing] [nvarchar](100) NULL,
	[Waist] [nvarchar](100) NULL,
	[Buttocks] [nvarchar](100) NULL,
	[LongPants] [nvarchar](100) NULL,
	[LongSkirt] [nvarchar](100) NULL,
	[LapThigh] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_SkinInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TrainingProcessInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TrainingProcessInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[TrainingProcessCode] [nvarchar](100) NULL,
	[TrainingProcessName] [nvarchar](100) NULL,
	[StartDay] [date] NULL,
	[EndDate] [date] NULL,
	[Purpose] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_TrainingProcessInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Wards]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Wards](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DistrictId] [int] NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_Wards] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WorkExperienceInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkExperienceInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[FromMonthAndYear] [date] NULL,
	[ByMonthAndYear] [date] NULL,
	[Workplace] [nvarchar](100) NULL,
	[JobPosition] [nvarchar](100) NULL,
	[Wage] [nvarchar](100) NULL,
	[JobDescription] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[FirstAndLastName] [nvarchar](100) NULL,
	[Title] [nvarchar](100) NULL,
	[Phone] [nvarchar](100) NULL,
	[Email] [nvarchar](100) NULL,
	[HaveCheckedCompared] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_WorkExperienceInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WorkProgressInformation]    Script Date: 6/10/2022 2:47:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkProgressInformation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
	[JobPositionId] [int] NULL,
	[WorkUnitId] [int] NULL,
	[WorkStatusId] [int] NULL,
	[DirectManagementId] [int] NULL,
	[IndirectManagementId] [int] NULL,
	[DecisionNumber] [nvarchar](100) NULL,
	[DecisionDate] [date] NULL,
	[Note] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_WorkProgressInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spCheckDuplicate]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spCheckDuplicate]
	@Id INT,
	@TableName NVARCHAR(100),
	@Name NVARCHAR(100),
	@ListCategoryTypeId INT
AS
BEGIN  
	SELECT
		(
			CASE
				WHEN @TableName = 'ListCategory' AND @Id > 0 AND (SELECT COUNT(*) FROM ListCategory WHERE Id <> @Id AND [Name] = @Name AND ListCategoryTypeId = @ListCategoryTypeId AND ISNULL(IsDeleted, 0) = 0) > 0 THEN 1
				WHEN @TableName = 'ListCategory' AND @Id = 0 AND (SELECT COUNT(*) FROM ListCategory WHERE [Name] = @Name AND ListCategoryTypeId = @ListCategoryTypeId AND ISNULL(IsDeleted, 0) = 0) > 0 THEN 1
				WHEN @TableName = 'AssetInformation' AND @Id > 0 AND (SELECT COUNT(*) FROM AssetInformation WHERE Id <> @Id AND [AssetCode] = @Name AND [AssetTypeId] = @ListCategoryTypeId AND ISNULL(IsDeleted, 0) = 0) > 0 THEN 1
				WHEN @TableName = 'AssetInformation' AND @Id = 0 AND (SELECT COUNT(*) FROM AssetInformation WHERE [AssetCode] = @Name AND [AssetTypeId] = @ListCategoryTypeId AND ISNULL(IsDeleted, 0) = 0) > 0 THEN 1
				WHEN @TableName = 'Evaluate' AND @Id > 0 AND (SELECT COUNT(*) FROM Evaluate WHERE Id <> @Id AND NameOfAudit = @Name AND EvaluationPeriodId = @ListCategoryTypeId AND ISNULL(IsDeleted, 0) = 0) > 0 THEN 1
				WHEN @TableName = 'Evaluate' AND @Id = 0 AND (SELECT COUNT(*) FROM Evaluate WHERE NameOfAudit = @Name AND EvaluationPeriodId = @ListCategoryTypeId AND ISNULL(IsDeleted, 0) = 0) > 0 THEN 1
				WHEN @TableName = 'EvaluateDetail' AND @Id = 0 AND (SELECT COUNT(*) FROM EvaluateDetail WHERE EmployeeId = @Name AND EvaluateId = @ListCategoryTypeId AND ISNULL(IsDeleted, 0) = 0) > 0 THEN 1
			ELSE 0
			END
		) AS Duplicate
END

GO
/****** Object:  StoredProcedure [dbo].[spDownloadAttachmentInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spDownloadAttachmentInformation]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT TOP 1 [FileName], FileContent
	FROM AttachmentInformation
	WHERE Id = @Id

END

GO
/****** Object:  StoredProcedure [dbo].[spGetAllBonus]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllBonus]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_StatusBonusName_Val NVARCHAR(100),
		@Filter_StatusBonusName_Ope NVARCHAR(100),
		@Filter_BonusDay_Val NVARCHAR(100),	
		@Filter_BonusDay_Ope NVARCHAR(100),	
		@Filter_DecisionNumber_Val NVARCHAR(100),	
		@Filter_DecisionNumber_Ope NVARCHAR(100),	
		@Filter_DecisionDate_Val NVARCHAR(100),	
		@Filter_DecisionDate_Ope NVARCHAR(100),	
		@Filter_RewardReasonName_Val NVARCHAR(100),	
		@Filter_RewardReasonName_Ope NVARCHAR(100),	
		@Filter_CommendationFormName_Val NVARCHAR(100),	
		@Filter_CommendationFormName_Ope NVARCHAR(100),	
		@Filter_TotalValue_Val NVARCHAR(100),	
		@Filter_TotalValue_Ope NVARCHAR(100),	

		@Member_StatusBonusName_Col NVARCHAR(100) = N'StatusBonusName',
		@Member_BonusDay_Col NVARCHAR(100) = N'BonusDay',
		@Member_DecisionNumber_Col NVARCHAR(100) = N'DecisionNumber',
		@Member_DecisionDate_Col NVARCHAR(100) = N'DecisionDate',
		@Member_RewardReasonName_Col NVARCHAR(100) = N'RewardReasonName',
		@Member_CommendationFormName_Col NVARCHAR(100) = N'CommendationFormName',
		@Member_TotalValue_Col NVARCHAR(100) = N'TotalValue'

	SELECT 
		@Filter_StatusBonusName_Val = TRIM([Value]),
		@Filter_StatusBonusName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_StatusBonusName_Col
	IF @Filter_StatusBonusName_Ope = @Contains SET @Filter_StatusBonusName_Val = N'%' + @Filter_StatusBonusName_Val + '%'

	SELECT 
		@Filter_BonusDay_Val = TRIM([Value]),
		@Filter_BonusDay_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_BonusDay_Col
	IF @Filter_BonusDay_Ope = @Contains SET @Filter_BonusDay_Val = @Filter_BonusDay_Val

	SELECT 
		@Filter_DecisionNumber_Val = TRIM([Value]),
		@Filter_DecisionNumber_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_DecisionNumber_Col
	IF @Filter_DecisionNumber_Ope = @Contains SET @Filter_DecisionNumber_Val = N'%' + @Filter_DecisionNumber_Val + '%'

	SELECT 
		@Filter_DecisionDate_Val = TRIM([Value]),
		@Filter_DecisionDate_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_DecisionDate_Col
	IF @Filter_DecisionDate_Ope = @Contains SET @Filter_DecisionDate_Val = @Filter_DecisionDate_Val
	
	SELECT 
		@Filter_RewardReasonName_Val = TRIM([Value]),
		@Filter_RewardReasonName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_RewardReasonName_Col
	IF @Filter_RewardReasonName_Ope = @Contains SET @Filter_RewardReasonName_Val = N'%' + @Filter_RewardReasonName_Val + '%'

	SELECT 
		@Filter_CommendationFormName_Val = TRIM([Value]),
		@Filter_CommendationFormName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_CommendationFormName_Col
	IF @Filter_CommendationFormName_Ope = @Contains SET @Filter_CommendationFormName_Val = N'%' + @Filter_CommendationFormName_Val + '%'

	SELECT 
		@Filter_TotalValue_Val = TRIM([Value]),
		@Filter_TotalValue_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_TotalValue_Col
	IF @Filter_TotalValue_Ope = @Contains SET @Filter_TotalValue_Val = N'%' + REPLACE(CONVERT(NVARCHAR(100),CAST(@Filter_TotalValue_Val AS MONEY),1),'.00','') + '%'

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM BonusInformation AS b (NOLOCK)
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = b.StatusBonusId AND ISNULL(lc1.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = b.RewardReasonId AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = b.CommendationFormId AND ISNULL(lc3.IsDeleted,0) = 0
		WHERE ISNULL(b.IsDeleted,0) = 0
		--Filter
			AND (@Filter_StatusBonusName_Val IS NULL OR (@Filter_StatusBonusName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_StatusBonusName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_StatusBonusName_Val)))
			AND (@Filter_BonusDay_Val IS NULL OR (@Filter_BonusDay_Ope = @Contains AND CONVERT(VARCHAR(100),b.BonusDay,103) = @Filter_BonusDay_Val))
			AND (@Filter_DecisionNumber_Val IS NULL OR (@Filter_DecisionNumber_Ope = @Contains AND (b.DecisionNumber LIKE @Filter_DecisionNumber_Val OR dbo.ufn_removeMark(b.DecisionNumber) LIKE @Filter_DecisionNumber_Val)))
			AND (@Filter_DecisionDate_Val IS NULL OR (@Filter_DecisionDate_Ope = @Contains AND CONVERT(VARCHAR(100),b.DecisionDate,103) = @Filter_DecisionDate_Val))
			AND (@Filter_RewardReasonName_Val IS NULL OR (@Filter_RewardReasonName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_RewardReasonName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_RewardReasonName_Val)))
			AND (@Filter_CommendationFormName_Val IS NULL OR (@Filter_CommendationFormName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_CommendationFormName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_CommendationFormName_Val)))
			AND (@Filter_TotalValue_Val IS NULL OR (@Filter_TotalValue_Ope = @Contains AND (b.TotalValue LIKE @Filter_TotalValue_Val OR REPLACE(CONVERT(NVARCHAR(100),CAST(b.TotalValue AS MONEY),1),'.00','') LIKE @Filter_TotalValue_Val)))
	);

	WITH TempResult AS (
		SELECT
		 b.Id,
		 lc1.[Name] AS StatusBonusName,
		 b.BonusDay,
		 b.DecisionNumber,
		 b.DecisionDate,
		 lc2.[Name] AS RewardReasonName,
		 lc3.[Name] AS CommendationFormName,
		 REPLACE(CONVERT(NVARCHAR(100),CAST(b.TotalValue AS MONEY),1),'.00','') AS TotalValue
	FROM BonusInformation AS b (NOLOCK)
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = b.StatusBonusId AND ISNULL(lc1.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = b.RewardReasonId AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = b.CommendationFormId AND ISNULL(lc3.IsDeleted,0) = 0
	WHERE ISNULL(b.IsDeleted,0) = 0
	--Filter
		AND (@Filter_StatusBonusName_Val IS NULL OR (@Filter_StatusBonusName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_StatusBonusName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_StatusBonusName_Val)))
		AND (@Filter_BonusDay_Val IS NULL OR (@Filter_BonusDay_Ope = @Contains AND CONVERT(VARCHAR(100),b.BonusDay,103) = @Filter_BonusDay_Val))
		AND (@Filter_DecisionNumber_Val IS NULL OR (@Filter_DecisionNumber_Ope = @Contains AND (b.DecisionNumber LIKE @Filter_DecisionNumber_Val OR dbo.ufn_removeMark(b.DecisionNumber) LIKE @Filter_DecisionNumber_Val)))
		AND (@Filter_DecisionDate_Val IS NULL OR (@Filter_DecisionDate_Ope = @Contains AND CONVERT(VARCHAR(100),b.DecisionDate,103) = @Filter_DecisionDate_Val))
		AND (@Filter_RewardReasonName_Val IS NULL OR (@Filter_RewardReasonName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_RewardReasonName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_RewardReasonName_Val)))
		AND (@Filter_CommendationFormName_Val IS NULL OR (@Filter_CommendationFormName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_CommendationFormName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_CommendationFormName_Val)))
		AND (@Filter_TotalValue_Val IS NULL OR (@Filter_TotalValue_Ope = @Contains AND (b.TotalValue LIKE @Filter_TotalValue_Val OR REPLACE(CONVERT(NVARCHAR(100),CAST(b.TotalValue AS MONEY),1),'.00','') LIKE @Filter_TotalValue_Val)))
	
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_StatusBonusName_Col THEN lc1.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_BonusDay_Col THEN b.BonusDay END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_DecisionNumber_Col THEN b.DecisionNumber END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_DecisionDate_Col THEN b.DecisionDate END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_RewardReasonName_Col THEN lc2.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_CommendationFormName_Col THEN lc3.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_TotalValue_Col THEN b.TotalValue END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_StatusBonusName_Col THEN lc1.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_BonusDay_Col THEN b.BonusDay END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_DecisionNumber_Col THEN b.DecisionNumber END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_DecisionDate_Col THEN b.DecisionDate END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_RewardReasonName_Col THEN lc2.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_CommendationFormName_Col THEN lc3.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_TotalValue_Col THEN b.TotalValue END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN b.Id END DESC	

	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY
	)
	
	SELECT * FROM TempResult
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllDistrict]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllDistrict]
	@ProvinceCityId INT
AS
BEGIN  
	SELECT
		Id,
		[Name]
	FROM District
	WHERE ProvinceCityId = @ProvinceCityId
	ORDER BY [Name] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllEmployeeInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllEmployeeInformation]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_EmployeeCode_Val NVARCHAR(100),
		@Filter_EmployeeCode_Ope NVARCHAR(100),
		@Filter_EmployeeName_Val NVARCHAR(100),	
		@Filter_EmployeeName_Ope NVARCHAR(100),	
		@Filter_SexName_Val NVARCHAR(100),	
		@Filter_SexName_Ope NVARCHAR(100),	
		@Filter_DateOfBirth_Val NVARCHAR(100),	
		@Filter_DateOfBirth_Ope NVARCHAR(100),	
		@Filter_JobPositionName_Val NVARCHAR(100),	
		@Filter_JobPositionName_Ope NVARCHAR(100),	
		@Filter_WorkUnitName_Val NVARCHAR(100),	
		@Filter_WorkUnitName_Ope NVARCHAR(100),	
		@Filter_WorkStatusName_Val NVARCHAR(100),	
		@Filter_WorkStatusName_Ope NVARCHAR(100),	
		@Filter_DegreeTrainingName_Val NVARCHAR(100),	
		@Filter_DegreeTrainingName_Ope NVARCHAR(100),	
		@Filter_TrainingPlacesName_Val NVARCHAR(100),	
		@Filter_TrainingPlacesName_Ope NVARCHAR(100),	
		@Filter_SpecializedName_Val NVARCHAR(100),	
		@Filter_SpecializedName_Ope NVARCHAR(100),	
		@Filter_ProbationDay_Val NVARCHAR(100),	
		@Filter_ProbationDay_Ope NVARCHAR(100),	
		@Filter_OfficialDate_Val NVARCHAR(100),	
		@Filter_OfficialDate_Ope NVARCHAR(100),	
		@Filter_ContractTypeName_Val NVARCHAR(100),	
		@Filter_ContractTypeName_Ope NVARCHAR(100),	

		@Member_EmployeeCode_Col NVARCHAR(100) = N'EmployeeCode',
		@Member_EmployeeName_Col NVARCHAR(100) = N'EmployeeName',
		@Member_SexName_Col NVARCHAR(100) = N'SexName',
		@Member_DateOfBirth_Col NVARCHAR(100) = N'DateOfBirth',
		@Member_JobPositionName_Col NVARCHAR(100) = N'JobPositionName',
		@Member_WorkUnitName_Col NVARCHAR(100) = N'WorkUnitName',
		@Member_WorkStatusName_Col NVARCHAR(100) = N'WorkStatusName',
		@Member_DegreeTrainingName_Col NVARCHAR(100) = N'DegreeTrainingName',
		@Member_TrainingPlacesName_Col NVARCHAR(100) = N'TrainingPlacesName',
		@Member_SpecializedName_Col NVARCHAR(100) = N'SpecializedName',
		@Member_ProbationDay_Col NVARCHAR(100) = N'ProbationDay',
		@Member_OfficialDate_Col NVARCHAR(100) = N'OfficialDate',
		@Member_ContractTypeName_Col NVARCHAR(100) = N'ContractTypeName'

	SELECT 
		@Filter_EmployeeCode_Val = TRIM([Value]),
		@Filter_EmployeeCode_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeCode_Col
	IF @Filter_EmployeeCode_Ope = @Contains SET @Filter_EmployeeCode_Val = N'%' + @Filter_EmployeeCode_Val + '%'

	SELECT 
		@Filter_EmployeeName_Val = TRIM([Value]),
		@Filter_EmployeeName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeName_Col
	IF @Filter_EmployeeName_Ope = @Contains SET @Filter_EmployeeName_Val = N'%' + @Filter_EmployeeName_Val + '%'

	SELECT 
		@Filter_SexName_Val = TRIM([Value]),
		@Filter_SexName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_SexName_Col
	IF @Filter_SexName_Ope = @Contains SET @Filter_SexName_Val = N'%' + @Filter_SexName_Val + '%'

	SELECT 
		@Filter_DateOfBirth_Val = TRIM([Value]),
		@Filter_DateOfBirth_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_DateOfBirth_Col
	IF @Filter_DateOfBirth_Ope = @Contains SET @Filter_DateOfBirth_Val = @Filter_DateOfBirth_Val
	
	SELECT 
		@Filter_JobPositionName_Val = TRIM([Value]),
		@Filter_JobPositionName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_JobPositionName_Col
	IF @Filter_JobPositionName_Ope = @Contains SET @Filter_JobPositionName_Val = N'%' + @Filter_JobPositionName_Val + '%'

	SELECT 
		@Filter_WorkUnitName_Val = TRIM([Value]),
		@Filter_WorkUnitName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_WorkUnitName_Col
	IF @Filter_WorkUnitName_Ope = @Contains SET @Filter_WorkUnitName_Val = N'%' + @Filter_WorkUnitName_Val + '%'

	SELECT 
		@Filter_WorkStatusName_Val = TRIM([Value]),
		@Filter_WorkStatusName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_WorkStatusName_Col
	IF @Filter_WorkStatusName_Ope = @Contains SET @Filter_WorkStatusName_Val = N'%' + @Filter_WorkStatusName_Val + '%'

	SELECT 
		@Filter_DegreeTrainingName_Val = TRIM([Value]),
		@Filter_DegreeTrainingName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_DegreeTrainingName_Col
	IF @Filter_DegreeTrainingName_Ope = @Contains SET @Filter_DegreeTrainingName_Val = N'%' + @Filter_DegreeTrainingName_Val + '%'

	SELECT 
		@Filter_TrainingPlacesName_Val = TRIM([Value]),
		@Filter_TrainingPlacesName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_TrainingPlacesName_Col
	IF @Filter_TrainingPlacesName_Ope = @Contains SET @Filter_TrainingPlacesName_Val = N'%' + @Filter_TrainingPlacesName_Val + '%'

	SELECT 
		@Filter_SpecializedName_Val = TRIM([Value]),
		@Filter_SpecializedName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_SpecializedName_Col
	IF @Filter_SpecializedName_Ope = @Contains SET @Filter_SpecializedName_Val = N'%' + @Filter_SpecializedName_Val + '%'

	SELECT 
		@Filter_ProbationDay_Val = TRIM([Value]),
		@Filter_ProbationDay_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_ProbationDay_Col
	IF @Filter_ProbationDay_Ope = @Contains SET @Filter_ProbationDay_Val = @Filter_ProbationDay_Val

	SELECT 
		@Filter_OfficialDate_Val = TRIM([Value]),
		@Filter_OfficialDate_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_OfficialDate_Col
	IF @Filter_OfficialDate_Ope = @Contains SET @Filter_OfficialDate_Val = @Filter_OfficialDate_Val

	SELECT 
		@Filter_ContractTypeName_Val = TRIM([Value]),
		@Filter_ContractTypeName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_ContractTypeName_Col
	IF @Filter_ContractTypeName_Ope = @Contains SET @Filter_ContractTypeName_Val = N'%' + @Filter_ContractTypeName_Val + '%'

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM EmployeeInformation AS e (NOLOCK)
		LEFT JOIN JobInformation AS j (NOLOCK) ON j.EmployeeId = e.Id AND ISNULL(j.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.[SexId] AND ISNULL(lc1.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = e.[JobPositionId] AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = e.[WorkUnitId] AND ISNULL(lc3.IsDeleted,0) = 0
		LEFT JOIN ListStatus AS lc4 (NOLOCK) ON lc4.Id = j.[WorkStatusId] AND ISNULL(lc4.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc5 (NOLOCK) ON lc5.Id = e.[DegreeTrainingId] AND ISNULL(lc5.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc6 (NOLOCK) ON lc6.Id = e.[TrainingPlacesId] AND ISNULL(lc6.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc7 (NOLOCK) ON lc7.Id = e.[SpecializedId] AND ISNULL(lc7.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc8 (NOLOCK) ON lc8.Id = j.[ContractTypeId] AND ISNULL(lc8.IsDeleted,0) = 0
		WHERE ISNULL(e.IsDeleted,0) = 0
		--Filter
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_SexName_Val IS NULL OR (@Filter_SexName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_SexName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_SexName_Val)))
			AND (@Filter_DateOfBirth_Val IS NULL OR (@Filter_DateOfBirth_Ope = @Contains AND CONVERT(VARCHAR(100),e.DateOfBirth,103) = @Filter_DateOfBirth_Val))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_WorkUnitName_Val)))
			AND (@Filter_WorkStatusName_Val IS NULL OR (@Filter_WorkStatusName_Ope = @Contains AND (lc4.[Name] LIKE @Filter_WorkStatusName_Val OR dbo.ufn_removeMark(lc4.[Name]) LIKE @Filter_WorkStatusName_Val)))
			AND (@Filter_DegreeTrainingName_Val IS NULL OR (@Filter_DegreeTrainingName_Ope = @Contains AND (lc5.[Name] LIKE @Filter_DegreeTrainingName_Val OR dbo.ufn_removeMark(lc5.[Name]) LIKE @Filter_DegreeTrainingName_Val)))
			AND (@Filter_TrainingPlacesName_Val IS NULL OR (@Filter_TrainingPlacesName_Ope = @Contains AND (lc6.[Name] LIKE @Filter_TrainingPlacesName_Val OR dbo.ufn_removeMark(lc6.[Name]) LIKE @Filter_TrainingPlacesName_Val)))
			AND (@Filter_SpecializedName_Val IS NULL OR (@Filter_SpecializedName_Ope = @Contains AND (lc7.[Name] LIKE @Filter_SpecializedName_Val OR dbo.ufn_removeMark(lc7.[Name]) LIKE @Filter_SpecializedName_Val)))
			AND (@Filter_ProbationDay_Val IS NULL OR (@Filter_ProbationDay_Ope = @Contains AND CONVERT(VARCHAR(100),j.ProbationDay,103) = @Filter_ProbationDay_Val))
			AND (@Filter_OfficialDate_Val IS NULL OR (@Filter_OfficialDate_Ope = @Contains AND CONVERT(VARCHAR(100),j.OfficialDate,103) = @Filter_OfficialDate_Val))
			AND (@Filter_ContractTypeName_Val IS NULL OR (@Filter_ContractTypeName_Ope = @Contains AND (lc8.[Name] LIKE @Filter_ContractTypeName_Val OR dbo.ufn_removeMark(lc8.[Name]) LIKE @Filter_ContractTypeName_Val)))
	);

	WITH TempResult AS (
		SELECT
		 e.Id,
		 e.[EmployeeCode],
		 e.[EmployeeName],
		 lc1.[Name] AS SexName,
		 e.[DateOfBirth],
		 lc2.[Name] AS JobPositionName,
		 lc3.[Name] AS WorkUnitName,
		 lc4.[Name] AS WorkStatusName,
		 lc5.[Name] AS DegreeTrainingName,
		 lc6.[Name] AS TrainingPlacesName,
		 lc7.[Name] AS SpecializedName,
		 j.[ProbationDay],
		 j.[OfficialDate],
		 lc8.[Name] AS ContractTypeName

	FROM EmployeeInformation AS e (NOLOCK)
		LEFT JOIN JobInformation AS j (NOLOCK) ON j.EmployeeId = e.Id AND ISNULL(j.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.[SexId] AND ISNULL(lc1.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = e.[JobPositionId] AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = e.[WorkUnitId] AND ISNULL(lc3.IsDeleted,0) = 0
		LEFT JOIN ListStatus AS lc4 (NOLOCK) ON lc4.Id = j.[WorkStatusId] AND ISNULL(lc4.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc5 (NOLOCK) ON lc5.Id = e.[DegreeTrainingId] AND ISNULL(lc5.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc6 (NOLOCK) ON lc6.Id = e.[TrainingPlacesId] AND ISNULL(lc6.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc7 (NOLOCK) ON lc7.Id = e.[SpecializedId] AND ISNULL(lc7.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc8 (NOLOCK) ON lc8.Id = j.[ContractTypeId] AND ISNULL(lc8.IsDeleted,0) = 0
	WHERE ISNULL(e.IsDeleted,0) = 0
	--Filter
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_SexName_Val IS NULL OR (@Filter_SexName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_SexName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_SexName_Val)))
			AND (@Filter_DateOfBirth_Val IS NULL OR (@Filter_DateOfBirth_Ope = @Contains AND CONVERT(VARCHAR(100),e.DateOfBirth,103) = @Filter_DateOfBirth_Val))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_WorkUnitName_Val)))
			AND (@Filter_WorkStatusName_Val IS NULL OR (@Filter_WorkStatusName_Ope = @Contains AND (lc4.[Name] LIKE @Filter_WorkStatusName_Val OR dbo.ufn_removeMark(lc4.[Name]) LIKE @Filter_WorkStatusName_Val)))
			AND (@Filter_DegreeTrainingName_Val IS NULL OR (@Filter_DegreeTrainingName_Ope = @Contains AND (lc5.[Name] LIKE @Filter_DegreeTrainingName_Val OR dbo.ufn_removeMark(lc5.[Name]) LIKE @Filter_DegreeTrainingName_Val)))
			AND (@Filter_TrainingPlacesName_Val IS NULL OR (@Filter_TrainingPlacesName_Ope = @Contains AND (lc6.[Name] LIKE @Filter_TrainingPlacesName_Val OR dbo.ufn_removeMark(lc6.[Name]) LIKE @Filter_TrainingPlacesName_Val)))
			AND (@Filter_SpecializedName_Val IS NULL OR (@Filter_SpecializedName_Ope = @Contains AND (lc7.[Name] LIKE @Filter_SpecializedName_Val OR dbo.ufn_removeMark(lc7.[Name]) LIKE @Filter_SpecializedName_Val)))
			AND (@Filter_ProbationDay_Val IS NULL OR (@Filter_ProbationDay_Ope = @Contains AND CONVERT(VARCHAR(100),j.ProbationDay,103) = @Filter_ProbationDay_Val))
			AND (@Filter_OfficialDate_Val IS NULL OR (@Filter_OfficialDate_Ope = @Contains AND CONVERT(VARCHAR(100),j.OfficialDate,103) = @Filter_OfficialDate_Val))
			AND (@Filter_ContractTypeName_Val IS NULL OR (@Filter_ContractTypeName_Ope = @Contains AND (lc8.[Name] LIKE @Filter_ContractTypeName_Val OR dbo.ufn_removeMark(lc8.[Name]) LIKE @Filter_ContractTypeName_Val)))
	
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeCode_Col  THEN e.EmployeeCode END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_SexName_Col THEN lc1.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_DateOfBirth_Col THEN e.DateOfBirth END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_JobPositionName_Col THEN lc2.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc3.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_WorkStatusName_Col THEN lc4.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_DegreeTrainingName_Col THEN lc5.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_TrainingPlacesName_Col THEN lc6.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_SpecializedName_Col THEN lc7.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_ProbationDay_Col THEN j.ProbationDay END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_OfficialDate_Col THEN j.OfficialDate END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_ContractTypeName_Col THEN lc8.[Name] END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeCode_Col  THEN e.EmployeeCode END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_SexName_Col THEN lc1.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_DateOfBirth_Col THEN e.DateOfBirth END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_JobPositionName_Col THEN lc2.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc3.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_WorkStatusName_Col THEN lc4.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_DegreeTrainingName_Col THEN lc5.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_TrainingPlacesName_Col THEN lc6.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_SpecializedName_Col THEN lc7.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_ProbationDay_Col THEN j.ProbationDay END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_OfficialDate_Col THEN j.OfficialDate END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_ContractTypeName_Col THEN lc8.[Name] END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN e.Id END DESC	

	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY
	)
	
	SELECT * FROM TempResult
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllEmployeesOnBusinessTrip]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllEmployeesOnBusinessTrip]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_BrowsingStatusName_Val NVARCHAR(100),
		@Filter_BrowsingStatusName_Ope NVARCHAR(100),
		@Filter_EmployeeCode_Val NVARCHAR(100),
		@Filter_EmployeeCode_Ope NVARCHAR(100),
		@Filter_EmployeeName_Val NVARCHAR(100),	
		@Filter_EmployeeName_Ope NVARCHAR(100),	
		@Filter_JobPositionName_Val NVARCHAR(100),	
		@Filter_JobPositionName_Ope NVARCHAR(100),	
		@Filter_WorkUnitName_Val NVARCHAR(100),	
		@Filter_WorkUnitName_Ope NVARCHAR(100),	
		@Filter_RecommendedDate_Val NVARCHAR(100),	
		@Filter_RecommendedDate_Ope NVARCHAR(100),	
		@Filter_DayTo_Val NVARCHAR(100),	
		@Filter_DayTo_Ope NVARCHAR(100),	
		@Filter_ReturnDate_Val NVARCHAR(100),	
		@Filter_ReturnDate_Ope NVARCHAR(100),	
		@Filter_WorkingPlace_Val NVARCHAR(100),	
		@Filter_WorkingPlace_Ope NVARCHAR(100),	
		@Filter_AmountProposedForAdvance_Val NVARCHAR(100),	
		@Filter_AmountProposedForAdvance_Ope NVARCHAR(100),	
		@Filter_Deadline_Val NVARCHAR(100),	
		@Filter_Deadline_Ope NVARCHAR(100),	
		@Filter_EmployeeApprovedName_Val NVARCHAR(100),	
		@Filter_EmployeeApprovedName_Ope NVARCHAR(100),	
		
		@Member_BrowsingStatusName_Col NVARCHAR(100) = N'BrowsingStatusName',
		@Member_EmployeeCode_Col NVARCHAR(100) = N'EmployeeCode',
		@Member_EmployeeName_Col NVARCHAR(100) = N'EmployeeName',
		@Member_JobPositionName_Col NVARCHAR(100) = N'JobPositionName',
		@Member_WorkUnitName_Col NVARCHAR(100) = N'WorkUnitName',
		@Member_RecommendedDate_Col NVARCHAR(100) = N'RecommendedDate',
		@Member_DayTo_Col NVARCHAR(100) = N'DayTo',
		@Member_ReturnDate_Col NVARCHAR(100) = N'ReturnDate',
		@Member_WorkingPlace_Col NVARCHAR(100) = N'WorkingPlace',
		@Member_AmountProposedForAdvance_Col NVARCHAR(100) = N'AmountProposedForAdvance',
		@Member_Deadline_Col NVARCHAR(100) = N'Deadline',
		@Member_EmployeeApprovedName_Col NVARCHAR(100) = N'EmployeeApprovedName'

	SELECT 
		@Filter_BrowsingStatusName_Val = TRIM([Value]),
		@Filter_BrowsingStatusName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_BrowsingStatusName_Col
	IF @Filter_BrowsingStatusName_Ope = @Contains SET @Filter_BrowsingStatusName_Val = N'%' + @Filter_BrowsingStatusName_Val + '%'

	SELECT 
		@Filter_EmployeeCode_Val = TRIM([Value]),
		@Filter_EmployeeCode_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeCode_Col
	IF @Filter_EmployeeCode_Ope = @Contains SET @Filter_EmployeeCode_Val = N'%' + @Filter_EmployeeCode_Val + '%'

	SELECT 
		@Filter_EmployeeName_Val = TRIM([Value]),
		@Filter_EmployeeName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeName_Col
	IF @Filter_EmployeeName_Ope = @Contains SET @Filter_EmployeeName_Val = N'%' + @Filter_EmployeeName_Val + '%'

	SELECT 
		@Filter_JobPositionName_Val = TRIM([Value]),
		@Filter_JobPositionName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_JobPositionName_Col
	IF @Filter_JobPositionName_Ope = @Contains SET @Filter_JobPositionName_Val = N'%' + @Filter_JobPositionName_Val + '%'

	SELECT 
		@Filter_WorkUnitName_Val = TRIM([Value]),
		@Filter_WorkUnitName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_WorkUnitName_Col
	IF @Filter_WorkUnitName_Ope = @Contains SET @Filter_WorkUnitName_Val = N'%' + @Filter_WorkUnitName_Val + '%'

	SELECT 
		@Filter_RecommendedDate_Val = TRIM([Value]),
		@Filter_RecommendedDate_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_RecommendedDate_Col
	IF @Filter_RecommendedDate_Ope = @Contains SET @Filter_RecommendedDate_Val = @Filter_RecommendedDate_Val

	SELECT 
		@Filter_DayTo_Val = TRIM([Value]),
		@Filter_DayTo_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_DayTo_Col
	IF @Filter_DayTo_Ope = @Contains SET @Filter_DayTo_Val = @Filter_DayTo_Val

	SELECT 
		@Filter_ReturnDate_Val = TRIM([Value]),
		@Filter_ReturnDate_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_ReturnDate_Col
	IF @Filter_ReturnDate_Ope = @Contains SET @Filter_ReturnDate_Val = @Filter_ReturnDate_Val

	SELECT 
		@Filter_WorkingPlace_Val = TRIM([Value]),
		@Filter_WorkingPlace_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_WorkingPlace_Col
	IF @Filter_WorkingPlace_Ope = @Contains SET @Filter_WorkingPlace_Val = N'%' + @Filter_WorkingPlace_Val + '%'

	SELECT 
		@Filter_AmountProposedForAdvance_Val = TRIM([Value]),
		@Filter_AmountProposedForAdvance_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_AmountProposedForAdvance_Col
	IF @Filter_AmountProposedForAdvance_Ope = @Contains SET @Filter_AmountProposedForAdvance_Val = N'%' + REPLACE(CONVERT(NVARCHAR(100),CAST(@Filter_AmountProposedForAdvance_Val AS MONEY),1),'.00','') + '%'

	SELECT 
		@Filter_Deadline_Val = TRIM([Value]),
		@Filter_Deadline_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_Deadline_Col
	IF @Filter_Deadline_Ope = @Contains SET @Filter_Deadline_Val = @Filter_Deadline_Val

	SELECT 
		@Filter_EmployeeApprovedName_Val = TRIM([Value]),
		@Filter_EmployeeApprovedName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeApprovedName_Col
	IF @Filter_EmployeeApprovedName_Ope = @Contains SET @Filter_EmployeeApprovedName_Val = N'%' + @Filter_EmployeeApprovedName_Val + '%'

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM EmployeesOnBusinessTrip AS eobt (NOLOCK)
		LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = eobt.EmployeeId AND ISNULL(e.IsDeleted,0) = 0
		LEFT JOIN ListStatus AS lc1 (NOLOCK) ON lc1.Id = eobt.BrowsingStatusId AND ISNULL(lc1.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = e.JobPositionId AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = e.WorkUnitId AND ISNULL(lc3.IsDeleted,0) = 0
		LEFT JOIN EmployeeInformation AS e1 (NOLOCK) ON e1.Id = eobt.EmployeeApprovedId AND ISNULL(e1.IsDeleted,0) = 0
		
		WHERE ISNULL(eobt.IsDeleted,0) = 0
		--Filter
			AND (@Filter_BrowsingStatusName_Val IS NULL OR (@Filter_BrowsingStatusName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_BrowsingStatusName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_BrowsingStatusName_Val)))
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_WorkUnitName_Val)))
			AND (@Filter_RecommendedDate_Val IS NULL OR (@Filter_RecommendedDate_Ope = @Contains AND CONVERT(VARCHAR(100),eobt.RecommendedDate,103) = @Filter_RecommendedDate_Val))
			AND (@Filter_DayTo_Val IS NULL OR (@Filter_DayTo_Ope = @Contains AND CONVERT(VARCHAR(100),eobt.DayTo,103) = @Filter_DayTo_Val))
			AND (@Filter_ReturnDate_Val IS NULL OR (@Filter_ReturnDate_Ope = @Contains AND CONVERT(VARCHAR(100),eobt.ReturnDate,103) = @Filter_ReturnDate_Val))
			AND (@Filter_WorkingPlace_Val IS NULL OR (@Filter_WorkingPlace_Ope = @Contains AND (eobt.WorkingPlace LIKE @Filter_WorkingPlace_Val OR dbo.ufn_removeMark(eobt.WorkingPlace) LIKE @Filter_WorkingPlace_Val)))
			AND (@Filter_AmountProposedForAdvance_Val IS NULL OR (@Filter_AmountProposedForAdvance_Ope = @Contains AND (eobt.AmountProposedForAdvance LIKE @Filter_AmountProposedForAdvance_Val OR REPLACE(CONVERT(NVARCHAR(100),CAST(eobt.AmountProposedForAdvance AS MONEY),1),'.00','') LIKE @Filter_AmountProposedForAdvance_Val)))
			AND (@Filter_Deadline_Val IS NULL OR (@Filter_Deadline_Ope = @Contains AND CONVERT(VARCHAR(100),eobt.Deadline,103) = @Filter_Deadline_Val))
			AND (@Filter_EmployeeApprovedName_Val IS NULL OR (@Filter_EmployeeApprovedName_Ope = @Contains AND (e1.EmployeeName LIKE @Filter_EmployeeApprovedName_Val OR dbo.ufn_removeMark(e1.EmployeeName) LIKE @Filter_EmployeeApprovedName_Val)))
	);

	WITH TempResult AS (
	SELECT
		 eobt.Id,
		 lc1.[Name] AS BrowsingStatusName,
		 e.EmployeeCode,
		 e.[EmployeeName],
		 lc2.[Name] AS JobPositionName,
		 lc3.[Name] AS WorkUnitName,
		 eobt.RecommendedDate,
		 eobt.DayTo,
		 eobt.ReturnDate,
		 eobt.WorkingPlace,
		 REPLACE(CONVERT(NVARCHAR(100),CAST(eobt.AmountProposedForAdvance AS MONEY),1),'.00','') AS AmountProposedForAdvance,
		 eobt.Deadline,
		 e1.EmployeeName AS EmployeeApprovedName
	FROM EmployeesOnBusinessTrip AS eobt (NOLOCK)
		LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = eobt.EmployeeId AND ISNULL(e.IsDeleted,0) = 0
		LEFT JOIN ListStatus AS lc1 (NOLOCK) ON lc1.Id = eobt.BrowsingStatusId AND ISNULL(lc1.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = e.JobPositionId AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = e.WorkUnitId AND ISNULL(lc3.IsDeleted,0) = 0
		LEFT JOIN EmployeeInformation AS e1 (NOLOCK) ON e1.Id = eobt.EmployeeApprovedId AND ISNULL(e1.IsDeleted,0) = 0
	WHERE ISNULL(eobt.IsDeleted,0) = 0
	--Filter
			AND (@Filter_BrowsingStatusName_Val IS NULL OR (@Filter_BrowsingStatusName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_BrowsingStatusName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_BrowsingStatusName_Val)))
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_WorkUnitName_Val)))
			AND (@Filter_RecommendedDate_Val IS NULL OR (@Filter_RecommendedDate_Ope = @Contains AND CONVERT(VARCHAR(100),eobt.RecommendedDate,103) = @Filter_RecommendedDate_Val))
			AND (@Filter_DayTo_Val IS NULL OR (@Filter_DayTo_Ope = @Contains AND CONVERT(VARCHAR(100),eobt.DayTo,103) = @Filter_DayTo_Val))
			AND (@Filter_ReturnDate_Val IS NULL OR (@Filter_ReturnDate_Ope = @Contains AND CONVERT(VARCHAR(100),eobt.ReturnDate,103) = @Filter_ReturnDate_Val))
			AND (@Filter_WorkingPlace_Val IS NULL OR (@Filter_WorkingPlace_Ope = @Contains AND (eobt.WorkingPlace LIKE @Filter_WorkingPlace_Val OR dbo.ufn_removeMark(eobt.WorkingPlace) LIKE @Filter_WorkingPlace_Val)))
			AND (@Filter_AmountProposedForAdvance_Val IS NULL OR (@Filter_AmountProposedForAdvance_Ope = @Contains AND (eobt.AmountProposedForAdvance LIKE @Filter_AmountProposedForAdvance_Val OR REPLACE(CONVERT(NVARCHAR(100),CAST(eobt.AmountProposedForAdvance AS MONEY),1),'.00','') LIKE @Filter_AmountProposedForAdvance_Val)))
			AND (@Filter_Deadline_Val IS NULL OR (@Filter_Deadline_Ope = @Contains AND CONVERT(VARCHAR(100),eobt.Deadline,103) = @Filter_Deadline_Val))
			AND (@Filter_EmployeeApprovedName_Val IS NULL OR (@Filter_EmployeeApprovedName_Ope = @Contains AND (e1.EmployeeName LIKE @Filter_EmployeeApprovedName_Val OR dbo.ufn_removeMark(e1.EmployeeName) LIKE @Filter_EmployeeApprovedName_Val)))
	
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_BrowsingStatusName_Col  THEN lc1.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeCode_Col THEN e.EmployeeCode END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_JobPositionName_Col THEN lc2.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc3.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_RecommendedDate_Col THEN eobt.RecommendedDate END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_DayTo_Col THEN eobt.DayTo END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_ReturnDate_Col THEN eobt.ReturnDate END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_WorkingPlace_Col THEN eobt.WorkingPlace END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_AmountProposedForAdvance_Col THEN eobt.AmountProposedForAdvance END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_Deadline_Col THEN eobt.Deadline END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeApprovedName_Col THEN e1.EmployeeName END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_BrowsingStatusName_Col  THEN lc1.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeCode_Col THEN e.EmployeeCode END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_JobPositionName_Col THEN lc2.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc3.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_RecommendedDate_Col THEN eobt.RecommendedDate END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_DayTo_Col THEN eobt.DayTo END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_ReturnDate_Col THEN eobt.ReturnDate END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_WorkingPlace_Col THEN eobt.WorkingPlace END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_AmountProposedForAdvance_Col THEN eobt.AmountProposedForAdvance END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_Deadline_Col THEN eobt.Deadline END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeApprovedName_Col THEN e1.EmployeeName END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN eobt.Id END DESC	

	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY
	)
	
	SELECT * FROM TempResult
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllEvaluate]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllEvaluate]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_NameOfAudit_Val NVARCHAR(100),
		@Filter_NameOfAudit_Ope NVARCHAR(100),
		@Filter_EvaluationPeriodName_Val NVARCHAR(100),	
		@Filter_EvaluationPeriodName_Ope NVARCHAR(100),	
		@Filter_Since_Val NVARCHAR(100),	
		@Filter_Since_Ope NVARCHAR(100),	
		@Filter_ToDate_Val NVARCHAR(100),	
		@Filter_ToDate_Ope NVARCHAR(100),	
		@Filter_EvaluationTerm_Val NVARCHAR(100),	
		@Filter_EvaluationTerm_Ope NVARCHAR(100),	
		@Filter_EvaluationStatusName_Val NVARCHAR(100),	
		@Filter_EvaluationStatusName_Ope NVARCHAR(100),	

		@Member_NameOfAudit_Col NVARCHAR(100) = N'NameOfAudit',
		@Member_EvaluationPeriodName_Col NVARCHAR(100) = N'EvaluationPeriodName',
		@Member_Since_Col NVARCHAR(100) = N'Since',
		@Member_ToDate_Col NVARCHAR(100) = N'ToDate',
		@Member_EvaluationTerm_Col NVARCHAR(100) = N'EvaluationTerm',
		@Member_EvaluationStatusName_Col NVARCHAR(100) = N'EvaluationStatusName'

	SELECT 
		@Filter_NameOfAudit_Val = TRIM([Value]),
		@Filter_NameOfAudit_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_NameOfAudit_Col
	IF @Filter_NameOfAudit_Ope = @Contains SET @Filter_NameOfAudit_Val =  N'%' + @Filter_NameOfAudit_Val + '%'

	SELECT 
		@Filter_EvaluationPeriodName_Val = TRIM([Value]),
		@Filter_EvaluationPeriodName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EvaluationPeriodName_Col
	IF @Filter_EvaluationPeriodName_Ope = @Contains SET @Filter_EvaluationPeriodName_Val =  N'%' + @Filter_EvaluationPeriodName_Val + '%'

	SELECT 
		@Filter_Since_Val = TRIM([Value]),
		@Filter_Since_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_Since_Col
	IF @Filter_Since_Ope = @Contains SET @Filter_Since_Val = @Filter_Since_Val

	SELECT 
		@Filter_ToDate_Val = TRIM([Value]),
		@Filter_ToDate_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_ToDate_Col
	IF @Filter_ToDate_Ope = @Contains SET @Filter_ToDate_Val = @Filter_ToDate_Val

	SELECT 
		@Filter_EvaluationTerm_Val = TRIM([Value]),
		@Filter_EvaluationTerm_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EvaluationTerm_Col
	IF @Filter_EvaluationTerm_Ope = @Contains SET @Filter_EvaluationTerm_Val = @Filter_EvaluationTerm_Val

	SELECT 
		@Filter_EvaluationStatusName_Val = TRIM([Value]),
		@Filter_EvaluationStatusName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EvaluationStatusName_Col
	IF @Filter_EvaluationStatusName_Ope = @Contains SET @Filter_EvaluationStatusName_Val =  N'%' + @Filter_EvaluationStatusName_Val + '%'

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM Evaluate AS e (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.EvaluationPeriodId AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.EvaluationStatusId AND ISNULL(lc1.IsDeleted, 0) = 0
		WHERE ISNULL(e.IsDeleted,0) = 0
		--Filter
			AND (@Filter_NameOfAudit_Val IS NULL OR (@Filter_NameOfAudit_Ope = @Contains AND (e.NameOfAudit LIKE @Filter_NameOfAudit_Val OR dbo.ufn_removeMark(e.NameOfAudit) LIKE @Filter_NameOfAudit_Val)))
			AND (@Filter_EvaluationPeriodName_Val IS NULL OR (@Filter_EvaluationPeriodName_Ope = @Contains AND (lc.[Name] LIKE @Filter_EvaluationPeriodName_Val OR dbo.ufn_removeMark(lc.[Name]) LIKE @Filter_EvaluationPeriodName_Val)))
			AND (@Filter_Since_Val IS NULL OR (@Filter_Since_Ope = @Contains AND CONVERT(VARCHAR(100),e.Since,103) = @Filter_Since_Val))
			AND (@Filter_ToDate_Val IS NULL OR (@Filter_ToDate_Ope = @Contains AND CONVERT(VARCHAR(100),e.ToDate,103) = @Filter_ToDate_Val))
			AND (@Filter_EvaluationTerm_Val IS NULL OR (@Filter_EvaluationTerm_Ope = @Contains AND CONVERT(VARCHAR(100),e.EvaluationTerm,103) = @Filter_EvaluationTerm_Val))
			AND (@Filter_EvaluationStatusName_Val IS NULL OR (@Filter_EvaluationStatusName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_EvaluationStatusName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_EvaluationStatusName_Val)))
	);

	WITH TempResult AS (
		SELECT
		 e.Id,
		 e.NameOfAudit,
		 lc.[Name] AS EvaluationPeriodName,
		 e.Since,
		 e.ToDate,
		 e.EvaluationTerm,
		 lc1.[Name] AS EvaluationStatusName
	FROM Evaluate AS e (NOLOCK)
	LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.EvaluationPeriodId AND ISNULL(lc.IsDeleted, 0) = 0
	LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.EvaluationStatusId AND ISNULL(lc1.IsDeleted, 0) = 0
	WHERE ISNULL(e.IsDeleted,0) = 0
	--Filter
		AND (@Filter_NameOfAudit_Val IS NULL OR (@Filter_NameOfAudit_Ope = @Contains AND (e.NameOfAudit LIKE @Filter_NameOfAudit_Val OR dbo.ufn_removeMark(e.NameOfAudit) LIKE @Filter_NameOfAudit_Val)))
		AND (@Filter_EvaluationPeriodName_Val IS NULL OR (@Filter_EvaluationPeriodName_Ope = @Contains AND (lc.[Name] LIKE @Filter_EvaluationPeriodName_Val OR dbo.ufn_removeMark(lc.[Name]) LIKE @Filter_EvaluationPeriodName_Val)))
		AND (@Filter_Since_Val IS NULL OR (@Filter_Since_Ope = @Contains AND CONVERT(VARCHAR(100),e.Since,103) = @Filter_Since_Val))
		AND (@Filter_ToDate_Val IS NULL OR (@Filter_ToDate_Ope = @Contains AND CONVERT(VARCHAR(100),e.ToDate,103) = @Filter_ToDate_Val))
		AND (@Filter_EvaluationTerm_Val IS NULL OR (@Filter_EvaluationTerm_Ope = @Contains AND CONVERT(VARCHAR(100),e.EvaluationTerm,103) = @Filter_EvaluationTerm_Val))
		AND (@Filter_EvaluationStatusName_Val IS NULL OR (@Filter_EvaluationStatusName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_EvaluationStatusName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_EvaluationStatusName_Val)))
	
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_NameOfAudit_Col  THEN e.NameOfAudit END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EvaluationPeriodName_Col THEN lc.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_Since_Col THEN e.Since END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_ToDate_Col THEN e.ToDate END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EvaluationTerm_Col THEN e.EvaluationTerm END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EvaluationStatusName_Col THEN lc1.[Name] END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_NameOfAudit_Col  THEN e.NameOfAudit END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EvaluationPeriodName_Col THEN lc.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_Since_Col THEN e.Since END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_ToDate_Col THEN e.ToDate END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EvaluationTerm_Col THEN e.EvaluationTerm END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EvaluationStatusName_Col THEN lc1.[Name] END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN e.Id END DESC	
	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY
	)
	
	SELECT * FROM TempResult
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllEvaluateDetail]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllEvaluateDetail]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY,
	@EvaluateId INT
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_EvaluateStatus_Val NVARCHAR(100),
		@Filter_EvaluateStatus_Ope NVARCHAR(100),
		@Filter_EmployeeCode_Val NVARCHAR(100),	
		@Filter_EmployeeCode_Ope NVARCHAR(100),	
		@Filter_EmployeeName_Val NVARCHAR(100),	
		@Filter_EmployeeName_Ope NVARCHAR(100),	
		@Filter_WorkUnitName_Val NVARCHAR(100),	
		@Filter_WorkUnitName_Ope NVARCHAR(100),	
		@Filter_JobPositionName_Val NVARCHAR(100),	
		@Filter_JobPositionName_Ope NVARCHAR(100),	
		@Filter_Result_Val NVARCHAR(100),	
		@Filter_Result_Ope NVARCHAR(100),	

		@Member_EvaluateStatus_Col NVARCHAR(100) = N'EvaluateStatus',
		@Member_EmployeeCode_Col NVARCHAR(100) = N'EmployeeCode',
		@Member_EmployeeName_Col NVARCHAR(100) = N'EmployeeName',
		@Member_WorkUnitName_Col NVARCHAR(100) = N'WorkUnitName',
		@Member_JobPositionName_Col NVARCHAR(100) = N'JobPositionName',
		@Member_Result_Col NVARCHAR(100) = N'Result',
		@Member_Rank_Col NVARCHAR(100) = N'Rank'

	SELECT 
		@Filter_EvaluateStatus_Val = TRIM([Value]),
		@Filter_EvaluateStatus_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EvaluateStatus_Col
	IF @Filter_EvaluateStatus_Ope = @Contains SET @Filter_EvaluateStatus_Val = N'%' + @Filter_EvaluateStatus_Val + '%'

	SELECT 
		@Filter_EmployeeCode_Val = TRIM([Value]),
		@Filter_EmployeeCode_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeCode_Col
	IF @Filter_EmployeeCode_Ope = @Contains SET @Filter_EmployeeCode_Val =  N'%' + @Filter_EmployeeCode_Val + '%'

	SELECT 
		@Filter_EmployeeName_Val = TRIM([Value]),
		@Filter_EmployeeName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeName_Col
	IF @Filter_EmployeeName_Ope = @Contains SET @Filter_EmployeeName_Val =  N'%' + @Filter_EmployeeName_Val + '%'

	SELECT 
		@Filter_WorkUnitName_Val = TRIM([Value]),
		@Filter_WorkUnitName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_WorkUnitName_Col
	IF @Filter_WorkUnitName_Ope = @Contains SET @Filter_WorkUnitName_Val =  N'%' + @Filter_WorkUnitName_Val + '%'

	SELECT 
		@Filter_JobPositionName_Val = TRIM([Value]),
		@Filter_JobPositionName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_JobPositionName_Col
	IF @Filter_JobPositionName_Ope = @Contains SET @Filter_JobPositionName_Val =  N'%' + @Filter_JobPositionName_Val + '%'

	SELECT 
		@Filter_Result_Val = TRIM([Value]),
		@Filter_Result_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_Result_Col
	IF @Filter_Result_Ope = @Contains SET @Filter_Result_Val =  N'%' + @Filter_Result_Val + '%'

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM EvaluateDetail AS ed (NOLOCK)
		LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = ed.EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.WorkUnitId AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.JobPositionId AND ISNULL(lc1.IsDeleted, 0) = 0
		WHERE ISNULL(ed.IsDeleted,0) = 0 AND ed.EvaluateId = @EvaluateId
		--Filter
			AND (@Filter_EvaluateStatus_Val IS NULL OR (@Filter_EvaluateStatus_Ope = @Contains AND (ed.EvaluateStatus LIKE @Filter_EvaluateStatus_Val OR dbo.ufn_removeMark(ed.EvaluateStatus) LIKE @Filter_EvaluateStatus_Val)))
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc.[Name]) LIKE @Filter_WorkUnitName_Val)))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_Result_Val IS NULL OR (@Filter_Result_Ope = @Contains AND (ed.Result LIKE @Filter_Result_Val OR dbo.ufn_removeMark(ed.Result) LIKE @Filter_Result_Val)))
	);

	WITH TempResult AS (
		SELECT
		 ed.Id,
		 ed.EvaluateStatus,
		 e.EmployeeCode,
		 e.EmployeeName,
		 lc.[Name] AS WorkUnitName,
		 lc1.[Name] AS JobPositionName,
		 ed.Result,
		 DENSE_RANK() OVER(ORDER BY CONVERT(INT,ed.Result) DESC) AS [Rank]
	FROM EvaluateDetail AS ed (NOLOCK)
		LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = ed.EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.WorkUnitId AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.JobPositionId AND ISNULL(lc1.IsDeleted, 0) = 0
	WHERE ISNULL(ed.IsDeleted,0) = 0 AND ed.EvaluateId = @EvaluateId
	--Filter
		AND (@Filter_EvaluateStatus_Val IS NULL OR (@Filter_EvaluateStatus_Ope = @Contains AND (ed.EvaluateStatus LIKE @Filter_EvaluateStatus_Val OR dbo.ufn_removeMark(ed.EvaluateStatus) LIKE @Filter_EvaluateStatus_Val)))
		AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
		AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
		AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc.[Name]) LIKE @Filter_WorkUnitName_Val)))
		AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_JobPositionName_Val)))
		AND (@Filter_Result_Val IS NULL OR (@Filter_Result_Ope = @Contains AND (ed.Result LIKE @Filter_Result_Val OR dbo.ufn_removeMark(ed.Result) LIKE @Filter_Result_Val)))
	
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EvaluateStatus_Col  THEN ed.EvaluateStatus END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeCode_Col THEN e.EmployeeCode END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_JobPositionName_Col THEN lc1.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_Rank_Col THEN DENSE_RANK() OVER(ORDER BY CONVERT(INT,ed.Result) DESC) END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EvaluateStatus_Col  THEN ed.EvaluateStatus END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeCode_Col THEN e.EmployeeCode END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_JobPositionName_Col THEN lc1.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_Result_Col THEN ed.Result END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_Rank_Col THEN DENSE_RANK() OVER(ORDER BY CONVERT(INT,ed.Result) DESC) END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN ed.Id END DESC	
	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY
	)
	
	SELECT * FROM TempResult
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllListCategory]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllListCategory]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_Code_Val		NVARCHAR(100),
		@Filter_Code_Ope 		NVARCHAR(100),
		@Filter_Name_Val		NVARCHAR(100),
		@Filter_Name_Ope 		NVARCHAR(100),
		@Filter_Address_Val		NVARCHAR(100),
		@Filter_Address_Ope 		NVARCHAR(100),
		@Filter_ListCategoryTypeName_Val	nvarchar(100),	
		@Filter_ListCategoryTypeName_Ope	nvarchar(100),	
		
		@Member_Code_Col NVARCHAR(100) = N'Code',
		@Member_Name_Col NVARCHAR(100) = N'Name',
		@Member_Address_Col NVARCHAR(100) = N'Address',
		@Member_ListCategoryTypeName_Col NVARCHAR(100) = N'ListCategoryTypeName'

	SELECT 
		@Filter_Code_Val = TRIM([Value]),
		@Filter_Code_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_Code_Col
	IF @Filter_Code_Ope = @Contains SET @Filter_Code_Val =  N'%' + @Filter_Code_Val + '%'

	SELECT 
		@Filter_Name_Val = TRIM([Value]),
		@Filter_Name_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_Name_Col
	IF @Filter_Name_Ope = @Contains SET @Filter_Name_Val =  N'%' + @Filter_Name_Val + '%'

	SELECT 
		@Filter_Address_Val = TRIM([Value]),
		@Filter_Address_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_Address_Col
	IF @Filter_Address_Ope = @Contains SET @Filter_Address_Val =  N'%' + @Filter_Address_Val + '%'

	SELECT 
		@Filter_ListCategoryTypeName_Val = TRIM([Value]),
		@Filter_ListCategoryTypeName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_ListCategoryTypeName_Col
	IF @Filter_ListCategoryTypeName_Ope = @Contains SET @Filter_ListCategoryTypeName_Val =  N'%' + @Filter_ListCategoryTypeName_Val + '%'

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM ListCategory AS lc (NOLOCK)
		LEFT JOIN ListCategoryType AS lct (NOLOCK) ON lct.Id = lc.ListCategoryTypeId AND ISNULL(lct.IsDeleted,0) = 0
		WHERE ISNULL(lc.IsDeleted,0) = 0
		--Filter
			AND (@Filter_Code_Val IS NULL OR (@Filter_Code_Ope = @Contains AND lc.[Code] LIKE @Filter_Code_Val))
			AND (@Filter_Name_Val IS NULL OR (@Filter_Name_Ope = @Contains AND (lc.[Name] LIKE @Filter_Name_Val OR dbo.ufn_removeMark(lc.[Name]) LIKE @Filter_Name_Val)))
			AND (@Filter_Address_Val IS NULL OR (@Filter_Address_Ope = @Contains AND (lc.[Address] LIKE @Filter_Address_Val OR dbo.ufn_removeMark(lc.[Address]) LIKE @Filter_Address_Val)))
			AND (@Filter_ListCategoryTypeName_Val IS NULL OR (@Filter_ListCategoryTypeName_Ope = @Contains AND (lct.[Name] LIKE @Filter_ListCategoryTypeName_Val OR dbo.ufn_removeMark(lct.[Name]) LIKE @Filter_ListCategoryTypeName_Val)))
	);

	WITH TempResult AS (
		SELECT
		 lc.Id,
		 lc.[Code],
		 lc.[Name],
		 lc.[Address],
		 lc.[ListCategoryTypeId],
		 lct.[Name] AS ListCategoryTypeName,
		 lc.[CreatedDate],	
		 lc.[CreatedBy],
		 lc.[ModifiedDate],
		 lc.[ModifiedBy]
	FROM ListCategory AS lc (NOLOCK)
		LEFT JOIN ListCategoryType AS lct (NOLOCK) ON lct.Id = lc.ListCategoryTypeId AND ISNULL(lct.IsDeleted,0) = 0
	WHERE ISNULL(lc.IsDeleted,0) = 0
	--Filter
			AND (@Filter_Code_Val IS NULL OR (@Filter_Code_Ope = @Contains AND lc.[Code] LIKE @Filter_Code_Val))
			AND (@Filter_Name_Val IS NULL OR (@Filter_Name_Ope = @Contains AND (lc.[Name] LIKE @Filter_Name_Val OR dbo.ufn_removeMark(lc.[Name]) LIKE @Filter_Name_Val)))
			AND (@Filter_Address_Val IS NULL OR (@Filter_Address_Ope = @Contains AND (lc.[Address] LIKE @Filter_Address_Val OR dbo.ufn_removeMark(lc.[Address]) LIKE @Filter_Address_Val)))
			AND (@Filter_ListCategoryTypeName_Val IS NULL OR (@Filter_ListCategoryTypeName_Ope = @Contains AND (lct.[Name] LIKE @Filter_ListCategoryTypeName_Val OR dbo.ufn_removeMark(lct.[Name]) LIKE @Filter_ListCategoryTypeName_Val)))
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_Code_Col THEN lc.[Code] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_Name_Col THEN lc.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_Address_Col THEN lc.[Address] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_ListCategoryTypeName_Col THEN lct.[Name] END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_Code_Col THEN lc.[Code] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_Name_Col THEN lc.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_Address_Col THEN lc.[Address] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_ListCategoryTypeName_Col THEN lct.[Name] END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN lc.Id END DESC
		OFFSET @Offset ROWS
		FETCH NEXT @PageSize ROWS ONLY
	)

	SELECT * FROM TempResult
	
END

GO
/****** Object:  StoredProcedure [dbo].[spGetAllNationality]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllNationality]
AS
BEGIN  
	SELECT
        Id,
        [Name]
    FROM Nationality
	ORDER BY [Name] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllowanceInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllowanceInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
		    [EmployeeId],
		    [AllowanceTypeId],
		    [Price],
		    [StartDate],
		    [ToDate]
		FROM AllowanceInformation (NOLOCK)
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			lc.[Name] AS AllowanceTypeName,
			CONCAT(CONVERT(varchar(20),f.StartDate, 103), ' - ', CASE WHEN ISNULL(f.ToDate, '') <> '' THEN CONVERT(varchar(20),f.ToDate, 103) ELSE N'Nay' END) AS DateString,
			REPLACE(CONVERT(NVARCHAR(100),CAST(f.Price AS MONEY),1),'.00','') AS Price
		FROM AllowanceInformation AS f (NOLOCK)
		LEFT JOIN ListCategory AS lc ON lc.Id = f.AllowanceTypeId AND ISNULL(lc.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetAllProblem'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('CREATE PROCEDURE [dbo].spGetAllProblem AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllProblem]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spGetAllProblem]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_CompensationStatusName_Val NVARCHAR(100),
		@Filter_CompensationStatusName_Ope NVARCHAR(100),
		@Filter_ProblemName_Val NVARCHAR(100),	
		@Filter_ProblemName_Ope NVARCHAR(100),	
		@Filter_HappenDay_Val NVARCHAR(100),	
		@Filter_HappenDay_Ope NVARCHAR(100),	
		@Filter_TypeOfIncidentName_Val NVARCHAR(100),	
		@Filter_TypeOfIncidentName_Ope NVARCHAR(100),	
		@Filter_TotalValueOfDamage_Val NVARCHAR(100),	
		@Filter_TotalValueOfDamage_Ope NVARCHAR(100),	
		@Filter_TotalCompensationValue_Val NVARCHAR(100),	
		@Filter_TotalCompensationValue_Ope NVARCHAR(100),	

		@Member_CompensationStatusName_Col NVARCHAR(100) = N'CompensationStatusName',
		@Member_ProblemName_Col NVARCHAR(100) = N'ProblemName',
		@Member_HappenDay_Col NVARCHAR(100) = N'HappenDay',
		@Member_TypeOfIncidentName_Col NVARCHAR(100) = N'TypeOfIncidentName',
		@Member_TotalValueOfDamage_Col NVARCHAR(100) = N'TotalValueOfDamage',
		@Member_TotalCompensationValue_Col NVARCHAR(100) = N'TotalCompensationValue'

	SELECT 
		@Filter_CompensationStatusName_Val = TRIM([Value]),
		@Filter_CompensationStatusName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_CompensationStatusName_Col
	IF @Filter_CompensationStatusName_Ope = @Contains SET @Filter_CompensationStatusName_Val = N'%' + @Filter_CompensationStatusName_Val + '%'

	SELECT 
		@Filter_ProblemName_Val = TRIM([Value]),
		@Filter_ProblemName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_ProblemName_Col
	IF @Filter_ProblemName_Ope = @Contains SET @Filter_ProblemName_Val = N'%' + @Filter_ProblemName_Val + '%'

	SELECT 
		@Filter_HappenDay_Val = TRIM([Value]),
		@Filter_HappenDay_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_HappenDay_Col
	IF @Filter_HappenDay_Ope = @Contains SET @Filter_HappenDay_Val = @Filter_HappenDay_Val

	SELECT 
		@Filter_TypeOfIncidentName_Val = TRIM([Value]),
		@Filter_TypeOfIncidentName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_TypeOfIncidentName_Col
	IF @Filter_TypeOfIncidentName_Ope = @Contains SET @Filter_TypeOfIncidentName_Val = @Filter_TypeOfIncidentName_Val
	
	SELECT 
		@Filter_TotalValueOfDamage_Val = TRIM([Value]),
		@Filter_TotalValueOfDamage_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_TotalValueOfDamage_Col
	IF @Filter_TotalValueOfDamage_Ope = @Contains SET @Filter_TotalValueOfDamage_Val = N'%' + REPLACE(CONVERT(NVARCHAR(100),CAST(@Filter_TotalValueOfDamage_Val AS MONEY),1),'.00','') + '%'

	SELECT 
		@Filter_TotalCompensationValue_Val = TRIM([Value]),
		@Filter_TotalCompensationValue_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_TotalCompensationValue_Col
	IF @Filter_TotalCompensationValue_Ope = @Contains SET @Filter_TotalCompensationValue_Val = N'%' + REPLACE(CONVERT(NVARCHAR(100),CAST(@Filter_TotalCompensationValue_Val AS MONEY),1),'.00','') + '%'

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM ProblemInformation AS p (NOLOCK)
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = p. CompensationStatusId AND ISNULL(lc1.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = p.TypeOfIncidentId AND ISNULL(lc2.IsDeleted,0) = 0
		WHERE ISNULL(p.IsDeleted,0) = 0
		--Filter
			AND (@Filter_CompensationStatusName_Val IS NULL OR (@Filter_CompensationStatusName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_CompensationStatusName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_CompensationStatusName_Val)))
			AND (@Filter_ProblemName_Val IS NULL OR (@Filter_ProblemName_Ope = @Contains AND (p.ProblemName LIKE @Filter_ProblemName_Val OR dbo.ufn_removeMark(p.ProblemName) LIKE @Filter_ProblemName_Val)))
			AND (@Filter_HappenDay_Val IS NULL OR (@Filter_HappenDay_Ope = @Contains AND CONVERT(VARCHAR(100),p.HappenDay,103) = @Filter_HappenDay_Val))
			AND (@Filter_TypeOfIncidentName_Val IS NULL OR (@Filter_TypeOfIncidentName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_TypeOfIncidentName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_TypeOfIncidentName_Val)))
			AND (@Filter_TotalValueOfDamage_Val IS NULL OR (@Filter_TotalValueOfDamage_Ope = @Contains AND (p.TotalValueOfDamage LIKE @Filter_TotalValueOfDamage_Val OR REPLACE(CONVERT(NVARCHAR(100),CAST(p.TotalValueOfDamage AS MONEY),1),'.00','') LIKE @Filter_TotalValueOfDamage_Val)))
			AND (@Filter_TotalCompensationValue_Val IS NULL OR (@Filter_TotalCompensationValue_Ope = @Contains AND (p.TotalCompensationValue LIKE @Filter_TotalCompensationValue_Val OR REPLACE(CONVERT(NVARCHAR(100),CAST(p.TotalCompensationValue AS MONEY),1),'.00','') LIKE @Filter_TotalCompensationValue_Val)))
	);

	WITH TempResult AS (
		SELECT
		 p.Id,
		 lc1.[Name] AS CompensationStatusName,
		 p.ProblemName,
		 p.HappenDay,
		 lc2.[Name] AS TypeOfIncidentName,
		 REPLACE(CONVERT(NVARCHAR(100),CAST(p.TotalValueOfDamage AS MONEY),1),'.00','') AS TotalValueOfDamage,
		 REPLACE(CONVERT(NVARCHAR(100),CAST(p.TotalValueOfDamage AS MONEY),1),'.00','') AS TotalCompensationValue
	FROM ProblemInformation AS p (NOLOCK)
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = p. CompensationStatusId AND ISNULL(lc1.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = p.TypeOfIncidentId AND ISNULL(lc2.IsDeleted,0) = 0
	WHERE ISNULL(p.IsDeleted,0) = 0
	--Filter
		AND (@Filter_CompensationStatusName_Val IS NULL OR (@Filter_CompensationStatusName_Ope = @Contains AND (lc1.[Name] LIKE @Filter_CompensationStatusName_Val OR dbo.ufn_removeMark(lc1.[Name]) LIKE @Filter_CompensationStatusName_Val)))
		AND (@Filter_ProblemName_Val IS NULL OR (@Filter_ProblemName_Ope = @Contains AND (p.ProblemName LIKE @Filter_ProblemName_Val OR dbo.ufn_removeMark(p.ProblemName) LIKE @Filter_ProblemName_Val)))
		AND (@Filter_HappenDay_Val IS NULL OR (@Filter_HappenDay_Ope = @Contains AND CONVERT(VARCHAR(100),p.HappenDay,103) = @Filter_HappenDay_Val))
		AND (@Filter_TypeOfIncidentName_Val IS NULL OR (@Filter_TypeOfIncidentName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_TypeOfIncidentName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_TypeOfIncidentName_Val)))
		AND (@Filter_TotalValueOfDamage_Val IS NULL OR (@Filter_TotalValueOfDamage_Ope = @Contains AND (p.TotalValueOfDamage LIKE @Filter_TotalValueOfDamage_Val OR REPLACE(CONVERT(NVARCHAR(100),CAST(p.TotalValueOfDamage AS MONEY),1),'.00','') LIKE @Filter_TotalValueOfDamage_Val)))
		AND (@Filter_TotalCompensationValue_Val IS NULL OR (@Filter_TotalCompensationValue_Ope = @Contains AND (p.TotalCompensationValue LIKE @Filter_TotalCompensationValue_Val OR REPLACE(CONVERT(NVARCHAR(100),CAST(p.TotalCompensationValue AS MONEY),1),'.00','') LIKE @Filter_TotalCompensationValue_Val)))
	
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_CompensationStatusName_Col THEN lc1.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_ProblemName_Col THEN p.ProblemName END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_HappenDay_Col THEN p.HappenDay END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_TypeOfIncidentName_Col THEN lc2.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_TotalValueOfDamage_Col THEN p.TotalValueOfDamage END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_TotalCompensationValue_Col THEN p.TotalCompensationValue END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_CompensationStatusName_Col THEN lc1.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_ProblemName_Col THEN p.ProblemName END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_HappenDay_Col THEN p.HappenDay END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_TypeOfIncidentName_Col THEN lc2.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_TotalValueOfDamage_Col THEN p.TotalValueOfDamage END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_TotalCompensationValue_Col THEN p.TotalCompensationValue END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN p.Id END DESC	

	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY
	)
	
	SELECT * FROM TempResult
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllProvinceCity]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllProvinceCity]
	@NationalityId INT
AS
BEGIN  
	IF @NationalityId > 0
	BEGIN
		SELECT
			Id,
			[Code],
			[Name]
		FROM ProvinceCity
		WHERE NationalityId = @NationalityId
		ORDER BY [Name] ASC
	END
	ELSE
	BEGIN
		SELECT
			Id,
			[Code],
			[Name]
		FROM ProvinceCity
		ORDER BY [Name] ASC
	END
	
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetAllWards'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('CREATE PROCEDURE [dbo].spGetAllWards AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllWards]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetAllWards]
	@DistrictId INT
AS
BEGIN  
	SELECT
		Id,
		[Name]
	FROM Wards
	WHERE DistrictId = @DistrictId
	ORDER BY [Name] ASC
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetAssetInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('CREATE PROCEDURE [dbo].spGetAssetInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetAssetInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spGetAssetInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
		    [EmployeeId],
		    [AssetCode],
		    [AssetName],
		    [AssetTypeId],
		    [ReceivedDate],
		    [PayDay],
		    [AssetStatusId],
		    [Note]
		FROM AssetInformation (NOLOCK)
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			f.[AssetCode],
			f.[AssetName],
			lc.[Name] AS AssetTypeName,
			CONVERT(varchar(20),f.ReceivedDate, 103) AS ReceivedDateString,
			CONVERT(varchar(20),f.PayDay, 103) AS PayDayString,
			lc1.[Name] AS AssetStatusName
		FROM AssetInformation AS f(NOLOCK)
		LEFT JOIN ListCategory AS lc ON lc.Id = f.[AssetTypeId] AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 ON lc1.Id = f.[AssetStatusId] AND ISNULL(lc1.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetAttachmentInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('CREATE PROCEDURE [dbo].spGetAttachmentInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetAttachmentInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spGetAttachmentInformation]
	@Id INT,
	@EmployeeId INT,
	@EmployeesOnBusinessTripId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
		    [EmployeeId],
			[EmployeesOnBusinessTripId],
		    [FileName],
		    [FileType],
		    [FileSize],
		    [IsDownload]
		FROM AttachmentInformation (NOLOCK)
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			[Id],
		    [FileName],
			[IsDownload]
		FROM AttachmentInformation (NOLOCK)
		WHERE EmployeeId = @EmployeeId AND EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(IsDeleted, 0) = 0
		ORDER BY Id DESC
	END
END
GO
/****** Object:  StoredProcedure [dbo].[spGetBonusById]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetBonusById]
	@Id INT
AS
BEGIN  
	SELECT
       b.[Id],
	   b.[DecisionNumber],
       b.[DecisionDate],
       b.[ThePersonSignedTheDecisionId],
       b.[BonusDay],
       b.[RewardPlanId],
       b.[BonusGrounds],
       b.[RewardReasonId],
       b.[CommendationFormId],
       b.[BonusBudgetSourceId],
       b.[TotalValue],
       b.[StatusBonusId],
	   e.EmployeeCode + ' - ' + e.EmployeeName AS ThePersonSignedTheDecisionName,
	   lc.[Name] AS JobPositionName
    FROM BonusInformation AS b (NOLOCK)
	LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = b.ThePersonSignedTheDecisionId AND ISNULL(e.IsDeleted,0) = 0
	LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.JobPositionId AND ISNULL(lc.IsDeleted,0) = 0
    WHERE b.Id = @Id AND ISNULL(b.IsDeleted,0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetBonusInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('CREATE PROCEDURE [dbo].spGetBonusInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetBonusInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetBonusInformation]
	@EmployeeId INT
AS
BEGIN  
	SELECT
		biswc.[Id],
		bi.DecisionNumber,
		CONVERT(varchar(20),bi.DecisionDate, 103) AS DecisionDateString,
		lc.[Name] AS CommendationFormName,
		REPLACE(CONVERT(NVARCHAR(100),CAST(biswc.BonusValue AS MONEY),1),'.00','') AS BonusValue,
		CASE
			WHEN ISNULL(biswc.[Status], 0) = 0 THEN N'Chưa hoàn thành'
			ELSE N'Đã hoàn thành'
		END AS StatusName
	FROM BonusInformationStaffWereCommended AS biswc (NOLOCK)
	LEFT JOIN BonusInformation AS bi (NOLOCK) ON bi.Id = biswc.BonusInformationId AND ISNULL(biswc.IsDeleted, 0) = 0 
	LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = bi.CommendationFormId AND ISNULL(lc.IsDeleted, 0) = 0 
	WHERE biswc.EmployeeId = @EmployeeId AND ISNULL(biswc.IsDeleted, 0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetBonusInformationStaffWereCommended'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetBonusInformationStaffWereCommended AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetBonusInformationStaffWereCommended]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetBonusInformationStaffWereCommended]
	@BonusInformationId INT
AS
BEGIN  
	SELECT
		f.[Id],
		CONCAT(e.EmployeeCode, ' - ',e.EmployeeName) AS EmployeeName,
		lc.[Name] AS JobPositionName,
		lc1.[Name] AS WorkUnitName,
		f.[BonusValue],
		f.[Status]
	FROM BonusInformationStaffWereCommended AS f (NOLOCK)
	LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = f.EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
	LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.JobPositionId AND ISNULL(lc.IsDeleted, 0) = 0
	LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.WorkUnitId AND ISNULL(lc1.IsDeleted, 0) = 0
	LEFT JOIN ContactInformation AS c (NOLOCK) ON c.EmployeeId = f.EmployeeId AND ISNULL(c.IsDeleted, 0) = 0
	WHERE f.BonusInformationId = @BonusInformationId AND ISNULL(f.IsDeleted, 0) = 0
	ORDER BY f.Id DESC
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetCertificateInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetCertificateInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetCertificateInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetCertificateInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
		    [EmployeeId],
		    [CertificateGroupId],
		    [CertificateName],
		    [NumberOfCertificates],
		    [DegreeTrainingId],
		    [DateRange],
		    [ExpirationDate],
		    [IssuedBy],
		    [ClassificationId],
		    [Note]
		FROM CertificateInformation AS f (NOLOCK)
		WHERE f.Id = @Id AND f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			f.[NumberOfCertificates],
			f.[CertificateName],
			lc.[Name] AS CertificateGroupName,
			CONVERT(varchar(20),f.[DateRange], 103) AS DateRangeString,
			CONVERT(varchar(20),f.[ExpirationDate], 103) AS ExpirationDateString,
			lc1.[Name] AS ClassificationName
		FROM CertificateInformation AS f (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = f.CertificateGroupId AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = f.ClassificationId AND ISNULL(lc1.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetContractInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetContractInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetContractInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetContractInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			f.[Id],
		    f.[EmployeeId],
		    f.[JobPositionId],
		    f.[SomeContracts],
		    f.[WorkUnitId],
		    f.[SignDay],
		    f.[ContractName],
		    f.[ContractTypeId],
		    f.[ContractTermId],
		    f.[TheFormOfWorkId],
		    f.[WageRate],
		    f.[EffectiveDate],
		    f.[ExpirationDate],
		    f.[Abstract],
		    f.[Note]
		FROM ContractInformation AS f (NOLOCK)
		WHERE f.Id = @Id AND f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
	END 
	ELSE
	BEGIN
		SELECT
			f.Id,
			CASE
				WHEN (ISNULL(f.ExpirationDate, '') <> '' AND CONVERT(varchar(20),f.ExpirationDate, 103) > CONVERT(varchar(20),GETDATE(), 103)) OR ISNULL(f.ExpirationDate, '') = '' THEN N'Đang có hiệu lực'
				ELSE N'Hết hiệu lực'
			END AS ExpirationDateString,
			CASE
				WHEN (ISNULL(f.ExpirationDate, '') <> '' AND CONVERT(varchar(20),f.ExpirationDate, 103) > CONVERT(varchar(20),GETDATE(), 103)) OR ISNULL(f.ExpirationDate, '') = '' THEN '1' -- value = 1 when ExpirationDate > CurrentDate
				ELSE '0' -- value = 1 when ExpirationDate < CurrentDate OR ExpirationDate null
			END AS ExpirationDateValue,
			f.[SomeContracts],
			lc2.[Name] AS ContractTypeName,
			lc3.[Name] AS ContractTermName,
			lc1.[Name] AS JobPositionName
		FROM ContractInformation AS f (NOLOCK)
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = f.[JobPositionId] AND ISNULL(lc1.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = f.[ContractTypeId] AND ISNULL(lc2.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = f.ContractTermId AND ISNULL(lc3.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END


IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetDataForDropdown'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetDataForDropdown AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetDataForDropdown]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetDataForDropdown]
	@ListCategoryTypeId INT
AS
BEGIN  
	SELECT
        Id,
		Code,
        [Name],
		[Address]
    FROM ListCategory
	WHERE ISNULL(IsDeleted, 0) = 0 AND ListCategoryTypeId = @ListCategoryTypeId
	ORDER BY [Name] ASC
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetDegreeInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetDegreeInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetDegreeInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetDegreeInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
			[EmployeeId],
			[TrainingPlacesId],
			[FromYear],
			[ToYear],
			[FacultyId],
			[SpecializedId],
			[DegreeTrainingId],
			[FormsOfTrainingId],
			[ClassificationId],
			[Graduated],
			[DateReceived],
			[Note]
		FROM DegreeInformation
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			CONCAT(f.FromYear,N' Đến ', f.ToYear) AS TrainingTime,
			lc.[Name] AS TrainingPlacesName,
			lc1.[Name] AS SpecializedName,
			lc2.[Name] AS DegreeTrainingName,
			lc3.[Name] AS ClassificationName
		FROM DegreeInformation AS f (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = f.TrainingPlacesId AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = f.SpecializedId AND ISNULL(lc1.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = f.DegreeTrainingId AND ISNULL(lc2.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = f.ClassificationId AND ISNULL(lc3.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployee'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployee AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployee]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetEmployee]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY,
	@Id INT,
	@Type NVARCHAR(100)
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_EmployeeCode_Val NVARCHAR(100),
		@Filter_EmployeeCode_Ope NVARCHAR(100),
		@Filter_EmployeeName_Val NVARCHAR(100),	
		@Filter_EmployeeName_Ope NVARCHAR(100),	
		@Filter_JobPositionName_Val NVARCHAR(100),	
		@Filter_JobPositionName_Ope NVARCHAR(100),	
		@Filter_WorkUnitName_Val NVARCHAR(100),	
		@Filter_WorkUnitName_Ope NVARCHAR(100),	
		
		@Member_EmployeeCode_Col NVARCHAR(100) = N'EmployeeCode',
		@Member_EmployeeName_Col NVARCHAR(100) = N'EmployeeName',
		@Member_JobPositionName_Col NVARCHAR(100) = N'JobPositionName',
		@Member_WorkUnitName_Col NVARCHAR(100) = N'WorkUnitName'

	SELECT 
		@Filter_EmployeeCode_Val = TRIM([Value]),
		@Filter_EmployeeCode_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeCode_Col
	IF @Filter_EmployeeCode_Ope = @Contains SET @Filter_EmployeeCode_Val = N'%' + @Filter_EmployeeCode_Val + '%'

	SELECT 
		@Filter_EmployeeName_Val = TRIM([Value]),
		@Filter_EmployeeName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeName_Col
	IF @Filter_EmployeeName_Ope = @Contains SET @Filter_EmployeeName_Val = N'%' + @Filter_EmployeeName_Val + '%'

	SELECT 
		@Filter_JobPositionName_Val = TRIM([Value]),
		@Filter_JobPositionName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_JobPositionName_Col
	IF @Filter_JobPositionName_Ope = @Contains SET @Filter_JobPositionName_Val = N'%' + @Filter_JobPositionName_Val + '%'

	SELECT 
		@Filter_WorkUnitName_Val = TRIM([Value]),
		@Filter_WorkUnitName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_WorkUnitName_Col
	IF @Filter_WorkUnitName_Ope = @Contains SET @Filter_WorkUnitName_Val = N'%' + @Filter_WorkUnitName_Val + '%'

	IF OBJECT_ID('tempdb..#ViewSource') IS NOT NULL
	DROP TABLE #ViewSource

	CREATE TABLE #ViewSource (
		EmployeeId INT
	)

	IF @Type = 'EmployeesOnBusinessTripAssignedStaff'
	BEGIN
		INSERT INTO #ViewSource (
			EmployeeId
		)
		SELECT
			EmployeeId
		FROM dbo.EmployeesOnBusinessTripAssignedStaff (NOLOCK)
		WHERE EmployeesOnBusinessTripId = @Id AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	IF @Type = 'BonusInformationStaffWereCommended'
	BEGIN
		INSERT INTO #ViewSource (
			EmployeeId
		)
		SELECT
			EmployeeId
		FROM dbo.BonusInformationStaffWereCommended (NOLOCK)
		WHERE BonusInformationId = @Id AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	IF @Type = 'ProblemInformationRelatedStaff'
	BEGIN
		INSERT INTO #ViewSource (
			EmployeeId
		)
		SELECT
			EmployeeId
		FROM dbo.ProblemInformationRelatedStaff (NOLOCK)
		WHERE ProblemInformationId = @Id AND ISNULL(IsDeleted, 0) = 0
	END

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM EmployeeInformation AS e (NOLOCK)
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = e.JobPositionId AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = e.WorkUnitId AND ISNULL(lc3.IsDeleted,0) = 0
		INNER JOIN JobInformation AS j ON j.EmployeeId = e.Id AND ISNULL(j.IsDeleted, 0) = 0 AND ISNULL(j.WorkStatusId, 0) NOT IN (2,3,8)
		WHERE e.Id NOT IN (SELECT EmployeeId FROM #ViewSource) AND ISNULL(e.IsDeleted,0) = 0
		--Filter
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_WorkUnitName_Val)))
	);

	WITH TempResult AS (
	SELECT
		 e.Id,
		 e.EmployeeCode,
		 e.[EmployeeName],
		 lc2.[Name] AS JobPositionName,
		 lc3.[Name] AS WorkUnitName,
		 'false' AS IsCheck
	FROM EmployeeInformation AS e (NOLOCK)
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = e.JobPositionId AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = e.WorkUnitId AND ISNULL(lc3.IsDeleted,0) = 0
		INNER JOIN JobInformation AS j ON j.EmployeeId = e.Id AND ISNULL(j.IsDeleted, 0) = 0 AND ISNULL(j.WorkStatusId, 0) NOT IN (2,3,8)
	WHERE e.Id NOT IN (SELECT EmployeeId FROM #ViewSource) AND ISNULL(e.IsDeleted,0) = 0
	--Filter
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_WorkUnitName_Val)))
	
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeCode_Col THEN e.EmployeeCode END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_JobPositionName_Col THEN lc2.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc3.[Name] END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeCode_Col THEN e.EmployeeCode END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_JobPositionName_Col THEN lc2.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc3.[Name] END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN e.Id END DESC	

	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY
	)
	
	SELECT * FROM TempResult

	DROP TABLE #ViewSource
END


IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployeeForAutoCompleBox'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployeeForAutoCompleBox AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployeeForAutoCompleBox]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetEmployeeForAutoCompleBox]
	@Keyword NVARCHAR(MAX)
AS
BEGIN  
	SET @Keyword =  '%' + @Keyword +'%'

	SELECT TOP 20 
		e.Id,
		CONCAT(e.EmployeeCode, ' - ' , e.EmployeeName) AS EmployeeName,
		e.EmployeeCode,
		lc.[Name] AS WorkUnitName,
		lc1.[Name] AS JobPositionName
	FROM EmployeeInformation AS e (NOLOCK)
	LEFT JOIN ListCategory AS lc ON lc.Id = e.WorkUnitId AND ISNULL(lc.IsDeleted, 0) = 0
	LEFT JOIN ListCategory AS lc1 ON lc1.Id = e.JobPositionId AND ISNULL(lc1.IsDeleted, 0) = 0
	INNER JOIN JobInformation AS j ON j.EmployeeId = e.Id AND ISNULL(j.IsDeleted, 0) = 0 AND ISNULL(j.WorkStatusId, 0) NOT IN (2,3,8)
	WHERE e.EmployeeName LIKE @Keyword
		  OR CONCAT(e.EmployeeCode ,' - ' ,e.EmployeeName) LIKE @Keyword
	AND ISNULL(e.IsDeleted, 0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployeeInformationById'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployeeInformationById AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployeeInformationById]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetEmployeeInformationById]
	@Id INT
AS
BEGIN  
	SELECT
        e.Id,
		e.Id AS EmployeeId,
		-- EmployeeInformation
		e.[Image],
        e.[EmployeeCode],
        e.[EmployeeName],
        e.[SexId],
        e.[DateOfBirth],
        e.[PersonalTaxCode],
        e.[WorkUnitId],
        e.[JobPositionId],
        e.[NationId],
        e.[ReligionId],
        e.[NationalityId],
        e.[IdentificationCardNumber],
        e.[DateOfIssueOfIdentificationCard],
        e.[PlaceOfIssueOfIdCard],
        e.[IdentificationCardExpirationDate],
        e.[PassportNumber],
        e.[PassportDate],
        e.[PlaceOfIssueOfPassport],
        e.[PassportExpirationDate],
        e.[EducationalLevelId],
        e.[DegreeTrainingId],
        e.[TrainingPlacesId],
        e.[FacultyId],
        e.[SpecializedId],
        e.[GraduationYear],
        e.[ClassificationId],
        e.[MaritalStatusId],
        e.[FamilyMemberId],
        e.[IngredientsThemselvesId],
		-- ContactInformation
		c.[MobilePhone],
		c.[OfficePhone],
		c.[HomePhone],
		c.[OtherPhone],
		c.[PersonalEmail],
		c.[CompanyEmail],
		c.[OtherEmail],
		c.[Skype],
		c.[Facebook],
		c.[Domicile],
		c.[ProvinceCityId],
		c.[PlaceBirth],
		c.[ResidenceNationalityId],
		c.[ResidenceProvinceCityId],
		c.[ResidenceDistrictId],
		c.[ResidenceWardsId],
		c.[ResidenceHouseStreetVillageNumber],
		c.[ResidenceAddress],
		c.[ResidenceHouseholdRegistrationNumber],
		c.[ResidenceHouseholdCode],
		c.[ResidenceIsHeadHousehold],
		c.[CurrentNationalityId],
		c.[CurrentProvinceCityId],
		c.[CurrentDistrictId],
		c.[CurrentWardsId],
		c.[CurrentHouseStreetVillageNumber],
		c.[CurrentAddress],
		c.[UrgentContactFirstAndLastName],
		c.[UrgentContactRelationshipId],
		c.[UrgentContactMobilePhone],
		c.[UrgentContactHomePhone],
		c.[UrgentContactEmail],
		c.[UrgentContactAddress],
		--JobInformation
        j.[TimekeepingCode],
        j.[WorkStatusId],
        j.[DirectManagementId],
		e1.EmployeeCode + ' - ' + e1.EmployeeName AS DirectManagementName,
        j.[IndirectManagementId],
		e2.EmployeeCode + ' - ' + e2.EmployeeName AS IndirectManagementName,
        j.[WorkLocationId],
        j.[LaborManagementBookNumber],
        j.[ContractTypeId],
        j.[ApprenticeDay],
        j.[ProbationDay],
        j.[OfficialDate],
        j.[NumberOfDaysOff],
        j.[AutomaticallyIncreasesMagicAccordingToSeniority],
        j.[IncreaseLaterSpells],
        j.[WageId],
        j.[BasicSalary],
        j.[InsurancePremiums],
        j.[StandardPublicNumber],
        j.[StandardPublicId],
        j.[BankAccoun],
        j.[BankId],
        j.[JoinTheUnion],
        j.[DateOfInsurance],
        j.[InsurancePremiumRate],
        j.[SomeSocialInsuranceBooks],
        j.[SocialInsuranceNumber],
        j.[ProvinceCodeLevel],
        j.[ProvinceNameLevelId],
        j.[HealthInsuranceCardNumber],
        j.[HealthInsuranceExpirationDate],
        j.[PlaceOfRegistrationForMedicalExaminationAndTreatmentId],
        j.[CodesOfMedicalExaminationAndTreatmentPlaces]
    FROM EmployeeInformation AS e (NOLOCK)
	LEFT JOIN JobInformation AS j (NOLOCK) ON j.EmployeeId = e.Id AND ISNULL(j.IsDeleted,0) = 0
	LEFT JOIN EmployeeInformation AS e1 (NOLOCK) ON e1.Id = j.DirectManagementId AND ISNULL(e1.IsDeleted,0) = 0
	LEFT JOIN EmployeeInformation AS e2 (NOLOCK) ON e2.Id = j.IndirectManagementId AND ISNULL(e2.IsDeleted,0) = 0
	LEFT JOIN ContactInformation AS c (NOLOCK) ON c.EmployeeId = e.Id AND ISNULL(c.IsDeleted,0) = 0
    WHERE e.Id = @Id AND ISNULL(e.IsDeleted,0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployeesOnBusinessTripAdvances'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployeesOnBusinessTripAdvances AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployeesOnBusinessTripAdvances]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetEmployeesOnBusinessTripAdvances]
	@Id INT,
	@EmployeesOnBusinessTripId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
		    [EmployeesOnBusinessTripId],
		    [Expenses],
		    [Unit],
		    [Amount],
		    [UnitPrice],
		    [Money],
		    [Note]
		FROM EmployeesOnBusinessTripAdvances (NOLOCK)
		WHERE Id = @Id AND EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.[Id],
			f.[Expenses],
		    f.[Unit],
		    f.[Amount],
		    REPLACE(CONVERT(NVARCHAR(100),CAST(f.[UnitPrice] AS MONEY),1),'.00','') AS UnitPrice,
		    REPLACE(CONVERT(NVARCHAR(100),CAST(f.[Money] AS MONEY),1),'.00','') AS [Money],
		    f.[Note]
		FROM EmployeesOnBusinessTripAdvances AS f (NOLOCK)
		WHERE f.EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployeesOnBusinessTripAssignedStaff'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployeesOnBusinessTripAssignedStaff AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployeesOnBusinessTripAssignedStaff]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetEmployeesOnBusinessTripAssignedStaff]
	@EmployeesOnBusinessTripId INT
AS
BEGIN  
	SELECT
		f.[Id],
		e.EmployeeCode,
		e.EmployeeName,
		lc.[Name] AS JobPositionName,
		lc1.[Name] AS WorkUnitName,
		c.MobilePhone,
		c.CompanyEmail
	FROM EmployeesOnBusinessTripAssignedStaff AS f (NOLOCK)
	LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = f.EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
	LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.JobPositionId AND ISNULL(lc.IsDeleted, 0) = 0
	LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.WorkUnitId AND ISNULL(lc1.IsDeleted, 0) = 0
	LEFT JOIN ContactInformation AS c (NOLOCK) ON c.EmployeeId = f.EmployeeId AND ISNULL(c.IsDeleted, 0) = 0
	WHERE f.EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(f.IsDeleted, 0) = 0
	ORDER BY f.Id DESC
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployeesOnBusinessTripById'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployeesOnBusinessTripById AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployeesOnBusinessTripById]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetEmployeesOnBusinessTripById]
	@Id INT
AS
BEGIN  
	SELECT
       eobt.[Id],
       eobt.[EmployeeId],
	   e.EmployeeCode + ' - ' + e.EmployeeName AS EmployeeName,
	   e.EmployeeCode,
	   lc.[Name] AS JobPositionName,
	   lc1.[Name] AS WorkUnitName,
       eobt.[EmployeeApprovedId],
	   e1.EmployeeCode + ' - ' + e1.EmployeeName AS EmployeeApprovedName,
       eobt.[DayTo],
       eobt.[ReturnDate],
       eobt.[WorkingPlace],
       eobt.[WorkingPurpose],
       eobt.[RecommendedDate],
       eobt.[Deadline],
       eobt.[AmountProposedForAdvance],
       eobt.[AmountOfAdvance],
       eobt.[ReasonForAdvance],
       eobt.[RequireToBeSupported],
       eobt.[BrowsingStatusId],
	   eobt.ReasonsForNotBrowsing
    FROM EmployeesOnBusinessTrip AS eobt (NOLOCK)
	LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = eobt.EmployeeId AND ISNULL(e.IsDeleted,0) = 0
	LEFT JOIN EmployeeInformation AS e1 (NOLOCK) ON e1.Id = eobt.[EmployeeApprovedId] AND ISNULL(e1.IsDeleted,0) = 0
	LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.JobPositionId AND ISNULL(lc.IsDeleted,0) = 0
	LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.WorkUnitId AND ISNULL(lc1.IsDeleted,0) = 0
    WHERE eobt.Id = @Id AND ISNULL(eobt.IsDeleted,0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployeesOnBusinessTripInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployeesOnBusinessTripInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployeesOnBusinessTripInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetEmployeesOnBusinessTripInformation]
	@EmployeeId INT
AS
BEGIN  
	SELECT
		eobtas.[Id],
		e.EmployeeName AS Proponent,
		CONVERT(varchar(20),ebt.DayTo, 103) AS DayToString,
		CONVERT(varchar(20),ebt.ReturnDate, 103) AS ReturnDateString,
		ebt.WorkingPlace,
		lc.[Name] AS BrowsingStatusName
	FROM EmployeesOnBusinessTripAssignedStaff AS eobtas (NOLOCK)
	LEFT JOIN EmployeesOnBusinessTrip AS ebt (NOLOCK) ON ebt.Id = eobtas.EmployeesOnBusinessTripId AND ISNULL(ebt.IsDeleted, 0) = 0
	LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = ebt.EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
	LEFT JOIN ListStatus AS lc (NOLOCK) ON lc.Id = ebt.BrowsingStatusId AND ISNULL(lc.IsDeleted, 0) = 0
	WHERE eobtas.EmployeeId = @EmployeeId AND ISNULL(eobtas.IsDeleted, 0) = 0
	ORDER BY ebt.Id DESC
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployeesOnBusinessTripPayments'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployeesOnBusinessTripPayments AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployeesOnBusinessTripPayments]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetEmployeesOnBusinessTripPayments]
	@Id INT,
	@EmployeesOnBusinessTripId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
		    [EmployeesOnBusinessTripId],
		    [Expenses],
		    [VoucherNumber],
		    [DayVouchers],
		    [AmountSpent],
		    [Note]
		FROM EmployeesOnBusinessTripPayments (NOLOCK)
		WHERE Id = @Id AND EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.[Id],
		    f.[Expenses],
		    f.[VoucherNumber],
		    CONVERT(varchar(20),f.[DayVouchers], 103) AS DayVouchersString,
		    REPLACE(CONVERT(NVARCHAR(100),CAST(f.[AmountSpent] AS MONEY),1),'.00','') AS AmountSpent,
		    f.[Note]
		FROM EmployeesOnBusinessTripPayments AS f (NOLOCK)
		WHERE f.EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEmployeesOnBusinessTripRevenueEstimates'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEmployeesOnBusinessTripRevenueEstimates AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEmployeesOnBusinessTripRevenueEstimates]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetEmployeesOnBusinessTripRevenueEstimates]
	@Id INT,
	@EmployeesOnBusinessTripId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
		    [EmployeesOnBusinessTripId],
		    [Revenue],
		    [AmountOfMoney],
		    [Note]
		FROM EmployeesOnBusinessTripRevenueEstimates (NOLOCK)
		WHERE Id = @Id AND EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.[Id],
		    f.[Revenue],
		    REPLACE(CONVERT(NVARCHAR(100),CAST(f.[AmountOfMoney] AS MONEY),1),'.00','') AS AmountOfMoney,
		    f.[Note]
		FROM EmployeesOnBusinessTripRevenueEstimates AS f (NOLOCK)
		WHERE f.EmployeesOnBusinessTripId = @EmployeesOnBusinessTripId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEvaluateByEmployee'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEvaluateByEmployee AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEvaluateByEmployee]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetEvaluateByEmployee]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			e.[Id],
			e.NameOfAudit,
			lc.[Name] AS EvaluationPeriodName,
			CONCAT(CONVERT(varchar(20),e.Since, 103), ' - ', CONVERT(varchar(20),e.ToDate, 103)) AS DateString,
			CASE
				WHEN ISNULL(ed.Result, '') <> '' THEN CONCAT(ed.Result, N' điểm')
				ELSE ed.Result
			END AS Result,
			ed.ResultJson
		FROM Evaluate AS e (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.EvaluationPeriodId AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN EvaluateDetail AS ed (NOLOCK) ON ed.EvaluateId = e.Id AND ISNULL(ed.IsDeleted, 0) = 0
		WHERE e.Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			e.[Id],
			e.NameOfAudit,
			lc.[Name] AS EvaluationPeriodName,
			CONCAT(CONVERT(varchar(20),e.Since, 103), ' - ', CONVERT(varchar(20),e.ToDate, 103)) AS DateString,
			CASE
				WHEN ISNULL(ed.Result, '') <> '' THEN CONCAT(ed.Result, N' điểm')
				ELSE ed.Result
			END AS Result
		FROM Evaluate AS e (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.EvaluationPeriodId AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN EvaluateDetail AS ed (NOLOCK) ON ed.EvaluateId = e.Id AND ISNULL(ed.IsDeleted, 0) = 0
		WHERE ed.EmployeeId = @EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetEvaluateById'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetEvaluateById AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetEvaluateById]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetEvaluateById]
	@Id INT
AS
BEGIN  
	SELECT
        e.[Id],
        e.[NameOfAudit],
        e.[EvaluationPeriodId],
        e.[WorkUnitId],
        e.[EvaluationStatusId],
        e.[PersonInChargeId],
		e1.EmployeeCode + ' - ' + e1.EmployeeName AS PersonInChargeName,
        e.[Since],
        e.[ToDate],
        e.[EvaluationTerm],
        e.[BriefDescription]
    FROM Evaluate AS e (NOLOCK)
	LEFT JOIN EmployeeInformation AS e1 (NOLOCK) ON e1.Id = e.PersonInChargeId AND ISNULL(e1.IsDeleted, 0) = 0
    WHERE e.Id = @Id
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetFamilyInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetFamilyInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetFamilyInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetFamilyInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			f.Id,
			f.[RelationshipId],
			f.[FirstAndLastName],
			f.[DateOfBirth],
			f.[SexId],
			f.[NationalityId],
			f.[IdPassportNumber],
			f.[Address],
			f.[MobilePhone],
			f.[HomePhone],
			f.[Email],
			f.[Job],
			f.[PersonalTaxCode],
			f.[Workplace],
			f.[SameHouseholdRegistrationBook],
			f.[BeTheHeadOfTheHousehold],
			f.[IsADependent],
			f.[TimeToCalculateDeduction],
			f.[TimeToEndTheDeduction],
			f.[Note],
			f.[IsDead],
			f.[DeadDate],
			f.[AsAnEmergencyContact],
			f.[Number],
			f.[NumberBook]
		FROM FamilyInformation AS f (NOLOCK)
		WHERE f.Id = @Id AND f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			lc.[Name] AS RelationshipName,
			f.AsAnEmergencyContact,
			f.[FirstAndLastName],
			CONVERT(varchar(20),f.[DateOfBirth], 103) AS DateOfBirthString,
			f.[MobilePhone],
			f.BeTheHeadOfTheHousehold
		FROM FamilyInformation AS f (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = f.RelationshipId AND ISNULL(lc.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetGeneralInformationForResignationProcedures'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetGeneralInformationForResignationProcedures AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetGeneralInformationForResignationProcedures]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetGeneralInformationForResignationProcedures]
	@Id INT
AS
BEGIN  
	SELECT
      rp.Id,
	  rp.EmployeeId,
	  e.EmployeeCode,
	  e.EmployeeName,
	  lc.[Name] AS JobPositionName,
	  lc1.[Name] AS WorkUnitName,
	  CONVERT(varchar(20),j.ProbationDay, 103) AS ProbationDayString,
	  c.MobilePhone,
	  c.CompanyEmail,
	  (SELECT TOP 1 SomeContracts FROM ContractInformation WHERE EmployeeId = rp.EmployeeId AND ISNULL(IsDeleted,0) = 0 ORDER BY Id DESC) AS SomeContracts,
	  (
		SELECT TOP 1 lc.[Name] 
		FROM ContractInformation c
		LEFT JOIN ListCategory lc ON lc.Id = c.ContractTypeId AND ISNULL(lc.IsDeleted,0) = 0
		WHERE c.EmployeeId = rp.EmployeeId AND ISNULL(c.IsDeleted,0) = 0 ORDER BY c.Id DESC
	  ) AS ContractTypeName,
	  (
		SELECT TOP 1 lc.[Name] 
		FROM ContractInformation c
		LEFT JOIN ListCategory lc ON lc.Id = c.ContractTermId AND ISNULL(lc.IsDeleted,0) = 0
		WHERE c.EmployeeId = rp.EmployeeId AND ISNULL(c.IsDeleted,0) = 0 ORDER BY c.Id DESC
	  ) AS ContractTermName,
	  CONVERT(varchar(20),(SELECT TOP 1 EffectiveDate FROM ContractInformation WHERE EmployeeId = rp.EmployeeId AND ISNULL(IsDeleted,0) = 0 ORDER BY Id DESC), 103) AS EffectiveDateString,
	  CONVERT(varchar(20),(SELECT TOP 1 ExpirationDate FROM ContractInformation WHERE EmployeeId = rp.EmployeeId AND ISNULL(IsDeleted,0) = 0 ORDER BY Id DESC), 103) AS ExpirationDateString,
	  CONVERT(varchar(20),rp.DayOff, 103) AS DayOfftring,
	  CASE
		WHEN ISNULL(rp.ReviewerId, 0) <> 0 THEN CONCAT(e1.EmployeeCode, ' - ', e1.EmployeeName)
		ELSE ''
	  END AS ReviewerName,
	  rp.ReasonForRest,
	  rp.Comments,
	  rp.DecisionNumber,
	  rp.Note
    FROM ResignationProcedures AS rp (NOLOCK)
	LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = rp.EmployeeId AND ISNULL(e.IsDeleted,0) = 0
	LEFT JOIN JobInformation AS j (NOLOCK) ON j.EmployeeId = e.Id AND ISNULL(j.IsDeleted,0) = 0
	LEFT JOIN ContactInformation AS c (NOLOCK) ON c.EmployeeId = e.Id AND ISNULL(c.IsDeleted,0) = 0
	LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.JobPositionId AND ISNULL(lc.IsDeleted,0) = 0
	LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.WorkUnitId AND ISNULL(lc1.IsDeleted,0) = 0
	LEFT JOIN EmployeeInformation AS e1 (NOLOCK) ON e1.Id = rp.ReviewerId AND ISNULL(e1.IsDeleted,0) = 0
    WHERE rp.Id = @Id AND ISNULL(rp.IsDeleted,0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetListCategoryById'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetListCategoryById AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetListCategoryById]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetListCategoryById]
	@Id INT
AS
BEGIN  
	SELECT
        Id,
		[Code],
        [Name],
		[Address],
        ListCategoryTypeId
    FROM ListCategory
    WHERE Id = @Id
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetListCategoryType'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetListCategoryType AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetListCategoryType]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetListCategoryType]
	
AS
BEGIN  
	SELECT
        Id,
		Code,
        [Name]
    FROM ListCategoryType
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetPageInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetPageInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetPageInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetPageInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
		    [EmployeeId],
		    [NameOfPapers],
		    [IssuedBy],
		    [DateRange],
		    [ExpirationDate],
		    [Note]
		FROM PageInformation (NOLOCK)
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			[Id],
		    [NameOfPapers],
			[IssuedBy],
			CONVERT(varchar(20),[DateRange], 103) AS DateRangeString,
			CONVERT(varchar(20),[ExpirationDate], 103) AS ExpirationDateString
		FROM PageInformation (NOLOCK)
		WHERE EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
		ORDER BY Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetPoliticsHealthMilitaryInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetPoliticsHealthMilitaryInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetPoliticsHealthMilitaryInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetPoliticsHealthMilitaryInformation]
	@EmployeeId INT
AS
BEGIN  
	SELECT
		[Id],
        [IsAUnionMember],
        [DayToUnion],
        [GroupPositionId],
        [PlaceOfUnionAdmission],
        [AsAPartyMember],
        [DayToParty],
        [PartyPositionId],
        [PlaceOfAdmissionToTheParty],
        [BloodGroupId],
        [Height],
        [Weight],
        [HealthStatus],
        [Diseases],
        [Note],
        [PeopleWithDisabilities],
        [AsASsoldier],
        [DateOfEnlistment],
        [ArmyId],
        [MilitaryUnit],
        [MilitaryRankId],
        [MilitaryPositionId],
        [DateOfDemobilization],
        [TheReason],
        [AsWoundedSoldiersSickSoldiers],
        [DateToJoinRevolution],
        [RankId],
        [RateOfLaborDecline],
        [EnjoyTheMode]
	FROM PoliticsHealthMilitaryInformation (NOLOCK)
	WHERE EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetProblemById'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetProblemById AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetProblemById]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetProblemById]
	@Id INT
AS
BEGIN  
	SELECT
       p.[Id],
	   p.[ProblemName],
       p.[TypeOfIncidentId],
       p.[HappenDay],
       p.[WhereHappened],
       p.[Reason],
       p.[DescriptionOfTheProblem],
       p.[RelatedUnitId],
       p.[TotalValueOfDamage],
       p.[TotalCompensationValue],
       p.[CompensationStatusId]
    FROM ProblemInformation AS p (NOLOCK)
    WHERE p.Id = @Id AND ISNULL(p.IsDeleted,0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetProblemInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetProblemInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetProblemInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetProblemInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
			[EmployeeId],
			[ProblemName],
			[HappenDay],
			[TypeOfIncidentId],
			[TotalValueOfDamage],
			[TotalCompensationValue],
			[CompensationStatusId]
		FROM ProblemInformation
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			f.[ProblemName],
			CONVERT(varchar(20),f.[HappenDay], 103) AS HappenDayString,
			lc.[Name] AS TypeOfIncidentName,
			REPLACE(CONVERT(NVARCHAR(100),CAST(f.[TotalValueOfDamage] AS MONEY),1),'.00','') AS TotalValueOfDamage,
			REPLACE(CONVERT(NVARCHAR(100),CAST(f.[TotalCompensationValue] AS MONEY),1),'.00','') AS TotalCompensationValue,
			lc1.[Name] AS CompensationStatusName
		FROM ProblemInformation AS f (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = f.[TypeOfIncidentId] AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = f.[CompensationStatusId] AND ISNULL(lc1.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetProblemInformationRelatedStaff'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetProblemInformationRelatedStaff AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetProblemInformationRelatedStaff]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetProblemInformationRelatedStaff]
	@Id INT,
	@ProblemInformationId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			f.[Id],
			f.[ProblemInformationId],
		    f.[EmployeeId],
		    f.[DescribeTheRelationship],
		    f.[TotalNumberOfDaysOffDueToOccupationalAccidents],
		    f.[InjuryConditionId],
		    f.[ProcessingStatusId],
		    f.[HavePassedLaborSafetyTraining],
		    f.[DecisionNumber],
		    f.[DecisionDate],
		    f.[KindOfDecisionId],
		    f.[EffectiveDate],
		    f.[FormsProcessingId],
		    f.[TheDecisionId],
		    f.[CitationOfContent],
			e.EmployeeCode,
			e.EmployeeName,
			lc.[Name] AS JobPositionName,
			lc1.[Name] AS WorkUnitName,
			e1.EmployeeCode + ' - ' + e1.EmployeeName AS TheDecisionAutoComplete
		FROM ProblemInformationRelatedStaff AS f (NOLOCK)
			LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = f.EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
			LEFT JOIN EmployeeInformation AS e1 (NOLOCK) ON e1.Id = f.TheDecisionId AND ISNULL(e1.IsDeleted, 0) = 0
			LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.JobPositionId AND ISNULL(lc.IsDeleted, 0) = 0
			LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.WorkUnitId AND ISNULL(lc1.IsDeleted, 0) = 0
		WHERE f.Id = @Id AND f.ProblemInformationId = @ProblemInformationId AND ISNULL(f.IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.[Id],
			e.EmployeeCode,
			e.EmployeeName,
			lc.[Name] AS JobPositionName,
			lc1.[Name] AS WorkUnitName,
			lc2.[Name] AS ProcessingStatusName
		FROM ProblemInformationRelatedStaff AS f (NOLOCK)
			LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = f.EmployeeId AND ISNULL(e.IsDeleted, 0) = 0
			LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = e.JobPositionId AND ISNULL(lc.IsDeleted, 0) = 0
			LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = e.WorkUnitId AND ISNULL(lc1.IsDeleted, 0) = 0
			LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = f.ProcessingStatusId AND ISNULL(lc2.IsDeleted, 0) = 0
		WHERE f.ProblemInformationId = @ProblemInformationId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetProblemInformationTrackEmployeeCompensation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetProblemInformationTrackEmployeeCompensation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetProblemInformationTrackEmployeeCompensation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetProblemInformationTrackEmployeeCompensation]
	@ProblemInformationId INT,
	@EmployeeId INT,
	@Type INT
AS
BEGIN  
	SELECT
		f.[Id],
		f.[ProblemInformationId],
		f.[EmployeeId],
		f.[AmountMoney],
		f.[PayDay],
		CONVERT(varchar(20),f.[PayDay], 103) AS PayDayString,
		f.[SourceCompensation],
		f.[Type],
		0 AS IsEdit,
		f.IsDeleted
	FROM ProblemInformationTrackEmployeeCompensation AS f (NOLOCK)
	WHERE f.EmployeeId = @EmployeeId 
		  AND f.ProblemInformationId = @ProblemInformationId 
		  AND f.[Type] = @Type
		  AND ISNULL(f.IsDeleted, 0) = 0
	ORDER BY f.Id DESC
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetQuitInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetQuitInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetQuitInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetQuitInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
			[EmployeeId],
			[NameOfProcedure],
		    [ProcedureGroupQuitId],
		    [Accomplished],
		    [FinishDay]
		FROM QuitInformation (NOLOCK)
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			f.NameOfProcedure,
			lc.[Name] AS ProcedureGroupQuitName,
			CASE
				WHEN ISNULL(f.Accomplished, 0) = 0 THEN N'Chưa hoàn thành'
				ELSE N'Đã hoàn thành'
			END AS AccomplishedString,
			f.Accomplished,
			CONVERT(varchar(20),f.FinishDay, 103) AS FinishDayString
		FROM QuitInformation AS f(NOLOCK)
		LEFT JOIN ListCategory AS lc ON lc.Id = f.[ProcedureGroupQuitId] AND ISNULL(lc.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetReceiveInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetReceiveInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetReceiveInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetReceiveInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
			[EmployeeId],
			[NameOfProcedure],
		    [ProcedureGroupReceiveId],
		    [Accomplished],
		    [FinishDay]
		FROM ReceiveInformation (NOLOCK)
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			f.NameOfProcedure,
			lc.[Name] AS ProcedureGroupReceiveName,
			CASE
				WHEN ISNULL(f.Accomplished, 0) = 0 THEN N'Chưa hoàn thành'
				ELSE N'Đã hoàn thành'
			END AS AccomplishedString,
			f.Accomplished,
			CONVERT(varchar(20),f.FinishDay, 103) AS FinishDayString
		FROM ReceiveInformation AS f(NOLOCK)
		LEFT JOIN ListCategory AS lc ON lc.Id = f.[ProcedureGroupReceiveId] AND ISNULL(lc.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetResignationProcedures'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetResignationProcedures AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetResignationProcedures]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetResignationProcedures]
	@Offset int,
	@PageSize int,
	@Total int output,
	@SortColumns TypeSortDescriptor READONLY,
	@FilterColumns TypeFilterDescriptor READONLY
AS
BEGIN  
	DECLARE @Contains NVARCHAR(8) = 'Contains'
	DECLARE @ASC  NVARCHAR(3) = 'ASC'
	DECLARE @DESC  NVARCHAR(4) = 'DESC'
	DECLARE @StringEmpty  NVARCHAR(2) = ''

	--Sorting parameters
	DECLARE 
		@Sort_Column  NVARCHAR(100),
		@Sort_Direction  NVARCHAR(4)

	SELECT TOP 1 
		@Sort_Direction  = Direction,
		@Sort_Column  = Member 
	FROM @SortColumns;

	--Filtering parameters
	DECLARE
		@Filter_EmployeeCode_Val NVARCHAR(100),
		@Filter_EmployeeCode_Ope NVARCHAR(100),
		@Filter_EmployeeName_Val NVARCHAR(100),	
		@Filter_EmployeeName_Ope NVARCHAR(100),	
		@Filter_JobPositionName_Val NVARCHAR(100),	
		@Filter_JobPositionName_Ope NVARCHAR(100),	
		@Filter_WorkUnitName_Val NVARCHAR(100),	
		@Filter_WorkUnitName_Ope NVARCHAR(100),	
		@Filter_DayOff_Val NVARCHAR(100),	
		@Filter_DayOff_Ope NVARCHAR(100),
		
		@Member_EmployeeCode_Col NVARCHAR(100) = N'EmployeeCode',
		@Member_EmployeeName_Col NVARCHAR(100) = N'EmployeeName',
		@Member_JobPositionName_Col NVARCHAR(100) = N'JobPositionName',
		@Member_WorkUnitName_Col NVARCHAR(100) = N'WorkUnitName',
		@Member_DayOff_Col NVARCHAR(100) = N'DayOff'

	SELECT 
		@Filter_EmployeeCode_Val = TRIM([Value]),
		@Filter_EmployeeCode_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeCode_Col
	IF @Filter_EmployeeCode_Ope = @Contains SET @Filter_EmployeeCode_Val = N'%' + @Filter_EmployeeCode_Val + '%'

	SELECT 
		@Filter_EmployeeName_Val = TRIM([Value]),
		@Filter_EmployeeName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_EmployeeName_Col
	IF @Filter_EmployeeName_Ope = @Contains SET @Filter_EmployeeName_Val = N'%' + @Filter_EmployeeName_Val + '%'

	SELECT 
		@Filter_JobPositionName_Val = TRIM([Value]),
		@Filter_JobPositionName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_JobPositionName_Col
	IF @Filter_JobPositionName_Ope = @Contains SET @Filter_JobPositionName_Val = N'%' + @Filter_JobPositionName_Val + '%'

	SELECT 
		@Filter_WorkUnitName_Val = TRIM([Value]),
		@Filter_WorkUnitName_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_WorkUnitName_Col
	IF @Filter_WorkUnitName_Ope = @Contains SET @Filter_WorkUnitName_Val = N'%' + @Filter_WorkUnitName_Val + '%'

	SELECT 
		@Filter_DayOff_Val = TRIM([Value]),
		@Filter_DayOff_Ope = Operator
	FROM @FilterColumns
	WHERE Member= @Member_DayOff_Col
	IF @Filter_DayOff_Ope = @Contains SET @Filter_DayOff_Val = @Filter_DayOff_Val

	--Total item
	SET @Total = (
		SELECT COUNT(1) FROM ResignationProcedures AS ep (NOLOCK)
		LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = ep.EmployeeId AND ISNULL(ep.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = e.JobPositionId AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = e.WorkUnitId AND ISNULL(lc3.IsDeleted,0) = 0
		WHERE ISNULL(ep.IsDeleted,0) = 0
		--Filter
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_WorkUnitName_Val)))
			AND (@Filter_DayOff_Val IS NULL OR (@Filter_DayOff_Ope = @Contains AND CONVERT(VARCHAR(100),ep.DayOff,103) = @Filter_DayOff_Val))
	);

	WITH TempResult AS (
	SELECT
		 ep.Id,
		 ep.EmployeeId,
		 e.EmployeeCode,
		 e.[EmployeeName],
		 lc2.[Name] AS JobPositionName,
		 lc3.[Name] AS WorkUnitName,
		 ep.DayOff
	FROM ResignationProcedures AS ep (NOLOCK)
		LEFT JOIN EmployeeInformation AS e (NOLOCK) ON e.Id = ep.EmployeeId AND ISNULL(ep.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc2 (NOLOCK) ON lc2.Id = e.JobPositionId AND ISNULL(lc2.IsDeleted,0) = 0
		LEFT JOIN ListCategory AS lc3 (NOLOCK) ON lc3.Id = e.WorkUnitId AND ISNULL(lc3.IsDeleted,0) = 0
	WHERE ISNULL(ep.IsDeleted,0) = 0
	--Filter
			AND (@Filter_EmployeeCode_Val IS NULL OR (@Filter_EmployeeCode_Ope = @Contains AND (e.EmployeeCode LIKE @Filter_EmployeeCode_Val OR dbo.ufn_removeMark(e.EmployeeCode) LIKE @Filter_EmployeeCode_Val)))
			AND (@Filter_EmployeeName_Val IS NULL OR (@Filter_EmployeeName_Ope = @Contains AND (e.EmployeeName LIKE @Filter_EmployeeName_Val OR dbo.ufn_removeMark(e.EmployeeName) LIKE @Filter_EmployeeName_Val)))
			AND (@Filter_JobPositionName_Val IS NULL OR (@Filter_JobPositionName_Ope = @Contains AND (lc2.[Name] LIKE @Filter_JobPositionName_Val OR dbo.ufn_removeMark(lc2.[Name]) LIKE @Filter_JobPositionName_Val)))
			AND (@Filter_WorkUnitName_Val IS NULL OR (@Filter_WorkUnitName_Ope = @Contains AND (lc3.[Name] LIKE @Filter_WorkUnitName_Val OR dbo.ufn_removeMark(lc3.[Name]) LIKE @Filter_WorkUnitName_Val)))
			AND (@Filter_DayOff_Val IS NULL OR (@Filter_DayOff_Ope = @Contains AND CONVERT(VARCHAR(100),ep.DayOff,103) = @Filter_DayOff_Val))
	
	ORDER BY
	--Sorting
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeCode_Col THEN e.EmployeeCode END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_JobPositionName_Col THEN lc2.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc3.[Name] END ASC,
		CASE WHEN @Sort_Direction = @ASC AND @Sort_Column = @Member_DayOff_Col THEN ep.DayOff END ASC,
		
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeCode_Col THEN e.EmployeeCode END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_EmployeeName_Col THEN e.EmployeeName END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_JobPositionName_Col THEN lc2.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_WorkUnitName_Col THEN lc3.[Name] END DESC,
		CASE WHEN @Sort_Direction = @DESC AND @Sort_Column = @Member_DayOff_Col THEN ep.DayOff END DESC,

		CASE WHEN @Sort_Column = @StringEmpty THEN ep.Id END DESC	

	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY
	)
	
	SELECT * FROM TempResult
END


IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetResignationProceduresEmployeeDebt'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetResignationProceduresEmployeeDebt AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetResignationProceduresEmployeeDebt]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetResignationProceduresEmployeeDebt]
	@Id INT,
	@ResignationProceduresId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id]
		    [ResignationProceduresId],
		    [NameOfTheDebtId],
		    [AmountOfMoney],
		    [FinishDay],
		    [Accomplished]
		FROM ResignationProceduresEmployeeDebt (NOLOCK)
		WHERE Id = @Id AND ResignationProceduresId = @ResignationProceduresId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			CASE
				WHEN ISNULL(f.Accomplished, 0) = 0 THEN N'Chưa hoàn thành'
				ELSE N'Đã hoàn thành'
			END AS AccomplishedString,
			f.Accomplished,
			CONVERT(varchar(20),f.[FinishDay], 103) AS FinishDayString,
			lc.[Name] AS NameOfTheDebtName,
			REPLACE(CONVERT(NVARCHAR(100),CAST(f.AmountOfMoney AS MONEY),1),'.00','') AS AmountOfMoney
		FROM ResignationProceduresEmployeeDebt AS f(NOLOCK)
		LEFT JOIN ListStatus AS lc ON lc.Id = f.[NameOfTheDebtId] AND ISNULL(lc.IsDeleted, 0) = 0
		WHERE f.ResignationProceduresId = @ResignationProceduresId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetSalaryHistoryInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetSalaryHistoryInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetSalaryHistoryInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetSalaryHistoryInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			f.Id,
			f.[EmployeeId],
			f.[JobPositionId],
			f.[DateOfChange],
			f.[BasicSalary],
			f.[InsurancePremiums],
			f.[JoinInsurance],
			f.[Explain]
		FROM SalaryHistoryInformation AS f (NOLOCK)
		WHERE f.Id = @Id AND f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			CONVERT(varchar(20),f.[DateOfChange], 103) AS DateOfChangeString,
			lc.[Name] AS JobPositionName,
			REPLACE(CONVERT(NVARCHAR(100),CAST(f.[BasicSalary] AS MONEY),1),'.00','') AS BasicSalary,
			REPLACE(CONVERT(NVARCHAR(100),CAST(f.[InsurancePremiums] AS MONEY),1),'.00','') AS InsurancePremiums,
			f.Explain
		FROM SalaryHistoryInformation AS f (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = f.JobPositionId AND ISNULL(lc.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetSkillInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetSkillInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetSkillInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetSkillInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
			[EmployeeId],
			[SkillName],
			[SkillGroupId],
			[SkillLevelId],
			[Note]
		FROM SkillInformation (NOLOCK)
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			f.[SkillName],
			lc.[Name] AS SkillGroupName,
			lc1.[Name] AS SkillLevelName,
			f.[Note]
		FROM SkillInformation AS f(NOLOCK)
		LEFT JOIN ListCategory AS lc ON lc.Id = f.[SkillGroupId] AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 ON lc1.Id = f.[SkillLevelId] AND ISNULL(lc1.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetSkinInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetSkinInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetSkinInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetSkinInformation]
	@EmployeeId INT
AS
BEGIN  
	SELECT
		[Id],
        [EmployeeId],
        [ShirtStringId],
        [TrousersStringId],
        [ZuypStringId],
        [ProtectiveGearStringId],
        [ShirtNumberId],
        [TrousersNumberId],
        [ZuypNumberId],
        [ProtectiveGearNumberId],
        [ShoulderWidth],
        [LongSleeve],
        [LongCoat],
        [ChestRing],
        [Waist],
        [Buttocks],
        [LongPants],
        [LongSkirt],
        [LapThigh]
	FROM SkinInformation (NOLOCK)
	WHERE EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetStatusForDropdown'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetStatusForDropdown AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetStatusForDropdown]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetStatusForDropdown]
	@Type NVARCHAR(50)
AS
BEGIN  
	SELECT
        Id,
        [Name]
    FROM ListStatus
	WHERE ISNULL(IsDeleted, 0) = 0 AND [Type] = @Type
	ORDER BY [Name] ASC
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetTrainingProcessInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetTrainingProcessInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetTrainingProcessInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetTrainingProcessInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			[Id],
			[EmployeeId],
			[TrainingProcessCode],
			[TrainingProcessName],
			[StartDay],
			[EndDate],
			[Purpose]
		FROM TrainingProcessInformation
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			f.Id,
			f.[TrainingProcessCode],
			f.[TrainingProcessName],
			CONVERT(varchar(20),f.[StartDay], 103) AS StartDayString,
			CONVERT(varchar(20),f.[EndDate], 103) AS EndDateString,
			f.[Purpose]
		FROM TrainingProcessInformation AS f (NOLOCK)
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetWorkExperienceInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetWorkExperienceInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetWorkExperienceInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGetWorkExperienceInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			Id,
			[EmployeeId],
			[FromMonthAndYear],
			[ByMonthAndYear],
			[Workplace],
			[JobPosition],
			[Wage],
			[JobDescription],
			[Note],
			[FirstAndLastName],
			[Title],
			[Phone],
			[Email],
			[HaveCheckedCompared]
		FROM WorkExperienceInformation (NOLOCK)
		WHERE Id = @Id AND EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
	END
	ELSE
	BEGIN
		SELECT
			Id,
			CONCAT(FORMAT([FromMonthAndYear],'MM/yyyy'),N' Đến ',FORMAT([ByMonthAndYear],'MM/yyyy')) AS TimeWork,
			Workplace,
			JobPosition,
			REPLACE(CONVERT(NVARCHAR(100),CAST(Wage AS MONEY),1),'.00','') AS Wage,
			HaveCheckedCompared
		FROM WorkExperienceInformation (NOLOCK)
		WHERE EmployeeId = @EmployeeId AND ISNULL(IsDeleted, 0) = 0
		ORDER BY Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spGetWorkProgressInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spGetWorkProgressInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spGetWorkProgressInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetWorkProgressInformation]
	@Id INT,
	@EmployeeId INT
AS
BEGIN  
	IF @Id > 0
	BEGIN
		SELECT
			f.Id,
			f.EmployeeId,
			f.[StartDate],
			f.[EndDate],
			f.[JobPositionId],
			f.[WorkUnitId],
			f.[WorkStatusId],
			f.[DirectManagementId],
			e1.EmployeeCode + ' - ' + e1.EmployeeName AS DirectManagementName,
			f.[IndirectManagementId],
			e2.EmployeeCode + ' - ' + e2.EmployeeName AS IndirectManagementName,
			f.[DecisionNumber],
			f.[DecisionDate],
			f.[Note]
		FROM WorkProgressInformation AS f (NOLOCK)
		LEFT JOIN EmployeeInformation AS e1 (NOLOCK) ON e1.Id = f.DirectManagementId AND ISNULL(e1.IsDeleted, 0) = 0
		LEFT JOIN EmployeeInformation AS e2 (NOLOCK) ON e2.Id = f.IndirectManagementId AND ISNULL(e2.IsDeleted, 0) = 0
		WHERE f.Id = @Id AND f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
	END 
	ELSE
	BEGIN
		SELECT
			f.Id,
			CASE
				WHEN ISNULL(f.[EndDate], '') <> '' THEN CONCAT(CONVERT(varchar(20),f.[StartDate], 103),' - ' ,CONVERT(varchar(20),f.[EndDate], 103))
				ELSE CONCAT(CONVERT(varchar(20),f.[StartDate], 103),' - ' ,'Nay')
			END AS StartDateEndDate,
			f.[DecisionNumber],
			lc.[Name] AS WorkUnitName,
			lc1.[Name] AS JobPositionName			
		FROM WorkProgressInformation AS f (NOLOCK)
		LEFT JOIN ListCategory AS lc (NOLOCK) ON lc.Id = f.[WorkUnitId] AND ISNULL(lc.IsDeleted, 0) = 0
		LEFT JOIN ListCategory AS lc1 (NOLOCK) ON lc1.Id = f.[JobPositionId] AND ISNULL(lc1.IsDeleted, 0) = 0
		WHERE f.EmployeeId = @EmployeeId AND ISNULL(f.IsDeleted, 0) = 0
		ORDER BY f.Id DESC
	END
END



IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveAllowanceInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveAllowanceInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveAllowanceInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveAllowanceInformation]
	@TypeAllowanceInformation TypeAllowanceInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeAllowanceInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE AllowanceInformation
			SET 
				[AllowanceTypeId] = f.[AllowanceTypeId],
				[Price] = f.[Price],
				[StartDate] = f.[StartDate],
				[ToDate] = f.[ToDate],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeAllowanceInformation AS f
			WHERE AllowanceInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE AllowanceInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeAllowanceInformation AS f
				WHERE AllowanceInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO AllowanceInformation ([EmployeeId],[AllowanceTypeId],[Price],[StartDate],[ToDate],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[AllowanceTypeId],[Price],[StartDate],[ToDate],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeAllowanceInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveAssetInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveAssetInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveAssetInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveAssetInformation]
	@TypeAssetInformation TypeAssetInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeAssetInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE AssetInformation
			SET 
				[AssetCode] = f.[AssetCode],
				[AssetName] = f.[AssetName],
				[AssetTypeId] = f.[AssetTypeId],
				[ReceivedDate] = f.[ReceivedDate],
				[PayDay] = f.[PayDay],
				[AssetStatusId] = f.[AssetStatusId],
				[Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeAssetInformation AS f
			WHERE AssetInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE AssetInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeAssetInformation AS f
				WHERE AssetInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO AssetInformation ([EmployeeId],[AssetCode],[AssetName],[AssetTypeId],[ReceivedDate],[PayDay],[AssetStatusId],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[AssetCode],[AssetName],[AssetTypeId],[ReceivedDate],[PayDay],[AssetStatusId],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeAssetInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveAttachmentInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveAttachmentInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveAttachmentInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveAttachmentInformation]
	@TypeAttachmentInformation TypeAttachmentInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeAttachmentInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE AttachmentInformation
			SET 
			    [IsDownload] = f.[IsDownload],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeAttachmentInformation AS f
			WHERE AttachmentInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE AttachmentInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeAttachmentInformation AS f
				WHERE AttachmentInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO AttachmentInformation ([EmployeeId],[EmployeesOnBusinessTripId],[FileName],[FileType],[FileSize],[FileContent],[IsDownload],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[EmployeesOnBusinessTripId],[FileName],[FileType],[FileSize],[FileContent],[IsDownload],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeAttachmentInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveBonusInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveBonusInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveBonusInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveBonusInformation]
	@TypeBonusInformation TypeBonusInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeBonusInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE BonusInformation
			SET 
				[DecisionNumber] = f.[DecisionNumber],
			    [DecisionDate] = f.[DecisionDate],
			    [ThePersonSignedTheDecisionId] = f.[ThePersonSignedTheDecisionId],
			    [BonusDay] = f.[BonusDay],
			    [RewardPlanId] = f.[RewardPlanId],
			    [BonusGrounds] = f.[BonusGrounds],
			    [RewardReasonId] = f.[RewardReasonId],
			    [CommendationFormId] = f.[CommendationFormId],
			    [BonusBudgetSourceId] = f.[BonusBudgetSourceId],
			    [TotalValue] = f.[TotalValue],
			    [StatusBonusId] = f.[StatusBonusId],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeBonusInformation AS f
			WHERE BonusInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE BonusInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeBonusInformation AS f
				WHERE BonusInformation.[Id] = @Id_Temp;

				-- Delete BonusInformationStaffWereCommended
				UPDATE BonusInformationStaffWereCommended
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeBonusInformation AS f
				WHERE BonusInformationStaffWereCommended.BonusInformationId = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO BonusInformation ([DecisionNumber],[DecisionDate],[ThePersonSignedTheDecisionId],[BonusDay],[RewardPlanId],[BonusGrounds],[RewardReasonId],[CommendationFormId],[BonusBudgetSourceId],[TotalValue],[StatusBonusId],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [DecisionNumber],[DecisionDate],[ThePersonSignedTheDecisionId],[BonusDay],[RewardPlanId],[BonusGrounds],[RewardReasonId],[CommendationFormId],[BonusBudgetSourceId],[TotalValue],[StatusBonusId],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeBonusInformation

			DECLARE @Id INT = (SELECT SCOPE_IDENTITY())
			-- Update BonusInformation
			UPDATE BonusInformation SET [DecisionNumber] = CONCAT(N'Số',CONVERT(NVARCHAR(6), @Id),'/QĐ-TTX') WHERE Id = @Id AND ISNULL(IsDeleted,0) = 0
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveBonusInformationStaffWereCommended'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveBonusInformationStaffWereCommended AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveBonusInformationStaffWereCommended]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveBonusInformationStaffWereCommended]
	@TypeBonusInformationStaffWereCommended TypeBonusInformationStaffWereCommended READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeBonusInformationStaffWereCommended)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE BonusInformationStaffWereCommended
			SET 
			    [BonusValue] = f.[BonusValue],
			    [Status] = f.[Status],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeBonusInformationStaffWereCommended AS f
			WHERE BonusInformationStaffWereCommended.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE BonusInformationStaffWereCommended
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeBonusInformationStaffWereCommended AS f
				WHERE BonusInformationStaffWereCommended.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO BonusInformationStaffWereCommended ([BonusInformationId],[EmployeeId],[BonusValue],[Status],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [BonusInformationId],[EmployeeId],0,0,GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeBonusInformationStaffWereCommended
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveCertificateInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveCertificateInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveCertificateInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[spSaveCertificateInformation]
	@TypeCertificateInformation TypeCertificateInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeCertificateInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE CertificateInformation
			SET 
				[CertificateGroupId] = f.[CertificateGroupId],
				[CertificateName] = f.[CertificateName],
				[NumberOfCertificates] = f.[NumberOfCertificates],
				[DegreeTrainingId] = f.[DegreeTrainingId],
				[DateRange] = f.[DateRange],
				[ExpirationDate] = f.[ExpirationDate],
				[IssuedBy] = f.[IssuedBy],
				[ClassificationId] = f.[ClassificationId],
				[Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeCertificateInformation AS f
			WHERE CertificateInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE CertificateInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeCertificateInformation AS f
				WHERE CertificateInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO CertificateInformation ([EmployeeId],[CertificateGroupId],[CertificateName],[NumberOfCertificates],[DegreeTrainingId],[DateRange],[ExpirationDate],[IssuedBy],[ClassificationId],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[CertificateGroupId],[CertificateName],[NumberOfCertificates],[DegreeTrainingId],[DateRange],[ExpirationDate],[IssuedBy],[ClassificationId],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeCertificateInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveContractInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveContractInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveContractInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveContractInformation]
	@TypeContractInformation TypeContractInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeContractInformation)
	DECLARE @EmployeeId_Temp INT = (SELECT EmployeeId FROM @TypeContractInformation)

	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE ContractInformation
			SET 
				[JobPositionId] = f.[JobPositionId],
				[WorkUnitId] = f.[WorkUnitId],
				[SignDay] = f.[SignDay],
				[ContractName] = f.[ContractName],
				[ContractTypeId] = f.[ContractTypeId],
				[ContractTermId] = f.[ContractTermId],
				[TheFormOfWorkId] = f.[TheFormOfWorkId],
				[WageRate] = f.[WageRate],
				[EffectiveDate] = f.[EffectiveDate],
				[ExpirationDate] = f.[ExpirationDate],
				[Abstract] = f.[Abstract],
				[Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeContractInformation AS f
			WHERE ContractInformation.[Id] = @Id_Temp;
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			
			DECLARE @Id_Contract_Insert INT = (SELECT TOP 1 Id FROM ContractInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
			IF ISNULL(@Id_Contract_Insert, 0) > 0
			BEGIN
				UPDATE ContractInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], ExpirationDate = GETDATE()
				FROM @TypeContractInformation AS f
				WHERE ContractInformation.[Id] = @Id_Contract_Insert;
			END
			-- Insert ContractInformation
			INSERT INTO ContractInformation ([EmployeeId], [JobPositionId], [SomeContracts], [WorkUnitId], [SignDay], [ContractName], [ContractTypeId], [ContractTermId], [TheFormOfWorkId], [WageRate], [EffectiveDate], [ExpirationDate], [Abstract], [Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId], [JobPositionId], '', [WorkUnitId], [SignDay], [ContractName], [ContractTypeId], [ContractTermId], [TheFormOfWorkId], [WageRate], [EffectiveDate], [ExpirationDate], [Abstract], [Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeContractInformation

			DECLARE @Id INT = (SELECT SCOPE_IDENTITY())
			-- Update ContractInformation
			UPDATE ContractInformation SET [SomeContracts] = 'HĐLĐ-TTX' + CONVERT(NVARCHAR(6), @Id) WHERE Id = @Id AND ISNULL(IsDeleted,0) = 0
		END
	END

	-- Update JobInformation
	UPDATE JobInformation
	SET [ModifiedDate] = GETDATE(), 
		[ModifiedBy] = f.[ModifiedBy], 
		ContractTypeId = f.ContractTypeId
	FROM @TypeContractInformation AS f
	WHERE JobInformation.EmployeeId = @EmployeeId_Temp;
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveDegreeInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveDegreeInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveDegreeInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveDegreeInformation]
	@TypeDegreeInformation TypeDegreeInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeDegreeInformation)
	DECLARE @EmployeeId_Temp INT = (SELECT EmployeeId FROM @TypeDegreeInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE DegreeInformation
			SET 
				[TrainingPlacesId] = f.[TrainingPlacesId],
				[FromYear] = f.[FromYear],
				[ToYear] = f.[ToYear],
				[FacultyId] = f.[FacultyId],
				[SpecializedId] = f.[SpecializedId],
				[DegreeTrainingId] = f.[DegreeTrainingId],
				[FormsOfTrainingId] = f.[FormsOfTrainingId],
				[ClassificationId] = f.[ClassificationId],
				[Graduated] = f.[Graduated],
				[DateReceived] = f.[DateReceived],
				[Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeDegreeInformation AS f
			WHERE DegreeInformation.[Id] = @Id_Temp;

			-- Update EmployeeInformation
			DECLARE @Id_Degree_Update INT = (SELECT TOP 1 Id FROM DegreeInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
			IF @Id_Degree_Update = @Id_Temp
			BEGIN
				UPDATE EmployeeInformation
				SET [TrainingPlacesId] = wpi.[TrainingPlacesId],
					GraduationYear = wpi.[ToYear],
					[FacultyId] = wpi.[FacultyId],
					[SpecializedId] = wpi.[SpecializedId],
					[DegreeTrainingId] = wpi.[DegreeTrainingId],
					[ClassificationId] = wpi.[ClassificationId],
					[ModifiedDate] = GETDATE(), 
					[ModifiedBy] = wpi.[ModifiedBy]
				FROM @TypeDegreeInformation AS wpi
				WHERE EmployeeInformation.Id = @EmployeeId_Temp;
			END
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE DegreeInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeDegreeInformation AS f
				WHERE DegreeInformation.[Id] = @Id_Temp;

				-- Update EmployeeInformation
				DECLARE @Id_Degree_Delete INT = (SELECT TOP 1 Id FROM DegreeInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
				IF @Id_Degree_Delete > 0
				BEGIN
					UPDATE EmployeeInformation
					SET [TrainingPlacesId] = wpi.[TrainingPlacesId],
						GraduationYear = wpi.[ToYear],
						[FacultyId] = wpi.[FacultyId],
						[SpecializedId] = wpi.[SpecializedId],
						[DegreeTrainingId] = wpi.[DegreeTrainingId],
						[ClassificationId] = wpi.[ClassificationId],
						[ModifiedDate] = GETDATE(), 
						[ModifiedBy] = wpi.[ModifiedBy]
					FROM DegreeInformation AS wpi
					WHERE EmployeeInformation.Id = @EmployeeId_Temp AND wpi.Id = @Id_Degree_Delete AND ISNULL(wpi.IsDeleted, 0) = 0;
				END
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO DegreeInformation ([EmployeeId],[TrainingPlacesId],[FromYear],[ToYear],[FacultyId],[SpecializedId],[DegreeTrainingId],[FormsOfTrainingId],[ClassificationId],[Graduated],[DateReceived],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[TrainingPlacesId],[FromYear],[ToYear],[FacultyId],[SpecializedId],[DegreeTrainingId],[FormsOfTrainingId],[ClassificationId],[Graduated],[DateReceived],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeDegreeInformation

			-- Update EmployeeInformation
			UPDATE EmployeeInformation
			SET [TrainingPlacesId] = wpi.[TrainingPlacesId],
				GraduationYear = wpi.[ToYear],
				[FacultyId] = wpi.[FacultyId],
				[SpecializedId] = wpi.[SpecializedId],
				[DegreeTrainingId] = wpi.[DegreeTrainingId],
				[ClassificationId] = wpi.[ClassificationId],
				[ModifiedDate] = GETDATE(), 
				[ModifiedBy] = wpi.[ModifiedBy]
			FROM @TypeDegreeInformation AS wpi
			WHERE EmployeeInformation.Id = @EmployeeId_Temp;
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveEmployeesOnBusinessTrip'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveEmployeesOnBusinessTrip AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveEmployeesOnBusinessTrip]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveEmployeesOnBusinessTrip]
	@TypeEmployeesOnBusinessTrip TypeEmployeesOnBusinessTrip READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeEmployeesOnBusinessTrip)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE EmployeesOnBusinessTrip
			SET 
				[EmployeeId] = f.[EmployeeId],
			    [EmployeeApprovedId] = f.[EmployeeApprovedId],
			    [DayTo] = f.[DayTo],
			    [ReturnDate] = f.[ReturnDate],
			    [WorkingPlace] = f.[WorkingPlace],
			    [WorkingPurpose] = f.[WorkingPurpose],
			    [RecommendedDate] = f.[RecommendedDate],
			    [Deadline] = f.[Deadline],
			    [AmountProposedForAdvance] = f.[AmountProposedForAdvance],
			    [AmountOfAdvance] = f.[AmountOfAdvance],
			    [ReasonForAdvance] = f.[ReasonForAdvance],
			    [RequireToBeSupported] = f.[RequireToBeSupported],
			    [BrowsingStatusId] = f.[BrowsingStatusId],
				[ReasonsForNotBrowsing] = f.[ReasonsForNotBrowsing],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeEmployeesOnBusinessTrip AS f
			WHERE EmployeesOnBusinessTrip.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE EmployeesOnBusinessTrip
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTrip AS f
				WHERE EmployeesOnBusinessTrip.[Id] = @Id_Temp;

				-- Delete EmployeesOnBusinessTripAdvances
				UPDATE EmployeesOnBusinessTripAdvances
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTrip AS f
				WHERE EmployeesOnBusinessTripAdvances.EmployeesOnBusinessTripId = @Id_Temp;

				-- Delete EmployeesOnBusinessTripAssignedStaff
				UPDATE EmployeesOnBusinessTripAssignedStaff
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTrip AS f
				WHERE EmployeesOnBusinessTripAssignedStaff.EmployeesOnBusinessTripId = @Id_Temp;

				-- Delete EmployeesOnBusinessTripPayments
				UPDATE EmployeesOnBusinessTripPayments
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTrip AS f
				WHERE EmployeesOnBusinessTripPayments.EmployeesOnBusinessTripId = @Id_Temp;

				-- Delete EmployeesOnBusinessTripRevenueEstimates
				UPDATE EmployeesOnBusinessTripRevenueEstimates
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTrip AS f
				WHERE EmployeesOnBusinessTripRevenueEstimates.EmployeesOnBusinessTripId = @Id_Temp;

				-- Delete AttachmentInformation
				UPDATE AttachmentInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTrip AS f
				WHERE AttachmentInformation.EmployeesOnBusinessTripId = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO EmployeesOnBusinessTrip ([EmployeeId],[EmployeeApprovedId],[DayTo],[ReturnDate],[WorkingPlace],[WorkingPurpose],[RecommendedDate],[Deadline],[AmountProposedForAdvance],[AmountOfAdvance],[ReasonForAdvance],[RequireToBeSupported],[BrowsingStatusId],[ReasonsForNotBrowsing],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[EmployeeApprovedId],[DayTo],[ReturnDate],[WorkingPlace],[WorkingPurpose],[RecommendedDate],[Deadline],[AmountProposedForAdvance],[AmountOfAdvance],[ReasonForAdvance],[RequireToBeSupported],[BrowsingStatusId],[ReasonsForNotBrowsing],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeEmployeesOnBusinessTrip
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveEmployeesOnBusinessTripAdvances'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveEmployeesOnBusinessTripAdvances AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveEmployeesOnBusinessTripAdvances]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



create PROCEDURE [dbo].[spSaveEmployeesOnBusinessTripAdvances]
	@TypeEmployeesOnBusinessTripAdvances TypeEmployeesOnBusinessTripAdvances READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeEmployeesOnBusinessTripAdvances)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE EmployeesOnBusinessTripAdvances
			SET 
			    [Expenses]	   = f.[Expenses],
			    [Unit] = f.[Unit],
			    [Amount] = f.[Amount],
			    [UnitPrice] = f.[UnitPrice],
			    [Money] = f.[Money],
			    [Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeEmployeesOnBusinessTripAdvances AS f
			WHERE EmployeesOnBusinessTripAdvances.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE EmployeesOnBusinessTripAdvances
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTripAdvances AS f
				WHERE EmployeesOnBusinessTripAdvances.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO EmployeesOnBusinessTripAdvances ([EmployeesOnBusinessTripId],[Expenses],[Unit],[Amount],[UnitPrice],[Money],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeesOnBusinessTripId],[Expenses],[Unit],[Amount],[UnitPrice],[Money],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeEmployeesOnBusinessTripAdvances
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveEmployeesOnBusinessTripAssignedStaff'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveEmployeesOnBusinessTripAssignedStaff AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveEmployeesOnBusinessTripAssignedStaff]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveEmployeesOnBusinessTripAssignedStaff]
	@TypeEmployeesOnBusinessTripAssignedStaff TypeEmployeesOnBusinessTripAssignedStaff READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeEmployeesOnBusinessTripAssignedStaff)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 2 --Action Delete
		BEGIN
			UPDATE EmployeesOnBusinessTripAssignedStaff
			SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
			FROM @TypeEmployeesOnBusinessTripAssignedStaff AS f
			WHERE EmployeesOnBusinessTripAssignedStaff.[Id] = @Id_Temp;
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO EmployeesOnBusinessTripAssignedStaff ([EmployeeId],[EmployeesOnBusinessTripId],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[EmployeesOnBusinessTripId],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeEmployeesOnBusinessTripAssignedStaff
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveEmployeesOnBusinessTripPayments'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveEmployeesOnBusinessTripPayments AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveEmployeesOnBusinessTripPayments]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveEmployeesOnBusinessTripPayments]
	@TypeEmployeesOnBusinessTripPayments TypeEmployeesOnBusinessTripPayments READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeEmployeesOnBusinessTripPayments)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE EmployeesOnBusinessTripPayments
			SET 
			    [Expenses] = f.[Expenses],
			    [VoucherNumber] = f.[VoucherNumber],
			    [DayVouchers] = f.[DayVouchers],
			    [AmountSpent] = f.[AmountSpent],
			    [Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeEmployeesOnBusinessTripPayments AS f
			WHERE EmployeesOnBusinessTripPayments.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE EmployeesOnBusinessTripPayments
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTripPayments AS f
				WHERE EmployeesOnBusinessTripPayments.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO EmployeesOnBusinessTripPayments ([EmployeesOnBusinessTripId],[Expenses],[VoucherNumber],[DayVouchers],[AmountSpent],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeesOnBusinessTripId],[Expenses],[VoucherNumber],[DayVouchers],[AmountSpent],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeEmployeesOnBusinessTripPayments
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveEmployeesOnBusinessTripRevenueEstimates'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveEmployeesOnBusinessTripRevenueEstimates AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveEmployeesOnBusinessTripRevenueEstimates]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveEmployeesOnBusinessTripRevenueEstimates]
	@TypeEmployeesOnBusinessTripRevenueEstimates TypeEmployeesOnBusinessTripRevenueEstimates READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeEmployeesOnBusinessTripRevenueEstimates)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE EmployeesOnBusinessTripRevenueEstimates
			SET 
			    [Revenue] = f.[Revenue],
			    [AmountOfMoney] = f.[AmountOfMoney],
			    [Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeEmployeesOnBusinessTripRevenueEstimates AS f
			WHERE EmployeesOnBusinessTripRevenueEstimates.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE EmployeesOnBusinessTripRevenueEstimates
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeEmployeesOnBusinessTripRevenueEstimates AS f
				WHERE EmployeesOnBusinessTripRevenueEstimates.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO EmployeesOnBusinessTripRevenueEstimates ([EmployeesOnBusinessTripId],[Revenue],[AmountOfMoney],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeesOnBusinessTripId],[Revenue],[AmountOfMoney],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeEmployeesOnBusinessTripRevenueEstimates
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveEvaluate'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveEvaluate AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveEvaluate]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveEvaluate]
	@TypeEvaluate TypeEvaluate READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeEvaluate)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE Evaluate
			SET [NameOfAudit] = tlc.[NameOfAudit],
                [EvaluationPeriodId] = tlc.[EvaluationPeriodId],
                [WorkUnitId] = tlc.[WorkUnitId],
                [EvaluationStatusId] = tlc.[EvaluationStatusId],
                [PersonInChargeId] = tlc.[PersonInChargeId],
                [Since] = tlc.[Since],
                [ToDate] = tlc.[ToDate],
                [EvaluationTerm] = tlc.[EvaluationTerm],
                [BriefDescription] = tlc.[BriefDescription]
			FROM @TypeEvaluate AS tlc
			WHERE Evaluate.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
				BEGIN
					UPDATE Evaluate
					SET [ModifiedDate] = GETDATE(), [ModifiedBy] = tlc.[ModifiedBy], [IsDeleted] = 1
					FROM @TypeEvaluate AS tlc
					WHERE Evaluate.[Id] = @Id_Temp;

					-- Delete EvaluateDetail
					UPDATE EvaluateDetail
					SET [ModifiedDate] = GETDATE(), [ModifiedBy] = tlc.[ModifiedBy], [IsDeleted] = 1
					FROM @TypeEvaluate AS tlc
					WHERE EvaluateDetail.EvaluateId = @Id_Temp;
				END
			END
		END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO Evaluate ([NameOfAudit],[EvaluationPeriodId],[WorkUnitId],[EvaluationStatusId],[PersonInChargeId],[Since],[ToDate],[EvaluationTerm],[BriefDescription], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [IsDeleted] )
			SELECT [NameOfAudit],[EvaluationPeriodId],[WorkUnitId],[EvaluationStatusId],[PersonInChargeId],[Since],[ToDate],[EvaluationTerm],[BriefDescription],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeEvaluate
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveEvaluateDetail'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveEvaluateDetail AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveEvaluateDetail]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveEvaluateDetail]
	@TypeEvaluateDetail TypeEvaluateDetail READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeEvaluateDetail)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE EvaluateDetail
			SET [EvaluateStatus] = N'Đã đánh giá',
				[Result] = tlc.[Result],
				[ResultJson] = tlc.[ResultJson],
				[ModifiedDate] = GETDATE(), 
				[ModifiedBy] = tlc.[ModifiedBy]
			FROM @TypeEvaluateDetail AS tlc
			WHERE EvaluateDetail.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
				BEGIN
					UPDATE EvaluateDetail
					SET [ModifiedDate] = GETDATE(), [ModifiedBy] = tlc.[ModifiedBy], [IsDeleted] = 1
					FROM @TypeEvaluateDetail AS tlc
					WHERE EvaluateDetail.[Id] = @Id_Temp;
				END
			END
		END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO EvaluateDetail ([EmployeeId],[EvaluateId],[EvaluateStatus], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [IsDeleted] )
			SELECT [EmployeeId],[EvaluateId],N'Chưa đánh giá',GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeEvaluateDetail
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveFamilyInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveFamilyInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveFamilyInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveFamilyInformation]
	@TypeFamilyInformation TypeFamilyInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeFamilyInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE FamilyInformation
			SET 
				[RelationshipId] = f.[RelationshipId],
				[FirstAndLastName] = f.[FirstAndLastName],
				[DateOfBirth] = f.[DateOfBirth],
				[SexId] = f.[SexId],
				[NationalityId] = f.[NationalityId],
				[IdPassportNumber] = f.[IdPassportNumber],
				[Address] = f.[Address],
				[MobilePhone] = f.[MobilePhone],
				[HomePhone] = f.[HomePhone],
				[Email] = f.[Email],
				[Job] = f.[Job],
				[PersonalTaxCode] = f.[PersonalTaxCode],
				[Workplace] = f.[Workplace],
				[SameHouseholdRegistrationBook]	 = f.[SameHouseholdRegistrationBook],
				[BeTheHeadOfTheHousehold] = f.[BeTheHeadOfTheHousehold],
				[IsADependent] = f.[IsADependent],
				[TimeToCalculateDeduction] = f.[TimeToCalculateDeduction],
				[TimeToEndTheDeduction] = f.[TimeToEndTheDeduction],
				[Note] = f.[Note],
				[IsDead] = f.[IsDead],
				[DeadDate] = f.[DeadDate],
				[AsAnEmergencyContact] = f.[AsAnEmergencyContact],
				[Number] = f.[Number],
				[NumberBook] = f.[NumberBook],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeFamilyInformation AS f
			WHERE FamilyInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE FamilyInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeFamilyInformation AS f
				WHERE FamilyInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO FamilyInformation ([EmployeeId],[RelationshipId],[FirstAndLastName],[DateOfBirth],[SexId],[NationalityId],[IdPassportNumber],[Address],[MobilePhone],[HomePhone],[Email],[Job],[PersonalTaxCode],[Workplace],[SameHouseholdRegistrationBook],[BeTheHeadOfTheHousehold],[IsADependent],[TimeToCalculateDeduction],[TimeToEndTheDeduction],[Note],[IsDead],[DeadDate],[AsAnEmergencyContact],[Number],[NumberBook],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[RelationshipId],[FirstAndLastName],[DateOfBirth],[SexId],[NationalityId],[IdPassportNumber],[Address],[MobilePhone],[HomePhone],[Email],[Job],[PersonalTaxCode],[Workplace],[SameHouseholdRegistrationBook],[BeTheHeadOfTheHousehold],[IsADependent],[TimeToCalculateDeduction],[TimeToEndTheDeduction],[Note],[IsDead],[DeadDate],[AsAnEmergencyContact],[Number],[NumberBook],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeFamilyInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveListCategory'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveListCategory AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveListCategory]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSaveListCategory]
	@TypeListCategory TypeListCategory READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeListCategory)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE ListCategory
			SET [Name] = tlc.[Name], [Address] = tlc.[Address], [ListCategoryTypeId] = tlc.[ListCategoryTypeId], [ModifiedDate] = GETDATE(), [ModifiedBy] = tlc.[ModifiedBy]
			FROM @TypeListCategory AS tlc
			WHERE ListCategory.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
				BEGIN
					UPDATE ListCategory
					SET [ModifiedDate] = GETDATE(), [ModifiedBy] = tlc.[ModifiedBy], [IsDeleted] = 1
					FROM @TypeListCategory AS tlc
					WHERE ListCategory.[Id] = @Id_Temp;
				END
			END
		END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO ListCategory ([Code], [Name], [Address], [ListCategoryTypeId], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [IsDeleted] )
			SELECT [Code],[Name],[Address],[ListCategoryTypeId],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeListCategory
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSavePageInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSavePageInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSavePageInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[spSavePageInformation]
	@TypePageInformation TypePageInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypePageInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE PageInformation
			SET 
				[NameOfPapers] = f.[NameOfPapers],
				[IssuedBy] = f.[IssuedBy],
				[DateRange] = f.[DateRange],
				[ExpirationDate] = f.[ExpirationDate],
				[Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypePageInformation AS f
			WHERE PageInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE PageInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypePageInformation AS f
				WHERE PageInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO PageInformation ([EmployeeId],[NameOfPapers],[IssuedBy],[DateRange],[ExpirationDate],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[NameOfPapers],[IssuedBy],[DateRange],[ExpirationDate],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypePageInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSavePoliticsHealthMilitaryInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSavePoliticsHealthMilitaryInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSavePoliticsHealthMilitaryInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[spSavePoliticsHealthMilitaryInformation]
	@TypePoliticsHealthMilitaryInformation TypePoliticsHealthMilitaryInformation READONLY
AS
BEGIN
	SET NOCOUNT ON;
		
	UPDATE PoliticsHealthMilitaryInformation
	SET 
		[IsAUnionMember] = p.[IsAUnionMember],
	    [DayToUnion] = p.[DayToUnion],
	    [GroupPositionId] = p.[GroupPositionId],
	    [PlaceOfUnionAdmission] = p.[PlaceOfUnionAdmission],
	    [AsAPartyMember] = p.[AsAPartyMember],
	    [DayToParty] = p.[DayToParty],
	    [PartyPositionId] = p.[PartyPositionId],
	    [PlaceOfAdmissionToTheParty] = p.[PlaceOfAdmissionToTheParty],
	    [BloodGroupId] = p.[BloodGroupId],
	    [Height] = p.[Height],
	    [Weight] = p.[Weight],
	    [HealthStatus] = p.[HealthStatus],
	    [Diseases] = p.[Diseases],
	    [Note] = p.[Note],
	    [PeopleWithDisabilities] = p.[PeopleWithDisabilities],
	    [AsASsoldier] = p.[AsASsoldier],
	    [DateOfEnlistment] = p.[DateOfEnlistment],
	    [ArmyId] = p.[ArmyId],
	    [MilitaryUnit] = p.[MilitaryUnit],
	    [MilitaryRankId] = p.[MilitaryRankId],
	    [MilitaryPositionId] = p.[MilitaryPositionId],
	    [DateOfDemobilization] = p.[DateOfDemobilization],
	    [TheReason] = p.[TheReason],
	    [AsWoundedSoldiersSickSoldiers] = p.[AsWoundedSoldiersSickSoldiers],
	    [DateToJoinRevolution] = p.[DateToJoinRevolution],
	    [RankId] = p.[RankId],
	    [RateOfLaborDecline] = p.[RateOfLaborDecline],
	    [EnjoyTheMode] = p.[EnjoyTheMode],
		[ModifiedDate] = GETDATE(),
		[ModifiedBy] = p.[ModifiedBy]
	FROM @TypePoliticsHealthMilitaryInformation AS p
	WHERE PoliticsHealthMilitaryInformation.EmployeeId = p.EmployeeId;

END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveProblemInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveProblemInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveProblemInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveProblemInformation]
	@TypeProblemInformation TypeProblemInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeProblemInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE ProblemInformation
			SET 
				[ProblemName] = f.[ProblemName],
			    [TypeOfIncidentId] = f.[TypeOfIncidentId],
			    [HappenDay] = f.[HappenDay],
			    [WhereHappened] = f.[WhereHappened],
			    [Reason] = f.[Reason],
			    [DescriptionOfTheProblem] = f.[DescriptionOfTheProblem],
			    [RelatedUnitId] = f.[RelatedUnitId],
			    [TotalValueOfDamage] = f.[TotalValueOfDamage],
			    [TotalCompensationValue] = f.[TotalCompensationValue],
			    [CompensationStatusId] = f.[CompensationStatusId],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeProblemInformation AS f
			WHERE ProblemInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE ProblemInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeProblemInformation AS f
				WHERE ProblemInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO ProblemInformation ([ProblemName],[TypeOfIncidentId],[HappenDay],[WhereHappened],[Reason],[DescriptionOfTheProblem],[RelatedUnitId],[TotalValueOfDamage],[TotalCompensationValue],[CompensationStatusId],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [ProblemName],[TypeOfIncidentId],[HappenDay],[WhereHappened],[Reason],[DescriptionOfTheProblem],[RelatedUnitId],[TotalValueOfDamage],[TotalCompensationValue],[CompensationStatusId],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeProblemInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveProblemInformationRelatedStaff'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveProblemInformationRelatedStaff AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveProblemInformationRelatedStaff]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveProblemInformationRelatedStaff]
	@TypeProblemInformationRelatedStaff TypeProblemInformationRelatedStaff READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeProblemInformationRelatedStaff)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE ProblemInformationRelatedStaff
			SET 
			    [DescribeTheRelationship] = f.[DescribeTheRelationship],
			    [TotalNumberOfDaysOffDueToOccupationalAccidents] = f.[TotalNumberOfDaysOffDueToOccupationalAccidents],
			    [InjuryConditionId] = f.[InjuryConditionId],
			    [ProcessingStatusId] = f.[ProcessingStatusId],
			    [HavePassedLaborSafetyTraining] = f.[HavePassedLaborSafetyTraining],
			    [DecisionNumber] = f.[DecisionNumber],
			    [DecisionDate] = f.[DecisionDate],
			    [KindOfDecisionId] = f.[KindOfDecisionId],
			    [EffectiveDate] = f.[EffectiveDate],
			    [FormsProcessingId] = f.[FormsProcessingId],
			    [TheDecisionId] = f.[TheDecisionId],
			    [CitationOfContent] = f.[CitationOfContent],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeProblemInformationRelatedStaff AS f
			WHERE ProblemInformationRelatedStaff.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE ProblemInformationRelatedStaff
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeProblemInformationRelatedStaff AS f
				WHERE ProblemInformationRelatedStaff.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO ProblemInformationRelatedStaff ([ProblemInformationId],[EmployeeId],[DescribeTheRelationship],[TotalNumberOfDaysOffDueToOccupationalAccidents],[InjuryConditionId],[ProcessingStatusId],[HavePassedLaborSafetyTraining],[DecisionNumber],[DecisionDate],[KindOfDecisionId],[EffectiveDate],[FormsProcessingId],[TheDecisionId],[CitationOfContent],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [ProblemInformationId],[EmployeeId],[DescribeTheRelationship],[TotalNumberOfDaysOffDueToOccupationalAccidents],[InjuryConditionId],[ProcessingStatusId],[HavePassedLaborSafetyTraining],[DecisionNumber],[DecisionDate],[KindOfDecisionId],[EffectiveDate],[FormsProcessingId],[TheDecisionId],[CitationOfContent],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeProblemInformationRelatedStaff
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveProblemInformationTrackEmployeeCompensation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveProblemInformationTrackEmployeeCompensation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveProblemInformationTrackEmployeeCompensation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveProblemInformationTrackEmployeeCompensation]
	@TypeProblemInformationTrackEmployeeCompensation TypeProblemInformationTrackEmployeeCompensation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @TotalRow INT
	DECLARE @Index INT = 1

	DECLARE @TemTable TABLE (
		RowIndex [int] IDENTITY(1, 1),
		Id [int],
		[ProblemInformationId] [int],
		[EmployeeId] [int],
		[AmountMoney] [nvarchar](100),
		[PayDay] [date],
		[SourceCompensation] [nvarchar](100),
		[Type] [int],
		[CreatedDate] [datetime],
		[CreatedBy] [nvarchar](100),
		[ModifiedDate] [datetime],
		[ModifiedBy] [nvarchar](100),
		[IsDeleted] [bit]
	)
	INSERT INTO @TemTable (
		Id,
		[ProblemInformationId],
		[EmployeeId],
		[AmountMoney],
		[PayDay],
		[SourceCompensation],
		[Type],
		[CreatedDate],
		[CreatedBy],
		[ModifiedDate],
		[ModifiedBy],
		[IsDeleted]
	)
	SELECT
		Id,
		[ProblemInformationId],
		[EmployeeId],
		[AmountMoney],
		[PayDay],
		[SourceCompensation],
		[Type],
		[CreatedDate],
		[CreatedBy],
		[ModifiedDate],
		[ModifiedBy],
		[IsDeleted]
	FROM @TypeProblemInformationTrackEmployeeCompensation
	SELECT * FROM @TemTable

	SET @TotalRow = (SELECT COUNT(1) FROM @TypeProblemInformationTrackEmployeeCompensation)
	WHILE @Index <= @TotalRow
	BEGIN
		DECLARE @Id [int]
		DECLARE @ProblemInformationId [int]
		DECLARE @EmployeeId [int]
		DECLARE @AmountMoney [nvarchar](100)
		DECLARE @PayDay [date]
		DECLARE @SourceCompensation [nvarchar](100)
		DECLARE @Type [int]
		DECLARE @CreatedDate [datetime]
		DECLARE @CreatedBy [nvarchar](100)
		DECLARE @ModifiedDate [datetime]
		DECLARE @ModifiedBy [nvarchar](100)
		DECLARE @IsDeleted [bit]

		SELECT TOP 1
			@Id = Id,
			@ProblemInformationId = ProblemInformationId,
			@EmployeeId = EmployeeId, 
			@AmountMoney = AmountMoney, 
			@PayDay = PayDay, 
			@SourceCompensation = SourceCompensation, 
			@Type = [Type], 
			@CreatedDate = CreatedDate, 
			@CreatedBy = CreatedBy, 
			@ModifiedDate = ModifiedDate, 
			@ModifiedBy = ModifiedBy, 
			@IsDeleted = IsDeleted 
		FROM @TemTable
		WHERE RowIndex = @Index

		IF(ISNULL(@Id, 0) = 0)
		BEGIN
			INSERT INTO ProblemInformationTrackEmployeeCompensation ([ProblemInformationId],[EmployeeId],[AmountMoney],[PayDay],[SourceCompensation],[Type],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			VALUES (@ProblemInformationId,@EmployeeId,@AmountMoney,@PayDay,@SourceCompensation,@Type,GETDATE(),@CreatedBy,GETDATE(),@ModifiedBy,@IsDeleted)
		END

		IF(ISNULL(@Id, 0) > 0)
		BEGIN
			UPDATE ProblemInformationTrackEmployeeCompensation
			SET 
			    [AmountMoney] = @AmountMoney,
			    [PayDay] = @PayDay,
			    [SourceCompensation] = @SourceCompensation,
			    [Type] = @Type,
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = @ModifiedBy,
				[IsDeleted] = @IsDeleted
			WHERE Id = @Id
		END
		SET @Index = @Index + 1
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveProfile'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveProfile AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveProfile]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSaveProfile]
	@TypeProfile TypeProfile READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @EmployeeId INT = 0
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeProfile)
	
	DECLARE @WorkUnitId_JobPositionId_Old NVARCHAR(100) = (SELECT CONCAT(WorkUnitId, '-', JobPositionId) FROM EmployeeInformation WHERE Id = @Id_Temp AND ISNULL(IsDeleted, 0) = 0)
	DECLARE @WorkUnitId_JobPositionId_New NVARCHAR(100) = (SELECT CONCAT(WorkUnitId, '-', JobPositionId) FROM @TypeProfile)

	DECLARE @WorkStatusId_DirectManagementId_IndirectManagementId_Old NVARCHAR(100) = (SELECT CONCAT(WorkStatusId, '-', DirectManagementId, '-', IndirectManagementId) FROM JobInformation WHERE EmployeeId = @Id_Temp AND ISNULL(IsDeleted, 0) = 0)
	DECLARE @WorkStatusId_DirectManagementId_IndirectManagementId_New NVARCHAR(100) = (SELECT CONCAT(WorkStatusId, '-', DirectManagementId, '-', IndirectManagementId) FROM @TypeProfile)

	DECLARE @BasicSalary_InsurancePremiums_Old NVARCHAR(100) = (SELECT CONCAT(BasicSalary, '-', InsurancePremiums) FROM JobInformation WHERE EmployeeId = @Id_Temp AND ISNULL(IsDeleted, 0) = 0)
	DECLARE @BasicSalary_InsurancePremiums_New NVARCHAR(100) = (SELECT CONCAT(BasicSalary, '-', InsurancePremiums) FROM @TypeProfile)

	DECLARE @BasicSalary_Param NVARCHAR(100) = (SELECT BasicSalary FROM @TypeProfile)
	DECLARE @InsurancePremiums_Param NVARCHAR(100) = (SELECT InsurancePremiums FROM @TypeProfile)
	
	DECLARE @DegreeTrainingId_Temp INT = (SELECT DegreeTrainingId FROM @TypeProfile)
	DECLARE @TrainingPlacesId_Temp INT = (SELECT TrainingPlacesId FROM @TypeProfile)
	DECLARE @FacultyId_Temp INT = (SELECT FacultyId FROM @TypeProfile)
	DECLARE @SpecializedId_Temp INT = (SELECT SpecializedId FROM @TypeProfile)
	DECLARE @GraduationYear_Temp NVARCHAR(100) = (SELECT GraduationYear FROM @TypeProfile)
	DECLARE @ClassificationId_Temp INT = (SELECT ClassificationId FROM @TypeProfile)

	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			-- Update EmployeeInformation
			UPDATE EmployeeInformation SET 
				[Image] = tp.[Image],
				[EmployeeName] = tp.[EmployeeName],
				[SexId] = tp.[SexId],
				[DateOfBirth] = tp.[DateOfBirth],
				[PersonalTaxCode] = tp.[PersonalTaxCode],
				[WorkUnitId] = tp.[WorkUnitId],
				[JobPositionId] = tp.[JobPositionId],
				[NationId] = tp.[NationId],
				[ReligionId] = tp.[ReligionId],
				[NationalityId] = tp.[NationalityId],
				[IdentificationCardNumber] = tp.[IdentificationCardNumber],
				[DateOfIssueOfIdentificationCard] = tp.[DateOfIssueOfIdentificationCard],
				[PlaceOfIssueOfIdCard] = tp.[PlaceOfIssueOfIdCard],
				[IdentificationCardExpirationDate] = tp.[IdentificationCardExpirationDate],
				[PassportNumber] = tp.[PassportNumber],
				[PassportDate] = tp.[PassportDate],
				[PlaceOfIssueOfPassport] = tp.[PlaceOfIssueOfPassport],
				[PassportExpirationDate] = tp.[PassportExpirationDate],
				[EducationalLevelId] = tp.[EducationalLevelId],
				[DegreeTrainingId] = tp.[DegreeTrainingId],
				[TrainingPlacesId] = tp.[TrainingPlacesId],
				[FacultyId] = tp.[FacultyId],
				[SpecializedId] = tp.[SpecializedId],
				[GraduationYear] = tp.[GraduationYear],
				[ClassificationId] = tp.[ClassificationId],
				[MaritalStatusId] = tp.[MaritalStatusId],
				[FamilyMemberId] = tp.[FamilyMemberId],
				[IngredientsThemselvesId] = tp.[IngredientsThemselvesId],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = tp.[ModifiedBy]
			FROM @TypeProfile AS tp
			WHERE EmployeeInformation.[Id] = @Id_Temp;

			-- Update ContactInformation
			UPDATE ContactInformation SET 
			    [MobilePhone] = tp.[MobilePhone],
			    [OfficePhone] = tp.[OfficePhone],
			    [HomePhone] = tp.[HomePhone],
			    [OtherPhone] = tp.[OtherPhone],
			    [PersonalEmail] = tp.[PersonalEmail],
			    [CompanyEmail] = tp.[CompanyEmail],
			    [OtherEmail] = tp.[OtherEmail],
			    [Skype] = tp.[Skype],
			    [Facebook] = tp.[Facebook],
			    [Domicile] = tp.[Domicile],
			    [ProvinceCityId] = tp.[ProvinceCityId],
			    [PlaceBirth] = tp.[PlaceBirth],
			    [ResidenceNationalityId] = tp.[ResidenceNationalityId],
			    [ResidenceProvinceCityId] = tp.[ResidenceProvinceCityId],
			    [ResidenceDistrictId] = tp.[ResidenceDistrictId],
			    [ResidenceWardsId] = tp.[ResidenceWardsId],
			    [ResidenceHouseStreetVillageNumber] = tp.[ResidenceHouseStreetVillageNumber],
			    [ResidenceAddress] = tp.[ResidenceAddress],
			    [ResidenceHouseholdRegistrationNumber] = tp.[ResidenceHouseholdRegistrationNumber],
			    [ResidenceHouseholdCode] = tp.[ResidenceHouseholdCode],
			    [ResidenceIsHeadHousehold] = tp.[ResidenceIsHeadHousehold],
			    [CurrentNationalityId] = tp.[CurrentNationalityId],
			    [CurrentProvinceCityId] = tp.[CurrentProvinceCityId],
			    [CurrentDistrictId] = tp.[CurrentDistrictId],
			    [CurrentWardsId] = tp.[CurrentWardsId],
			    [CurrentHouseStreetVillageNumber] = tp.[CurrentHouseStreetVillageNumber],
			    [CurrentAddress] = tp.[CurrentAddress],
			    [UrgentContactFirstAndLastName] = tp.[UrgentContactFirstAndLastName],
			    [UrgentContactRelationshipId] = tp.[UrgentContactRelationshipId],
			    [UrgentContactMobilePhone] = tp.[UrgentContactMobilePhone],
			    [UrgentContactHomePhone] = tp.[UrgentContactHomePhone],
			    [UrgentContactEmail] = tp.[UrgentContactEmail],
			    [UrgentContactAddress] = tp.[UrgentContactAddress],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = tp.[ModifiedBy]
			FROM @TypeProfile AS tp
			WHERE ContactInformation.[Id] = @Id_Temp;
			
			-- Update JobInformation
			UPDATE JobInformation SET 
			    [WorkStatusId] = tp.[WorkStatusId],
			    [DirectManagementId] = tp.[DirectManagementId],
			    [IndirectManagementId] = tp.[IndirectManagementId],
			    [WorkLocationId] = tp.[WorkLocationId],
			    [LaborManagementBookNumber] = tp.[LaborManagementBookNumber],
			    [ContractTypeId] = tp.[ContractTypeId],
			    [ApprenticeDay] = tp.[ApprenticeDay],
			    [ProbationDay] = tp.[ProbationDay],
			    [OfficialDate] = tp.[OfficialDate],
			    [NumberOfDaysOff] = tp.[NumberOfDaysOff],
			    [AutomaticallyIncreasesMagicAccordingToSeniority] = tp.[AutomaticallyIncreasesMagicAccordingToSeniority],
			    [IncreaseLaterSpells] = tp.[IncreaseLaterSpells],
			    [WageId] = tp.[WageId],
			    [BasicSalary] = tp.[BasicSalary],
			    [InsurancePremiums] = tp.[InsurancePremiums],
			    [StandardPublicNumber] = tp.[StandardPublicNumber],
			    [StandardPublicId] = tp.[StandardPublicId],
			    [BankAccoun] = tp.[BankAccoun],
			    [BankId] = tp.[BankId],
			    [JoinTheUnion] = tp.[JoinTheUnion],
			    [DateOfInsurance] = tp.[DateOfInsurance],
			    [InsurancePremiumRate] = tp.[InsurancePremiumRate],
			    [SomeSocialInsuranceBooks] = tp.[SomeSocialInsuranceBooks],
			    [SocialInsuranceNumber] = tp.[SocialInsuranceNumber],
			    [ProvinceCodeLevel] = tp.[ProvinceCodeLevel],
			    [ProvinceNameLevelId] = tp.[ProvinceNameLevelId],
			    [HealthInsuranceCardNumber] = tp.[HealthInsuranceCardNumber],
			    [HealthInsuranceExpirationDate] = tp.[HealthInsuranceExpirationDate],
			    [PlaceOfRegistrationForMedicalExaminationAndTreatmentId] = tp.[PlaceOfRegistrationForMedicalExaminationAndTreatmentId],
			    [CodesOfMedicalExaminationAndTreatmentPlaces] = tp.[CodesOfMedicalExaminationAndTreatmentPlaces],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = tp.[ModifiedBy]
			FROM @TypeProfile AS tp
			WHERE JobInformation.[Id] = @Id_Temp;

			-- Insert WorkProgressInformation
			IF @WorkUnitId_JobPositionId_Old <> @WorkUnitId_JobPositionId_New
			BEGIN
				DECLARE @Id_WorkProgress INT = (SELECT TOP 1 Id FROM WorkProgressInformation WHERE EmployeeId = @Id_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
				DECLARE @EndDateValue DATE = (SELECT TOP 1 EndDate FROM WorkProgressInformation WHERE EmployeeId = @Id_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
				IF ISNULL(@EndDateValue, '') = ''
				BEGIN
					UPDATE WorkProgressInformation
					SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [EndDate] = GETDATE()
					FROM @TypeProfile AS f
					WHERE WorkProgressInformation.[Id] = @Id_WorkProgress;

					INSERT INTO WorkProgressInformation (
					    [EmployeeId],
					    [StartDate],
					    [JobPositionId],
					    [WorkUnitId],
					    [WorkStatusId],
					    [DirectManagementId],
					    [IndirectManagementId],
					    [CreatedDate],
					    [CreatedBy],
					    [ModifiedDate],
					    [ModifiedBy],
					    [IsDeleted]
					)
					SELECT
						@Id_Temp,
					    GETDATE(),
					    [JobPositionId],
					    [WorkUnitId],
					    [WorkStatusId],
					    [DirectManagementId],
					    [IndirectManagementId],
						GETDATE(),
						CreatedBy,
						GETDATE(),
						ModifiedBy,
						0
					FROM @TypeProfile
				END
			END

			-- Update WorkProgressInformation
			IF @WorkStatusId_DirectManagementId_IndirectManagementId_Old <> @WorkStatusId_DirectManagementId_IndirectManagementId_New
			BEGIN
				UPDATE WorkProgressInformation
				SET 
					[WorkUnitId] = f.[WorkUnitId],
					[JobPositionId] = f.[JobPositionId],
					[WorkStatusId] = f.[WorkStatusId],
					[DirectManagementId] = f.[DirectManagementId],
					[IndirectManagementId] = f.[IndirectManagementId],
					[ModifiedDate] = GETDATE(),
					[ModifiedBy] = f.[ModifiedBy]
				FROM @TypeProfile AS f
				WHERE WorkProgressInformation.EmployeeId = @Id_Temp AND ISNULL(WorkProgressInformation.EndDate, '') = '' AND ISNULL(WorkProgressInformation.IsDeleted, 0) = 0;
			END

			-- Insert SalaryHistoryInformation
			IF @BasicSalary_InsurancePremiums_Old <> @BasicSalary_InsurancePremiums_New
			BEGIN
				INSERT INTO SalaryHistoryInformation (
					[EmployeeId],
					[JobPositionId],
					[DateOfChange],
					[BasicSalary],
					[InsurancePremiums],
					[JoinInsurance],
					[CreatedDate],
					[CreatedBy],
					[ModifiedDate],
					[ModifiedBy],
					[IsDeleted]
				)
				SELECT
					@Id_Temp,
					JobPositionId,
					GETDATE(),
					BasicSalary,
					InsurancePremiums,
					CASE
						WHEN ISNULL(DateOfInsurance, '') <> '' THEN 1
						ELSE 0
					END,
					GETDATE(),
					CreatedBy,
					GETDATE(),
					ModifiedBy,
					0
				FROM @TypeProfile
			END			
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			-- Insert EmployeeInformation
			INSERT INTO EmployeeInformation (
				[Image],
				[EmployeeCode],
				[EmployeeName],
				[SexId],
				[DateOfBirth],
				[PersonalTaxCode],
				[WorkUnitId],
				[JobPositionId],
				[NationId],
				[ReligionId],
				[NationalityId],
				[IdentificationCardNumber],
				[DateOfIssueOfIdentificationCard],
				[PlaceOfIssueOfIdCard],
				[IdentificationCardExpirationDate],
				[PassportNumber],
				[PassportDate],
				[PlaceOfIssueOfPassport],
				[PassportExpirationDate],
				[EducationalLevelId],
				[DegreeTrainingId],
				[TrainingPlacesId],
				[FacultyId],
				[SpecializedId],
				[GraduationYear],
				[ClassificationId],
				[MaritalStatusId],
				[FamilyMemberId],
				[IngredientsThemselvesId],
				[CreatedDate],
				[CreatedBy],
				[ModifiedDate],
				[ModifiedBy],
				[IsDeleted]
			)
			SELECT 
				[Image],
				'',
				[EmployeeName],
				[SexId],
				[DateOfBirth],
				[PersonalTaxCode],
				[WorkUnitId],
				[JobPositionId],
				[NationId],
				[ReligionId],
				[NationalityId],
				[IdentificationCardNumber],
				[DateOfIssueOfIdentificationCard],
				[PlaceOfIssueOfIdCard],
				[IdentificationCardExpirationDate],
				[PassportNumber],
				[PassportDate],
				[PlaceOfIssueOfPassport],
				[PassportExpirationDate],
				[EducationalLevelId],
				[DegreeTrainingId],
				[TrainingPlacesId],
				[FacultyId],
				[SpecializedId],
				[GraduationYear],
				[ClassificationId],
				[MaritalStatusId],
				[FamilyMemberId],
				[IngredientsThemselvesId],
				GETDATE(),
				[CreatedBy],
				GETDATE(),
				[ModifiedBy],
				0
			FROM @TypeProfile
				
			SET @EmployeeId = (SELECT SCOPE_IDENTITY())
			IF ISNULL(@EmployeeId, 0) > 0
			BEGIN
				-- Update EmployeeInformation
				UPDATE EmployeeInformation SET EmployeeCode = 'TTX' + CONVERT(NVARCHAR(6), @EmployeeId) WHERE Id = @EmployeeId AND ISNULL(IsDeleted,0) = 0
			
				-- Insert PoliticsHealthMilitaryInformation
				INSERT INTO PoliticsHealthMilitaryInformation (
					EmployeeId,
					IsAUnionMember,
					AsAPartyMember,
					PeopleWithDisabilities,
					AsASsoldier,
					AsWoundedSoldiersSickSoldiers,
					EnjoyTheMode,
					CreatedDate,
					CreatedBy,
					ModifiedDate,
					ModifiedBy,
					IsDeleted
				)
				SELECT 
					@EmployeeId,
					0,
					0,
					0,
					0,
					0,
					0,
					GETDATE(),
					[CreatedBy],
					GETDATE(),
					[ModifiedBy],
					0
				FROM @TypeProfile

				-- Insert ContactInformation
				INSERT INTO ContactInformation (
					[EmployeeId],
					[MobilePhone],
					[OfficePhone],
					[HomePhone],
					[OtherPhone],
					[PersonalEmail],
					[CompanyEmail],
					[OtherEmail],
					[Skype],
					[Facebook],
					[Domicile],
					[ProvinceCityId],
					[PlaceBirth],
					[ResidenceNationalityId],
					[ResidenceProvinceCityId],
					[ResidenceDistrictId],
					[ResidenceWardsId],
					[ResidenceHouseStreetVillageNumber],
					[ResidenceAddress],
					[ResidenceHouseholdRegistrationNumber],
					[ResidenceHouseholdCode],
					[ResidenceIsHeadHousehold],
					[CurrentNationalityId],
					[CurrentProvinceCityId],
					[CurrentDistrictId],
					[CurrentWardsId],
					[CurrentHouseStreetVillageNumber],
					[CurrentAddress],
					[UrgentContactFirstAndLastName],
					[UrgentContactRelationshipId],
					[UrgentContactMobilePhone],
					[UrgentContactHomePhone],
					[UrgentContactEmail],
					[UrgentContactAddress],
					[CreatedDate],
					[CreatedBy],
					[ModifiedDate],
					[ModifiedBy],
					[IsDeleted]
				)
				SELECT 
					@EmployeeId,
					[MobilePhone],
					[OfficePhone],
					[HomePhone],
					[OtherPhone],
					[PersonalEmail],
					[CompanyEmail],
					[OtherEmail],
					[Skype],
					[Facebook],
					[Domicile],
					[ProvinceCityId],
					[PlaceBirth],
					[ResidenceNationalityId],
					[ResidenceProvinceCityId],
					[ResidenceDistrictId],
					[ResidenceWardsId],
					[ResidenceHouseStreetVillageNumber],
					[ResidenceAddress],
					[ResidenceHouseholdRegistrationNumber],
					[ResidenceHouseholdCode],
					[ResidenceIsHeadHousehold],
					[CurrentNationalityId],
					[CurrentProvinceCityId],
					[CurrentDistrictId],
					[CurrentWardsId],
					[CurrentHouseStreetVillageNumber],
					[CurrentAddress],
					[UrgentContactFirstAndLastName],
					[UrgentContactRelationshipId],
					[UrgentContactMobilePhone],
					[UrgentContactHomePhone],
					[UrgentContactEmail],
					[UrgentContactAddress],
					GETDATE(),
					[CreatedBy],
					GETDATE(),
					[ModifiedBy],
					0
				FROM @TypeProfile

				-- Insert JobInformation
				INSERT INTO JobInformation (
					[EmployeeId],
					[TimekeepingCode],
					[WorkStatusId],
					[DirectManagementId],
					[IndirectManagementId],
					[WorkLocationId],
					[LaborManagementBookNumber],
					[ContractTypeId],
					[ApprenticeDay],
					[ProbationDay],
					[OfficialDate],
					[NumberOfDaysOff],
					[AutomaticallyIncreasesMagicAccordingToSeniority],
					[IncreaseLaterSpells],
					[WageId],
					[BasicSalary],
					[InsurancePremiums],
					[StandardPublicNumber],
					[StandardPublicId],
					[BankAccoun],
					[BankId],
					[JoinTheUnion],
					[DateOfInsurance],
					[InsurancePremiumRate],
					[SomeSocialInsuranceBooks],
					[SocialInsuranceNumber],
					[ProvinceCodeLevel],
					[ProvinceNameLevelId],
					[HealthInsuranceCardNumber],
					[HealthInsuranceExpirationDate],
					[PlaceOfRegistrationForMedicalExaminationAndTreatmentId],
					[CodesOfMedicalExaminationAndTreatmentPlaces],
					[CreatedDate],
					[CreatedBy],
					[ModifiedDate],
					[ModifiedBy],
					[IsDeleted]
				)
				SELECT 
					@EmployeeId,
					'MCC-TTX' + CONVERT( NVARCHAR(6) , @EmployeeId),
					[WorkStatusId],
					[DirectManagementId],
					[IndirectManagementId],
					[WorkLocationId],
					[LaborManagementBookNumber],
					[ContractTypeId],
					[ApprenticeDay],
					[ProbationDay],
					[OfficialDate],
					[NumberOfDaysOff],
					[AutomaticallyIncreasesMagicAccordingToSeniority],
					[IncreaseLaterSpells],
					[WageId],
					[BasicSalary],
					[InsurancePremiums],
					[StandardPublicNumber],
					[StandardPublicId],
					[BankAccoun],
					[BankId],
					[JoinTheUnion],
					[DateOfInsurance],
					[InsurancePremiumRate],
					[SomeSocialInsuranceBooks],
					[SocialInsuranceNumber],
					[ProvinceCodeLevel],
					[ProvinceNameLevelId],
					[HealthInsuranceCardNumber],
					[HealthInsuranceExpirationDate],
					[PlaceOfRegistrationForMedicalExaminationAndTreatmentId],
					[CodesOfMedicalExaminationAndTreatmentPlaces],
					GETDATE(),
					[CreatedBy],
					GETDATE(),
					[ModifiedBy],
					0
				FROM @TypeProfile

				-- Insert WorkProgressInformation
				INSERT INTO WorkProgressInformation (
					[EmployeeId],
					[StartDate],
					[JobPositionId],
					[WorkUnitId],
					[WorkStatusId],
					[DirectManagementId],
					[IndirectManagementId],
					[CreatedDate],
					[CreatedBy],
					[ModifiedDate],
					[ModifiedBy],
					[IsDeleted]
				)
				SELECT
					@EmployeeId,
					GETDATE(),
					[JobPositionId],
					[WorkUnitId],
					[WorkStatusId],
					[DirectManagementId],
					[IndirectManagementId],
					GETDATE(),
					CreatedBy,
					GETDATE(),
					ModifiedBy,
					0
				FROM @TypeProfile

				IF (ISNULL(@BasicSalary_Param, '0') <> '0' AND ISNULL(@InsurancePremiums_Param, '0') = '0') OR (ISNULL(@BasicSalary_Param, '0') = '0' AND ISNULL(@InsurancePremiums_Param, '0') <> '0') OR (ISNULL(@BasicSalary_Param, '0') <> '0' AND ISNULL(@InsurancePremiums_Param, '0') <> '0')
				BEGIN
					-- Insert SalaryHistoryInformation
					INSERT INTO SalaryHistoryInformation (
						[EmployeeId],
						[JobPositionId],
						[DateOfChange],
						[BasicSalary],
						[InsurancePremiums],
						[JoinInsurance],
						[CreatedDate],
						[CreatedBy],
						[ModifiedDate],
						[ModifiedBy],
						[IsDeleted]
					)
					SELECT
						@EmployeeId,
						JobPositionId,
						GETDATE(),
						BasicSalary,
						InsurancePremiums,
						CASE
							WHEN ISNULL(DateOfInsurance, '') <> '' THEN 1
							ELSE 0
						END,
						GETDATE(),
						CreatedBy,
						GETDATE(),
						ModifiedBy,
						0
					FROM @TypeProfile
				END
			
				-- Insert SkinInformation
				INSERT INTO SkinInformation (
					EmployeeId,
					CreatedDate,
					CreatedBy,
					ModifiedDate,
					ModifiedBy,
					IsDeleted
				)
				SELECT 
					@EmployeeId,
					GETDATE(),
					[CreatedBy],
					GETDATE(),
					[ModifiedBy],
					0
				FROM @TypeProfile
			END
		END
	END	

	-- Insert DegreeInformation
	IF ISNULL(@DegreeTrainingId_Temp, 0) > 0 OR ISNULL(@TrainingPlacesId_Temp, 0) > 0 OR ISNULL(@FacultyId_Temp, 0) > 0 OR ISNULL(@SpecializedId_Temp, 0) > 0 OR ISNULL(@GraduationYear_Temp, '') <> '' OR ISNULL(@ClassificationId_Temp, 0) > 0
	BEGIN
		DECLARE @CheckExistDegreeInformation INT = (SELECT COUNT(Id) FROM DegreeInformation WHERE DegreeTrainingId = @DegreeTrainingId_Temp AND TrainingPlacesId = @TrainingPlacesId_Temp AND FacultyId = @FacultyId_Temp AND SpecializedId = @SpecializedId_Temp AND ToYear = @GraduationYear_Temp AND ClassificationId = @ClassificationId_Temp AND ISNULL(IsDeleted, 0) = 0)
		IF ISNULL(@CheckExistDegreeInformation, 0) = 0
		BEGIN
			INSERT INTO DegreeInformation (
				[EmployeeId],
				[TrainingPlacesId],
				[ToYear],
				[FacultyId],
				[SpecializedId],
				[DegreeTrainingId],
				[ClassificationId],
				[CreatedDate],
				[CreatedBy],
				[ModifiedDate],
				[ModifiedBy],
				[IsDeleted]
			)
			SELECT 
				CASE
					WHEN @Id_Temp > 0 THEN @Id_Temp
					ELSE @EmployeeId
				END,
				[TrainingPlacesId],
				GraduationYear,
				[FacultyId],
				[SpecializedId],
				[DegreeTrainingId],
				[ClassificationId],
				GETDATE(),
				[CreatedBy],
				GETDATE(),
				[ModifiedBy],
				0
			FROM @TypeProfile
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveQuitInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveQuitInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveQuitInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[spSaveQuitInformation]
	@TypeQuitInformation TypeQuitInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeQuitInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE QuitInformation
			SET 
				[NameOfProcedure] = f.[NameOfProcedure],
				[ProcedureGroupQuitId] = f.[ProcedureGroupQuitId],
				[Accomplished] = f.[Accomplished],
				[FinishDay] = f.[FinishDay],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeQuitInformation AS f
			WHERE QuitInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE QuitInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeQuitInformation AS f
				WHERE QuitInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO QuitInformation ([EmployeeId],[NameOfProcedure],[ProcedureGroupQuitId],[Accomplished],[FinishDay],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[NameOfProcedure],[ProcedureGroupQuitId],[Accomplished],[FinishDay],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeQuitInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveReceiveInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveReceiveInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveResignationProcedures]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveResignationProcedures]
	@TypeResignationProcedures TypeResignationProcedures READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeResignationProcedures)
	DECLARE @EmployeeId_Temp INT = (SELECT [EmployeeId] FROM @TypeResignationProcedures)
	DECLARE @IdWorkProgressInformation_Value INT = (SELECT TOP 1 Id FROM WorkProgressInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
	DECLARE @IdContractInformation_Value INT = (SELECT TOP 1 Id FROM ContractInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE ResignationProcedures
			SET 
				[Note] = f.[Note],
			    [DecisionNumber] = f.[DecisionNumber],
			    [DecisionDate] = f.[DecisionDate],
			    [DayOff] = f.[DayOff],
			    [ReviewerId] = f.[ReviewerId],
			    [ReasonForRest] = f.[ReasonForRest],
			    [Comments] = f.[Comments],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeResignationProcedures AS f
			WHERE ResignationProcedures.[Id] = @Id_Temp;

			-- update JobInformation
			UPDATE JobInformation
			SET [WorkStatusId] = 8, [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy]
			FROM @TypeResignationProcedures AS f
			WHERE JobInformation.EmployeeId = @EmployeeId_Temp;

			-- update WorkProgressInformation
			UPDATE WorkProgressInformation
			SET [WorkStatusId] = 8, [EndDate] = GETDATE(), [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy]
			FROM @TypeResignationProcedures AS f
			WHERE WorkProgressInformation.Id = @IdWorkProgressInformation_Value;

			-- update ContractInformation
			UPDATE ContractInformation
			SET ExpirationDate = GETDATE(), [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy]
			FROM @TypeResignationProcedures AS f
			WHERE ContractInformation.Id = @IdContractInformation_Value;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE ResignationProcedures
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeResignationProcedures AS f
				WHERE ResignationProcedures.[Id] = @Id_Temp;

				UPDATE ResignationProceduresEmployeeDebt
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeResignationProcedures AS f
				WHERE ResignationProceduresEmployeeDebt.ResignationProceduresId = @Id_Temp;

				-- update JobInformation
				UPDATE JobInformation
				SET [WorkStatusId] = 1, [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy]
				FROM @TypeResignationProcedures AS f
				WHERE JobInformation.EmployeeId = @EmployeeId_Temp;

				-- update WorkProgressInformation
				UPDATE WorkProgressInformation
				SET [WorkStatusId] = 1, [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy]
				FROM @TypeResignationProcedures AS f
				WHERE WorkProgressInformation.Id = @IdWorkProgressInformation_Value;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			-- Insert ResignationProcedures
			INSERT INTO ResignationProcedures ([EmployeeId],[ExpectedResignationDate],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[ExpectedResignationDate],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeResignationProcedures
			DECLARE @ResignationProceduresId INT = @@IDENTITY

			-- Số tiền đã tạm ứng
			DECLARE @AmountOfAdvance_Value INT = 
			(
				SELECT SUM(CONVERT(INT,AmountOfAdvance)) AS AmountOfAdvance 
				FROM EmployeesOnBusinessTrip 
				WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted,0) = 0 AND ISNULL(BrowsingStatusId, 0) = 7
			)

			-- Số tiền thanh toán
			DECLARE @AmountSpent_Value INT = 
			(
				SELECT SUM(CONVERT(INT,e.AmountSpent)) AS AmountSpent
				FROM EmployeesOnBusinessTripPayments e
				LEFT JOIN EmployeesOnBusinessTrip e1 ON e1.Id = e.EmployeesOnBusinessTripId AND ISNULL(e1.IsDeleted,0) = 0 AND ISNULL(BrowsingStatusId, 0) = 7
				WHERE e1.EmployeeId = @EmployeeId_Temp AND ISNULL(e.IsDeleted,0) = 0
			)

			-- Insert ResignationProceduresEmployeeDebt
			INSERT INTO ResignationProceduresEmployeeDebt 
			(
				[ResignationProceduresId],
				[NameOfTheDebtId],
				[AmountOfMoney],
				[Accomplished],
				[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted]
			)
			SELECT 
				@ResignationProceduresId,
				CASE
					WHEN @AmountOfAdvance_Value > @AmountSpent_Value THEN 9
					ELSE 10
				END,
				CASE
					WHEN @AmountOfAdvance_Value > @AmountSpent_Value THEN (@AmountOfAdvance_Value - @AmountSpent_Value)
					ELSE (@AmountSpent_Value - @AmountOfAdvance_Value)
				END,
				0,
				GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeResignationProcedures

			-- update JobInformation
			UPDATE JobInformation
			SET [WorkStatusId] = 2, [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy]
			FROM @TypeResignationProcedures AS f
			WHERE JobInformation.EmployeeId = @EmployeeId_Temp;

			-- update WorkProgressInformation
			UPDATE WorkProgressInformation
			SET [WorkStatusId] = 2, [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy]
			FROM @TypeResignationProcedures AS f
			WHERE WorkProgressInformation.Id = @IdWorkProgressInformation_Value;
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveResignationProceduresEmployeeDebt'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveResignationProceduresEmployeeDebt AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveResignationProceduresEmployeeDebt]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveResignationProceduresEmployeeDebt]
	@TypeResignationProceduresEmployeeDebt TypeResignationProceduresEmployeeDebt READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeResignationProceduresEmployeeDebt)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE ResignationProceduresEmployeeDebt
			SET 
				[NameOfTheDebtId] = f.[NameOfTheDebtId],
				[AmountOfMoney] = f.[AmountOfMoney],
				[FinishDay] = f.[FinishDay],
				[Accomplished] = f.[Accomplished],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeResignationProceduresEmployeeDebt AS f
			WHERE ResignationProceduresEmployeeDebt.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE ResignationProceduresEmployeeDebt
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeResignationProceduresEmployeeDebt AS f
				WHERE ResignationProceduresEmployeeDebt.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO ResignationProceduresEmployeeDebt ([ResignationProceduresId],[NameOfTheDebtId],[AmountOfMoney],[FinishDay],[Accomplished],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [ResignationProceduresId],[NameOfTheDebtId],[AmountOfMoney],[FinishDay],[Accomplished],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeResignationProceduresEmployeeDebt
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveSalaryHistoryInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveSalaryHistoryInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveSalaryHistoryInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveSalaryHistoryInformation]
	@TypeSalaryHistoryInformation TypeSalaryHistoryInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeSalaryHistoryInformation)
	DECLARE @EmployeeId_Temp INT = (SELECT EmployeeId FROM @TypeSalaryHistoryInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE SalaryHistoryInformation
			SET 
				[DateOfChange] = f.[DateOfChange],
				[BasicSalary] = f.[BasicSalary],
				[InsurancePremiums] = f.[InsurancePremiums],
				[JoinInsurance] = f.[JoinInsurance],
				[Explain] = f.[Explain],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeSalaryHistoryInformation AS f
			WHERE SalaryHistoryInformation.[Id] = @Id_Temp;

			DECLARE @Id_Salary INT = (SELECT TOP 1 Id FROM SalaryHistoryInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
			IF @Id_Salary = @Id_Temp
			BEGIN
				-- Update JobInformation
				UPDATE JobInformation
				SET [BasicSalary] = wpi.[BasicSalary],
					[InsurancePremiums] = wpi.InsurancePremiums,
					[ModifiedDate] = GETDATE(), 
					[ModifiedBy] = wpi.[ModifiedBy]
				FROM @TypeSalaryHistoryInformation AS wpi
				WHERE JobInformation.EmployeeId = @EmployeeId_Temp;
			END
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE SalaryHistoryInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeSalaryHistoryInformation AS f
				WHERE SalaryHistoryInformation.[Id] = @Id_Temp;

				DECLARE @Id_SalaryHistory_Delete INT = (SELECT TOP 1 Id FROM SalaryHistoryInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC) 
				IF ISNULL(@Id_SalaryHistory_Delete, 0) > 0
				BEGIN
					-- Update JobInformation
					UPDATE JobInformation
					SET [BasicSalary] = wpi.[BasicSalary],
						[InsurancePremiums] = wpi.InsurancePremiums,
						[ModifiedDate] = GETDATE(), 
						[ModifiedBy] = wpi.[ModifiedBy]
					FROM SalaryHistoryInformation AS wpi
					WHERE JobInformation.EmployeeId = @EmployeeId_Temp AND wpi.Id = @Id_SalaryHistory_Delete AND ISNULL(wpi.IsDeleted, 0) = 0;
				END
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO SalaryHistoryInformation ([EmployeeId],[JobPositionId],[DateOfChange],[BasicSalary],[InsurancePremiums],[JoinInsurance],[Explain],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[JobPositionId],[DateOfChange],[BasicSalary],[InsurancePremiums],[JoinInsurance],[Explain],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeSalaryHistoryInformation

			-- Update JobInformation
			UPDATE JobInformation
			SET [BasicSalary] = wpi.[BasicSalary],
				[InsurancePremiums] = wpi.InsurancePremiums,
				[ModifiedDate] = GETDATE(), 
				[ModifiedBy] = wpi.[ModifiedBy]
			FROM @TypeSalaryHistoryInformation AS wpi
			WHERE JobInformation.EmployeeId = @EmployeeId_Temp;
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveSkillInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveSkillInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveSkillInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveSkillInformation]
	@TypeSkillInformation TypeSkillInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeSkillInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE SkillInformation
			SET 
				[SkillName] = f.[SkillName],
				[SkillGroupId] = f.[SkillGroupId],
				[SkillLevelId] = f.[SkillLevelId],
				[Note] = f.[Note],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeSkillInformation AS f
			WHERE SkillInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE SkillInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeSkillInformation AS f
				WHERE SkillInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO SkillInformation ([EmployeeId],[SkillName],[SkillGroupId],[SkillLevelId],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[SkillName],[SkillGroupId],[SkillLevelId],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeSkillInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveSkinInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveSkinInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveSkinInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveSkinInformation]
	@TypeSkinInformation TypeSkinInformation READONLY
AS
BEGIN
	SET NOCOUNT ON;
		
	UPDATE SkinInformation
	SET 
		[ShirtStringId] = p.[ShirtStringId],
        [TrousersStringId] = p.[TrousersStringId],
        [ZuypStringId] = p.[ZuypStringId],
        [ProtectiveGearStringId] = p.[ProtectiveGearStringId],
        [ShirtNumberId] = p.[ShirtNumberId],
        [TrousersNumberId] = p.[TrousersNumberId],
        [ZuypNumberId] = p.[ZuypNumberId],
        [ProtectiveGearNumberId] = p.[ProtectiveGearNumberId],
        [ShoulderWidth] = p.[ShoulderWidth],
        [LongSleeve] = p.[LongSleeve],
        [LongCoat] = p.[LongCoat],
        [ChestRing] = p.[ChestRing],
        [Waist] = p.[Waist],
        [Buttocks] = p.[Buttocks],
        [LongPants] = p.[LongPants],
        [LongSkirt] = p.[LongSkirt],
        [LapThigh] = p.[LapThigh],
		[ModifiedDate] = GETDATE(),
		[ModifiedBy] = p.[ModifiedBy]
	FROM @TypeSkinInformation AS p
	WHERE SkinInformation.EmployeeId = p.EmployeeId;

END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveTrainingProcessInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveTrainingProcessInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveTrainingProcessInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveTrainingProcessInformation]
	@TypeTrainingProcessInformation TypeTrainingProcessInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeTrainingProcessInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE TrainingProcessInformation
			SET 
				[TrainingProcessCode] = f.[TrainingProcessCode],
				[TrainingProcessName] = f.[TrainingProcessName],
				[StartDay] = f.[StartDay],
				[EndDate] = f.[EndDate],
				[Purpose] = f.[Purpose],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeTrainingProcessInformation AS f
			WHERE TrainingProcessInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE TrainingProcessInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeTrainingProcessInformation AS f
				WHERE TrainingProcessInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO TrainingProcessInformation ([EmployeeId],[TrainingProcessCode],[TrainingProcessName],[StartDay],[EndDate],[Purpose],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[TrainingProcessCode],[TrainingProcessName],[StartDay],[EndDate],[Purpose],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeTrainingProcessInformation

			DECLARE @Id INT = (SELECT SCOPE_IDENTITY())
			-- Update TrainingProcessInformation
			UPDATE TrainingProcessInformation SET [TrainingProcessCode] = CONCAT(N'QTĐT-TTX',CONVERT(NVARCHAR(6), @Id)) WHERE Id = @Id AND ISNULL(IsDeleted,0) = 0
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveWorkExperienceInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveWorkExperienceInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveWorkExperienceInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveWorkExperienceInformation]
	@TypeWorkExperienceInformation TypeWorkExperienceInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeWorkExperienceInformation)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			UPDATE WorkExperienceInformation
			SET 
				[FromMonthAndYear] = f.[FromMonthAndYear],
			    [ByMonthAndYear] = f.[ByMonthAndYear],
			    [Workplace] = f.[Workplace],
			    [JobPosition] = f.[JobPosition],
			    [Wage] = f.[Wage],
			    [JobDescription] = f.[JobDescription],
			    [Note] = f.[Note],
			    [FirstAndLastName] = f.[FirstAndLastName],
			    [Title] = f.[Title],
			    [Phone] = f.[Phone],
			    [Email] = f.[Email],
			    [HaveCheckedCompared] = f.[HaveCheckedCompared],
				[ModifiedDate] = GETDATE(),
				[ModifiedBy] = f.[ModifiedBy]
			FROM @TypeWorkExperienceInformation AS f
			WHERE WorkExperienceInformation.[Id] = @Id_Temp;
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE WorkExperienceInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeWorkExperienceInformation AS f
				WHERE WorkExperienceInformation.[Id] = @Id_Temp;
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			INSERT INTO WorkExperienceInformation ([EmployeeId],[FromMonthAndYear],[ByMonthAndYear],[Workplace],[JobPosition],[Wage],[JobDescription],[Note],[FirstAndLastName],[Title],[Phone],[Email],[HaveCheckedCompared],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[FromMonthAndYear],[ByMonthAndYear],[Workplace],[JobPosition],[Wage],[JobDescription],[Note],[FirstAndLastName],[Title],[Phone],[Email],[HaveCheckedCompared],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeWorkExperienceInformation
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spSaveWorkProgressInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spSaveWorkProgressInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spSaveWorkProgressInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spSaveWorkProgressInformation]
	@TypeWorkProgressInformation TypeWorkProgressInformation READONLY,
	@IsAction INT
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @Id_Temp INT = (SELECT [Id] FROM @TypeWorkProgressInformation)
	DECLARE @EmployeeId_Temp INT = (SELECT EmployeeId FROM @TypeWorkProgressInformation)

	DECLARE @WorkUnitId_JobPositionId_Old NVARCHAR(100) = (SELECT TOP 1 CONCAT(WorkUnitId, '-', JobPositionId) FROM WorkProgressInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
	DECLARE @WorkUnitId_JobPositionId_New NVARCHAR(100) = (SELECT CONCAT(WorkUnitId, '-', JobPositionId) FROM @TypeWorkProgressInformation)

	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			DECLARE @EndDate_Update DATE = (SELECT EndDate FROM WorkProgressInformation WHERE Id = @Id_Temp AND ISNULL(IsDeleted, 0) = 0)
			IF ISNULL(@EndDate_Update, '') = ''
			BEGIN
				UPDATE WorkProgressInformation
				SET 
					[StartDate] = f.[StartDate],
					[WorkStatusId] = f.[WorkStatusId],
					[DirectManagementId] = f.[DirectManagementId],
					[IndirectManagementId] = f.[IndirectManagementId],
					[DecisionNumber] = f.[DecisionNumber],
					[DecisionDate] = f.[DecisionDate],
					[Note] = f.[Note],
					[ModifiedDate] = GETDATE(),
					[ModifiedBy] = f.[ModifiedBy]
				FROM @TypeWorkProgressInformation AS f
				WHERE WorkProgressInformation.[Id] = @Id_Temp;

				IF @WorkUnitId_JobPositionId_Old <> @WorkUnitId_JobPositionId_New
				BEGIN
					DECLARE @Id_WorkProgress_Update INT = (SELECT TOP 1 Id FROM WorkProgressInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
					DECLARE @EndDateValue DATE = (SELECT TOP 1 EndDate FROM WorkProgressInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
					IF ISNULL(@EndDateValue, '') = ''
					BEGIN
						UPDATE WorkProgressInformation
						SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [EndDate] = GETDATE()
						FROM @TypeWorkProgressInformation AS f
						WHERE WorkProgressInformation.[Id] = @Id_WorkProgress_Update;
					END

					-- Insert WorkProgressInformation
					INSERT INTO WorkProgressInformation (
						[EmployeeId],
						[StartDate],
						[JobPositionId],
						[WorkUnitId],
						[WorkStatusId],
						[DirectManagementId],
						[IndirectManagementId],
						[DecisionNumber],
						[DecisionDate],
						[Note],
						[CreatedDate],
						[CreatedBy],
						[ModifiedDate],
						[ModifiedBy],
						[IsDeleted]
					)
					SELECT 
						[EmployeeId],
						[StartDate],
						[JobPositionId],
						[WorkUnitId],
						[WorkStatusId],
						[DirectManagementId],
						[IndirectManagementId],
						[DecisionNumber],
						[DecisionDate],
						[Note],
						GETDATE(),
						[CreatedBy],
						GETDATE(),
						[ModifiedBy],
						0
					FROM @TypeWorkProgressInformation
				END

				-- Update EmployeeInformation
				UPDATE EmployeeInformation
				SET WorkUnitId = wpi.WorkUnitId,
					JobPositionId = wpi.JobPositionId,
					[ModifiedDate] = GETDATE(), 
					[ModifiedBy] = wpi.[ModifiedBy]
				FROM @TypeWorkProgressInformation AS wpi
				WHERE EmployeeInformation.Id = @EmployeeId_Temp;

				-- Update JobInformation
				UPDATE JobInformation
				SET	DirectManagementId = wpi.DirectManagementId,
					IndirectManagementId = wpi.IndirectManagementId,
					WorkStatusId = wpi.WorkStatusId,
					[ModifiedDate] = GETDATE(), 
					[ModifiedBy] = wpi.[ModifiedBy]
				FROM @TypeWorkProgressInformation AS wpi
				WHERE JobInformation.EmployeeId = @EmployeeId_Temp;
			END
			ELSE
			BEGIN
				UPDATE WorkProgressInformation
				SET 
					[StartDate] = f.[StartDate],
					[WorkUnitId] = f.[WorkUnitId],
					[JobPositionId] = f.[JobPositionId],
					[WorkStatusId] = f.[WorkStatusId],
					[DirectManagementId] = f.[DirectManagementId],
					[IndirectManagementId] = f.[IndirectManagementId],
					[DecisionNumber] = f.[DecisionNumber],
					[DecisionDate] = f.[DecisionDate],
					[Note] = f.[Note],
					[ModifiedDate] = GETDATE(),
					[ModifiedBy] = f.[ModifiedBy]
				FROM @TypeWorkProgressInformation AS f
				WHERE WorkProgressInformation.[Id] = @Id_Temp;
			END
		END
		ELSE
		BEGIN
			IF ISNULL(@IsAction, 0) = 2 --Action Delete
			BEGIN
				UPDATE WorkProgressInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [IsDeleted] = 1
				FROM @TypeWorkProgressInformation AS f
				WHERE WorkProgressInformation.[Id] = @Id_Temp;

				DECLARE @EndDate_Delete DATE = (SELECT EndDate FROM WorkProgressInformation WHERE Id = @Id_Temp)
				IF ISNULL(@EndDate_Delete, '') = ''
				BEGIN
					DECLARE @Id_WorkProgress_Delete INT = (SELECT TOP 1 Id FROM WorkProgressInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC) 
					IF ISNULL(@Id_WorkProgress_Delete, 0) > 0
					BEGIN
						UPDATE WorkProgressInformation
						SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [EndDate] = NULL
						FROM @TypeWorkProgressInformation AS f
						WHERE WorkProgressInformation.[Id] = @Id_WorkProgress_Delete;

						-- Update EmployeeInformation
						UPDATE EmployeeInformation
						SET WorkUnitId = wpi.WorkUnitId,
							JobPositionId = wpi.JobPositionId,
							[ModifiedDate] = GETDATE(), 
							[ModifiedBy] = wpi.[ModifiedBy]
						FROM WorkProgressInformation AS wpi
						WHERE EmployeeInformation.Id = @EmployeeId_Temp AND wpi.Id = @Id_WorkProgress_Delete AND ISNULL(wpi.IsDeleted, 0) = 0;

						-- Update JobInformation
						UPDATE JobInformation
						SET	DirectManagementId = wpi.DirectManagementId,
							IndirectManagementId = wpi.IndirectManagementId,
							WorkStatusId = wpi.WorkStatusId,
							[ModifiedDate] = GETDATE(), 
							[ModifiedBy] = wpi.[ModifiedBy]
						FROM WorkProgressInformation AS wpi
						WHERE JobInformation.EmployeeId = @EmployeeId_Temp AND wpi.Id = @Id_WorkProgress_Delete AND ISNULL(wpi.IsDeleted, 0) = 0;
					END
				END
			END
		END
	END
	ELSE
	BEGIN
		IF ISNULL(@IsAction, 0) = 1 --Action Create&Update
		BEGIN
			-- Update before insert
			DECLARE @Id_WorkProgress_Insert INT = (SELECT TOP 1 Id FROM WorkProgressInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
			DECLARE @EndDate_Insert DATE = (SELECT TOP 1 EndDate FROM WorkProgressInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
			IF ISNULL(@EndDate_Insert, '') = ''
			BEGIN
				UPDATE WorkProgressInformation
				SET [ModifiedDate] = GETDATE(), [ModifiedBy] = f.[ModifiedBy], [EndDate] = GETDATE()
				FROM @TypeWorkProgressInformation AS f
				WHERE WorkProgressInformation.[Id] = @Id_WorkProgress_Insert;
			END
			
			INSERT INTO WorkProgressInformation ([EmployeeId],[StartDate],[JobPositionId],[WorkUnitId],[WorkStatusId],[DirectManagementId],[IndirectManagementId],[DecisionNumber],[DecisionDate],[Note],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDeleted])
			SELECT [EmployeeId],[StartDate],[JobPositionId],[WorkUnitId],[WorkStatusId],[DirectManagementId],[IndirectManagementId],[DecisionNumber],[DecisionDate],[Note],GETDATE(),[CreatedBy],GETDATE(),[ModifiedBy],0
			FROM @TypeWorkProgressInformation

			-- Update EmployeeInformation
			UPDATE EmployeeInformation
			SET WorkUnitId = wpi.WorkUnitId,
				JobPositionId = wpi.JobPositionId,
				[ModifiedDate] = GETDATE(), 
				[ModifiedBy] = wpi.[ModifiedBy]
			FROM @TypeWorkProgressInformation AS wpi
			WHERE EmployeeInformation.Id = @EmployeeId_Temp;

			-- Update JobInformation
			UPDATE JobInformation
			SET	DirectManagementId = wpi.DirectManagementId,
				IndirectManagementId = wpi.IndirectManagementId,
				WorkStatusId = wpi.WorkStatusId,
				[ModifiedDate] = GETDATE(), 
				[ModifiedBy] = wpi.[ModifiedBy]
			FROM @TypeWorkProgressInformation AS wpi
			WHERE JobInformation.EmployeeId = @EmployeeId_Temp;
		END
	END
END




IF NOT EXISTS ( SELECT  1
            FROM    sys.procedures
            WHERE   name = 'spUpdateAttachmentInformation'
                    AND SCHEMA_NAME(schema_id) = 'dbo' )
    BEGIN 
        exec('alter PROCEDURE [dbo].spUpdateAttachmentInformation AS BEGIN SET NOCOUNT ON; END')
    END
GO
/****** Object:  StoredProcedure [dbo].[spUpdateAttachmentInformation]    Script Date: 6/10/2022 2:47:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[spUpdateAttachmentInformation]
	@TypeAttachment TypeAttachment READONLY
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @EmployeeId_Temp INT = (SELECT EmployeeId FROM @TypeAttachment)
	DECLARE @Id_Temp INT = (SELECT TOP 1 [Id] FROM AttachmentInformation WHERE EmployeeId = @EmployeeId_Temp AND ISNULL(IsDeleted, 0) = 0 ORDER BY Id DESC)
	IF ISNULL(@Id_Temp, 0) > 0 
	BEGIN
		UPDATE AttachmentInformation
		SET 
			FileContent = f.FileContent,
			[FileName] = f.[FileName],
			FileSize = f.FileSize,
			FileType = f.FileType
		FROM @TypeAttachment AS f
		WHERE AttachmentInformation.[Id] = @Id_Temp;
	END
END

GO
USE [master]
GO
ALTER DATABASE [DEV_NHIGIA] SET  READ_WRITE 
GO
