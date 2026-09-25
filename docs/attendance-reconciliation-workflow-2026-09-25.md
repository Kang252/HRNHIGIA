# Đối soát và chốt công

## Luồng sử dụng

1. HANET đồng bộ dữ liệu và ghi nhật ký cho từng ngày. Quản trị viên xem số lượt nhận, lượt mới, lượt đã ánh xạ và chưa ánh xạ trong tab **Đối soát**.
2. Nhân viên kiểm tra bảng công theo tháng. Nếu sai giờ vào hoặc giờ ra, nhân viên gửi yêu cầu điều chỉnh kèm lý do.
3. Trưởng phòng chỉ xử lý nhân viên trong phòng; HR, Giám đốc và quản trị viên xử lý toàn công ty. Người gửi không được tự duyệt.
4. Mỗi nhân viên xác nhận dữ liệu tháng. HR chỉ khóa kỳ khi không còn yêu cầu chờ xử lý và toàn bộ tài khoản nhân viên đang hoạt động đã xác nhận.
5. Sau khi khóa, sự kiện HANET đến muộn vẫn được lưu để không mất dữ liệu nhưng chưa tham gia kết quả chấm công. Khi mở khóa có lý do, hệ thống tính lại từ toàn bộ sự kiện đã nhận.
6. HR xem danh sách người chưa xác nhận và gửi thông báo nhắc trong hệ thống. Mỗi người chỉ nhận tối đa một thông báo nhắc cho cùng kỳ trong ngày.

## Quy tắc dữ liệu

- Mỗi nhân viên chỉ có một yêu cầu điều chỉnh đang chờ cho một ngày.
- Giờ đề nghị phải thuộc đúng ngày công; giờ ra phải sau giờ vào.
- Yêu cầu đã duyệt mới thay thế giờ thiết bị. Nếu có nhiều lần duyệt cho cùng ngày, lần duyệt mới nhất được dùng.
- Nhật ký đồng bộ thành công và thất bại được giữ riêng để phục vụ đối soát.
- Mở khóa kỳ bắt buộc nhập lý do và mọi thao tác xác nhận, khóa, mở khóa, tạo và duyệt điều chỉnh đều ghi nhật ký kiểm toán.
- Khóa ngoại lệ được phép khi còn người chưa xác nhận hoặc điều chỉnh đang chờ, nhưng bắt buộc nhập lý do và được ghi riêng trong nhật ký kiểm toán.
- Màn hình đối soát cho phép xem tối đa 1.000 sự kiện của một ngày, lọc theo trạng thái ánh xạ và chạy đồng bộ lại ngày đó. Payload JSON gốc không được đưa ra giao diện.

## Luồng quyết định điều chuyển

- Quyết định mới luôn bắt đầu ở trạng thái **Chờ phê duyệt**, không nhận trạng thái từ biểu mẫu.
- Chỉ quản trị viên hoặc Giám đốc được phê duyệt quyết định.
- Chỉ quyết định đã duyệt mới được thực thi để cập nhật phòng ban/chức vụ của nhân viên.
- Quyết định đã thực thi không thể sửa hoặc xóa qua giao diện thông thường.
