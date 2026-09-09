# HRM Nhị Gia Modern

ASP.NET Core MVC .NET 8 migration host for the existing HRM system. The app now includes cookie authentication, role-based access, dashboard, employee profile, attendance, work schedules, leave requests and approvals, internal communications, reports, and the optional HANET integration.

## Run locally

```powershell
dotnet run --urls http://localhost:5090
```

Open `http://localhost:5090`. Unauthenticated users are redirected to `/Account/Login`; use `/Health/Database` to check the configured SQL Server connection.

The development connection targets `DEV_NHIGIA` on the standard LocalDB instance `(localdb)\MSSQLLocalDB`. Initialize it from the repository root:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "IF DB_ID('DEV_NHIGIA') IS NULL CREATE DATABASE DEV_NHIGIA"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d DEV_NHIGIA -E -b -i "NHIGIA.Modern\App_Data\hrm-mvp.sql"
```

For deployment, provide `ConnectionStrings__MainConnectionString` as an environment variable.

## Docker

```powershell
docker build -t hrm-nhigia-modern .
docker run --rm -p 8080:8080 -e ConnectionStrings__MainConnectionString="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;" hrm-nhigia-modern
```

The original ASP.NET MVC 5 solution remains available as the migration source. Older Kendo-based employee administration, import, evaluation, rewards, incidents, business-trip, and resignation screens still require a separate migration before they can run on Linux.

## Role-based interface

The navigation and dashboard adapt to the signed-in account:

| Role | Interface scope |
| --- | --- |
| Employee | Personal profile, attendance, leave, KPI, published payslips, assigned training/assets, overtime, resignation and Helpdesk requests |
| Manager | Department employees, schedules, leave approvals, department KPI/training/overtime/resignation/assets, plus personal payslips |
| HR | Company employee operations, final leave approval, payroll preparation, recruitment, training, transfers, assets and reports |
| Director | Company overview, reports, KPI and approval-focused payroll/recruitment/transfer views |
| Admin | System-wide operational access to all HR modules, assets and Helpdesk |

Managers are restricted to employees in their own department. Their payroll view remains personal and only includes published records.

## KPI and internal services

The sidebar and mobile navigation include `/Work?kind=kpi`, `payroll`, `recruitment`, `training`, `overtime`, `resignation`, `transfer`, `assets`, and `helpdesk`.
These pages list and create SQL-backed records in `dbo.HrmWorkItem`; no demo records are shown.
HR, directors and admins can create KPI records, transfer proposals and assets. Employees can view their own records and submit IT requests.
Payroll drafts are created and submitted by HR, approved by directors, then locked, published and marked paid by HR. Admins can perform either side for operational recovery. Employees and managers can open Payroll but only see their own published records and can report a discrepancy. Recruitment remains restricted to HR, directors and admins. Managers can create training plans and review overtime/resignation requests; employees can submit their own overtime and resignation requests.
KPI progress is actual / target; weight is recorded but no aggregate appraisal score is calculated.
Transfers are proposals only and do not update employee departments. Asset records capture the initial assignee.
Editing, transfer approval, asset return history, ticket assignment and resolution are not yet implemented.
Existing vehicle/meeting-room booking source was not found in this checkout and has not been duplicated.

Set `HRM_CONNECTION_STRING` to the real SQL Server connection string in the deployment environment. This takes precedence over `ConnectionStrings__MainConnectionString`.
Startup runs `App_Data/hrm-mvp.sql`, which creates missing tables and includes the existing HRM seed data.
Without a working database the new pages display a connection error and disable submission.

Run `dotnet build NHIGIA.Modern -c Release`, then `dotnet run --project tests/WorkModules.Smoke` from the repository root.
The smoke check uses loopback and synthetic local authentication to verify page rendering, role restrictions, missing-database handling and anti-forgery enforcement; it does not verify SQL writes.

## Render deployment

The root `render.yaml` deploys the root Dockerfile with `/Health` as its liveness check.
Set `ConnectionStrings__MainConnectionString` in Render Environment using the legacy SQL Server host, database and SQL login. Do not use LocalDB or `CHANGE_ME` on Render.
Use explicit `Encrypt=True;TrustServerCertificate=True` for the existing hosting configuration. SQL credentials remain in Render and the ignored local Production settings; they are excluded from Docker builds.
`HRM_CONNECTION_STRING`, when non-empty, overrides this variable; remove any stale override.
Validate `/Health/Database` after deployment. `/Health` alone does not establish SQL connectivity.
Schema creation preserves existing rows and does not rename existing departments or accounts on startup. New profile seeds use Nhị Gia identifiers.
Reference: https://render.com/docs/configure-environment-variables and https://learn.microsoft.com/en-us/sql/connect/ado-net/connection-string-syntax
