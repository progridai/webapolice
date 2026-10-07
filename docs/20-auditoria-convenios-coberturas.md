# Auditoria de Convênios de Cobrança e Coberturas

Data: 06/10/2026. Estado: auditoria inicial preservada abaixo como registro histórico. O responsável autorizou a implementação e confirmou as definições restantes; consulte [a entrega e as regras vigentes](21-cadastros-convenios-planos-coberturas.md).

## Contexto recebido

Esta auditoria parte do resumo consolidado fornecido pelo responsável pelo projeto, originado do projeto BI Rsul no ChatGPT. O resumo é a referência de continuidade desta sessão; não representa acesso ao histórico completo daquele projeto.

A orientação recebida estabelece a precedência: documentação vigente, padrões consolidados no código, modelagem/migrations atuais e nova decisão apenas quando necessária. Divergências devem ser apresentadas antes de implementar.

Decisões consolidadas informadas:

- Estipulante → Apólice; Subgrupos, Módulos, Subestipulantes e Vidas são estruturas diretamente ligadas à Apólice e independentes entre si.
- Subgrupo é contextual e pertence a uma única Apólice (`seguro.apolice_subgrupo`).
- Módulo global (`cadastro.modulo`) e vínculo (`seguro.apolice_modulo`) possuem identificadores públicos distintos.
- Vida pode ter Subgrupo e/ou Módulo opcionais, pertencentes à mesma Apólice.
- Relações antigas Vida → Subestipulante e Subestipulante → Módulo foram removidas.
- Não vincular Subestipulantes por IDs internos durante a criação da Apólice.
- Próximas funcionalidades: Convênio de Cobrança, com um convênio por Subgrupo; depois Coberturas, com várias coberturas por Plano.
- Cadastro base de Cobertura e permissão contratual de Cobertura na Apólice são conceitos diferentes.
- Apresentar auditoria e plano antes de implementar.

## Escopo e evidências

### Decisões confirmadas durante a auditoria

O responsável confirmou nesta sessão:

- Um Convênio de Cobrança pode ser compartilhado por vários Subgrupos. Cada Subgrupo possui um Convênio. A FK do Subgrupo não terá índice único; o tratamento transitório dos registros existentes ainda precisa ser definido.
- Uma Cobertura pode participar de vários Planos. O cadastro global permanece independente; `seguro.plano_cobertura` representará a associação N:N, com unicidade do par Plano/Cobertura.
- O cadastro de Convênios será um recurso do módulo de segurança `CADASTRO` existente. A propriedade técnica da entidade e da tabela continua no Financeiro. Não criar módulo habilitável `FINANCEIRO` para esta entrega.

Foram examinados a documentação local, modelos/configurações EF, migrations relacionadas, controllers, casos de uso, composição de serviços, constantes de autorização e estruturas do frontend. Não foram encontrados arquivos `AGENTS.md` no repositório nem nas pastas ancestrais verificadas.

O banco consultado foi **`webapolice_teste`**, apontado por `ConnectionStrings__PostgreSql` no arquivo local. As sessões PostgreSQL utilizaram `default_transaction_read_only=on`, confirmado pela consulta de `transaction_read_only`. Consultas limitaram-se a metadados, histórico de migrations, catálogo de segurança e contagens agregadas.

**Na etapa de auditoria inicial**, não houve consulta à produção, execução de migrations, gravação no banco, implementação de código funcional ou execução de testes. A auditoria não comprova a homologação mencionada no resumo; comprova as estruturas e os registros explicitamente descritos abaixo. O histórico Git e os arquivos não rastreados preexistentes foram preservados.

Referências principais:

- `docs/03-arquitetura-backend.md`: separação de módulos, contratos públicos e proibição de acessar Infrastructure de outro módulo.
- `docs/04-convencoes-backend.md`: Commands/Queries/Handlers, `CancellationToken`, tipos de data e auditoria.
- `docs/09-persistencia-postgresql-ef-core.md`: contexto por módulo e aplicação controlada de migrations.
- `docs/10-auditoria-rastreabilidade.md`: auditoria persistida e atomicidade de operações.
- `docs/16-modelagem-banco-dados-webapolice.md` e `docs/17-modelagem-banco-dados-webapolice.md`: localização dos conceitos por schema.
- `docs/guias/guia-implementacao-novos-modulos.md`: matriz de permissões, habilitação, aprovação de concessões a perfis e migrations separadas de segurança.
- `docs/guias/guia-uso-design-system.md`: componentes, tokens e paridade entre edição e detalhes.

## Convênio de Cobrança

### Código existente

O módulo `WebApolice.Modulos.Financeiro` possui a entidade EF `ConvenioCobranca`, mapeada para **`financeiro.convenio_cobranca`**. Portanto, o conceito pertence ao Financeiro, independentemente da localização do menu de cadastro.

O módulo `WebApolice.Modulos.Convenio` representa integrações SIAPE/Corsan. Não é o proprietário do cadastro de convênios de cobrança.

Campos existentes no modelo de Convênio:

- `Id`, `BancoId`, `Nome`.
- Agência, conta corrente, nome/código da empresa.
- Número/nome inicial/extensão/layout de arquivo, locais de remessa e retorno.
- Comunicação Vindi, observação.
- Inscrição estadual e campos de nome/endereço com prefixo `Est`.
- `LegadoId`, `CreatedAt`, `UpdatedAt`.

O modelo não possui `PublicId`, `Ativo` ou `DeletedAt`. Nome é anulável. Não há regras funcionais suficientes nos documentos para transformar todos os campos legados em campos editáveis obrigatórios. Especialmente número de arquivo, layout e campos `Est` precisam de semântica confirmada antes de definir o formulário.

Existem referências internas ao convênio em `ContaCobranca`, `Titulo`, `EstipulanteFaturamentoConfig` e campos de Proposta. `ContaCobranca` representa uma conta/agrupamento de cobrança ligada a pessoa, cliente e vínculo; não substitui o cadastro de Convênio.

O EF de Financeiro declara relações de várias contas, títulos e configurações para um convênio. Essas declarações não comprovam FKs físicas na base consultada. `BancoId` possui índice no mapeamento de Convênio, mas não há relacionamento EF correspondente nessa configuração.

Não foram encontrados CRUD/controller/casos de uso/frontend de Convênio de Cobrança, constantes específicas de permissão, registro do `FinanceiroDbContext` no host ou migrations próprias desse contexto no repositório examinado. A referência ao projeto no host não significa que seu fluxo funcional está registrado.

### Banco consultado

**Não existem `financeiro.convenio_cobranca` nem `financeiro.conta_cobranca` em `webapolice_teste`.** A consulta a `information_schema.tables` não retornou tabelas do schema Financeiro.

`seguro.apolice_subgrupo` existe com `public_id`, `apolice_id`, nome, observação, status e timestamps. Não possui coluna de Convênio. Há **1 Subgrupo** nessa base.

Consequência: não basta adicionar endpoints sobre o modelo existente. É necessário estabelecer a criação física do cadastro e a origem de suas migrations, sem gerar acidentalmente todas as tabelas financeiras modeladas pelo contexto.

### Proposta, com compartilhamento confirmado

1. Preservar a propriedade do cadastro no módulo técnico Financeiro e a tabela `financeiro.convenio_cobranca`.
2. Definir os campos funcionais da primeira versão, aproveitando o modelo existente conforme necessidade comprovada.
3. Acrescentar UUID público único, status ativo e timestamps coerentes com o padrão atual. Definir explicitamente necessidade de exclusão lógica; inativação, por si só, não exige `DeletedAt`.
4. Implementar listar/consultar/criar/alterar/inativar/reativar. Nome obrigatório na API e domínio; restrição física deve acompanhar a análise dos dados de cada ambiente.
5. Acrescentar `convenio_cobranca_id` a `seguro.apolice_subgrupo`, com FK e índice. API recebe `convenioCobrancaPublicId`; o ID numérico permanece interno.
6. Criar contrato público de consulta/validação do convênio no Financeiro, usado pelo Seguro. Não importar entidades, DbContext ou Infrastructure de Financeiro no Seguro.
7. Validar a propriedade do Subgrupo pela Apólice e a disponibilidade do Convênio. Política de inativação deve preservar referências existentes; efeito sobre novos vínculos precisa ser definido.
8. Coordenar alterações e auditoria atomicamente, conforme documentação.

O compartilhamento foi confirmado: a relação será **Convênio 1:N Subgrupos**. Cada Subgrupo terá uma FK; não haverá índice único sobre essa FK. A obrigatoriedade final foi informada pelo negócio, mas o preenchimento do Subgrupo existente e a transição física ainda precisam ser definidos.

Como já existe um Subgrupo sem convênio, não adicionar `NOT NULL` sem estratégia aprovada de preenchimento. Proposta: adicionar FK anulável, exigir convênio nos novos cadastros após ativar o fluxo, corrigir os existentes com uma escolha real e só depois avaliar a obrigatoriedade física. Não inventar convênio padrão para preencher dados.

## Coberturas, Plano e Produto

`seguro.cobertura`, `seguro.plano` e `seguro.produto` existem no modelo e no banco. As três tabelas estão **sem registros** na base consultada.

Cobertura possui nome, nome reduzido, `basica` (texto, não booleano), reajuste, identificadores legados, ativo e timestamps. Não possui UUID público nem vínculo ao Plano. Plano e Produto também não possuem UUID público nesses modelos/banco.

Não foram encontrados CRUDs completos de Produto, Plano ou Cobertura no backend/frontend examinado. Assim, o cadastro de Plano necessário ao fluxo não deve ser considerado pronto.

A relação física global atual é **`seguro.produto.plano_id` → `seguro.plano.id`**: um Produto referencia opcionalmente um Plano; um Plano pode ser referenciado por vários Produtos. Ela diverge da hierarquia conceitual Produto → Planos descrita no resumo e deve ser discutida antes de qualquer inversão.

Não existe `seguro.plano_cobertura` nem FK de Plano em `seguro.cobertura` na base consultada.

### Universo Permitido já parcialmente existente

Existem `seguro.apolice_produto`, `seguro.apolice_plano` e `seguro.apolice_cobertura`, migration `AdicionarUniversoPermitidoApolice`, consulta e aba de frontend. `apolice_plano` e `apolice_cobertura` estão sem registros.

`apolice_cobertura` liga uma cobertura global a um Plano contextual da Apólice e contém overrides de importância segurada/prêmio. Não é o catálogo de coberturas do Plano global.

A consulta e os tipos do frontend ainda expõem IDs internos no Universo Permitido. Isso diverge do padrão público UUID consolidado no resumo e não deve ser reproduzido nos novos contratos.

### Proposta, com compartilhamento confirmado

- Acrescentar UUIDs públicos aos cadastros que serão expostos e implementar o cadastro base de Cobertura no Seguro.
- O responsável confirmou que a mesma Cobertura pode estar em vários Planos: criar `seguro.plano_cobertura` com FKs, unicidade do par e status. A associação física será N:N entre os cadastros globais, mantendo várias Coberturas por Plano.
- Preservar `seguro.apolice_cobertura` para a seleção contratual; não misturar overrides contratuais com o cadastro base.
- Resolver o fluxo de cadastro/seleção de Plano e a divergência Produto–Plano antes de ampliar a hierarquia.

## Divergências e lacunas transversais

| Tema | Evidência atual | Tratamento proposto |
|---|---|---|
| Subgrupo global | Docs 16/17/18 ainda descrevem `cadastro.subgrupo`; tabela existe, vazia | Registrar a decisão contextual e atualizar os documentos afetados. Não remover a tabela legada automaticamente |
| IDs públicos | ADR-007 ainda prevê IDs numéricos nas APIs; resumo e CRUDs recentes utilizam UUID | Atualizar a decisão documental antes dos novos contratos; manter `long` nas PKs internas |
| Financeiro | Modelos existentes, tabelas ausentes, sem migrations próprias encontradas | Definir baseline e migration revisada, com escopo limitado ao cadastro necessário |
| Produto–Plano | FK atual no Produto aponta para Plano | Confirmar a hierarquia pretendida; não inverter silenciosamente |
| Universo Permitido | Já possui persistência e leitura; IDs internos na resposta | Aproveitar o que existe, separando cadastro base de seleção contratual |
| Permissões de Subgrupo | Constantes e migration no código, mas zero `apolices.subgrupos.*` na base | Investigar a carga aplicada e propor correção de catálogo com migration separada |
| Histórico de Subgrupo | `CriarTabelaApoliceSubgrupo` e `AjustesFinaisApoliceSubgrupo` criam a mesma tabela; apenas a segunda consta no histórico consultado | Verificar descoberta/atributos das migrations antes de novos scripts; não executar ambas indiscriminadamente |
| Auditoria de Subgrupo | Handlers examinados salvam diretamente sem registrador de auditoria | Não repetir a lacuna no novo vínculo; definir atomicidade conforme documentação |

O histórico do Seguro contém 14 migrations aplicadas, incluindo as remoções de `ApoliceSubestipulanteModulo` e do vínculo Vida–Subestipulante. Os metadados de `apolice_vida` confirmam `apolice_subgrupo_id` e `apolice_modulo_id`, sem os dois campos antigos. Existem **2 Vidas**.

## Segurança proposta

O catálogo consultado possui módulos `CADASTRO`, `APOLICES` e outros, mas não `FINANCEIRO`. Também não possui recursos de Convênios/Coberturas/Planos nem permissões específicas desses cadastros.

Proposta inicial para revisão:

| Funcionalidade | Local no catálogo proposto | Permissões propostas | Perfil inicial | Auditoria |
|---|---|---|---|---|
| Convênios de Cobrança | Recurso `CONVENIOS_COBRANCA` de `CADASTRO`, conforme escolha do responsável | `convenios_cobranca.visualizar/inserir/alterar/inativar/reativar` | Nenhuma concessão nova a perfis sem aprovação | Mutações |
| Escolha de Convênio no Subgrupo | Recurso `APOLICES` | Manter as permissões de inserir/alterar Subgrupo, ou separar vínculo se o responsável exigir | Sem concessões automáticas | Alteração do vínculo |
| Cadastro base de Cobertura | Recurso `COBERTURAS` de `CADASTRO`, sujeito à revisão | `coberturas.visualizar/inserir/alterar/inativar/reativar` | Sem concessões automáticas | Mutações |
| Coberturas de um Plano | A definir com o cadastro de Plano | Permissões próprias para visualizar e gerenciar o vínculo | Sem concessões automáticas | Mutações |

Localização no catálogo de segurança não muda o proprietário técnico das tabelas. UUIDs de carga de catálogo devem ser fixos, conforme o guia vigente. Leituras de lista/detalhes podem compartilhar `visualizar`. Manter a verificação central de módulo/recurso habilitado, inclusive para Administrador.

## Sequência de implementação proposta

1. Com compartilhamento e módulo de segurança confirmados, definir tratamento do Subgrupo existente e campos da primeira versão. O código técnico do recurso e a matriz de permissões ainda são propostas.
2. Atualizar documentação de hierarquia, identificadores e novas regras. Aprovar matriz de segurança e concessões a perfis.
3. Resolver baseline do Financeiro e avaliar diferenças dos ambientes antes de aplicar scripts. Revisar migrations geradas para impedir criação incidental de todo o modelo financeiro.
4. Implementar persistência, contratos públicos, casos de uso, auditoria e API do cadastro de Convênio.
5. Implementar migrations de negócio e segurança separadas, com ordem de aplicação Financeiro → vínculo no Seguro e rollback em ordem inversa.
6. Implementar frontend CRUD e seleção do Convênio na criação/edição/detalhes de Subgrupo, com componentes/tokens existentes e controle de permissões.
7. Validar UUIDs públicos, dados inválidos, autorização (401/403), módulo/recurso desabilitado, inativação/reativação, isolamento pela Apólice e rollback de auditoria. Usar banco de testes separado e controlar dados de teste.
8. Homologar Convênios e o vínculo ao Subgrupo.
9. Com compartilhamento de Cobertura confirmado, resolver relação Produto–Plano e escopo do CRUD de Plano. Implementar cadastro base/vínculo sem alterar silenciosamente o Universo Permitido existente.

Na conclusão da auditoria inicial, antes da autorização posterior para implementar, permaneciam pendentes os campos funcionais, o tratamento do Subgrupo existente, a relação Produto–Plano e a aprovação da matriz de segurança. As cardinalidades e a localização do cadastro de Convênios registradas acima estão confirmadas. Este documento não autoriza aplicação de migrations.
