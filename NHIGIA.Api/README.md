# NHIGIA.Api

Backend NestJS/TypeScript cho quá trình chuyển dần từ `NHIGIA.Modern` ASP.NET
Core. Nó là một service riêng, không thay thế root Dockerfile hoặc Render service
C# hiện hữu. Dockerfile của thư mục này build và phục vụ React client tại
`/modern-ui/` cùng origin với API.

## Local use

```powershell
Copy-Item .env.example .env
npm ci
npm run build
npm run lint
npm run start:dev
```

With the default `DB_ENABLED=false`, `GET /api/v1/health` is safe to call and
reports that SQL Server is disabled. `GET /api/v1/auth/me` refuses access until
the direct SQL session mode or the optional trusted authentication bridge is
configured. To serve a local React build, set `CLIENT_APP_DIR` to the client
`dist` directory before starting the API.

Before connecting to a real database or publishing a route, read
[`docs/nest-backend-migration.md`](../docs/nest-backend-migration.md). It
includes the additive SQL migration, SQL secret configuration and the separate
Render blueprint. Production requires a SQL-login string with certificate
verification; `TrustServerCertificate=True` is rejected by design.

With `AUTH_MODE=session`, apply `sql/001_api_sessions.sql` first. Startup
checks the required session schema rather than waiting for the first login to
fail. Cookie login/logout only accepts same-origin XMLHttpRequest traffic;
serve the React bundle from this service in production or use the configured
Vite development proxy locally.

## Current API surface

- Authentication and identity: `/api/v1/auth/*`
- Dashboard: `/api/v1/dashboard`
- Employee directory and protected avatars: `/api/v1/users/*`
- Internal communications: `/api/v1/communications/*` (bài đăng, duyệt,
  tương tác và bình luận văn bản)
- Personal notifications: `/api/v1/notifications/*`
- Attendance and attendance-period confirmation: `/api/v1/attendance/*`
- Read-only employee procedures: `/api/v1/procedures/*`
- Remote-work plans and manual review: `/api/v1/remote-attendance/*`

`/api/v1/remote-attendance` is a deliberately limited migration slice. It
lists and creates offsite-work plans, lets authorized people review/cancel plans
and manually review existing punches. Camera uploads, photos, face-enrollment
images and OpenCV comparison remain in `NHIGIA.Modern`; the Nest API neither
accepts nor exposes biometric images, and it never automatically approves a
punch from a face signal.

Before a feature query, notifications, communications, attendance, procedures
and remote attendance
probe their required SQL tables and columns for 30 seconds. Schema drift returns
an actionable `503` rather than leaking an `Invalid column name` SQL error.
The attendance, procedures and remote-attendance routes use the same role scope as the legacy
application, but they still require live-SQL comparison before a production UI
is switched to them. See the migration guide for the verification checklist.
