# Encerramento de Vigência e Descarte de Pré Proposta

> Referência atual — 25/11/2026: As ausências de funcionalidades descritas abaixo são relativas à etapa original. A sequência de entregas posteriores, incluindo confirmação, agendamento, cancelamento e reinclusão, está no [índice funcional 106](106-indice-funcional-atual.md). O escopo financeiro atual está consolidado no [104](104-alcance-atual-rotinas-financeiras.md). As evidências originais permanecem vinculadas à etapa em que foram obtidas; a homologação visual continua pendente.

Data: 12/11/2026. Etapa 5. Complementa o documento 86.

Continuidade de 17/11/2026: o [documento 91](91-gestao-pedido-cancelamento-vigencia.md)
registra alteração/retirada do pedido até sua efetivação, inclusive com data
alcançada. As limitações desta etapa 5 abaixo descrevem seu recorte original.

O [documento 92](92-reinclusao-propostas.md) registra a continuidade de reinclusão
com outra versão após cancelamento com vigência, preservando a encerrada.

## Regras e Modelagem Antes da Evolução

O responsável aprovou cancelamento de propostas vigentes com justificativa e data informada, aceitando hoje, retroativa ou futura. A proposta permanece vigente até a data futura. Bloquear enquanto houver pré proposta/sucessora não encerrada, exigindo resolvê-la antes. Vida independente, versões anteriores não são reativadas e não há geração financeira. Foi aprovado também descartar pré proposta com justificativa/histórico, preservando os dados e liberando outra tentativa.

O fluxo continua restrito às propostas nativas do processo atual. Cancelamento não altera condições, valores, beneficiários, identidade, evidências legadas ou cobranças. A data é o primeiro dia **sem** vigência (fim exclusivo, como na substituição). Deve ser igual/posterior ao início da proposta e pertencer aos períodos conhecidos da Apólice/Vida na solicitação. A operação não exige que esses cadastros continuem ativos ou que Plano/beneficiários continuem elegíveis: encerrar não equivale a confirmar nova cobertura. Retroatividade registra a data de negócio e preserva o instante real da ação, sem recalcular financeiro.

Acrescentar somente metadados técnicos nullable em `seguro.proposta`: `data_cancelamento_vigencia` (date), `cancelamento_solicitado_em` e `descartada_em` (timestamptz). Nenhum registro existente é reescrito. `cancelada_em` continua sendo o instante real do encerramento; `data_fim_vigencia` recebe a data efetiva apenas ao cancelar. `substituida_em` não é preenchido por cancelamento.

Solicitação para hoje/retroativa cancela imediatamente na transação auditada. Futura registra pedido e histórico, mantém `CONFIRMADA`/vigente e é processada pelo mesmo serviço de vigências no calendário de São Paulo, inclusive após reinício. Exigir processamento habilitado para solicitar início futuro e catálogo `CANCELADA` compatível. Pedido já registrado não pode ser duplicado ou alterado por esta etapa. A criação de sucessora fica bloqueada após solicitar cancelamento; todas as operações usam bloqueio da Apólice/Proposta, revisão esperada e transação física compartilhada com auditoria.

A efetivação revalida a proposta ainda vigente, sua unicidade na Vida e ausência de sucessora aberta, sem exigir contexto ativo. Falha de elegibilidade registra pendência no campo humano existente `motivo_pendencia_vigencia`, com instante e revisão; repetição da mesma causa não duplica escrita/auditoria. Falha transacional reverte tudo e permite nova tentativa. Não encerrar outra versão nem reativar predecessoras. Reinclusão após cancelamento de uma versão que já teve vigência permanece para uma etapa própria.

Descarte somente de pré proposta nativa ainda sem confirmação/vigência. Não exigir condições completas ou contexto ativo. Exigir revisão, justificativa e catálogo `DESCARTADA`, visível, finalizador e sem movimentação, configurado externamente por `Propostas__StatusDescartadaCodigo`. Guardar `descartada_em`; não fabricar confirmação, datas de vigência ou cancelamento. Permitir nova tentativa com número superior ao maior já registrado, preservando a origem vigente quando existente. Ocultações, exclusões e outros estados não equivalem a descarte.

Justificativas e datas explicativas usam `observacao` e `motivo` do histórico, já humanos e classificados em C#/TypeScript/SQL. Reutilizar os normalizadores e o histórico existente: origem/versão quando há predecessor, autorreferência quando é a primeira tentativa. Datas/códigos são técnicos. Não há palavra ou coluna humana nova.

## Contratos e Interface

Novas operações com UUID da rota e `propostas.alterar`/gates APOLICES/PROPOSTAS: solicitar cancelamento de vigência (revisão, data e observação), efetivar cancelamento vencido (revisão), descartar pré proposta (revisão e observação). O detalhe informa datas/instantes, elegibilidade e situações `CANCELAMENTO_AGENDADO`, `CANCELAMENTO_PENDENTE` e `DESCARTADA`, mantendo `CANCELADA` para encerramento concluído. A data planejada é distinta do fim efetivado.

- `POST /api/propostas/{publicId}/vigencia/cancelamento`: `revisaoEsperada`, `dataCancelamentoVigencia` e `observacao` obrigatórios; retorna `cancelada`, `agendada`, revisão e eventual pendência.
- `POST /api/propostas/{publicId}/vigencia/cancelamento/efetivar`: revisão obrigatória; não antecipa data. Repetição após efetivação retorna o estado sem duplicar histórico/auditoria, inclusive com revisão antiga.
- `POST /api/propostas/{publicId}/rascunho/descarte`: revisão e observação obrigatórias; retorna nova revisão.
- GET do detalhe: adições `dataCancelamentoVigencia`, `cancelamentoSolicitadoEm`, `descartadaEm`, `podeDescartarRascunho`, `podeCancelarVigencia` e `podeEfetivarCancelamento`. Metadados/flags anteriores são preservados.

Formulários explicam a data exclusiva, ausência de reativação anterior e preservação financeira; envio definitivo explícito e justificativa obrigatória. Durante operação aberta, bloquear outras ações/navegação; durante envio, bloquear campos e desistência. Erros preservam entradas e oferecem recarga; sucesso recarrega detalhes/histórico. Cancelamento futuro fica para consulta/efetivação; alteração ou retirada desse pedido não faz parte desta entrega.

## Validação e Instalação

Backend: **645 testes aprovados** — 6 Auditoria Unit, 2 Auditoria Integration, 70 Cadastro, 28 Arquitetura, 148 API e 391 Integration. Build sem erros/avisos e modelo EF sem alterações pendentes. Cobertura de datas informadas hoje/retroativa/futura, períodos/início, justificativa/revisão, proposta legada/oculta/pré proposta, contexto inativo, preservação da Vida/condições, bloqueio por sucessora, descarte incompleto, nova tentativa com numeração superior e ausência de reativação da anterior. Processamento testa fuso de São Paulo, atraso/reinício, duas instâncias, repetição sem auditoria extra e pendência/correção. As duas ordens da disputa entre cancelamento e criação de sucessora são serializadas. Falha de auditoria reverte pedido imediato/futuro, efetivação e descarte integralmente.

O primeiro teste de repetição de pendência esperava quatro auditorias, omitindo o evento de solicitação que antecede a pendência. Foi corrigido para conferir uma auditoria nova na primeira pendência e nenhuma na repetição. A suíte completa passou depois da correção. Os testes de migrations comparam os registros anteriores, verificam metadados inicialmente nulos e recusam rollback que apagaria descarte/pedido; comparações antigas foram atualizadas para excluir somente as novas colunas nullable do cálculo anterior.

Frontend: **1.298 testes em 85 arquivos**, com as seis verificações `format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run` e `build` aprovadas. Zero pendências de formatação, erros de tipos, erros/avisos de lint ou ocorrências no scanner. Testes dos três temas, datas imediata/futura, descarte/justificativa, permissões/elegibilidade, bloqueio entre formulários, duplicidade durante validação/envio, falha/recarga, efetivação pendente sem falso sucesso e bloqueio da preparação durante pedido futuro. A validação de justificativa foi compartilhada entre os dois fluxos existentes, sem duplicar a regra. Corrigida formatação de um novo teste com o Prettier existente e repetida a rodada completa; permanece o aviso preexistente de chunk acima de 500 kB, sem alterar limites.

Instaladas exclusivamente `20261112194158_EncerramentoVigenciaDescartePropostas` e `20261112194300_DefinirDescartadaProposta`, por `backend/scripts/aplicar-propostas-teste.ps1 -Encerramento -Aplicar`, somente em `webapolice_teste`. A nova opção de etapa é mutuamente exclusiva com as anteriores e permanece somente leitura sem `-Aplicar`. `08-encerramento-descarte.sql` deriva do script EF idempotente e reúne as duas migrations na mesma transação, exigindo baseline dos agendamentos e comparando os campos anteriores das propostas/filhos, histórico, movimentos, catálogos existentes e perfis/concessões antes de commit. Não aplica migrations gerais ou reescreve propostas. Catálogo `DESCARTADA` usa ID disponível, recusa configuração incompatível e não é removido automaticamente por rollback.

Backup **somente da estrutura**, sem registros, em `scratch/propostas-encerramento-20261112-165320/estrutura-anterior.sql`. Diagnóstico/instalação em `scratch/propostas-encerramento-{diagnostico,instalacao}.log`. Triggers Title Case reaplicados integralmente com normalização histórica desligada. O verificador conferiu as três colunas nullable, catálogos e proteção do histórico; encontrou **uma proposta, zero descartadas, zero pedidos programados e zero vigências canceladas**. A proposta existente foi preservada; nenhuma proposta real foi confirmada, descartada ou cancelada para ensaio.

Adicionada somente a chave pública `Propostas__StatusDescartadaCodigo=DESCARTADA` na configuração externa local, ignorada pelo Git, preservando as entradas anteriores. O código padrão é vazio em outros ambientes. Nenhum perfil, concessão, role, configuração JWT/Keycloak, biblioteca ou projeto foi alterado por esta etapa.

API publicada em `backend/src/WebApolice.Api/bin/ExecucaoEncerramentoPropostas-20261112`, conferida temporariamente em 5008 e ativada em **5007, PID 25312**. Hashes dos dois `appsettings` iguais aos da publicação anterior. Health live/ready e frontend 5173 retornaram 200; as três novas operações POST retornaram 401 sem token com UUIDs fictícios. A fila de cancelamentos foi consultada sem erro, stderr vazio e instância temporária encerrada. Logs/metadados em `scratch/propostas-encerramento-api-20261112/`. Não há migrations no startup.

A primeira sondagem de readiness do lançador expirou em dois segundos no startup e cancelou a consulta PostgreSQL, gerando um registro `DefaultHealthCheckService` com `OperationCanceledException`. O lançador repetiu a sondagem e recebeu 200; conferências posteriores de readiness também retornaram 200. Não foi falha do processador de vigências nem exigiu alterar configuração de saúde ou autenticação. Os ciclos posteriores da fila foram conferidos sem falhas do processador.

Scanner local de segredos, conferência do diff e comparação do assembly publicado aprovados. Não há navegador conectado (`apps`/`browsers` vazios na consulta das superfícies), portanto a homologação visual autenticada permanece pendente. Reinclusão de proposta que já teve vigência, retirada/alteração do pedido futuro, revisão explícita do contexto e financeiro continuam para etapas próprias.

## Arquivos Centrais

- Backend: `EncerrarProposta/EncerramentoPropostas.cs`, handlers de solicitação/efetivação/descarte, controlador, regras de tentativa/origem, consultas/repositório/DTO e extensão do processador existente.
- Dados/operação: entidade/configuração/migration/designer/snapshot dos três metadados, migration de `DESCARTADA`, `08-encerramento-descarte.sql`, instalador/verificador e carregador da configuração externa.
- Frontend: `EncerramentoProposta.tsx`/testes, schema e justificativa compartilhada, integração em confirmação/detalhes, contratos/API/fixtures e teste da consulta de cancelamento programado.
- Testes: `PropostasEncerramentoTests.cs`, `PropostasEncerramentoMigrationTests.cs`, autorização das três rotas, preservação dos campos técnicos e comparações de migrations anteriores.
- Documentação: este documento, continuidade no 86, autorização no 05 e classificação no 26. Alterações locais anteriores foram preservadas; nenhum commit foi criado.
