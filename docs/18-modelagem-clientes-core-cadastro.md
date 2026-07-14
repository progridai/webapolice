# Modelagem de Clientes (Core e Cadastro)

## Objetivo do documento

Este documento detalha a estrutura de tabelas que suportam as informações e funcionalidades ligadas à entidade Cliente. Ele atua como referência específica para a implementação da funcionalidade de **Detalhes do Cliente** e apoiará a construção de fluxos relacionados aos domínios Core e Cadastro.

Este documento será a referência técnica primária para a implementação das features de:
* detalhes do cliente;
* cadastro de cliente;
* edição de cliente;
* vínculos;
* dependentes;
* contatos;
* endereços;
* consultas de apoio;
* DTOs de cliente;
* mapeamentos EF Core;
* validações de dados pessoais.

## Tabelas do schema `core`

O schema `core` concentra as tabelas base que sustentam as informações de pessoas (físicas ou jurídicas) em todo o sistema. Estas entidades podem ser reaproveitadas além do conceito de cliente (ex: corretoras, agenciadores, usuários).

### `core.pessoa`

Representa uma pessoa física ou jurídica centralizando seus dados pessoais e fiscais essenciais. Um cliente no banco de dados necessariamente possui uma associação com um registro em `core.pessoa`.

**Principais Campos:**
* `id`
* `public_id`
* `tipo_pessoa`
* `nome`
* `nome_normalizado`
* `documento_principal`
* `documento_principal_limpo`
* `documento_valido`
* `data_nascimento`
* `sexo`
* `observacao`
* `created_at`
* `updated_at`
* `deleted_at`

**Regras e Uso:**
* Concentra dados pessoais principais;
* O cliente referencia pessoa através da chave estrangeira `pessoa_id`;
* `public_id` deve ser preferido em APIs quando possível, embora para detalhes de cliente normalmente a rota parta do cliente;
* O campo `documento_principal_limpo` (apenas números) é de uso estritamente técnico e interno do backend (ex: integração e busca exata); não deve trafegar ou ser exibido para o frontend sem justificativa cabal;
* `deleted_at` indica exclusão lógica do registro e deve ser respeitado em filtros operacionais.

### `core.pessoa_documento`

Armazena documentos de identidade adicionais de uma pessoa (como RG, CNH, Passaporte, etc.).

**Principais Campos:**
* `id`
* `pessoa_id`
* `tipo_documento`
* `numero`
* `numero_limpo`
* `orgao_emissor`
* `data_emissao`
* `principal`
* `created_at`

**Regras e Uso:**
* Permite armazenar mais de um documento adicional para a mesma pessoa;
* O campo `numero_limpo` é de uso técnico;
* Os dados de documentos devem ser mascarados (ou parcialmente ofuscados) ao serem apresentados em componentes tipo listagem (grids, tabelas);
* O valor de qualquer documento jamais deve ser usado como chave em parâmetros de rotas URL.

### `core.pessoa_contato`

Tabela de cadastro flexível de contatos da pessoa, como telefones fixos, celulares e e-mails.

**Principais Campos:**
* `id`
* `pessoa_id`
* `tipo_contato`
* `valor`
* `valor_normalizado`
* `principal`
* `ativo`
* `created_at`

**Regras e Uso:**
* As listagens de contatos do cliente sempre devem ser filtradas por contatos onde `ativo = true`;
* Pode haver mais de um e-mail ou número de telefone associado;
* O contato marcado como `principal` deve ser priorizado e retornado em endpoints que exibem telas resumidas ou cabeçalhos;
* `valor_normalizado` serve para fins técnicos (buscas, validação de duplicidade) e não compõe a exibição ao frontend.

### `core.pessoa_endereco`

Gestão dos endereços físicos vinculados a uma pessoa.

**Principais Campos:**
* `id`
* `pessoa_id`
* `cidade_id`
* `tipo_endereco`
* `cep`
* `logradouro`
* `numero`
* `complemento`
* `bairro`
* `uf`
* `principal`
* `ativo`
* `legado_situacao_endereco`
* `created_at`

**Regras e Uso:**
* A pessoa pode registrar múltiplos endereços;
* O endereço `principal` deve ser retornado por padrão em requisições de resumo ou detalhes rápidos;
* Endereços são considerados dados pessoais PII;
* Em listagens ou visões abertas e gerais, evitar o preenchimento integral dos campos do endereço sem necessidade (exibir apenas estado, cidade ou bairro).

### Tabelas auxiliares do `core`

* **`core.estado`**: Tabela fixa com as Unidades Federativas de localização do país.
* **`core.cidade`**: Tabela fixa com municípios. O endereço referencia `cidade` para garantir padronização.
* **`core.banco`**: Listagem de instituições bancárias, necessária para vínculos, transferências e pagamentos.

**Uso:** As tabelas complementares evitam duplicação e desnormalização de nomes nas tabelas transacionais, fornecendo segurança de dados e relatórios unificados. Novas funcionalidades que precisarem dessas entidades devem referenciá-las ao invés de duplicar as colunas texto em seus schemas.

---

## Tabelas do schema `cadastro`

O schema `cadastro` mapeia as entidades específicas que representam a vida e as associações de negócio operacionais dos clientes e corretores do sistema webapolice.

### `cadastro.cliente`

Representa o núcleo do participante dentro do negócio. Ele não duplica o CPF ou Nome do usuário (estas informações ficam atreladas à Pessoa), mas gerencia a vida útil e regras da entidade enquanto cliente.

**Principais Campos:**
* `id`
* `public_id`
* `pessoa_id`
* `status_id`
* `falecido`
* `data_obito`
* `observacao`
* `data_cadastro_legado`
* `legado_id`
* `created_at`
* `updated_at`
* `deleted_at`

**Regras e Uso:**
* Representa estritamente a "entidade pessoa" vivendo seu ciclo de vida "como cliente" no ecossistema;
* Delega todos os dados pessoais básicos para sua relação em `pessoa_id`;
* A coluna `status_id` possui chave estrangeira em `cadastro.cliente_status`;
* As informações booleanas e temporais de `falecido` e `data_obito` são dados estritamente sensíveis;
* Utilizar obrigatoriamente `public_id` em rotas (ex: `/clientes/detalhes/{public_id}`) e APIs públicas expostas;
* A regra de `deleted_at` deve ser observada em qualquer fluxo ou pesquisa.

### `cadastro.cliente_status`

Domínio de estados possíveis em que o cliente se encontra.

**Principais Campos:**
* `id`
* `codigo`
* `nome`
* `ativo`

**Valores Iniciais Identificados:**
* ativo
* inativo

**Regras e Uso:**
* O frontend é agnóstico ao ID numérico. Ele não deve em hipótese alguma realizar condicionais baseadas na coluna `id` (ou `status_id`);
* A API deve mapear e retornar ao frontend um código padronizado (ou o próprio nome descritivo amigável);
* Requisições e filtros de busca pelo frontend devem usar o valor em string do status ou um código estável pré-acordado.

### `cadastro.cliente_vinculo`

Responsável por armazenar toda forma de vínculo, subordinação de grupo ou convênio (ex: emprego público, empresa, sindicato) pertencente ao cliente.

**Principais Campos:**
* `id`
* `cliente_id`
* `pessoa_id`
* `estipulante_id`
* `subestipulante_id`
* `grupo_id`
* `subgrupo_id`
* `lotacao_id`
* `matricula`
* `matricula_normalizada`
* `banco_id`
* `agencia`
* `conta_corrente`
* `legado_cliente_id`
* `criterio_criacao`
* `ativo`
* `created_at`
* `updated_at`

**Regras e Uso:**
* É possível e comum um cliente possuir múltiplos vínculos (múltiplas matrículas e empregos); não assumir cardinalidade 1:1 de vínculos por cliente;
* O atributo matrícula é sempre referente a determinado vínculo e não à pessoa em si;
* Os vínculos servem como elo referencial para níveis de hierarquia: estipulante, subestipulante, grupo, subgrupo e lotação;
* Campos contendo banco, agência e conta corrente representam dados de alto teor financeiro e sensibilidade — devem ser devolvidos à interface apenas mediante necessidade e autorização formal;
* A coluna `criterio_criacao` atende a necessidades técnicas subjacentes do backend/negócio;
* As interfaces de Detalhe e formulários necessitam separar visualmente vínculos que estão ativos e os inativos (histórico).

### `cadastro.cliente_dependente`

Listagem e associação de dependentes (familiares, tutelados, etc.) do cliente.

**Principais Campos:**
* `id`
* `cliente_id`
* `pessoa_id`
* `tipo_relacao`
* `nome`
* `cpf`
* `cpf_limpo`
* `rg`
* `orgao_rg`
* `data_emissao_rg`
* `data_nascimento`
* `legado_origem`
* `created_at`

**Regras e Uso:**
* A modelagem aceita que um dependente possua ou não ligação direta (`pessoa_id`) como entidade isolada de Pessoa no sistema (dependentes com menor idade podem não estar consolidados);
* O CPF é frequentemente o identificador principal e constitui dado sensível — aplicam-se máscaras quando exibido;
* O campo `legado_origem` reflete rastreabilidades migratórias técnicas e não entra na validação de regras correntes.

### Entidades relacionadas ao vínculo

Essas entidades fornecem contexto de hierarquia e identificação organizacional aos vínculos do cliente:
* **`cadastro.estipulante`**: Entidade principal que representa a contratante, empregadora, sindicato ou convênio macro.
* **`cadastro.subestipulante`**: Agrupamento lógico, departamento maior ou subdivisão institucional abaixo do estipulante.
* **`cadastro.grupo`** / **`cadastro.subgrupo`**: Divisões organizacionais de médio e menor nível, usadas para classificação comercial, parametrizações e permissões de cliente/vínculos.
* **`cadastro.lotacao`**: Agrupamento ou localidade de atuação final/alocação do indivíduo.

A tela de Detalhes do Cliente deverá buscar e apresentar os nomes destas entidades sempre que houver vinculações ativas para o cliente selecionado.

### Entidades complementares de cadastro

Tabelas adicionais do schema:
* `cadastro.seguradora`
* `cadastro.corretora`
* `cadastro.agenciador`

Elas gerenciam os parceiros da operação e são fortemente utilizadas por módulos de propostas, emissões, repasses e comissões. Contudo, **não são** a ênfase primária na visualização inicial do "Resumo/Detalhes do Cliente".

---

## Mapa conceitual do Cliente

Este diagrama representa conceitualmente o fluxo e a interligação das entidades. (*Nota: As restrições e as Cardinalidades finais são impostas pelas chaves estrangeiras no banco.*)

```text
core.pessoa
   ↓
cadastro.cliente
   ↓
cadastro.cliente_vinculo
   ├── cadastro.estipulante
   ├── cadastro.subestipulante
   ├── cadastro.grupo
   ├── cadastro.subgrupo
   ├── cadastro.lotacao
   └── core.banco

core.pessoa
   ├── core.pessoa_documento
   ├── core.pessoa_contato
   └── core.pessoa_endereco

cadastro.cliente
   └── cadastro.cliente_dependente
```

---

## Preparação para Detalhes do Cliente

A interface a ser criada para **Detalhes do Cliente** agrupará informações espalhadas por diversas tabelas num arranjo coeso e amigável. A arquitetura recomendada para organização da página completa seria:

1. Resumo do cliente
2. Dados pessoais
3. Documentos
4. Contatos
5. Endereços
6. Vínculos
7. Dependentes
8. Propostas vinculadas
9. Títulos financeiros resumidos
10. Documentos anexados
11. Atendimentos/protocolos
12. Dados de migração

### Escopo sugerido (Primeira Versão / Etapa Inicial)

Para iniciar os trabalhos técnicos de endpoints e frontend, a primeira versão se limitará a um escopo fechado, contendo somente dados centrais de `core` e `cadastro`:
* **Resumo do cliente:** Nome, CPF mascarado, status, data de nascimento.
* **Dados pessoais principais:** Dados agregados de `pessoa`.
* **Contatos principais:** Telefone e email priorizados.
* **Endereço principal:** O endereço padrão para correspondência/registro.
* **Vínculos ativos:** Matrícula e entidades agrupadoras do seu status atual.
* **Dependentes:** Relação rápida de seus dependentes diretos.

**Atenção:** Propostas, quadros financeiros, sinistros e documentação formal em anexo serão omitidos neste momento, sendo incorporados nas iterações seguintes caso endpoints exclusivos não existam, já que geram uma carga substancial (dados pesados) e requerem lógicas de paginação diferentes.

### DTO recomendado para Detalhes do Cliente (Referência)

Esta é uma estruturação *conceitual* recomendada para que o backend modele a entrega dos dados agregados solicitados pela interface na etapa 1 (O contrato real e exato deverá ser consolidado pelo desenvolvedor no backend e assegurado em testes).

```ts
interface ClienteDetalheDto {
  publicId: string;
  nome: string;
  documentoMascarado?: string;
  status: {
    codigo: string;
    nome: string;
  };
  dataNascimento?: string;
  falecido: boolean;
  contatos: ClienteContatoDto[];
  enderecos: ClienteEnderecoDto[];
  vinculos: ClienteVinculoDto[];
  dependentes: ClienteDependenteDto[];
}
```

## Regras e obrigações para endpoints do Detalhes do Cliente

Ao iniciar a codificação das respostas de backend, as seguintes diretrizes são mandatórias:
* A rota pública de detalhes e consultas do cliente requer o parâmetro em formato UUID referenciando o `public_id`.
* Rotas da aplicação nunca devem receber e passar diretamente o número do CPF do cliente como seu identificador de URL.
* Toda solicitação do frontend precisa repassar validação de permissão de visualização (Autorização) para a camada de acesso do usuário.
* Documentos (`pessoa_documento` e `documento_principal`) devem passar por métodos de máscara de ofuscação no DTO caso as credenciais não exijam visão plena.
* Em hipótese nenhuma deve-se retornar como dados do DTO as colunas `_limpo` e os dados puros em plain-text sem formatação/negócio.
* Evite repassar colunas como `legado_id` nos DTOs que populam interface ao consumidor comum.
* Caso as listagens como Vínculos, Propostas e Financeiro expandam muito, essas *collections* devem ser extraídas para rotas filhas independentes, adotando paginação desde o dia um (`/clientes/{id}/vinculos`, `/clientes/{id}/titulos`).
* Preservar o mascaramento e bloqueios a registros PII sensíveis até nos middlewares de logs, não injetando todo o JSON da resposta nos canais internos (App Insights/Seq).
* Certificar-se e garantir `deleted_at IS NULL` ativo por padrão nas queries de busca no EF Core.

## Perguntas em aberto

Apesar da modelagem inicial clara, durante o desenrolar das sprints algumas decisões poderão demandar refinamento e registro explícito. Seguem pendências sugeridas como guia de esclarecimento de negócio para implementações vindouras:

* O endpoint inicial responsável pelo agrupamento de detalhes usará a chave de pesquisa `id` interno ou o `public_id` nos acessos de roteamento interno não-público?
  * **Decisão (Julho/2026):** Foi utilizado o `id` interno (`long`) para a rota `/api/clientes/{id}`, já que a entidade `Cliente` em sua modelagem em código ainda não dispõe de `public_id`.
* Existe um mapeamento consolidado de "qual usuário/role/perfil" pode solicitar visão do documento completo sem máscara na interface web?
* No conceito da interface de usuário existirá a definição de "vínculo principal" fixo, ou exibem-se apenas as entidades em formato "Vínculos Ativos"?
* Os dados bancários dos vínculos (que exigem privilégios sensíveis) devem popular o componente de tela do Detalhe do Cliente desde a V1?
* A exibição dos dependentes deverá constar integralmente já na versão inicial de resumo?
* Os relatórios, propostas vinculadas e arquivos do cliente serão sub-abas dinâmicas de chamadas HTTP assíncronas na mesma rota de página, ou haverá separação de *routes* distintas no React Router?
* Os títulos financeiros e repasses irão requerer `role` e política de IAM adicional apartada das permissões de Gestor de Clientes regular?
