# Estabilização do Frontend

A [revisão geral de arquitetura, segurança e qualidade de 23/10/2026](76-revisao-arquitetura-seguranca-qualidade.md) registra pendências do backend, segurança e operação. O fechamento dos checks frontend descrito aqui não encerra essas pendências.

Data: 21/10/2026. Primeira entrega: recuperação dos testes. A estabilização geral permanece em andamento.

Estado atual, atualizado em 23/10/2026: a estabilização técnica do frontend está concluída nas verificações locais, com zero erros de TypeScript, 1.125 testes aprovados, build aprovado, ESLint sem erros ou avisos, scanner de design system sem ocorrências e Prettier global aprovado. O [fechamento da formatação](75-conclusao-formatacao-frontend.md) eliminou as 168 pendências restantes, preservando as alterações locais anteriores. A [estabilização de tipos](35-conclusao-estabilizacao-tipos.md), a [correção dos tokens](53-estabilizacao-tokens-design-system.md) e a [correção do envio de cidade](56-correcao-envio-cidade-cooperados.md) permanecem registradas nas respectivas entregas. A homologação integrada permanece pendente; os checks locais não constituem auditoria visual integral nem comprovam a paridade funcional com o legado. As tabelas abaixo preservam o retrato da primeira entrega; resultados posteriores estão no registro de cada etapa.

## Escopo

Recuperar a suíte existente, corrigir defeitos de formulário evidenciados pelos testes e registrar uma base verificável. Não implementa regras novas de negócio nem acessa banco, Keycloak ou serviços externos.

`ClienteForm.tsx`, `ClienteDetalhePage.tsx` e o teste não versionado `ClienteForm.test.tsx` já estavam alterados antes desta etapa. Foram preservados. Os testes dos contatos principais também foram executados.

## Correções

- A suíte de `RoleProtectedRoute`, que importava um componente removido, foi substituída por testes de `PermissionProtectedRoute`: carregamento, erro, usuário indisponível/inativo, permissão, módulo habilitado, acesso total e função de operador. Módulo desabilitado bloqueia também acesso total.
- Home, layout e páginas de Clientes declaram a autorização necessária ao cenário. O auxiliar `tests/helpers/authorization.ts` nega acesso por padrão. Não existe concessão global em `tests/setup.ts` nem alteração de permissões de produção. Home ainda exerce seu comportamento atual baseado em roles para o cartão Clientes; sua migração para permissões granulares não integra esta entrega.
- Ramos passou de APIs do Jest para Vitest, preservando os cenários de leitura, permissões e abertura do modal, sem instalar dependências.
- `FormField` passou para dentro do `render` do `Controller`, envolvendo o campo real. Antes, o rótulo e os atributos de erro injetados no Controller não alcançavam o HTML. Corrigidos CNPJ de Seguradora/Subestipulante e CEP, cidade, telefone institucional e início/fim de vigência de Estipulante.
- Regressão verifica rótulos, `aria-invalid`, `aria-describedby` e rejeição de CNPJ inválido. Renderização DOM exercitada em claro, escuro e sistema, incluindo sistema claro/escuro. Não houve alteração de estilos.
- Os testes usam eventos de entrada para máscaras, conferem CNPJ formatado na tela e em dígitos no envio e usam CPF/CNPJ válidos nos cenários de sucesso. As validações de produção não foram flexibilizadas.
- Conflito de Clientes usa `HttpApiError`, conforme o cliente HTTP. Consulta de status delimitada à tabela para não confundir o resultado com a opção do filtro.

## Verificação de tipos

O comando anterior, `tsc --noEmit`, era executado sobre um `tsconfig.json` com `files: []` e referências a outros projetos. Não verificava os projetos referenciados e podia indicar sucesso sem analisar a aplicação.

`npm run typecheck` agora usa `tsc -b --pretty false`, verificando aplicação/testes e configuração do Vite. Reprova com erros reais pendentes de correção. O build do Vite não substitui essa verificação.

## Validações executadas

| Verificação | Resultado |
|---|---|
| Testes iniciais | 179 aprovados, 28 falhando e duas suítes sem carregar |
| `npm.cmd run test:run` | 228 aprovados, zero falhas, 42 arquivos |
| `npm.cmd run build` | Aprovado |
| ESLint dos 16 arquivos TypeScript desta etapa | Zero erros e avisos |
| `npm.cmd run typecheck` corrigido | Reprovado: 284 erros em 92 arquivos |
| `npm.cmd run lint` global | Reprovado: 126 erros e 9 avisos; inicialmente 128 erros e 9 avisos |
| `npm.cmd run lint:design-system` | Reprovado: sete ocorrências em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | Reprovado: 288 arquivos na primeira execução desta etapa |
| `git diff --check` | Aprovado |

No Windows, `npm.cmd` usa o lançador instalado sem mudar a política do PowerShell. Não foram instaladas dependências nem desativadas verificações. Testes em jsdom não constituem homologação visual em navegador ou verificação do ambiente publicado. Backend não alterado; seus testes não foram executados.

Relatórios locais ignorados pelo Git: `scratch/estabilizacao-frontend-*.json` e `.log`. Contagens incluem as alterações locais preexistentes de Clientes.

## Continuidade em etapas

1. **Testes do frontend:** concluídos nesta entrega.
2. **Tipos:** concluídos no [fechamento da estabilização](35-conclusao-estabilizacao-tipos.md), com zero erros no compilador, 389 testes aprovados e build aprovado. Foram mantidas as opções e a abrangência da verificação; a dívida de lint foi concluída posteriormente, conforme os registros de continuidade abaixo.
3. **Lint, scanner de design system e formatação:** concluídos nas verificações locais. Os lotes [36](36-estabilizacao-lint-testes-apolices.md), [37](37-estabilizacao-erros-apolices.md), [38](38-estabilizacao-erros-cadastros.md), [39](39-estabilizacao-erros-estipulantes-cooperados.md), [40](40-estabilizacao-erros-seguranca.md), [41](41-estabilizacao-tipos-campos-seletor.md), [42](42-estabilizacao-efeitos-consultas-apolices.md), [43](43-estabilizacao-consultas-detalhe-vidas-historico-universo.md), [44](44-estabilizacao-campos-busca-apolices.md), [45](45-estabilizacao-modais-vinculos-apolices.md), [46](46-estabilizacao-listagens-cadastros.md), [47](47-estabilizacao-consultas-cooperados.md), [48](48-estabilizacao-rotas-cooperados.md), [49](49-estabilizacao-autorizacao.md), [50](50-estabilizacao-constantes-validadores.md), [51](51-estabilizacao-assinaturas-formularios.md), [52](52-correcao-cidade-cep-cooperados.md) e [53](53-estabilizacao-tokens-design-system.md) corrigiram testes, tratamento de erros, tipos de campos/seletores e o ciclo dos oito hooks de consulta, os campos de busca e os modais de vínculos de Apólices e as listagens de Corretoras, Módulos, Ramos e Seguradoras e as consultas/rotas de Cooperados, além do carregamento de autorizações por sessão e das quatro declarações com `prefer-const`. Os três avisos dos formulários foram eliminados, com assinaturas de `useWatch`; o lint global está com zero erros e zero avisos. A seleção automática de cidade após CEP em Cooperados foi corrigida no lote 52, com cancelamento por UF, descarte de respostas antigas e nova tentativa em falha. As sete ocorrências de design system foram corrigidas no lote 53, junto aos aliases do seletor e à escolha automática de contraste. O [lote 54](54-formatacao-tokens-identidade.md) padronizou oito arquivos de tokens/identidade e reduziu a dívida de formatação de 262 para 254 arquivos, mantendo os 12 arquivos do escopo aprovados no check restrito. O [lote 55](55-formatacao-componentes-formulario.md) formatou oito arquivos de `Button`, `Input`, `Select` e `FormField`, reduzindo a dívida para 246. A falha intermitente no cenário de cidade vazia de Cooperados foi reproduzida e corrigida no [lote 56](56-correcao-envio-cidade-cooperados.md), com 1.125 testes aprovados. O [lote 57](57-formatacao-checkbox-textarea-campos.md) concluiu `Checkbox`, `Textarea` e `components/fields`, retirando 11 pendências de formatação e mantendo 1.125 testes aprovados. O [lote 58](58-formatacao-estrutura-formularios.md) concluiu `FormSection`, `FormGrid`, `FormActions` e `ReadOnlyField`, retirando sete pendências e mantendo 1.125 testes aprovados. O [lote 59](59-formatacao-alertas-indicadores.md) conferiu os nove arquivos de `Alert`, `Badge` e `EmptyState`, formatou dois e reduziu a dívida para 226, com 1.125 testes aprovados. O [lote 60](60-formatacao-cards-carregamento.md) conferiu os nove arquivos de `Card`, `Spinner` e `Skeleton`, formatou três e reduziu a dívida para 223, com 1.125 testes aprovados. O [lote 61](61-formatacao-modais-confirmacao.md) conferiu os seis arquivos de `Modal` e `ConfirmDialog`, formatou dois e reduziu a dívida para 221, com 1.125 testes aprovados. O [lote 62](62-formatacao-tabelas-paginacao.md) conferiu os seis arquivos de `Table` e `Pagination`, formatou três (um deles apenas com finais de linha LF) e reduziu a dívida para 218, com 1.125 testes aprovados. O [lote 63](63-formatacao-datatable-resumo.md) conferiu os seis arquivos de `DataTable` e `ResultsSummary`, formatou dois e reduziu a dívida para 216, com 1.125 testes aprovados. O [lote 64](64-formatacao-filtros-busca.md) conferiu os seis arquivos de `FilterBar` e `SearchField`, formatou três e reduziu a dívida para 213, com 1.125 testes aprovados. O [lote 65](65-formatacao-menus-acoes.md) conferiu os seis arquivos de `DropdownMenu` e `RowActions`, formatou quatro e reduziu a dívida para 209, com 1.125 testes aprovados. O [lote 66](66-formatacao-navegacao-cabecalho.md) conferiu os seis arquivos de `Breadcrumbs` e `PageHeader`, formatou três (um deles apenas com finais de linha LF) e reduziu a dívida para 206, com 1.125 testes aprovados. O [lote 67](67-formatacao-detalhes-resumo.md) conferiu os oito arquivos de `DescriptionList`, `DetailsSection` e `EntitySummary`, formatou cinco (um deles apenas com finais de linha LF) e reduziu a dívida para 201, com 1.125 testes aprovados. O [lote 68](68-formatacao-status-icones.md) conferiu os quatro arquivos de `StatusBadge`, ícones e exportações, formatou dois e reduziu a dívida para 199, com 1.125 testes aprovados. Todo o diretório `components/ui` passou no Prettier. O [lote 69](69-formatacao-layout-autenticado.md) conferiu os oito arquivos de `AuthenticatedLayout`, formatou seis e reduziu a dívida para 193, com 1.125 testes aprovados. Todo o diretório `layouts` passou no Prettier. O [lote 70](70-formatacao-nucleo-app.md) conferiu os nove arquivos de `src/app`, formatou oito (dois deles apenas com finais de linha LF) e reduziu a dívida para 185, com 1.125 testes aprovados. Todo o diretório `src/app` passou no Prettier. O [lote 71](71-formatacao-autenticacao.md) conferiu os 11 arquivos de `src/auth`, formatou cinco (um deles apenas com finais de linha LF) e reduziu a dívida para 180, com 1.125 testes aprovados. Todo o diretório `src/auth` passou no Prettier. O [lote 72](72-formatacao-aplicacao-tema.md) conferiu os sete arquivos dos componentes de aplicação e contexto de tema, formatou dois e reduziu a dívida para 178, com 1.125 testes aprovados. O [lote 73](73-formatacao-cliente-http.md) conferiu os sete arquivos de `services/http`, formatou seis (um deles apenas com finais de linha LF) e reduziu a dívida para 172, com 1.125 testes aprovados. O [lote 74](74-formatacao-utilitarios-compartilhados.md) conferiu os seis arquivos de API, configuração, hooks e utilitários compartilhados, formatou quatro (um deles apenas com finais de linha LF) e reduziu a dívida para 168, com 1.125 testes aprovados. O [fechamento 75](75-conclusao-formatacao-frontend.md) tratou todos os 168 arquivos restantes em uma etapa única, conforme solicitado pelo usuário. O Prettier global passou com zero pendências, mantendo 1.125 testes aprovados e os demais checks de código aprovados. Não restam lotes de formatação nesta sequência; seguir para a homologação integrada.
4. **Homologação:** conferir fluxos autenticados e apresentação em navegador nos temas suportados.
5. **Próxima entrega funcional:** completar manutenção de vínculos, dependentes/cônjuge e dados pessoais necessários, levantando as rotinas no legado. Depois, concluir Propostas alinhadas a Apólice → Módulo → Plano → Coberturas. Financeiro vem após essa base.

Referências: [sequência funcional](17-modelagem-banco-dados-webapolice.md#sequência-recomendada-de-evolução), [estado de Clientes](19-modulo-clientes-detalhes.md) e [Planos/Coberturas atuais](22-plano-coberturas-modulo-apolice.md).

## Arquivos da entrega

- Produção: `EstipulanteForm.tsx`, `SeguradoraForm.tsx`, `SubestipulanteForm.tsx` e somente o script `typecheck` em `apps/web/package.json`.
- Testes atualizados: Ramos, cadastro/listagem/edição de Clientes, cadastro/edição de Estipulantes, formulário de Seguradora/Subestipulante, Home e layout autenticado.
- Novos: `PermissionProtectedRoute.test.tsx`, `FormAccessibility.test.tsx` e auxiliar `tests/helpers/authorization.ts`. A suíte antiga `RoleProtectedRoute.test.tsx` foi substituída.
- Documentação: este registro, fundação do frontend, guia de `FormField` e atualização de estado do detalhe de Clientes.

Nenhuma migration, estrutura de banco, classificação de campo humano ou regra de Title Case foi alterada.
