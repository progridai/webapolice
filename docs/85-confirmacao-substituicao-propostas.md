# Confirmação e Substituição de Propostas

> Referência atual — 25/11/2026: As ausências de funcionalidades descritas abaixo são relativas à etapa original. A sequência de entregas posteriores, incluindo confirmação, agendamento, cancelamento e reinclusão, está no [índice funcional 106](106-indice-funcional-atual.md). O escopo financeiro atual está consolidado no [104](104-alcance-atual-rotinas-financeiras.md). As evidências originais permanecem vinculadas à etapa em que foram obtidas; a homologação visual continua pendente.

Data: 12/11/2026. Etapa 3. Complementa os documentos 83 e 84.

Continuidade: o [documento 86](86-gestao-agendamentos-propostas.md) define a etapa 4 de reagendamento/cancelamento antes da vigência, aprovada pelo responsável. As referências abaixo a essas operações como futuras registram o recorte original da etapa 3.

## Base Confirmada

O responsável definiu que uma nova versão preserva a anterior até sua efetivação: hoje/retroativa válida troca na confirmação; início futuro troca quando a data chegar e as condições forem revalidadas. A troca é atômica: falha de validação, concorrência ou auditoria preserva a anterior vigente. Beneficiários precisam estar completos e somar 100% na confirmação. Prêmios ajustados exigem justificativa; os valores registrados e suas bases são preservados. Não há regra financeira aprovada para gerar título, parcela, comissão, competência ou cobrança.

## Decisões Apresentadas Nesta Etapa

O responsável definiu data de início informada e exigência de Plano do Módulo, pelo menos uma cobertura ativa e beneficiários completos somando 100%. A proposta inicial desta etapa usa situação `CONFIRMADA`, mantendo a Vida independente e sem financeiro; a alteração solicitada foi informar a data. Foi aprovado também início futuro, substituindo a anterior apenas na data de início. Hoje/retroativa válida efetiva a troca na confirmação; futura confirma e agenda sem retirar a anterior de vigência.

## Desenho Técnico Antes da Evolução

Reutilizar `propostas.alterar`, revisão técnica esperada, contrato de identidade do Cadastro, bloqueios e operação auditada. A confirmação deve revalidar o contexto no servidor; não confiar na elegibilidade exibida anteriormente. Sem substituir as condições individuais por valores atuais do Módulo.

A proposta precisa registrar explicitamente `confirmada_em` e `substituida_em` (timestamptz nullable), `data_inicio_vigencia` e `data_fim_vigencia` (date nullable), sem inferir esses valores a partir das datas de inclusão/movimento do legado. Os registros existentes recebem null, preservando seu conteúdo. O fim expressa a data a partir da qual a versão foi substituída (limite exclusivo), permitindo substituir duas versões no mesmo dia sem inventar um último dia anterior ao início.

Acrescentar índice único parcial `ux_proposta_nativa_vigente_vida`, por Vida, apenas para propostas nativas (`legado_id IS NULL`), vigentes, não excluídas e com Vida. Não impor a restrição aos registros legados nem corrigir duplicidades silenciosamente. A operação deve conferir todas as vigentes da Vida e recusar ambiguidades mesmo em registros legados.

A proposta anterior conserva situação, conteúdos, beneficiários, prêmios e versão; somente o encerramento da vigência e os metadados de substituição mudam. Históricos já gravados na preparação não serão reescritos; acrescentar evento de confirmação/substituição quando aplicável. UUIDs, datas, instantes, revisão e flags são técnicos. Motivo/observação usam os campos humanos existentes e a política compartilhada de Title Case. Não normalizar evidências históricas.

Consultar o histórico existente e navegar entre versões faz parte desta etapa, sem conceder permissões adicionais ou expor IDs internos. Eventos de preparação, agendamento e substituição são acrescentados sem editar os anteriores. A confirmação inicial fica registrada no instante da proposta e na auditoria, sem fabricar uma proposta anterior para preencher a tabela de histórico.

## Agendamento e Revalidação

As datas usam o calendário de `America/Sao_Paulo`; instantes são persistidos em UTC. A data informada deve pertencer aos períodos da Apólice/Vida e não anteceder o início conhecido da versão de origem. Para ativar, a data deve ter chegado e os períodos ainda devem abranger o dia atual. Planos/coberturas e identidade são revalidados na confirmação e na efetivação, preservando os valores registrados. Mudança de Módulo/Subgrupo ou condições indisponíveis impede a ativação, sem atualizar silenciosamente o snapshot.

O processador hospedado na API, sem novo projeto ou biblioteca, verifica os agendamentos a cada minuto quando `Propostas__ProcessarAgendamentos=true`. Também processa datas vencidas após reinício. Cada ativação tem escopo e transação próprios, bloqueia Apólice/Proposta, confere a origem e é auditada com identidade técnica `sistema:vigencias-propostas`. Múltiplas instâncias são toleradas pela revalidação após o bloqueio e pela unicidade nativa. A API também permite solicitar a efetivação de um agendamento cuja data chegou, com `propostas.alterar` e a revisão esperada; não antecipa vigência.

A leitura de elegibilidade mantém bloqueios compartilhados até o commit sobre Vida, situações, Módulo/Subgrupo, Plano, vínculos de Cobertura e cadastro global da Cobertura. O Cadastro fornece contratos específicos que bloqueiam Cliente/Pessoa/Vínculo e Módulo global na mesma transação física, sem consultas entre schemas na Application/Seguro. Assim, uma alteração concorrente desses dados não passa entre a validação e a auditoria. O encerramento da origem é salvo antes de ativar a nova, dentro da mesma transação, garantindo a ordem da unicidade sem depender da ordenação dos comandos do EF.

Acrescentar `motivo_pendencia_vigencia` (varchar(1000), nullable, texto humano) e `pendencia_vigencia_em` (timestamptz nullable, técnico). Falha de elegibilidade na data de ativação mantém ambas as versões e registra uma pendência auditada. Repetir a mesma pendência não gera novas escritas/auditorias; novas tentativas são feitas sem depender dessa alteração. Uma causa diferente atualiza a pendência, e a ativação bem-sucedida a limpa. Listagem em páginas por ID evita que uma pendência antiga impeça o processamento das demais. A classificação do motivo será sincronizada em C#, TypeScript e SQL, com testes e reaplicação de triggers sem normalizar histórico.

`Propostas__StatusConfirmadaCodigo` é obrigatório para confirmar/agendar; o catálogo específico define `CONFIRMADA`, visível, não finalizador e sem movimentação financeira. Essa situação registra a confirmação; o indicador de vigência e as datas distinguem agendada, vigente e substituída. Pré Propostas/legados não recebem confirmação retroativa pela migration. Agendamentos confirmados ficam somente para consulta; cancelamento e reagendamento são operações futuras.

Confirmações com início futuro são recusadas se o processamento agendado estiver desabilitado. A indicação `Vigente` representa a situação operacional registrada; não substitui os limites dos períodos da Apólice/Vida nem implementa cancelamento/expiração automática. A ativação atrasada mantém a data informada como início de negócio e registra o instante real do processamento. Mudança de contexto de uma pré proposta não é revisada automaticamente; a operação explícita de revisão desse contexto permanece fora desta etapa.

## Contratos e Uso

- `POST /api/propostas/{publicId}/confirmacao`: `revisaoEsperada` e `dataInicioVigencia` obrigatórios; responde `vigente`, `agendada` e nova revisão.
- `POST /api/propostas/{publicId}/vigencia/efetivar`: revisão obrigatória, data já alcançada; responde `ativada`, `jaEfetivada`, revisão e eventual `pendenciaVigencia`. Uma repetição após ativação retorna a situação atual sem duplicar auditoria/histórico.
- GET de detalhe: acrescenta datas/instantes, motivo/instante da pendência, `podeConfirmar`, `podeEfetivarVigencia`, `situacaoVigenciaCodigo` e `historico`. Os códigos técnicos são `SEM_VIGENCIA`, `AGENDADA`, `ATIVACAO_PENDENTE`, `VIGENTE` e `SUBSTITUIDA`. São adições aos contratos anteriores.

Na consulta da proposta, `Confirmar proposta` abre formulário com data vazia, preenchida explicitamente pelo usuário, e explica o efeito sobre a anterior. `Confirmar com esta data` é o envio definitivo. Campos e navegação ficam bloqueados durante a operação; cancelamento não grava. Falhas preservam a data e oferecem recarga. O detalhe é recarregado após a confirmação/efetivação. Quando uma ativação fica pendente, mostra a causa e oferece `Efetivar vigência agora` após correção do contexto, sem antecipar datas. Histórico mantém eventos de versões ocultas/excluídas sem oferecer links para elas.

## Preservação de Textos

O mecanismo compartilhado passou a normalizar apenas textos humanos incluídos ou alterados, tanto no EF quanto nos triggers SQL de UPDATE. Isso preserva observações e outros textos históricos intocados quando a versão anterior recebe apenas metadados de substituição. INSERT e novas edições humanas continuam obrigatoriamente em Title Case; listas compartilhadas e proteção dos campos técnicos/originais permanecem. Não há exceção local ao normalizador. Testes C#/SQL e comparação integral dos demais campos de uma origem legada comprovam a preservação.

## Instalação e Execução

Aplicadas exclusivamente `20261112111100_ConfirmacaoVigenciaPropostas` e `20261112111200_DefinirConfirmadaProposta`, pelo instalador `backend/scripts/aplicar-propostas-teste.ps1 -Confirmacao -Aplicar`. As etapas `-Preenchimento` e `-Confirmacao` são mutuamente exclusivas. Sem `-Aplicar`, o instalador permanece somente leitura. O script `06-confirmacao-vigencia.sql` reúne as duas migrations em uma transação, exige o baseline do preenchimento e recusa outro banco além de `webapolice_teste`. Compara todos os campos anteriores das quatro tabelas de Propostas, os registros já existentes de situações, perfis e concessões antes do commit. Não executa atualização geral de migrations, não reescreve as propostas e não concede permissões.

O índice parcial e as colunas foram conferidos por `verificar-confirmacao-propostas.sql`, assim como `CONFIRMADA` e o argumento do motivo nos triggers. O script integral Title Case foi reaplicado com normalização histórica desligada; sua função de UPDATE compara os campos humanos com OLD. A conferência final da instalação encontrou uma proposta, zero confirmadas, zero agendadas/pendentes e nenhuma pendência. A proposta existente foi preservada. Não foi confirmada nem alterada proposta real para ensaio.

Backup **somente da estrutura**, sem registros, em `scratch/propostas-confirmacao-20261112-123235/estrutura-anterior.sql`. Diagnóstico e instalação em `scratch/propostas-confirmacao-{diagnostico,instalacao}.log`, ignorados pelo Git. O rollback estrutural recusa a remoção das colunas se houver datas, confirmações ou pendências; o catálogo `CONFIRMADA` não é removido automaticamente e seu rollback é bloqueado quando usado.

A configuração externa local, ignorada pelo Git, preserva as entradas anteriores e acrescenta:

```text
Propostas__StatusConfirmadaCodigo=CONFIRMADA
Propostas__ProcessarAgendamentos=true
```

`Propostas__StatusInicialCodigo=RASCUNHO` permanece. O carregador preserva as duas novas entradas opcionais, sem assumir valores quando ausentes. O processador é desabilitado por padrão e a configuração de confirmação é vazia por padrão. Migrations não executam no startup.

API final publicada em `backend/src/WebApolice.Api/bin/ExecucaoConfirmacaoPropostas-20261112-r2`, validada temporariamente em 5008 e ativada em 5007, PID 19480. A revisão final acrescenta os bloqueios de leitura de contexto descritos acima, sem migrations adicionais. Os hashes de `appsettings.json`/`appsettings.Development.json` coincidiram com a publicação anterior; JWT/audience/Keycloak não foram alterados. Os endpoints `/api/health/live` e `/api/health/ready` e o frontend 5173 responderam 200; as duas novas operações POST retornaram 401 sem token, com UUID fictício. A consulta do processador foi observada sem erro, com a fila vazia. O stderr ficou vazio; as instâncias temporárias foram encerradas. Logs/metadados em `scratch/propostas-confirmacao-api-20261112/`.

## Validação

Backend completo: **581 testes aprovados** — 6 Auditoria Unit, 2 Auditoria Integration, 66 Cadastro, 28 Arquitetura, 143 API e 336 Integration. Compilação sem erros ou avisos; modelo EF conferido sem alterações pendentes de migration. Os testes cobrem confirmação inicial, data informada/retroativa, agendamento, fronteira de meia-noite em São Paulo, retomada após reinício, duas instâncias, pendência/repetição/nova tentativa, identidade técnica na auditoria, contexto indisponível, revisão antiga, unicidade por SQL direto e rollback conjunto. Comparação dos demais campos de uma origem legada comprova que a substituição não reescreve seu conteúdo.

O teste que pausa a confirmação na auditoria tenta inativar Vida, vínculo e Módulo global em outra conexão; as três alterações são bloqueadas até a conclusão. A primeira execução desse teste encontrou um problema na fixture: a conexão de `SET lock_timeout` era devolvida ao pool e perdia a configuração, causando timeout do cliente em vez do erro PostgreSQL esperado. A fixture passou a manter a sessão aberta, sem alterar a exigência da verificação; a suíte completa foi repetida com sucesso.

Frontend: **1.268 testes aprovados em 83 arquivos**, com `format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run` e `build` aprovados. Zero pendências de formatação, erros de tipos, erros/avisos de lint ou ocorrências no scanner do design system. A consulta preserva as quatro casas dos percentuais para conferir exatamente a participação antes de confirmar. Cobertura dos três temas, permissões, confirmação/data, erro e recarga, envio duplicado, agendamento, efetivação e histórico. Uma pendência inicial de formatação em um teste foi corrigida com o Prettier existente e a rodada completa repetida. O aviso preexistente do build sobre chunk acima de 500 kB permanece sem flexibilização de limites.

Scanner local de segredos e diff aprovados no fechamento. Não há navegador conectado; a homologação visual autenticada permanece pendente. Cancelamento, reagendamento, revisão explícita de contexto e regras financeiras continuam para próximas etapas.

## Arquivos Centrais

- Frontend: `ConfirmacaoProposta.tsx`/schema/testes, integração em `PropostaDetalhes.tsx`, consulta de histórico, contratos/API e testes dos três temas.
- Backend: handlers `ConfirmarProposta`/`EfetivarVigencia`, validações compartilhadas, `ProcessadorVigenciasPropostas`, consultas/repositório/DTO/controlador, configuração e contratos de leitura protegida do Cadastro.
- Dados: entidade/configuração/migration/designer/snapshot, migration específica de `CONFIRMADA`, índice nativo e motivo de pendência classificado nas três camadas; padronização compartilhada preservando textos intocados.
- Operação: script pontual de instalação, verificadores, carregador externo e documentação. Alterações locais anteriores foram preservadas; nenhuma biblioteca, projeto ou permissão de perfil foi adicionada.
