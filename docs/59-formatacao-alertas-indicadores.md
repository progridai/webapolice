# Formatação de Alert, Badge e EmptyState

Data: 22/10/2026. Continuação da [formatação da estrutura dos formulários](58-formatacao-estrutura-formularios.md).

Aplicado o Prettier instalado aos diretórios `Alert`, `Badge` e `EmptyState`, dentro de `apps/web/src/components/ui`. Os nove arquivos passaram no check restrito. Apenas `Alert/Alert.tsx` e `Badge/Badge.tsx` precisavam de formatação; os três estilos, três arquivos de exportação e `EmptyState/EmptyState.tsx` já estavam padronizados.

As alterações se limitam a quebras de linha. Preservados contratos, variantes, ícones, textos, roles de acessibilidade, ação de fechamento e ações do estado vazio. A comparação com os parsers de TypeScript e PostCSS já instalados confirmou estruturas equivalentes nos nove arquivos, incluindo conteúdo literal, seletores, propriedades, valores, comentários e ordem das regras CSS.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos três diretórios | Aprovado nos nove arquivos |
| Comparação estrutural antes/depois | Aprovada nos nove arquivos |
| `npm.cmd run format:check` global | Reprovado: 226 arquivos pendentes; antes, 228; dois retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Os checks e a comparação passaram na primeira execução após a formatação. Não foram criados testes para mudanças de estilo de código; foi executada a suíte existente, incluindo consumidores, temas claro/escuro/sistema e tokens. Os checks locais não substituem homologação visual/autenticada em navegador.

Não houve mudança funcional, dependência, configuração, regra de negócio, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa31-*`, incluindo retrato anterior, comparações e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 58. O lote de `Card`, `Spinner` e `Skeleton` foi concluído no [documento 60](60-formatacao-cards-carregamento.md): nove arquivos conferidos, três formatados e 223 pendentes no global. Os números acima preservam o retrato deste lote de alertas. Próximo lote: `Modal` e `ConfirmDialog`, sem mudanças funcionais. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
