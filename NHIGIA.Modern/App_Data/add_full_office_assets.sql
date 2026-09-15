-- KHO TÀI SẢN VĂN PHÒNG TOÀN DIỆN CHO HRM NHỊ GIA
USE [DEV_NHIGIA];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @AdminId INT = 1;
DECLARE @HrId INT = 2;
DECLARE @DirId INT = 3;
DECLARE @MgrId INT = 4;
DECLARE @EmpA INT = 5;
DECLARE @EmpNhat INT = 6;
DECLARE @EmpSon INT = 7;

DECLARE @DeptBOD INT = 1;
DECLARE @DeptHR INT = 2;
DECLARE @DeptIT INT = 3;
DECLARE @DeptQLTS INT = 4;
DECLARE @DeptBHCU INT = 6;

-- BẢNG TẠM CHỨA DANH MỤC TÀI SẢN VĂN PHÒNG MỚI
CREATE TABLE #NewAssets (
    Ref NVARCHAR(50),
    Title NVARCHAR(250),
    Grp NVARCHAR(250),
    Cat NVARCHAR(100),
    Brand NVARCHAR(100),
    Model NVARCHAR(100),
    Supplier NVARCHAR(100),
    Stat NVARCHAR(30),
    Cond NVARCHAR(50),
    Price DECIMAL(18,2),
    StDate DATETIME2,
    EmpId INT NULL,
    DeptId INT NULL,
    Descr NVARCHAR(MAX)
);

INSERT INTO #NewAssets VALUES
-- 1. THIẾT BỊ VĂN PHÒNG & MÁY TÍNH
('TS-IT-001', N'Laptop doanh nghiệp Dell Latitude 5430 Core i5-1245U 16GB SSD 512GB', N'Thiết bị văn phòng', N'Laptop', N'Dell', N'Latitude 5430', N'FPT Synnex', 'ALLOCATED', 'NORMAL', 21500000, '2026-01-10', @EmpA, @DeptIT, N'Cấp phát cho kỹ sư phần mềm phát triển hệ thống nội bộ.'),
('TS-IT-002', N'Laptop Lenovo ThinkPad T14 Gen 4 Core i7 16GB SSD 512GB', N'Thiết bị văn phòng', N'Laptop', N'Lenovo', N'ThinkPad T14 Gen 4', N'FPT Synnex', 'ALLOCATED', 'NORMAL', 28900000, '2026-01-15', @MgrId, @DeptIT, N'Cấp phát cho Trưởng phòng công nghệ thông tin phục vụ quản lý dự án.'),
('TS-IT-003', N'Laptop Apple MacBook Pro 14 inch M3 16GB SSD 512GB Space Gray', N'Thiết bị văn phòng', N'Laptop', N'Apple', N'MacBook Pro 14 M3', N'ShopDunk Pro B2B', 'ALLOCATED', 'NORMAL', 42900000, '2026-01-05', @DirId, @DeptBOD, N'Trang bị phục vụ công tác đối ngoại và điều hành của Ban Giám đốc.'),
('TS-IT-004', N'Laptop mỏng nhẹ Asus ExpertBook B1 B1402CBA Core i5 8GB SSD 256GB', N'Thiết bị văn phòng', N'Laptop', N'Asus', N'ExpertBook B1402', N'Phong Vũ IT', 'IN_STOCK', 'NORMAL', 13200000, '2026-02-18', NULL, @DeptQLTS, N'Lưu kho P.QLTS, sẵn sàng cấp phát cho nhân viên thử việc đạt yêu cầu.'),
('TS-IT-005', N'Màn hình chuyên đồ họa văn phòng Dell UltraSharp U2422H 23.8 inch IPS', N'Thiết bị văn phòng', N'Máy tính để bàn', N'Dell', N'UltraSharp U2422H', N'Phong Vũ IT', 'IN_STOCK', 'NORMAL', 6150000, '2026-02-20', NULL, @DeptQLTS, N'Màn hình viền siêu mỏng chuẩn màu 100% sRGB, lưu kho sẵn sàng bàn giao.'),
('TS-IT-006', N'Màn hình vi tính văn phòng LG 24MP500-B 23.8 inch IPS Full HD', N'Thiết bị văn phòng', N'Máy tính để bàn', N'LG', N'24MP500-B', N'FPT Synnex', 'IN_STOCK', 'NORMAL', 2850000, '2026-02-20', NULL, @DeptQLTS, N'Màn hình góc rộng chống lóa bảo vệ mắt, lưu kho dự phòng.'),
('TS-VP-011', N'Máy in laser đơn năng trắng đen Canon LBP 2900 kinh điển', N'Thiết bị văn phòng', N'Máy in', N'Canon', N'LBP 2900', N'Lê Bảo Minh', 'IN_STOCK', 'NORMAL', 4150000, '2026-02-12', NULL, @DeptQLTS, N'Máy in laser tốc độ cao hộp mực lớn độ bền cực cao, mới 100% trong kho.'),
('TS-VP-012', N'Máy in phun màu đa năng tiếp mực ngoài Epson EcoTank L3250 Wifi', N'Thiết bị văn phòng', N'Máy in', N'Epson', N'EcoTank L3250', N'Phong Vũ IT', 'ALLOCATED', 'NORMAL', 4650000, '2026-01-22', @HrId, @DeptHR, N'Cấp phát cho Phòng Nhân sự in biểu mẫu màu và hợp đồng lao động.'),
('TS-VP-013', N'Máy photocopy đa chức năng A3 mạng Ricoh Aficio MP 3055SP', N'Thiết bị văn phòng', N'Máy photocopy', N'Ricoh', N'MP 3055SP A3', N'Siêu Thanh Corp', 'ALLOCATED', 'NORMAL', 38500000, '2025-12-10', NULL, @DeptQLTS, N'Máy photocopy dùng chung tại sảnh tầng 2 cho toàn thể cán bộ nhân viên.'),
('TS-VP-014', N'Máy hủy tài liệu bảo mật văn phòng Silicon PS-800C cắt vụn', N'Thiết bị văn phòng', N'Máy hủy tài liệu', N'Silicon', N'PS-800C', N'Thời Đại Mới B2B', 'IN_STOCK', 'NORMAL', 2650000, '2026-02-14', NULL, @DeptQLTS, N'Máy hủy tài liệu bảo mật chuẩn DIN 4 cắt vụn thẻ và ghim, lưu kho.'),
('TS-VP-015', N'Máy quét tài liệu 2 mặt tốc độ cao Brother ADS-2800W', N'Thiết bị văn phòng', N'Máy scan tài liệu', N'Brother', N'ADS-2800W', N'Khuê Tú Tech', 'IN_STOCK', 'NORMAL', 14200000, '2026-02-10', NULL, @DeptQLTS, N'Máy scan tốc độ 40 tờ/phút có kết nối Wifi, sẵn sàng cấp phát số hóa tài liệu.'),

-- 2. THIẾT BỊ MẠNG, SERVER & VIỄN THÔNG
('TS-NET-001', N'Switch mạng quản trị Gigabit 24 cổng Cisco Catalyst 1000-24T-4G-L', N'Thiết bị mạng & Tin học', N'Thiết bị mạng & Switch', N'Cisco', N'C1000-24T-4G-L', N'FPT Synnex', 'ALLOCATED', 'NORMAL', 15800000, '2025-11-05', @EmpA, @DeptIT, N'Lắp đặt tại tủ Rack tầng 2 phân phối mạng nội bộ toàn công ty.'),
('TS-NET-002', N'Tường lửa bảo mật doanh nghiệp thế hệ mới Fortinet FortiGate 60F', N'Thiết bị mạng & Tin học', N'Thiết bị mạng & Switch', N'Fortinet', N'FortiGate 60F', N'FPT Synnex', 'ALLOCATED', 'NORMAL', 18900000, '2025-11-05', @EmpA, @DeptIT, N'Kiểm soát tường lửa, chống mã độc và phân luồng VPN làm việc từ xa.'),
('TS-NET-003', N'Bộ phát Wifi chuyên dụng doanh nghiệp Aruba Instant On AP22 WiFi 6', N'Thiết bị mạng & Tin học', N'Bộ phát Wifi', N'Aruba HPE', N'Instant On AP22', N'FPT Synnex', 'IN_STOCK', 'NORMAL', 3950000, '2026-02-15', NULL, @DeptQLTS, N'Bộ phát wifi chuẩn AX chịu tải 100 kết nối đồng thời, lưu kho dự phòng mở rộng.'),
('TS-NET-004', N'Tủ Rack máy chủ Server chuyên dụng 19 inch 27U sâu 1000 có quạt hút', N'Thiết bị mạng & Tin học', N'Thiết bị mạng & Switch', N'VietRack', N'VR-27U-D1000', N'Việt Mạng IT', 'ALLOCATED', 'NORMAL', 7800000, '2025-10-15', @MgrId, @DeptIT, N'Bàn giao cho phòng IT chứa cụm máy chủ và switch trung tâm.'),
('TS-NET-005', N'Máy chủ Server doanh nghiệp Dell PowerEdge R450 Rack 1U 32GB RAM', N'Thiết bị mạng & Tin học', N'Máy chủ Server & Lưu điện', N'Dell', N'PowerEdge R450', N'FPT Synnex', 'ALLOCATED', 'NORMAL', 58500000, '2025-10-20', @MgrId, @DeptIT, N'Vận hành hệ cơ sở dữ liệu và phần mềm HRM Nhị Gia nội bộ.'),

-- 3. THIẾT BỊ GHI HÌNH, HỘI NGHỊ & ÂM THANH
('TS-AV-001', N'Camera hội nghị trực tuyến truyền hình All-in-one Logitech MeetUp 4K', N'Thiết bị ghi hình & Hội nghị', N'Thiết bị họp trực tuyến', N'Logitech', N'MeetUp 4K', N'Phúc Anh Smart', 'ALLOCATED', 'NORMAL', 24500000, '2026-01-08', @DirId, @DeptBOD, N'Lắp đặt tại Phòng Họp Ban Giám đốc phục vụ hội thảo trực tuyến quốc tế.'),
('TS-AV-002', N'Loa hội nghị micro thu âm đa hướng không dây Jabra Speak 710 Bluetooth', N'Thiết bị ghi hình & Hội nghị', N'Thiết bị họp trực tuyến', N'Jabra', N'Speak 710', N'GTC Telecom', 'IN_STOCK', 'NORMAL', 6500000, '2026-02-11', NULL, @DeptQLTS, N'Micro thu âm 360 độ bán kính 3m pin 15 giờ, sẵn sàng mang đi công tác.'),
('TS-AV-003', N'Máy chiếu độ sáng cao 3600 Ansi Lumens Epson EB-E01', N'Thiết bị ghi hình & Hội nghị', N'Máy chiếu & Màn chiếu', N'Epson', N'EB-E01 3LCD', N'Điện Máy Xanh B2B', 'IN_STOCK', 'NORMAL', 9800000, '2026-02-16', NULL, @DeptQLTS, N'Máy chiếu công nghệ 3LCD sắc nét, dùng chung cho các buổi đào tạo nội bộ.'),
('TS-AV-004', N'Màn chiếu điện tử có điều khiển từ xa Dalite 100 inch (1m78 x 1m78)', N'Thiết bị ghi hình & Hội nghị', N'Máy chiếu & Màn chiếu', N'Dalite', N'PW100ES 100 inch', N'Thế Giới Máy Chiếu', 'IN_STOCK', 'NORMAL', 1850000, '2026-02-16', NULL, @DeptQLTS, N'Màn chiếu mô-tơ điện tự cuộn, lưu kho sẵn sàng lắp đặt phòng đào tạo mới.'),
('TS-AV-005', N'Smart Tivi 4K UHD Samsung 65 inch 65DU7700 màn hình lớn', N'Thiết bị ghi hình & Hội nghị', N'Tivi & Màn hình hiển thị', N'Samsung', N'UA65DU7700', N'Điện Máy Chợ Lớn Pro', 'ALLOCATED', 'NORMAL', 14500000, '2026-01-12', @DirId, @DeptBOD, N'Lắp đặt tại Phòng Họp Lớn phục vụ trình chiếu báo cáo tuần và hội nghị cổ đông.'),
('TS-CAM-001', N'Camera IP Dome giám sát an ninh văn phòng Hikvision 4MP DS-2CD1143G0-I', N'Thiết bị ghi hình & Hội nghị', N'Camera', N'Hikvision', N'DS-2CD1143G0-I', N'Phúc Anh Smart', 'ALLOCATED', 'NORMAL', 1250000, '2025-12-01', NULL, @DeptQLTS, N'Lắp đặt tại khu vực cửa ra vào sảnh chính văn phòng.'),
('TS-CAM-002', N'Đầu ghi hình camera IP 16 kênh chuẩn 4K Hikvision DS-7616NXI-K2', N'Thiết bị ghi hình & Hội nghị', N'Camera', N'Hikvision', N'DS-7616NXI-K2', N'Phúc Anh Smart', 'ALLOCATED', 'NORMAL', 4200000, '2025-12-01', @EmpA, @DeptIT, N'Lưu trữ dữ liệu camera giám sát tòa nhà 30 ngày liên tục.'),

-- 4. NỘI THẤT VĂN PHÒNG
('TS-NT-003', N'Cụm bàn làm việc nhân viên 4 chỗ ngồi có vách ngăn Hòa Phát HRMD02', N'Nội thất văn phòng', N'Bàn làm việc', N'Hòa Phát', N'HRMD02 2m4 x 1m2', N'Nội Thất Miền Nam', 'ALLOCATED', 'NORMAL', 6850000, '2025-12-15', @EmpA, @DeptIT, N'Bàn giao cụm 4 chỗ ngồi cho phòng IT trang bị đồng bộ.'),
('TS-NT-004', N'Bàn làm việc giám đốc cao cấp gỗ sơn PU Hòa Phát DT1890H1 1m8', N'Nội thất văn phòng', N'Bàn làm việc', N'Hòa Phát', N'DT1890H1 1m8', N'Nội Thất Miền Nam', 'ALLOCATED', 'NORMAL', 5250000, '2025-11-20', @DirId, @DeptBOD, N'Bàn làm việc Giám đốc kèm tủ phụ di động và hộc tài liệu.'),
('TS-NT-005', N'Ghế chân quỳ bọc da đệm dày phòng họp cao cấp Hòa Phát SL718M', N'Nội thất văn phòng', N'Ghế phòng họp', N'Hòa Phát', N'SL718M Chân Mạ', N'Nội Thất Miền Nam', 'IN_STOCK', 'NORMAL', 1150000, '2026-02-18', NULL, @DeptQLTS, N'Ghế chân quỳ inox sáng bóng bọc da êm ái, lưu kho sẵn sàng bàn giao phòng họp.'),
('TS-NT-006', N'Ghế chân quỳ văn phòng bọc vải nỉ cao cấp Hòa Phát VT1', N'Nội thất văn phòng', N'Ghế phòng họp', N'Hòa Phát', N'VT1 Đen', N'Nội Thất Miền Nam', 'IN_STOCK', 'NORMAL', 680000, '2026-02-18', NULL, @DeptQLTS, N'Ghế họp quỳ tiêu chuẩn khung thép sơn tĩnh điện, số lượng lưu kho dự phòng.'),
('TS-NT-007', N'Bộ bàn ghế sofa tiếp khách văn phòng bọc da cao cấp Hòa Phát SF35', N'Nội thất văn phòng', N'Sofa & Bàn tiếp khách', N'Hòa Phát', N'SF35 (1 dài + 2 đơn)', N'Nội Thất Miền Nam', 'ALLOCATED', 'NORMAL', 12800000, '2025-11-25', @DirId, @DeptBOD, N'Bàn giao tại phòng khánh tiết phục vụ tiếp đón đối tác và khách hàng VIP.'),
('TS-NT-008', N'Tủ sắt hồ sơ văn phòng sơn tĩnh điện 4 cánh mở Hòa Phát TU09K3', N'Nội thất văn phòng', N'Tủ tài liệu & Locker', N'Hòa Phát', N'TU09K3 Xám', N'Nội Thất Miền Nam', 'ALLOCATED', 'NORMAL', 3450000, '2025-12-05', @HrId, @DeptHR, N'Lưu trữ hồ sơ nhân viên, hợp đồng và chứng từ bảo hiểm xã hội an toàn.'),
('TS-NT-009', N'Tủ sắt locker văn phòng 12 ngăn có khóa riêng biệt Hòa Phát LK12', N'Nội thất văn phòng', N'Tủ tài liệu & Locker', N'Hòa Phát', N'LK12-12C', N'Nội Thất Miền Nam', 'ALLOCATED', 'NORMAL', 4150000, '2026-01-10', NULL, @DeptQLTS, N'Bố trí tại sảnh để nhân viên cất giữ đồ đạc cá nhân trong giờ làm việc.'),
('TS-NT-010', N'Kệ tài liệu gỗ công nghiệp 5 tầng văn phòng Hòa Phát KG02', N'Nội thất văn phòng', N'Tủ tài liệu & Locker', N'Hòa Phát', N'KG02 W800', N'Nội Thất Miền Nam', 'IN_STOCK', 'NORMAL', 1850000, '2026-02-15', NULL, @DeptQLTS, N'Kệ sách và hồ sơ văn phòng thiết kế thoáng tiện dụng, lưu kho.'),
('TS-NT-011', N'Hộc tủ di động 3 ngăn kéo có khóa dàn bánh xe Hòa Phát M3D', N'Nội thất văn phòng', N'Tủ tài liệu & Locker', N'Hòa Phát', N'M3D Melamine', N'Nội Thất Miền Nam', 'IN_STOCK', 'NORMAL', 920000, '2026-02-15', NULL, @DeptQLTS, N'Hộc tủ cá nhân lắp dưới gầm bàn làm việc, có khóa an toàn.'),
('TS-NT-012', N'Bảng từ trắng chống lóa Hàn Quốc viết bút lông 1.2m x 2.4m di động', N'Nội thất văn phòng', N'Bảng văn phòng & Phụ kiện', N'Tân Hà Bảng', N'BTT-1224', N'Bảng Văn Phòng VN', 'ALLOCATED', 'NORMAL', 2350000, '2026-01-18', @EmpA, @DeptIT, N'Bố trí tại phòng họp IT phục vụ họp Scrum và vẽ sơ đồ hệ thống.'),

-- 5. THIẾT BỊ TIỆN ÍCH, PANTRY & AN TOÀN
('TS-EL-001', N'Cây nước nóng lạnh hút bình 3 vòi cao cấp Karofi HC18', N'Thiết bị tiện ích & Pantry', N'Máy lọc nước & Cây nước', N'Karofi', N'HC18 3 vòi', N'Điện Máy Xanh B2B', 'ALLOCATED', 'NORMAL', 5650000, '2026-01-15', NULL, @DeptBHCU, N'Bố trí tại khu vực pantry tầng 1 phục vụ nước uống nóng lạnh cho nhân viên.'),
('TS-EL-002', N'Máy lọc nước tinh khiết RO 10 lõi Hydrogen Kangaroo KG100HA', N'Thiết bị tiện ích & Pantry', N'Máy lọc nước & Cây nước', N'Kangaroo', N'KG100HA', N'Điện Máy Chợ Lớn Pro', 'IN_STOCK', 'NORMAL', 6250000, '2026-02-10', NULL, @DeptQLTS, N'Máy lọc nước uống trực tiếp giàu khoáng chất, sẵn sàng lắp đặt pantry tầng 3.'),
('TS-EL-003', N'Tủ lạnh 2 cánh Inverter kháng khuẩn Samsung 236L RT22M4032DX', N'Thiết bị tiện ích & Pantry', N'Tủ lạnh & Thiết bị nhà bếp', N'Samsung', N'RT22M4032DX/SV', N'Điện Máy Xanh B2B', 'ALLOCATED', 'NORMAL', 6390000, '2026-01-10', NULL, @DeptQLTS, N'Tủ lạnh pantry công ty để nhân viên bảo quản thức ăn trưa và sữa chua.'),
('TS-EL-004', N'Lò vi sóng cơ có nướng văn phòng Sharp R-205VN-S 20 Lít 800W', N'Thiết bị tiện ích & Pantry', N'Tủ lạnh & Thiết bị nhà bếp', N'Sharp', N'R-205VN-S', N'Điện Máy Xanh B2B', 'IN_STOCK', 'NORMAL', 1490000, '2026-02-12', NULL, @DeptQLTS, N'Lò vi sóng hâm nóng thức ăn tiện lợi cho nhân viên, lưu kho.'),
('TS-EL-005', N'Máy pha cà phê hạt tự động văn phòng DeLonghi Magnifica S ECAM 22.110.B', N'Thiết bị tiện ích & Pantry', N'Tủ lạnh & Thiết bị nhà bếp', N'DeLonghi', N'ECAM 22.110.B', N'Quang Hạnh Pro', 'ALLOCATED', 'NORMAL', 18500000, '2026-01-08', @DirId, @DeptBOD, N'Bàn giao tại quầy bar phòng khách Ban Giám đốc phục vụ đón tiếp đối tác.'),
('TS-EL-006', N'Máy lọc không khí và bù ẩm thông minh Sharp KC-G40EV-W Plasmacluster', N'Thiết bị tiện ích & Pantry', N'Máy lọc không khí', N'Sharp', N'KC-G40EV-W', N'Phong Vũ IT', 'IN_STOCK', 'NORMAL', 6990000, '2026-02-14', NULL, @DeptQLTS, N'Lọc bụi mịn PM2.5 khử mùi diệt khuẩn cho phòng họp kín, lưu kho sẵn sàng.'),
('TS-SEC-001', N'Máy chấm công nhận diện khuôn mặt & thẻ ZKTeco FaceDepot-7A màn 7 inch', N'Thiết bị tiện ích & Pantry', N'Thiết bị an toàn & PCCC', N'ZKTeco', N'FaceDepot-7A', N'Phúc Anh Smart', 'ALLOCATED', 'NORMAL', 8900000, '2025-12-20', @HrId, @DeptHR, N'Lắp đặt tại cửa chính văn phòng để ghi nhận thời gian chấm công nhân sự.'),
('TS-SEC-002', N'Bình chữa cháy khí CO2 loại 3kg MT3 an toàn cho thiết bị điện tử', N'Thiết bị tiện ích & Pantry', N'Thiết bị an toàn & PCCC', N'Dragon VN', N'CO2 MT3', N'PCCC Sài Gòn', 'IN_STOCK', 'NORMAL', 450000, '2026-02-01', NULL, @DeptQLTS, N'Bình chữa cháy đạt chuẩn kiểm định PCCC trang bị phòng máy chủ Server.'),
('TS-SEC-003', N'Bình chữa cháy bột khô tổng hợp ABC 4kg Dragon MFZL4', N'Thiết bị tiện ích & Pantry', N'Thiết bị an toàn & PCCC', N'Dragon VN', N'MFZL4 4kg', N'PCCC Sài Gòn', 'IN_STOCK', 'NORMAL', 280000, '2026-02-01', NULL, @DeptQLTS, N'Bình bột dập cháy nhanh đặt tại các góc hành lang tòa nhà.'),
('TS-SEC-004', N'Tủ thuốc y tế sơ cấp cứu văn phòng gắn tường Inox chuẩn Thông tư Bộ Y tế', N'Thiết bị tiện ích & Pantry', N'Thiết bị an toàn & PCCC', N'Việt Nhật Med', N'First-Aid Inox 01', N'Thiết Bị Y Tế SG', 'ALLOCATED', 'NORMAL', 850000, '2026-01-05', @HrId, @DeptHR, N'Bàn giao cho phòng nhân sự theo dõi cơ số thuốc sơ cấp cứu khẩn cấp.');

-- INSERT VÀO dbo.HrmWorkItem (Bỏ qua nếu đã tồn tại mã)
INSERT INTO dbo.HrmWorkItem
    (Kind, Reference, Title, WorkLocation, Category, JobLevel, ExperienceRequired, EducationRequired, Status, SalaryRange, Target, StartDate, DueDate, EmployeeId, DepartmentId, Description, Priority, CreatedBy, CreatedAt)
SELECT
    'assets', n.Ref, n.Title, n.Grp, n.Cat, n.Brand, n.Model, n.Supplier, n.Stat, n.Cond, n.Price, n.StDate, NULL, n.EmpId, n.DeptId, n.Descr, 'NORMAL', @AdminId, n.StDate
FROM #NewAssets n
WHERE NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem w WHERE w.Kind = 'assets' AND w.Reference = n.Ref);

-- TẠO BIÊN BẢN BÀN GIAO CHO CÁC TÀI SẢN ALLOCATED ĐƯỢC CẤP PHÁT CHO NHÂN SỰ
DECLARE @HandoverCount INT = (SELECT COUNT(1) FROM dbo.HrmWorkItem WHERE Kind = 'asset-handover');
DECLARE @NextIdx INT = @HandoverCount + 1;

INSERT INTO dbo.HrmWorkItem
    (Kind, Reference, Title, ExperienceRequired, WorkLocation, Category, EmployeeId, DepartmentId, StartDate, DueDate, SalaryRange, Target, Description, Status, Priority, CreatedBy, CreatedAt)
SELECT
    'asset-handover',
    CONCAT('BG-26-', RIGHT(CONCAT('000', ROW_NUMBER() OVER (ORDER BY n.Ref) + @HandoverCount), 3)),
    CONCAT(N'Bàn giao ', n.Title),
    n.Ref,
    n.Grp,
    N'Cấp phát',
    n.EmpId,
    n.DeptId,
    n.StDate,
    n.StDate,
    n.Cond,
    n.Price,
    CONCAT(N'Biên bản bàn giao tài sản: ', n.Descr),
    'ALLOCATED',
    'NORMAL',
    @AdminId,
    n.StDate
FROM #NewAssets n
WHERE n.Stat = 'ALLOCATED' AND n.EmpId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.HrmWorkItem h WHERE h.Kind = 'asset-handover' AND h.ExperienceRequired = n.Ref);

DROP TABLE #NewAssets;
GO

