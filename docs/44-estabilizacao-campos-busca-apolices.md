# Estabilização dos Campos de Busca de Apólices

Data: 22/10/2026. Continuação da [revisão dos hooks de consulta](43-estabilizacao-consultas-detalhe-vidas-historico-universo.md).

## Resultado

Revisados `AsyncEntityField`, `EntityNameResolver` e `ClienteAsyncSelect`. O lint global passou de 23 para 18 erros, mantendo quatro avisos. Removidos os cinco apontamentos de `set-state-in-effect` desses componentes, sem novos apontamentos por arquivo/regra/severidade. Os nove arquivos TypeScript deste lote estão sem erros ou avisos de ESLint.

A suíte completa passou de 790 para 851 testes aprovados, em 65 arquivos. TypeScript e build aprovados. Foram acrescentados 60 testes dos componentes e um de contrato HTTP; os testes existentes de formulários foram adaptados para conferir também o sinal de cancelamento.

## Comportamento e decisões

- **Campo compartilhado:** `ClienteAsyncSelect` passou a usar `EntitySearchField`, já existente em `AsyncEntityField`. Removidas a interface de busca duplicada, a sincronização por efeito e a construção de um `ClienteListItem` incompleto por cast. O componente de Clientes continua expondo `value`, `onChange`, `initialNome`, `disabled` e `error`, além de encaminhar atributos de acessibilidade e `onBlur` ao campo.
- **Consulta:** os campos e o resolvedor reutilizam o ciclo local de `useConsultaApolice`. Não foi criada infraestrutura global de busca. Alterar o termo oculta imediatamente as opções anteriores e encerra a operação antiga. Dados e erros tardios são descartados mesmo se o serviço simulado ignorar o cancelamento.
- **Digitação:** preservados os intervalos de 350 ms para entidades e 500 ms para Clientes. A espera agora pertence à operação cancelável e também se aplica à abertura e à nova tentativa. Fechar, selecionar, desabilitar, trocar o contexto ou desmontar o campo cancela tanto essa espera quanto a chamada em andamento. O spinner cobre a espera e a consulta; opções antigas não ficam disponíveis para seleção nesse período. O timer agrupa a digitação, não adia artificialmente alterações de estado para contornar o lint.
- **Seleção e reset:** rótulos iniciais são derivados do valor recebido. Rótulo tardio, troca de identificador ou limpeza externa não reutilizam a busca anterior. O rascunho é vinculado ao contexto do campo e descartado por ajuste condicionado durante a renderização, sem efeito de sincronização. Ao editar uma seleção, o identificador é limpo e o texto digitado é preservado. Ao fechar sem editar, o nome selecionado volta a ser exibido.
- **Interação:** opções usam o `Button` do design system, permitindo ativação por mouse ou teclado sem submeter o formulário. Clique externo, Escape e saída de foco do conjunto fecham a lista. Mover o foco do input para opções ou nova tentativa mantém a lista disponível. O nome selecionado fica separado do termo usado na busca: ao focar, entidades consultam a lista geral e Clientes conservam a busca pelo nome, conforme o comportamento anterior.
- **Falha e vazio:** ambos os seletores apresentam falha com texto e ação de tentar novamente. Clientes deixa de apresentar falha de rede como lista vazia. Vazio mantém as mensagens específicas existentes; cancelamento não vira alerta. A nova tentativa permanece vinculada ao termo atual e é abortada na desmontagem.
- **Funções estáveis:** as três buscas de `ApoliceForm` foram movidas para o escopo do módulo, pois não dependem de props ou estado. Renderizar novamente o formulário não muda a função de busca nem reinicia a consulta. Os filtros, nomes e identificadores numéricos usados na gravação foram preservados.
- **Cancelamento até a API:** as buscas encaminham `AbortSignal` às funções existentes de Estipulantes, Seguradoras e Clientes. `corretorasApi.listar` recebeu o segundo argumento opcional `signal?: AbortSignal`, encaminhado ao cliente HTTP. Chamadas anteriores sem sinal continuam válidas, com os mesmos filtros, endpoint e resposta.
- **Resolvedor de nome:** preservados o critério de identificador com 36 caracteres e a prioridade razão social → nome fantasia → nome → mensagem de nome ausente. Troca de identificador/função oculta o resultado anterior e cancela a chamada. Identificador inválido não consulta nem apresenta conteúdo. Os estados usam os tokens visuais existentes.

Não houve alteração de banco, campos persistidos, classificação de textos humanos, permissões, dependências ou contratos REST. Buscas não são reformatadas: o texto digitado é enviado como antes. A normalização compartilhada e a preservação de evidências continuam conforme o documento 26. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Primeira rodada direcionada | 80 aprovados, quatro arquivos |
| Cenários do lote na suíte final | 86 aprovados: 42 dos campos, 18 do resolvedor, 12 de formulários e 14 de HTTP |
| `npm.cmd run test:run` | 851 aprovados, zero falhas, 65 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint dos cinco arquivos de produção e quatro testes | Zero erros e avisos |
| `npm.cmd run lint` global | 18 erros e quatro avisos; antes, 23 e quatro |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 271 arquivos pendentes; antes, 274 |
| Prettier dos três componentes e quatro testes | Aprovado |
| `git diff --check` | Aprovado |

Os testes dos campos cobrem debounce com relógio simulado, preservação dos filtros de Clientes ativos, cancelamento em espera e em chamada, respostas/falhas antigas, seleção, edição de seleção, rótulo tardio, reset e troca externos, foco, Escape, desabilitação, desmontagem, falha/vazio, nova tentativa e `StrictMode`. O resolvedor cobre identificadores ausentes/inválidos, prioridade dos nomes, consulta antiga, troca de função, recuperação de falha e desmontagem. O contrato HTTP novo confirma a propagação de cancelamento na listagem de Corretoras, preservando busca e `ativo: false`.

Os três componentes foram exercitados em DOM nos temas claro, escuro e sistema, incluindo sistema claro e escuro. As opções usam o componente `Button`, com seus estados de interação e tokens existentes. Nenhum estilo com cor fixa ou token novo foi introduzido. A ativação por teclado é simulada como click de botão com `detail: 0`; isso não equivale a uma avaliação completa de navegação e apresentação em navegador.

A primeira passagem de ESLint apontou duas dependências desnecessárias no novo `useMemo`; elas foram removidas. Não houve falhas nos testes executados ou na verificação de tipos. A rodada final ampliou os testes de foco e temas. O comparativo final não apresenta novos avisos ou erros. A formatação foi aplicada aos componentes reestruturados e testes, preservando alterações mínimas em `ApoliceForm` e na API de Corretoras.

As validações são locais, com APIs/fetch simulados. Permanecem pendentes homologação visual e integrada com autenticação, sete ocorrências do design system e formatação geral. Backend não alterado; seus testes não foram executados. Não foram adicionados `any`, casts para esconder contratos, supressões ou relaxamentos de compilação.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa16-*`, incluindo retrato inicial, testes, build, tipos e comparativo de lint.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Produção: `src/features/apolices/components/forms/AsyncEntityField.tsx`, `EntityNameResolver.tsx` e `ApoliceForm.tsx`; `src/features/apolices/components/tabs/ClienteAsyncSelect.tsx`; `src/features/corretoras/api/corretoras.api.ts`.
- Testes novos: `src/features/apolices/components/forms/AsyncEntityFields.test.tsx` e `EntityNameResolver.test.tsx`.
- Testes atualizados: `tests/forms/ApoliceContracts.test.tsx` e `tests/http/FeatureContracts.test.ts`.
- Documentação: este registro e continuidade nos documentos 29 e 43.

## Próxima etapa

A revisão de `SubestipulanteApoliceModal` e `ApoliceRamoFormModal` foi concluída no [lote de modais de vínculos](45-estabilizacao-modais-vinculos-apolices.md), com 903 testes aprovados e lint global em 14 erros e quatro avisos. Os resultados acima preservam o retrato desta entrega. A próxima etapa é revisar as listagens de Corretoras, Módulos, Ramos e Seguradoras.

Depois, continuar pelos demais efeitos e regras de lint. Design system, formatação geral e homologação integrada permanecem no [plano de estabilização](29-estabilizacao-frontend.md).
