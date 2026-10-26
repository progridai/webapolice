# Formatação de FilterBar e SearchField

Data: 22/10/2026. Continuação da [formatação de DataTable e resumo](63-formatacao-datatable-resumo.md).

Aplicado o Prettier instalado aos diretórios `FilterBar` e `SearchField`, dentro de `apps/web/src/components/ui`. Os seis arquivos passaram no check restrito. Foram formatados `FilterBar/FilterBar.tsx`, `SearchField/SearchField.tsx` e `SearchField/SearchField.css`; o estilo de `FilterBar` e os dois arquivos de exportação já estavam padronizados.

Preservados contratos, conteúdo filho, classes, sincronização do valor externo, debounce e seu cancelamento, callbacks, limpeza da busca, estado desabilitado e atributos de acessibilidade. No CSS, foram acrescentados apenas espaços entre argumentos do fallback `rgba`, sem mudança de valor. Permanecem os mesmos seletores, regras responsivas, tokens e estados visuais.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos dois diretórios | Aprovado nos seis arquivos |
| Comparação estrutural antes/depois | Aprovada nos seis arquivos; JavaScript emitido equivalente nos quatro arquivos TypeScript |
| `npm.cmd run format:check` global | Reprovado: 213 arquivos pendentes; antes, 216; três retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A primeira leitura auxiliar do relatório ocorreu antes de sua geração; após o encerramento da suíte com saída 0, o relatório foi lido e conferido. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa36-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 63. O lote de `DropdownMenu` e `RowActions` foi concluído no [registro 65](65-formatacao-menus-acoes.md), com quatro arquivos formatados e dívida reduzida para 209. Próximo lote: `Breadcrumbs` e `PageHeader`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
