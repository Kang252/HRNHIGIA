# HRM Nhị Gia

Ứng dụng chính hiện tại là `NHIGIA.Modern` (ASP.NET Core .NET 8). Đây cũng là
ứng dụng được build và triển khai trên Render.

## Chạy bằng Visual Studio

1. Mở `NHIGIA.sln`.
2. Chọn cấu hình `Debug` và profile `http`.
3. Nhấn `F5`.

Solution chính chỉ chứa `NHIGIA.Modern`, vì vậy Visual Studio không còn khởi
chạy nhầm ứng dụng ASP.NET Framework cũ. Mã nguồn cũ được lưu riêng trong
`NHIGIA.Legacy.sln` để tham khảo và bảo trì dữ liệu cũ khi cần.

## Chạy bằng dòng lệnh

```powershell
sqllocaldb start NHIGIA
dotnet run --project NHIGIA.Modern --launch-profile http
```

Ứng dụng chạy tại `http://localhost:5116`. Cấu hình Development dùng database
`DEV_NHIGIA` trên SQL Server 2019 LocalDB instance `(localdb)\NHIGIA`.
