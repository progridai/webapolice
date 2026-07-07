# Modulo Clientes: Listagem

## Escopo

A tela atual implementa a listagem de clientes e inclui um botão "Novo Cliente" no cabeçalho, que redireciona para a rota `/clientes/novo` (tela de cadastro). As demais ações (edição, exclusão, inativação ou detalhes) não fazem parte deste escopo inicial.

## Endpoint

```text
GET /api/clientes
```

Base local:

```text
http://127.0.0.1:5007
```

URL final:

```text
http://127.0.0.1:5007/api/clientes?pagina=1&tamanho_pagina=20
```

## Autorizacao

O endpoint exige autenticacao Bearer e a politica `ConsultaClientes`, que aceita
as roles `admin`, `gestor` e `operador`.

## Parametros

- `pagina`
- `tamanho_pagina`
- `nome`
- `cpf`
- `status` (`1` ativo, `2` inativo)
- `ordenar_por`
- `direcao` (`asc` ou `desc`)

## Resposta

```json
{
  "itens": [],
  "paginaAtual": 1,
  "tamanhoPagina": 20,
  "totalItens": 0,
  "totalPaginas": 0
}
```

Cada item contem `id`, `nome`, `cpfMascarado`, `status` (`ativo` ou `inativo`)
e `dataCadastroUtc`.

## Tratamento de erros

Erros de rede sao normalizados para uma mensagem segura:

```text
Nao foi possivel conectar ao servidor. Verifique sua conexao e tente novamente.
```

Mensagens tecnicas do navegador, como `Failed to fetch`, nao devem ser exibidas
ao usuario.

Erros HTTP seguem a normalizacao de `src/services/http/httpError.ts`, incluindo
401, 403, 404, 429, 500 e 503.

## Layout

Os filtros usam uma grade responsiva:

- desktop: busca maior, status menor, acoes alinhadas ao final;
- tablet: duas colunas;
- mobile: uma coluna, sem scroll horizontal.

Os icones estruturais da navegacao usam SVGs centralizados em
`src/components/ui/Icons.tsx`; emojis nao devem ser usados como icones de
navegacao.

## Referências Relacionadas

- [17-modelagem-banco-dados-webapolice.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/17-modelagem-banco-dados-webapolice.md)
- [18-modelagem-clientes-core-cadastro.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/18-modelagem-clientes-core-cadastro.md)
