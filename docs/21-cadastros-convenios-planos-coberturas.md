# Convênios de Cobrança, Planos e Coberturas

**Continuidade:** o cadastro contratual de Planos/Coberturas foi refatorado para o vínculo do Módulo na Apólice, com Plano único exclusivo e Coberturas do cadastro compartilhado, com prêmios na associação. A regra atual e a transição estão em [22 — Plano e Coberturas do Módulo](22-plano-coberturas-modulo-apolice.md). Este documento preserva o histórico da entrega anterior; Convênios/Subgrupos continuam conforme descritos abaixo.

Entrega de 06/10/2026, autorizada pelo responsável após a auditoria em [20](20-auditoria-convenios-coberturas.md).

## Regras confirmadas

- Convênio de Cobrança pertence tecnicamente ao Financeiro e é um recurso do módulo de segurança CADASTRO. O formulário disponibiliza todos os campos funcionais existentes: banco, agência, conta, empresa, remessa/retorno, layout, Vindi, observação, inscrição estadual e estabelecimento/endereço. Nome é obrigatório; os demais campos são opcionais. Identificadores legados e técnicos permanecem internos.
- Cada novo Subgrupo exige um Convênio ativo. Um mesmo Convênio pode ser compartilhado por vários Subgrupos. O Subgrupo existente sem Convênio pode continuar temporariamente vazio e receber um vínculo posteriormente. Depois de preenchido, o vínculo não pode ser removido; pode ser substituído por outro Convênio ativo.
- A inativação de Convênio impede novos vínculos e preserva os existentes. A edição do Subgrupo pode manter seu Convênio inativo, exibido como tal no seletor.
- Cobertura é um cadastro base do Seguro e pode participar de vários Planos. O par Plano/Cobertura é único; o vínculo possui status, timestamps e prêmios monetários padrão de titular e cônjuge. Inativar um vínculo preserva o histórico e não inativa a Cobertura. Para vincular ou reativar, Plano e Cobertura devem estar ativos.
- A relação atual `seguro.produto.plano_id` é preservada. Esta entrega inclui o cadastro de Plano necessário ao vínculo de Coberturas, sem CRUD de Produto.
- `seguro.apolice_cobertura` continua representando o Universo Permitido contratual, independente do catálogo global `seguro.plano_cobertura`.

## Funcionalidades e segurança

As telas ficam em `/convenios-cobranca`, `/coberturas` e `/planos`. Cada cadastro oferece lista paginada, busca por nome, filtro de status, detalhes, criação, edição, inativação e reativação. Os detalhes e a edição de Plano incluem seus vínculos de Coberturas. No novo cadastro, o botão "Salvar e vincular coberturas" salva o Plano e abre sua edição com o seletor de Coberturas. Quando não há Cobertura ativa, a tela informa essa situação e oferece acesso ao cadastro, conforme as permissões. A aba Subgrupos da Apólice mostra o Convênio e permite sua seleção na criação/edição.

| Recurso do módulo CADASTRO | Permissões |
|---|---|
| CONVENIOS_COBRANCA | `convenios_cobranca.visualizar`, `.inserir`, `.alterar`, `.inativar`, `.reativar` |
| COBERTURAS | `coberturas.visualizar`, `.inserir`, `.alterar`, `.inativar`, `.reativar` |
| PLANOS | `planos.visualizar`, `.inserir`, `.alterar`, `.inativar`, `.reativar`, `planos.coberturas.alterar` |

A leitura dos vínculos usa `planos.visualizar`; a seleção de novas Coberturas também exige `coberturas.visualizar`. A consulta mínima de Convênios para Subgrupo usa `apolices.visualizar` e retorna somente UUID, nome e status, sem detalhes financeiros. As mutações do Subgrupo continuam exigindo `apolices.subgrupos.inserir/alterar/inativar`.

Não foram concedidas permissões automaticamente a perfis. Administrador com acesso total continua sujeito a módulo e recurso habilitados. Para outros perfis, as concessões podem ser feitas no gerenciamento de segurança existente.

As rotas de entidade usam UUID público. PKs e FKs numéricas são internas. O Seguro consulta Convênios por contrato público do Financeiro, sem importar seu DbContext/modelo nem executar join entre os schemas de negócio. As FKs entre schemas são criadas explicitamente nas migrations.

As mutações de Convênios, Coberturas, Planos, vínculos e Subgrupos usam a mesma conexão/transação física para negócio e auditoria. Uma falha na gravação da auditoria reverte o cadastro. A auditoria identifica o registro por UUID e inclui estados anterior/posterior.

## Migrations e ambiente local

Ordem: Financeiro → Seguro → Segurança.

1. `20261006224700_CriarConveniosCobranca`: cria somente `financeiro.convenio_cobranca`, com UUID, status e FK opcional de banco. Os demais modelos financeiros estão excluídos desta baseline; sua persistência precisa de entrega própria.
2. `20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo`: acrescenta UUIDs a Plano/Cobertura, vínculo opcional no Subgrupo e `seguro.plano_cobertura`. A coluna opcional permite a transição dos registros antigos.
3. `20261006030000_CatalogosConveniosCoberturasPlanos`: cadastra recursos/permissões com UUIDs fixos e recompõe as três permissões de mutação de Subgrupos quando ausentes. Não altera perfis.
4. `20261007000150_PremiosPlanoCoberturaEAjustesApolice`: acrescenta prêmios no Plano/Cobertura, ajustes separados por Apólice e UUID público para o vínculo contratual. O identificador técnico da migration segue o relógio UTC; a entrega ocorreu em 06/10/2026 no horário de São Paulo.

Scripts SQL idempotentes revisáveis estão em `backend/scripts/migrations/catalogos/`. `backend/scripts/aplicar-catalogos-dev.ps1` carrega a conexão externa, aceita somente `webapolice_teste`, valida os baselines anteriores do Seguro e Segurança e aplica esses quatro scripts. Foram aplicados nesta base; a API local foi reiniciada em `http://localhost:5007`.

A produção não foi consultada ou alterada. Antes de transportar a entrega para outro ambiente, comparar estruturas e histórico, em especial a existência prévia de tabelas financeiras. O rollback de schema elimina dados dos novos cadastros/vínculos e exige planejamento; não é um mecanismo de desfazer operações do usuário.

Foi removida dos contextos Financeiro e Seguro a configuração de conexão fixa. Eles agora exigem configuração externa e respeitam a conexão injetada. Durante a primeira tentativa de teste, o comportamento legado do Financeiro direcionou a criação da tabela de Convênios para desenvolvimento. Foi confirmado que ela estava vazia; nenhum registro de teste foi criado ali.

## Validação

`backend/scripts/test-catalogos-isolado.ps1` cria uma base temporária com nome controlado, copia estrutura, histórico de migrations e catálogo técnico, executa os testes e remove a base ao finalizar. Não copia cadastros de clientes nem usuários. Os testes rejeitam conexão com qualquer nome fora do padrão temporário e verificam que os contextos usam a conexão fornecida.

- Doze casos PostgreSQL: cadastro e status, campos financeiros, compartilhamento, propriedade do Subgrupo pela Apólice, transição do legado, vínculos N:N únicos, inativação, rollback de auditoria e autorização com módulo/recurso desabilitado.
- Sete testes HTTP verificam autenticação obrigatória nas consultas e na alteração dos prêmios.
- Doze testes da interface cobrem a aba Subgrupos e a seleção obrigatória, vínculo legado vazio, preservação de Convênio inativo, vínculo de Cobertura na edição de Plano, a etapa após criar Plano e ausência de Coberturas ativas.
- Vinte testes de arquitetura passaram. Build do backend, typecheck/build do frontend e lint dos componentes alterados foram executados.
- API local: health/live 200; novas consultas sem token 401.

A inspeção visual em navegador não pôde ser executada porque esta sessão não disponibilizou navegador conectado. A homologação visual autenticada permanece para o responsável; os testes de componentes e os builds foram realizados. Os demais testes antigos de integração dependem de Docker e não foram executados nesta máquina.

## Continuidade

O Subgrupo legado continua sem Convênio até uma escolha real do responsável. A obrigatoriedade física futura (`NOT NULL`) depende desse preenchimento. Permanecem fora desta entrega a inversão Produto–Plano, o CRUD de Produto e a conversão dos IDs internos na resposta antiga do Universo Permitido. As referências históricas a Subgrupo global nos documentos 16/17/18 estão sinalizadas como superadas; `cadastro.subgrupo` legado não foi removido.


## Prêmios de titular e cônjuge — complemento confirmado em 06/10/2026

O responsável definiu valores monetários em R$ no vínculo Plano–Cobertura, com possibilidade de ajustes próprios por Apólice.

- `seguro.plano_cobertura.premio_titular` e `.premio_conjuge`: valores padrão, `numeric(18,2)`. Novos vínculos e edições dos prêmios exigem os dois valores. São aceitos zero e valores positivos, com até duas casas decimais; negativos e casas adicionais são rejeitados.
- `seguro.apolice_cobertura.premio_titular_override` e `.premio_conjuge_override`: ajustes opcionais e independentes. Para cada pessoa, o valor efetivo é o ajuste quando informado, ou o padrão do vínculo Plano/Cobertura. `null` usa o padrão; zero é ajuste explícito de R$ 0,00.
- Alterar um padrão do Plano atualiza a leitura das Apólices que o herdam. As Apólices com ajuste próprio preservam seu valor. A inativação do vínculo global preserva os parâmetros e os contratos existentes.
- Registros preexistentes recebem valores nulos. Nenhum prêmio é inventado ou convertido automaticamente em zero. O campo genérico legado `premio_override` permanece independente; não foi convertido automaticamente para titular/cônjuge.

A seção Coberturas do Plano exibe os dois prêmios e abre o formulário ao vincular ou editar prêmios. A reativação preserva valores existentes; vínculos antigos sem valores precisam ser preenchidos antes de reativar. A edição de um vínculo inativo pode manter sua inativação.

Na aba Universo Permitido da Apólice, os valores efetivos e sua origem aparecem por Cobertura. O botão "Ajustar prêmios" permite personalizar cada valor ou deixar em branco para voltar ao padrão. Esse ajuste usa `apolices.alterar`, valida a propriedade do vínculo pela Apólice e exige Apólice/Produto/Plano/Cobertura contratuais ativos. Não cria produtos, planos ou coberturas no Universo Permitido: pressupõe os vínculos contratuais existentes.

Contratos da API:

- `GET /api/planos/{planoPublicId}/coberturas`: inclui `premioTitular` e `premioConjuge`.
- `PUT /api/planos/{planoPublicId}/coberturas/{coberturaPublicId}`: recebe os dois valores e `ativo`, cria/atualiza o vínculo com auditoria. Exige `planos.coberturas.alterar`.
- `PATCH .../reativar` e `PATCH .../inativar`: preservam os prêmios.
- A consulta do Universo Permitido inclui UUID do vínculo contratual, nomes, prêmios padrão, ajustes e valores efetivos.
- `PUT /api/apolices/{apolicePublicId}/coberturas/{vinculoPublicId}/premios`: recebe os dois valores opcionais; permite retornar ao padrão com `null`. O UUID identifica o vínculo contextual em `apolice_cobertura`, sem expor ID numérico na nova mutação.

A migration foi validada e aplicada somente em `webapolice_teste`. Doze testes PostgreSQL passaram, incluindo persistência de centavos, validação, zero, herança, mudança de padrão, ajustes independentes, isolamento por Apólice e rollback da auditoria. Oito testes de interface passaram na verificação deste complemento, incluindo envio dos prêmios no cadastro de Plano, validação monetária, retorno ao padrão e permissões na aba da Apólice. A leitura do Universo Permitido resolve os parâmetros; os fluxos futuros de emissão/cobrança devem consumir esses valores conforme seu próprio escopo, sem presumir que este complemento implementou cálculo ou emissão financeira.
