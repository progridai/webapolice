# Fluxo Operacional de Propostas a Partir da Vida

Implementação: 11/11/2026. Conferência final: 12/11/2026. Complementa os documentos 22, 77, 81 e 82.

## Diagnóstico e Primeira Etapa

No diagnóstico inicial, o backend possuía listagem, criação e inclusão de item, com permissão específica e operação auditada. A criação resolvia Cliente/Pessoa/vínculo pelo Cadastro, mas recebia IDs internos para alguns parâmetros e gravava `Vigente = true`. A inclusão de item validava somente o Universo anterior. `proposta_item.plano_id` identifica Plano global e não pode ser reutilizado para o Plano contextual do Módulo.

O responsável confirmou preenchimento inicial **sem vigência**, situação **Rascunho** (código `RASCUNHO`) e versionamento com preservação da anterior até confirmar a substituta. `Propostas__StatusInicialCodigo` continua configuração externa obrigatória; não selecionar status por posição ou ID presumido. A criação copia condições do Módulo sem calcular totais ou gerar movimentos. Confirmação, cancelamento, edição das condições e geração financeira continuam para etapas próprias.

A consulta compreende listagem geral paginada, filtro por número, Apólice e Vida, acesso pela aba Vidas e detalhe por UUID. Uma Vida encerrada também permite consultar suas propostas. Uma combinação Apólice/Vida incompatível é rejeitada. Propostas excluídas logicamente não aparecem na consulta operacional; `VisivelOperacional = false` é respeitado. Inatividade/vigência não apaga o histórico consultável. Não se concede permissão a perfis.

## Contratos e Limites

- `GET /api/propostas`: preserva os campos e parâmetros anteriores e acrescenta `apolicePublicId` e `apoliceVidaPublicId`, ambos opcionais. A busca continua sendo pelo número. A resposta acrescenta referências públicas, nome atual do Cliente, matrícula, Apólice e indicadores registrados de vigência/versão. Não expõe PKs/FKs numéricas.
- `GET /api/propostas/{publicId}`: detalhe operacional, protegido por `propostas.visualizar`, assim como a lista. Retorna condições financeiras já registradas, itens, coberturas e beneficiários, sem dados bancários, documentos pessoais, IDs legados ou chaves normalizadas. Não calcula nem gera movimentos.
- `POST /api/apolices/{apolicePublicId}/vidas/{vidaPublicId}/propostas`: criação contextual com `propostas.inserir`. Recebe somente `propostaOrigemPublicId` opcional; dados de identidade e contexto são resolvidos e validados no servidor. Retorna o UUID da proposta criada. Não exige permissão de alterar Vida ou Cliente para criar Proposta a partir de vínculo já existente.
- O POST anterior `/api/propostas` permanece para compatibilidade, mas novas gravações também começam sem vigência e respeitam as barreiras de rascunho/versionamento. A inclusão de itens do Universo anterior é rejeitada quando a proposta usa o Módulo contextual. Não há endpoint de confirmação, substituição efetiva, cancelamento ou alteração das condições nesta etapa.
- Identificação do Cliente e vínculo é obtida por contrato público de Cadastro em lote por página; Seguro não consulta tabelas de Cadastro. Referências históricas inativas continuam consultáveis. Cadastros excluídos não são recuperados por essa consulta.
- Nomes de cadastros são identificados como referências atuais; não representam snapshots. Valores registrados de itens/coberturas vêm exclusivamente da proposta. Dados originais de beneficiários não são transformados. Não se deriva um plano contextual de IDs globais nem se atribuem as condições atuais do Módulo a propostas antigas.

## Modelagem da Criação Contextual

Antes das alterações de schema: acrescentar referências opcionais `proposta.apolice_subgrupo_id`/`apolice_modulo_id`, distintas de `subgrupo_id` legado, e seus nomes registrados. Acrescentar `proposta_item.apolice_modulo_plano_id`, nome registrado e flags `plano_paga`/`plano_reajuste`, mantendo `plano_id` global anterior. Acrescentar `proposta_cobertura.public_id`, `apolice_modulo_cobertura_id`, nome, nome reduzido, atributo básica e reajuste registrados; tornar seu `legado_id` anulável. Valores de prêmio já têm colunas próprias e serão copiados sem agregação ou cálculo. FKs são restritivas; nenhum registro anterior é associado retroativamente ao Módulo.

Os novos campos humanos `SubgrupoNomeRegistrado`, `ModuloNomeRegistrado`, `PlanoNomeRegistrado`, `CoberturaNomeRegistrado` e `CoberturaNomeReduzidoRegistrado` devem ser classificados em C#, TypeScript e SQL. `CoberturaBasicaRegistrada` é atributo técnico do cadastro, preservado como texto sem inferir equivalência com o booleano legado `Basica`. UUIDs, FKs, flags, números e campos de origem são técnicos. Aplicar os triggers Title Case após a migration, com normalização histórica desligada. Os campos `*Legado` permanecem intactos.

A primeira proposta da Vida é versão 1. Para uma nova versão, o usuário informa explicitamente a proposta de origem da mesma Vida, vigente e visível; grava `proposta_origem_id` e incrementa a versão, mantendo a anterior inalterada. A nova versão copia a configuração atual do Módulo, apresentada como tal, sem presumir migração de itens legados. Apenas um rascunho operacional por Vida: uma segunda tentativa é rejeitada com orientação para consultar o existente. Se existem propostas e nenhuma origem é informada, não criar adesão independente implicitamente. Registros excluídos continuam evidência de uso. O bloqueio da Apólice na transação serializa criação e alterações do contexto. Não há mudança automática de situação da Vida.

O novo POST contextual recebe apenas UUID de origem opcional; Cliente/Pessoa/matrícula/estipulante/Apólice/Subgrupo/Módulo vêm da Vida validada no servidor. Sem Módulo, pode ser registrado um rascunho sem itens, claramente incompleto. Com Módulo, exigir vínculo e Plano ativos e copiar associações de Coberturas ativas, inclusive prêmios zero. Não preencher prêmio líquido/parcela/IOF por soma presumida. A configuração do catálogo compartilhado não reescreve esses snapshots.

O catálogo `proposta_status` do ambiente de teste estava vazio. Uma migration específica registra `RASCUNHO` por código, sem ID fixo, com `finalizador=false`, `visivel_operacional=true` e `permite_movimentacao=false`. O catálogo `movimento_tipo` também estava vazio: não foram carregados automaticamente os movimentos enviados do legado. `Mov_Gera_Titulo` não define o ciclo de vida nem autoriza cobranças. Os números da lista fornecida são identificadores de origem e não serão presumidos como PKs atuais.

## Uso e Segurança

1. Abrir **Apólices → Detalhes → Vidas**. Uma participação ativa, com vínculo e Apólice ativa, oferece **Criar proposta**, mediante `propostas.inserir`, quando ainda não possui propostas.
2. Conferir Cliente, matrícula, estipulante, Subgrupo e Módulo na confirmação. **Criar rascunho** grava a proposta e suas condições numa operação auditada. Durante o envio, fechamento e duplicidade de envio são bloqueados. Falha mantém a confirmação para nova tentativa.
3. Usar **Consultar propostas** na Vida ou o menu **Propostas** (`/#/propostas`). A área geral pesquisa por número e apresenta Cliente/matrícula/Apólice/situação/versão/vigência. O acesso pela Vida restringe a consulta àquela participação, inclusive encerrada.
4. Abrir **Consultar** para ver condições, itens, coberturas e beneficiários existentes. Nomes atuais e nomes registrados são apresentados separadamente. Prêmio zero é distinto de valor não informado.
5. Em uma proposta vigente, **Preparar nova versão** oferece criação de outro rascunho ligado à origem. A anterior permanece vigente; o histórico registra preparação, sem declarar substituição concluída.

Menu, rota e ações verificam módulo APOLICES, recurso PROPOSTAS e permissões, inclusive para Acesso Total quando módulo/recurso estão indisponíveis. A autorização definitiva continua na API. Nenhuma permissão foi concedida a perfis. A criação e a consulta são ações separadas; usuário apenas com inserir recebe confirmação de sucesso sem disparar consulta não autorizada.

Os novos vínculos de snapshot complementam as proteções hierárquicas. Mesmo que a Vida posteriormente mude de Subgrupo/Módulo, a proposta preserva seu contexto e bloqueia a exclusão dos vínculos utilizados. Sete triggers adicionais validam ancestralidade e impedem exclusão por SQL direto. Plano de outro Módulo ou Cobertura de outro contexto são rejeitados. Não há cascata de negócio.

## Movimentos e Continuidade

A lista enviada pelo responsável mistura alterações contratuais (aumentos, redução, inclusão, reinclusão, atualização cadastral), resultados de cobrança (pago, insuficiência de fundos, não descontou) e ocorrências de tratamento (duplicidade, acerto e outros). Essa referência foi preservada como insumo para o próximo desenho; não se converteram todos esses tipos em situações da proposta. Em particular, a situação inicial Rascunho não representa o movimento de Inclusão de Proposta.

Pendências ao concluir esta primeira etapa: edição das condições individuais, inclusão/manutenção de beneficiários, confirmação, substituição efetiva da anterior, cancelamento/reinclusão e relação dessas transições com a Vida. Na etapa inicial, beneficiários eram apenas consultados e `legado_id` permanecia obrigatório. A escrita nativa com UUID, manutenção de beneficiários e ajustes justificados dos prêmios foram entregues na [etapa 2](84-preenchimento-rascunho-propostas.md). Continuam pendentes confirmação/substituição e regras de capital, prêmio líquido/parcela, IOF, comissão, competência, faturamento e movimentos financeiros. Nenhuma cobrança, pagamento, cálculo ou movimento foi gerado.

Continuidade de 12/11: a [etapa 3](85-confirmacao-substituicao-propostas.md) acrescenta confirmação com data informada, início futuro agendado e substituição atômica na efetivação. A Vida permanece independente; os cálculos e a geração financeira continuam pendentes.

Snapshots são capturados na criação do rascunho nesta etapa. Uma futura operação de atualização ou confirmação deverá explicitar como revisar/revalidar condições; não atualizar silenciosamente snapshots de propostas existentes por alteração do Módulo. Propostas simultâneas independentes e descarte de rascunho não foram liberados.

## Instalação na Base de Teste

O diagnóstico somente leitura confirmou as tabelas de Propostas vazias e ausência da identidade dos itens e das permissões. Foram aplicadas exclusivamente:

- Seguro: `20261023000100_AdicionarPublicIdPropostaItem`;
- Seguro: `20261111224713_PropostasOperacionaisModulo`;
- Seguro: `20261111224800_DefinirRascunhoProposta`;
- Segurança: `20261023000200_CatalogarPropostas`.

Os scripts pontuais estão em `backend/scripts/migrations/propostas/`. `backend/scripts/aplicar-propostas-teste.ps1` usa a configuração específica `ConnectionStrings__PostgreSqlTestes`, recusa destino diferente de `webapolice_teste`, exige o baseline de hierarquia e tabelas de Propostas vazias, e compara hashes de perfis e concessões antes/depois. Sem `-Aplicar`, executa apenas diagnóstico. Não usa atualização geral de migrations.

As quatro migrations usam uma transação. A reaplicação integral do script Title Case usa transação própria, com normalização histórica desligada. Na primeira tentativa, o PowerShell tratou um NOTICE informativo como erro; o lançador foi corrigido para ocultar somente essas mensagens, mantendo `ON_ERROR_STOP` e verificação de saída. A repetição idempotente terminou com sucesso. A conferência final em outra conexão confirmou RASCUNHO, recurso/permissões disponíveis, sete triggers de contexto e a classificação dos cinco campos humanos nos três triggers das tabelas de Propostas. As tabelas continuaram sem propostas, itens ou coberturas de negócio criados pela instalação.

Backup **somente da estrutura**, sem registros, em `scratch/propostas-aplicacao-20261111-200455/estrutura-anterior.sql`; não representa backup completo dos dados. O segundo ensaio idempotente também gerou cópia estrutural, em `scratch/propostas-aplicacao-20261111-200557/`. Logs técnicos estão em `scratch/propostas-implantacao-20261111.log` e `scratch/propostas-implantacao-final-20261111.log`, ignorados pelo Git. Nenhum perfil ou concessão foi alterado. Produção não foi acessada.

A configuração externa local recebeu `Propostas__StatusInicialCodigo=RASCUNHO`, e o carregador preserva essa entrada opcional sem inventar valor quando ausente. O `.env.local` continua ignorado pelo Git. A API foi publicada em `bin/ExecucaoPropostas-20261111`, verificada temporariamente na porta 5008 e ativada na 5007 (PID 50764). Os hashes dos arquivos de configuração permaneceram iguais aos da publicação anterior; JWT/audience/Keycloak não foram alterados. Health/live, health/ready e frontend responderam 200. Lista, detalhe e POST contextual retornaram 401 sem token, usando UUIDs fictícios. Não foi criada proposta autenticada em dados reais para homologação.

## Validação

Backend completo: 542 testes aprovados (6 Auditoria Unit, 2 Auditoria Integration, 59 Cadastro, 28 Arquitetura, 140 API e 307 Integration). A suíte de Propostas compreende 33 casos, incluindo gravação sem legado artificial, condições congeladas, zeros monetários, versionamento sem alterar a anterior, rollback por falha de auditoria, criação concorrente, ancestralidade no PostgreSQL, bloqueio de exclusão após mudar a Vida, migração de registros históricos sem reescrita e Title Case em SQL direto. Build da solução sem erros ou avisos.

Frontend: `format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run` e `build` aprovados, incluindo 1.236 testes em 80 arquivos, sem erros/avisos de lint ou ocorrências no scanner. O build mantém o aviso preexistente de chunk acima de 500 kB; os limites não foram alterados. Os testes novos cobrem temas claro/escuro/sistema, acesso por recurso/módulo/permissão, estados de consulta, cancelamento e descarte de resposta atrasada, identidade contextual e envio sem duplicidade. Scanner local de segredos aprovado.

Não houve navegador conectado; a tentativa de abrir o navegador integrado também retornou indisponibilidade. A homologação visual autenticada permanece pendente. A última conferência de leitura foi inicialmente recusada por falha da revisão automática devido ao limite de uso, sem execução; após a retomada em 12/11, a mesma conferência terminou com sucesso. Isso não foi uma recusa por risco da ação.

## Arquivos Centrais

Frontend: `features/propostas/` (contratos, API, consulta/detalhes, criação e testes), ações de Propostas em `VidasTab`, contexto em `ApoliceDetalhesPage`, rotas/menu e proteção opcional por recurso em `PermissionProtectedRoute`. Backend: DTOs e consultas de Propostas, `CriarPropostaDaVidaCommand`/handler, evolução de `CriarPropostaHandler`, `IPropostasRepository`/adaptador, contrato de referências em lote do Cadastro, entidades/configurações/snapshot/migrations, proteção de exclusão e tratamento de erros de contexto. Classificação Title Case sincronizada em C#, TypeScript e SQL; testes de API/integração/normalização e scripts de instalação/verificação atualizados. Alterações locais anteriores foram preservadas.
