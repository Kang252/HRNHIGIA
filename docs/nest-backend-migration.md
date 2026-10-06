# Chuyển đổi backend từng phần sang NestJS

`NHIGIA.Api` là backend NestJS/TypeScript mới chạy song song với
`NHIGIA.Modern`. Đây là migration kiểu từng phần: C# vẫn là hệ thống chủ cho
những nghiệp vụ chưa có API tương đương, còn NestJS chỉ nhận các route đã được
đối chiếu quyền và dữ liệu.

## Slice đầu tiên

- `POST /api/v1/auth/login` xác thực trực tiếp với `dbo.HrmUserAccount` bằng
  PBKDF2-HMAC-SHA1, 100.000 vòng, salt Base64 và hash 32 byte hiện tại.
- Sau khi đăng nhập, API đặt cookie opaque `HttpOnly` và chỉ lưu SHA-256 của
  token trong bảng mới `dbo.HrmApiSession`.
- Mỗi địa chỉ IP có tối đa 10 lần thử đăng nhập trong 15 phút trên một instance;
  IP chỉ được lưu dạng hash trong bộ nhớ của process. Khi chạy nhiều instance,
  giới hạn tương đương cần đặt tại gateway hoặc Redis dùng chung.
- `GET /api/v1/auth/me`, `GET /api/v1/dashboard` và
  `GET /api/v1/communications` dùng session đó; SQL luôn áp dụng cùng phạm vi
  nhân viên/phòng ban/toàn công ty đang dùng ở `NHIGIA.Modern`.
- `POST /api/v1/auth/logout` thu hồi session trong SQL và xóa cookie.
- `GET /api/v1/users` và `GET /api/v1/users/:id` là danh bạ nhân sự chỉ đọc.
  EMPLOYEE chỉ thấy chính mình, MANAGER thấy phòng ban, HR/DIRECTOR/ADMIN thấy
  toàn công ty; response không chứa CCCD, lương, bảo hiểm, địa chỉ, tài khoản
  ngân hàng hoặc hash mật khẩu.
- `GET /api/v1/users/:id/avatar` chỉ trả ảnh nhị phân cho người có quyền xem
  hồ sơ tương ứng. Khi ảnh được lưu trong SQL, URL trong danh bạ và truyền
  thông luôn trỏ về route này thay vì route ASP.NET cũ.
- Truyền thông nội bộ có đầy đủ `POST`, `PATCH`, `DELETE`, duyệt/từ chối,
  thích/bỏ thích và bình luận văn bản qua `/api/v1/communications`. Mọi thao
  tác ghi đều kiểm tra phiên, same-origin XHR và quyền SQL: bài `ALL` chỉ dành
  cho HR/Ban giám đốc/Admin; bài `MANAGER` chỉ dành cho quản lý hoặc cấp cao hơn.
  Tệp đính kèm vẫn ở C# cho tới khi endpoint multipart và quyền tải tệp được
  chuyển cùng một thay đổi đã kiểm tra.
- `GET /api/v1/notifications`, `GET /api/v1/notifications/unread-count`,
  `POST /api/v1/notifications/:id/read` và
  `POST /api/v1/notifications/read-all` chỉ thao tác thông báo của phiên hiện
  tại. Link trả về phải là đường dẫn nội bộ an toàn; API không dùng dữ liệu cũ
  để điều hướng sang website bên ngoài.
- `GET /api/v1/attendance` và `GET /api/v1/attendance/period` dùng cùng phạm
  vi dữ liệu C# (nhân viên: bản thân; trưởng phòng: phòng ban; HR/Giám đốc/
  Admin: toàn công ty). `POST /api/v1/attendance/period/confirm` chỉ xác nhận
  kỳ của chính người dùng, không khóa kỳ hoặc sửa log chấm công.
- `GET /api/v1/procedures` và `GET /api/v1/procedures/:source/:id` là phần đọc
  cho nghỉ phép và các thủ tục đã cho phép. Tệp đính kèm nhị phân, thông tin
  tài chính, tài sản, KPI, tuyển dụng và đào tạo vẫn ở C# cho đến khi có luồng
  tải xuống/ủy quyền được kiểm tra riêng.

Không có password, token session, chuỗi kết nối, hoặc khóa nhà cung cấp nào được
ghi log. Cookie `NHIGIA.Auth.v3` của ASP.NET Core không được NestJS giải mã hay
parse; đây là dữ liệu Microsoft Data Protection riêng của C#.

Các endpoint ghi của API (đăng nhập/đăng xuất, thông báo, chấm công và truyền
thông) chỉ nhận yêu cầu cùng origin từ giao diện NHIGIA, có header
`X-Requested-With: XMLHttpRequest`; login còn bắt buộc
`Content-Type: application/json`. Quy tắc này chặn website khác ép trình duyệt
tạo phiên hoặc thay đổi dữ liệu. Vì vậy client production phải được phục vụ cùng
origin với API; Vite development proxy đã chuyển tiếp origin an toàn khi phát
triển cục bộ.

## Áp dụng migration

Sao lưu cơ sở dữ liệu trước, sau đó chạy
[`001_api_sessions.sql`](../NHIGIA.Api/sql/001_api_sessions.sql) một lần trên
đúng database SQL Server mà `NHIGIA.Modern` đang dùng. Script chỉ thêm bảng và
index, không sửa bảng tài khoản hiện hữu.

Khi `AUTH_MODE=session`, service kiểm tra bảng `dbo.HrmApiSession` và các cột
cần thiết lúc khởi động. Nếu migration chưa chạy hoặc thiếu cột, service dừng
khởi động và endpoint health trả lỗi khi được kiểm tra lại; không có trạng thái
health thành công rồi mới hỏng ở bước login.

Sau đó đặt tối thiểu các biến môi trường cho dịch vụ NestJS. Có thể dùng các
biến `DB_*` tách rời hoặc một chuỗi `DB_CONNECTION_STRING` mới dành cho Node.
`ConnectionStrings__MainConnectionString` chỉ được nhận để hỗ trợ cấu hình đã
kiểm tra; `DB_CONNECTION_STRING` được ưu tiên nếu cả hai cùng tồn tại. Chuỗi kết
nối nhận các khóa ADO.NET thông dụng `Server`/
`Data Source`, `Database`/`Initial Catalog`, `User Id`, `Password`, `Encrypt`
và `TrustServerCertificate`; Node không log chuỗi này. `Integrated Security`
không dùng được trên Render, nên cần SQL login.

```text
NODE_ENV=production
DB_ENABLED=true
DB_CONNECT_ON_STARTUP=true
DB_SERVER=<sql-server-host>
DB_DATABASE=<database-name>
DB_USER=<sql-login>
DB_PASSWORD=<sql-password>
DB_ENCRYPT=true
DB_TRUST_SERVER_CERTIFICATE=false
AUTH_MODE=session
TRUST_PROXY_HOPS=1
```

Node từ chối chạy production nếu chuỗi kết nối có
`TrustServerCertificate=True`. Vì vậy không sao chép nguyên trạng secret C# cũ
nếu nó đang bỏ qua kiểm tra chứng chỉ SQL; hãy cấp chuỗi riêng sử dụng SQL login,
`Encrypt=True` và chứng chỉ SQL Server hợp lệ.

Ví dụ chuỗi riêng của service NestJS:

```text
NODE_ENV=production
DB_CONNECTION_STRING=Server=<sql-host>;Database=<database>;User Id=<sql-user>;Password=<secret>;Encrypt=True;TrustServerCertificate=False
DB_CONNECT_ON_STARTUP=true
AUTH_MODE=session
```

Không đưa `DB_PASSWORD` hoặc file `.env` lên Git. Sử dụng secret store của host.
`AUTH_MODE=disabled` chỉ dành cho foundation local; production validation sẽ từ
chối khởi động nếu chế độ này vẫn bật.

Ngoài session migration, các route nghiệp vụ cần đúng schema đã được C# sử
dụng. Đặc biệt endpoint chấm công đọc dữ liệu chấm công ngoài công ty, nên SQL
Server phải đã áp dụng [`remote-attendance.sql`](../NHIGIA.Modern/App_Data/remote-attendance.sql)
trước khi route `/api/v1/attendance` được bật. Không có migration này, bảng
`HrmRemoteWorkPlan` và `HrmRemotePunch` chưa tồn tại; không nên dùng endpoint
để suy đoán hay tự tạo schema trên production.

Khi một route notifications, truyền thông, chấm công hoặc thủ tục được gọi,
NestJS kiểm tra metadata của các bảng/cột mà route đó thực sự sử dụng và cache
kết quả 30 giây. Nếu thiếu migration hoặc SQL không phản hồi, API trả `503` với
thông điệp tiếng Việt nêu rõ feature bị chặn; API không trả trực tiếp lỗi SQL như
`Invalid column name 'ApprovedByUserId'`. Đây là cơ chế chặn an toàn, không tự
tạo hoặc tự sửa schema production.

## Phục vụ React mới

[`NHIGIA.Api/Dockerfile`](../NHIGIA.Api/Dockerfile) có multi-stage build riêng:
nó build `NHIGIA.Modern/ClientApp`, copy bundle vào `/app/client-app`, và NestJS
phục vụ bundle tại `/modern-ui/` với SPA fallback. API và static site cùng origin
nên cookie session không cần CORS. Image mới mặc định lắng nghe
`0.0.0.0:10000`, tương thích Render khi tạo **một service mới** trỏ đến
`NHIGIA.Api/Dockerfile`.

Không sửa root `Dockerfile` hoặc `render.yaml` trong giai đoạn này, nên Render
service C# hiện tại không thay đổi. Khi thử nghiệm, cấu hình service mới với
`CLIENT_APP_DIR=/app/client-app` (Dockerfile đã đặt sẵn), `AUTH_MODE=session`,
và các SQL secrets nói trên. Route `/modern-ui/*` chỉ fallback về `index.html`
khi trình duyệt yêu cầu HTML; file asset bị thiếu không bị biến thành HTML.

## Quy tắc chuyển dần

1. Giữ URL C# hiện tại và Render service hiện tại làm nguồn chính.
2. Chỉ đưa frontend mới sang endpoint NestJS sau khi so sánh phản hồi với C# ở
   các vai trò EMPLOYEE, MANAGER, HR, DIRECTOR và ADMIN.
3. Dashboard Nest hiện chỉ có ba metric đã kiểm tra (`totalEmployees`,
   `pendingApprovals`, `unmappedHanetUsers`), nên chưa thay trang dashboard C#.
4. Không chuyển bảng SQL Server, khả năng chấm công HANET, webhook TopCV/
   CareerViet, AI Gemini, dữ liệu sinh trắc học hoặc OpenCV trong giai đoạn đầu.
5. Chỉ tách các tính năng đó sau khi có hợp đồng API, bộ dữ liệu replay và cơ
   chế queue/khóa phân tán riêng.

## Kiểm thử trước khi bật route mới

Trước khi chuyển bất kỳ màn hình nào sang NestJS, chạy kiểm thử tĩnh và smoke
test trong `NHIGIA.Api`, sau đó đối chiếu trực tiếp với C# bằng tài khoản thuộc
các vai trò EMPLOYEE, MANAGER, HR, DIRECTOR và ADMIN. Với chấm công, kiểm tra
riêng ngày đang trong ca, ngày đã kết thúc ca, nghỉ nửa ngày, ca qua đêm và kỳ
đã khóa. Chỉ bật UI sau khi database thật đã có các bảng tương ứng và phản hồi
được đối chiếu; build thành công không chứng minh API đã sẵn sàng vận hành.

## Cầu nối tùy chọn

`AUTH_MODE=proxy-hmac` được chuẩn bị cho tương lai, nhưng không phải đường đăng
nhập chính. Nếu dùng, proxy nội bộ phải kiểm tra cookie C# trước, loại bỏ mọi
header `X-NHIGIA-*` do Internet gửi vào, rồi ký canonical request bằng
HMAC-SHA256 theo code trong `ProxyIdentityGuard`. Không public NestJS trực tiếp
ở chế độ này nếu chưa có proxy đáng tin cậy.
