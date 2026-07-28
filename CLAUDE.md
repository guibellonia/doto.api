# doto.api

Backend do Dotô — app de controle de medicações (agendamentos, registro de doses, sinais vitais,
sintomas e relatórios).

**Estado atual: greenfield.** O projeto foi reiniciado do zero em 2026-07-28. Hoje existe apenas o
esqueleto da solution — nenhuma entidade, service, controller ou migration foi escrita ainda. O
`WeatherForecast` que veio do template é placeholder e deve ser removido assim que o primeiro
endpoint real existir.

## Estrutura

```
Doto.slnx
src/
  Doto.Domain/          entidades, enums, interfaces de repositório — sem dependências
  Doto.Application/     DTOs, interfaces e implementações de service — depende de Domain
  Doto.Infrastructure/  EF Core, repositórios, auth — depende de Domain + Application
  Doto.Api/             controllers, middleware, Program.cs — depende de Application + Infrastructure
reference/              ⚠️ documentação da versão anterior, ver reference/README.md
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

Ainda não há banco nem projeto de auth configurados. O Supabase da versão anterior
(`pvtffkgbyqsqtaxntrgd`) foi deletado. Quando um novo for criado, os segredos vão em
`dotnet user-secrets` — **não** em `appsettings.Development.json`, que é versionado.
