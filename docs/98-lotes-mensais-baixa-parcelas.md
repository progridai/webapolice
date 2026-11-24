# Lotes Mensais e Baixa de Parcelas Nativas

Data: 18/11/2026. Regras aprovadas; ciclo financeiro implementado e validado localmente. Complementa o documento 97. Não há emissão de títulos, envio de cobrança ou execução bancária.

## Regras e Escopo

Primeira parcela automática ao efetivar vigência. Meses seguintes gerados manualmente por Apólice e competência, com conferência anterior, incluindo existentes, elegíveis, exclusões e pendências. A competência seguinte exige geração/fechamento da anterior; o fechamento pode deixar parcelas abertas. A baixa é manual e integral nesta entrega, registrando a data informada; pagamentos parciais e estornos exigem evolução própria. Fechar significa concluir a conferência, não declarar que todos pagaram. Não há job de geração mensal nem fechamento automático na data do Estipulante: o usuário executa a rotina na data aplicável.

A inadimplência permanece nas parcelas antigas; gera-se apenas a nova mensalidade e apresenta-se saldo anterior e total acumulado por Vida, sem capitalizar ou duplicar débitos. Parcelas de competências já geradas não mudam com atualização de proposta: o novo valor vale para a próxima competência ainda não gerada. A sequência crescente é da Vida dentro da Apólice, entre versões e reinclusões, considerando o maior número registrado, inclusive legado vinculado, sem renumerar registros anteriores.

O usuário confere Ramo ativo da mesma Apólice, início, competência inicial (a partir do mês de início) e primeiro vencimento livre. Congelar líquido, alíquota e IOF na confirmação. O valor é mensal integral. Os vencimentos seguintes mantêm o dia original e o deslocamento de meses em relação à competência, com último dia em meses curtos e retorno ao dia original. Entre versões, reutilizar a referência do primeiro plano financeiro da Vida; mudanças de condições antes da ativação exigem nova conferência. Mudanças no IOF cadastral posteriores à confirmação não reprecificam o plano congelado.

Cancelamentos e substituições preservam parcelas e baixas anteriores. O lote não cria nova mensalidade para competências sem cobertura; pendências de ativação ou conferência são apresentadas. Propostas anteriores sem plano financeiro nativo não recebem gravação financeira presumida: permanecem no fluxo operacional anterior e são indicadas na conferência. Uma competência fechada não recebe inclusão automática tardia; a ativação registra pendência e exige corrigir a competência inicial por nova conferência.

## Modelagem Antes das Migrations

- `seguro.proposta_financeiro`: Id interno, UUID único, proposta única/FK, vínculo de Ramo/FK, início conferido, competência inicial, vencimento inicial e referência original de competência/vencimento, líquido, percentual e valor de IOF, total mensal, hash técnico das condições e instante da conferência. Plano só é alterado antes da vigência, sem parcelas que o utilizem.
- `seguro.parcela_competencia`: Id interno, UUID único, Apólice/FK, competência, instante de geração do lote (nullable para primeira inclusão automática), instante de fechamento e revisão técnica. Unicidade por Apólice/competência.
- Movimento existente recebe FKs opcionais para Vida, plano financeiro e competência, mais revisão de baixa. Registros legados continuam nulos/inalterados nesses campos. Mensalidade nativa é identificada pelo código técnico `PARCELA_MENSAL`; índices únicos por Vida/competência e Vida/número da parcela, restritos a essa classificação nativa. Restrições validam seus vínculos, valores não negativos, líquido + IOF = total e baixa integral. Não inferir catálogo de movimento legado nem ativar GeraTitulo.

UUIDs, hash, códigos, revisão, competência, datas e valores são técnicos. Nenhuma coluna humana nova. Observações e nomes existentes seguem Title Case compartilhado; não alterar listas ou reprocessar histórico por campos técnicos. DbContext Seguro mantém a base TextoPadronizadoDbContext. Rollback bloqueado após uso financeiro nativo; preservar os dados originais por comparação de hashes em testes.

## Transações, Autorização e Compatibilidade

Reutilizar `IOperacaoPropostas` para negócio e auditoria na mesma transação. Serializar por Apólice com o bloqueio já usado por confirmação, ativação e cancelamento. Revalidar conferência no servidor antes de gravar, sem confiar em totais do cliente. Repetição não duplica mensalidades ou baixa. Consultas exigem `propostas.visualizar`; conferência/gravação/geração/baixa/fechamento exigem `propostas.alterar`, preservando catálogo e permissões existentes.

Novas confirmações passam a exigir dados financeiros conferidos quando `Propostas:ExigirConferenciaFinanceira` estiver habilitado (padrão true). O payload financeiro é aditivo, mas a exigência funcional é explícita; clientes antigos precisam atualizar a confirmação. O parâmetro permite implantação controlada e testes isolados do fluxo operacional anterior. Confirmações já realizadas sem plano permanecem ativáveis sem criar cobranças retroativas presumidas.

Migrações somente preparadas nesta implementação; conferir baseline, backup e aplicar antes de publicar a API. Frontend segue o guia obrigatório, com consultas canceláveis, estados de erro/vazio e conferência explícita antes de cada escrita. A homologação visual autenticada é separada dos testes DOM.


## Operação na Interface e Contratos

Na confirmação da Pré Proposta, informar início, competência inicial, vencimento e Ramo. O botão “Conferir primeira parcela” consulta os valores; “Confirmar proposta e valores” salva o plano conferido e confirma a proposta. Qualquer edição descarta a conferência anterior. Proposta confirmada sem vigência oferece “Conferir financeiro novamente”, inclusive após revisão de condições ou reagendamento. A alíquota cadastral posterior não altera o plano já congelado. Competências pertencentes a ciclos fechados são recusadas na conferência e na ativação.

Na Apólice, a aba **Parcelas** exige `propostas.visualizar`. O usuário informa a competência, consulta o lote e identifica cada Vida por nome, matrícula, proposta e versão. A tabela separa nova mensalidade, saldo anterior e total acumulado, com paginação e detalhes das parcelas abertas. A geração exige confirmação explícita com hash da conferência atual. Parcelas automáticas da primeira competência aparecem como já geradas: confirmar o lote completa o controle do mês sem duplicá-las, antes de seu fechamento. O fechamento conserva parcelas não pagas; cada baixa exige a data do pagamento e confirma o valor integral. Baixar um atraso de mês fechado não reabre a competência.

| Endpoint | Permissão | Finalidade |
| --- | --- | --- |
| `GET /api/propostas/{id}/financeiro` | `propostas.visualizar` | Plano congelado e referência original; ausências permanecem null |
| `POST /api/propostas/{id}/financeiro/conferencia` | `propostas.alterar` | Conferência sem escrita com revisão e hash |
| `POST /api/propostas/{id}/confirmacao` | `propostas.alterar` | Payload aditivo `financeiro`, exigido quando a configuração estiver habilitada |
| `PUT /api/propostas/{id}/financeiro` | `propostas.alterar` | Nova conferência antes de efetivar a vigência |
| `GET /api/apolices/{id}/parcelas?competencia=YYYYMM` | `propostas.visualizar` | Elegibilidade, parcelas existentes, saldo e pendências |
| `POST /api/apolices/{id}/parcelas/lote` | `propostas.alterar` | Geração atômica de uma competência conferida |
| `POST /api/apolices/{id}/parcelas/fechamento` | `propostas.alterar` | Encerramento manual do controle mensal |
| `POST /api/apolices/{id}/parcelas/{parcelaId}/baixa` | `propostas.alterar` | Baixa integral de parcela nativa da mesma Apólice |

Valores monetários dos contratos financeiros usam strings canônicas de duas casas, preservando centavos acima da precisão do JavaScript. Percentuais permanecem números com até quatro casas. Hashes são técnicos e preservados nos normalizadores. A assinatura das condições usa prêmios canônicos, para que `30` e `30.00` tenham o mesmo significado. A auditoria de confirmação/ativação registra também identidade e valores da parcela criada. Nome e matrícula são consultados pelo contrato de Cadastro em lote, sem duplicar cadastros.

O processador de vigências existente recebe o repositório financeiro e cria somente a primeira parcela ao ativar a proposta. Não gera meses seguintes. Geração, baixas e fechamento compartilham lock por Apólice, revisão, revalidação e transação com auditoria. Falhas da auditoria revertem proposta/plano/parcela e as demais escritas. O controle de versão e as restrições únicas do banco impedem repetição de mensalidade e sequência; requisições de escrita desatualizadas são recusadas e exigem nova conferência.

## Arquivos e Implantação

Backend: modelos `FinanceiroPropostas`, configuração `SeguroDbContext.Financeiro`, porta/repositório `ParcelasPropostasRepository`, casos de uso `ConferenciaFinanceira`, `ParcelasNativas` e `LotesMensais`; integração em confirmação, efetivação e processador. API ampliada em Propostas e novo controlador `ParcelasApoliceController`.

Frontend: contratos/API `financeiro`, formulários `FinanceiroPropostaForm` e `LotesMensaisApolice`, integração na confirmação e na aba da Apólice. O DataTable compartilhado recebeu rótulo acessível no botão de expandir/recolher detalhes. Consultas usam `useConsulta` e AbortSignal, descartando resultados após mudança de contexto; escritas bloqueiam reenvio e controles da operação até concluir. A tela usa componentes e tokens existentes.

Migrations preparadas: `20261118140000_IdentidadeMovimentoNativo` e `20261118194921_CicloFinanceiroPropostas`. A segunda cria plano/controle mensal e FKs/revisão técnica nos movimentos; não preenche UUIDs nem altera valores/evidências legados. O snapshot corresponde ao modelo completo. O rollback encadeado é bloqueado antes de remover as novas colunas quando houver identidade nativa ou uso financeiro, evitando retorno parcial incompatível com os dados preservados.

Aplicar somente após conferir backup e baseline do ambiente. Nesta entrega, os testes usam PostgreSQL efêmero Docker; nenhuma dessas duas migrations foi aplicada a bancos persistentes. Publicar API/frontend em conjunto, pois a exigência financeira padrão altera a confirmação funcional. Não há dependência nova, emissão de título, remessa bancária ou envio de cobrança.


## Validação Local Concluída

| Verificação | Resultado |
| --- | --- |
| Suíte completa backend | 819 aprovados: 541 Integration, 172 API, 28 Arquitetura, 70 Cadastro, 6 Auditoria Unit e 2 Auditoria Integration |
| Revalidação final de Propostas/calendário após integração do processador | 212 aprovados; inclui o novo cenário de primeira parcela na ativação agendada, sem duplicação na segunda execução |
| Nova versão após ciclo fechado | 1 cenário adicional aprovado: recusa a competência fechada, aceita a seguinte e preserva sequência/histórico |
| Build backend final | Aprovado; zero erros e avisos |
| Consistência EF | `has-pending-model-changes`: nenhuma alteração pendente no modelo |
| Script SQL de implantação | Gerado de forma idempotente entre o baseline `20261118103000` e a migration financeira; não executado em banco persistente |
| `npm.cmd run format:check` | Aprovado; nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado; zero erros |
| `npm.cmd run lint` | Aprovado; zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado; zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.410 aprovados em 99 arquivos |
| `npm.cmd run build` | Aprovado |
| `git diff --check` | Aprovado |
| Verificação local de padrões reconhecidos de credenciais nos arquivos alterados/novos | Zero ocorrências |

Os testes cobrem IOF congelado, recusa de conferência antiga, reagendamento com nova conferência, primeira parcela imediata e agendada, repetição sem duplicação, sequência contínua entre versões incluindo maior número legado, preservação da parcela já gerada, cancelamentos, fechamento com inadimplência, transporte do saldo sem duplicação, baixa de dívida antiga após fechamento, isolamento entre Apólices, concorrência de lotes, restrições do banco e rollback de negócio quando a auditoria falha. Migrations preservam evidências legadas por hash e bloqueiam retorno destrutivo após uso nativo. A API verifica autenticação, permissões, catálogo e parâmetros das novas rotas. A interface cobre temas claro/escuro/sistema, valores exatos, conferência anterior à gravação, duplo envio, erro/nova tentativa/vazio, cancelamento de consulta pendente e descarte de resposta antiga.

As verificações completas foram executadas uma vez; após o ajuste final do caminho agendado e da validação de competência fechada, foram repetidos os 212 cenários de Propostas/calendário e o build do backend, além de aprovado um caso adicional de nova versão após fechamento. Logs locais em `scratch/financeiro-backend-validation.log`, `scratch/financeiro-ativacao-final-testes.log` e `scratch/financeiro-frontend-validation.log` e `scratch/financeiro-competencia-fechada-testes.log`; SQL preparado em `scratch/ciclo-financeiro-propostas.sql`. Nenhuma configuração de qualidade foi flexibilizada. O aviso preexistente de chunk de 505,44 kB do Vite permanece; o build foi aprovado. O runner backend também emite o aviso preexistente de licença do Fluent Assertions, sem nova dependência nesta entrega.

A homologação visual autenticada em navegador não foi executada. As duas migrations continuam pendentes de aplicação em ambiente persistente, antes da publicação. Não houve commit, push, alteração de produção ou operação bancária nesta etapa.


## Atualiza??o de Implanta??o Local

Ap?s autoriza??o posterior do usu?rio, as duas migrations foram aplicadas em `webapolice_teste`, com backup e confer?ncia transacional dos dados anteriores. O EF confirmou 35 migrations aplicadas e zero pendentes; API atualizada e frontend dispon?veis em `http://127.0.0.1:5007` e `http://127.0.0.1:5173`. Esta atualiza??o substitui a situa??o anterior de migrations somente preparadas. Evid?ncias e verifica??es no [documento 99](99-aplicacao-financeiro-execucao-local.md). A homologa??o visual autenticada permanece a cargo do teste do usu?rio.
