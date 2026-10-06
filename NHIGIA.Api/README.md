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
