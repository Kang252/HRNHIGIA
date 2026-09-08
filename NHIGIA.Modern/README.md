# HRM Nhị Gia Modern

ASP.NET Core MVC .NET 8 migration host for the existing HRM system.

## Run locally

```powershell
dotnet run --urls http://localhost:5090
```

Open `http://localhost:5090` and use `/Health/Database` to check the configured SQL Server connection.

The default development connection targets the existing LocalDB database `DEV_NHIGIA`. For deployment, provide `ConnectionStrings__MainConnectionString` as an environment variable or update `appsettings.Production.json` with the Somee SQL Server details.

## Docker

```powershell
docker build -t hrm-nhigia-modern .
docker run --rm -p 8080:8080 -e ConnectionStrings__MainConnectionString="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;" hrm-nhigia-modern
```

The original ASP.NET MVC 5 solution remains unchanged and can continue running while modules are migrated into this project.
