# Guia Oficial para Implementação de Novos Módulos

**Status:** Em vigor  
**Versão:** 2.1  
**Data da última revisão:** 31 de Julho de 2026  
**Etapa do projeto contemplada:** Definição e Implementação de Novos Módulos  
**Responsável pela aprovação:** A definir  

> **REGRA CRÍTICA:** Caso o código atual ou a documentação oficial contradiga este guia, a IA deverá interromper a implementação, informar a divergência em linguagem compreensível e aguardar uma decisão.

## 1. Análise Inicial Obrigatória

Antes de iniciar a implementação, consulte a seguinte tabela de caminhos reais dos componentes do módulo de Segurança, evitando a necessidade de reanalisar o projeto inteiro:

- **Entidade e configuração de Modulo**: `backend/src/WebApolice.Modulos.Seguranca/Domain/Modulo.cs`, `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Persistence/Configurations/ModuloConfiguration.cs`
- **Entidade Recurso**: `backend/src/WebApolice.Modulos.Seguranca/Domain/Recurso.cs`, `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Persistence/Configurations/RecursoConfiguration.cs`
- **Entidade Permissao**: `backend/src/WebApolice.Modulos.Seguranca/Domain/Permissao.cs`, `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Persistence/Configurations/PermissaoConfiguration.cs`
- **Entidade Perfil**: `backend/src/WebApolice.Modulos.Seguranca/Domain/Perfil.cs`, `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Persistence/Configurations/PerfilConfiguration.cs`
- **PermissoesSeguranca (constantes)**: `backend/src/WebApolice.Modulos.Seguranca/Application/Authorization/PermissoesSeguranca.cs`
- **Migration de carga inicial**: `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260723101318_CargaInicialSeguranca.cs`
- **Migration administrativa de Segurança**: `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260724103424_CargaAdministracaoSeguranca.cs`
- **Migration de habilitação de módulos**: `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260724194126_AdicionarHabilitacaoModulo.cs`
- **Migration de CADASTRO para CLIENTES**: `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260728144458_RenomearModuloCadastroParaClientes.cs`
- **PermissaoAuthorizationHandler**: `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Authorization/PermissaoAuthorizationHandler.cs`
- **AuthorizePermissaoAttribute**: `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Authorization/AuthorizePermissaoAttribute.cs`
- **PermissaoPolicyProvider**: `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Authorization/PermissaoPolicyProvider.cs`
- **IPermissoesEfetivasService**: `backend/src/WebApolice.Modulos.Seguranca/Application/Ports/IPermissoesEfetivasService.cs`
- **IAcessoOperadorSistemaService**: `backend/src/WebApolice.Modulos.Seguranca/Application/Ports/IAcessoOperadorSistemaService.cs`
- **Controller de GET /api/seguranca/me**: `backend/src/WebApolice.Modulos.Seguranca/Api/Controllers/MeController.cs`
- **Caso de uso de /api/seguranca/me**: `backend/src/WebApolice.Modulos.Seguranca/Application/UseCases/Me/ObterUsuarioAutenticadoUseCase.cs`
- **AuthorizationProvider do frontend**: `apps/web/src/auth/AuthorizationProvider.tsx`
- **PermissionProtectedRoute**: `apps/web/src/app/routes/PermissionProtectedRoute.tsx`
- **AppNavigation**: `apps/web/src/layouts/AuthenticatedLayout/AppNavigation.tsx`
- **Arquivo de rotas de Clientes**: `apps/web/src/features/clientes/routes/clientes.routes.tsx`
- **Arquivo de rotas de Segurança**: `apps/web/src/features/seguranca/routes/seguranca.routes.tsx`
- **Componente de seleção de permissões**: `apps/web/src/features/seguranca/components/SelecaoPermissoes.tsx`
- **Componente de seleção de perfis**: `apps/web/src/features/seguranca/components/SelecaoPerfis.tsx`
- **Auditoria administrativa de Segurança**: `backend/src/WebApolice.Auditoria/Infrastructure/RegistradorAuditoria.cs`
- **Testes de Segurança**: `backend/tests/WebApolice.Integration.Tests/Modulos/Seguranca/`

*(Não inventar caminhos. A IA deve consultar primeiro os caminhos de referência registrados acima e, caso não exista, informar a divergência. Caso algum caminho não seja encontrado, registrar: PENDENTE DE CONFIRMAÇÃO).*

---

## 2. Estrutura Obrigatória do Guia

### 2.1. Finalidade do guia

Este documento deve ser utilizado sempre que houver o planejamento ou a execução de um **novo módulo** no sistema WebApólice. Ele será utilizado por diferentes pessoas, inclusive responsáveis pelo negócio que podem não possuir conhecimento técnico.
Ele deve ser anexado ao início da conversa ou chat com uma IA de desenvolvimento para garantir o alinhamento.
**Nenhuma implementação deve começar antes que as perguntas obrigatórias sejam respondidas** pelo responsável. Este guia serve como a principal referência oficial de Segurança e Autorização do sistema WebApólice.

### 2.2. Arquitetura consolidada

A arquitetura do WebApólice separa as responsabilidades de segurança da seguinte forma:

- **Keycloak**: Responsável exclusivo por autenticação, gerenciamento de login/senha, sessão, emissão do JWT e identidade do usuário.
- **PostgreSQL**: Responsável por dados do usuário interno, perfis, permissões funcionais, módulos habilitados, auditoria e autorização da aplicação.
- **Usuário interno e keycloak_sub**: O vínculo entre o usuário do Identity Provider (Keycloak) e o banco local ocorre pelo atributo de _subject_ (`OIDC sub`), armazenado na coluna `seguranca.usuario.keycloak_sub`.
- **Módulos, recursos e permissões**: As permissões são sempre vinculadas a um recurso e os recursos a um módulo (entidades estruturais do sistema).
- **Perfis e permissões efetivas**: O usuário pode possuir zero, um ou vários perfis. As permissões efetivas são a união das permissões dos perfis ativos.
- **Acesso total**: Qualquer perfil ativo com a _flag_ `acesso_total` habilitada concede acesso total, não precisando ter registros diretos de permissões, **mas** seu acesso continua limitado aos módulos habilitados.
- **Módulos habilitados**: Definem a disponibilidade funcional da instalação ou quais blocos funcionais estão ativos. O módulo habilitado é validado centralmente pelo mecanismo de autorização. *(O controle de módulos representa apenas habilitação de funcionalidades. Não representa automaticamente planos, cobrança, licenciamento, multiempresa ou multitenancy. Esses recursos só poderão ser criados futuramente mediante aprovação específica).*
- **Autorização no backend**: É a proteção definitiva. Feita primordialmente por intermédio do `AuthorizePermissaoAttribute` e policies dinâmicas resolvidas pelo `PermissaoPolicyProvider`. Não se deve repetir manualmente a mesma validação em cada controller.
- **Autorização no frontend**: O frontend controla menu, rotas e ações (ocultando botões), melhorando a experiência do usuário. O contexto de estado é provido pelo `AuthorizationProvider` carregando as flags da chamada `/api/seguranca/me`.
- **Auditoria**: Diferencia-se a auditoria administrativa (Segurança) da auditoria funcional (negócio). Não se deve presumir a gravação de eventos operacionais puramente na tabela da área de segurança.
- **Ausência de permissões diretas**: Regra estrita: **não existem permissões diretas por usuário**. A concessão deve ser _sempre_ por perfil.

### 2.3. Conceitos técnicos

- **Módulo**: Produto ou conjunto comercial habilitável no sistema. (Ex: `CLIENTES`, `APOLICES`, `FINANCEIRO`).
- **Recurso**: Área funcional interna pertencente ao módulo. Exemplo:
  - Módulo: `CLIENTES`
  - Recurso: `CLIENTES`
  - Permissões: `clientes.visualizar`, `clientes.inserir`, `clientes.alterar`, `clientes.inativar`, `clientes.reativar`.
- **Permissão**: Ação granular executada e permitida sobre um recurso. (Ex: `clientes.visualizar`, `clientes.inserir`).
- **Perfil**: Conjunto de permissões atribuído a usuários (Ex: `ADMINISTRADOR`, `ADMINISTRATIVO`).
- **Módulo habilitado**: Flag que define se a funcionalidade está disponível na instalação específica.
- **Acesso total**: Permite acesso irrestrito, mas obedece à regra de atuar SOMENTE em módulos habilitados.

---

## 3. Dinâmica do Questionário (Separação de Negócio e Análise)

O processo de definição deve ser dividido em duas partes distintas. O questionário apresentado ao usuário **não deve exigir conhecimento** sobre banco de dados, schemas, migrations, DbContext, EF Core, UUID, policies, attributes, handlers, componentes React ou estrutura interna do projeto.

- **PARTE A — Perguntas para o responsável pelo módulo**: Feitas em linguagem simples, focando em negócio.
- **PARTE B — Análise técnica da IA**: Realizada internamente e apresentada ao responsável para aprovação antes da implementação. (verificações de tabelas, código, etc.). A IA será responsável por transformar as respostas de negócio em decisões técnicas.

### Condução das Perguntas

A IA poderá apresentar todas as perguntas de negócio de uma única vez, organizadas por assunto.

Ela deverá:
- considerar respostas já fornecidas;
- não repetir perguntas já respondidas corretamente;
- repetir ou reformular perguntas sem resposta, incompletas ou ambíguas;
- explicar a pergunta com um exemplo quando necessário;
- não aceitar “não sei” como definição final;
- ajudar o usuário a encontrar a resposta;
- apresentar uma recomendação quando possível;
- solicitar confirmação explícita;
- não iniciar a implementação enquanto houver decisão obrigatória pendente.

> **Importante:** Quando o usuário não souber responder, a IA deverá:
> 1. explicar a decisão em linguagem simples;
> 2. informar por que ela é necessária;
> 3. apresentar exemplos ou opções;
> 4. consultar o projeto quando a resposta puder ser descoberta tecnicamente;
> 5. recomendar a alternativa mais adequada;
> 6. pedir confirmação.
> 
> A IA não poderá escolher silenciosamente uma regra de negócio.

---

## 4. Questionário Simplificado para o Usuário

A IA deve fazer as seguintes perguntas de negócio (podendo apresentá-las organizadas por assunto):

### 4.1. Objetivo do módulo
1. Qual será o nome apresentado no sistema? *(Exemplo: “Apólices”, “Comissões” ou “Financeiro”)*
2. Para que esse módulo será utilizado? *(Exemplo: “Cadastrar e acompanhar as apólices dos clientes.”)*
3. Quem utilizará esse módulo no dia a dia? *(Exemplo: “Equipe administrativa, corretores e gestores.”)*
4. Esse módulo poderá ser habilitado separadamente para cada empresa ou instalação que utiliza o WebApólice? *(Exemplo: “Uma empresa pode utilizar o WebApólice com ou sem esse módulo.”)*

*A definição do código técnico deve ser proposta pela IA com base no nome informado (ex: Nome = Apólices -> APOLICES), pedindo somente aprovação.*

### 4.2. Funcionalidades principais
1. O que o usuário poderá fazer dentro desse módulo? *(Exemplo: consultar, cadastrar, alterar, anexar documentos)*
2. Existem áreas diferentes dentro do módulo? *(Exemplo: apólices, endossos, parcelas)*
3. Alguma dessas áreas será apenas para consulta?
4. Existem informações que só alguns usuários poderão visualizar?

### 4.3. Ações permitidas
Para cada funcionalidade identificada, perguntar:
1. O usuário poderá consultar os registros?
2. Poderá cadastrar novos registros?
3. Poderá alterar registros existentes?
4. Poderá inativar ou cancelar registros?
5. Um registro inativado poderá ser reativado?
6. Um registro poderá ser apagado definitivamente? *(Aviso da IA: Apagar definitivamente remove o registro do sistema. Inativar mantém o histórico e impede seu uso. Qual comportamento deve ser utilizado?)*
7. Existem ações especiais além de cadastrar e alterar? *(Exemplo: aprovar, emitir, importar, exportar)*

*Regras da IA: Não criar exclusão física por padrão (apenas com aprovação explícita); preferir inativação quando for necessário preservar histórico; criar permissão separada para ações importantes de negócio.*

### 4.4. Perfis e acessos
*(A IA não deve pedir ao usuário para informar códigos de permissões)*
1. Quem poderá apenas consultar?
2. Quem poderá cadastrar?
3. Quem poderá alterar?
4. Quem poderá inativar ou cancelar?
5. Quem poderá realizar ações especiais, como aprovar ou emitir?
6. Algum perfil existente deve receber acesso automaticamente?
7. Será necessário criar um novo perfil padrão?
8. O módulo ficará inicialmente disponível apenas para administradores?

*Regras da IA: ADMINISTRADOR utiliza acesso_total e não recebe todos os vínculos explícitos de permissões; ADMINISTRATIVO não recebe novas permissões automaticamente; permissões são concedidas por perfis; não existem permissões diretas por usuário.*

### 4.5. Habilitação do módulo
1. Esse módulo poderá ser ligado ou desligado para cada instalação?
2. Quando desligado, ninguém poderá consultar ou alterar suas informações?
3. Alguma rotina precisa continuar funcionando mesmo com o módulo desligado?
4. O módulo depende de outra funcionalidade do sistema? *(A IA deverá investigar o código técnico da dependência posteriormente).*

### 4.6. Informações e regras do cadastro
1. Quais são as principais informações que deverão ser cadastradas?
2. Quais campos são obrigatórios?
3. Existe alguma informação que não pode ser alterada depois do cadastro?
4. Existem números ou códigos que não podem se repetir?
5. Quais situações ou status um registro pode possuir? *(Exemplo: proposta, vigente, cancelada)*
6. Como ocorre a mudança entre esses status?
7. Existem regras diferentes para determinados tipos de registro?

### 4.7. Dados já existentes
1. Essas informações já existem no banco atual?
2. Existe algum sistema antigo que contenha esses dados?
3. Será necessário importar dados anteriores?
4. O novo módulo utilizará cadastros já existentes, como Clientes, Seguradoras ou Produtos?
5. Existe algum documento técnico ou planilha que descreva esses dados?

*A IA deverá, com essas respostas, consultar o banco, código existente ou documentações técnicas, sem perguntar de schema, tabelas ou migrations.*

### 4.8. Integrações
1. O módulo enviará ou receberá informações de outro sistema?
2. Será necessário importar planilhas ou arquivos?
3. Será necessário gerar documentos?
4. Será necessário enviar e-mails ou notificações?
5. Existe alguma rotina automática? *(Exemplo: consultar dados externos após o cadastro)*

### 4.9. Auditoria e histórico
1. Quais ações precisam ficar registradas no histórico?
2. É necessário saber quem cadastrou ou alterou?
3. É necessário guardar os dados anteriores?
4. Alguma informação não pode aparecer no histórico?
5. O usuário precisa visualizar esse histórico em uma tela?

*A IA deverá distinguir a auditoria administrativa de Segurança da auditoria funcional, não assumindo gravação automática em seguranca.auditoria_permissao para eventos puramente operacionais.*

### 4.10. Critérios de aceite
1. O que precisa estar funcionando para o módulo ser considerado concluído?
2. Quais cenários precisam ser testados?
3. Quem será responsável pela homologação?
4. Existe alguma funcionalidade que ficará para uma etapa futura?

---

## 5. Ficha de Definição do Módulo

O documento a ser preenchido deve conter as duas seções abaixo. O usuário responde a Parte A, e a IA investiga e preenche a Parte B (apresentando para aprovação antes de codificar).

```text
PARTE A — DEFINIÇÕES DE NEGÓCIO

Nome apresentado no sistema:
Objetivo:
Usuários do módulo:
Funcionalidades principais:
Áreas internas:
Ações permitidas:
Regras de consulta:
Regras de cadastro:
Regras de alteração:
Regras de inativação:
Regras de reativação:
Regras de exclusão:
Ações especiais:
Perfis e responsabilidades:
Módulo habilitável:
Dependências:
Informações principais:
Campos obrigatórios:
Regras de unicidade:
Status e transições:
Dados já existentes:
Importação de dados:
Integrações:
Processos automáticos:
Eventos que precisam de histórico:
Dados sensíveis:
Critérios de aceite:
Pendências:
Decisões aprovadas por:


PARTE B — MAPEAMENTO TÉCNICO DA IA

Código técnico do módulo:
Descrição técnica:
Código dos recursos:
Códigos das permissões:
Matriz de permissões:
Schema oficial:
Tabelas existentes:
Tabelas novas:
Entidades compartilhadas:
Raiz do agregado:
Identificador público:
Estratégia de exclusão lógica:
DbContext responsável:
Tabelas excluídas de migrations:
Migration estrutural:
Migration do catálogo de Segurança:
Unidade transacional:
Endpoints:
Rotas frontend:
Componentes de autorização:
Estratégia de auditoria:
Testes:
Riscos técnicos:
Divergências encontradas:
```

---

## 6. Descoberta Técnica Interna (Uso Exclusivo da IA)

Abaixo, a checklist técnica para a IA investigar sem transformar em perguntas ao usuário. A IA deve consultar caminhos de referência registrados na seção 1 e investigar:

- [ ] Documentação oficial do banco e Schemas
- [ ] Tabelas existentes, entidades, relacionamentos e chaves
- [ ] Utilização de `public_id`, `deleted_at`
- [ ] Ownership do DbContext e uso de `ExcludeFromMigrations`
- [ ] Migrations existentes
- [ ] Unidade transacional
- [ ] Dados sensíveis a mascarar
- [ ] Módulos e recursos existentes no Catálogo
- [ ] Convenção das permissões (para módulos com múltiplos recursos)
- [ ] Padrões de backend e frontend existentes
- [ ] Auditoria (funcional vs segurança)
- [ ] Testes automatizados e mocks
- [ ] Não adicionar bibliotecas sem necessidade
- [ ] Não criar novas tabelas sem verificar o banco existente

---

## 7. Matriz de Permissões

A matriz deve ser produzida pela IA de forma técnica a partir das respostas simples.

*O usuário informa:* “O atendente pode consultar e cadastrar apólices. Somente o gestor pode alterar ou cancelar.”
*A IA mapeia e propõe aprovação para:*
- `apolices.visualizar`
- `apolices.inserir`
- `apolices.alterar`
- `apolices.cancelar`

**Modelo Técnico da Matriz:**

| Módulo | Recurso | Ação | Código da permissão | Descrição | Endpoint | Tela/Ação frontend |
|--------|---------|------|---------------------|-----------|----------|---------------------|
| Módulo | Recurso | Ação | mod.rec.acao | Exemplo | GET /api/... | Tela ou botão |

---

## 8. Convenção de Nomes das Permissões

As permissões do WebApólice devem seguir o padrão oficial do projeto:

**Quando existir um único recurso principal, utilizar:**
`recurso.acao`

Exemplos:
- `clientes.visualizar`
- `clientes.inserir`
- `apolices.alterar`

**Quando o módulo possuir vários recursos diferentes, poderá ser utilizado:**
`modulo.recurso.acao`

Exemplos:
- `financeiro.pagamentos.visualizar`
- `financeiro.recebimentos.alterar`

A IA deverá propor a convenção na matriz de permissões e solicitar aprovação antes da migration.

- Manter o uso de `.alterar` e **não** `.editar`.
- O fato do código da rota no frontend ser `/editar` não altera o código interno oficial da permissão.

---

## 9. Regras de Migrations

- **Migration estrutural do domínio:** Cria ou altera tabelas que realmente pertencem ao novo módulo de negócio. (Não gerar migration estrutural sem necessidade comprovada).
- **Migration do catálogo de Segurança:** Cadastra apenas registros base de controle (módulo, recursos, permissões e vínculos aprovados em `WebApolice.Modulos.Seguranca`).
- **Mapeamento de tabela existente:** Quando a tabela já existe no banco, a IA deverá avaliar se é necessário apenas mapeá-la, não criando ou alterando tabela automaticamente. Essas decisões devem ser apresentadas em linguagem simples. *(Exemplo: “Identificamos que a tabela de apólices já existe no banco oficial. Portanto, não será criada uma nova tabela; apenas faremos o mapeamento no módulo.”)*

---

## 10. Checklist de Implementação Backend / Frontend

*(Estes itens devem compor o plano final submetido à aprovação).*

**Backend:**
- Entidade, Configuração EF Core, DbSet
- Repository, Caso de uso, Request/DTO, Controller, Rota
- Permission constant definida
- Atributo de autorização e Validação do módulo
- Auditoria (funcional ou administrativa)
- Migrations adequadas (Separar Estrutura x Catálogo Segurança)
- Injeção de dependência e Tratamentos
- Testes unitários/integração

**Frontend:**
- Tipos TypeScript, Cliente HTTP, Hooks
- Telas de Listagem, Cadastro, Detalhes, Edição
- Paginação, Loading, Erros, Estado vazio
- Atualização do Menu e Rotas Protegidas
- Ocultação via botões condicionais e ações
- Temas, Responsividade, Lint, Typecheck, Build
- Reutilizar o Design System (ex: módulo Clientes). Não adicionar bibliotecas novas.

---

## 11. Testes e Homologação

**Testes Obrigatórios (Backend e Frontend):**
Validar usuário com permissão, sem permissão, administrador (acesso_total), módulo desabilitado, inativos, falhas de schema. Frontend deve validar visibilidade de menus e travamento de rotas/botões baseados na carga de `/api/seguranca/me`.

**Checklist Homologação Manual:**
- [ ] Módulo habilitado e desabilitado (trava navegação).
- [ ] Perfil com acesso completo vs somente leitura vs sem acesso.
- [ ] Usuário sem perfil e Usuário inativo.
- [ ] Menu, rotas e botões exibidos corretamente.
- [ ] Proteção direta em API (Postman/cURL).
- [ ] Auditoria (quando requisitada).
- [ ] Aspectos visuais (Temas, Responsividade).

---

## 12. Fluxo Obrigatório de Implementação

Siga explicitamente esta sequência. A IA **não deverá iniciar código** antes da aprovação da Fase 1, e não deve iniciar implementação enquanto houver decisões pendentes.

**FASE 1 — Descoberta**
- Ler o guia.
- Fazer perguntas simples (Parte A).
- Realizar a descoberta técnica (Parte B).
- Preencher a ficha (A e B), a matriz e apresentar plano. Aguardar aprovação explícita.

**FASE 2 — Banco e Catálogo**
- Modelagem, mapeamento ou migration estrutural.
- Migration do catálogo (módulo, recursos, permissões).

**FASE 3 — Backend**
- Casos de uso, endpoints, auditoria funcional e testes.

**FASE 4 — Frontend**
- Integrações, telas, proteção visual e ações.

**FASE 5 — Homologação e Documentação**
- Execução de cenários, atualizações de Readmes e aprovação final.

---

## 13. Exemplo Ilustrativo de Apólices

> **EXEMPLO NÃO DEFINITIVO — NÃO REPRESENTA AS REGRAS REAIS DO MÓDULO DE APÓLICES.**

**Pergunta (IA):** “O que os usuários poderão fazer?”
**Resposta (Usuário):** “Consultar, cadastrar, alterar e inativar apólices.”

**Proposta técnica da IA (A ser aprovada):**
- `apolices.visualizar`
- `apolices.inserir`
- `apolices.alterar`
- `apolices.inativar`

**Pergunta (IA):** “Quem poderá realizar cada ação?”
**Resposta (Usuário):** “O atendente consulta e cadastra. O gestor também altera e inativa.”

**Matriz Técnica Ilustrativa Proposta pela IA:**

| Módulo | Recurso | Ação | Código | Descrição | Endpoint | Tela |
|---|---|---|---|---|---|---|
| Apólices | Apólices | Visualizar | `apolices.visualizar` | Lê apólices | GET /api/apolices | Lista/Detalhes |
| Apólices | Apólices | Inserir | `apolices.inserir` | Cria apólices | POST /api/apolices | Cadastro |
| Apólices | Apólices | Alterar | `apolices.alterar` | Edita apólices | PUT /api/apolices | Edição |
| Apólices | Apólices | Inativar | `apolices.inativar` | Inativa apólice | POST /api/apolices/inativar | Botão Inativar |

---

## 14. Prompt Reutilizável

Ao iniciar uma nova interação com a IA de Desenvolvimento para construir o novo módulo, o usuário colará o prompt abaixo:

> “Vamos iniciar a definição de um novo módulo do WebApólice.
> 
> Leia integralmente o Guia Oficial para Implementação de Novos Módulos.
> 
> Primeiro, faça somente as perguntas de negócio destinadas ao responsável pelo módulo em linguagem simples. Apresente todas juntas, organizadas por assunto.
> 
> Não faça perguntas sobre schemas, tabelas, migrations, DbContext, policies ou componentes técnicos. Essas informações deverão ser investigadas por você na documentação e nos arquivos de referência.
> 
> Ajude o usuário quando ele não souber responder. Não aceite 'não sei' como definição final, investigue internamente as informações técnicas e apresente uma recomendação para aprovação.
> 
> Depois das respostas:
> 
> 1. preencha a Ficha de Definição de Negócio;
> 2. realize a descoberta técnica;
> 3. preencha o Mapeamento Técnico;
> 4. crie a matriz de permissões;
> 5. apresente o plano de implementação;
> 6. informe dúvidas ou divergências;
> 7. aguarde aprovação antes de codificar.
> 
> Não invente requisitos e não inicie a implementação antes da aprovação.”
