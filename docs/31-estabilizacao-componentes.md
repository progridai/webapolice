# Estabilização dos Consumidores de Componentes

Data: 21/10/2026. Continuação do [lote de imports e exportações](30-estabilizacao-tipos-imports.md).

## Resultado e escopo

Os consumidores passaram a usar os contratos já implementados no design system e nas rotas protegidas. O lote reduziu os erros de TypeScript de 180 para 82, sem relaxar o compilador, adicionar aliases de compatibilidade ou excluir arquivos. A verificação global de tipos continua reprovada.

Foram ajustados 52 arquivos de produção e seis arquivos de testes, sendo quatro novos. As alterações anteriores da estabilização e o trabalho local de Clientes foram preservados. Não foram alterados endpoints, payloads, backend, banco, permissões cadastradas, Keycloak ou regras de Title Case; não foram adicionadas dependências, campos de negócio ou tokens visuais.

## Contratos aplicados

| Consumidor | Ajuste |
|---|---|
| `Button` | `outline` → `secondary`, `ghost` → `text`, `sm` → `small`, `isLoading` → `loading`; ícone como filho |
| `Alert` | `type` → `variant`; botão de retentativa dentro de `children` |
| `Pagination` | Somente navegação: `currentPage`, `totalPages` e `onPageChange`; removidas contagens ignoradas |
| `ResultsSummary` | `totalItems` em Cooperados; removidos nomes de item não suportados |
| `PermissionProtectedRoute` | `permissoes`/`permissao` → `permissaoCodigo` nas quatro declarações de rotas afetadas |
| `ConfirmDialog` | `aberto`, `onClose`, `confirmText` e `cancelText` em tabela e lista mobile de Estipulantes |
| `RowActions` | `variant: 'danger'` para inativação; lista da tabela tipada com `RowActionItem[]` |
| `Spinner` / `Badge` | Tamanho `large` e variante de marca `brand` |
| `StatusBadge` | Removido `label`, que era ignorado; preservado o status ativo/inativo utilizado pelo componente |
| `PageHeader` | `description` em Ramos; botão Voltar no slot `actions` da página de Apólice |
| `FormField` | Classes de layout em contêineres explícitos; rótulo de Observações em Cooperados |
| `ReadOnlyField` | Rótulos de Ramo e Cliente compostos com `FormField` |
| `FormGrid` | Removida a propriedade inexistente `columns`; grupos de um campo usam contêiner simples |
| `DescriptionItem` | Rótulo textual e marcador de contato principal junto ao valor em Estipulantes |
| `SearchField` | Removida a propriedade `icon` ignorada pelo `Input` e seu import sem uso |

## Comportamentos recuperados

- **Acesso direto às rotas:** Ramos, Seguradoras, Subestipulantes e Cooperados agora enviam a permissão declarada à propriedade efetivamente verificada pelo guard. Nas rotas com guard pai, as condições do pai e do filho continuam cumulativas. O guard e a autorização do backend não foram modificados. Em Ramos, foi preservada a exigência existente de visualização; não foram inventadas permissões adicionais neste lote.
- **Inativação e reativação de Estipulantes:** o diálogo abre com os textos corretos; cancelar apenas fecha; confirmar aciona o callback correspondente. A inativação usa a variante de perigo do menu existente.
- **Retentativa de carregamento:** Clientes e Estipulantes passam a renderizar o botão Tentar novamente dentro do alerta e chamar o hook existente.
- **Gravação de Módulo:** `loading` aciona o estado desabilitado, o indicador de processamento e `aria-busy` do botão existente.
- **Feedback em Ramos:** sucesso e erro usam os estilos e papéis acessíveis correspondentes após a confirmação da operação.
- **Navegação de Apólice:** o botão Voltar passa a ser renderizado e utiliza o callback de navegação já previsto na página.

## Testes de regressão

| Arquivo | Cobertura adicionada |
|---|---|
| `tests/routes/FeaturePermissions.test.tsx` | 30 casos nas declarações reais das rotas: acesso permitido, ausência de permissão e falta de permissão específica no filho |
| `src/features/estipulantes/components/EstipulantesActions.test.tsx` | 18 casos em tabela/mobile: abrir, cancelar e confirmar inativação/reativação, variante de perigo e ausência de ações sem permissão |
| `tests/pages/ComponentConsumers.test.tsx` | 12 casos nas páginas reais de detalhe e formulário de Módulo: retentativa e bloqueio durante processamento |
| `src/features/apolices/pages/ApoliceFormPage.test.tsx` | Navegação pelo botão Voltar da página real |
| `src/features/apolices/components/tabs/RamosTab.test.tsx` | Dois casos de feedback após confirmação: sucesso e falha da API simulada |

Os testes de ações de Estipulantes e dos consumidores de alerta/carregamento exercitam temas claro, escuro e sistema, incluindo ambas as preferências do sistema. Conferem tema resolvido, DOM, textos, papéis acessíveis e callbacks, reutilizando os componentes e tokens existentes. São testes em jsdom, sem homologação visual de layout, contraste ou navegação autenticada em navegador.

## Validações

| Verificação | Resultado |
|---|---|
| `npm.cmd run typecheck` | 82 erros em 28 arquivos; antes, 180 em 67 arquivos |
| `npm.cmd run test:run` | 291 aprovados, zero falhas, 46 arquivos; 63 casos adicionados |
| `npm.cmd run build` | Aprovado |
| `npm.cmd run lint` | 116 erros e 9 avisos; mesmas mensagens da etapa anterior |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 286 arquivos pendentes, mesma contagem da etapa anterior; os quatro testes novos e a suíte de Ramos estão formatados |
| `git diff --check` | Aprovado |

Relatórios locais em `scratch/componentes-etapa3-*`, ignorados pelo Git. Não houve consulta ao ambiente publicado nem execução de operações reais nas APIs. Backend não alterado e seus testes não foram executados.

## Arquivos e grupos alterados

- Componentes, modais, abas e página de formulário de Apólices.
- Formulário, detalhe e edição de Clientes; formulário, listas, detalhe e edição de Estipulantes.
- Formulário, listagem e rotas de Cooperados; páginas de Corretoras.
- Formulário/listagem de Módulos; páginas/rotas de Ramos, Seguradoras e Subestipulantes; catálogo de seguros.
- Formulários de Perfil/Usuário e páginas de Segurança com variantes ou tamanhos antigos.
- `SearchField`, catálogo visual `DesignSystemPage` e `tests/Components.test.tsx`.
- Quatro testes novos e regressões na suíte de Ramos indicados acima.
- Documentação: guia do design system e registros de continuidade 29, 30 e 31.

## Próximo lote

Atualização de 21/10/2026: o [lote de formulários](32-estabilizacao-formularios.md) tratou Clientes, Estipulantes, Cooperados, Módulos e vínculo de Ramos. O estado passou a 57 erros de tipos e 321 testes aprovados. As contagens acima preservam o encerramento do lote de componentes; seguir com os contratos de Usuários e da API.

Alinhar os tipos de entrada e saída dos schemas Zod com React Hook Form e depois os DTOs efetivos da API, preservando as validações e os formatos enviados. Restam também referências à hierarquia anterior de Apólices e imports/variáveis que se tornaram desnecessários. A revisão de formulários deve conferir as associações de rótulos/erros em usos antigos de `Controller` envolvidos por `FormField`, conforme o [guia](13-componentes-design-system.md#formfield-com-campos-controlados--21102026).

Lint, cores fixas, formatação global e homologação em navegador continuam pendentes e serão tratados em lotes próprios. Este registro não representa conclusão da estabilização global ou implementação do dashboard do esboço.
