# Padronização geral dos dados em Title Case

Estipulantes (24/11/2026): `SeguradoraNome` é texto humano de resposta, classificado nas listas de C#, TypeScript e SQL. A consulta reutiliza `core.pessoa.nome`, já padronizado; não foi criada coluna humana nem há necessidade de reaplicar triggers ou reprocessar dados. `seguradoraPublicId` e o `grupoId` legado são técnicos e preservados. Grupo foi retirado das telas por decisão do responsável, sem apagar vínculos históricos. Ver [documento 103](103-vinculo-seguradora-estipulantes.md).

Revisão explícita de condições (18/11/2026): `CondicoesProprias`, flags de elegibilidade, UUIDs de contexto, revisão, hash `Comparacao` e `TratamentoAjustes` são técnicos. `Observacao`, `Nome` e snapshots/justificativas reutilizam classificações humanas existentes em C#, TypeScript e SQL. Não foi adicionada coluna humana; detalhes no [documento 94](94-revisao-explicita-condicoes-propostas.md).

Regra vigente em 15/10/2026, obrigatória para todos os módulos atuais e futuros.

## Comportamento

Todo texto humano de negócio usa inicial maiúscula nas palavras principais e demais letras minúsculas, com cultura pt-BR: `JOÃO DA SILVA` → `João da Silva`, `MARIA D’ÁVILA-SOUZA` → `Maria d’Ávila-Souza`. Preposições, artigos, conjunções e contrações da lista compartilhada permanecem em minúsculas, inclusive no início do campo: `de`, `da`, `dos`, `a`, `o`, `e`, `ou`, `para`, `com`, `pelo`, entre outras. A partícula `d` antes de apóstrofo também fica minúscula. A classificação é lexical, pela lista explícita em C#, TypeScript e SQL, sem análise gramatical de contexto. Siglas dentro de textos humanos seguem a capitalização das palavras principais (`LTDA` → `Ltda`). Espaços, quebras de linha, pontuação, números, vazio e null são preservados. A operação é idempotente.

Aplica-se a nomes, razão social/nome fantasia, descrições, títulos, observações, logradouros, bairros, complementos, departamentos e demais campos classificados em `PadronizacaoTexto.CampoHumano`. A classificação explícita protege strings com significado técnico.

Permanecem nas regras próprias: senhas/tokens, username/sub/roles, e-mails, URLs, arquivos/caminhos/storage keys, códigos externos/internos, CPF/CNPJ/RG, UF, números de contratos/apólices, estados/enums, dados JSON, chaves de busca (`NomeNormalizado`) e campos originais de migração. Auditoria e mapas de migração são evidências históricas imutáveis. Texto digitado em campos de busca não é reformatado.

## Implementação

- `WebApolice.SharedKernel/Text/PadronizacaoTexto.cs`: fonte da política de capitalização e classificação dos campos.
- Middleware da API: normaliza JSON de POST/PUT/PATCH, inclusive estruturas aninhadas, antes do binding e da validação; erros de JSON continuam sendo tratados pelo binding padrão. Não altera query strings nem arquivos.
- `TextoPadronizadoDbContext`: normaliza textos incluídos ou efetivamente alterados em SaveChanges síncrono e assíncrono. Uma alteração apenas técnica preserva textos históricos intocados. Todos os contextos de negócio existentes usam essa base; auditoria permanece separada.
- `apps/web/src/services/http/titleCase.ts`: normaliza uma cópia do payload antes de enviar pelo cliente HTTP. A garantia final é do backend/banco. Não depende de transformação visual CSS, nem interfere na posição do cursor durante digitação.
- `backend/scripts/padronizar-title-case.sql`: normaliza registros existentes e instala triggers BEFORE INSERT/UPDATE para cobrir SQL direto, COPY e importadores. INSERT normaliza todos os textos humanos; UPDATE normaliza os textos humanos que mudaram em relação a OLD, preservando evidências intocadas em alterações de metadados. Restringe-se aos schemas de negócio e às colunas humanas conhecidas; exclui auditoria e migration maps. Reexecução é idempotente. Em conflito de unicidade ou erro, toda a transação é revertida.

## Aplicação no banco existente

Aplicar o script integralmente na conexão do ambiente de destino durante a implantação, após as migrations. Verificar backup recuperável antes da execução: a capitalização anterior não pode ser inferida após a conversão. O script registra somente tabela e quantidade de linhas alteradas, sem dados pessoais. Para simular, substituir o COMMIT final por ROLLBACK. Confirmar as contagens e eventuais colisões de nomes únicos na homologação antes da produção. Não executar apenas partes do arquivo.

Para instalar ou atualizar a proteção de novas gravações sem normalizar o histórico existente, executar `SET webapolice.title_case_normalizar_existentes = 'off';` na mesma sessão antes do script integral. Esse modo instala as mesmas funções e triggers e não executa os UPDATEs em lote; o padrão continua sendo normalizar os registros existentes. Restaurar a configuração com `RESET webapolice.title_case_normalizar_existentes;` ao terminar. Correções pontuais devem preservar antes uma imagem original do registro. Fazer push não atualiza processos da API em execução: recompilar e reiniciar o processo no ambiente de destino também é necessário.

A função SQL usa a classificação alfabética/capitalização do locale UTF-8 da base. Validar os exemplos com acentos no ambiente de destino. Preservar as colunas de pesquisa normalizadas; elas não recebem Title Case.

## Novas implementações

Consolidação de 23/10/2026: mover os tipos de Pessoa/Cliente para Domain.Clientes não altera a classificação por propriedade nem os mecanismos de SaveChanges/HTTP/SQL. O novo PublicId de PropostaItem é Guid técnico, preservado. LegadoId continua técnico, agora anulável em Proposta/Item para gravações nativas; valores históricos não são reescritos. Não foram acrescentados campos humanos. A reaplicação dos triggers no destino continua obrigatória conforme a orientação acima, sem normalizar evidências históricas nesta correção.

1. Classificar cada novo campo como texto humano ou técnico.
2. Para campos humanos, atualizar as listas equivalentes em C#, TypeScript e SQL, adicionando testes com caixa alta, caixa mista, acentos, null e idempotência.
3. Herdar os novos contextos de negócio da base compartilhada; não usar DbContext diretamente.
4. Reaplicar o script após migrations que adicionem tabelas/colunas humanas. SQL em lote não passa por SaveChanges e precisa dos triggers.
5. Preservar valores originais de importações em campos próprios, sem reformatação.


Consolidação de 26/10/2026: os demais POCOs usados pelas portas de Cadastro/Seguro foram movidos para Domain.Modelos sem alterar propriedades ou classificações. Snapshots atuais refletem os nomes CLR; designers históricos foram preservados. Não há nova coluna humana nem necessidade de reprocessar dados para essa reorganização. Testes antigos que esperavam observações/nomes sem Title Case foram corrigidos para a política vigente, sem alterar a lista compartilhada ou criar exceções para os testes.

Continuidade de 09/11/2026: `novaMatricula` nos requests de Vida é um código técnico, persistido na coluna existente `cadastro.cliente_vinculo.matricula`. Preserva a caixa digitada e recebe apenas Trim; a chave `matricula_normalizada` usa a normalização técnica existente para duplicidades. A classificação humana compartilhada permanece a mesma em C#, TypeScript e SQL. Testes de classificação C#, payload HTTP do frontend e persistência PostgreSQL cobrem a preservação de `aBc-X009`. Nenhuma nova coluna foi criada, portanto esta continuidade não requer reaplicar triggers.

Continuidade de 11/11/2026: a exclusão hierárquica de vínculos da Apólice adiciona somente UUIDs contextuais e timestamps técnicos (`public_id`/`deleted_at`). Não cria texto humano ou modifica a classificação compartilhada. Descrições do histórico e nomes das permissões usam campos existentes e os mecanismos atuais; números de apólice e evidências anteriores são preservados. A migration compara os dados existentes sem reprocessar textos, e não exige reaplicar triggers de Title Case por não adicionar coluna humana. Regras, validação e aplicação constam no documento 82.

Fluxo de Propostas (conferência em 12/11/2026): acrescentados os campos humanos `SubgrupoNomeRegistrado`, `ModuloNomeRegistrado`, `PlanoNomeRegistrado`, `CoberturaNomeRegistrado` e `CoberturaNomeReduzidoRegistrado`, com classificação equivalente em C#, TypeScript e SQL. O atributo `CoberturaBasicaRegistrada`, UUIDs, referências, flags, status e matrículas são técnicos. `PlanoNomeLegado`/`CoberturaNomeLegado` continuam evidências preservadas. Os testes cobrem caixa alta/mista, acentos, null, idempotência e SQL direto. O script integral foi reaplicado em `webapolice_teste` com normalização histórica desligada; a proteção dos cinco campos foi confirmada em nova conexão. Detalhes no [documento 83](83-fluxo-operacional-propostas.md).

Preenchimento de Pré Proposta (12/11/2026): `ParentescoRegistrado` e `JustificativaAjustePremio` são textos humanos, classificados nas três listas e cobertos pelos testes. UUID de beneficiário, revisão técnica, indicador de ajuste e valores iniciais dos prêmios são técnicos. Campos originais/normalizados, CPF e vínculos legados permanecem preservados. O script integral foi reaplicado em `webapolice_teste` sem normalização histórica; a presença dos dois campos nos triggers foi confirmada. Detalhes no [documento 84](84-preenchimento-rascunho-propostas.md).

Confirmação e Vigência (12/11/2026): `MotivoPendenciaVigencia` é texto humano, classificado em C#, TypeScript e SQL; datas, instantes, indicador de processamento e códigos de situação de vigência são técnicos. A atualização da versão anterior precisa preservar seus textos intocados; por isso o mecanismo compartilhado de UPDATE passou a normalizar apenas os campos humanos alterados, sem mudar a lista de palavras, INSERT ou a proteção de campos técnicos/originais. Testes cobrem edição humana, alteração apenas técnica, SQL direto, acentos, null e idempotência. A instalação deve reaplicar o script integral sem normalização histórica, conforme o [documento 85](85-confirmacao-substituicao-propostas.md).

Gestão dos Agendamentos (12/11/2026): `CanceladaEm`/`cancelada_em` é timestamp técnico nullable. Justificativas usam `Observacao` no contrato e no histórico; o motivo do evento usa `Motivo`, ambos humanos já classificados nas três listas. Não há palavra ou campo humano novo nem alteração da política compartilhada. API, cliente HTTP e persistência usam os normalizadores existentes. Testes verificam a justificativa em Title Case e a preservação do timestamp. A instalação reaplica o script integral com normalização histórica desligada, preservando evidências, conforme o [documento 86](86-gestao-agendamentos-propostas.md).

Encerramento e Descarte (12/11/2026): `DataCancelamentoVigencia`, `CancelamentoSolicitadoEm` e `DescartadaEm` são data/instantes técnicos, preservados pelo cliente HTTP e excluídos da classificação humana. Justificativas/eventos continuam em `Observacao`/`Motivo` do histórico, já classificados em C#, TypeScript e SQL, sem nova coluna humana ou mudança da lista de palavras. Testes cobrem o timestamp/data e os textos normalizados da solicitação/descarte. A instalação reaplica os triggers sem normalizar dados históricos. Regras no [documento 87](87-encerramento-vigencia-descarte-propostas.md).

Gestão do Pedido de Cancelamento (17/11/2026): alteração/retirada reutilizam
`Observacao`/`Motivo` e os metadados técnicos existentes. Nenhum campo humano,
coluna ou palavra nova; classificações C#/TypeScript/SQL permanecem sincronizadas
sem mudanças. Testes HTTP e PostgreSQL verificam justificativas com acentos em
Title Case e a preservação de UUID, revisão, datas e instante original do pedido.
Não é necessário reaplicar triggers ou normalizar registros anteriores nesta
etapa. Regras no [documento 91](91-gestao-pedido-cancelamento-vigencia.md).

Reinclusão (17/11/2026): nenhuma coluna ou campo humano novo. `OrigemCondicoes`
(`ANTERIOR`/`MODULO`), seu metadado interno `OrigemCondicoesReinclusao`,
`Reinclusao`, `PodePrepararReinclusao`, revisão e
`DataFimVigenciaOrigem` são códigos/flags/data técnicos. Nomes, parentescos,
snapshots e justificativas copiados usam campos existentes classificados nas
listas C#/TypeScript/SQL e a normalização compartilhada de novos registros.
A origem cancelada e suas evidências permanecem intocadas. Testes HTTP conferem
a preservação dos dois códigos e testes PostgreSQL comparam integralmente origem,
itens, coberturas e beneficiários. Não há migration, nova classificação humana
ou necessidade de reaplicar triggers nesta etapa. Ver [documento 92](92-reinclusao-propostas.md).

Simulação mensal (18/11/2026): `IncluiConjuge`/`inclui_conjuge` é booleano técnico anulável. `PodeSimularValorMensal`, UUID do vínculo Ramo/Apólice, revisão, alíquota, instante e valores monetários canônicos textuais são técnicos e preservados. Nomes de Ramo/Cobertura reutilizam os campos humanos já classificados em C#, TypeScript e SQL; nenhuma lista de palavras ou campo humano novo. Não há reprocessamento histórico nem necessidade de reinstalar triggers por esta coluna. Regras e testes no [documento 95](95-formacao-valores-financeiros-propostas.md).

Identidade de movimentos nativos (18/11/2026, migration preparada): `PropostaMovimento.PublicId`/`public_id` é UUID técnico anulável; `LegadoId`/`legado_id` continua identificador técnico, agora anulável para permitir escrita nativa sem inventar evidência de migração. Nenhum campo humano novo ou mudança nas listas C#/TypeScript/SQL. A migration preserva os campos anteriores, não preenche UUID em movimentos históricos e não exige reinstalar triggers de Title Case por essa coluna técnica. Ver [documento 97](97-parcelas-movimentos-nativos-propostas.md).


Ciclo financeiro nativo (18/11/2026): `CondicoesHash`, `Conferencia`, UUIDs, competências, datas, revisões de baixa/fechamento, classificação `PARCELA_MENSAL`, alíquota e valores monetários canônicos são técnicos. `ClienteNome` e matrícula reutilizam o contrato/classificações existentes (nome humano e matrícula técnica); observações/motivos permanecem nos campos humanos já classificados. Os novos modelos `PropostaFinanceiro` e `ParcelaCompetencia` não possuem texto humano. Nenhuma palavra/classificação humana nova ou reprocessamento de evidências, mantendo C#/TypeScript/SQL sincronizados. As migrations não exigem reinstalar triggers por estas colunas técnicas. Casos de uso continuam no SeguroDbContext derivado de TextoPadronizadoDbContext; os clientes HTTP usam a normalização compartilhada. Testes preservam caixa de tokens, UUIDs, matrícula e valores exatos. Ver [documento 98](98-lotes-mensais-baixa-parcelas.md).
