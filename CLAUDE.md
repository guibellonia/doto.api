# doto.api

.NET 10 backend for Doto, Clean Architecture across 4 projects. See the workspace root
`../CLAUDE.md` for naming conventions and `../.claude/rules/backend-conventions.md` for the full
conventions/guardrails this file summarizes.

## Stack

- **.NET 10**, ASP.NET Core Web API, minimal hosting model (`Program.cs`, no `Startup.cs`).
- **EF Core 10 + Npgsql** — Postgres hosted on Supabase. Heavy `HasConversion` usage to force UTC
  `DateTime`/`DateTimeOffset` and map `DateOnly`/`TimeOnly`.
- **Auth**: Supabase JWT bearer (`AddJwtBearer` validated against Supabase's issuer/JWKS), no
  ASP.NET Identity. `ICurrentUserService` reads claims off `HttpContext.User`.
- **SignalR** (`NotificationHub` at `/hubs/notifications`) for real-time medication reminder/taken
  events.
- **Swashbuckle.AspNetCore 10.x** (OpenAPI.NET v2 — types live in the `Microsoft.OpenApi` namespace,
  not `Microsoft.OpenApi.Models`; security requirements use the
  `document => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(...)] = [...] }`
  delegate form, not the old `Reference`/`ReferenceType` pattern).
- No test project exists yet. No MediatR/CQRS, no AutoMapper, no FluentValidation actually wired up
  (the package is referenced but unused).

## Architecture (4 projects under `src/`)

```
Doto.Domain          entities, enums, repository interfaces — no dependencies
Doto.Application     DTOs, service interfaces + implementations (plain service layer, not CQRS)
Doto.Infrastructure   DotoDbContext, repositories, Supabase/auth glue, DI registration
Doto.Api             controllers, middleware, SignalR hub, Program.cs composition root
```

Full conventions (naming, layering rules, what not to introduce) are in
`../.claude/rules/backend-conventions.md`.

## Request logging (doto.monitor)

`Middleware/RequestLoggingMiddleware.cs` mirrors every request/response to `doto.monitor` when
`Monitor:Url` is configured in `appsettings` (empty by default = no-op, zero overhead). Secrets are
redacted before leaving the process — see `../.claude/rules/security-guardrails.md`.

## Known gaps (flagged, not silently fixed)

- No global exception-handling middleware — each controller try/catches specific exceptions.
- `DevPolicy` CORS allows any origin/method/header, applied unconditionally.
- `appsettings.Development.json` has real Supabase secrets committed to git — needs rotation, see
  `docs/DEPLOYMENT.md`.
- Azure Web App hosting was removed; see `docs/DEPLOYMENT.md` for the migration plan to a free
  alternative (Render recommended).

## Commands you'll actually run

```bash
dotnet build
dotnet ef migrations list --project src/Doto.Infrastructure --startup-project src/Doto.Api
dotnet run --project src/Doto.Api
```
