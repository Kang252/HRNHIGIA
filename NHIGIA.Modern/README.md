# HRM Nhị Gia Modern

ASP.NET Core MVC .NET 8 migration host for the existing HRM system. The app now includes cookie authentication, role-based access, dashboard, employee profile, attendance, work schedules, leave requests and approvals, internal communications, reports, and the optional HANET integration.

## Run locally

```powershell
dotnet run --urls http://localhost:5090
```

Open `http://localhost:5090`. Unauthenticated users are redirected to `/Account/Login`; use `/Health/Database` to check the configured SQL Server connection.

The default development connection targets the existing LocalDB database `DEV_NHIGIA`. For deployment, provide `ConnectionStrings__MainConnectionString` as an environment variable.

## Docker

```powershell
docker build -t hrm-nhigia-modern .
docker run --rm -p 8080:8080 -e ConnectionStrings__MainConnectionString="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;" hrm-nhigia-modern
```

The original ASP.NET MVC 5 solution remains available as the migration source. Older Kendo-based employee administration, import, evaluation, rewards, incidents, business-trip, and resignation screens still require a separate migration before they can run on Linux.
