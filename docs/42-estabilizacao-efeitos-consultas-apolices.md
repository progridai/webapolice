# Estabilização dos Efeitos das Consultas de Apólices

Data: 22/10/2026. Continuação do [lote de campos e seletor](41-estabilizacao-tipos-campos-seletor.md).

## Resultado

Revisados os hooks de listagem de Apólices, Módulos, Subgrupos e Subestipulantes. O lint global passou de 27 para 23 erros, mantendo quatro avisos: removidos três erros de `set-state-in-effect` e um de `use-memo`. Os cinco arquivos de produção deste lote e o teste novo passaram no ESLint sem erros ou avisos, sem novos apontamentos globais por arquivo/regra.

Adicionados 42 cenários de regressão. A suíte completa passou com 704 testes em 63 arquivos; TypeScript e build aprovados. Permanecem pendentes os demais hooks e outras dívidas de lint, design system, formatação e homologação.

## Correções e comportamento

- **Ciclo de consulta:** o carregamento inicial é derivado da identidade da consulta. O efeito inicia a operação e registra resultados/erros após a conclusão da chamada assíncrona; não atualiza sincronamente o estado de carregamento. A nova tentativa, acionada por evento, pode iniciar seu estado de carregamento imediatamente. Não foram adicionados timers ou adiamentos artificiais para silenciar o lint.
- **Compartilhamento local:** `useConsultaApolice` concentra esse ciclo nos quatro hooks, que já repetiam carregamento, erro e consulta. O auxiliar permanece no domínio de Apólices, sem infraestrutura global ou dependências novas. Os contratos de dados continuam inferidos das APIs existentes.
- **Filtros da listagem:** substituída a dependência de `JSON.stringify(query)` pelos nove valores definidos em `ApolicesQuery`. Objetos equivalentes, inclusive com ordem diferente das propriedades, não repetem a consulta. Alterar qualquer filtro ou `refreshTrigger` inicia a atualização correspondente. Removidas duas supressões antigas de lint da listagem.
- **Troca de consulta:** ao mudar filtros ou apólice, o resultado anterior fica oculto e o carregamento da consulta atual é apresentado. Na atualização da mesma consulta, por retry/refetch ou `refreshTrigger`, os dados existentes continuam disponíveis enquanto carrega. Erros da consulta anterior não são reapresentados na nova.
- **Concorrência e cancelamento:** cada operação usa `AbortController` e encaminha seu sinal às funções de API, que já o suportavam. Troca de consulta, nova tentativa ou desmontagem aborta a operação anterior. Respostas e falhas tardias são descartadas mesmo se a API simulada ignorar o cancelamento; não alteram dados, erro ou carregamento da operação atual.
- **Retry/refetch:** os nomes e o retorno `Promise<void>` foram mantidos. A Promise acompanha o término da chamada. Callbacks guardados de outra consulta ou de uma tela desmontada não iniciam novas chamadas. A limpeza do efeito também aborta uma eventual nova tentativa em andamento.
- **Identificador ausente:** os três hooks de vínculos retornam lista vazia, sem erro e sem carregamento quando não há apólice; nenhuma chamada é iniciada. Remover o identificador encerra a consulta anterior.
- **Falhas:** instâncias de `Error`/`HttpApiError` e as mensagens de contingência existentes foram preservadas. `AbortError` e `CanceledError` são tratados como cancelamento, incluindo `DOMException`, sem alerta de falha ou carregamento permanente.

Não houve alteração de endpoints, payloads, funções de API, permissões, banco, componentes visuais ou campos de negócio. A regra de Title Case e a preservação das evidências permanecem conforme a documentação existente. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados de ciclo de vida e erros | 78 aprovados, zero falhas, dois arquivos |
| `npm.cmd run test:run` | 704 aprovados, zero falhas, 63 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, código de saída 0 |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| ESLint dos cinco arquivos de produção e teste novo | Zero erros e avisos |
| `npm.cmd run lint` global | 23 erros e 4 avisos; antes, 27 erros e 4 avisos |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 277 arquivos pendentes; antes, 278 |
| `git diff --check` | Aprovado |

`useApolicesLifecycle.test.tsx` cobre 42 cenários: 28 de concorrência/recuperação nos quatro hooks, três de remoção do identificador, nove de filtros da listagem, um de filtros equivalentes e um de `refreshTrigger`. Inclui sucesso e falha tardios, callback de consulta anterior, manutenção de dados na atualização da mesma consulta, Promise de refetch, desmontagem durante refetch e montagem dupla em `StrictMode`.

Os 36 testes anteriores de erros de Apólices continuaram aprovados, assim como a suíte completa das abas, seletores e páginas consumidoras. A verificação de tipos inicial do teste novo identificou uma inferência restrita demais para `initialProps.key` no cenário que remove a apólice; a declaração foi explicitada para permitir `undefined`, e a verificação final passou.

As validações são locais, com APIs simuladas, incluindo serviços que resolvem/rejeitam após o sinal ser abortado. Não houve operações no ambiente publicado ou alterações visuais que exigissem novas verificações de tema; os testes de temas existentes também passaram na suíte completa. Isso não substitui homologação integrada ou visual em navegador. Backend não alterado; seus testes não foram executados.

Não foram adicionadas dependências, `any`, casts para ocultar contratos, supressões ou relaxamentos de compilação. Os arquivos reestruturados e o teste novo foram formatados. Relatórios locais ignorados pelo Git em `scratch/lint-etapa14-*`, incluindo retrato inicial dos quatro hooks e comparativo de lint.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- `src/features/apolices/hooks/useConsultaApolice.ts` (novo auxiliar local).
- `src/features/apolices/hooks/useApolices.ts`.
- `src/features/apolices/hooks/useApoliceModulos.ts`.
- `src/features/apolices/hooks/useApoliceSubgrupos.ts`.
- `src/features/apolices/hooks/useApoliceSubestipulantes.ts`.
- `src/features/apolices/hooks/useApolicesLifecycle.test.tsx` (novo).

Documentação: este registro e continuidade nos documentos 29 e 41.

## Próxima etapa

A revisão dos demais hooks de Apólices foi concluída no [lote de Detalhe, Vidas, Histórico e Universo Permitido](43-estabilizacao-consultas-detalhe-vidas-historico-universo.md), com 790 testes aprovados. Os resultados acima preservam o retrato desta entrega. A próxima etapa é revisar os campos assíncronos `AsyncEntityField`, `EntityNameResolver` e `ClienteAsyncSelect`, preservando o contrato de cada componente.

Depois, continuar pelos outros efeitos e regras de lint. Design system, formatação geral e homologação integrada permanecem conforme o [plano de estabilização](29-estabilizacao-frontend.md).
