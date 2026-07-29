# doto.api

Backend do **Dotô**, app de controle de medicações — agendamentos, registro de doses, sinais
vitais, sintomas e relatórios.

> **Greenfield, com o modelo de dados pronto.** O projeto foi reiniciado do zero em 2026-07-28.
> Hoje existem as entidades, enums, interfaces de repositório e o `DotoDbContext` (14 tabelas,
> migrations já aplicadas no Postgres do Supabase — ver `docs/database-model.md`), mas nenhum
> controller, service, DTO ou endpoint real ainda; o `WeatherForecast` do template continua no
> lugar. A implementação da versão anterior está no histórico do git, no commit `de59481`, e sua
> documentação está em `reference/` (ver `reference/README.md`).

## Stack

.NET 10 · ASP.NET Core Web API (controllers) · Clean Architecture em 4 projetos.

## Estrutura

```
Doto.slnx
src/
  Doto.Domain/          entidades, enums, interfaces de repositório — sem dependências
  Doto.Application/     DTOs, interfaces e implementações de service — → Domain
  Doto.Infrastructure/  EF Core, repositórios, auth — → Domain, Application
  Doto.Api/             controllers, middleware, Program.cs — → Application, Infrastructure
docs/                database-model.md + database-model.mermaid
```

## Rodando

```bash
dotnet run --project src/Doto.Api
```

A URL fica em `src/Doto.Api/Properties/launchSettings.json`. Em Development, o documento OpenAPI
é servido em `/openapi/v1.json`.

## Configuração

O banco (Postgres do Supabase, projeto `yrecrgyecunwheqodlca`) já está modelado e migrado — ver
`docs/database-model.md` para os detalhes e a armadilha do pooler. Resumo:

```bash
dotnet user-secrets set "ConnectionStrings:DotoDb" "<connection string>" --project src/Doto.Api
```

Use o **Session pooler** do Supabase (porta **5432**), não o Transaction pooler (6543) — este
quebra os prepared statements que o Npgsql usa por padrão. Nunca coloque a connection string em
`appsettings.Development.json`, que é versionado.

Migrations não rodam automaticamente no startup (`Program.cs` não chama `Database.Migrate()`).
Para aplicar/atualizar o schema:

```bash
dotnet ef database update --project src/Doto.Infrastructure --startup-project src/Doto.Api
```

**Ainda não há provedor de autenticação configurado.** `app_users.id` já é modelado para ser o
`auth.users.id` do Supabase, mas não existe `AddJwtBearer` nem `ICurrentUserService` ainda.
