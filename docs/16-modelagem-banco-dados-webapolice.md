# 17 — Modelagem do Banco de Dados do webapolice

## 1. Objetivo deste documento

Este documento registra a estrutura base do banco de dados PostgreSQL do projeto **webapolice**.

Ele deve ser utilizado como referência obrigatória para:

- novas implementações de backend;
- criação de endpoints;
- evolução dos módulos funcionais;
- mapeamentos Entity Framework Core;
- definição de DTOs;
- consultas e relatórios;
- migração de dados legados;
- integração com frontend;
- análise de impacto antes de alterar tabelas;
- padronização de nomenclatura e organização por domínio.

A estrutura descrita aqui passa a ser a base de trabalho do sistema daqui em diante.

---

## 2. Tecnologia e extensões

Banco de dados:

```text
PostgreSQL 16.3
```

Extensões utilizadas:

- pgcrypto
- unaccent
- pg_trgm

Uso esperado das extensões:

**pgcrypto**
Utilizada principalmente para geração de UUIDs com:
`gen_random_uuid()`
Aplicável em colunas públicas como:
`public_id`

**unaccent**
Deve ser utilizada em buscas textuais onde acentos não devem afetar o resultado.
Exemplo de uso futuro:
João e Joao devem poder ser localizados pela mesma busca, quando aplicável.

**pg_trgm**
Deve ser utilizada para melhorar buscas textuais aproximadas, especialmente em campos como:
- nome;
- nome_normalizado;
- documento;
- matrícula;
- código;
- identificadores externos.

## 3. Organização geral por schemas

O banco foi separado por domínio funcional através de schemas PostgreSQL.

Schemas identificados:

- core
- cadastro
- convenio
- financeiro
- seguro
- comissao
- integracao
- legado
- sinistro
- documento
- atendimento

A separação por schema é uma decisão arquitetural importante e deve ser preservada.

Não criar novas tabelas no schema public.

## 4. Responsabilidade de cada schema

### 4.1 core
Contém entidades centrais e reutilizáveis do sistema.
Principais tabelas:
- core.pessoa
- core.pessoa_documento
- core.pessoa_contato
- core.pessoa_endereco
- core.estado
- core.cidade
- core.banco

Responsabilidade:
- cadastro central de pessoas físicas e jurídicas;
- documentos;
- contatos;
- endereços;
- localização;
- bancos;
- dados reutilizados por vários módulos.

A entidade `core.pessoa` é a base para clientes, estipulantes, seguradoras, corretoras, agenciadores e demais entidades que possam representar uma pessoa física ou jurídica.

### 4.2 cadastro
Contém entidades cadastrais principais do negócio.
Principais tabelas:
- cadastro.cliente
- cadastro.cliente_status
- cadastro.cliente_vinculo
- cadastro.cliente_dependente
- cadastro.estipulante
- cadastro.estipulante_configuracao
- cadastro.subestipulante
- cadastro.seguradora
- cadastro.corretora
- cadastro.agenciador
- cadastro.grupo
- cadastro.subgrupo
- cadastro.lotacao

Responsabilidade:
- clientes;
- vínculos do cliente;
- dependentes;
- estipulantes;
- subestipulantes;
- seguradoras;
- corretoras;
- agenciadores;
- agrupamentos cadastrais;
- configurações comerciais e operacionais de entidades cadastrais.

### 4.3 seguro
Contém o núcleo operacional de seguros.
Principais tabelas:
- seguro.proposta
- seguro.proposta_status
- seguro.proposta_item
- seguro.proposta_cobertura
- seguro.proposta_beneficiario
- seguro.proposta_movimento
- seguro.proposta_historico
- seguro.tipo_produto
- seguro.produto
- seguro.plano
- seguro.tabela_preco
- seguro.cobertura
- seguro.movimento_tipo

Responsabilidade:
- propostas;
- vigência;
- movimentações;
- itens da proposta;
- produtos;
- planos;
- coberturas;
- beneficiários;
- histórico de alterações;
- status da proposta.

A tabela `seguro.proposta` é uma das entidades centrais do sistema.

### 4.4 financeiro
Contém cobrança, títulos, pagamentos, retornos bancários e configurações de faturamento.
Principais tabelas:
- financeiro.titulo
- financeiro.titulo_status
- financeiro.titulo_pagamento
- financeiro.titulo_retorno_bancario
- financeiro.retorno_bancario_codigo
- financeiro.conta_cobranca
- financeiro.convenio_cobranca
- financeiro.estipulante_faturamento_config
- financeiro.forma_pagamento_estipulante
- financeiro.forma_retorno
- financeiro.forma_retorno_estipulante
- financeiro.regra_agrupamento_fatura
- financeiro.cobranca_acompanhamento
- financeiro.movimento_cobranca_log
- financeiro.identificador_remessa_api

Responsabilidade:
- contas de cobrança;
- convênios de cobrança;
- títulos;
- pagamentos;
- retorno bancário;
- logs de cobrança;
- regras de agrupamento;
- configurações de faturamento por estipulante.

A tabela `financeiro.titulo` deve ser tratada como a entidade principal de cobrança.

### 4.5 comissao
Contém configuração, geração e acompanhamento de comissões e agenciamentos.
Principais tabelas:
- comissao.estipulante_comissao_config
- comissao.agenciador_comissao_config
- comissao.corretora_agenciador
- comissao.proposta_participante
- comissao.lancamento_comissao
- comissao.agenciamento_corretora_lancamento
- comissao.fatura_integracao
- comissao.fatura_vida_agenciamento
- comissao.fatura_vida_recebimento
- comissao.fatura_comissao_resumo
- comissao.lancamento_fatura_estipulante

Responsabilidade:
- configurações de comissão;
- participantes da proposta;
- agenciadores;
- corretoras;
- lançamentos de comissão;
- faturas de comissão;
- integração com faturamento legado.

### 4.6 convenio
Contém dados específicos de convênios e integrações operacionais por origem.
Principais tabelas:
- convenio.siape_cliente
- convenio.siape_orgao
- convenio.siape_parametro
- convenio.corsan_cliente
- convenio.corsan_proposta

Responsabilidade:
- dados específicos de convênios;
- SIAPE;
- CORSAN;
- parâmetros operacionais por convênio;
- vínculo entre cliente/proposta e dados externos de convênio.

Essas tabelas não devem substituir as entidades principais de cliente e proposta. Elas complementam os dados conforme o convênio.

### 4.7 sinistro
Contém gestão de sinistros.
Principais tabelas:
- sinistro.sinistro
- sinistro.sinistro_status
- sinistro.sinistro_beneficiario
- sinistro.sinistro_cobertura
- sinistro.acompanhamento

Responsabilidade:
- registro de sinistros;
- status;
- beneficiários;
- coberturas;
- acompanhamento do processo.

A tabela `sinistro.sinistro` se relaciona ao contexto de proposta, cliente, pessoa e estipulante.

### 4.8 documento
Contém controle de arquivos, anexos, versões e acessos.
Principais tabelas:
- documento.arquivo
- documento.arquivo_vinculo
- documento.arquivo_versao
- documento.arquivo_acesso_log
- documento.storage_provider
- documento.tipo_anexo

Responsabilidade:
- metadados de arquivos;
- storage local, S3, MinIO, Azure Blob ou Supabase Storage;
- vínculos de documentos com entidades do sistema;
- versões de arquivos;
- logs de acesso;
- classificação de tipos de anexos.

Arquivos físicos não devem ser armazenados diretamente no banco. O banco armazena metadados, vínculos e chaves de storage.

### 4.9 atendimento
Contém protocolos e acompanhamentos de atendimento.
Principais tabelas:
- atendimento.protocolo_lote
- atendimento.protocolo_item
- atendimento.protocolo_acompanhamento
- atendimento.protocolo_relatorio_seguradora
- atendimento.protocolo_relatorio_seguradora_item

Responsabilidade:
- protocolos;
- itens de protocolo;
- acompanhamentos;
- relatórios enviados ou recebidos de seguradoras;
- vínculo entre protocolo, cliente, estipulante e dados legados.

### 4.10 integracao
Contém referências externas genéricas.
Principal tabela:
- integracao.referencia_externa

Responsabilidade:
- mapear entidades internas com sistemas externos;
- armazenar chaves externas;
- manter dados auxiliares em JSONB quando necessário.

Essa tabela deve ser usada com cuidado para integrações genéricas, sem substituir relacionamentos estruturados quando existir domínio claro.

### 4.11 legado
Contém tabelas de mapeamento entre dados migrados do sistema legado e as novas entidades.
Principais tabelas:
- legado.cliente_migration_map
- legado.proposta_migration_map
- legado.proposta_item_migration_map
- legado.proposta_cobertura_migration_map
- legado.proposta_beneficiario_migration_map
- legado.movimento_proposta_migration_map
- legado.estipulante_migration_map
- legado.corretora_migration_map
- legado.agenciador_migration_map
- legado.sinistro_migration_map
- legado.documento_anexo_migration_map
- legado.protocolo_lote_migration_map
- legado.protocolo_item_migration_map

Responsabilidade:
- rastreabilidade da migração;
- vínculo entre IDs legados e IDs novos;
- registro de critérios de migração;
- auditoria técnica da origem dos dados;
- suporte a reconciliação e correção de dados.

O schema legado não deve ser usado para novas funcionalidades operacionais. Ele serve para rastreabilidade, migração e suporte.

## 5. Entidades centrais do sistema

### 5.1 Pessoa
Tabela principal: `core.pessoa`

Campos relevantes:
- id
- public_id
- tipo_pessoa
- nome
- nome_normalizado
- documento_principal
- documento_principal_limpo
- documento_valido
- data_nascimento
- sexo
- observacao
- created_at
- updated_at
- deleted_at

Regras:
- `id` é identificador interno.
- `public_id` deve ser preferido para exposição externa quando possível.
- `documento_principal` pode conter máscara.
- `documento_principal_limpo` deve ser usado para comparação, busca e validação.
- Dados sensíveis não devem ser expostos sem necessidade.
- Soft delete é indicado por `deleted_at`.

### 5.2 Cliente
Tabela principal: `cadastro.cliente`

Relacionamentos centrais:
- `cadastro.cliente.pessoa_id` → `core.pessoa.id`
- `cadastro.cliente.status_id` → `cadastro.cliente_status.id`

Campos relevantes:
- id
- public_id
- pessoa_id
- status_id
- falecido
- data_obito
- observacao
- data_cadastro_legado
- legado_id
- created_at
- updated_at
- deleted_at

Regras:
- Cliente não deve duplicar dados pessoais principais.
- Dados pessoais devem vir preferencialmente de `core.pessoa`.
- Status deve ser resolvido via `cadastro.cliente_status`.
- A listagem de clientes deve mascarar documentos sensíveis.
- `deleted_at` deve ser respeitado em consultas operacionais.

### 5.3 Vínculo do Cliente
Tabela principal: `cadastro.cliente_vinculo`

Responsabilidade:
- vincular cliente a estipulante, subestipulante, grupo, subgrupo e lotação;
- armazenar matrícula e dados bancários associados ao vínculo;
- controlar se o vínculo está ativo.

Campos relevantes:
- cliente_id
- pessoa_id
- estipulante_id
- subestipulante_id
- grupo_id
- subgrupo_id
- lotacao_id
- matricula
- matricula_normalizada
- banco_id
- agencia
- conta_corrente
- criterio_criacao
- ativo

Regras:
- Um cliente pode possuir mais de um vínculo.
- A matrícula pertence ao vínculo, não necessariamente à pessoa.
- Filtros por estipulante, grupo, subgrupo ou matrícula devem considerar esta tabela.

### 5.4 Proposta
Tabela principal: `seguro.proposta`

Relacionamentos principais:
- pessoa_id
- cliente_id
- cliente_vinculo_id
- estipulante_id
- subestipulante_id
- seguradora_id
- corretora_id
- convenio_cobranca_id
- conta_cobranca_id
- status_id
- movimento_tipo_id

Campos relevantes:
- public_id
- numero
- data_inclusao
- data_movimento
- data_primeiro_vencimento
- data_proximo_vencimento
- premio_liquido
- valor_parcela
- vigente
- visivel_operacional
- versao
- proposta_origem_id
- legado_id
- created_at
- updated_at
- deleted_at

Regras:
- Proposta é a entidade central do módulo de seguros.
- A proposta sempre deve estar vinculada a cliente, pessoa, vínculo e estipulante.
- `vigente` indica a situação operacional atual.
- `visivel_operacional` deve ser respeitado em telas operacionais.
- Histórico e versionamento devem ser tratados com cuidado.

### 5.5 Título financeiro
Tabela principal: `financeiro.titulo`

Responsabilidade:
- representar cobrança;
- controlar vencimento;
- controlar pagamento;
- associar cobrança à proposta, movimento, cliente e conta de cobrança.

Campos relevantes:
- proposta_movimento_id
- proposta_id
- pessoa_id
- cliente_id
- cliente_vinculo_id
- estipulante_id
- convenio_cobranca_id
- conta_cobranca_id
- status_id
- competencia_ano
- competencia_mes
- competencia_int
- data_vencimento
- data_pagamento
- valor_original
- valor_atual
- valor_pago
- premio_total
- deleted_at

Regras:
- Não calcular inadimplência somente no frontend.
- Status deve ser resolvido por `financeiro.titulo_status`.
- Competência deve usar preferencialmente `competencia_int` para filtros e ordenação.
- Pagamentos devem ser consultados em `financeiro.titulo_pagamento`.

### 5.6 Sinistro
Tabela principal: `sinistro.sinistro`

Responsabilidade:
- registrar ocorrência;
- vincular proposta, cliente, pessoa, estipulante e seguradora;
- controlar status e valores indenizatórios.

Campos relevantes:
- public_id
- proposta_id
- pessoa_id
- cliente_id
- cliente_vinculo_id
- estipulante_id
- seguradora_id
- status_id
- numero_sinistro
- data_ocorrencia
- data_aviso
- data_envio_seguradora
- data_encerramento
- valor_avisado
- valor_importancia
- valor_indenizacao
- causa
- observacao
- deleted_at

### 5.7 Documento / Arquivo
Tabela principal: `documento.arquivo`

Responsabilidade:
- armazenar metadados dos arquivos;
- controlar origem;
- controlar provider de storage;
- registrar hash, extensão, MIME type e tamanho;
- controlar status da migração de arquivos.

Tabelas auxiliares:
- documento.arquivo_vinculo
- documento.arquivo_versao
- documento.arquivo_acesso_log
- documento.tipo_anexo
- documento.storage_provider

Regras:
- Nunca expor caminho interno diretamente ao usuário.
- Download deve passar por autorização.
- Acesso a documentos deve ser registrado quando aplicável.
- Arquivos sensíveis devem respeitar regras de permissão.

## 6. Convenções de nomenclatura

### 6.1 Schemas
Usar nomes em português, minúsculos e por domínio:
- cadastro
- financeiro
- seguro
- documento
- sinistro

### 6.2 Tabelas
Usar nomes no singular quando representar entidade principal:
- cliente
- proposta
- titulo
- sinistro
- arquivo

Usar nomes compostos quando representar relação ou especialização:
- cliente_vinculo
- proposta_item
- proposta_cobertura
- titulo_pagamento
- arquivo_vinculo

### 6.3 Colunas
Usar `snake_case`.
Exemplos:
- created_at
- updated_at
- deleted_at
- public_id
- pessoa_id
- cliente_id
- status_id
- legado_id

### 6.4 Chaves primárias
Padrão predominante:
`id bigint`

ou, para tabelas pequenas de domínio/status:
`id smallint`

### 6.5 Identificador público
Quando existir:
`public_id uuid DEFAULT gen_random_uuid()`

Deve ser preferido para exposição externa em APIs e rotas, principalmente em entidades sensíveis.

### 6.6 Campos legados
Campos como:
- legado_id
- legado_cliente_id
- legado_proposta_id
- legado_estipulante_id

devem ser tratados como rastreabilidade da migração.
Não devem ser usados como identificador principal em novas funcionalidades.

## 7. Regras de exposição em APIs

### 7.1 Não expor IDs internos sem necessidade
Evitar expor diretamente:
`id bigint`
quando houver `public_id`.
Preferir:
`publicId`
em DTOs públicos.

### 7.2 Não expor documentos completos em listagens
Campos como:
- documento_principal
- cpf
- cnpj
- cpf_limpo
- cnpj_limpo
- documento_principal_limpo

devem ser mascarados quando exibidos em listagens.
Exemplo:
`***.456.789-**`
ou equivalente conforme regra de negócio.

### 7.3 Não expor campos técnicos
Evitar em respostas comuns:
- deleted_at
- legado_id
- criterio_migracao
- observacao técnica
- *_migration_map

Esses campos devem aparecer apenas em contextos administrativos, auditoria ou suporte técnico.

### 7.4 Soft delete
Toda consulta operacional deve considerar:
`deleted_at IS NULL`
quando a tabela possuir `deleted_at`.

## 8. Regras para backend e Entity Framework Core

### 8.1 Mapeamento por schema
Toda entidade deve mapear explicitamente o schema.
Exemplo conceitual:
`builder.ToTable("cliente", "cadastro");`
Não confiar no schema padrão.

### 8.2 Mapeamento explícito de colunas
Mapear nomes de colunas em `snake_case`.
Exemplo conceitual:
```csharp
builder.Property(x => x.CreatedAt)
    .HasColumnName("created_at");
```

### 8.3 Separar domínio de persistência quando necessário
Nem toda tabela precisa virar uma entidade rica de domínio imediatamente.
Classificação recomendada:
- entidades de domínio: Cliente, Pessoa, Proposta, Título, Sinistro;
- entidades de configuração: Status, Tipo, Forma, Regra;
- entidades de apoio: Cidade, Banco, Estado;
- entidades de migração: tabelas *_migration_map;
- entidades de documento: Arquivo, ArquivoVinculo, TipoAnexo;
- entidades de leitura: projections/views/DTOs para listagens complexas.

### 8.4 Evitar acoplamento direto com legado
Novas regras de negócio não devem depender diretamente de `legado_id`.
O legado deve ser usado para:
- rastreabilidade;
- conciliação;
- importação;
- auditoria de migração;
- suporte.

## 9. Regras para frontend

O frontend não deve refletir diretamente a estrutura física do banco.
A UI deve consumir DTOs preparados pelo backend.

Exemplo ruim:
- core.pessoa.documento_principal_limpo
- cadastro.cliente.status_id
- cadastro.cliente_vinculo.criterio_criacao

Exemplo melhor:
```typescript
{
  publicId: string;
  nome: string;
  documentoMascarado: string;
  status: "ativo" | "inativo";
  matricula?: string;
}
```

A responsabilidade de montar dados complexos deve ficar no backend.

## 10. Módulo Clientes — referência inicial

### 10.1 Tabelas envolvidas
Para listagem, detalhes, cadastro e edição de Clientes, considerar principalmente:
- core.pessoa
- core.pessoa_documento
- core.pessoa_contato
- core.pessoa_endereco
- cadastro.cliente
- cadastro.cliente_status
- cadastro.cliente_vinculo
- cadastro.cliente_dependente
- cadastro.estipulante
- cadastro.subestipulante
- cadastro.grupo
- cadastro.subgrupo
- cadastro.lotacao
- core.banco

### 10.2 Listagem de clientes
A listagem deve buscar dados de:
- cliente;
- pessoa;
- status;
- vínculo principal ou vínculo ativo;
- matrícula;
- documento mascarado;
- contato principal, se necessário.

Não deve carregar:
- todos os dependentes;
- todas as propostas;
- todos os títulos;
- todos os documentos;
- histórico completo.

Esses dados pertencem à página de detalhes.

### 10.3 Detalhes do cliente
A página de detalhes pode ser organizada em seções:
- Dados pessoais
- Documentos
- Contatos
- Endereços
- Vínculos
- Dependentes
- Propostas
- Títulos financeiros
- Documentos anexados
- Atendimentos / protocolos
- Sinistros
- Dados de migração

Nem todas as seções precisam ser implementadas na primeira versão.

### 10.4 Cadastro e edição
Cadastro e edição devem respeitar a separação:
- Pessoa
- Cliente
- Vínculo
- Contatos
- Endereços
- Dependentes

Não misturar tudo em uma única entidade artificial.

## 11. Módulo Propostas — referência inicial

### 11.1 Tabelas envolvidas
- seguro.proposta
- seguro.proposta_status
- seguro.proposta_item
- seguro.proposta_cobertura
- seguro.proposta_beneficiario
- seguro.proposta_movimento
- seguro.proposta_historico
- seguro.tipo_produto
- seguro.produto
- seguro.plano
- seguro.tabela_preco
- seguro.cobertura
- seguro.movimento_tipo
- cadastro.cliente
- cadastro.cliente_vinculo
- cadastro.estipulante
- cadastro.subestipulante
- cadastro.seguradora
- cadastro.corretora
- financeiro.conta_cobranca
- financeiro.convenio_cobranca

### 11.2 Regras iniciais
- Proposta pertence a um cliente e a um vínculo.
- Proposta possui status.
- Proposta pode ter itens, coberturas, beneficiários e movimentos.
- Movimentos podem gerar títulos financeiros e comissões.
- Alterações devem preservar histórico quando necessário.

## 12. Módulo Financeiro — referência inicial

### 12.1 Tabelas envolvidas
- financeiro.titulo
- financeiro.titulo_status
- financeiro.titulo_pagamento
- financeiro.titulo_retorno_bancario
- financeiro.retorno_bancario_codigo
- financeiro.conta_cobranca
- financeiro.convenio_cobranca
- financeiro.cobranca_acompanhamento
- financeiro.movimento_cobranca_log

### 12.2 Regras iniciais
- Títulos devem ser paginados e filtrados no backend.
- Valores financeiros devem usar numeric, nunca float.
- Competência deve ser tratada com `competencia_int`.
- Baixas e retornos bancários devem ser auditáveis.
- Erros de cobrança não devem ser sobrescritos sem histórico.

## 13. Módulo Documentos — referência inicial

### 13.1 Tabelas envolvidas
- documento.arquivo
- documento.arquivo_vinculo
- documento.arquivo_versao
- documento.arquivo_acesso_log
- documento.tipo_anexo
- documento.storage_provider

### 13.2 Regras iniciais
- Arquivo é metadado.
- O conteúdo físico fica em storage externo ou local controlado.
- Vínculos são polimórficos através de:
  - `entidade_tipo`
  - `entidade_id`
- Downloads devem validar permissão.
- Acesso a documentos sensíveis deve ser registrado.
- Versões devem preservar histórico.

## 14. Módulo Legado e Migração
O schema legado é essencial para rastreabilidade, mas não deve virar base da operação diária.

### 14.1 Uso permitido
- localizar origem de um dado;
- validar migração;
- investigar inconsistência;
- reconciliar registros;
- gerar relatórios técnicos de migração.

### 14.2 Uso proibido ou desencorajado
- usar `legado_id` como chave principal em telas novas;
- filtrar regras de negócio novas diretamente por tabelas `*_migration_map`;
- expor tabelas de migração em APIs públicas;
- permitir que frontend dependa de IDs legados.

## 15. Cuidados com dados pessoais
O banco contém dados pessoais e dados potencialmente sensíveis, como:
- CPF;
- CNPJ;
- RG;
- data de nascimento;
- endereço;
- telefone;
- e-mail;
- dados bancários;
- informações de sinistro;
- documentos anexados;
- beneficiários;
- dependentes.

Regras obrigatórias:
- Não registrar dados pessoais completos em logs.
- Não exibir CPF/CNPJ completo em listagens sem necessidade real.
- Não usar documento como chave de rota.
- Não usar documento como chave React.
- Não retornar campos `*_limpo` ao frontend sem justificativa.
- Não expor dados bancários fora de telas autorizadas.
- Downloads de documentos devem exigir autorização.
- Dados de sinistro devem ter controle de acesso específico.
- Erros de API não devem conter dados pessoais.
- Testes automatizados não devem usar CPFs reais.

## 16. Consultas e performance

### 16.1 Busca textual
Campos normalizados devem ser priorizados em buscas:
- nome_normalizado
- documento_principal_limpo
- cpf_limpo
- cnpj_limpo
- matricula_normalizada

### 16.2 Paginação
Nenhuma listagem operacional deve carregar todos os registros.
Todas as listagens devem usar paginação no servidor.

### 16.3 Ordenação
Ordenação deve ser feita no banco, não apenas na página atual do frontend.

### 16.4 Índices
Antes de criar telas pesadas, revisar se existem índices adequados para:
- FKs;
- public_id;
- legado_id;
- documentos limpos;
- nomes normalizados;
- deleted_at;
- status_id;
- competencia_int;
- datas de vencimento;
- combinações frequentes de filtros.

Não criar índices sem medir ou justificar.

## 17. Regras para novas implementações
Ao criar uma nova feature:
1. Identificar o schema principal.
2. Identificar as tabelas envolvidas.
3. Separar entidade principal, tabelas auxiliares e tabelas de histórico.
4. Verificar se há `public_id`.
5. Definir quais campos podem ser expostos.
6. Definir filtros suportados pelo backend.
7. Definir paginação.
8. Definir ordenação.
9. Definir permissões.
10. Definir logs e auditoria.
11. Criar DTOs específicos.
12. Criar testes de aplicação e integração.
13. Atualizar a documentação da feature.

## 18. Regras permanentes para o projeto

**Regra 1**
Não criar tabelas no schema public.

**Regra 2**
Toda tabela nova deve pertencer a um schema de domínio.

**Regra 3**
Toda entidade exposta externamente deve preferir `public_id` quando existir.

**Regra 4**
IDs legados não devem orientar novas regras de negócio.

**Regra 5**
Campos `deleted_at` devem ser respeitados por consultas operacionais.

**Regra 6**
Dados pessoais devem ser mascarados em listagens.

**Regra 7**
O frontend não deve conhecer a modelagem física do banco.

**Regra 8**
DTOs devem ser desenhados para o caso de uso, não espelhar tabelas.

**Regra 9**
Novas consultas devem ser paginadas por padrão.

**Regra 10**
Alterações estruturais no banco devem ser feitas por migrations versionadas.

**Regra 11**
Toda migration deve ser revisada quanto a impacto em dados existentes.

**Regra 12**
Tabelas do schema legado são de rastreabilidade e migração, não de operação principal.

**Regra 13**
Documentos e arquivos devem ser acessados por serviço autorizado, nunca por caminho físico direto.

**Regra 14**
Dados financeiros devem usar tipos decimais adequados.

**Regra 15**
Toda feature nova deve atualizar a documentação do projeto.

## 19. Sequência recomendada de evolução dos módulos
Ordem sugerida após a listagem de Clientes:
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

Não tentar implementar vários módulos ao mesmo tempo.

## 20. Mapa resumido de dependências funcionais
```text
core.pessoa
   ↓
cadastro.cliente
   ↓
cadastro.cliente_vinculo
   ↓
seguro.proposta
   ↓
seguro.proposta_item
seguro.proposta_cobertura
seguro.proposta_beneficiario
seguro.proposta_movimento
   ↓
financeiro.titulo
financeiro.titulo_pagamento
comissao.lancamento_comissao
   ↓
sinistro.sinistro
documento.arquivo_vinculo
atendimento.protocolo_item
```
Esse mapa é conceitual e não substitui as FKs reais do banco.

## 21. Cuidados antes de alterar o banco
Antes de qualquer alteração estrutural:
- Confirmar se a tabela já existe.
- Confirmar se o campo já existe em outro local.
- Verificar impacto em migrations.
- Verificar impacto no EF Core.
- Verificar impacto nos endpoints.
- Verificar impacto no frontend.
- Verificar dados legados.
- Verificar performance.
- Verificar necessidade de índice.
- Verificar necessidade de backfill.
- Verificar rollback.
- Documentar a alteração.

## 22. Pendências técnicas para documentação futura
Este documento registra a visão estrutural inicial.
Ainda devem ser documentados em arquivos próprios:
- docs/18-dicionario-dados-core.md
- docs/19-dicionario-dados-cadastro.md
- docs/20-dicionario-dados-seguro.md
- docs/21-dicionario-dados-financeiro.md
- docs/22-dicionario-dados-documento.md
- docs/23-dicionario-dados-sinistro.md
- docs/24-dicionario-dados-legado-migracao.md

Cada dicionário deve conter:
- tabela;
- finalidade;
- colunas;
- tipo;
- obrigatoriedade;
- regra de negócio;
- relacionamento;
- se pode ser exposto em API;
- se contém dado sensível;
- exemplos de uso.

## 23. Diretriz final
A modelagem atual está organizada para suportar uma aplicação modular e escalável.

A regra principal daqui para frente é:
> Banco orienta o domínio, backend traduz domínio em casos de uso, frontend consome DTOs próprios da experiência do usuário.

Não permitir que o frontend, os DTOs ou as telas fiquem acoplados diretamente à estrutura física das tabelas.
