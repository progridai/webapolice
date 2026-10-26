# Estabilização das Rotas de Cooperados

Data: 22/10/2026. Continuação da [revisão das consultas de Cooperados](47-estabilizacao-consultas-cooperados.md).

## Resultado

Separados os componentes de carregamento das páginas e a configuração das rotas de Cooperados. O lint global passou de nove para cinco erros, mantendo três avisos. Removidos os quatro apontamentos de `react-refresh/only-export-components`, sem novos apontamentos por arquivo/regra/severidade. Os três arquivos TypeScript deste lote estão sem erros ou avisos de ESLint.

A suíte completa passou de 1.028 para 1.032 testes aprovados, em 70 arquivos. TypeScript e build aprovados. Acrescentados quatro cenários de carregamento/permissões das rotas, com verificações de tema em cada uma.

## Estrutura e comportamento

- **Configuração das rotas:** `cooperados.routes.tsx` continua exportando `CooperadosRoutes`, consumido pelo roteador principal e testes existentes. O arquivo conserva a árvore de `Route`, o layout autenticado e os mesmos guards. Não declara mais os componentes `lazy` locais que causavam os quatro apontamentos de Fast Refresh.
- **Componentes de rota:** o novo `CooperadosRoutePages.tsx` declara as quatro páginas `lazy` e exporta somente componentes. Cada componente envolve sua página em `Suspense`, com o `PageLoading` já existente. Não foram alteradas regras de ESLint nem adicionadas supressões.
- **Carregamento local:** o limite de `Suspense` pertence à página, dentro da área protegida. Enquanto o módulo da página está pendente, o indicador aparece e o layout continua disponível. A árvore anterior não declarava esse limite para Cooperados, nem o recebia do layout ou do provider global.
- **Autorização antes do carregamento:** os guards continuam fora dos componentes de página. Enquanto a autorização carrega, o módulo de página não é solicitado. Usuário inativo ou sem a permissão exigida é redirecionado conforme a política existente, sem iniciar o import da página. Nenhuma concessão ou exigência de permissão foi acrescentada.
- **Lazy loading preservado:** os imports continuam apontando para os mesmos quatro módulos e seus exports nomeados. As páginas não são importadas estaticamente. Enquanto um módulo está pendente e ao revisitar a rota já carregada, o `lazy` reutiliza o carregamento/cache existente.
- **Identificadores e navegação:** conservados caminho de listagem, prioridade da rota `novo`, parâmetro `id` de detalhe/edição, query string e layout autenticado. O consumidor `AppRoutes.tsx` não precisou ser alterado.

| Caminho | Página | Permissão existente |
|---|---|---|
| `/cooperados` | Listagem | `cooperados.visualizar` |
| `/cooperados/novo` | Cadastro | `cooperados.inserir` |
| `/cooperados/:id` | Detalhe | `cooperados.visualizar` |
| `/cooperados/:id/editar` | Edição | `cooperados.alterar` |

Não houve alteração de API, banco, campos persistidos, contratos, dependências ou políticas de acesso. A regra de Title Case e a preservação de evidências permanecem conforme o documento 26. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados de carregamento e permissões | 34 aprovados, zero falhas, dois arquivos; quatro novos e 30 anteriores |
| `npm.cmd run test:run` | 1.032 aprovados, zero falhas, 70 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint dos dois arquivos de produção e teste novo | Zero erros e avisos |
| `npm.cmd run lint` global | Cinco erros e três avisos; antes, nove e três |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 263 arquivos pendentes; antes, 264 |
| Prettier dos dois arquivos de produção e teste novo | Aprovado |
| Comparação de caminhos, index e permissões com o retrato inicial | Preservados |
| `git diff --check` | Aprovado |

`CooperadosLazyRoutes.test.tsx` usa a árvore de rotas e os guards reais, páginas simuladas com disponibilização controlada e um layout de teste com `Outlet`. Não há um `Suspense` externo no teste: o indicador precisa vir dos componentes de produção. Cada uma das quatro rotas verifica autorização carregando, usuário inativo, permissão insuficiente, carregamento autorizado, resolução da página, parâmetro `id`, query string e reutilização do módulo ao revisitar.

Enquanto cada módulo permanece pendente, seu indicador é exercitado em claro, escuro e sistema com preferência clara/escura: 16 combinações de rota e tema. As páginas e o `PageLoading` usam os componentes/tokens existentes, sem estilo ou cor nova neste lote. Os 30 cenários anteriores de `FeaturePermissions.test.tsx` continuaram aprovados, assim como os testes de páginas/layout na suíte completa.

A execução inicial direcionada passou; os testes foram ampliados para conferir todos os temas em cada rota e passaram novamente na suíte completa. Não houve falhas nos testes, TypeScript ou ESLint do lote. Não foram adicionados `any`, casts para esconder contratos, supressões ou relaxamentos de compilação.

As verificações são locais, em DOM e com páginas/autorização simuladas. Não substituem homologação com autenticação, apresentação em navegador e verificação de HMR no servidor de desenvolvimento. Backend não alterado; seus testes não foram executados. Permanecem pendentes as sete ocorrências anteriores do design system e a formatação geral.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa20-*`, incluindo retrato inicial, testes, build, tipos e comparativo de lint.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Produção: `src/features/cooperados/routes/cooperados.routes.tsx` e `src/features/cooperados/routes/CooperadosRoutePages.tsx` (novo).
- Teste novo: `tests/routes/CooperadosLazyRoutes.test.tsx`.
- Documentação: este registro e continuidade nos documentos 29 e 47.

## Próxima etapa

A revisão de `AuthorizationProvider` foi concluída no [documento 49](49-estabilizacao-autorizacao.md), com carregamento por sessão, atualização, descarte de respostas antigas e recuperação de erro. Os números acima preservam o retrato desta etapa de rotas.

Continuar pelos quatro erros restantes de `prefer-const` e, depois, pelos três avisos de lint. Design system, formatação geral e homologação integrada permanecem no [plano de estabilização](29-estabilizacao-frontend.md).
