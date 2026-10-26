# Formatação de DataTable e ResultsSummary

Data: 22/10/2026. Continuação da [formatação de tabelas e paginação](62-formatacao-tabelas-paginacao.md).

Aplicado o Prettier instalado aos diretórios `DataTable` e `ResultsSummary`, dentro de `apps/web/src/components/ui`. Os seis arquivos passaram no check restrito. Foram formatados `DataTable/DataTable.tsx` e `ResultsSummary/ResultsSummary.tsx`; os dois estilos CSS e os dois arquivos de exportação já estavam padronizados.

Preservados contratos genéricos, renderização de células, callbacks de ordenação, expansão de linhas, estados de carregamento/vazio, atributos de acessibilidade e cálculo/texto do intervalo de resultados. A comparação estrutural confirmou equivalência nos seis arquivos, incluindo os estilos e seus tokens; nenhuma regra CSS foi alterada.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos dois diretórios | Aprovado nos seis arquivos |
| Comparação estrutural antes/depois | Aprovada nos seis arquivos; JavaScript emitido equivalente nos quatro arquivos TypeScript |
| `npm.cmd run format:check` global | Reprovado: 216 arquivos pendentes; antes, 218; dois retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

A primeira comparação auxiliar apontou o espaço JSX que o Prettier tornou explícito como `{' '}` em `ResultsSummary`. Após inspeção do diff, o comparador passou a considerar essa representação de espaço. A comparação adicional do JavaScript emitido pelo TypeScript instalado manteve a verificação dos espaços efetivos e confirmou o mesmo conteúdo. Nenhuma validação do projeto foi flexibilizada.

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa35-*`, incluindo retrato anterior, comparações e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 62. O lote de `FilterBar` e `SearchField` foi concluído no [registro 64](64-formatacao-filtros-busca.md), com três arquivos formatados e dívida reduzida para 213. Próximo lote: `DropdownMenu` e `RowActions`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
