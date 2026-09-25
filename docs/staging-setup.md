# Môi trường staging an toàn

Staging phải dùng một SQL Server/database riêng. Không sao chép connection string của production.

## Biến môi trường bắt buộc

- `ASPNETCORE_ENVIRONMENT=Staging`
- `ConnectionStrings__MainConnectionString`: connection string của database staging
- `HRM_EXPECTED_DATABASE`: đúng tên database staging trong connection string
- `HRM_DATA_PROTECTION_PATH`: thư mục key bền vững của staging

Ứng dụng từ chối khởi động kết nối SQL trong môi trường Staging nếu thiếu `HRM_EXPECTED_DATABASE` hoặc tên database không khớp. Cấu hình này ngăn smoke/integration test ghi nhầm vào production.

## Quy trình kiểm tra

1. Tạo database staging trống bằng tài khoản riêng, quyền giới hạn trên database đó.
2. Khởi động ứng dụng; script schema idempotent tạo các bảng còn thiếu.
3. Chạy `scripts/verify-staging.ps1 -BaseUrl https://<staging-host>`.
4. Dùng tài khoản thử nghiệm để kiểm tra tạo/sửa/duyệt; không nhập dữ liệu nhân viên thật.
5. Xóa và tạo lại database staging để kiểm tra migration từ đầu trước khi áp dụng lên production.

GitHub Actions chỉ chạy build, policy tests và smoke tests với SQL loopback không tồn tại. Workflow CI không nhận hoặc sử dụng connection string production.
