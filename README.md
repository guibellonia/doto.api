# doto.api

Backend do **Dotô**, app de controle de medicações — agendamentos, registro de doses, sinais
vitais, sintomas e relatórios.

> **Greenfield.** O projeto foi reiniciado do zero em 2026-07-28. Hoje existe apenas o esqueleto
> da solution; o `WeatherForecast` do template ainda está no lugar. A implementação da versão
> anterior está no histórico do git, no commit `de59481`, e sua documentação está em
> `reference/` (ver `reference/README.md`).

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
```

## Rodando

```bash
dotnet run --project src/Doto.Api
```

A URL fica em `src/Doto.Api/Properties/launchSettings.json`. Em Development, o documento OpenAPI
é servido em `/openapi/v1.json`.

## Configuração

Ainda não há banco nem provedor de autenticação configurados. Quando houver, os segredos vão em
`dotnet user-secrets` — **nunca** em `appsettings.Development.json`, que é versionado.
