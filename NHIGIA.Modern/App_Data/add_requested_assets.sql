-- Thêm các tài sản mới: Quạt, Máy lạnh, Bàn, Ghế
USE [DEV_NHIGIA];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @AdminId INT;
SELECT TOP 1 @AdminId = Id FROM dbo.HrmUserAccount WHERE RoleCode IN ('ADMIN', 'HR');
IF @AdminId IS NULL SET @AdminId = 1;

DECLARE @QltsDeptId INT;
SELECT TOP 1 @QltsDeptId = Id FROM dbo.HrmDepartment WHERE Code = 'QLTS' OR Name LIKE N'%Tài Sản%';
IF @QltsDeptId IS NULL SET @QltsDeptId = 4;

-- 1. Quạt cây đứng văn phòng Senko DCN1806
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem WHERE Kind = 'assets' AND Reference = 'TS-VP-003')
BEGIN
    INSERT INTO dbo.HrmWorkItem
        (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
    VALUES
        ('assets', 'TS-VP-003', N'Quạt cây đứng văn phòng Senko DCN1806', N'Thiết bị văn phòng', N'Quạt máy văn phòng', N'Senko', N'DCN1806', N'Điện Máy Xanh B2B', 'IN_STOCK', 'NORMAL', 480000, '2026-02-10', NULL, NULL, @QltsDeptId, N'Quạt đứng 5 cánh sải cánh 45cm, công suất 65W, 3 tốc độ gió chuyển hướng cơ, lưu kho sẵn sàng cấp phát.', 'NORMAL', @AdminId, '2026-02-10');
END

-- 2. Quạt đứng cao cấp Mitsubishi Tatami LV16-RA
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem WHERE Kind = 'assets' AND Reference = 'TS-VP-004')
BEGIN
    INSERT INTO dbo.HrmWorkItem
        (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
    VALUES
        ('assets', 'TS-VP-004', N'Quạt đứng cao cấp Mitsubishi Tatami LV16-RA', N'Thiết bị văn phòng', N'Quạt máy văn phòng', N'Mitsubishi Electric', N'Tatami LV16-RA CY-GY', N'Điện Máy Chợ Lớn Pro', 'IN_STOCK', 'NORMAL', 1890000, '2026-02-15', NULL, NULL, @QltsDeptId, N'Quạt đứng điều khiển từ xa, động cơ bạc đạn kín chống bụi, hẹn giờ tắt mở thông minh, mới 100% trong kho.', 'NORMAL', @AdminId, '2026-02-15');
END

-- 3. Máy lạnh Daikin Inverter 2.0 HP FTKB50WAVMV
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem WHERE Kind = 'assets' AND Reference = 'TS-VP-005')
BEGIN
    INSERT INTO dbo.HrmWorkItem
        (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
    VALUES
        ('assets', 'TS-VP-005', N'Máy lạnh Daikin Inverter 2.0 HP FTKB50WAVMV', N'Thiết bị văn phòng', N'Máy lạnh / Điều hòa', N'Daikin', N'FTKB50WAVMV Inverter', N'Điện Máy Xanh B2B', 'IN_STOCK', 'NORMAL', 16490000, '2026-01-20', NULL, NULL, @QltsDeptId, N'Máy lạnh 1 chiều Inverter tiết kiệm điện 2.0 HP (18.100 BTU), phin lọc Enzyme Blue chống ẩm mốc.', 'NORMAL', @AdminId, '2026-01-20');
END

-- 4. Máy lạnh Panasonic Inverter 1.5 HP CU/CS-XPU12XKH-8
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem WHERE Kind = 'assets' AND Reference = 'TS-VP-006')
BEGIN
    INSERT INTO dbo.HrmWorkItem
        (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
    VALUES
        ('assets', 'TS-VP-006', N'Máy lạnh Panasonic Inverter 1.5 HP CU/CS-XPU12XKH-8', N'Thiết bị văn phòng', N'Máy lạnh / Điều hòa', N'Panasonic', N'CU/CS-XPU12XKH-8', N'Điện Máy Chợ Lớn Pro', 'IN_STOCK', 'NORMAL', 11850000, '2026-02-05', NULL, NULL, @QltsDeptId, N'Máy lạnh công nghệ Nanoe-X lọc khuẩn khử mùi, làm lạnh nhanh P-Tech, công nghệ ECO tích hợp AI.', 'NORMAL', @AdminId, '2026-02-05');
END

-- 5. Bàn làm việc nhân viên chân sắt Hòa Phát HR120SC1
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem WHERE Kind = 'assets' AND Reference = 'TS-VP-007')
BEGIN
    INSERT INTO dbo.HrmWorkItem
        (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
    VALUES
        ('assets', 'TS-VP-007', N'Bàn làm việc nhân viên chân sắt Hòa Phát HR120SC1', N'Thiết bị văn phòng', N'Bàn làm việc', N'Nội Thất Hòa Phát', N'HR120SC1 1m2', N'Nội Thất Văn Phòng Miền Nam', 'IN_STOCK', 'NORMAL', 1650000, '2026-02-01', NULL, NULL, @QltsDeptId, N'Bàn làm việc khung thép sơn tĩnh điện, mặt bàn gỗ Melamine cao cấp chống trầy, kích thước W1200 x D600 x H750mm.', 'NORMAL', @AdminId, '2026-02-01');
END

-- 6. Bàn họp lớn 10 chỗ mặt gỗ sơn PU Hòa Phát CT2412H1
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem WHERE Kind = 'assets' AND Reference = 'TS-VP-008')
BEGIN
    INSERT INTO dbo.HrmWorkItem
        (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
    VALUES
        ('assets', 'TS-VP-008', N'Bàn họp lớn 10 chỗ mặt gỗ sơn PU Hòa Phát CT2412H1', N'Thiết bị văn phòng', N'Bàn làm việc', N'Nội Thất Hòa Phát', N'CT2412H1 2m4', N'Nội Thất Văn Phòng Miền Nam', 'IN_STOCK', 'NORMAL', 4850000, '2026-02-12', NULL, NULL, @QltsDeptId, N'Bàn họp văn phòng cao cấp mặt gỗ công nghiệp phủ sơn PU, kích thước W2400 x D1200 x H760mm.', 'NORMAL', @AdminId, '2026-02-12');
END

-- 7. Ghế xoay lưới công thái học Ergonomic Hòa Phát GL309
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem WHERE Kind = 'assets' AND Reference = 'TS-VP-009')
BEGIN
    INSERT INTO dbo.HrmWorkItem
        (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
    VALUES
        ('assets', 'TS-VP-009', N'Ghế xoay lưới công thái học Ergonomic Hòa Phát GL309', N'Thiết bị văn phòng', N'Ghế xoay văn phòng', N'Nội Thất Hòa Phát', N'GL309 Ergonomic', N'Nội Thất Văn Phòng Miền Nam', 'IN_STOCK', 'NORMAL', 1450000, '2026-02-01', NULL, NULL, @QltsDeptId, N'Ghế lưới văn phòng cao cấp có tựa đầu, ngả lưng điều chỉnh nhiều góc độ, đệm bọc mút định hình êm ái.', 'NORMAL', @AdminId, '2026-02-01');
END

-- 8. Ghế xoay da lãnh đạo cao cấp Hòa Phát SG920
IF NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem WHERE Kind = 'assets' AND Reference = 'TS-VP-010')
BEGIN
    INSERT INTO dbo.HrmWorkItem
        (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
    VALUES
        ('assets', 'TS-VP-010', N'Ghế xoay da lãnh đạo cao cấp Hòa Phát SG920', N'Thiết bị văn phòng', N'Ghế xoay văn phòng', N'Nội Thất Hòa Phát', N'SG920 Lãnh đạo', N'Nội Thất Văn Phòng Miền Nam', 'IN_STOCK', 'NORMAL', 2950000, '2026-02-15', NULL, NULL, @QltsDeptId, N'Ghế da văn phòng chân nhôm đúc sáng bóng, tay ghế ốp gỗ, đệm và tựa bọc da công nghiệp cao cấp may trang trí.', 'NORMAL', @AdminId, '2026-02-15');
END
GO

