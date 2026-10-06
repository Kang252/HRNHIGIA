# NHIGIA Modern Client

Nền giao diện React + TypeScript để chuyển dần các màn hình Razor của HRM Nhị Gia.
Thư mục này không chạy máy chủ, không kết nối SQL Server và không tự thay thế bất kỳ
màn hình nào của ứng dụng .NET hiện tại.

## Phát triển cục bộ

```powershell
Set-Location NHIGIA.Modern\ClientApp
npm ci
$env:HRM_DEV_API_ORIGIN = "http://localhost:10000"
npm run dev
```

Vite chạy ở `http://127.0.0.1:5173`. Mọi yêu cầu bắt đầu bằng `/api` được chuyển
qua ứng dụng backend tại `HRM_DEV_API_ORIGIN`; khi triển khai, ứng dụng gọi API cùng
tên miền nên không cần cấu hình CORS. Proxy cục bộ chuyển tiếp origin của API cho
các thao tác đăng nhập/đăng xuất để giữ cùng cơ chế chống CSRF như khi chạy production.

## Build

```powershell
Set-Location NHIGIA.Modern\ClientApp
npm ci
npm run build
```

Mặc định tệp tĩnh được tạo trong `dist`. Để CI hoặc Docker cung cấp thư mục xuất,
đặt biến `HRM_CLIENT_OUT_DIR`. Biến `VITE_APP_BASE_PATH` đặt vị trí public của tệp
tĩnh; mặc định là `/modern-ui/`.

```powershell
$env:HRM_CLIENT_OUT_DIR = "..\wwwroot\modern-ui"
$env:VITE_APP_BASE_PATH = "/modern-ui/"
npm run build
```

Không dùng `HRM_CLIENT_OUT_DIR` trỏ tới thư mục có nội dung cần giữ, vì Vite sẽ
làm sạch thư mục xuất trước mỗi lần build.

## Hợp đồng API

Client mặc định gọi API cùng tên miền tại `/api/v1`. Có thể thay đổi đường dẫn
gốc tương đối qua `VITE_API_BASE_PATH`, ví dụ `/api/v2`; không chấp nhận URL của
tên miền khác. Phiên đăng nhập dùng cookie opaque do API phát hành cùng tên miền,
không đọc hoặc phụ thuộc vào cookie của ứng dụng Razor cũ. API cần trả lỗi JSON cho
các mã 401/403/422 và kiểm tra quyền ở backend, kể cả khi giao diện ẩn một nút thao tác.

Các endpoint hiện dùng trong giao diện là:

- `GET /api/v1/health`
- `POST /api/v1/auth/login` với `{ "username", "password" }`
- `GET /api/v1/auth/me`
- `POST /api/v1/auth/logout`
- `GET /api/v1/dashboard`
- `GET /api/v1/communications?keyword=&category=&status=`

Định dạng lỗi NestJS được hiểu trực tiếp: `{ "success": false, "error": { "code", "message" }, "requestId" }`.
`requestId` được hiển thị ở các trạng thái lỗi để hỗ trợ tra log phía máy chủ.

Các API ghi dữ liệu dùng cookie phải có cơ chế chống giả mạo yêu cầu ở backend.
Hiện login/logout bắt buộc cùng origin, `X-Requested-With: XMLHttpRequest` và
(với login) JSON; `apiRequest` đã gửi header này mặc định. Tùy chọn `csrfToken`
vẫn sẵn sàng cho endpoint mới khi backend bổ sung token bootstrap riêng.
