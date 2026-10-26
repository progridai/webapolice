# Formatação de Modal e ConfirmDialog

Data: 22/10/2026. Continuação da [formatação de cards e carregamento](60-formatacao-cards-carregamento.md).

Aplicado o Prettier instalado aos diretórios `Modal` e `ConfirmDialog`, dentro de `apps/web/src/components/ui`. Os seis arquivos passaram no check restrito. Foram formatados `Modal/Modal.tsx` e `ConfirmDialog/ConfirmDialog.tsx`; os dois estilos CSS e os dois arquivos de exportação já estavam padronizados.

A comparação estrutural antes/depois com TypeScript e PostCSS confirmou equivalência nos seis arquivos. Foram preservados contratos, conteúdo literal, atributos de acessibilidade, foco, tratamento de Escape/Tab, restauração da rolagem, fechamento, callbacks de confirmação, variantes e estado de carregamento. Nenhuma regra CSS foi alterada.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos dois diretórios | Aprovado nos seis arquivos |
| Comparação estrutural antes/depois | Aprovada nos seis arquivos |
| `npm.cmd run format:check` global | Reprovado: 221 arquivos pendentes; antes, 223; dois retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A comparação estrutural e os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa33-*`, incluindo retrato anterior, comparações e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 60. O lote de `Table` e `Pagination` foi concluído no [registro 62](62-formatacao-tabelas-paginacao.md), com três arquivos formatados e dívida reduzida para 218. Próximo lote: `DataTable` e `ResultsSummary`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
