# ⚠️ BASE OF WORK — material de referência, não fonte da verdade

Tudo nesta pasta descreve a **versão anterior** da `doto.api`, que foi descartada. O código-fonte
correspondente não existe mais no working tree — ele está no histórico do git, no commit
`de59481` ("initial commit da nova versao Doto").

## Como usar

- **Serve para**: entender as decisões de modelagem que já foram tomadas (entidades, relações,
  fluxos de agendamento/aderência), lembrar quais funcionalidades o app precisa ter, e evitar
  repetir erros já conhecidos.
- **Não serve para**: ser copiado literalmente, nem para ser tratado como descrição do estado
  atual do projeto. Nenhum arquivo, namespace, rota ou tabela citado aqui existe hoje.

Ao consultar qualquer coisa daqui, valide contra o código atual antes de assumir que ainda vale.

## Conteúdo

| Arquivo | O que é |
|---|---|
| `ARCHITECTURE-legacy.md` | Antigo `CLAUDE.md` da raiz do repo — descrevia a Clean Architecture da versão anterior em detalhe. |
| `docs/DEPLOYMENT.md` | Notas de deploy da versão anterior, incluindo a exposição de credenciais já sinalizada. |

## Contexto de infraestrutura

O projeto Supabase da versão anterior (`pvtffkgbyqsqtaxntrgd`) **foi deletado** — o DNS não
resolve mais. Qualquer connection string, chave ou project ref citado nesta pasta está morto.
