using System;
using System.Collections.Generic;

namespace NHIGIA.Modern.Models
{
    public class WorkflowDefinitionModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; }
        public int StepCount { get; set; }
    }

    public class WorkflowStepModel
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public string ApproverType { get; set; }
        public int? SpecificUserId { get; set; }
        public string SpecificUserName { get; set; }
        public decimal? ThresholdAmount { get; set; }
        public int SlaHours { get; set; }
        public bool CanRejectToStart { get; set; }
    }

    public class WorkflowInstanceModel
    {
        public long Id { get; set; }
        public string InstanceCode { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowCode { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowIcon { get; set; }
        public int RequesterUserId { get; set; }
        public string RequesterName { get; set; }
        public string RequesterDepartment { get; set; }
        public int? CurrentStepId { get; set; }
        public string CurrentStepName { get; set; }
        public int? CurrentApproverId { get; set; }
        public string CurrentApproverName { get; set; }
        public string StatusCode { get; set; } // PENDING, APPROVED, REJECTED, CANCELLED
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        public string TargetTable { get; set; }
        public long? TargetRecordId { get; set; }
        public string PayloadJson { get; set; }
        public string DigitalSignature { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<WorkflowLogModel> Logs { get; set; } = new List<WorkflowLogModel>();
    }

    public class WorkflowLogModel
    {
        public long Id { get; set; }
        public long InstanceId { get; set; }
        public int? StepId { get; set; }
        public string StepName { get; set; }
        public int ActorUserId { get; set; }
        public string ActorName { get; set; }
        public string ActionCode { get; set; } // SUBMIT, APPROVE, REJECT, CANCEL, DELEGATE
        public string Comment { get; set; }
        public string DigitalSignature { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateWorkflowRequest
    {
        public string WorkflowCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        public string PayloadJson { get; set; }
        public string DigitalSignature { get; set; }
        public string TargetTable { get; set; }
        public long? TargetRecordId { get; set; }
    }

    public class ProcessWorkflowRequest
    {
        public long InstanceId { get; set; }
        public bool Approve { get; set; }
        public string Comment { get; set; }
        public string DigitalSignature { get; set; }
    }

    public class GpsAttendanceLogModel
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? AccuracyMeters { get; set; }
        public string LocationAddress { get; set; }
        public DateTime CheckTime { get; set; }
        public string CheckType { get; set; } // IN, OUT
        public string SelfieImageUrl { get; set; }
        public decimal? FaceMatchScore { get; set; }
        public bool IsMockLocation { get; set; }
        public string DeviceInfo { get; set; }
        public bool IsWithinOfficeGeofence { get; set; }
        public string MatchedOfficeName { get; set; }
        public double DistanceMeters { get; set; }
    }

    public class OfficeLocationModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusMeters { get; set; }
        public bool IsActive { get; set; }
    }

    public class EmployeeDocumentModel
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string DocType { get; set; } // CONTRACT, ID_CARD, DIPLOMA, CERTIFICATE, DECISION
        public string DocNumber { get; set; }
        public string DocTitle { get; set; }
        public string FileUrl { get; set; }
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsVerifiedByHr { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}

