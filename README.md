# HRM Nhị Gia

Ứng dụng chính hiện tại là `NHIGIA.Modern` (ASP.NET Core .NET 10 LTS). Đây cũng là
ứng dụng được build và triển khai trên Render.

## Chuyển đổi TypeScript

`NHIGIA.Api` là backend NestJS/TypeScript chạy song song, còn
`NHIGIA.Modern/ClientApp` là giao diện React + TypeScript mới. Hai phần này không
thay đổi root Dockerfile hoặc Render service C# hiện hữu. Slice đầu tiên đã có
đăng nhập phiên opaque, dashboard, truyền thông nội bộ, danh bạ nhân sự và ảnh
đại diện theo phạm vi quyền; những nghiệp vụ còn lại vẫn do C# xử lý đến khi đối
chiếu dữ liệu và quyền hoàn tất.

Hướng dẫn cấu hình SQL, migration phiên và mẫu Render riêng nằm tại
[`docs/nest-backend-migration.md`](docs/nest-backend-migration.md). Không tạo
service NestJS công khai trước khi chạy migration `HrmApiSession` và cấu hình
secret SQL Server trên service mới.

## Chạy bằng Visual Studio

1. Mở `NHIGIA.sln`.
2. Chọn cấu hình `Debug` và profile `http`.
3. Nhấn `F5`.

Solution chính chỉ chứa `NHIGIA.Modern`, vì vậy Visual Studio không còn khởi
chạy nhầm ứng dụng ASP.NET Framework cũ. Mã nguồn cũ được lưu riêng trong
`NHIGIA.Legacy.sln` để tham khảo và bảo trì dữ liệu cũ khi cần.

## Chạy bằng dòng lệnh

Cài .NET 10 SDK trước khi chạy ứng dụng. Image Docker dùng .NET 10 runtime.

```powershell
sqllocaldb start MSSQLLocalDB
dotnet run --project NHIGIA.Modern --launch-profile http
```

Ứng dụng chạy tại `http://localhost:5116`. Cấu hình Development dùng database
`DEV_NHIGIA` trên instance chuẩn `(localdb)\MSSQLLocalDB`.
