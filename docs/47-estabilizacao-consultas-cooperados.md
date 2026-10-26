# Estabilização das Consultas de Cooperados

Data: 22/10/2026. Continuação da [revisão das listagens de cadastros](46-estabilizacao-listagens-cadastros.md).

## Resultado

Revisado `useCooperados`, reutilizando o controle compartilhado de consultas. O lint global passou de 10 erros e quatro avisos para nove erros e três avisos. Removidos o erro de `set-state-in-effect` e o aviso de dependências do hook, sem novos apontamentos por arquivo/regra/severidade. Os quatro arquivos TypeScript deste lote estão sem erros ou avisos de ESLint.

A suíte completa passou de 984 para 1.028 testes aprovados, em 69 arquivos. TypeScript e build aprovados. Acrescentados 26 testes do hook, 15 da página real e três de contrato HTTP.

## Comportamento e contratos

- **Controle de consulta:** `useCooperados` passou a usar `useConsulta`, sem outra implementação de carregamento, cancelamento ou refetch. O carregamento inicial é derivado da consulta. O efeito que iniciava uma função com alterações síncronas de estado foi removido.
- **Sete parâmetros explícitos:** página, limite, nome, CPF, status, campo de ordenação e direção compõem a identidade da consulta. Alterar um deles inicia a consulta correspondente. Objetos equivalentes, inclusive com ordem diferente das propriedades, não repetem chamadas nem alteram a identidade do retry.
- **Concorrência:** mudar filtros/página ou tentar novamente aborta a operação anterior. Sucesso, falha e término de uma chamada cancelada não alteram dados, erro ou carregamento da consulta atual, mesmo que a API simulada ignore o sinal. Desmontagem também aborta a chamada pendente, inclusive uma nova tentativa.
- **Dados durante carregamento:** ao trocar de consulta, o resultado anterior fica oculto imediatamente. Na atualização da mesma consulta, os dados existentes permanecem disponíveis enquanto carrega e caso a atualização falhe. Erros de outra consulta são limpos ao trocar de contexto.
- **Retry:** mantidos o nome e a chamada sem argumentos usados pela página. A função agora retorna `Promise<void>`, permitindo acompanhar o término da consulta; o consumidor existente continua compatível como handler do botão. Usa os filtros atuais, cancela uma chamada anterior e não inicia I/O por callbacks guardados de uma consulta antiga ou após desmontagem.
- **Erros preservados:** `HttpApiError` continua convertido em `Error`, conservando sua mensagem ou `Erro ao comunicar com a API` quando vazia. A causa original agora fica registrada em `cause`. Outras instâncias de `Error` continuam preservadas; rejeições sem contrato usam `Erro desconhecido`. A causa não é exibida pela página nem persistida.
- **Cancelamento sem falha:** tratados `AbortError` como `Error`/`DOMException` e `CanceledError`. Uma rejeição de cancelamento não mostra erro nem deixa o carregamento permanente. Respostas descartadas por um sinal abortado não encerram o carregamento de outra consulta.
- **Consumidor existente:** continuam disponíveis `data`, `isLoading`, `error` e `retry`. A página, os componentes de filtros, tabela e cartões mobile não foram alterados. Testados filtros da URL, ordenação, paginação, busca por nome/CPF, limpeza dos filtros aplicados, recuperação de falha e permissões de criação/edição.
- **API e dados:** `listarCooperados` já suportava `AbortSignal` e não precisou ser alterada. Preservados endpoint e nomes de query string, incluindo `tamanho_pagina`, `ordenar_por` e `direcao`. O hook não formata texto de busca, CPF, status ou identificadores.

Não houve alteração de banco, permissões, dependências, campos persistidos ou contratos REST. A regra de Title Case e a preservação de evidências continuam conforme o documento 26. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados do hook, página e contratos HTTP | 62 aprovados, zero falhas, três arquivos; 26 do hook, 15 da página e 21 de HTTP no total |
| `npm.cmd run test:run` | 1.028 aprovados, zero falhas, 69 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint do hook e três testes | Zero erros e avisos |
| `npm.cmd run lint` global | Nove erros e três avisos; antes, 10 e quatro |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 264 arquivos pendentes; antes, 265 |
| Prettier do hook e três testes | Aprovado |
| `git diff --check` | Aprovado |

`useCooperados.test.tsx` cobre os sete parâmetros, consultas equivalentes, sinal de cancelamento, respostas/falhas antigas antes e depois da consulta atual, dados/erro durante trocas e atualizações, Promise de retry, callbacks antigos, desmontagem, erros HTTP/comuns/desconhecidos, cancelamentos e `StrictMode`.

`CooperadosListLifecycle.test.tsx` usa a página, filtros, tabela/cartões e hook reais, com API simulada. Exercita desktop (1.024 px) e mobile (390 px), nos temas claro, escuro e sistema com preferência clara/escura. Cobre carregamento, erro, recuperação e vazio, além de filtros da URL, ordenação/paginação, debounce, nome/CPF, status, limpeza, permissões e saída da página durante retry.

`FeatureContracts.test.ts` passou de 18 para 21 cenários. Os três novos verificam todos os filtros de Cooperados, espaços/acentos no nome, CPF e status, propagação do sinal com/sem sinal externo e omissão dos filtros opcionais ausentes. A API permaneceu inalterada.

A primeira passagem de ESLint apontou `preserve-caught-error` ao converter e relançar o erro HTTP. Adicionada sua causa original, mantendo a mensagem anterior. A conferência de formatação apontou o teste novo da página; a formatação foi reaplicada e o check do lote passou. Não houve falhas nos testes executados ou no TypeScript. Não foram adicionados `any`, casts para esconder contratos, supressões, timers artificiais ou relaxamentos de compilação.

As verificações são locais, em DOM e com API/fetch simulados. Não substituem homologação visual e integrada em navegador com autenticação. Não houve alteração de estilos ou tokens. Permanecem pendentes as sete ocorrências anteriores do design system e a formatação geral. Backend não alterado; seus testes não foram executados.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa19-*`, incluindo retrato inicial, testes, build, tipos e comparativo de lint.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Produção: `src/features/cooperados/hooks/useCooperados.ts`.
- Testes novos: `src/features/cooperados/hooks/useCooperados.test.tsx` e `tests/pages/CooperadosListLifecycle.test.tsx`.
- Teste ampliado: `tests/http/FeatureContracts.test.ts`.
- Documentação: este registro e continuidade nos documentos 29 e 46.

## Próxima etapa

A revisão das exportações de `cooperados.routes.tsx` foi concluída no [lote de rotas de Cooperados](48-estabilizacao-rotas-cooperados.md), com 1.032 testes aprovados e lint global em cinco erros e três avisos. Os resultados acima preservam o retrato desta entrega. A próxima etapa é revisar o efeito de `AuthorizationProvider`.

Depois, continuar por autorização e demais regras de lint. Design system, formatação geral e homologação integrada permanecem no [plano de estabilização](29-estabilizacao-frontend.md).
