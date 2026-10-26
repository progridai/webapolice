# Formatação de StatusBadge, ícones e exportações da UI

Data: 22/10/2026. Continuação da [formatação de detalhes e resumo](67-formatacao-detalhes-resumo.md).

Aplicado o Prettier instalado aos dois arquivos de `StatusBadge`, a `Icons.tsx` e ao `index.ts` de `apps/web/src/components/ui`. Os quatro arquivos passaram no check restrito. Foram formatados `StatusBadge/StatusBadge.tsx` e `Icons.tsx`; os dois índices de exportação já estavam padronizados.

Preservados contratos, mapeamento de status para variantes, texto de exibição, exportações, tamanhos padrão, atributos de acessibilidade, dimensões, cores herdadas e geometria dos SVGs. A comparação estrutural e a comparação do JavaScript emitido pelo TypeScript instalado confirmaram equivalência nos quatro arquivos.

| Verificação | Resultado |
|---|---|
| Prettier restrito ao lote | Aprovado nos quatro arquivos |
| Prettier de todo `components/ui` | Aprovado; diretório com 98 arquivos, sem pendências |
| Comparação estrutural e do JavaScript emitido | Aprovada nos quatro arquivos |
| `npm.cmd run format:check` global | Reprovado: 199 arquivos pendentes; antes, 201; dois retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação restrita e as comparações passaram na primeira execução após a formatação. O fechamento da formatação de UI não constitui homologação visual nem dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa40-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 67. O lote de `AuthenticatedLayout` foi concluído no [registro 69](69-formatacao-layout-autenticado.md), com seis arquivos formatados, dívida reduzida para 193 e todo o diretório `layouts` aprovado no Prettier. Próximo lote: os nove arquivos de `src/app`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
