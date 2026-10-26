# Formatação de DescriptionList, DetailsSection e EntitySummary

Data: 22/10/2026. Continuação da [formatação de navegação e cabeçalho](66-formatacao-navegacao-cabecalho.md).

Aplicado o Prettier instalado aos diretórios `DescriptionList`, `DetailsSection` e `EntitySummary`, dentro de `apps/web/src/components/ui`. Os oito arquivos passaram no check restrito. Foram formatados os três componentes TSX e os estilos de `DescriptionList` e `EntitySummary`; no CSS de `EntitySummary`, a única alteração foi a padronização dos finais de linha para LF. Os três arquivos de exportação já estavam padronizados.

Preservados contratos, pontuação dos rótulos, valores e mensagens de ausência de informação, colunas, densidade, ícones, ações, estados vazios, documento, badges e informações secundárias. A comparação estrutural confirmou equivalência nos oito arquivos, incluindo classes, seletores, valores, tokens, transições e regras responsivas.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos três diretórios | Aprovado nos oito arquivos |
| Comparação estrutural antes/depois | Aprovada nos oito arquivos; JavaScript emitido equivalente nos seis arquivos TypeScript |
| `npm.cmd run format:check` global | Reprovado: 201 arquivos pendentes; antes, 206; cinco retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação restrita e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa39-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 66. O lote de `StatusBadge`, ícones e exportações foi concluído no [registro 68](68-formatacao-status-icones.md), com dois arquivos formatados, dívida reduzida para 199 e todo o diretório `components/ui` aprovado no Prettier. Próximo lote: os oito arquivos de `layouts/AuthenticatedLayout`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
