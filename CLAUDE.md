# doto.api

Backend do Dotô — app de controle de medicações (agendamentos, registro de doses, sinais vitais,
sintomas e relatórios).

**Estado atual: modelo de dados pronto, resto ainda greenfield.** O projeto foi reiniciado do zero
em 2026-07-28. O primeiro plano executado (`database-model-backend-plan.md`) modelou o banco
completo: `Doto.Domain` tem as 14 entidades ricas, 18 enums e as interfaces de repositório (só
interfaces, sem implementação); `Doto.Infrastructure` tem `DotoDbContext`, as 14
`IEntityTypeConfiguration` e duas migrations (`InitialCreate`, `AddAuthUsersForeignKey`) já
aplicadas no Postgres do Supabase. Ver `docs/database-model.md` e `docs/database-model.mermaid`.

Ainda **não existe** nenhum controller, service, DTO ou endpoint real — nem implementação de
repositório, nem auth (`AddJwtBearer`/`ICurrentUserService`), nem `BaseResponse<T>`. O
`WeatherForecast` que veio do template continua no lugar e só deve ser removido junto com o
primeiro endpoint real.

## Estrutura

```
Doto.slnx
src/
  Doto.Domain/          entidades, enums, interfaces de repositório — sem dependências
  Doto.Application/     DTOs, interfaces e implementações de service — depende de Domain
  Doto.Infrastructure/  EF Core, repositórios, auth — depende de Domain + Application
  Doto.Api/             controllers, middleware, Program.cs — depende de Application + Infrastructure
docs/                database-model.md + database-model.mermaid (modelo de dados atual)
reference/           ⚠️ documentação da versão anterior, ver reference/README.md
```

As referências entre projetos já estão ligadas nessa direção. Manter o sentido das setas.

## Convenções

As convenções de arquitetura, nomenclatura e guardrails vivem no workspace pai, em
`../.claude/rules/backend-conventions.md`. Elas descrevem o alvo desta reconstrução, não o estado
atual — vários itens ("o codebase tem X", "não renomear Y") se referem à versão anterior e só
voltam a valer conforme o código for sendo escrito.

## `reference/`

Contém a documentação da versão anterior, preservada como **base of work**. Serve para relembrar
decisões de modelagem e escopo funcional — **não** para copiar literalmente nem para ser tratada
como descrição do estado atual. Ver `reference/README.md`.

O código-fonte da versão anterior está no histórico do git, no commit `de59481`.

## Infraestrutura

Há um projeto Supabase Postgres novo (`yrecrgyecunwheqodlca`) com o schema `public` já migrado —
ver `docs/database-model.md` para a connection string (Session pooler, porta 5432) e o comando de
`dotnet ef database update`. O Supabase da versão anterior (`pvtffkgbyqsqtaxntrgd`) continua
deletado e é irrelevante. **Ainda não há provedor de auth configurado** — `app_users.id` já espera
ser o `auth.users.id` do Supabase, mas `AddJwtBearer`/`ICurrentUserService` não existem. Segredos
vão em `dotnet user-secrets` — **não** em `appsettings.Development.json`, que é versionado.
