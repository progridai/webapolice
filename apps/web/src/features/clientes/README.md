# Feature: Clientes

## Escopo atual

Este modulo implementa a listagem de clientes autenticada e autorizada da
plataforma WebApolice.

Nao fazem parte do escopo atual:

- pagina de detalhes;
- cadastro;
- edicao;
- exclusao;
- inativacao.

## Endpoint utilizado

```text
GET /api/clientes
```

A base da API vem de `VITE_API_BASE_URL`. Em desenvolvimento local:

```text
http://127.0.0.1:5007/api/clientes
```

## Politica de acesso

```text
/#/clientes -> roles: admin, gestor, operador
```

## Organizacao

- `api/clientesApi.ts`: composicao da query string e chamada via `httpClient`.
- `hooks/useClientes.ts`: ciclo de carregamento, cancelamento e retry.
- `hooks/useClientesFilters.ts`: filtros sincronizados com query params.
- `components/ClientesFilters.tsx`: filtros responsivos.
- `components/ClientesTable.tsx`: tabela desktop.
- `components/ClientesMobileList.tsx`: lista mobile.
- `pages/ClientesListPage.tsx`: pagina da listagem.

## Erros

Erros HTTP e de rede devem vir normalizados de `src/services/http`. A UI nao
deve exibir mensagens tecnicas cruas, como `Failed to fetch`, stack traces ou
tokens.

## Design

Todos os controles usam o Design System em `src/components/ui`. Os estilos usam
tokens CSS, sem cores fixas.

Referencias:

- `docs/14-fundacao-frontend.md`
- `docs/15-modulo-clientes-listagem.md`
- `docs/11-modulo-clientes-backend.md`
