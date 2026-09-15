SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- This file contains deterministic DEMO data only. Every insert is idempotent.
-- Operational records can be separated from these records by their DEMO prefix.

DECLARE @AdminId INT = (SELECT TOP (1) Id FROM dbo.HrmUserAccount WHERE Username = 'admin');
DECLARE @HrId INT = (SELECT TOP (1) Id FROM dbo.HrmUserAccount WHERE Username = 'hradmin');
DECLARE @ManagerId INT = (SELECT TOP (1) Id FROM dbo.HrmUserAccount WHERE Username = 'huongtm');

-- Work schedules for all demo employees.
INSERT dbo.HrmEmployeeSchedule
    (UserId, ShiftTemplateId, ShiftName, StartTime, EndTime, BreakMinutes, GraceMinutes,
     EffectiveFrom, EffectiveTo, StatusCode, CreatedByUserId)
SELECT u.Id, s.Id, s.Name, s.StartTime, s.EndTime, s.BreakMinutes, s.GraceMinutes,
       '2026-01-01', NULL, 'ACTIVE', @HrId
FROM dbo.HrmUserAccount u
CROSS JOIN dbo.HrmShiftTemplate s
WHERE u.Username LIKE 'demo[0-9][0-9]'
  AND s.Code = CASE WHEN u.Username IN ('demo06', 'demo12', 'demo18') THEN 'SANG' ELSE 'HC' END
  AND NOT EXISTS
  (
      SELECT 1 FROM dbo.HrmEmployeeSchedule existing
      WHERE existing.UserId = u.Id AND existing.EffectiveFrom = '2026-01-01'
  );
GO

-- Attendance events covering on-time, late, early-leave, missing check-out and absent scenarios.
DECLARE @DemoAttendance TABLE
(
    EventKey NVARCHAR(200) NOT NULL,
    Username NVARCHAR(80) NOT NULL,
    CheckTime DATETIME2 NOT NULL,
    EventType NVARCHAR(80) NOT NULL
);
INSERT @DemoAttendance(EventKey, Username, CheckTime, EventType)
VALUES
    ('DEMO-ATT-20260915-01-IN',  'demo01', '2026-09-15T07:55:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-01-OUT', 'demo01', '2026-09-15T17:05:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-02-IN',  'demo02', '2026-09-15T08:12:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-02-OUT', 'demo02', '2026-09-15T17:10:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-03-IN',  'demo03', '2026-09-15T07:58:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-03-OUT', 'demo03', '2026-09-15T16:35:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-04-IN',  'demo04', '2026-09-15T08:03:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-04-OUT', 'demo04', '2026-09-15T17:02:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-05-IN',  'demo05', '2026-09-15T08:25:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-05-OUT', 'demo05', '2026-09-15T17:30:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-06-IN',  'demo06', '2026-09-15T05:52:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-06-OUT', 'demo06', '2026-09-15T14:06:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-07-IN',  'demo07', '2026-09-15T07:49:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-07-OUT', 'demo07', '2026-09-15T17:12:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-08-IN',  'demo08', '2026-09-15T08:09:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-08-OUT', 'demo08', '2026-09-15T17:00:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-09-IN',  'demo09', '2026-09-15T08:18:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-11-IN',  'demo11', '2026-09-15T07:57:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-11-OUT', 'demo11', '2026-09-15T17:01:00', 'CHECK_OUT'),
    ('DEMO-ATT-20260915-12-IN',  'demo12', '2026-09-15T06:07:00', 'CHECK_IN'),
    ('DEMO-ATT-20260915-12-OUT', 'demo12', '2026-09-15T14:00:00', 'CHECK_OUT');

INSERT dbo.HrmAttendanceEvent
    (EventKey, UserId, PersonId, AliasId, PlaceId, DeviceId, CheckTime, EventType, PayloadJson)
SELECT a.EventKey, u.Id, CONCAT('DEMO-PERSON-', RIGHT(a.Username, 2)),
       CONCAT('DEMO-', RIGHT(a.Username, 2)), 'DEMO-PLACE', 'DEMO-CAMERA-01',
       a.CheckTime, a.EventType,
       CONCAT('{"source":"DEMO","employee":"', a.Username, '","event":"', a.EventType, '"}')
FROM @DemoAttendance a
INNER JOIN dbo.HrmUserAccount u ON u.Username = a.Username
WHERE NOT EXISTS (SELECT 1 FROM dbo.HrmAttendanceEvent e WHERE e.EventKey = a.EventKey);
GO

-- Leave requests for all approval states.
DECLARE @DemoLeave TABLE
(
    Username NVARCHAR(80), LeaveType NVARCHAR(80), StartDate DATE, EndDate DATE,
    SessionCode NVARCHAR(30), HandoverTo NVARCHAR(150), Reason NVARCHAR(1000),
    StatusCode NVARCHAR(30), ManagerNote NVARCHAR(1000), HrNote NVARCHAR(1000), CreatedAt DATETIME2
);
INSERT @DemoLeave VALUES
    ('demo01', N'Phép năm',       '2026-09-21', '2026-09-22', 'FULL_DAY', N'Bùi Khánh Linh', N'[DEMO-LEAVE-01] Nghỉ phép giải quyết việc gia đình.', 'PENDING_MANAGER', NULL, NULL, '2026-09-14T08:20:00'),
    ('demo02', N'Nghỉ ốm',        '2026-09-16', '2026-09-16', 'MORNING',  N'Hoàng Đức Nam',  N'[DEMO-LEAVE-02] Khám sức khỏe theo lịch hẹn.',       'PENDING_HR', N'Đã kiểm tra bàn giao công việc.', NULL, '2026-09-13T09:10:00'),
    ('demo03', N'Phép năm',       '2026-09-28', '2026-09-30', 'FULL_DAY', N'Tạ Minh Châu',   N'[DEMO-LEAVE-03] Nghỉ phép cá nhân.',                 'APPROVED', N'Đồng ý.', N'Đã duyệt phép.', '2026-09-10T10:05:00'),
    ('demo04', N'Nghỉ không lương','2026-10-05','2026-10-06', 'FULL_DAY', N'Phan Bảo Ngọc',  N'[DEMO-LEAVE-04] Giải quyết công việc riêng.',         'REJECTED', N'Cần sắp xếp lại lịch khách hàng.', N'Từ chối theo ý kiến quản lý.', '2026-09-09T14:30:00'),
    ('demo05', N'Phép năm',       '2026-09-18', '2026-09-18', 'AFTERNOON',N'Cao Mỹ Linh',    N'[DEMO-LEAVE-05] Nghỉ phép buổi chiều.',              'PENDING_MANAGER', NULL, NULL, '2026-09-15T07:45:00'),
    ('demo06', N'Nghỉ bù',        '2026-09-25', '2026-09-25', 'FULL_DAY', N'Dương Thành Đạt',N'[DEMO-LEAVE-06] Nghỉ bù thời gian trực cuối tuần.',   'APPROVED', N'Đồng ý.', N'Đã đối chiếu tăng ca.', '2026-09-08T11:20:00');

INSERT dbo.HrmLeaveRequest
    (UserId, LeaveType, StartDate, EndDate, SessionCode, HandoverTo, Reason,
     AttachmentName, AttachmentContentType, AttachmentContent, StatusCode,
     ManagerNote, HrNote, ApprovedByManagerId, ApprovedByHrId, CreatedAt)
SELECT u.Id, l.LeaveType, l.StartDate, l.EndDate, l.SessionCode, l.HandoverTo, l.Reason,
       CASE WHEN l.Username = 'demo02' THEN 'demo-giay-kham.txt' ELSE NULL END,
       CASE WHEN l.Username = 'demo02' THEN 'text/plain' ELSE NULL END,
       CASE WHEN l.Username = 'demo02' THEN CONVERT(VARBINARY(MAX), N'Giấy xác nhận khám sức khỏe - dữ liệu thử nghiệm') ELSE NULL END,
       l.StatusCode, l.ManagerNote, l.HrNote,
       CASE WHEN l.StatusCode <> 'PENDING_MANAGER' THEN @ManagerId ELSE NULL END,
       CASE WHEN l.StatusCode IN ('APPROVED', 'REJECTED') THEN @HrId ELSE NULL END,
       l.CreatedAt
FROM @DemoLeave l
INNER JOIN dbo.HrmUserAccount u ON u.Username = l.Username
WHERE NOT EXISTS (SELECT 1 FROM dbo.HrmLeaveRequest r WHERE r.Reason = l.Reason);
GO

-- Internal communications, including a downloadable attachment.
IF NOT EXISTS (SELECT 1 FROM dbo.HrmCommunication WHERE Title = N'[DEMO] Thông báo lịch nghỉ lễ')
    INSERT dbo.HrmCommunication
        (AuthorUserId, Category, ScopeCode, DepartmentId, Title, Body, AttachmentName,
         AttachmentContentType, AttachmentContent, IsPinned, IsPublished, PublishedAt)
    SELECT Id, N'Thông báo', 'ALL', NULL, N'[DEMO] Thông báo lịch nghỉ lễ',
           N'Công ty thông báo lịch nghỉ và lịch trực. Đây là dữ liệu dùng để kiểm tra giao diện bài đăng.',
           'lich-nghi-demo.txt', 'text/plain', CONVERT(VARBINARY(MAX), N'Lịch nghỉ lễ - tệp kiểm thử'), 1, 1, '2026-09-15T07:30:00'
    FROM dbo.HrmUserAccount WHERE Username = 'hradmin';

IF NOT EXISTS (SELECT 1 FROM dbo.HrmCommunication WHERE Title = N'[DEMO] Chương trình đào tạo nội bộ')
    INSERT dbo.HrmCommunication(AuthorUserId, Category, ScopeCode, DepartmentId, Title, Body, IsPinned, IsPublished, PublishedAt)
    SELECT Id, N'Đào tạo', 'ALL', NULL, N'[DEMO] Chương trình đào tạo nội bộ',
           N'Mời nhân viên đăng ký các khóa kỹ năng và an toàn thông tin trong tháng 9.', 0, 1, '2026-09-14T09:00:00'
    FROM dbo.HrmUserAccount WHERE Username = 'hradmin';

IF NOT EXISTS (SELECT 1 FROM dbo.HrmCommunication WHERE Title = N'[DEMO] Bảo trì hệ thống IT')
    INSERT dbo.HrmCommunication(AuthorUserId, Category, ScopeCode, DepartmentId, Title, Body, IsPinned, IsPublished, PublishedAt)
    SELECT a.Id, N'Hệ thống', 'DEPARTMENT', d.Id, N'[DEMO] Bảo trì hệ thống IT',
           N'Hệ thống nội bộ bảo trì sau 18:00. Vui lòng lưu công việc trước khi kết thúc ngày.', 0, 1, '2026-09-13T16:00:00'
    FROM dbo.HrmUserAccount a CROSS JOIN dbo.HrmDepartment d
    WHERE a.Username = 'admin' AND d.Code = 'IT';
GO

-- Cross-module records with realistic states and dates.
DECLARE @DemoWork TABLE
(
    Kind NVARCHAR(20), Reference NVARCHAR(100), Title NVARCHAR(200), Description NVARCHAR(2000),
    Category NVARCHAR(100), Username NVARCHAR(80), DepartmentCode NVARCHAR(30), DueDate DATE,
    Target DECIMAL(19,4), Actual DECIMAL(19,4), Weight DECIMAL(5,2), Priority NVARCHAR(20),
    Status NVARCHAR(30), KpiType NVARCHAR(30), Quarter NVARCHAR(30), StartAt DATETIME2, EndAt DATETIME2,
    Location NVARCHAR(250), Destination NVARCHAR(250), CreatedByUsername NVARCHAR(80), CreatedAt DATETIME2,
    Keywords NVARCHAR(MAX)
);

INSERT @DemoWork VALUES
    ('kpi','DEMO-KPI-001',N'Tỷ lệ tuyển dụng đúng hạn',N'Hoàn thành nhu cầu tuyển dụng trong SLA.',N'%', 'demo01','HR','2026-09-30',100,85,25,'HIGH','WAITING_PROOF','ASSIGNED',N'Quý 3-2026',NULL,NULL,NULL,NULL,'hradmin','2026-07-01',NULL),
    ('kpi','DEMO-KPI-002',N'Độ ổn định hệ thống',N'Duy trì độ sẵn sàng của hệ thống nội bộ.',N'%', 'demo02','IT','2026-09-30',99.5,99.8,30,'HIGH','PROVEN','ASSIGNED',N'Quý 3-2026',NULL,NULL,NULL,NULL,'huongtm','2026-07-01',NULL),
    ('kpi','DEMO-KPI-003',N'Đối soát chứng từ đúng hạn',N'Hoàn tất chứng từ trước ngày khóa sổ.',N'%', 'demo03','ACC','2026-09-30',100,65,20,'NORMAL','NEEDS_REVISION','ASSIGNED',N'Quý 3-2026',NULL,NULL,NULL,NULL,'hradmin','2026-07-02',NULL),
    ('kpi','DEMO-KPI-004',N'Doanh số khách hàng mới',N'Giá trị hợp đồng khách hàng mới trong quý.',N'Triệu đồng','demo04','SALES','2026-09-30',800,620,25,'HIGH','WAITING_RESULT','ASSIGNED',N'Quý 3-2026',NULL,NULL,NULL,NULL,'thedt','2026-07-03',NULL),
    ('kpi','DEMO-KPI-005',N'Kế hoạch nâng cao năng lực phòng Nhân sự',N'KPI cấp phòng dùng để kiểm tra tab kế hoạch.',N'%',NULL,'HR','2026-12-31',100,40,100,'NORMAL','WAITING_RESULT','PLAN',N'Quý 4-2026',NULL,NULL,NULL,NULL,'thedt','2026-09-01',NULL),

    ('recruitment','DEMO-REC-001',N'Tuyển Kỹ sư phần mềm .NET',N'Mở rộng đội phát triển hệ thống HRM.',N'Toàn thời gian',NULL,'IT','2026-10-15',3,1,NULL,'HIGH','IN_PROGRESS',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-09-01',NULL),
    ('recruitment','DEMO-REC-002',N'Tuyển Chuyên viên kinh doanh',N'Phát triển khách hàng doanh nghiệp.',N'Toàn thời gian',NULL,'SALES','2026-10-20',5,2,NULL,'NORMAL','OPEN',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-09-02',NULL),
    ('recruitment','DEMO-REC-003',N'Tuyển Thực tập sinh Marketing',N'Hỗ trợ nội dung và sự kiện.',N'Thực tập',NULL,'MKT','2026-09-30',2,2,NULL,'LOW','COMPLETED',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-08-15',NULL),
    ('recruitment','DEMO-REC-004',N'Tuyển Kế toán tổng hợp',N'Bổ sung nhân sự phụ trách báo cáo tài chính.',N'Toàn thời gian',NULL,'ACC','2026-11-01',1,0,NULL,'HIGH','OPEN',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-09-10',NULL),

    ('training','DEMO-TRN-001',N'An toàn thông tin cơ bản',N'Nhận diện lừa đảo, quản lý mật khẩu và bảo vệ dữ liệu.',N'Online',NULL,'IT','2026-09-25',20,12,NULL,'NORMAL','IN_PROGRESS',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-09-01',N'[{"date":"2026-09-18","time":"09:00 - 11:00","title":"Nhận diện rủi ro"},{"date":"2026-09-25","time":"14:00 - 16:00","title":"Thực hành xử lý sự cố"}]'),
    ('training','DEMO-TRN-002',N'Kỹ năng quản lý nhóm',N'Đào tạo dành cho cấp quản lý và nhân sự kế thừa.',N'Offline',NULL,'HR','2026-10-10',15,8,NULL,'HIGH','PLANNED',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-09-05',N'[{"date":"2026-10-10","time":"08:30 - 16:30","title":"Quản lý hiệu suất"}]'),
    ('training','DEMO-TRN-003',N'Excel cho nghiệp vụ kế toán',N'Tổng hợp dữ liệu, PivotTable và báo cáo.',N'Offline',NULL,'ACC','2026-08-28',10,10,NULL,'NORMAL','COMPLETED',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-08-01',N'[{"date":"2026-08-28","time":"08:00 - 17:00","title":"Excel nâng cao"}]'),

    ('overtime','DEMO-OT-001',N'Tăng ca hoàn thiện báo cáo tháng',N'Hoàn tất báo cáo trước ngày khóa sổ.',N'Ca tối (trong tuần)','demo03','ACC','2026-09-15',3,4.5,1.5,'NORMAL','PENDING',NULL,NULL,NULL,NULL,NULL,NULL,'demo03','2026-09-14T15:10:00',NULL),
    ('overtime','DEMO-OT-002',N'Tăng ca triển khai hệ thống',N'Triển khai bản cập nhật ngoài giờ làm việc.',N'Ca tối (trong tuần)','demo02','IT','2026-09-16',4,6,1.5,'HIGH','APPROVED',NULL,NULL,NULL,NULL,NULL,NULL,'demo02','2026-09-13T14:20:00',NULL),
    ('overtime','DEMO-OT-003',N'Tăng ca sự kiện khách hàng',N'Hỗ trợ sự kiện cuối tuần.',N'Cuối tuần','demo05','MKT','2026-09-19',8,16,2,'NORMAL','PENDING',NULL,NULL,NULL,NULL,NULL,NULL,'demo05','2026-09-15T08:00:00',NULL),
    ('overtime','DEMO-OT-004',N'Tăng ca kiểm kê kho',N'Kiểm kê định kỳ tại kho Hà Nội.',N'Ca tối (trong tuần)','demo18','OPS','2026-09-12',3,4.5,1.5,'NORMAL','REJECTED',NULL,NULL,NULL,NULL,NULL,NULL,'demo18','2026-09-11T11:30:00',NULL),

    ('resignation','DEMO-RES-001',N'Đề nghị nghỉ việc - Nguyễn Minh Anh',N'Chuyển nơi cư trú và thay đổi kế hoạch cá nhân.',N'Theo nguyện vọng','demo01','HR','2026-10-31',NULL,NULL,NULL,'NORMAL','PENDING',NULL,NULL,NULL,NULL,NULL,NULL,'demo01','2026-09-15T09:00:00',NULL),
    ('resignation','DEMO-RES-002',N'Đề nghị nghỉ việc - Võ Ngọc Mai',N'Tìm kiếm định hướng nghề nghiệp mới.',N'Theo nguyện vọng','demo05','MKT','2026-10-15',NULL,NULL,NULL,'NORMAL','APPROVED',NULL,NULL,NULL,NULL,NULL,NULL,'demo05','2026-09-05T10:15:00',NULL),
    ('resignation','DEMO-RES-003',N'Hoàn tất thôi việc - Đặng Tuấn Kiệt',N'Đã hoàn tất bàn giao công việc và tài sản.',N'Hết hạn hợp đồng','demo06','OPS','2026-08-31',NULL,NULL,NULL,'NORMAL','RESOLVED',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-08-01',NULL),
    ('resignation','DEMO-RES-004',N'Đề nghị nghỉ việc - Trần Quốc Bảo',N'Đề nghị chưa đáp ứng thời gian báo trước.',N'Theo nguyện vọng','demo02','IT','2026-09-30',NULL,NULL,NULL,'NORMAL','REJECTED',NULL,NULL,NULL,NULL,NULL,NULL,'demo02','2026-09-01',NULL),

    ('transfer','DEMO-TRA-001',N'Điều chuyển Nguyễn Minh Anh sang Marketing',N'Bổ sung nguồn lực truyền thông tuyển dụng.',N'Luân chuyển','demo01','MKT','2026-10-01',NULL,NULL,NULL,'NORMAL','PENDING',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-09-12',NULL),
    ('transfer','DEMO-TRA-002',N'Điều chuyển Trần Quốc Bảo sang Vận hành',N'Hỗ trợ dự án số hóa vận hành.',N'Biệt phái','demo02','OPS','2026-09-20',NULL,NULL,NULL,'HIGH','MANAGER_APPROVED',NULL,NULL,NULL,NULL,NULL,NULL,'huongtm','2026-09-08',NULL),
    ('transfer','DEMO-TRA-003',N'Bổ nhiệm Bùi Khánh Linh',N'Bổ nhiệm phụ trách nhóm chính sách nhân sự.',N'Bổ nhiệm','demo07','HR','2026-09-01',NULL,NULL,NULL,'NORMAL','APPROVED',NULL,NULL,NULL,NULL,NULL,NULL,'thedt','2026-08-20',NULL),
    ('transfer','DEMO-TRA-004',N'Điều chuyển Phạm Hoàng Long',N'Yêu cầu điều chuyển chưa phù hợp kế hoạch nhân sự.',N'Luân chuyển','demo04','OPS','2026-10-01',NULL,NULL,NULL,'NORMAL','REJECTED',NULL,NULL,NULL,NULL,NULL,NULL,'hradmin','2026-09-03',NULL),

    ('assets','DEMO-ASSET-001',N'Laptop Dell Latitude 5450',N'Laptop văn phòng, RAM 16GB, SSD 512GB.',N'Laptop','demo02','IT','2026-12-31',28500000,NULL,NULL,'NORMAL','ALLOCATED',NULL,NULL,NULL,NULL,NULL,NULL,'admin','2026-08-01',NULL),
    ('assets','DEMO-ASSET-002',N'Màn hình Dell P2425H',N'Màn hình 24 inch Full HD.',N'Màn hình',NULL,'IT',NULL,5200000,NULL,NULL,'NORMAL','IN_STOCK',NULL,NULL,NULL,NULL,NULL,NULL,'admin','2026-08-02',NULL),
    ('assets','DEMO-ASSET-003',N'iPhone 15 128GB',N'Điện thoại phục vụ kinh doanh.',N'Điện thoại','demo04','SALES','2027-01-31',19500000,NULL,NULL,'NORMAL','ALLOCATED',NULL,NULL,NULL,NULL,NULL,NULL,'admin','2026-08-03',NULL),
    ('assets','DEMO-ASSET-004',N'Máy in HP LaserJet Pro',N'Máy in dùng chung phòng Kế toán.',N'Máy in',NULL,'ACC',NULL,8200000,NULL,NULL,'NORMAL','MAINTENANCE',NULL,NULL,NULL,NULL,NULL,NULL,'admin','2026-08-04',NULL),
    ('assets','DEMO-ASSET-005',N'Bộ phát Wi-Fi Aruba',N'Thiết bị mạng dự phòng.',N'Thiết bị mạng',NULL,'IT',NULL,12300000,NULL,NULL,'NORMAL','IN_STOCK',NULL,NULL,NULL,NULL,NULL,NULL,'admin','2026-08-05',NULL),
    ('assets','DEMO-ASSET-006',N'Máy chiếu Epson EB-E01',N'Máy chiếu phòng đào tạo.',N'Thiết bị phòng họp','demo13','HR','2027-03-31',14800000,NULL,NULL,'NORMAL','ALLOCATED',NULL,NULL,NULL,NULL,NULL,NULL,'admin','2026-08-06',NULL),

    ('helpdesk','DEMO-HD-001',N'Không truy cập được VPN',N'Ứng dụng VPN báo lỗi xác thực sau khi đổi mạng.',N'Mạng và kết nối','demo01','HR','2026-09-15',NULL,NULL,NULL,'URGENT','OPEN',NULL,NULL,NULL,NULL,NULL,NULL,'demo01','2026-09-15T08:15:00',NULL),
    ('helpdesk','DEMO-HD-002',N'Cài đặt phần mềm kế toán',N'Cần cài phần mềm kế toán trên máy tính mới.',N'Phần mềm','demo03','ACC','2026-09-16',NULL,NULL,NULL,'HIGH','IN_PROGRESS',NULL,NULL,NULL,NULL,NULL,NULL,'demo03','2026-09-14T10:30:00',NULL),
    ('helpdesk','DEMO-HD-003',N'Máy in không nhận lệnh',N'Đã kiểm tra giấy và kết nối nhưng máy chưa in.',N'Thiết bị','demo09','ACC','2026-09-14',NULL,NULL,NULL,'NORMAL','RESOLVED',NULL,NULL,NULL,NULL,NULL,NULL,'demo09','2026-09-13T09:00:00',NULL),
    ('helpdesk','DEMO-HD-004',N'Yêu cầu cấp tài khoản CRM',N'Cấp quyền CRM cho nhân viên kinh doanh mới.',N'Tài khoản và phân quyền','demo19','SALES','2026-09-18',NULL,NULL,NULL,'NORMAL','OPEN',NULL,NULL,NULL,NULL,NULL,NULL,'demo19','2026-09-15T08:40:00',NULL),

    ('payroll','DEMO-PAY-001',N'Nguyễn Minh Anh',N'Bảng lương tháng 09/2026.',N'Hà Nội','demo01','HR','2026-09-30',15000000,14125000,NULL,'NORMAL','DRAFT',NULL,'2026-09',NULL,NULL,NULL,NULL,'hradmin','2026-09-01',N'{"baseSalary":15000000,"kpiSalary":1500000,"salesSalary":0,"otSalary":0,"totalAllowance":730000,"bonus":0,"socialInsurance":1200000,"healthInsurance":225000,"unemploymentInsurance":150000,"personalIncomeTax":530000,"advance":1000000,"deduction":0,"branch":"Hà Nội","email":"demo01@nhigia.vn"}'),
    ('payroll','DEMO-PAY-002',N'Trần Quốc Bảo',N'Bảng lương tháng 09/2026.',N'Hà Nội','demo02','IT','2026-09-30',22000000,22040000,NULL,'NORMAL','PENDING_APPROVAL',NULL,'2026-09',NULL,NULL,NULL,NULL,'hradmin','2026-09-01',N'{"baseSalary":22000000,"kpiSalary":3000000,"salesSalary":0,"otSalary":1800000,"totalAllowance":1230000,"bonus":1000000,"socialInsurance":1760000,"healthInsurance":330000,"unemploymentInsurance":220000,"personalIncomeTax":2680000,"advance":2000000,"deduction":0,"branch":"Hà Nội","email":"demo02@nhigia.vn"}'),
    ('payroll','DEMO-PAY-003',N'Lê Thu Hà',N'Bảng lương tháng 09/2026.',N'Hà Nội','demo03','ACC','2026-09-30',16000000,15120000,NULL,'NORMAL','APPROVED',NULL,'2026-09',NULL,NULL,NULL,NULL,'hradmin','2026-09-01',N'{"baseSalary":16000000,"kpiSalary":1500000,"salesSalary":0,"otSalary":900000,"totalAllowance":730000,"bonus":0,"socialInsurance":1280000,"healthInsurance":240000,"unemploymentInsurance":160000,"personalIncomeTax":1330000,"advance":1000000,"deduction":0,"branch":"Hà Nội","email":"demo03@nhigia.vn"}'),
    ('payroll','DEMO-PAY-004',N'Phạm Hoàng Long',N'Bảng lương tháng 09/2026.',N'Hà Nội','demo04','SALES','2026-09-30',18000000,21370000,NULL,'NORMAL','PUBLISHED',NULL,'2026-09',NULL,NULL,NULL,NULL,'hradmin','2026-09-01',N'{"baseSalary":18000000,"kpiSalary":2500000,"salesSalary":4500000,"otSalary":0,"totalAllowance":1230000,"bonus":1000000,"socialInsurance":1440000,"healthInsurance":270000,"unemploymentInsurance":180000,"personalIncomeTax":2970000,"advance":1000000,"deduction":0,"branch":"Hà Nội","email":"demo04@nhigia.vn"}'),
    ('payroll','DEMO-PAY-005',N'Võ Ngọc Mai',N'Bảng lương tháng 09/2026.',N'Hà Nội','demo05','MKT','2026-09-30',14500000,13650000,NULL,'NORMAL','PAID',NULL,'2026-09',NULL,NULL,NULL,NULL,'hradmin','2026-09-01',N'{"baseSalary":14500000,"kpiSalary":1500000,"salesSalary":0,"otSalary":0,"totalAllowance":730000,"bonus":500000,"socialInsurance":1160000,"healthInsurance":217500,"unemploymentInsurance":145000,"personalIncomeTax":1057500,"advance":1000000,"deduction":0,"branch":"Hà Nội","email":"demo05@nhigia.vn"}'),
    ('payroll','DEMO-PAY-006',N'Đặng Tuấn Kiệt',N'Bảng lương tháng 09/2026 cần điều chỉnh.',N'Hà Nội','demo06','OPS','2026-09-30',17500000,16000000,NULL,'NORMAL','REJECTED',NULL,'2026-09',NULL,NULL,NULL,NULL,'hradmin','2026-09-01',N'{"baseSalary":17500000,"kpiSalary":1000000,"salesSalary":0,"otSalary":0,"totalAllowance":730000,"bonus":0,"socialInsurance":1400000,"healthInsurance":262500,"unemploymentInsurance":175000,"personalIncomeTax":1392500,"advance":0,"deduction":0,"branch":"Hà Nội","email":"demo06@nhigia.vn"}'),

    ('vehicle','DEMO-VEH-001',N'Đi khảo sát khách hàng Bắc Ninh',N'Đón đoàn dự án tại văn phòng.',N'Xe 7 chỗ',NULL,'SALES',NULL,4,NULL,NULL,'NORMAL','PENDING',NULL,NULL,'2026-09-18T07:30:00','2026-09-18T17:00:00',N'Văn phòng Nhị Gia',N'KCN Yên Phong, Bắc Ninh','demo04','2026-09-15T08:00:00',NULL),
    ('vehicle','DEMO-VEH-002',N'Đưa nhân viên đi đào tạo',N'Xe phục vụ khóa đào tạo nội bộ.',N'Xe 16 chỗ',NULL,'HR',NULL,10,NULL,NULL,'NORMAL','APPROVED',NULL,NULL,'2026-09-25T07:00:00','2026-09-25T18:00:00',N'Văn phòng Nhị Gia',N'Trung tâm đào tạo Hà Nội','hradmin','2026-09-10',NULL),
    ('meeting','DEMO-MTG-001',N'Họp kế hoạch quý IV',N'Rà soát mục tiêu và phân bổ nguồn lực.',NULL,NULL,'HR',NULL,6,NULL,NULL,'NORMAL','PENDING',NULL,NULL,'2026-09-17T09:00:00','2026-09-17T10:00:00',N'Phòng họp 1 · 8 người',NULL,'demo01','2026-09-15T08:30:00',NULL),
    ('meeting','DEMO-MTG-002',N'Họp triển khai HRM',N'Kiểm tra tiến độ triển khai các phân hệ.',NULL,NULL,'IT',NULL,8,NULL,NULL,'NORMAL','APPROVED',NULL,NULL,'2026-09-17T10:10:00','2026-09-17T11:10:00',N'Phòng họp 1 · 8 người',NULL,'huongtm','2026-09-14',NULL),
    ('business-trip','DEMO-TRIP-001',N'Công tác kiểm tra chi nhánh',N'Kiểm tra vận hành và hướng dẫn quy trình.',N'Ô tô','demo06','OPS',NULL,NULL,NULL,NULL,'NORMAL','PENDING',NULL,NULL,'2026-09-21T07:00:00','2026-09-23T18:00:00',N'Hà Nội',N'Đà Nẵng','hradmin','2026-09-12',NULL),
    ('business-trip','DEMO-TRIP-002',N'Công tác gặp khách hàng',N'Đàm phán hợp đồng dịch vụ năm 2027.',N'Máy bay','demo04','SALES',NULL,NULL,NULL,NULL,'HIGH','APPROVED',NULL,NULL,'2026-09-28T06:00:00','2026-09-30T20:00:00',N'Hà Nội',N'TP. Hồ Chí Minh','thedt','2026-09-10',NULL);

INSERT dbo.HrmWorkItem
    (Kind, Reference, Title, Description, Category, EmployeeId, DepartmentId, DueDate,
     Target, Actual, Weight, Priority, Status, KpiType, Quarter, StartAt, EndAt,
     Location, Destination, CreatedBy, CreatedAt, Keywords)
SELECT s.Kind, s.Reference, s.Title, s.Description, s.Category, employee.Id, department.Id, s.DueDate,
       s.Target, s.Actual, s.Weight, s.Priority, s.Status, s.KpiType, s.Quarter, s.StartAt, s.EndAt,
       s.Location, s.Destination, creator.Id, s.CreatedAt, s.Keywords
FROM @DemoWork s
LEFT JOIN dbo.HrmUserAccount employee ON employee.Username = s.Username
LEFT JOIN dbo.HrmDepartment department ON department.Code = s.DepartmentCode
INNER JOIN dbo.HrmUserAccount creator ON creator.Username = s.CreatedByUsername
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.HrmWorkItem existing
    WHERE existing.Kind = s.Kind AND existing.Reference = s.Reference
);
GO

-- Complete module-specific fields that do not fit the shared seed table.
UPDATE dbo.HrmWorkItem
SET WorkLocation = N'Hà Nội', JobLevel = N'Nhân viên', ExperienceRequired = N'2 năm',
    EducationRequired = N'Đại học', GenderRequirement = N'Không yêu cầu', AgeRange = N'22 - 35',
    SalaryRange = N'15 - 30 triệu', SkillRequirements = N'Chuyên môn phù hợp; giao tiếp và làm việc nhóm tốt.',
    Benefits = N'Đầy đủ bảo hiểm; thưởng hiệu suất; đào tạo định kỳ.',
    RecruitmentProcess = N'Sàng lọc hồ sơ → Phỏng vấn chuyên môn → Phỏng vấn quản lý → Nhận việc',
    RecruitmentReason = N'Bổ sung nhân sự theo kế hoạch năm 2026', StartDate = '2026-11-01',
    ContractType = N'Hợp đồng xác định thời hạn', ProbationPeriod = N'2 tháng',
    RecruitmentChannel = N'Website tuyển dụng và giới thiệu nội bộ', ContactName = N'Phòng Nhân sự',
    ContactEmail = 'tuyendung@nhigia.vn', ContactPhone = '02473009999',
    ContactAddress = N'Văn phòng Nhị Gia - Hà Nội'
WHERE Kind = 'recruitment' AND Reference LIKE 'DEMO-REC-%';

UPDATE dbo.HrmWorkItem
SET StartDate = DATEADD(DAY, -7, DueDate), WorkLocation = CASE Category WHEN N'Online' THEN N'Microsoft Teams' ELSE N'Phòng đào tạo tầng 3' END,
    ContactName = N'Giảng viên nội bộ', SkillRequirements = N'Hoàn thành bài kiểm tra cuối khóa.',
    Benefits = N'Cấp chứng nhận khi hoàn thành.'
WHERE Kind = 'training' AND Reference LIKE 'DEMO-TRN-%';

UPDATE dbo.HrmWorkItem
SET WorkLocation = N'Thiết bị văn phòng', JobLevel = N'Nhị Gia', ExperienceRequired = N'Model DEMO',
    EducationRequired = N'Nhà cung cấp thử nghiệm', SalaryRange = CASE Status WHEN 'MAINTENANCE' THEN N'Cần bảo trì' ELSE N'Tốt' END
WHERE Kind = 'assets' AND Reference LIKE 'DEMO-ASSET-%';

UPDATE dbo.HrmWorkItem
SET ProofNote = CASE Reference
    WHEN 'DEMO-KPI-001' THEN N'Đã hoàn thành 17/20 vị trí đúng thời hạn; kèm báo cáo tuyển dụng.'
    WHEN 'DEMO-KPI-002' THEN N'Uptime đo được 99,8% theo báo cáo giám sát.'
    WHEN 'DEMO-KPI-003' THEN N'Cần bổ sung chứng từ đối chiếu cho tháng 8.'
    ELSE ProofNote END,
    LastActionNote = CASE Reference
    WHEN 'DEMO-KPI-002' THEN N'Quản lý đã xác nhận kết quả.'
    WHEN 'DEMO-KPI-003' THEN N'Yêu cầu bổ sung bằng chứng.'
    ELSE LastActionNote END
WHERE Kind = 'kpi' AND Reference LIKE 'DEMO-KPI-%';

UPDATE dbo.HrmWorkItem
SET LastActionNote = CASE Status
    WHEN 'APPROVED' THEN N'Đã phê duyệt dữ liệu thử nghiệm.'
    WHEN 'REJECTED' THEN N'Từ chối để kiểm tra luồng chỉnh sửa và gửi lại.'
    WHEN 'RESOLVED' THEN N'Đã hoàn tất xử lý dữ liệu thử nghiệm.'
    ELSE LastActionNote END
WHERE Reference LIKE 'DEMO-%';
GO

-- Training enrollment and result/certificate scenarios.
DECLARE @DemoEnrollment TABLE
(
    TrainingReference NVARCHAR(100), Username NVARCHAR(80), Status NVARCHAR(30),
    ProgressPercent INT, Score DECIMAL(5,2), EvaluationResult NVARCHAR(100),
    EvaluationNote NVARCHAR(1000), CertificateNumber NVARCHAR(100), CertificateIssuedAt DATETIME2
);
INSERT @DemoEnrollment VALUES
    ('DEMO-TRN-001','demo01','STUDYING',60,NULL,N'Đang học',N'Đã hoàn thành buổi đầu tiên.',NULL,NULL),
    ('DEMO-TRN-001','demo02','COMPLETED',100,92,N'Đạt',N'Hoàn thành tốt bài kiểm tra.',N'DEMO-CERT-2026-001','2026-09-25'),
    ('DEMO-TRN-001','demo08','STUDYING',40,NULL,N'Đang học',N'Cần hoàn thành bài thực hành.',NULL,NULL),
    ('DEMO-TRN-002','demo07','ENROLLED',0,NULL,N'Chưa bắt đầu',NULL,NULL,NULL),
    ('DEMO-TRN-002','demo13','ENROLLED',0,NULL,N'Chưa bắt đầu',NULL,NULL,NULL),
    ('DEMO-TRN-003','demo03','COMPLETED',100,88,N'Đạt',N'Vận dụng tốt PivotTable.',N'DEMO-CERT-2026-002','2026-08-28'),
    ('DEMO-TRN-003','demo09','COMPLETED',100,76,N'Đạt',N'Hoàn thành đầy đủ nội dung.',N'DEMO-CERT-2026-003','2026-08-28'),
    ('DEMO-TRN-003','demo15','FAILED',100,42,N'Chưa đạt',N'Cần tham gia lớp bổ sung.',NULL,NULL);

INSERT dbo.HrmTrainingEnrollment
    (TrainingId, EmployeeId, Status, ProgressPercent, Score, EvaluationResult, EvaluationNote,
     CertificateNumber, CertificateIssuedAt, EnrolledAt)
SELECT training.Id, employee.Id, e.Status, e.ProgressPercent, e.Score, e.EvaluationResult,
       e.EvaluationNote, e.CertificateNumber, e.CertificateIssuedAt, '2026-09-01'
FROM @DemoEnrollment e
INNER JOIN dbo.HrmWorkItem training ON training.Kind = 'training' AND training.Reference = e.TrainingReference
INNER JOIN dbo.HrmUserAccount employee ON employee.Username = e.Username
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.HrmTrainingEnrollment existing
    WHERE existing.TrainingId = training.Id AND existing.EmployeeId = employee.Id
);
GO

-- Participants for vehicle and meeting-room notification tests.
DECLARE @DemoParticipants TABLE (WorkReference NVARCHAR(100), Username NVARCHAR(80));
INSERT @DemoParticipants VALUES
    ('DEMO-VEH-001','demo04'),('DEMO-VEH-001','demo10'),('DEMO-VEH-001','demo16'),('DEMO-VEH-001','demo19'),
    ('DEMO-VEH-002','demo01'),('DEMO-VEH-002','demo02'),('DEMO-VEH-002','demo03'),('DEMO-VEH-002','demo05'),
    ('DEMO-VEH-002','demo07'),('DEMO-VEH-002','demo08'),('DEMO-VEH-002','demo09'),('DEMO-VEH-002','demo11'),
    ('DEMO-VEH-002','demo13'),('DEMO-VEH-002','demo15'),
    ('DEMO-MTG-001','demo01'),('DEMO-MTG-001','demo03'),('DEMO-MTG-001','demo05'),
    ('DEMO-MTG-001','demo07'),('DEMO-MTG-001','demo09'),('DEMO-MTG-001','demo13'),
    ('DEMO-MTG-002','demo02'),('DEMO-MTG-002','demo04'),('DEMO-MTG-002','demo06'),('DEMO-MTG-002','demo08'),
    ('DEMO-MTG-002','demo10'),('DEMO-MTG-002','demo12'),('DEMO-MTG-002','demo14'),('DEMO-MTG-002','demo20');

INSERT dbo.HrmWorkItemParticipant(WorkItemId, UserId)
SELECT w.Id, u.Id
FROM @DemoParticipants p
INNER JOIN dbo.HrmWorkItem w ON w.Reference = p.WorkReference
INNER JOIN dbo.HrmUserAccount u ON u.Username = p.Username
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.HrmWorkItemParticipant existing
    WHERE existing.WorkItemId = w.Id AND existing.UserId = u.Id
);
GO

-- Notifications for unread/read list behavior.
DECLARE @DemoNotifications TABLE
(
    Username NVARCHAR(80), Title NVARCHAR(200), Message NVARCHAR(1000), LinkUrl NVARCHAR(500), IsRead BIT
);
INSERT @DemoNotifications VALUES
    ('demo01',N'[DEMO] Lịch họp mới',N'Bạn được thêm vào cuộc họp Kế hoạch quý IV.',N'/Work?kind=meeting',0),
    ('demo02',N'[DEMO] KPI đã được xác nhận',N'Kết quả KPI Độ ổn định hệ thống đã được quản lý xác nhận.',N'/Work?kind=kpi',0),
    ('demo03',N'[DEMO] Đơn tăng ca chờ duyệt',N'Đơn tăng ca hoàn thiện báo cáo tháng đang chờ xử lý.',N'/Work?kind=overtime',1),
    ('demo04',N'[DEMO] Lịch công tác đã duyệt',N'Lịch công tác gặp khách hàng đã được phê duyệt.',N'/Work?kind=business-trip',0),
    ('demo07',N'[DEMO] Được ghi danh khóa đào tạo',N'Bạn đã được ghi danh khóa Kỹ năng quản lý nhóm.',N'/Work?kind=training',0);

INSERT dbo.HrmNotification(UserId, Title, Message, LinkUrl, IsRead, CreatedAt)
SELECT u.Id, n.Title, n.Message, n.LinkUrl, n.IsRead, '2026-09-15T08:00:00'
FROM @DemoNotifications n
INNER JOIN dbo.HrmUserAccount u ON u.Username = n.Username
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.HrmNotification existing
    WHERE existing.UserId = u.Id AND existing.Title = n.Title
);
GO
