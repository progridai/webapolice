# Preenchimento de Pré Proposta

Data: 12/11/2026. Complementa o [documento 83](83-fluxo-operacional-propostas.md). Etapa implementada: editar observação, preencher beneficiários e ajustar prêmios de coberturas da proposta nativa em pré proposta, sem confirmar adesão, alterar a Vida ou gerar movimentos financeiros.

## Modelagem Antes das Alterações

- Acrescentar `proposta.revisao_preenchimento`, inteiro não nulo, padrão zero. É uma revisão técnica de edição, independente da versão de negócio. O cliente envia a revisão lida; outra gravação concluída antes exige recarregar a pré proposta. Não reutilizar `versao` para controlar concorrência.
- Acrescentar `proposta_beneficiario.public_id`, UUID não nulo e único, gerando identidade técnica para os registros existentes; tornar `legado_id` anulável para registros nativos, preservando os valores e sua unicidade.
- Acrescentar `proposta_beneficiario.parentesco_registrado`, texto humano opcional durante a transição. Não escrever em `parentesco_original`, `parentesco_normalizado`, CPF original/limpo, PessoaId ou nomes normalizados. Novos beneficiários não duplicam o cadastro de Cliente/Pessoa: são registros de indicação na proposta.
- `Nome`, `Observacao` e `ParentescoRegistrado` são textos humanos. Nome/observação já possuem classificação; incluir o parentesco registrado nas listas compartilhadas de C#, TypeScript e SQL, com testes. UUID, revisão, percentuais e identificadores são técnicos. Não reprocessar o histórico.

## Contrato e Proteções

O salvamento é uma operação única de preenchimento, com observação, beneficiários editáveis, coberturas editáveis e revisão esperada, protegida por `propostas.alterar`. A omissão de beneficiário nativo anteriormente indicado representa remoção lógica apenas ao salvar o conjunto revisado. Cancelar o editor não grava nada e alterações locais exigem confirmação de descarte. Beneficiários de origem anterior continuam consultáveis e protegidos; não são removidos nem reescritos pela ausência no conjunto editável.

Somente proposta nativa, visível, não excluída, sem vigência e no status inicial configurado, não finalizador e sem movimentação permitida, pode receber o preenchimento. Confirmadas, legadas e outras situações são consulta. Referências, nomes e flags registrados do Plano/Coberturas são preservados. Ajustes dos prêmios seguem a regra específica abaixo. A validação definitiva ocorre novamente no backend dentro da transação auditada, com bloqueio da Apólice/Proposta e revisão técnica.

Regras aprovadas: cada beneficiário exige nome (150 caracteres), parentesco (100 caracteres) e percentual maior que zero, até quatro casas decimais. A soma pode ser menor que 100% na pré proposta, nunca maior. CPF/Pessoa ficam para etapa posterior; a confirmação futura exigirá 100%.

Prêmios titular/cônjuge podem ser ajustados em coberturas nativas ativas do Módulo: valores não negativos, até duas casas decimais, vazio preserva ausência de valor. Cada mudança exige `JustificativaAjustePremio` de até 1000 caracteres, texto humano classificado em C#, TypeScript e SQL. Acrescentar `premio_ajustado` (bool, padrão false), `premio_titular_inicial`/`premio_conjuge_inicial` (numeric(18,2), nullable) e `justificativa_ajuste_premio` (varchar(1000)). O primeiro ajuste guarda a base, inclusive null; os seguintes preservam essa base. A auditoria transacional conserva cada antes/depois dos valores e justificativa. Alterar somente a justificativa sem mudar valores não reescreve a anterior. Não recalcular totais ou alterar o Módulo.

Nenhuma confirmação, cálculo, título, cobrança, comissão ou substituição efetiva está incluída nesta etapa.

## API e Interface

`PUT /api/propostas/{publicId}/rascunho` recebe `revisaoEsperada`, `observacao`, `beneficiarios` e `coberturas`. Ambas as listas são obrigatórias, inclusive quando vazias. Beneficiários novos usam `publicId: null`; os existentes usam o UUID retornado na consulta, nunca IDs internos. O conjunto de coberturas deve conter exatamente uma ocorrência de cada cobertura editável. A resposta devolve `revisaoPreenchimento`.

O GET de detalhe acrescenta `revisaoPreenchimento`/`podeEditarRascunho`, UUID/`podeAlterar` nos beneficiários e `podeAjustarPremio`/base/justificativa nos prêmios. São adições aos contratos anteriores. A autorização continua no catálogo APOLICES/PROPOSTAS; não há concessão automática a perfis. IDs desconhecidos, de outra proposta, repetidos, excluídos ou protegidos são recusados.

O editor aparece nos detalhes da proposta mediante permissão e elegibilidade. Usa os campos, mensagens e tokens compartilhados, com validação em schema. Aceita vírgula ou ponto decimal sem arredondar valores digitados; recusa representações que perderiam precisão ao serializar o JSON. Campos vazios de prêmio persistem null, diferentes de zero. O botão de salvar fica bloqueado desde o início da validação assíncrona. Durante a edição, os comandos de navegação interna e o fechamento do modal da Vida ficam bloqueados; cancelar ou descartar devolve a consulta.

Falhas preservam os dados do editor. O erro de revisão orienta recarga; a ação de recarregar confirma o descarte das alterações locais. Depois de salvar, a consulta é recarregada para mostrar os dados efetivamente persistidos e normalizados. A auditoria guarda UUIDs/percentuais dos beneficiários, presença de observação e antes/depois dos prêmios/justificativas, sem duplicar nomes, parentescos, CPF ou o conteúdo livre da observação.

## Validação e Instalação

Migration específica: `20261112104220_PreenchimentoRascunhoPropostas`. O rollback recusa a remoção das colunas se houver preenchimentos, beneficiários nativos ou novos dados de ajuste/parentesco, evitando perda silenciosa. O modelo, designer e snapshot foram atualizados juntos.

O instalador existente aceita `-Preenchimento -Aplicar`, utilizando exclusivamente `05-preenchimento-rascunho.sql`. Sem `-Aplicar`, permanece somente leitura. A instalação exige o baseline da Pré Proposta e `webapolice_teste`, bloqueia as tabelas de Propostas durante a transação e compara assinaturas de todos os campos anteriores das quatro tabelas, perfis e concessões antes do commit. O script é idempotente e não executa atualização geral de migrations. O script integral de Title Case é reaplicado em transação própria com normalização histórica desligada. `verificar-preenchimento-propostas.sql` confirma a migration, identidade pública e os dois novos campos nos triggers.

Aplicado em 12/11/2026. A proposta já existente foi preservada; a instalação terminou com uma proposta, revisão zero e nenhum beneficiário criado. Nenhuma proposta real foi preenchida para ensaio. Backup **somente da estrutura**, sem registros, em `scratch/propostas-preenchimento-20261112-075258/estrutura-anterior.sql`. Diagnóstico/instalação em `scratch/propostas-preenchimento-{diagnostico,instalacao}.log`, ignorados pelo Git.

A API foi publicada em `backend/src/WebApolice.Api/bin/ExecucaoPreenchimentoPropostas-20261112`, validada temporariamente em 5008 e ativada na porta 5007, PID 41836. Os hashes de `appsettings.json`/`appsettings.Development.json` coincidiram com a publicação anterior. Health/live e ready em `/api/health/` responderam 200; frontend 5173 respondeu 200; PUT de pré proposta sem token retornou 401. O stderr ficou vazio e o processo temporário foi encerrado. Metadados/logs em `scratch/propostas-preenchimento-api-20261112/`.

Backend: 557 testes aprovados nas seis suítes (6 Auditoria Unit, 2 Auditoria Integration, 63 Cadastro, 28 Arquitetura, 141 API e 317 Integration). A rodada completa identificou uma fixture SQL artificial sem `cpf_valido`, obrigatório no schema anterior; a fixture foi corrigida, os 43 testes de Propostas e toda a suíte de integração foram repetidos com sucesso. Compilação da entrega sem erros/avisos. Cobertura inclui soma parcial/excedente, precisão, campos obrigatórios, IDs inválidos/protegidos, exclusão lógica, origem dos prêmios, ajustes sucessivos, ausência de geração financeira, rollback da auditoria, revisão antiga e duas conexões concorrentes. Evolução de legado e Title Case via SQL também foram conferidos.

Frontend: os seis comandos obrigatórios passaram, incluindo 1.251 testes em 81 arquivos, zero pendências de formatação, tipos, erros/avisos de lint ou ocorrências de design system. O aviso preexistente do build sobre chunk acima de 500 kB permanece sem flexibilização dos limites. Testes cobrem os três temas, permissões, erros, recarga/descarte, leitura normalizada, precisão, null/zero e envio duplicado durante validação. O teste de duplicidade encontrou uma brecha inicial na validação assíncrona; o bloqueio foi antecipado ao evento de submit e a suíte repetida.

Na conclusão desta etapa não havia navegador conectado, portanto a homologação visual autenticada permaneceu pendente. Confirmação e substituição efetiva com início informado/agendamento foram implementadas posteriormente na [etapa 3](85-confirmacao-substituicao-propostas.md). Cancelamento, reagendamento e regras financeiras continuam para próximas etapas.

## Arquivos Centrais

- Frontend: `PreenchimentoRascunhoForm.tsx`, schema próprio da feature, integração em `PropostaDetalhes.tsx`, contratos/API e testes de Propostas.
- Backend: `PreencherRascunhoCommand.cs`/handler, porta/adaptador de Propostas, DTO de detalhe e endpoint; entidades/configurações, migration/designer/snapshot.
- Dados: classificação humana compartilhada C#/TypeScript/SQL, testes de normalização e migração; instalador de teste e verificadores específicos.
- As alterações locais anteriores foram preservadas; nenhuma dependência ou permissão de perfil foi adicionada.
