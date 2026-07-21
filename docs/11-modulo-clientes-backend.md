# 11. Módulo Clientes - Backend

O **Módulo Clientes** é o componente central para a gestão de informações cadastrais no sistema. Ele opera utilizando a arquitetura oficial do banco de dados e foi estruturado garantindo a separação rígida entre a identidade global e o papel operacional.

## 1. Fonte de Verdade e Nomenclatura Oficial do Banco
Para fins arquiteturais, adota-se a seguinte taxonomia restrita:
* **Banco Oficial Atual**: PostgreSQL 16 (conforme schema espelhado em `dump-webapolice-202607062125.sql`). Este é o único banco utilizado para a estruturação das entidades do domínio de clientes.
* **Banco Legado**: Banco SQL Server antigo do sistema AmauriBueno.
* **Schema `legado` (PostgreSQL)**: Um schema restrito existente no PostgreSQL oficial utilizado estritamente para manter a rastreabilidade e abrigar os mapas e metadados de migração. **Ele não deve ser utilizado** para a sustentação de novas regras de negócio.
* **Modelo Simplificado Incorreto**: O schema `clientes.clientes`, outrora projetado nos primórdios da implementação, é uma versão fictícia, descontinuada e incorreta. Este schema não faz parte do modelo de dados atual e não embasa as transações do sistema.

## 2. Arquitetura Final do Módulo
O fluxo de dados da aplicação funciona mediante os seguintes padrões arquiteturais:
* **Acesso a Dados**: Operamos integralmente sobre os schemas oficiais `core` e `cadastro` através de **Entity Framework Core (EF Core)** para ambas as operações de leitura e escrita.
* **Mapeamento de Leitura**: Consultas e DTOs de leitura são resolvidos na infraestrutura diretamente via projeções (LINQ Select), melhorando a performance ao descartar instâncias tracking pesadas do EF.
* **Gravação**: A camada de escrita adota o pattern *Repository* através da abstração orientada aos casos de uso (`IClienteRepository`).
* **Atomicidade (Transações)**: Inserções e atualizações que englobam a Pessoa, o Cliente, contatos e endereço são obrigatoriamente encapsuladas em uma transação de banco.
* **Identificadores**: Os identificadores de API, URLs de endpoints e contratos JSON (`Requests/Responses`) trafegam exclusivamente o `public_id` (UUIDv4). O ID numérico (bigint) permanece selado apenas nas tabelas relacionais físicas da infraestrutura.
* **Migrations**: Modelos que mapeiam a base de dados centralizada do PostgreSQL oficial contam com a configuração explícita de `ExcludeFromMigrations()`. Nenhuma migration do EF Core do módulo de clientes foi (ou será) disparada para alterar o schema das tabelas oficiais.
* **Resolução de Status**: O status do cliente é derivado dinamicamente da tabela catálogo `cadastro.cliente_status` através do referencial textual (`ativo`, `inativo`), rejeitando o emprego de identificadores numéricos hardcoded na aplicação.
* **Exclusão**: Não se aplica exclusão física. Quando acionada, a deleção submete-se ao modelo de exclusão lógica usando a coluna `deleted_at`.

## 3. Regras de Negócio e Domínio Implementadas

### Pessoa e Cliente
Os dois conceitos centrais de armazenamento estão subdivididos: `core.pessoa` (dados globais do indivíduo/empresa) e `cadastro.cliente` (regra transacional operacional).
* A entidade Pessoa pode existir livremente no sistema sem ter obrigatoriamente o papel de Cliente associado.
* Uma mesma Pessoa pode cumular inúmeros papéis operacionais (Cliente, Corretora, Seguradora, Agenciador).
* A tabela oficial do banco suporta que a Pessoa esteja referenciada em mais de um registro do tipo Cliente.
* **Bloqueio por Compartilhamento**: Para resguardar a integridade, a aplicação bloqueia sumariamente atualizações de dados pessoais gerais caso a Pessoa avaliada esteja sendo compartilhada simultaneamente em outras tabelas.
* **Tratativa HTTP**: Havendo conflito de papéis, o endpoint de edição sinaliza a situação por intermédio de um **HTTP 409 Conflict**.
* **Condição de Bloqueio**: Vínculos de papéis de uma pessoa que já possuam preenchimento em sua coluna `deleted_at` (ou seja, logicamente excluídos) não caracterizam um vínculo efetivo, permitindo dessa forma a atualização normal dos dados cadastrais.

### Documento (CPF/CNPJ)
* Durante a fase de cadastro, os documentos submetidos atravessam um fluxo completo de formatação, normalização algorítmica e dupla verificação.
* Campos contendo as versões expurgadas de pontuação (como `documento_principal_limpo`) não constam nos retornos públicos da API.
* Após o cadastro original, a regra fundamental torna a numeração do documento integralmente **imutável** para a funcionalidade de Edição Comum.
* Por segurança da informação, todo e qualquer retorno visual em tela entrega os documentos censurados e mascarados.

### Status Operacional
* O módulo repassa diretamente as solicitações para ativação ("ativo") ou inativação ("inativo") valendo-se dos códigos literais.
* Esses IDs associativos não figuram codificados como variáveis primitivas em handlers. O catálogo providencia a conversão instanciada.
* Fundamental notar que a flag booleana `cliente_status.ativo` serve *exclusivamente* para determinar se aquele respectivo status continua disponível no catálogo (para ser associado a clientes novos). O "estado" do Cliente é verificado pelo vínculo ao seu `status_id`.

### Contatos e Endereços
O agrupamento relacional de endereços de entrega/cobrança, além dos dados de contato, integram o conglomerado da Pessoa.
* **Múltiplos Contatos e Endereços**: O sistema suporta múltiplos registros de contato (com diferentes tipos como EMAIL, TELEFONE, CELULAR) e de endereço (RESIDENCIAL, COMERCIAL, CORRESPONDENCIA, OUTRO) associados a uma mesma Pessoa.
* **Marcação de Principal**: Cada contato ou endereço possui um indicador lógico `Principal` para definir qual registro deve ser considerado como padrão/principal para comunicação ou correspondência. Apenas um contato e um endereço podem ser designados como principal de cada vez no formulário.
* **Regra basilar no Update**: Nenhuma alteração pontual subscreve (overwrite) o valor de um contato ou endereço existente. As edições desencadeiam a inativação da entrada atual (`Ativo = false`), promovendo a inclusão de um contato ou endereço perfeitamente novo — salvaguardando a trilha histórica e integridade do registro de auditoria. Caso o valor permaneça idêntico, o registro ativo existente é mantido.
* **Exceção Assumida (Cidades)**: O registro no banco conta momentaneamente com submissões usando a chave `cidadeId` baseada em numerais (ID de integração legado). Por conta da indefinição ou omissão de um `public_id` formatado em `core.cidade`, essa exceção vigora como dívida provisória de desenho.

### Atomicidade Comprovada
* Todas as quatro vertentes de domínio relacional descritas (Pessoa, Cliente, Endereço e Contato) viajam em um único bloco transacional orquestrado pela interface portuária.
* Falhas inerentes na infraestrutura durante quaisquer etapas disparam invariavelmente um **Rollback**.
* A prova arquitetural desta funcionalidade encontra-se na suíte de testes de integração, munida de um teste que compele a inserção através de uma Chave Estrangeira de Cidade falsa/inexistente. O teste verifica fisicamente, após o DbUpdateException, o saldo nulo de manipulações acidentais nas 4 tabelas, evidenciando ausência absoluta de registros residuais.

---

## 4. Dívidas Técnicas Conhecidas

O sistema conta com um catálogo formal de defasagens técnicas que serão endereçadas nas próximas *sprints* operacionais:

* No Frontend de Clientes, a ferramenta ESLint dispara um `react-hooks/incompatible-library` ao escanear o uso sintático de `watch('falecido')` oriundo do pacote React Hook Form no `ClienteForm.tsx`.
* **Impactos**: O referido warning não causa quebras. Não impede o processamento dos testes (Vitest), nem inviabiliza o Type Checking do TypeScript (tsc) e não desarmou a integridade da *build* (Vite/Rolldown).
* **Solução Projetada**: Uma futura verificação sobre as opções do React Hook Form, explorando o utilitário isolado `useWatch` a fim de aprimorar a memorização do componente do React e coibir possíveis recargas de tela.

---

## 5. Próximas Evoluções Planejadas

O mapeamento da funcionalidade foi estendido prevendo futuras ramificações não finalizadas no escopo atual. Seguem relacionadas como etapas futuras:

* Substituição do provisório `cidadeId` (int) por um código representacional estável ou UUID associado à `core.cidade`.
* Estabelecimento de uma tabela visual (*dropdown*/busca dinâmica) de Cidades/UFs nos painéis, sincronizada à API.
* Implementação do controle de concorrência com travas otimistas (RowVersion) para a rota de edição de Cliente (`PUT`), protegendo *updates* simultâneos.
* Concepção e implantação de uma rota ou tela estritamente exclusiva (e altamente auditada) para fins de correção documental, com a capacidade de transpor a imutabilidade habitual de CPF/CNPJs equivocados.
* Integração e suporte completo à edição das propostas, convênios, controle financeiro, gerenciamento de vinculações de dependentes diretos e empresas consorciadas (Estipulantes/Correções).

---

## 6. Auditoria de Edição de Clientes (Julho 2026)

* **Causa da falha anterior**:
  * **Backend**: As atualizações de contatos e endereços subscreviam indevidamente o registro histórico.
  * **Frontend**: A rota de edição não estava configurada, botões de ação estavam ausentes, e o formulário não era recarregado via `reset` após leitura assíncrona.
* **Arquivos alterados**: `AlterarClienteHandler.cs`, `PessoaEnderecoModel.cs`, `routePaths.ts`, `ClienteForm.tsx`, `ClienteDetalhePage.tsx`, `ClientesTable.tsx`, `ClientesMobileList.tsx`, `EditarClientePage.test.tsx`.
* **Comportamento Pessoa Compartilhada**: Bloqueio total (HTTP 409) para dados globais caso a Pessoa pertença a múltiplos papéis.
* **Estratégia Contatos/Endereços**: Comparação prévia. Em caso de mudança, o registro antigo é inativado (`Ativo = false`) e um novo é criado, mantendo auditoria.
* **Evolução de Múltiplos Contatos/Endereços (Julho 2026)**:
  * Refatorado o fluxo de cadastro e edição de Clientes para suportar múltiplos registros de contatos e endereços (com marcação de principal).
  * O formulário no frontend (`ClienteForm.tsx`) foi atualizado utilizando o utilitário `useFieldArray` para permitir a inserção e remoção dinâmica desses registros.
  * O backend foi modificado (`CadastrarClienteCommand`, `AlterarClienteCommand` e respectivos handlers) para processar e persistir essas listas respeitando a regra de auditoria (inativação e nova inserção).
