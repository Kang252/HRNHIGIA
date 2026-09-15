-- 40 tai san thu nghiem day du. Script co the chay lai an toan theo ma tai san.
DECLARE @AssetCreatorId INT =
(
    SELECT TOP (1) Id
    FROM dbo.HrmUserAccount
    WHERE RoleCode = 'ADMIN'
    ORDER BY Id
);

IF @AssetCreatorId IS NULL
    SELECT TOP (1) @AssetCreatorId = Id FROM dbo.HrmUserAccount ORDER BY Id;

DECLARE @AssetSeed TABLE
(
    Reference NVARCHAR(100) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    AssetGroup NVARCHAR(250) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Brand NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    Supplier NVARCHAR(100) NOT NULL,
    AssetStatus NVARCHAR(30) NOT NULL,
    AssetCondition NVARCHAR(100) NOT NULL,
    PurchasePrice DECIMAL(19,4) NOT NULL,
    PurchaseDate DATE NOT NULL,
    WarrantyUntil DATE NULL,
    Username NVARCHAR(80) NULL,
    DepartmentCode NVARCHAR(30) NULL,
    Description NVARCHAR(2000) NOT NULL
);

INSERT @AssetSeed VALUES
('TS-TEST-101',N'Laptop Dell Latitude 5450',N'Thiết bị văn phòng',N'Laptop',N'Dell',N'Latitude 5450',N'FPT Synnex','ALLOCATED','NORMAL',28500000,'2026-01-08','2029-01-08','demo02','IT',N'Laptop Core Ultra 5, RAM 16GB, SSD 512GB cấp cho nhân viên công nghệ.'),
('TS-TEST-102',N'Laptop Lenovo ThinkPad T14 Gen 5',N'Thiết bị văn phòng',N'Laptop',N'Lenovo',N'ThinkPad T14 Gen 5',N'Petrosetco','IN_STOCK','NORMAL',31900000,'2026-01-12','2029-01-12',NULL,'IT',N'Laptop doanh nghiệp dự phòng cho nhân viên mới, Windows 11 Pro.'),
('TS-TEST-103',N'Laptop HP ProBook 440 G11',N'Thiết bị văn phòng',N'Laptop',N'HP',N'ProBook 440 G11',N'Digiworld','ALLOCATED','NORMAL',24400000,'2026-01-15','2029-01-15','demo03','ACC',N'Laptop phục vụ nghiệp vụ kế toán và lập báo cáo tài chính.'),
('TS-TEST-104',N'Apple MacBook Air M3 15 inch',N'Thiết bị văn phòng',N'Laptop',N'Apple',N'MacBook Air M3 15',N'ShopDunk Business','ALLOCATED','NORMAL',34990000,'2026-01-18','2027-01-18','demo05','MKT',N'Máy tính thiết kế nội dung và vận hành chiến dịch truyền thông.'),
('TS-TEST-105',N'Màn hình Dell P2425H 24 inch',N'Thiết bị văn phòng',N'Máy tính để bàn',N'Dell',N'P2425H',N'Phong Vũ','ALLOCATED','NORMAL',5250000,'2026-01-20','2029-01-20','demo08','IT',N'Màn hình IPS Full HD có cổng USB-C phục vụ phát triển phần mềm.'),
('TS-TEST-106',N'Màn hình LG 27UP850N 27 inch 4K',N'Thiết bị văn phòng',N'Máy tính để bàn',N'LG',N'27UP850N-W',N'An Phát Computer','IN_STOCK','NORMAL',8990000,'2026-01-22','2028-01-22',NULL,'IT',N'Màn hình 4K dự phòng cho nhóm thiết kế và công nghệ.'),
('TS-TEST-107',N'Máy tính Dell OptiPlex 7020 Tower',N'Thiết bị văn phòng',N'Máy tính để bàn',N'Dell',N'OptiPlex 7020',N'FPT Synnex','ALLOCATED','NORMAL',22900000,'2026-01-25','2029-01-25','demo12','SALES',N'Máy tính để bàn phục vụ quản lý khách hàng và báo giá.'),
('TS-TEST-108',N'Mini PC Intel NUC 13 Pro',N'Thiết bị văn phòng',N'Máy tính để bàn',N'Intel',N'NUC13ANHi5',N'Vĩnh Xuân','IN_STOCK','NORMAL',16900000,'2026-01-27','2029-01-27',NULL,'IT',N'Máy tính mini dự phòng cho quầy lễ tân và phòng họp.'),
('TS-TEST-109',N'Máy in HP LaserJet Pro 4003dn',N'Thiết bị văn phòng',N'Máy in',N'HP',N'LaserJet Pro 4003dn',N'Phong Vũ','ALLOCATED','NORMAL',7850000,'2026-02-01','2027-02-01','demo09','ACC',N'Máy in hai mặt tốc độ cao dùng cho phòng kế toán.'),
('TS-TEST-110',N'Máy quét Brother ADS-4700W',N'Thiết bị văn phòng',N'Máy scan tài liệu',N'Brother',N'ADS-4700W',N'Nguyễn Kim Business','MAINTENANCE','BROKEN',17600000,'2025-09-10','2027-09-10',NULL,'HR',N'Máy quét hồ sơ hai mặt đang bảo trì bộ cuốn giấy.'),
('TS-TEST-111',N'Máy chiếu Epson EB-FH06',N'Thiết bị ghi hình & Hội nghị',N'Máy chiếu & Màn chiếu',N'Epson',N'EB-FH06',N'Thế Giới Máy Chiếu','ALLOCATED','NORMAL',20900000,'2026-02-03','2028-02-03','demo13','HR',N'Máy chiếu Full HD phục vụ đào tạo và hội thảo nội bộ.'),
('TS-TEST-112',N'Camera hội nghị Logitech Rally Bar Mini',N'Thiết bị ghi hình & Hội nghị',N'Thiết bị họp trực tuyến',N'Logitech',N'Rally Bar Mini',N'CMC Telecom','ALLOCATED','NORMAL',48900000,'2026-02-05','2028-02-05','demo16','SALES',N'Thiết bị hội nghị tích hợp cho phòng họp kinh doanh.'),
('TS-TEST-113',N'Điểm truy cập Aruba Instant On AP25',N'Thiết bị mạng & Tin học',N'Bộ phát Wifi',N'Aruba',N'Instant On AP25',N'FPT Synnex','ALLOCATED','NORMAL',6790000,'2026-02-06','2029-02-06','demo02','IT',N'Điểm truy cập Wi-Fi 6 lắp tại khu vực làm việc tầng hai.'),
('TS-TEST-114',N'Switch Cisco CBS350 24 cổng',N'Thiết bị mạng & Tin học',N'Thiết bị mạng & Switch',N'Cisco',N'CBS350-24T-4G',N'VietNet','ALLOCATED','NORMAL',12800000,'2026-02-07','2031-02-07','demo10','IT',N'Switch quản trị mạng nội bộ 24 cổng Gigabit.'),
('TS-TEST-115',N'Bộ lưu điện APC Smart UPS 1500VA',N'Thiết bị mạng & Tin học',N'Máy chủ Server & Lưu điện',N'APC',N'SMT1500IC',N'Schneider Electric','ALLOCATED','NORMAL',18300000,'2026-02-08','2028-02-08','demo02','IT',N'Bộ lưu điện bảo vệ máy chủ và thiết bị mạng trung tâm.'),
('TS-TEST-116',N'NAS Synology DiskStation DS923+',N'Thiết bị mạng & Tin học',N'Máy chủ Server & Lưu điện',N'Synology',N'DS923+',N'Ánh Minh Cường','ALLOCATED','NORMAL',18900000,'2026-02-09','2029-02-09','demo20','IT',N'Thiết bị lưu trữ sao lưu tài liệu dùng chung của công ty.'),
('TS-TEST-117',N'Điện thoại Apple iPhone 15 128GB',N'Thiết bị di động',N'Điện thoại',N'Apple',N'iPhone 15',N'FPT Shop Business','ALLOCATED','NORMAL',19990000,'2026-02-10','2027-02-10','demo04','SALES',N'Điện thoại phục vụ liên hệ khách hàng và công tác.'),
('TS-TEST-118',N'Điện thoại Samsung Galaxy S24 FE',N'Thiết bị di động',N'Điện thoại',N'Samsung',N'Galaxy S24 FE',N'Samsung Business','IN_STOCK','NORMAL',14990000,'2026-02-11','2027-02-11',NULL,'SALES',N'Điện thoại dự phòng cho bộ phận kinh doanh.'),
('TS-TEST-119',N'Máy tính bảng Apple iPad Air M2',N'Thiết bị di động',N'Máy tính bảng',N'Apple',N'iPad Air M2 11',N'ShopDunk Business','ALLOCATED','NORMAL',16990000,'2026-02-12','2027-02-12','demo15','MKT',N'Máy tính bảng trình bày thiết kế và nội dung truyền thông.'),
('TS-TEST-120',N'Bộ đàm Motorola XiR P3688',N'Thiết bị di động',N'Bộ đàm',N'Motorola',N'XiR P3688',N'Viễn Thông Nhật Minh','ALLOCATED','NORMAL',6200000,'2026-02-13','2028-02-13','demo18','OPS',N'Bộ đàm điều phối nhân sự vận hành tại kho.'),
('TS-TEST-121',N'Bàn làm việc Hòa Phát HR120SC1',N'Nội thất văn phòng',N'Bàn làm việc',N'Hòa Phát',N'HR120SC1',N'Nội Thất Hòa Phát','ALLOCATED','NORMAL',1750000,'2026-02-14','2027-02-14','demo07','HR',N'Bàn làm việc chân sắt mặt gỗ Melamine cho nhân viên.'),
('TS-TEST-122',N'Ghế công thái học Hòa Phát GLE08',N'Nội thất văn phòng',N'Ghế xoay văn phòng',N'Hòa Phát',N'GLE08',N'Nội Thất Hòa Phát','ALLOCATED','NORMAL',3850000,'2026-02-15','2027-02-15','demo01','HR',N'Ghế lưới công thái học có tựa đầu và hỗ trợ thắt lưng.'),
('TS-TEST-123',N'Tủ hồ sơ sắt Hòa Phát TU09K3',N'Nội thất văn phòng',N'Tủ tài liệu & Locker',N'Hòa Phát',N'TU09K3',N'Nội Thất Hòa Phát','ALLOCATED','NORMAL',3550000,'2026-02-16','2027-02-16','demo03','ACC',N'Tủ hồ sơ bốn cánh có khóa cho chứng từ kế toán.'),
('TS-TEST-124',N'Bàn họp Hòa Phát CT2412H1',N'Nội thất văn phòng',N'Bàn phòng họp',N'Hòa Phát',N'CT2412H1',N'Nội Thất Miền Nam','ALLOCATED','NORMAL',4950000,'2026-02-17','2027-02-17','thedt','BOD',N'Bàn họp 10 chỗ đặt tại phòng họp ban giám đốc.'),
('TS-TEST-125',N'Smart TV Samsung 65 inch 4K',N'Thiết bị ghi hình & Hội nghị',N'Tivi & Màn hình hiển thị',N'Samsung',N'UA65DU8000',N'Điện Máy Xanh B2B','ALLOCATED','NORMAL',15490000,'2026-02-18','2028-02-18','thedt','BOD',N'Màn hình trình chiếu báo cáo tại phòng họp lớn.'),
('TS-TEST-126',N'Máy lạnh Daikin Inverter 2 HP',N'Thiết bị tiện ích & Pantry',N'Máy lạnh',N'Daikin',N'FTKB50YVMV',N'Điện Máy Chợ Lớn','ALLOCATED','NORMAL',17490000,'2026-02-19','2027-02-19','demo18','OPS',N'Máy lạnh lắp tại khu vực điều hành vận hành.'),
('TS-TEST-127',N'Tủ lạnh Panasonic Inverter 234 lít',N'Thiết bị tiện ích & Pantry',N'Tủ lạnh & Thiết bị nhà bếp',N'Panasonic',N'NR-TV261BPKV',N'Nguyễn Kim Business','ALLOCATED','NORMAL',8290000,'2026-02-20','2028-02-20','demo18','OPS',N'Tủ lạnh dùng chung tại khu vực pantry nhân viên.'),
('TS-TEST-128',N'Lò vi sóng Sharp R-G222VN-S',N'Thiết bị tiện ích & Pantry',N'Tủ lạnh & Thiết bị nhà bếp',N'Sharp',N'R-G222VN-S',N'Điện Máy Xanh B2B','IN_STOCK','NORMAL',2190000,'2026-02-21','2027-02-21',NULL,'OPS',N'Lò vi sóng có nướng dự phòng cho pantry tầng ba.'),
('TS-TEST-129',N'Máy lọc nước Karofi KAQ-U95',N'Thiết bị tiện ích & Pantry',N'Máy lọc nước & Cây nước',N'Karofi',N'KAQ-U95',N'Karofi Business','ALLOCATED','NORMAL',10490000,'2026-02-22','2028-02-22','demo07','HR',N'Máy lọc nước RO lắp tại khu vực làm việc nhân sự.'),
('TS-TEST-130',N'Máy pha cà phê DeLonghi Magnifica S',N'Thiết bị tiện ích & Pantry',N'Tủ lạnh & Thiết bị nhà bếp',N'DeLonghi',N'ECAM22.110.B',N'Quang Hạnh Pro','ALLOCATED','NORMAL',18900000,'2026-02-23','2028-02-23','thedt','BOD',N'Máy pha cà phê tự động tại khu tiếp khách ban giám đốc.'),
('TS-TEST-131',N'Bình chữa cháy CO2 Dragon MT3',N'Thiết bị an toàn',N'Thiết bị an toàn & PCCC',N'Dragon',N'MT3',N'PCCC An Phát','IN_STOCK','NORMAL',490000,'2026-02-24','2027-02-24',NULL,'OPS',N'Bình chữa cháy CO2 dự phòng cho khu vực thiết bị điện.'),
('TS-TEST-132',N'Tủ thuốc sơ cấp cứu văn phòng',N'Thiết bị an toàn',N'Thiết bị an toàn & PCCC',N'Việt Nhật',N'First Aid FA-02',N'Thiết Bị Y Tế Hà Nội','ALLOCATED','NORMAL',950000,'2026-02-25','2027-02-25','demo07','HR',N'Tủ thuốc sơ cấp cứu có danh mục vật tư theo quy định.'),
('TS-TEST-133',N'Máy hủy tài liệu Silicon PS-1000C',N'Thiết bị văn phòng',N'Máy hủy tài liệu',N'Silicon',N'PS-1000C',N'Thời Đại Mới','MAINTENANCE','BROKEN',3290000,'2025-11-15','2026-11-15','demo03','ACC',N'Máy hủy tài liệu đang thay dao cắt và bảo dưỡng định kỳ.'),
('TS-TEST-134',N'Máy chấm công khuôn mặt ZKTeco SpeedFace V5L',N'Thiết bị an toàn',N'Máy chấm công',N'ZKTeco',N'SpeedFace V5L',N'Ronald Jack Việt Nam','ALLOCATED','NORMAL',11900000,'2026-02-26','2028-02-26','demo07','HR',N'Máy chấm công nhận diện khuôn mặt lắp tại cửa phụ.'),
('TS-TEST-135',N'Camera IP Hikvision 4MP',N'Thiết bị ghi hình & Hội nghị',N'Camera',N'Hikvision',N'DS-2CD2143G2-I',N'Phúc Anh Smart','ALLOCATED','NORMAL',2190000,'2026-02-27','2028-02-27','demo18','OPS',N'Camera giám sát lối vào kho và khu vực giao nhận.'),
('TS-TEST-136',N'Đầu ghi Hikvision 16 kênh 4K',N'Thiết bị ghi hình & Hội nghị',N'Camera',N'Hikvision',N'DS-7616NXI-K2',N'Phúc Anh Smart','ALLOCATED','NORMAL',5790000,'2026-02-28','2028-02-28','demo02','IT',N'Đầu ghi hình camera có ổ cứng lưu trữ 30 ngày.'),
('TS-TEST-137',N'Ổ cứng SSD di động Samsung T7 2TB',N'Thiết bị mạng & Tin học',N'Thiết bị lưu trữ',N'Samsung',N'Portable SSD T7 2TB',N'An Phát Computer','IN_STOCK','NORMAL',4290000,'2026-03-01','2029-03-01',NULL,'IT',N'Ổ cứng di động mã hóa dùng sao lưu dữ liệu dự án.'),
('TS-TEST-138',N'Tai nghe Jabra Evolve2 65 Flex',N'Thiết bị văn phòng',N'Tai nghe',N'Jabra',N'Evolve2 65 Flex',N'GTC Telecom','ALLOCATED','NORMAL',7990000,'2026-03-02','2028-03-02','demo11','MKT',N'Tai nghe chống ồn phục vụ họp trực tuyến và sản xuất nội dung.'),
('TS-TEST-139',N'Máy in tem Zebra ZD421',N'Thiết bị văn phòng',N'Máy in',N'Zebra',N'ZD421',N'Tân Phát Barcode','ALLOCATED','NORMAL',10900000,'2026-03-03','2028-03-03','demo18','OPS',N'Máy in tem mã vạch dùng quản lý hàng hóa tại kho.'),
('TS-TEST-140',N'Máy quét mã vạch Honeywell 1470G',N'Thiết bị văn phòng',N'Máy quét mã vạch',N'Honeywell',N'Voyager XP 1470G',N'Tân Phát Barcode','RECOVERING','LOSS',3450000,'2025-12-12','2027-12-12','demo18','OPS',N'Thiết bị đang thực hiện thủ tục thu hồi để kiểm tra thất lạc phụ kiện.');

INSERT dbo.HrmWorkItem
(
    Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired,
    EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId,
    DepartmentId, Description, Priority, CreatedBy, CreatedAt
)
SELECT
    'assets', seed.Reference, seed.Title, seed.AssetGroup, seed.Category, seed.Brand,
    seed.Model, seed.Supplier, seed.AssetStatus, seed.AssetCondition, seed.PurchasePrice,
    seed.PurchaseDate, seed.WarrantyUntil, employee.Id, department.Id, seed.Description,
    'NORMAL', @AssetCreatorId, seed.PurchaseDate
FROM @AssetSeed seed
LEFT JOIN dbo.HrmUserAccount employee ON employee.Username = seed.Username
LEFT JOIN dbo.HrmDepartment department ON department.Code = seed.DepartmentCode
WHERE @AssetCreatorId IS NOT NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.HrmWorkItem existing
      WHERE existing.Kind = 'assets' AND existing.Reference = seed.Reference
  );
GO
