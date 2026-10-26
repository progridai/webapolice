# Formatação de Breadcrumbs e PageHeader

Data: 22/10/2026. Continuação da [formatação de menus e ações](65-formatacao-menus-acoes.md).

Aplicado o Prettier instalado aos diretórios `Breadcrumbs` e `PageHeader`, dentro de `apps/web/src/components/ui`. Os seis arquivos passaram no check restrito. Foram formatados `Breadcrumbs/Breadcrumbs.tsx`, `PageHeader/PageHeader.tsx` e `PageHeader/PageHeader.css`; no CSS, a única alteração foi a padronização dos finais de linha para LF. O estilo de `Breadcrumbs` e os dois arquivos de exportação já estavam padronizados.

Preservados contratos, destinos dos links, identificação da página atual, ícones, separadores, tratamento da lista vazia, título, descrição, ações e conteúdo adicional do cabeçalho. A comparação estrutural confirmou equivalência nos seis arquivos, incluindo atributos de acessibilidade, seletores, valores, tokens e regras responsivas.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos dois diretórios | Aprovado nos seis arquivos |
| Comparação estrutural antes/depois | Aprovada nos seis arquivos; JavaScript emitido equivalente nos quatro arquivos TypeScript |
| `npm.cmd run format:check` global | Reprovado: 206 arquivos pendentes; antes, 209; três retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação restrita e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa38-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 65. O lote de `DescriptionList`, `DetailsSection` e `EntitySummary` foi concluído no [registro 67](67-formatacao-detalhes-resumo.md), com cinco arquivos formatados e dívida reduzida para 201. Próximo lote: os dois arquivos de `StatusBadge`, `Icons.tsx` e `index.ts` de `components/ui`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
