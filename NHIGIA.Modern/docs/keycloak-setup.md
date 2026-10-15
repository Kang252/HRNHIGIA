# Cấu hình đăng nhập tập trung bằng Keycloak

## 1. Khởi động Keycloak

### Nạp cấu hình local có sẵn

Tệp `../keycloak/nhigia-realm.local.json` chứa realm và client dành riêng cho máy phát triển; không chứa user hoặc mật khẩu. Khi chạy Keycloak local lần đầu, sao chép tệp này vào thư mục `data/import` của Keycloak và khởi động với tham số `--import-realm`.

> Tệp import local dùng wildcard redirect URI để tương thích với cổng phát triển thay đổi. Không dùng tệp này cho production; production phải khai báo các redirect URI HTTPS cụ thể.

Từ thư mục `C:\Users\tranp\Downloads\keycloak-26.8.0\keycloak-26.8.0`, chạy thử nghiệm cục bộ:

```powershell
$env:KC_BOOTSTRAP_ADMIN_USERNAME = "admin"
$env:KC_BOOTSTRAP_ADMIN_PASSWORD = "đặt-mật-khẩu-mạnh"
.\bin\kc.bat start-dev --http-port=8081
```

Mở `http://localhost:8081`, đăng nhập quản trị và tạo Realm local tên `nhigia`.
Môi trường production trên Render hiện dùng Realm `hrm-web`.

> `start-dev` chỉ dùng trên máy phát triển. Khi triển khai thật, Keycloak phải chạy thành một dịch vụ riêng có tên miền HTTPS, cơ sở dữ liệu PostgreSQL và biến môi trường quản trị mạnh.

## 2. Tạo client cho HRM

Trong Realm production `hrm-web`, tạo client OpenID Connect với các giá trị:

| Thuộc tính | Giá trị |
| --- | --- |
| Client ID | `hrm-web` |
| Client authentication | Bật (confidential client) |
| Standard flow | Bật |
| Full scope allowed | Tắt |
| Valid redirect URIs | `https://hrnhigia.onrender.com/signin-oidc` |
| Valid post logout redirect URIs | `https://hrnhigia.onrender.com/*` |
| Web origins | `https://hrnhigia.onrender.com` |

Sao chép **Client secret** ở tab Credentials. Không đưa secret vào Git hay file `appsettings.json` đã commit. Trong **Client scopes → Dedicated scope → Scope**, tắt **Full scope allowed**. Client không cần role scope mapping vì quyền nghiệp vụ được HRM nạp từ database; giữ client scope `profile` để token có `preferred_username`.

Tạo người dùng với username trùng chính xác `HrmUserAccounts.Username` trong database HRM. HRM chỉ cho đăng nhập nếu tài khoản local đang hoạt động; dữ liệu phòng ban và vai trò HRM vẫn được lấy từ database để tránh một role trên Keycloak vô tình cấp thêm quyền trong HRM.

## 3. Chạy Keycloak trên Render

Không chạy Keycloak chung đường dẫn với website HRM. Có thể dùng URL mặc định
`https://nhigia-sso.onrender.com` của Render trước; tên miền riêng là tùy chọn
sau khi hệ thống đã hoạt động ổn định.

1. Trong Render, tạo **PostgreSQL** mới (cùng region với HRM). Lưu lại `Internal Database URL`, username, password và database name.
2. Tạo **Web Service** mới, chọn **Existing Image**: `quay.io/keycloak/keycloak:26.8.0`.
3. Đặt service name, ví dụ `nhigia-sso`; Render sẽ cấp URL tạm `https://nhigia-sso.onrender.com`.
4. Trong Environment, thêm các biến sau. Giá trị mật khẩu phải là secret do bạn tự tạo, không dùng ví dụ này cho môi trường thật.

```text
KC_DB=postgres
KC_DB_URL=jdbc:postgresql://<postgres-internal-host>:<port>/<database>
KC_DB_USERNAME=<postgres-username>
KC_DB_PASSWORD=<postgres-password>
KC_BOOTSTRAP_ADMIN_USERNAME=<keycloak-admin-username>
KC_BOOTSTRAP_ADMIN_PASSWORD=<keycloak-admin-password-mạnh>
KC_HTTP_ENABLED=true
KC_HTTP_PORT=10000
KC_PROXY_HEADERS=xforwarded
KC_HOSTNAME=https://nhigia-sso.onrender.com
KC_HEALTH_ENABLED=true
KC_HTTP_MANAGEMENT_HEALTH_ENABLED=false
```

5. Đặt Docker Command:

```text
start
```

6. Trong **Settings > Health Checks**, đặt Path là `/health/ready`.
7. Khi có tên miền riêng, thêm nó tại **Settings > Custom Domains**, tạo DNS record theo giá trị Render hiển thị, bấm Verify rồi đổi `KC_HOSTNAME` sang domain mới và redeploy.

> Không dùng `start-dev` trên Render. Keycloak cần PostgreSQL để dữ liệu người dùng, realm và client không mất khi dịch vụ redeploy.

## 4. Biến môi trường HRM trên Render

Thêm các biến sau vào dịch vụ HRM trên Render:

```text
Authentication__Keycloak__Enabled=true
Authentication__Keycloak__Authority=https://nhigia-sso.onrender.com/realms/hrm-web
Authentication__Keycloak__ClientId=hrm-web
Authentication__Keycloak__ClientSecret=<client-secret-tu-keycloak>
Authentication__Keycloak__RequireHttpsMetadata=true
Authentication__Keycloak__UsernameClaim=preferred_username
```

`RequireHttpsMetadata` mặc định là `true` và phải giữ nguyên ở Render hoặc bất kỳ môi trường không phải Development nào. Chỉ Keycloak cục bộ chạy HTTP mới dùng `Authentication__Keycloak__RequireHttpsMetadata=false` cùng `ASPNETCORE_ENVIRONMENT=Development`.

Sau khi redeploy, URL `/Account/Login` sẽ chuyển sang Keycloak. Callback đăng nhập cố định là `/signin-oidc`; callback đăng xuất là `/signout-callback-oidc`.

## 5. CRM, kế toán và website khác

Mỗi hệ thống là một client riêng trong cùng Realm production `hrm-web`, có redirect URI riêng. Như vậy người dùng đăng nhập Keycloak ở HRM rồi sang CRM/kế toán sẽ không phải nhập lại mật khẩu. Các hệ thống vẫn phải tự kiểm soát quyền nghiệp vụ của chúng.

## Kiểm tra trước khi mở cho toàn bộ nhân viên

1. Đăng nhập bằng một user Keycloak có username khớp user HRM đang hoạt động.
2. Xác nhận vào được trang chủ và đúng vai trò/phòng ban.
3. Thử user không có trong HRM hoặc đã bị khóa: phải bị từ chối.
4. Đăng xuất, sau đó đăng nhập lại để xác nhận phiên SSO và callback hoạt động.
