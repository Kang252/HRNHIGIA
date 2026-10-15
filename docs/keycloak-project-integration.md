# Keycloak cho HRM Nhị Gia

Keycloak là dịch vụ quản lý danh tính. Nó xác thực người dùng; HRM vẫn là nơi
quyết định quyền nghiệp vụ, phòng ban và trạng thái tài khoản từ bảng
`HrmUserAccount`.

`keycloak-main` được cung cấp kèm dự án là mã nguồn phát triển của Keycloak,
không phải bản cài để chạy. Dùng bản phát hành Keycloak hoặc image chính thức,
không build thư mục đó để vận hành.

## Luồng đăng nhập

1. Người dùng mở HRM và được chuyển đến Keycloak.
2. Keycloak xác thực người dùng rồi trả về mã OIDC cho HRM.
3. HRM kiểm tra claim `preferred_username`.
4. HRM tìm tài khoản local cùng username và chỉ cho vào nếu `IsActive = 1`.
5. Vai trò và phòng ban luôn lấy từ SQL HRM, không lấy role Keycloak để cấp
   quyền nghiệp vụ.

Vì vậy username tại Keycloak phải trùng chính xác `HrmUserAccount.Username`.
Không nhập password hash từ SQL HRM sang Keycloak; hai định dạng credential
không tương thích.

## Chạy thử theo cách cài Keycloak trên Windows

1. Tải bản Keycloak phát hành từ <https://www.keycloak.org/downloads>, giải nén
   ví dụ vào `C:\Tools\keycloak-26.8.0`. Thư mục phải có `bin\kc.bat`.
2. Mở PowerShell trong thư mục đó và đặt tài khoản quản trị chỉ cho máy local:

```powershell
$env:KC_BOOTSTRAP_ADMIN_USERNAME = 'admin'
$env:KC_BOOTSTRAP_ADMIN_PASSWORD = '<mat-khau-manh-rieng-cho-local>'
.\bin\kc.bat start-dev --http-port=8081
```

3. Mở `http://localhost:8081/admin`, đăng nhập bằng tài khoản quản trị vừa tạo.
   `start-dev` chỉ dùng để học và kiểm thử, không dùng trên Render.

## Tạo realm, client và người dùng local

Trong Admin Console:

1. Chọn **Create realm** và đặt tên `nhigia`.
2. Vào **Clients → Create client**:
   - Client type: `OpenID Connect`
   - Client ID: `hrm-web-local`
   - Client authentication: tắt (public client cho máy local)
   - Standard flow: bật
   - Valid redirect URIs: `http://localhost:5116/signin-oidc`
   - Valid post logout redirect URIs: `http://localhost:5116/signout-callback-oidc`
   - Web origins: `http://localhost:5116`
   - PKCE code challenge method: `S256`
3. Vào **Users → Add user**, đặt username trùng đúng tài khoản HRM đang hoạt
   động. Tại tab **Credentials**, đặt mật khẩu tạm và bật **Temporary** để
   người dùng đổi mật khẩu ở lần đăng nhập đầu.
4. Client scope `profile` mặc định trả `preferred_username`. Có thể kiểm tra ở
   **Client scopes → Evaluate** bằng ID token hoặc UserInfo. Nếu claim bị tắt,
   thêm mapper kiểu **User Property**: property `username`, claim name
   `preferred_username`, bật cho ID token và UserInfo.

## Chạy HRM với Keycloak local

Mở một PowerShell khác tại thư mục gốc dự án:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Authentication__Keycloak__Enabled = 'true'
$env:Authentication__Keycloak__Authority = 'http://localhost:8081/realms/nhigia'
$env:Authentication__Keycloak__ClientId = 'hrm-web-local'
$env:Authentication__Keycloak__RequireHttpsMetadata = 'false'
$env:Authentication__Keycloak__UsernameClaim = 'preferred_username'
Remove-Item Env:Authentication__Keycloak__ClientSecret -ErrorAction SilentlyContinue
dotnet run --project .\NHIGIA.Modern --launch-profile http
```

Mở `http://localhost:5116/Account/Login`. Nếu username không có trong HRM,
không hoạt động hoặc sai claim, Keycloak có thể xác thực thành công nhưng HRM
sẽ từ chối phiên.

## Chạy bằng cấu hình có sẵn trong dự án

Ngoài cách cài Windows ở trên, repository có bộ Docker Compose local:

```powershell
Copy-Item .\infra\keycloak\.env.example .\infra\keycloak\.env
# Thay hai mật khẩu change-this... trong .env
.\scripts\start-keycloak.ps1
```

Compose tạo Keycloak và PostgreSQL riêng, import realm `nhigia` với client
`hrm-web-local`. Dữ liệu nằm trong Docker volume. Dừng dịch vụ mà vẫn giữ dữ
liệu bằng `./scripts/stop-keycloak.ps1`; chỉ dùng `-RemoveData` khi thật sự muốn
xóa toàn bộ identity data local.

## Triển khai thật trên Render

Keycloak phải là Web Service riêng, có PostgreSQL riêng và một URL HTTPS công
khai. Có thể dùng URL mặc định của Render, ví dụ
`https://nhigia-sso.onrender.com`, trước khi mua hoặc cấu hình tên miền riêng.
Không chạy Keycloak chung Docker image hay chung database với HRM.

1. Tạo PostgreSQL trên Render cùng region với service Keycloak.
2. Tạo Web Service từ image `quay.io/keycloak/keycloak:26.8.1`, dùng Docker
   Command `/opt/keycloak/bin/kc.sh start --verbose` và service port `10000`.
   Image chính thức không có CMD mặc định; với service **Existing Image** trên
   Render cần dùng đường dẫn lệnh đầy đủ như trên. Nếu triển khai từ
   `infra/keycloak/Dockerfile`, image đã có lệnh mặc định này; để Docker
   Command trống hoặc đặt đúng `start`.
3. Chọn compute `1c-2g` (1 CPU, 2 GB RAM) cho production. Gói Free 512 MB có
   thể hết bộ nhớ trong lần khởi tạo database/realm. Đặt tối thiểu các biến môi
   trường sau trong service Keycloak. Giá trị nằm
   trong Render Secrets, không đưa vào Git hay chat.

```text
KC_DB=postgres
KC_DB_URL_HOST=<internal-db-host>
KC_DB_URL_PORT=5432
KC_DB_URL_DATABASE=<database>
KC_DB_USERNAME=<db-user>
KC_DB_PASSWORD=<db-password>
KC_BOOTSTRAP_ADMIN_USERNAME=<admin-user>
KC_BOOTSTRAP_ADMIN_PASSWORD=<admin-password-manh>
KC_HTTP_ENABLED=true
KC_HTTP_PORT=10000
KC_PROXY_HEADERS=xforwarded
KC_HOSTNAME=https://<ten-service>.onrender.com
KC_HEALTH_ENABLED=true
KC_HTTP_MANAGEMENT_HEALTH_ENABLED=false
```

4. Đặt Health Check Path của Render là `/health/ready`. Khi endpoint trả
   `status: UP`, đăng nhập Admin Console. Production sử dụng realm `hrm-web`;
   realm `nhigia` trong phần trên chỉ dành cho môi trường local. Nếu realm
   `hrm-web` chưa tồn tại, mở
   **Manage realms → Create realm** và import
   `infra/keycloak/templates/hrm-web-render-realm.json`. Nếu realm đã tồn tại,
   mở **Clients → Import client** và chọn
   `infra/keycloak/templates/hrm-web-render-client.json`. Các tệp này không
   chứa user, mật khẩu hoặc client secret cố định. Sau import, kiểm tra discovery
   document: `https://<ten-service>.onrender.com/realms/hrm-web/.well-known/openid-configuration`.
   Có thể thay URL này bằng custom domain sau khi đã mua tên miền và Render báo
   xác thực thành công.
5. Xác nhận client `hrm-web` đã bật Standard flow, Client authentication và
   PKCE S256. Trong **Client scopes → Dedicated scope → Scope**, tắt
   **Full scope allowed**. HRM tự lấy vai trò nghiệp vụ từ database nên client
   này không cần ánh xạ role Keycloak; giữ client scope `profile` để phát hành
   claim `preferred_username`. Redirect URI phải là
   `https://hrnhigia.onrender.com/signin-oidc`, post logout redirect URI là
   `https://hrnhigia.onrender.com/signout-callback-oidc`, và web origin là
   `https://hrnhigia.onrender.com`.
6. Sao chép **Client secret** được Keycloak tạo ở
   **Clients → hrm-web → Credentials** vào Render
   secrets của service HRM, cùng các biến sau:

```text
Authentication__Keycloak__Enabled=true
Authentication__Keycloak__Authority=https://<ten-service>.onrender.com/realms/hrm-web
Authentication__Keycloak__ClientId=hrm-web
Authentication__Keycloak__ClientSecret=<client-secret>
Authentication__Keycloak__RequireHttpsMetadata=true
Authentication__Keycloak__UsernameClaim=preferred_username
```

7. Redeploy HRM. Thử trước với một tài khoản active có username khớp. Chỉ sau
   khi đăng nhập, logout và tài khoản bị khóa đều cho kết quả đúng mới mở cho
   toàn bộ nhân viên.

## Vận hành

- **Realm**: vùng quản lý riêng cho Nhị Gia, chứa user, client, role và policy.
- **Client**: một ứng dụng kết nối Keycloak. HRM, CRM và website nên là các
  client riêng, không dùng chung secret.
- **Client secret**: mật khẩu của ứng dụng confidential, không phải mật khẩu
  người dùng. Khi lộ phải rotate trong Keycloak rồi cập nhật Render ngay.
- **preferred_username**: claim tên đăng nhập; không phải secret.
- **Role Keycloak**: chỉ dùng quản trị SSO trong giai đoạn này. Role HRM tiếp
  tục lấy từ database HRM.
- **Realm import**: chỉ tiện để bootstrap realm mới; import lại không phải cách
  cập nhật realm đã tồn tại. Các thay đổi sau đó thực hiện qua Admin Console
  hoặc Admin API.

## Sự cố thường gặp

| Hiện tượng | Kiểm tra |
| --- | --- |
| `invalid_redirect_uri` | Đối chiếu từng ký tự callback, protocol và port trong client Keycloak. |
| Metadata HTTPS error ở local | Chỉ ở Development đặt `RequireHttpsMetadata=false`; không áp dụng lên Render. |
| Keycloak login xong nhưng HRM từ chối | Username Keycloak phải trùng local và tài khoản HRM phải active. |
| `preferred_username` thiếu | Kiểm tra client scope `profile` hoặc mapper User Property. |
| Cảnh báo `Full scope allowed` | Vào **Clients → hrm-web → Client scopes → Dedicated scope → Scope**, tắt **Full scope allowed**. Không thêm role scope mapping khi HRM vẫn lấy quyền từ database. |
| Logout quay về Keycloak | Kiểm tra post logout redirect URI `/signout-callback-oidc`. |
| Không thấy client secret | Client authentication phải bật; public client không có secret. |
