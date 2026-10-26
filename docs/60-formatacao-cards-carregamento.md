# Formatação de Card, Spinner e Skeleton

Data: 22/10/2026. Continuação da [formatação de alertas e indicadores](59-formatacao-alertas-indicadores.md).

Aplicado o Prettier instalado aos diretórios `Card`, `Spinner` e `Skeleton`, dentro de `apps/web/src/components/ui`. Os nove arquivos passaram no check restrito. Foram formatados `Card/Card.tsx`, `Card/Card.css` e `Skeleton/Skeleton.css`; os seis arquivos restantes já estavam padronizados, incluindo todo o diretório de `Spinner`.

Preservados contratos, conteúdo filho, classes, variantes, tamanhos, dimensões, textos de acessibilidade, tokens e animações de carregamento. A comparação com TypeScript e PostCSS já instalados confirmou estruturas equivalentes nos nove arquivos, incluindo conteúdo literal, seletores, propriedades, valores, comentários e ordem das regras CSS.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos três diretórios | Aprovado nos nove arquivos |
| Comparação estrutural antes/depois | Aprovada nos nove arquivos |
| `npm.cmd run format:check` global | Reprovado: 223 arquivos pendentes; antes, 226; três retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Os checks e a comparação passaram na primeira execução após a formatação. Não foram criados testes para mudanças de estilo de código; foi executada a suíte existente, incluindo consumidores, carregamento, temas claro/escuro/sistema e tokens. Os checks locais não substituem homologação visual/autenticada em navegador.

Não houve mudança funcional, dependência, configuração, regra de negócio, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa32-*`, incluindo retrato anterior, comparações e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 59. O lote de `Modal` e `ConfirmDialog` foi concluído no [registro 61](61-formatacao-modais-confirmacao.md), com dois arquivos formatados e dívida reduzida para 221. Próximo lote: `Table` e `Pagination`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
