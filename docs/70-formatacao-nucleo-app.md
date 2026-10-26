# Formatação do núcleo src/app

Data: 22/10/2026. Continuação da [formatação do layout autenticado](69-formatacao-layout-autenticado.md).

Aplicado o Prettier instalado aos nove arquivos de `apps/web/src/app`. Foram formatados `app.css`, `config/env.ts`, `providers/AppProviders.tsx`, `routes/AppRoutes.tsx`, `routes/PermissionProtectedRoute.tsx`, `routes/ProtectedRoute.tsx`, `routes/index.ts` e `routes/routePaths.ts`. Em `AppProviders.tsx` e `routes/index.ts`, a única alteração foi a padronização dos finais de linha para LF. `providers/index.ts` já estava padronizado.

Preservados contratos, constantes e parâmetros de rotas, redirecionamentos, estados de carregamento, restrições por autenticação/módulo/permissão, carregamento lazy, flag do Design System e ordem dos providers. Também permaneceram equivalentes a leitura das variáveis de ambiente, valores padrão, mensagens de erro, seletores, valores CSS, tokens e regras responsivas. A comparação estrutural confirmou equivalência nos nove arquivos; o JavaScript emitido pelo TypeScript instalado permaneceu equivalente nos oito arquivos TypeScript.

| Verificação | Resultado |
|---|---|
| Prettier de todo `src/app` | Aprovado nos nove arquivos |
| Comparação estrutural antes/depois | Aprovada nos nove arquivos |
| `npm.cmd run format:check` global | Reprovado: 185 arquivos pendentes; antes, 193; oito retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação restrita e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração de ambiente, regra de autorização, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa42-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 69. O lote de `src/auth` foi concluído no [registro 71](71-formatacao-autenticacao.md), com cinco arquivos formatados e dívida reduzida para 180. Próximo lote: os seis arquivos de `components/application` e `shared/theme/ThemeContext.tsx`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
