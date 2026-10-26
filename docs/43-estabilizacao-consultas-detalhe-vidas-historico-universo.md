# Estabilização das Consultas de Detalhe, Vidas, Histórico e Universo Permitido

Data: 22/10/2026. Continuação do [primeiro lote de efeitos das consultas de Apólices](42-estabilizacao-efeitos-consultas-apolices.md).

## Resultado

Os quatro hooks restantes passaram a reutilizar `useConsultaApolice`, completando a revisão dos oito hooks de consulta de Apólices. Respostas antigas não substituem a consulta atual nem encerram seu carregamento. Novas tentativas também são canceladas ao trocar de contexto ou desmontar a tela.

A suíte completa passou de 704 para 790 testes aprovados, em 63 arquivos. TypeScript e build aprovados. Os nove arquivos TypeScript deste lote estão sem erros ou avisos de ESLint. O lint global permanece com 23 erros e quatro avisos; o comparativo por arquivo, regra e severidade não apresentou novos apontamentos ou remoções. Este lote corrige o comportamento das consultas e remove duas supressões antigas, sem reduzir a contagem global de lint.

## Comportamento e contratos

- **Detalhe:** carregamento, erro e `retry` usam o ciclo compartilhado. `obterApolice` recebeu o parâmetro opcional `signal?: AbortSignal`, encaminhado ao cliente HTTP. Chamadas existentes com apenas o identificador continuam válidas. O endpoint GET, o identificador e a resposta permanecem iguais.
- **Vidas:** preservados os sete filtros: página, tamanho da página, busca, status, subgrupo, módulo e data de referência. Alterar um valor cancela a consulta anterior. Objetos equivalentes não repetem chamadas; `retry` usa os filtros atuais e retorna `Promise<void>`.
- **Histórico:** preservados identificador, página, tamanho de página e o retorno público `{ data, isLoading, error }`. A troca de página também descarta resultados antigos. Removida a asserção de identificador não nulo; o identificador é verificado antes de construir a função de consulta. Evidências e descrições históricas não são reformatadas.
- **Universo Permitido:** preservados dados, mensagem de contingência e `refetch` com `Promise<void>`. Cancelamentos deixam de ser apresentados como falhas. A aba continua usando sua apresentação existente; nenhuma funcionalidade de manutenção foi adicionada.
- **Carregamento e contexto:** dados de outra consulta ficam ocultos imediatamente. A atualização da mesma consulta preserva seus dados enquanto carrega. Erros anteriores são limpos ao trocar de consulta. Sem identificador, os quatro hooks retornam dados nulos, sem erro ou carregamento e sem iniciar chamadas.
- **Concorrência:** troca de contexto, nova tentativa e desmontagem abortam a operação anterior. Respostas e falhas tardias são descartadas mesmo se o serviço simulado ignorar o sinal. Callbacks guardados de uma consulta antiga ou de uma tela desmontada não iniciam chamadas.
- **Falhas e recuperação:** mantidas instâncias de `Error`/`HttpApiError` e mensagens de contingência para rejeições sem contrato. Cobertos `AbortError` como `DOMException` e `Error`, além de `CanceledError`. A nova tentativa acompanha o término da operação.

Não houve alteração de banco, permissões, dependências, campos de negócio, normalização de texto, layout ou tokens visuais. As regras de Title Case e preservação de evidências continuam vigentes. As alterações locais das etapas anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados de ciclo, erros, Vidas e contratos HTTP | 180 aprovados, zero falhas, quatro arquivos |
| `npm.cmd run test:run` | 790 aprovados, zero falhas, 63 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint dos seis arquivos de produção e três testes alterados | Zero erros e avisos |
| `npm.cmd run lint` global | 23 erros e quatro avisos, iguais ao retrato inicial |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 274 arquivos pendentes; antes, 277 |
| Prettier dos hooks e testes deste lote | Aprovado; o arquivo de API recebeu somente a alteração necessária |
| `git diff --check` | Aprovado |

Ampliados os testes sem criar uma segunda infraestrutura de cenários:

- `useApolicesLifecycle.test.tsx`: 97 cenários, antes 42. Inclui os quatro hooks adicionais, trocas de identificador, remoção da apólice, sucesso/falha tardios, limpeza de erro, refetch, desmontagem, `StrictMode`, todos os filtros de Vidas e paginação do Histórico. O Histórico é exercitado sem exigir uma ação de refetch que seu contrato não oferece.
- `useApolicesErrors.test.tsx`: 65 cenários, antes 36. Inclui Vidas e Universo Permitido, recuperação após falhas e as três representações de cancelamento nos oito hooks.
- `FeatureContracts.test.ts`: 13 cenários, antes 11. Os dois novos verificam a consulta de Detalhe com e sem sinal externo, mantendo método, caminho e resposta. O teste confirma que abortar o sinal externo alcança o sinal usado pelo `fetch`.
- Os cinco testes anteriores de Vidas e todos os testes consumidores de abas, páginas e temas passaram na suíte completa.

A primeira verificação de tipos encontrou uma incompatibilidade no dado de teste de Vidas, que reutilizava uma página tipada de Apólices. O cenário passou a declarar uma página de Vidas com uma pessoa e seus identificadores. A verificação final e a suíte completa passaram após essa correção.

As verificações são locais, com APIs/fetch simulados. Não substituem homologação integrada e visual em navegador com autenticação. Não houve alteração visual neste lote nem testes de backend, que permaneceu inalterado. Não foram adicionados `any`, casts para ocultar contratos, supressões, timers artificiais ou relaxamentos de compilação.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa15-*`, incluindo retrato inicial dos quatro hooks e função de API, testes, build, tipos e comparativo de lint. A formatação geral e as sete ocorrências do design system continuam pendentes.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Produção: `src/features/apolices/hooks/useApoliceDetalhe.ts`, `useApoliceVidas.ts`, `useApoliceHistorico.ts`, `useApoliceUniversoPermitido.ts`; comentário do auxiliar `useConsultaApolice.ts` no mesmo diretório; e `src/features/apolices/api/apolices.api.ts`.
- Testes ampliados: `src/features/apolices/hooks/useApolicesLifecycle.test.tsx`, `src/features/apolices/hooks/useApolicesErrors.test.tsx` e `tests/http/FeatureContracts.test.ts`.
- Documentação: este registro e continuidade nos documentos 29 e 42.

## Próxima etapa

A revisão dos campos `AsyncEntityField`, `EntityNameResolver` e `ClienteAsyncSelect` foi concluída no [lote de campos de busca](44-estabilizacao-campos-busca-apolices.md), com 851 testes aprovados e lint global em 18 erros e quatro avisos. Os resultados acima preservam o retrato desta entrega. A próxima etapa é revisar os modais `SubestipulanteApoliceModal` e `ApoliceRamoFormModal`.

Depois, continuar pelos formulários/modais e demais regras de lint. Design system, formatação geral e homologação integrada permanecem no [plano de estabilização](29-estabilizacao-frontend.md).
