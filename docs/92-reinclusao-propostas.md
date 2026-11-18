# Reinclusão Após Cancelamento de Proposta com Vigência

Data: 17/11/2026. Status: implementada e validada automaticamente;
homologação visual autenticada pendente.
Continuidade dos documentos [87](87-encerramento-vigencia-descarte-propostas.md)
e [91](91-gestao-pedido-cancelamento-vigencia.md).

## Escopo da Implementação

Criar uma nova versão de Pré Proposta ligada explicitamente à proposta cancelada
que já teve vigência. Preservar integralmente a encerrada, suas condições,
datas, histórico e auditoria. A preparação não concede cobertura; a confirmação
e eventual efetivação futura seguem o fluxo operacional, revalidando a
elegibilidade atual. Numeração crescente, sem reutilizar tentativas encerradas.

Não reativar propostas anteriores, alterar a situação da Vida ou gerar
financeiro. O fluxo continua restrito a propostas nativas. Cancelamento antes
do início e descarte de pré proposta permanecem tentativas encerradas sem vigência,
distintas da reinclusão. Não alterar os contratos existentes silenciosamente.

## Decisões Aprovadas Antes da Implementação

O responsável definiu:

1. O usuário escolhe explicitamente entre configuração atual do Módulo e
   condições registradas na encerrada. A segunda opção preserva prêmios,
   ajustes, bases e justificativas; ambas continuam sujeitas à elegibilidade
   atual. Não transportar ajustes antigos ao escolher o Módulo.
2. Aceitar hoje, retroativa válida ou futura. Respeitar períodos da Apólice/Vida
   e impedir início anterior a `DataFimVigencia` da encerrada (primeiro dia sem
   cobertura). Eventual intervalo permanece sem cobertura.
3. Copiar somente beneficiários nativos não excluídos para revisão/edição,
   com UUIDs novos, sem alterar os originais. A confirmação exige dados
   completos e 100%; CPF/Pessoa/evidências legadas não são copiados.

Manter validações atuais de Apólice/Vida/identidade/Plano/Coberturas e impedir
tentativas concorrentes. A origem selecionada deve ser o último encerramento
com vigência da mesma Vida; posteriores só podem ser tentativas reconhecidas
como descartadas ou canceladas antes do início. Ocultação/exclusão não liberam
nova tentativa. Exigir revisão esperada da origem e `propostas.inserir`.

## Diagnóstico Técnico

`CriarPropostaHandler` aceita origem somente vigente e impede nova adesão sem
origem quando já existe proposta que teve vigência. `VigenciaPropostas` exige
origem vigente na confirmação/efetivação e altera seus metadados ao substituir.
Portanto, apenas liberar a criação seria insuficiente e poderia reescrever o
fim registrado de uma proposta cancelada.

A implementação deverá distinguir origem de reinclusão e origem de substituição
durante criação, confirmação, reagendamento e processamento após reinício.
Reutilizar `PropostaOrigemId` e os metadados já registrados de cancelamento com
vigência para distinguir a origem encerrada. Não é necessária coluna nova: a
operação explícita registra a escolha das condições no histórico existente.
A versão encerrada não participa do encerramento de origem da substituição.
Na confirmação/efetivação/reagendamento, revalidar origem cancelada e ausência
de outra vigente/tentativa aberta. Não atualizar nenhum campo da encerrada.

Novo POST `/api/propostas/{publicId}/reinclusao`: `revisaoEsperada` e
`origemCondicoes` (`ANTERIOR` ou `MODULO`) obrigatórios, retornando o UUID novo.
Escolhas/códigos, flags de consulta e UUIDs são técnicos. Nomes, parentescos,
justificativas e motivos reutilizam campos humanos já classificados nas três
listas e os normalizadores compartilhados. Não copiar evidências legadas,
datas operacionais ou dados financeiros de cobrança da proposta encerrada.

Manter bloqueios de Apólice/Proposta, revisão técnica e histórico/auditoria na
mesma transação física. Revalidar ausência de vigente e de tentativa aberta sob
os locks; não escolher implicitamente uma origem histórica ambígua. Consulta e
interface devem explicar a origem, a preparação sem vigência e o eventual
intervalo sem cobertura. Preservar os fluxos de tentativa após descarte ou
cancelamento antes do início.

## Validação

Cobrir criação e confirmação imediata/futura, limites de datas, reinício do
processador, concorrência de preparações/confirmações, revisão antiga, rollback
de auditoria, condições e beneficiários conforme as decisões, preservação
integral da encerrada, tentativas descartadas/canceladas sem reutilizar versão,
origem legada/oculta/incompatível e autorização por UUID/permissão/catálogo.

Frontend seguirá o guia obrigatório, temas e tokens existentes, bloqueios de
operação/navegação, prevenção de duplicidade, erro com entradas preservadas e
recarga. Executar os seis checks e testes backend pertinentes após implementar.
Usar PostgreSQL efêmero nos testes, sem alterar propostas existentes para ensaio.

Os resultados do documento 91 pertencem à entrega anterior. Nesta etapa foram
executados e aprovados os 149 testes PostgreSQL de Propostas, incluindo 30
cenários novos de reinclusão; o teste concorrente mantém a primeira transação
aberta na auditoria, confirma o bloqueio da segunda e garante uma única tentativa.

Frontend: os seis checks passaram nesta execução (`format:check`, `typecheck`,
`lint`, `lint:design-system`, `test:run -- --maxWorkers=2` e `build`), com
1.356 testes em 90 arquivos, zero pendências de formatação, tipos, erros/avisos
de lint ou ocorrências no scanner. Foram adicionados 10 cenários de interface
e 2 de API. O build mantém o aviso preexistente de chunk acima de 500 kB e o
aviso informativo de tempo dos plugins CSS/HTML, sem flexibilizar limites.

Os testes dos três temas identificaram seleção automática de Anterior pelo
select nativo, corrigida com valor inicial vazio para exigir escolha explícita.
A expectativa foi mantida e a suíte completa passou após a correção. Ajustes
iniciais das fixtures PostgreSQL corrigiram navegação inversa inexistente e
import do contrato de Cadastro no teste concorrente; nenhuma regra de produção
ou verificação foi flexibilizada. O novo teste HTTP foi formatado com o Prettier
existente; o check global foi repetido e aprovado.

Resultado final desta execução:

| Verificação | Resultado |
| --- | --- |
| `dotnet build backend/WebApolice.slnx --no-restore --verbosity quiet` | Aprovado; zero erros e avisos |
| `dotnet test backend/WebApolice.slnx --no-build --no-restore --verbosity quiet -m:1` | 728 aprovados; zero falhas/ignorados |
| `npm.cmd run format:check` | Aprovado; nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado; zero erros |
| `npm.cmd run lint` | Aprovado; zero erros/avisos |
| `npm.cmd run lint:design-system` | Aprovado; zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.356 aprovados em 90 arquivos |
| `npm.cmd run build` | Aprovado |
| Scanner local de segredos e `git diff --check` | Aprovados |

Backend: 159 API, 28 Arquitetura, 2 Auditoria Integration, 6 Auditoria Unit,
463 Integration e 70 Cadastro. Os 149 testes de Propostas já pertencem ao total
da integração e não são somados novamente. Acrescentados 30 cenários PostgreSQL
e 1 de autorização, além dos 12 de frontend. Os testes de autorização verificam
401, permissão incorreta, indisponibilidade do catálogo mesmo com Acesso Total,
UUID da rota, revisão e preservação do código da escolha.

Nenhuma migration criada ou aplicada: apenas métodos de domínio, contratos de
aplicação/projeções e novos casos de uso, sem propriedades persistidas novas.
As bases existentes, suas propostas, perfis e permissões não foram alterados.
Não foi necessário reaplicar triggers de Title Case. Toda integração PostgreSQL
foi executada em bancos efêmeros Docker, incluindo o processamento agendado.
As alterações locais das etapas anteriores foram preservadas.

## Uso e Arquivos

Nos detalhes da última proposta nativa cancelada com vigência, a ação
**Preparar reinclusão** exige `propostas.inserir` e elegibilidade da origem/Vida.
O formulário começa sem escolha; o usuário seleciona **Proposta anterior** ou
**Módulo atual** e confirma **Criar pré proposta de reinclusão**. Abrir ou voltar não
grava dados. O backend reobtém a origem depois do lock e exige a revisão lida.

A versão criada recebe novos UUIDs de Proposta, Item, Coberturas e Beneficiários,
`MAX(versao)+1`, revisão zero e nenhuma confirmação ou data de vigência. A fonte
das condições fica explícita no evento **Preparação de Reinclusão**. Ao escolher
Anterior, condições indisponíveis são recusadas com orientação para revisar a
configuração ou escolher Módulo; não há substituição silenciosa da escolha.

Conferir condições e beneficiários na pré proposta e usar a confirmação existente
com data informada. Uma data igual ao fim exclusivo da origem representa
continuidade; uma data posterior preserva o intervalo sem cobertura. Na
efetivação, acrescentar **Efetivação de Reinclusão**, mantendo todos os campos
da origem intocados. Início futuro registra **Reinclusão Agendada** e usa o
processador existente, inclusive após reinício e com múltiplas instâncias.

Descarte ou cancelamento antes do início preservam a tentativa e liberam outra
preparação pela origem cancelada, sem reutilizar versão. Depois que a reinclusão
teve vigência, uma reinclusão posterior usa seu próprio encerramento como origem;
a versão mais antiga não pode abrir uma cadeia paralela. Mensagens de
confirmação, efetivação, reagendamento, cancelamento de início e descarte
distinguem a origem cancelada de uma anterior que ainda está vigente.

- Backend: `ReincluirProposta/PrepararReinclusaoCommand.cs` e
  `ReinclusaoPropostas.cs`; `CriarPropostaCommand.cs`/`CriarPropostaHandler.cs`;
  método `CanceladaComVigencia` em `PropostaOperacional.cs`; `VigenciaPropostas.cs`,
  handlers de confirmação/efetivação/reagendamento, controlador, DTO e repositório.
- Frontend: `ReinclusaoProposta.tsx`/schema; API, contratos e fixtures;
  integração em `PropostaDetalhes.tsx`; mensagens em `ConfirmacaoProposta.tsx`,
  `GestaoAgendamentoProposta.tsx` e `EncerramentoProposta.tsx`.
- Testes: `PropostasReinclusaoTests.cs`, `PropostasFluxoAuthTests.cs`,
  `ReinclusaoProposta.test.tsx` e extensão de `gestaoCancelamento.api.test.ts`.
- Documentação: este documento e continuidades em 05, 26, 87 e 91.

O inventário de superfícies foi consultado nesta etapa e retornou `apps: []` e
`browsers: []`. A homologação visual autenticada permanece pendente; testes DOM
dos três temas não a substituem. Nenhuma configuração JWT, audience, Keycloak,
permissão ou concessão foi modificada. Nenhuma base existente foi usada para
ensaios, e não houve publicação/reinício da API, commit ou push.
