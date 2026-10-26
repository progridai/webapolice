# Formatação de src/auth

Data: 22/10/2026. Continuação da [formatação do núcleo da aplicação](70-formatacao-nucleo-app.md).

Aplicado o Prettier instalado aos 11 arquivos de `apps/web/src/auth`. Foram formatados `AuthContext.ts`, `AuthProvider.tsx`, `index.ts`, `keycloak.ts` e `useAuth.ts`; no índice, a única alteração foi a padronização dos finais de linha para LF. Os seis arquivos restantes, incluindo os dois testes existentes e `AuthorizationProvider.tsx`, já estavam padronizados.

Preservados contratos, extração dos dados do usuário, roles, políticas de autorização, inicialização única do Keycloak, PKCE S256, login, renovação de token, logout, redirecionamentos e estados de carregamento/erro. A comparação estrutural e a comparação do JavaScript emitido pelo TypeScript instalado confirmaram equivalência nos 11 arquivos. As opções de autenticação, intervalos, timeout e mensagens permaneceram iguais.

| Verificação | Resultado |
|---|---|
| Prettier de todo `src/auth` | Aprovado nos 11 arquivos |
| Comparação estrutural e do JavaScript emitido | Aprovada nos 11 arquivos |
| `npm.cmd run format:check` global | Reprovado: 180 arquivos pendentes; antes, 185; cinco retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação restrita e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação dos fluxos de login e das telas autenticadas em navegador com Keycloak real.

Não houve mudança funcional, dependência, configuração de ambiente/Keycloak, regra de autorização, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa43-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 70. O lote de componentes de aplicação e contexto de tema foi concluído no [registro 72](72-formatacao-aplicacao-tema.md), com dois arquivos formatados e dívida reduzida para 178. Próximo lote: os sete arquivos de `services/http`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
