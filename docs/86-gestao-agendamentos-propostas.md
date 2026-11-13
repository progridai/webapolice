# Gestão dos Agendamentos de Propostas

Data: 12/11/2026. Etapa 4. Complementa o documento 85.

Continuidade: o [documento 87](87-encerramento-vigencia-descarte-propostas.md) define a etapa 5, aprovada pelo responsável, para cancelamento de vigência com data informada e descarte de rascunho. As limitações abaixo registram o recorte original da etapa 4.

## Regras e Modelagem Antes da Evolução

O responsável aprovou reagendar e cancelar propostas confirmadas que ainda não entraram em vigência, com justificativa e histórico. O cancelamento encerra a versão como `CANCELADA`, preserva seus dados e permite preparar outra versão a partir da vigente. Cancelamento de vigentes, retorno ao rascunho e financeiro não fazem parte desta etapa.

As operações abrangem propostas nativas confirmadas, visíveis, não excluídas, sem substituição e ainda não vigentes, inclusive uma ativação atrasada ou pendente. O estado é conferido depois dos mesmos bloqueios de Apólice/Proposta usados pelo processador; se a ativação vencer a concorrência, a operação é recusada. Revisão esperada e justificativa não vazia de até 1.000 caracteres são obrigatórias. Vida e versão anterior não são alteradas.

Reagendar informa outra data **futura** no calendário de São Paulo. Hoje/retroativa continuam pertencendo ao fluxo de confirmação/efetivação, evitando ativação como efeito implícito do reagendamento. Revalidar contexto, condições individuais, períodos e origem como na confirmação; exigir processador habilitado. A confirmação original é preservada e a pendência anterior é limpa somente após revalidação bem-sucedida. Data igual é recusada, sem escrita/auditoria duplicada.

Cancelar não exige que Plano, Vida ou Apólice continuem elegíveis; indisponibilidade desses dados pode ser a razão da desistência. Exigir catálogo configurado em `Propostas__StatusCanceladaCodigo`, diferente dos códigos inicial/confirmada, visível, finalizador e sem movimentação. Acrescentar somente `cancelada_em` nullable (timestamptz), técnico; registros existentes recebem null. Preservar data de início planejada, confirmação, condições, valores e pendências já registradas como evidências. Não preencher fim de vigência ou substituição de uma versão que nunca foi vigente.

Reutilizar `proposta_historico`: motivos específicos de reagendamento/cancelamento e `observacao` para justificativa e datas planejadas anterior/nova. Quando existe origem, relacionar origem e versão agendada; sem origem, ambas as referências identificam a própria versão. Isso registra evento da mesma versão sem inventar predecessora. Não alterar eventos existentes. `observacao`/`motivo` já são humanos nas listas C#/TypeScript/SQL; nenhuma classificação humana nova. `cancelada_em` e códigos permanecem técnicos.

Uma sucessora cancelada por este fluxo deixa de bloquear outra preparação. Não ignorar exclusões, ocultações ou outros estados finalizadores como se fossem cancelamentos autorizados. A nova versão usa `MAX(versao)` da Vida + 1, incluindo tentativas canceladas, sem reutilizar números. A origem permanece explicitamente a proposta vigente e a validação passa a exigir versão superior à origem; mantém identidade, unicidade e bloqueios. Se a primeira proposta for cancelada, a Vida pode iniciar outra tentativa sem origem apenas quando todas as anteriores forem cancelamentos nativos reconhecidos. Catálogo/configuração incompatível mantém o bloqueio conservador.

## Contratos e Interface

`POST /api/propostas/{publicId}/agendamento/reagendar`: revisão, data de início e `observacao` (justificativa). `POST /api/propostas/{publicId}/agendamento/cancelar`: revisão e `observacao`. Reutilizar `propostas.alterar`, gates APOLICES/PROPOSTAS e auditoria na mesma transação física. Detalhe acrescenta `canceladaEm`, `podeReagendar`, `podeCancelarAgendamento` e situação técnica `CANCELADA`. Canceladas não entram na fila e não podem ser preenchidas, confirmadas ou efetivadas.

Formulários na consulta mostram efeito sobre a vigente, exigem justificativa e envio definitivo explícito. Abertura não grava. Bloquear navegação e demais operações enquanto o formulário estiver aberto, e todos os campos durante envio. Falhas preservam dados e permitem recarregar. Após sucesso, consultar novamente detalhes/histórico.

## Validação e Instalação

Backend: **610 testes aprovados**, sendo 6 Auditoria Unit, 2 Auditoria Integration, 67 Cadastro, 28 Arquitetura, 145 API e 362 Integration. Build sem erros/avisos e modelo EF sem alterações pendentes. Cobertura de reagendamento com histórico, datas inválidas/iguais/fora do período, justificativa, configuração, estados incompatíveis, revisão antiga, cancelamento com contexto inativo, preservação de pendência/dados/vigência, saída da fila, nova tentativa sem reutilizar número, primeira tentativa cancelada, rollback da auditoria e disputa cancelamento/ativação nas duas ordens. Migration confere os registros anteriores e recusa rollback que apagaria cancelamento.

A primeira rodada completa identificou um teste antigo que herdava `StatusInicialCodigo` do ambiente local, apesar de testar ausência de configuração. O próprio teste passou a fixar essa precondição via `PostConfigure`, preservando sua exigência de rejeição antes do banco. A suíte completa foi repetida com sucesso. A geração inicial da migration precisou do lançador PowerShell com `ExecutionPolicy Bypass` no processo; a política do sistema não foi alterada.

Frontend: **1.282 testes em 84 arquivos**, com `format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run` e `build` aprovados. Zero erros/avisos de lint, tipos, formatação ou ocorrências do scanner. Testes dos três temas, data/justificativa, permissão/elegibilidade, bloqueio de outra operação/navegação, envio duplicado durante validação e requisição, falha/repetição/recarga, cancelamento sem gravação ao voltar e consulta de cancelada/histórico. Corrigidos acentos afetados pelo pipe do terminal e o seletor de um novo teste para incluir os dois-pontos do rótulo padrão; rodada completa repetida. O aviso preexistente do build sobre chunk acima de 500 kB permanece, sem flexibilizar limites. O lançador considera o código de saída do npm; o aviso em stderr não é tratado como falha de build pelo PowerShell.

Aplicadas exclusivamente `20261112183644_GestaoAgendamentosPropostas` e `20261112183700_DefinirCanceladaProposta` por `backend/scripts/aplicar-propostas-teste.ps1 -Agendamentos -Aplicar`, em `webapolice_teste`. As opções de etapa são mutuamente exclusivas; sem `-Aplicar`, o script só diagnostica. `07-gestao-agendamentos.sql` deriva do script EF idempotente e envolve as duas migrations em uma única transação, conferindo baseline e assinaturas dos dados anteriores das propostas/filhos, histórico, movimentos, catálogos existentes e perfis/concessões antes do commit. Não executa migrations gerais. `CANCELADA` recebe ID livre do catálogo, sem presumir valor; configuração incompatível recusa instalação. Seu rollback não remove o catálogo automaticamente e é bloqueado quando usado.

Backup **somente da estrutura**, sem registros, em `scratch/propostas-agendamentos-20261112-154915/estrutura-anterior.sql`. Logs em `scratch/propostas-agendamentos-{diagnostico,instalacao}.log`. Triggers Title Case reaplicados integralmente com normalização histórica desligada; verificador confirmou proteção de motivo/observação do histórico. Conferência encontrou **uma proposta, zero agendamentos cancelados e zero agendadas/pendentes**, preservando o registro existente. Nenhuma proposta real foi confirmada, reagendada, cancelada ou alterada para ensaio.

Configuração externa local, ignorada pelo Git: acrescentado `Propostas__StatusCanceladaCodigo=CANCELADA`, preservando todas as entradas anteriores. O código padrão permanece vazio para exigir configuração explícita em outros ambientes. Autenticação, Keycloak, perfis e concessões não foram alterados.

Publicação em `backend/src/WebApolice.Api/bin/ExecucaoAgendamentosPropostas-20261112`, validada temporariamente em 5008 e ativada em **5007, PID 5824**. Hashes de `appsettings.json`/`appsettings.Development.json` iguais aos da publicação anterior. Health live/ready e frontend 5173 responderam 200; as duas novas operações retornaram 401 sem token, com UUIDs fictícios. Instância temporária encerrada; logs/metadados em `scratch/propostas-agendamentos-api-20261112/`. Não há migrations no startup.

Homologação visual autenticada permanece pendente. Cancelamento de propostas já vigentes, retorno da cancelada ao rascunho, reagendamento para hoje/retroativa, revisão explícita do contexto e financeiro continuam fora desta entrega.

## Arquivos Centrais

- Backend: `GerirAgendamento/GerirAgendamentoCommands.cs`, controlador de Propostas, metadado/consulta/repositório/processador e regras de criação/versão/origem. Mesmas portas, bloqueios e transação auditada; sem dependências/projetos novos.
- Dados/operação: migration/designer/snapshot de `cancelada_em`, migration específica de `CANCELADA`, `07-gestao-agendamentos.sql`, instalador/verificador e carregador da configuração externa.
- Frontend: `GestaoAgendamentoProposta.tsx`, schema/testes, integração em `ConfirmacaoProposta`/`PropostaDetalhes`, contratos/API e fixtures. Teste adicional da consulta de cancelada e preservação do campo técnico no cliente HTTP.
- Testes backend: `PropostasAgendamentosTests.cs`, `PropostasAgendamentosMigrationTests.cs`, autorização das duas rotas, classificação técnica e comparação histórica das migrations; teste de barreira fixa a própria precondição.
- Documentação: este documento, continuidade no 85, autorização no 05 e classificação dos dados no 26. Alterações locais anteriores foram preservadas; nenhum commit foi criado.
