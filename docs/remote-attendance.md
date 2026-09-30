# Chấm công ngoài công ty — bản đầu

## Cách sử dụng

Mở **Thủ tục → Tạo thủ tục → Đăng ký lịch ngoài công ty** để mở form đăng ký. Theo dõi lịch đã gửi tại danh sách bên dưới trang Thủ tục. Mở **Chấm công → Chấm công ngoài công ty** (hoặc mục cùng tên trong menu) để ghi nhận lượt vào/ra và ghé khách hàng. Nút “Dùng lại thông tin” ở trang chấm công sẽ mở form đăng ký trong cửa sổ Tạo thủ tục.

1. Đăng ký lịch theo ngày, thứ trong tuần, khung giờ, hình thức và nội dung công việc. Chọn ca linh hoạt nếu cần đủ số phút làm trong khung giờ; giờ làm được tính từ lượt vào đầu tới lượt ra cuối, trừ phút nghỉ đã đăng ký. Bản đầu hỗ trợ ca trong cùng ngày.
2. **Làm tại nhà:** nhập tọa độ nhà và bán kính 50–2.000 m, mặc định 200 m; dùng nút lấy vị trí hiện tại khi đang ở nhà và kiểm tra bằng liên kết bản đồ. Có thể dùng lại thông tin địa điểm từ lịch cũ để gửi đăng ký mới.
3. **Đi thị trường:** ghi tuyến/khu vực; tọa độ vùng là tùy chọn. Để trống tọa độ cho tuyến linh hoạt, hoặc nhập tọa độ/bán kính để đối chiếu. Có thể liên kết đơn “Công tác/Ra ngoài” đã được duyệt của chính nhân viên, bao phủ khoảng ngày. Mỗi lượt được nhập khách hàng/điểm phát sinh thực tế.
4. Quản lý duyệt lịch kèm ghi chú. Quản lý phòng chỉ xử lý nhân viên thuộc phòng; HR/Admin/Giám đốc có phạm vi toàn công ty. Không tự duyệt. Một nhân viên không có hai lịch ngoài công ty được duyệt trùng ngày áp dụng; hủy lịch trùng trước khi thay thế.
5. Chọn lịch, lượt **Vào làm / Kết thúc làm / Ghé khách hàng**, chụp ảnh và lấy GPS rồi gửi. Ảnh selfie phục vụ đối soát lúc vào/ra; ảnh điểm bán dùng cho lượt ghé khách hàng. Không có kiểm tra khuôn mặt/liveness tự động trong bản này.
6. Lượt đúng lịch/khung giờ, GPS đủ chính xác và chắc chắn trong vùng (nếu có vùng) được chấp nhận. Các ngoại lệ cần ghi chú, chuyển chờ xác minh; quản lý mở danh sách, xem ảnh/vị trí và duyệt hoặc từ chối kèm lý do.

## Offline và quyền thiết bị

Trang cần được mở và tải lịch khi đang có mạng. Có thể tiếp tục chụp/chấm nếu kết nối mất trong khi trang đang mở. Không hỗ trợ mở lại toàn bộ HRM khi chưa có mạng. Bản ghi được mã hóa AES-GCM trong IndexedDB, khóa không xuất được qua Web Crypto và tách theo tài khoản; đây không thay thế bảo mật máy dùng chung. Không xóa dữ liệu trình duyệt trước khi đồng bộ.

Bấm **Đồng bộ lại** hoặc đợi sự kiện có mạng khi trang đang mở. Server xác minh chủ tài khoản và mã duy nhất của từng lượt; gửi lại sau mất phản hồi không tạo bản sao. Giới hạn hàng chờ 50 lượt/thiết bị/tài khoản. Có thể bổ sung giải trình hoặc xóa bản chờ. Sau khi server xác nhận đã nhận, bản cục bộ được xóa. Lượt offline/đồng bộ lệch trên 2 phút phải được xác minh; quá 7 ngày hoặc đồng hồ đi trước trên 5 phút bị từ chối. Giữ cả giờ thiết bị, giờ nhận và giờ tính công.

Camera và GPS cần HTTPS và quyền của người dùng. GPS được lấy theo từng lượt, không có theo dõi nền. Tài liệu trình duyệt: [Geolocation](https://developer.mozilla.org/en-US/docs/Web/API/Geolocation/getCurrentPosition), [camera](https://developer.mozilla.org/en-US/docs/Web/API/MediaDevices/getUserMedia). Dữ liệu GPS/selfie phía trình duyệt có thể bị giả lập; bản này không tuyên bố chống giả mạo hoặc nhận diện AI.

## Bản đồ địa điểm

Form đăng ký tích hợp Leaflet 1.9.4 (thư viện lưu cùng ứng dụng) và nền OpenStreetMap. Mở vùng địa lý để bấm chọn hoặc kéo ghim; tọa độ và vòng bán kính cập nhật theo form. Nút lấy GPS vẫn cần quyền vị trí. Bản đồ lịch hiển thị bán kính đăng ký; bản đồ lượt chấm hiển thị vòng sai số GPS, không phải vùng làm việc được duyệt. Vị trí lượt chấm chỉ để xem, không chỉnh qua bản đồ.

Có thể đổi máy chủ raster tiles bằng `Maps__TileUrl` (HTTPS, mẫu `{z}/{x}/{y}`) và `Maps__Attribution` (ghi nguồn dạng văn bản) trên Render. Khóa nằm trong URL tiles là khóa công khai phía trình duyệt: chỉ dùng khóa được nhà cung cấp cho phép và giới hạn theo tên miền. Google Maps/Vietmap SDK và tìm địa chỉ chưa được tích hợp; cần cấu hình dịch vụ tương ứng nếu bổ sung.

Nền mặc định dùng `https://tile.openstreetmap.org/{z}/{x}/{y}.png`, hiển thị nguồn dữ liệu, giữ cache HTTP và chỉ tải khi bản đồ được mở. Không tải trước hoặc lưu bản đồ offline. Dịch vụ công cộng không có SLA; khi cần cam kết vận hành, đổi sang nhà cung cấp có hợp đồng. Tham khảo [Leaflet](https://leafletjs.com/examples/quick-start/) và [chính sách OSM](https://operations.osmfoundation.org/policies/tiles/). Tọa độ nhập tay/GPS và hàng chờ chấm công vẫn hoạt động khi nền bản đồ mất kết nối. Kiểm thử tự động dùng tiles giả lập để không gọi dịch vụ công cộng.

## Tính công và lưu dữ liệu

- SQL lưu lịch, vị trí, ảnh JPEG tối đa 2 MB, quyết định và nhật ký; ảnh không nằm trong thư mục tĩnh và không mất khi deploy. Endpoint ảnh yêu cầu đăng nhập/phạm vi phù hợp và không cache.
- Lượt ghé khách hàng lưu bằng chứng nhưng không tạo giờ vào/ra. Lượt chờ/từ chối không tính công. Lượt đã chấp nhận hợp nhất với HANET thành một dòng/người/ngày và dùng chung xuất Excel, dashboard, AI và chốt kỳ.
- Lượt vào/ra có loại rõ ràng: hai lượt vào không tự sinh lượt ra. Ca linh hoạt xét số phút đủ trong khung giờ thay cho phạt vào muộn/về sớm. Danh sách hiển thị “Chưa đủ giờ” khi hết khung giờ nhưng thiếu thời lượng.
- Lượt mới được chấp nhận làm mất xác nhận tháng cũ của nhân viên để đối chiếu lại. Khóa kỳ thông thường chặn khi còn lượt vào/ra chờ xác minh. Kỳ khóa không cho tải/duyệt lượt mới; HR phải mở khóa trước. Khóa ngoại lệ hiện có vẫn yêu cầu lý do.
- Hủy lịch không xóa các lượt đã được xác minh. Mọi thay đổi trạng thái tạo nhật ký; yêu cầu mới/kết quả duyệt tạo thông báo trong HRM.

## Triển khai và kiểm chứng

Startup chạy `App_Data/remote-attendance.sql` sau `hrm-mvp.sql`; migration chỉ tạo bảng mới nếu chưa có. Tài khoản SQL triển khai cần quyền tạo bảng và chỉ mục. Không sửa dữ liệu mẫu/nhân viên trong môi trường thật để kiểm thử.

`dotnet run --project tests/RemoteAttendance.Tests -c Release` kiểm tra policy không cần SQL. Cờ `--integration` tạo rồi xóa một database riêng có tiền tố `NHIGIA_RemoteTests_` trên LocalDB instance `NHIGIA`; không đọc chuỗi kết nối production. Cờ `--browser` chạy camera/GPS giả lập, kiểm tra offline và layout trong trình duyệt trên loopback. Chọn Playwright bằng biến `HRM_TEST_PLAYWRIGHT` nếu máy không có module trên đường dẫn mặc định.

Cần thí điểm trên điện thoại thực (quyền camera, GPS trong nhà, mạng chập chờn) trước khi dùng để chốt lương. Face ID/liveness và theo dõi lộ trình nền là các giai đoạn tiếp theo, chưa nằm trong tính năng đã triển khai ở bản này.
