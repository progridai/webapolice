# Alteração e Retirada do Pedido de Cancelamento de Vigência

Data: 17/11/2026. Complementa o [documento 87](87-encerramento-vigencia-descarte-propostas.md).

Continuidade: a reinclusão com nova versão após cancelamento com vigência consta
no [documento 92](92-reinclusao-propostas.md). As pendências abaixo registram o
recorte da entrega de gestão do pedido.

## Regras Definidas Antes da Implementação

O responsável definiu que o pedido pode ser alterado ou retirado até sua efetivação,
inclusive com a data já alcançada e pendência de processamento. Alteração aceita
hoje, retroativa válida ou futura no calendário de São Paulo. Hoje/retroativa
efetiva o cancelamento na mesma transação; futura mantém a proposta vigente até
o processamento. A nova data deve ser diferente da registrada, igual/posterior
ao início da proposta e pertencer aos períodos conhecidos da Apólice/Vida.

Somente propostas nativas, confirmadas, visíveis, não excluídas e ainda vigentes,
com pedido completo, podem receber as operações. Preservar unicidade na Vida e
bloqueio por sucessora não encerrada. Não exigir contexto ativo ou nova
elegibilidade de Plano/Coberturas/beneficiários para gerir o encerramento.
Cancelamento já efetivado nunca é desfeito por estas operações.

Revisão esperada e justificativa não vazia de até 1.000 caracteres são obrigatórias.
Reutilizar bloqueios de Apólice/Proposta e a transação física da auditoria. Se o
processador efetivar primeiro, a gestão será recusada; se a gestão concluir
primeiro, uma efetivação com a revisão antiga será recusada e a fila relida.

A alteração preserva `cancelamento_solicitado_em` como instante original e
acrescenta evento com data anterior, nova data e justificativa. A retirada limpa
somente os metadados do pedido ativo e sua pendência, mantém a proposta vigente
e acrescenta evento com a data retirada e justificativa. Histórico e auditorias
anteriores permanecem intactos. Nova solicitação posterior usa o fluxo existente.
Limpar pendência também após alteração validada; falhas revertem tudo.

Não alterar a Vida, condições, prêmios, beneficiários, versão de negócio ou
financeiro. Não reativar predecessoras. Não criar schema, catálogo, permissão,
dependência ou campo humano novo. Justificativas/eventos reutilizam
`Observacao`/`Motivo`, classificados em C#, TypeScript e SQL; a normalização
compartilhada permanece obrigatória. Não reprocessar textos históricos.

## Contratos e Interface Implementados

- `POST /api/propostas/{publicId}/vigencia/cancelamento/alterar`: revisão,
  `dataCancelamentoVigencia` e `observacao`; resultado de cancelamento existente.
- `POST /api/propostas/{publicId}/vigencia/cancelamento/retirar`: revisão e
  `observacao`; retorna a nova revisão.
- Detalhe acrescenta `podeAlterarCancelamento` e `podeRetirarCancelamento`.
  Alterar para data futura exige processador habilitado. Retirar independe dele
  e do catálogo de cancelada; alterar exige catálogo de cancelamento compatível.
- Ambas usam `propostas.alterar` e os gates APOLICES/PROPOSTAS existentes.

Formulários reutilizam componentes, schema de justificativa, cliente HTTP e
bloqueios de navegação/operações do encerramento. Explicar efeitos, solicitar
confirmação explícita, preservar entradas após erro, oferecer recarga e impedir
envios duplicados desde a validação. Sucesso recarrega detalhe e histórico.

O formulário apresenta a data registrada, mantém a nova data vazia para escolha
explícita e não oferece campo de data na retirada. A gestão também aparece em
`CANCELAMENTO_PENDENTE`. Canceladas continuam somente para consulta. Permissões e
flags do servidor determinam as ações disponíveis; indisponibilidade de outro
formulário ou envio bloqueia os comandos. Os componentes existentes fornecem
rótulos, mensagens associadas, foco e estilos dos três temas; os testes DOM não
substituem a conferência visual/teclado em navegador.

## Arquivos da Etapa

- Backend: `EncerrarProposta/GerirCancelamentoVigenciaCommands.cs` (novos handlers),
  `EncerramentoPropostas.cs` (validação compartilhada de data/pedido),
  `SolicitarCancelamentoVigenciaCommand.cs` (reutilização da validação),
  `ObterPropostaQuery.cs` (flags) e `PropostasController.cs` (duas rotas).
- Frontend: `EncerramentoProposta.tsx`, `encerramentoProposta.schema.ts`,
  `propostas.api.ts`, `proposta.types.ts` e `propostas.fixtures.ts`.
- Testes: novos `PropostasGestaoCancelamentoTests.cs`,
  `GestaoPedidoCancelamentoProposta.test.tsx` e `gestaoCancelamento.api.test.ts`;
  extensão de `PropostasFluxoAuthTests.cs` para as duas rotas.
- Documentação: este documento e continuidades nos documentos 05, 26 e 87.

## Conferência Inicial e Escopo

HEAD inicial: `801551c`, após `ed53a5d`. Preservar as alterações locais em
`docs/89-atalhos-pessoais-home.md`, `backend/scripts/aplicar-atalhos-home.ps1` e
`docs/90-correcao-login-migration-atalhos.md`. Não criar commit ou push.

Na conferência inicial, o código confirmou solicitação/efetivação existentes e
ausência de alteração ou retirada, implementadas nesta etapa. Reinclusão após
vigência, revisão explícita das condições e regras
financeiras permanecem em etapas próprias. A consulta atual às superfícies
retornou `apps: []` e `browsers: []`; homologação visual autenticada permanece
pendente. Não enfraquecer autenticação para viabilizá-la.

## Validação da Entrega

Verificações executadas nesta entrega, distintas dos resultados históricos dos
documentos anteriores:

| Verificação | Resultado |
| --- | --- |
| `dotnet build backend/WebApolice.slnx --no-restore --verbosity minimal` | Aprovado; zero erros e avisos |
| `dotnet test backend/WebApolice.slnx --no-build --no-restore --verbosity quiet -m:1` | 697 aprovados, zero falhas/ignorados |
| `npm.cmd run format:check` | Aprovado; nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado; zero erros |
| `npm.cmd run lint` | Aprovado; zero erros/avisos |
| `npm.cmd run lint:design-system` | Aprovado; zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.344 aprovados em 89 arquivos |
| `npm.cmd run build` | Aprovado |
| Scanner local de segredos e `git diff --check` | Aprovados |

Backend: 158 API, 28 Arquitetura, 2 Auditoria Integration, 6 Auditoria Unit,
433 Integration e 70 Cadastro. Os 119 testes PostgreSQL de Propostas e os
13 testes de autorização do fluxo também foram executados separadamente e
aprovados; esses números já pertencem ao total, não são somados novamente.
Acrescentados 30 cenários de integração, 2 casos de autorização e 15 de frontend.

Cobertura: alteração com data retroativa/hoje/futura; instante original do pedido;
gestão com data vencida e pendência; retirada sem contexto ativo, processador ou
catálogo de cancelada; preservação da Vida, condições e histórico; limpeza de
pendência; retirada da fila; nova solicitação; datas iguais/ausentes/fora dos
períodos; revisão ausente/antiga; justificativa vazia/longa; legado, ocultação,
exclusão e cancelamento já efetivado; rollback integral por falha de auditoria;
disputa com efetivação nas duas ordens, por duas conexões; processador relendo
nova data, fronteira de meia-noite de São Paulo e repetição segura. API verifica
401/403, catálogo indisponível mesmo com Acesso Total, UUID da rota e Title Case.

Frontend cobre os três temas no DOM, gestão pendente, data anterior, data vazia
para escolha explícita, cancelamento imediato conforme resposta do servidor,
justificativa, voltar sem gravar, conflito/entradas preservadas/recarga,
permissões/flags, bloqueio de navegação e duplicidade durante validação e envio.
O teste das duas funções de API usa o cliente HTTP real para verificar rota,
revisão, data técnica e normalização da justificativa.

O check global de formatação inicialmente apontou o novo teste de interface;
corrigido com o Prettier existente e repetido com sucesso. A primeira rodada
completa backend em execução restrita encontrou acesso negado ao Event Log do
Windows e ao pipe do Docker, impedindo startup/containers. A repetição com acesso
local aprovado pela revisão automática passou integralmente, com a configuração
padrão de logging e sem alterar código, autenticação ou critérios dos testes.
Nenhum teste foi excluído ou timeout/limite flexibilizado. O frontend usou dois
workers, mantendo a suíte integral. O build mantém o aviso preexistente de chunk
acima de 500 kB e o aviso informativo de tempo nos plugins CSS, sem mudar limites.

As integrações utilizam exclusivamente PostgreSQL efêmero via Docker. Nenhum
pedido real foi alterado, retirado ou efetivado para ensaio. Não houve aplicação
de migrations nas bases existentes, reaplicação de triggers, alteração de
configuração externa ou reinício/publicação da API em execução.

Nenhuma migration foi criada ou aplicada nesta etapa; schema e triggers
existentes são suficientes. As três alterações locais anteriores foram
preservadas. Não houve commit ou push. Permanecem pendentes a homologação visual
autenticada, reinclusão após cancelamento com vigência, revisão explícita de
condições e regras financeiras. CPF/Pessoa do beneficiário continuam adiados.
