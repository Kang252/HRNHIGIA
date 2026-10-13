# Cấu hình đăng nhập tập trung bằng Keycloak

## 1. Khởi động Keycloak

Từ thư mục `C:\Users\tranp\Downloads\keycloak-26.8.0\keycloak-26.8.0`, chạy thử nghiệm cục bộ:

```powershell
$env:KC_BOOTSTRAP_ADMIN_USERNAME = "admin"
$env:KC_BOOTSTRAP_ADMIN_PASSWORD = "đặt-mật-khẩu-mạnh"
.\bin\kc.bat start-dev --http-port=8081
```

Mở `http://localhost:8081`, đăng nhập quản trị và tạo Realm tên `nhigia`.

> `start-dev` chỉ dùng trên máy phát triển. Khi triển khai thật, Keycloak phải chạy thành một dịch vụ riêng có tên miền HTTPS, cơ sở dữ liệu PostgreSQL và biến môi trường quản trị mạnh.

## 2. Tạo client cho HRM

Trong Realm `nhigia`, tạo client OpenID Connect với các giá trị:

| Thuộc tính | Giá trị |
| --- | --- |
| Client ID | `hrm-web` |
| Client authentication | Bật (confidential client) |
| Standard flow | Bật |
| Valid redirect URIs | `https://hrnhigia.onrender.com/signin-oidc` |
| Valid post logout redirect URIs | `https://hrnhigia.onrender.com/*` |
| Web origins | `https://hrnhigia.onrender.com` |

Sao chép **Client secret** ở tab Credentials. Không đưa secret vào Git hay file `appsettings.json` đã commit.

Tạo người dùng với username trùng chính xác `HrmUserAccounts.Username` trong database HRM. HRM chỉ cho đăng nhập nếu tài khoản local đang hoạt động; dữ liệu phòng ban và vai trò HRM vẫn được lấy từ database để tránh một role trên Keycloak vô tình cấp thêm quyền trong HRM.

## 3. Chạy Keycloak trên tên miền bằng Render

Khuyến nghị dùng subdomain riêng, ví dụ `sso.ten-mien-cua-ban`. Không chạy Keycloak chung đường dẫn với website HRM.

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
```

5. Đặt Docker Command:

```text
start
```

6. Trong Settings > Custom Domains của service Keycloak, thêm `sso.ten-mien-cua-ban`. Tại nhà cung cấp tên miền, tạo DNS record đúng giá trị Render hiển thị, rồi bấm Verify trên Render. Render tự cấp và gia hạn TLS/HTTPS.
7. Chỉ sau khi Render báo domain đã verified, đổi `KC_HOSTNAME` thành `https://sso.ten-mien-cua-ban` và redeploy service.

> Không dùng `start-dev` trên Render. Keycloak cần PostgreSQL để dữ liệu người dùng, realm và client không mất khi dịch vụ redeploy.

## 4. Biến môi trường HRM trên Render

Thêm các biến sau vào dịch vụ HRM trên Render:

```text
Authentication__Keycloak__Enabled=true
Authentication__Keycloak__Authority=https://sso.ten-mien-cua-ban/realms/nhigia
Authentication__Keycloak__ClientId=hrm-web
Authentication__Keycloak__ClientSecret=<client-secret-tu-keycloak>
Authentication__Keycloak__UsernameClaim=preferred_username
```

Sau khi redeploy, URL `/Account/Login` sẽ chuyển sang Keycloak. Callback cố định là `/signin-oidc`.

## 5. CRM, kế toán và website khác

Mỗi hệ thống là một client riêng trong cùng Realm `nhigia`, có redirect URI riêng. Như vậy người dùng đăng nhập Keycloak ở HRM rồi sang CRM/kế toán sẽ không phải nhập lại mật khẩu. Các hệ thống vẫn phải tự kiểm soát quyền nghiệp vụ của chúng.

## Kiểm tra trước khi mở cho toàn bộ nhân viên

1. Đăng nhập bằng một user Keycloak có username khớp user HRM đang hoạt động.
2. Xác nhận vào được trang chủ và đúng vai trò/phòng ban.
3. Thử user không có trong HRM hoặc đã bị khóa: phải bị từ chối.
4. Đăng xuất, sau đó đăng nhập lại để xác nhận phiên SSO và callback hoạt động.
