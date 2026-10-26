# Formatação de AuthenticatedLayout

Data: 22/10/2026. Continuação da [formatação de status, ícones e exportações da UI](68-formatacao-status-icones.md).

Aplicado o Prettier instalado aos oito arquivos de `apps/web/src/layouts/AuthenticatedLayout`. Foram formatados `AppHeader.tsx`, `AppNavigation.tsx`, `AuthenticatedLayout.tsx`, `MobileNavigation.tsx`, `UserMenu.tsx` e `AuthenticatedLayout.css`; `AppSidebar.tsx` e `index.ts` já estavam padronizados.

Preservados contratos, rotas, ordem e visibilidade dos itens por módulo/permissão, flag do Design System, identificação da rota ativa, abertura/fechamento do menu mobile, foco, tratamento de Escape, bloqueio da rolagem, seletor de tema e logout. A comparação estrutural confirmou equivalência nos oito arquivos, incluindo textos, atributos de acessibilidade, seletores, valores, tokens, transições e regras responsivas. O JavaScript emitido pelo TypeScript instalado também permaneceu equivalente nos sete arquivos TypeScript.

| Verificação | Resultado |
|---|---|
| Prettier de todo `layouts` | Aprovado; diretório com 13 arquivos, incluindo os oito do lote |
| Comparação estrutural antes/depois | Aprovada nos oito arquivos |
| `npm.cmd run format:check` global | Reprovado: 193 arquivos pendentes; antes, 199; seis retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação de layouts e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, regra de autorização, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa41-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 68. O lote de `src/app` foi concluído no [registro 70](70-formatacao-nucleo-app.md), com oito arquivos formatados e dívida reduzida para 185. Próximo lote: os 11 arquivos de `src/auth`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
