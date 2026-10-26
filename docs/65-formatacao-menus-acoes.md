# Formatação de DropdownMenu e RowActions

Data: 22/10/2026. Continuação da [formatação de filtros e busca](64-formatacao-filtros-busca.md).

Aplicado o Prettier instalado aos diretórios `DropdownMenu` e `RowActions`, dentro de `apps/web/src/components/ui`. Os seis arquivos passaram no check restrito. Foram formatados os componentes `DropdownMenu.tsx` e `RowActions.tsx` e seus respectivos arquivos CSS; os dois arquivos de exportação já estavam padronizados.

Preservados contratos, abertura e fechamento do menu, tratamento do clique externo, remoção do listener, alinhamento, callbacks, ação principal, ícones, variantes, estados desabilitados e atributos de acessibilidade. No CSS, as alterações ficaram restritas a quebras de linha e espaços; seletores, propriedades, valores, tokens, sombras e transições permaneceram equivalentes.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos dois diretórios | Aprovado nos seis arquivos |
| Comparação estrutural antes/depois | Aprovada nos seis arquivos; JavaScript emitido equivalente nos quatro arquivos TypeScript |
| `npm.cmd run format:check` global | Reprovado: 209 arquivos pendentes; antes, 213; quatro retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação restrita e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa37-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 64. O lote de `Breadcrumbs` e `PageHeader` foi concluído no [registro 66](66-formatacao-navegacao-cabecalho.md), com três arquivos formatados e dívida reduzida para 206. Próximo lote: `DescriptionList`, `DetailsSection` e `EntitySummary`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
