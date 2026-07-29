# Modelo de dados

Companion em prosa de [`database-model.mermaid`](./database-model.mermaid) (14 tabelas, `schema
public`). O diagrama tem a lista completa de colunas, tipos e relacionamentos — este documento não
a repete; explica as decisões que não ficam óbvias só olhando o ER.

Implementado por: `src/Doto.Domain/Entities|Enums|Interfaces`, `src/Doto.Infrastructure/Persistence`
(`DotoDbContext`, `Configurations/`, `Migrations/`). Só modelo de dados — **não existe** ainda
controller, service, DTO, endpoint, implementação de repositório ou auth (`ICurrentUserService`).
Ver `../CLAUDE.md`.

## Convenções de nomeação

- **Banco**: `snake_case` plural (`app_users`, `dose_occurrences`). `user` é palavra reservada no
  Postgres, daí `app_users`.
- **C#**: `PascalCase` de sempre, mapeado por `EFCore.NamingConventions`
  (`UseSnakeCaseNamingConvention()` em `DotoDbContextFactory`/`AddInfrastructure`) — não há um
  helper manual de conversão de nome, o pacote tem release compatível com EF Core 10.
- A entidade de usuário chama-se **`AppUser`**, não `User`. `User` colidiria com
  `ControllerBase.User` (`ClaimsPrincipal`) e viraria fonte de bug silencioso assim que os
  controllers existirem.

## Enums como `varchar`

Todo enum é persistido via `HasConversion<string>()`, não como `int` nem como enum nativo do
Postgres. Motivos: legibilidade direta no SQL Editor do Supabase (que vai ser usado na banca do
TCC), e adicionar um valor novo não renumera nada nem exige alterar um tipo nativo do Postgres por
migration. O custo é armazenamento um pouco maior — aceito.

## `app_users.id` é `auth.users.id`

Não existe coluna `supabase_user_id` separada: a PK de `app_users` **é** a PK de `auth.users`
(schema gerenciado pelo Supabase Auth). Isso elimina uma classe inteira de bug de dessincronia
entre as duas tabelas.

Consequências no código:

- `AppUser.Id` é `ValueGeneratedNever()` — o banco não gera; o valor vem da Supabase Admin API no
  signup/convite. Todas as outras entidades continuam com `DEFAULT gen_random_uuid()` (útil para
  quem inserir direto pelo SQL Editor; o EF sempre manda o valor explícito).
- `DotoDbContext` **não tem `DbSet` para `auth.users`** — nenhuma entidade, nenhuma navegação. O EF
  não sabe que o schema `auth` existe, então nenhuma migration tenta criar, alterar ou dropar algo
  nele.
- A integridade referencial real contra `auth.users` **não é gerenciada pelo EF**: entra por SQL
  cru numa migration própria, `AddAuthUsersForeignKey`, separada de `InitialCreate`:
  ```sql
  ALTER TABLE public.app_users
  ADD CONSTRAINT fk_app_users_auth_users
  FOREIGN KEY (id) REFERENCES auth.users (id)
  ON DELETE NO ACTION;
  ```
  Migration separada de propósito: se a role de migração não tiver `REFERENCES` em `auth.users`,
  dá para reverter/pular só esta e o resto do schema continua íntegro. O `ModelSnapshot` do EF não
  contém essa constraint — é esperado, já que o EF nunca a viu.
- **`ON DELETE NO ACTION`, não `CASCADE`.** Cascade apagaria `app_users` — e, em cascata,
  medicações e todo o histórico de doses — quando alguém apertasse "delete user" no dashboard do
  Supabase. Com `NO ACTION` isso falha ruidosamente, que é o comportamento certo para dado de
  saúde. **Consequência**: exclusão de conta passa pela nossa API (soft delete + anonimização),
  nunca pelo dashboard do Supabase — apagar um usuário de teste durante o desenvolvimento exige
  apagar a linha em `public.app_users` antes.
- `app_users.email` é um espelho lowercase de `auth.users.email`. `auth.users` é a fonte da
  verdade; nada no código atual sincroniza o espelho ainda (isso é do plano de auth — o plano
  original registra que deve ler a claim `email` do JWT a cada request/login).

## Timestamps: instante vs. relógio de parede

A armadilha de correção mais séria do modelo, e a mais fácil de violar sem perceber.

- **Instante** (aconteceu num ponto do tempo) → `timestamptz` / `DateTime` UTC. Ex.:
  `taken_at_utc`, `created_at`, `sent_at_utc`, `measured_at_utc`.
- **Relógio de parede** (é um conceito local, não um ponto fixo no tempo) → `date` + `time`
  separados / `DateOnly` + `TimeOnly`. Ex.: `scheduled_local_date` + `scheduled_local_time`,
  `schedule_time_slots.time_of_day`, `quiet_hours_start/end`.

"Tomar às 8h da manhã" não é um instante — é uma intenção local. Guardar só o UTC equivalente faz
uma mudança de fuso (viagem) ou de DST transformar silenciosamente "8h" em "7h" ou "9h". É por isso
que `dose_occurrences` carrega as duas representações (ver "Ocorrências versionadas" abaixo), e por
que `app_users.time_zone` (IANA, default `America/Sao_Paulo`) é sempre o fuso **do sujeito do
dado** — nunca o do chamador da API.

A conversão UTC é global, não por propriedade, em `DotoDbContext.ConfigureConventions`:

```csharp
configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
```

Motivo de ser global: o Npgsql moderno lança exceção ao gravar um `DateTime` com
`Kind != Utc` num `timestamptz`, e uma convenção global cobre 100% das colunas — inclusive
qualquer uma que alguém adicionar depois e esquecer de configurar individualmente. `DateOnly` e
`TimeOnly` mapeiam nativamente para `date`/`time` no Npgsql atual, sem converter nenhum.

## Soft delete e a armadilha do `IgnoreQueryFilters()`

`ISoftDeletable` (`Doto.Domain/Common/ISoftDeletable.cs`) marca as entidades que têm `deleted_at`.
`DotoDbContext.OnModelCreating` aplica `HasQueryFilter(e => e.DeletedAt == null)` a todo tipo que
implementa a interface, construindo a expressão por reflexão (não há filtro escrito à mão por
entidade).

Aplica em: `app_users`, `health_conditions`, `medications`, `medication_schedules`,
`symptom_records`, `vital_readings`, `devices`, `notification_preferences`.

**Não** aplica em `dose_occurrences`, `schedule_adjustments`, `notification_deliveries`,
`child_invites`, `schedule_time_slots`, `report_exports` — cada uma tem seu próprio motivo (ver o
diagrama/plano), mas o fio condutor é: dado que é histórico, log ou trilha de auditoria não pode
"sumir" só porque o registro que o originou foi apagado.

**A armadilha a lembrar sempre que um relatório histórico for escrito**: um relatório de janeiro
precisa incluir doses de um medicamento que foi soft-deletado em março. O filtro global de query
esconderia esse medicamento (e, por navegação, prejudicaria o `Include`). Regra: todo repositório
que serve relatório usa `IgnoreQueryFilters()` e reaplica o escopo de `user_id` manualmente — sem
isso, é bug garantido, não hipotético.

Unicidade de `email`/`username` em `app_users` é **global**, não parcial por `deleted_at IS NULL`:
reciclar o email de uma conta apagada é perigoso, e `auth.users` já impõe unicidade global de
email de qualquer forma.

## Medicação → agendamento → ocorrência (três níveis, não dois)

```
medications          — o remédio (nome, dose, unidade, período de tratamento)
  └─ medication_schedules  — a REGRA de recorrência (tipo, dias, intervalo, período)
       ├─ schedule_time_slots  — os HORÁRIOS dentro do dia (08:00, 16:00, 00:00)
       └─ dose_occurrences     — as OCORRÊNCIAS materializadas (a dose de terça 08:00)
```

Uma medicação pode ter mais de um `medication_schedule` (normalmente 1; N cobre "de manhã 1
comprimido, à noite 2" sem gambiarra). `recurrence_type` (enum `RecurrenceType`: `SingleDose`,
`Daily`, `SpecificWeekDays`, `Weekly`, `Monthly`) discrimina o tipo de regra num único formato de
tabela, em vez de uma tabela por tipo — os cinco tipos compartilham a maior parte dos campos e
diferem só em *quais dias*.

`days_of_week` é um bitmask `int` (`[Flags] DayOfWeekFlags`, bit 0 = domingo), não uma tabela
filha — o conjunto é atômico e sempre lido junto com o schedule.

### Por que ocorrências são versionadas, não deletadas, quando o horário é reajustado

`dose_occurrences` materializa cada dose concreta. Quando uma dose é tomada fora do horário e o
schedule precisa reespaçar as doses restantes do dia:

- ocorrências **passadas ou já resolvidas** (`Taken`/`Skipped`/`Missed`) **nunca são tocadas** —
  reescrever o passado destruiria a reprodutibilidade do histórico de aderência.
- ocorrências futuras `Pending` da regra antiga viram `status = Superseded` com
  `superseded_by_occurrence_id` apontando para a nova ocorrência — não são deletadas.
- as novas ocorrências entram com `medication_schedules.generation_version` incrementado, e
  `unique (schedule_id, generation_version, occurrence_index)` evita duplicata na regeração.

Isso é o que torna o histórico de aderência auditável e reproduzível: nada que já aconteceu (ou já
foi cancelado) é reescrito, só suplantado por uma versão nova, com a trilha em
`schedule_adjustments` (append-only, sem soft delete, sem update) registrando o motivo do
reajuste. O custo é acúmulo de linhas `Superseded` — aceito, e elas ficam fora de toda query normal
pelo filtro de `status`.

`scheduled_local_date`/`scheduled_local_time` são a fonte da verdade do horário previsto;
`scheduled_at_utc` é **cache derivado** (local + `time_zone` do usuário) para queries por
intervalo e para o scheduler de notificação — nunca a fonte da verdade. `dose_occurrences` não é
soft-deletable: usa `status` (`Cancelled`, `Superseded`) porque apagar uma dose destruiria a
aderência.

## Conexão com o banco

O projeto Supabase é `yrecrgyecunwheqodlca` (Postgres 17.6). A connection string real **nunca** vai
em `appsettings.Development.json` (versionado) — só em `dotnet user-secrets`:

```bash
dotnet user-secrets set "ConnectionStrings:DotoDb" "<connection string>" --project src/Doto.Api
```

**Use o Session pooler (porta 5432), não o Transaction pooler (porta 6543).** O modo transaction do
PgBouncer quebra prepared statements, que o Npgsql usa por padrão — `dotnet ef` e a aplicação
falham de forma confusa contra o 6543. Se o 6543 for indispensável por algum motivo, é preciso
`No Reset On Close=true;Max Auto Prepare=0;Enlist=false` na connection string; a recomendação
padrão continua sendo 5432.

`DotoDbContextFactory` (design-time, usado pelas ferramentas `dotnet ef`) resolve a connection
string nesta ordem: variável de ambiente → user-secrets do `Doto.Api` → `appsettings*.json` do
`Doto.Api`. Em runtime, `AddInfrastructure` (`src/Doto.Infrastructure/Extensions/DependencyInjection.cs`)
lê de `IConfiguration` normalmente.

### Migrations

Duas migrations existem hoje, ambas já **aplicadas** no Supabase acima: `InitialCreate` (as 14
tabelas, PKs/FKs/índices/CHECKs, nada do schema `auth`) e `AddAuthUsersForeignKey` (só a FK cross-
schema descrita acima).

```bash
dotnet ef database update --project src/Doto.Infrastructure --startup-project src/Doto.Api
```

**Nunca automaticamente no startup.** `Program.cs` não chama `Database.Migrate()` — aplicar
migration automaticamente contra o Postgres de produção do Supabase é como se perde dado sem
perceber. Nova alteração de schema = nova migration; nunca editar `Up`/`Down` de uma migration já
aplicada.
