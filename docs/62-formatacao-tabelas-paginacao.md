# Formatação de Table e Pagination

Data: 22/10/2026. Continuação da [formatação de modais e confirmação](61-formatacao-modais-confirmacao.md).

Aplicado o Prettier instalado aos diretórios `Table` e `Pagination`, dentro de `apps/web/src/components/ui`. Os seis arquivos passaram no check restrito. Foram formatados `Table/Table.tsx`, `Pagination/Pagination.tsx` e `Table/Table.css`; no CSS, a única alteração foi a padronização dos finais de linha para LF. `Pagination.css` e os dois arquivos de exportação já estavam padronizados.

Preservados contratos, classes, atributos HTML e de acessibilidade, seleção de linhas, células de cabeçalho, navegação, cálculo de páginas visíveis, reticências e estados desabilitados. Os estilos continuam consumindo os mesmos tokens semânticos, sem mudança de seletores, propriedades ou valores.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos dois diretórios | Aprovado nos seis arquivos |
| Comparação estrutural antes/depois | Aprovada nos seis arquivos; JavaScript emitido equivalente nos quatro arquivos TypeScript |
| `npm.cmd run format:check` global | Reprovado: 218 arquivos pendentes; antes, 221; três retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

A primeira comparação auxiliar apontou diferença no texto JSX das reticências, movido para linhas próprias pelo Prettier. Após inspeção do diff, o comparador passou a considerar esse espaço de formatação e também comparar a estrutura do JavaScript emitido pelo TypeScript instalado, confirmando que o conteúdo renderizado foi preservado. Nenhuma validação do projeto foi flexibilizada.

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa34-*`, incluindo retrato anterior, comparações e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 61. O lote de `DataTable` e `ResultsSummary` foi concluído no [registro 63](63-formatacao-datatable-resumo.md), com dois arquivos formatados e dívida reduzida para 216. Próximo lote: `FilterBar` e `SearchField`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
