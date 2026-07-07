# Modelagem do Banco de Dados WebApolice

## Objetivo do documento

Este documento serve como referência arquitetural geral da modelagem do banco de dados do projeto webapolice. Ele será usado como guia oficial para:
* novas features;
* endpoints;
* Entity Framework Core;
* DTOs;
* migrations;
* consultas;
* relatórios;
* migração de dados;
* análise de impacto;
* decisões futuras do produto.

O dump PostgreSQL atual é a fonte oficial da nova modelagem. O banco SQL Server (AmauriBueno) atua como referência legada. Novas regras de negócio não devem depender diretamente de IDs legados; os campos `legado_id` e as tabelas `*_migration_map` existem puramente para rastreabilidade, migração e conciliação.

## Banco e extensões

**Banco de Dados:** PostgreSQL 16.3

Extensões habilitadas no banco de dados:
* `pgcrypto`: utilizado para geração de UUIDs (via função `gen_random_uuid()`).
* `unaccent`: utilizado para busca textual ignorando acentuação.
* `pg_trgm`: utilizado para busca aproximada e otimização de pesquisa textual (índices trigram).

## Schemas do banco

Os seguintes schemas organizam os domínios do banco de dados:
* `core`
* `cadastro`
* `convenio`
* `financeiro`
* `seguro`
* `comissao`
* `integracao`
* `legado`
* `sinistro`
* `documento`
* `atendimento`

**Regra Absoluta:** Não criar tabelas operacionais no schema `public`.

## Responsabilidade por schema

Cada schema concentra uma área específica do domínio de negócios.

### `core`
Base comum do sistema, abrigando entidades fundamentais.
* `pessoa`
* `pessoa_documento`
* `pessoa_contato`
* `pessoa_endereco`
* `estado`
* `cidade`
* `banco`

### `cadastro`
Cadastros principais do negócio e estrutura organizacional de clientes e entidades parceiras.
* `cliente`
* `cliente_status`
* `cliente_vinculo`
* `cliente_dependente`
* `estipulante`
* `estipulante_configuracao`
* `subestipulante`
* `seguradora`
* `corretora`
* `agenciador`
* `grupo`
* `subgrupo`
* `lotacao`

### `seguro`
Entidades focadas nos produtos de seguro, propostas e histórico de coberturas/movimentos.
* `proposta`
* `proposta_status`
* `proposta_item`
* `proposta_cobertura`
* `proposta_beneficiario`
* `proposta_movimento`
* `proposta_historico`
* `tipo_produto`
* `produto`
* `plano`
* `tabela_preco`
* `cobertura`
* `movimento_tipo`

### `financeiro`
Estruturas de cobrança, faturamento, pagamentos e integrações de retorno bancário.
* `titulo`
* `titulo_status`
* `titulo_pagamento`
* `titulo_retorno_bancario`
* `retorno_bancario_codigo`
* `conta_cobranca`
* `convenio_cobranca`
* `estipulante_faturamento_config`
* `forma_pagamento_estipulante`
* `forma_retorno`
* `forma_retorno_estipulante`
* `regra_agrupamento_fatura`
* `cobranca_acompanhamento`
* `movimento_cobranca_log`
* `identificador_remessa_api`

### `comissao`
Gestão de faturas de comissão, agenciamentos e repasses financeiros para corretoras e agenciadores.
* `estipulante_comissao_config`
* `agenciador_comissao_config`
* `corretora_agenciador`
* `proposta_participante`
* `lancamento_comissao`
* `agenciamento_corretora_lancamento`
* `fatura_integracao`
* `fatura_vida_agenciamento`
* `fatura_vida_recebimento`
* `fatura_comissao_resumo`
* `lancamento_fatura_estipulante`

### `convenio`
Dados específicos e parametrizações exclusivas de determinados convênios (ex: SIAPE, Corsan).
* `siape_cliente`
* `siape_orgao`
* `siape_parametro`
* `corsan_cliente`
* `corsan_proposta`

### `sinistro`
Acompanhamento, cobertura e gestão de sinistros de clientes/propostas.
* `sinistro`
* `sinistro_status`
* `sinistro_beneficiario`
* `sinistro_cobertura`
* `acompanhamento`

### `documento`
Gestão de arquivos anexos, metadados e provedores de storage (S3, locais).
* `arquivo`
* `arquivo_vinculo`
* `arquivo_versao`
* `arquivo_acesso_log`
* `storage_provider`
* `tipo_anexo`

### `atendimento`
Protocolos de chamados, relatórios com seguradoras e registros de atendimento ao cliente.
* `protocolo_lote`
* `protocolo_item`
* `protocolo_acompanhamento`
* `protocolo_relatorio_seguradora`
* `protocolo_relatorio_seguradora_item`

### `integracao`
Mapeamento de referências externas para viabilizar comunicações via APIs de terceiros.
* `referencia_externa`

### `legado`
Controle e suporte à migração de dados do antigo sistema AmauriBueno (SQL Server) para as novas tabelas, garantindo que o acoplamento seja mantido apenas nos mapeamentos e não no negócio.
* `cliente_migration_map`
* `proposta_migration_map`
* `proposta_item_migration_map`
* `proposta_cobertura_migration_map`
* `proposta_beneficiario_migration_map`
* `movimento_proposta_migration_map`
* `estipulante_migration_map`
* `corretora_migration_map`
* `agenciador_migration_map`
* `sinistro_migration_map`
* `documento_anexo_migration_map`
* `protocolo_lote_migration_map`
* `protocolo_item_migration_map`

## Entidades centrais

Algumas tabelas são a âncora do sistema para os demais relacionamentos:

* `core.pessoa`: Representa fisicamente o indivíduo/empresa, mantendo informações centrais e dados pessoais sensíveis. Contém `public_id` e `deleted_at` para exposição via API e exclusão lógica.
* `cadastro.cliente`: Especializa uma "Pessoa" em um "Cliente" ativo do sistema. Possui `public_id`, gerencia o status e usa `deleted_at`. O cuidado ao expor é manter apenas as referências necessárias e não trafegar dados pessoais irrestritos.
* `cadastro.cliente_vinculo`: Define a ligação/matrícula do cliente com sua estipulante, subestipulante, etc. Contém informações sensíveis como contas bancárias atreladas àquele vínculo específico.
* `seguro.proposta`: Reúne todos os seguros e apólices. Relaciona-se com diversos outros módulos (finanças, sinistro e documentos). Usar `public_id` para expor propostas na API.
* `financeiro.titulo`: Responsável pela rastreabilidade e histórico de cobranças. Contém chaves e status cruciais para faturamento. Exige alto cuidado e permissionamento adequado no acesso.
* `sinistro.sinistro`: Armazena a ocorrência de sinistros. Possui alto teor de dados sensíveis e requer rastreabilidade de acessos. Contém `public_id` e relaciona-se fortemente com documentos.
* `documento.arquivo`: Repositório de arquivos binários e metadados. Exposição requer controle rigoroso, pois os anexos normalmente contém informações de identidade e contratos. Possui `public_id`.

## Convenções de modelagem

* Os **schemas** são nomeados em português, separados por domínios.
* As **tabelas** são nomeadas no formato `snake_case`.
* As **colunas** são nomeadas no formato `snake_case`.
* A chave primária `id bigint` (ou `integer`) atua como identificador interno predominante para relacionamentos.
* A chave `id smallint` é usada para tabelas de domínio menores (ex: status).
* A coluna `public_id uuid` está presente nas entidades principais para a exposição externa na API e roteamento, substituindo a exibição do ID real sequencial.
* Campos de auditoria `created_at`, `updated_at`, e de exclusão lógica `deleted_at` estão presentes nas principais entidades operacionais.
* O campo `legado_id` é mantido apenas como rastreabilidade originária, enquanto as tabelas `*_migration_map` contêm a complexidade maior da migração.

## Regras para APIs

As APIs do projeto webapolice devem observar as seguintes regras permanentes:
* Não expor IDs internos (inteiros) quando a entidade possuir um `public_id`.
* Não expor CPF ou CNPJ completo em rotas de listagens irrestritas, realizando a devida máscara.
* Não retornar campos técnicos, como os terminados em `*_limpo` (ex: `documento_limpo`), ao frontend sem uma justificativa explícita no caso de uso.
* Não retornar dados das tabelas `*_migration_map` ou campos como `legado_id` em APIs operacionais padrão; eles servem apenas a processos de conciliação interna ou APIs técnicas de migração.
* Expor o mínimo necessário de campos técnicos, como `deleted_at`.
* Consultas operacionais devem sempre respeitar a exclusão lógica, garantindo que contenham a cláusula correspondente (no EF Core, `deleted_at IS NULL` é tratado de forma automática via global query filter, desde que corretamente configurado).
* Os DTOs da API devem ser estritamente orientados ao **caso de uso** ou à interface de usuário requisitada, evitando refletir cegamente o formato das tabelas.

## Regras para backend

No Entity Framework Core e C#:
* Deve-se mapear o schema de forma explícita na configuração das entidades.
* Não confiar ou utilizar o schema `public` por padrão.
* As colunas do banco estão em `snake_case`, as entidades em .NET seguirão `PascalCase`, utilizando convenções de naming para mapear automaticamente sem necessidade de `[Column]` em todas as propriedades.
* As lógicas de domínio não devem se acoplar ou possuir condições com base em propriedades legadas ou migrações (evitar acoplamento de regra nova com o legado).
* Quaisquer alterações estruturais do banco devem usar *migrations* versionadas da ferramenta EF Core CLI.
* Revisar profundamente o impacto no ecossistema de banco antes de adicionar/alterar migrations (por exemplo, uso de drop, locks em tabelas pesadas).
* Na modelagem .NET, separar conceitualmente entidades de domínio, entidades de configuração, tabelas de apoio, tabelas de migração e modelos exclusivos de leitura (quando houver CQRS).

## Regras para frontend

O frontend React não lida com a modelagem do banco. As seguintes diretrizes se aplicam:
* O frontend não deve ter conhecimento de como o banco de dados está fisicamente estruturado.
* O frontend consome apenas os DTOs modelados pela API.
* O backend tem a responsabilidade de montar dados complexos antes de enviar ao frontend.
* O frontend não deve depender ou utilizar campos como `status_id`, `legado_id`, `*_limpo` ou de tabelas físicas.
* As telas de listagem devem sempre utilizar os dados já tratados, formatados e mascarados pelo backend.
* Formulários devem seguir os contratos do caso de uso de escrita do backend.

**Exemplo Ruim:** O frontend recebe a entidade direta do banco e precisa resolver lógicas internas.
```json
{
  "documento_principal_limpo": "00000000000",
  "status_id": 1,
  "criterio_criacao": 5
}
```

**Exemplo Bom:** O frontend recebe o DTO orientado à interface e experiência do usuário.
```ts
{
  publicId: "uuid-aqui",
  nome: "João Silva",
  documentoMascarado: "***.000.000-**",
  status: "ativo",
  matricula: "2024A-1234"
}
```

## Proteção de dados pessoais

O banco de dados armazena informações sensíveis e PIIs (Personally Identifiable Information), incluindo:
* CPF e CNPJ
* RG
* Data de nascimento
* Endereço físico
* Telefone e E-mail
* Dados bancários
* Documentos anexados
* Dados de sinistro e ocorrências
* Informações sobre beneficiários e dependentes

**Regras estritas de segurança de dados:**
* Não logar (logs de aplicação) dados pessoais completos (ex: requests/responses de erro com o CPF legível).
* Mascarar sempre os documentos primários (CPF/RG) em listagens e *grids*.
* Não utilizar CPF ou CNPJ como chaves identificadoras em rotas de URL da aplicação.
* Não utilizar CPF/CNPJ como chave de iteração (*key*) nos componentes React.
* Não retornar os campos "limpos" (somente números) em requisições GET sem necessidade rigorosa, pois eles facilitam extração não autorizada.
* Limitar e auditar estritamente o acesso a arquivos de documentos e registros de sinistros de clientes, baseados nas roles do sistema.
* Mensagens de erro disparadas pela API nunca devem devolver PIIs completos ou trechos com dados reais.

## Performance

Diretrizes de desempenho a considerar ao modelar consultas ou novas regras no sistema:
* Uso obrigatório de paginação nas listagens e pesquisas gerais de tabelas transacionais no backend.
* Ordenações devem ocorrer no banco de dados e nunca em memória na aplicação (após materializar listas inteiras).
* Realizar os filtros de dados exclusivamente do lado do servidor (no *Query Provider* do EF).
* Utilizar preferencialmente os campos *normalizados* para efetuar as buscas e agrupamentos (ex: busca em textos e nomes).
* Avaliar criteriosamente os índices do banco antes de construir relatórios pesados ou telas analíticas.
* Não criar índices excessivos sem uma justificativa baseada no padrão de leitura predominante vs impacto na escrita.
* Prestar atenção extra ao indexar ou varrer colunas com altíssima ou baixíssima cardinalidade e uso misto (como `public_id`, `legado_id`, `status_id`, `deleted_at`, campos "limpos", `competencia_int`).

## Sequência recomendada de evolução

A adoção e integração do banco de dados na aplicação seguirá uma ordem de prioridade para a evolução do sistema baseada no grau de dependência dos módulos:

1. Detalhes do Cliente
2. Cadastro de Cliente
3. Edição de Cliente
4. Vínculos do Cliente
5. Dependentes
6. Propostas do Cliente
7. Detalhes da Proposta
8. Títulos Financeiros
9. Documentos
10. Sinistros
11. Atendimento / Protocolos
12. Comissões
13. Relatórios e integrações

## Diretriz final

> Banco orienta o domínio,
> backend traduz domínio em casos de uso,
> frontend consome DTOs próprios da experiência do usuário.
