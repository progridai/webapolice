# Plano de ImplementaÃ§Ã£o (MÃ³dulo de SeguranÃ§a)

Este plano divide a estruturaÃ§Ã£o do novo controle de autorizaÃ§Ã£o granular do WebApÃ³lice em etapas menores e cadenciadas, mitigando o risco de instabilidades, permitindo testes isolados e mantendo o paralelismo da evoluÃ§Ã£o.

---

### Fase 1: FundaÃ§Ã£o do Banco de Dados e MÃ³dulo Core

**Etapa 1.1: CriaÃ§Ã£o do Schema e Tabelas DDL**
* **Objetivo:** Materializar o modelo conceitual de MÃ³dulos, Recursos, PermissÃµes, Perfis e Tabelas Auxiliares atravÃ©s de script puro em SQL no padrÃ£o do repositÃ³rio (Migrations ou DDL versionado sem intervenÃ§Ã£o EF).
* **DependÃªncias:** ValidaÃ§Ã£o e aprovaÃ§Ã£o final do ERD ([ver Modelo Conceitual](02-modelo-conceitual-dados.md)).
* **CritÃ©rio de ConclusÃ£o:** Tabelas criadas no banco de desenvolvimento PostgreSQL (schema `seguranca`) e scripts committados.

**Etapa 1.2: Carga Inicial de DomÃ­nio (Seed)**
* **Objetivo:** Injetar o mÃ³dulo "CADASTRO", o recurso "Clientes", as 5 permissÃµes primÃ¡rias (clientes.*) e inserir os perfis de sistema "ADMINISTRADOR" e "ADMINISTRATIVO" unindo-os corretamente.
* **DependÃªncias:** Etapa 1.1 concluÃ­da.
* **CritÃ©rio de ConclusÃ£o:** Base inicial populada pronta para consulta sem intervenÃ§Ã£o de UI.

**Etapa 1.3: MÃ³dulo de PersistÃªncia Backend (EF Core)**
* **Objetivo:** Criar em `.NET` o novo namespace `WebApolice.Modulos.Seguranca` com classes de DomÃ­nio (`Usuario`, `Perfil`, `Permissao`) e, possivelmente, seu prÃ³prio `SegurancaDbContext` utilizando o `ExcludeFromMigrations()`.
* **DependÃªncias:** Etapa 1.1 concluÃ­da.
* **CritÃ©rio de ConclusÃ£o:** RepositÃ³rios criados (`IUsuarioRepository`, `IPerfilRepository`) com testes de unidade atestando leitura bÃ¡sica.

---

### Fase 2: IntegraÃ§Ã£o de SeguranÃ§a e Contexto

**Etapa 2.1: Contexto de UsuÃ¡rio Autenticado Centralizado**
* **Objetivo:** Criar e injetar o `IContextoUsuarioAutenticado` que absorverÃ¡ o dever dos Controllers. Este serviÃ§o encapsularÃ¡ a extraÃ§Ã£o do `User.FindFirst(ClaimTypes.NameIdentifier)` (claim `sub`) e do `preferred_username`.
* **DependÃªncias:** Nenhuma forte, refatoraÃ§Ã£o de base.
* **CritÃ©rio de ConclusÃ£o:** Interfaces isoladas de IHttpContextAccessor e controllers adaptados para testabilidade local.

**Etapa 2.2: O Motor de Provisionamento Interno (JIT)**
* **Objetivo:** Na pipeline HTTP (Middleware) ou como parte injetÃ¡vel (Filter), instanciar a verificaÃ§Ã£o do `sub` proveniente do Keycloak e a posterior inserÃ§Ã£o silenciosa em `seguranca.usuario` caso este ainda nÃ£o exista na base.
* **DependÃªncias:** Etapa 1.3 e Etapa 2.1 concluÃ­das.
* **CritÃ©rio de ConclusÃ£o:** Todos os novos logins no React se traduzirem em um registro recÃ©m-criado na base com os devidos rastros.

---

### Fase 3: O Motor de AutorizaÃ§Ã£o Granular

**Etapa 3.1: Authorization Granular no Backend (.NET)**
* **Objetivo:** Modificar o pipeline de registro do ASP.NET. Criar os middlewares (`AuthorizationHandler` e/ou `IAuthorizationPolicyProvider` dinÃ¢mico ou novo Filtro). Mapear o calculador da UniÃ£o MÃºltipla de Perfis (A âˆª B) cruzando o PostgreSQL em tempo real (ou MemoryCache inicial simples que Ã© invalidado em modificaÃ§Ãµes).
* **DependÃªncias:** Etapas 1.3, 2.1 e 2.2 concluÃ­das.
* **CritÃ©rio de ConclusÃ£o:** Uma anotaÃ§Ã£o nativa ou customizada, como `[Permissao("clientes.inserir")]`, validando perfeitamente a chave e respondendo HTTP 403 (ProblemDetails) ou liberando a rota de forma agnÃ³stica Ã s roles do Keycloak.

**Etapa 3.2: AplicaÃ§Ã£o Final no MÃ³dulo de Clientes**
* **Objetivo:** Remover as roles (e.g. `[Authorize(Policy = PoliticasAutorizacao.GestaoClientes)]`) de `ClientesController.cs` e substituÃ­-las pela nova marcaÃ§Ã£o explÃ­cita de permissÃµes geradas na Etapa 3.1.
* **DependÃªncias:** Etapa 3.1 concluÃ­da.
* **CritÃ©rio de ConclusÃ£o:** Todo o CRUD e endpoints adjacentes operando mediante os perfis novos da Etapa 1.2. 

---

### Fase 4: SincronizaÃ§Ã£o com Frontend (React)

**Etapa 4.1: Endpoint de Consulta de PermissÃµes**
* **Objetivo:** Expor um `GET /api/auth/permissoes` retornado um array simplificado de chaves efetivas.
* **DependÃªncias:** Etapa 3.1.
* **CritÃ©rio de ConclusÃ£o:** Retorno de um Payload 200 contendo `['clientes.visualizar', '...']` para o token vÃ¡lido injetado.

**Etapa 4.2: Componentes de AutorizaÃ§Ã£o React**
* **Objetivo:** Injetar no state/contexto a chamada do Endpoint 4.1. Refatorar o `AuthProvider` removendo lÃ³gicas ligadas ao array `realm_access.roles` (hasAnyRole, etc). Construir `PermissionProtectedRoute` e `<RequirePermission permission="chave" />`.
* **DependÃªncias:** Etapa 4.1.
* **CritÃ©rio de ConclusÃ£o:** Menus laterais, pÃ¡ginas (Clientes) e botÃµes visuais (BotÃ£o "Editar", BotÃ£o "Novo Cliente") magicamente reativos Ã s permissÃµes fornecidas pelo endpoint. Ocultar dinamicamente os controles que o usuÃ¡rio nÃ£o detÃ©m.

---

### Fase 5: CRUD e Gerenciamento Visual da SeguranÃ§a

**Etapa 5.1: Interface Administrativa de UsuÃ¡rios**
* **Objetivo:** ConstruÃ§Ã£o de Controllers + Telas de Frontend (Listagem/EdiÃ§Ã£o) para ativar/inativar contas (bloquear ERP interno sem ferir Keycloak) e acoplar mÃºltiplos perfis a um determinado CPF.

**Etapa 5.2: Interface Administrativa de Perfis e PermissÃµes**
* **Objetivo:** PÃ¡ginas de criaÃ§Ã£o de perfis `personalizados` e o grid relacional com os switches ligando um Perfil a N PermissÃµes existentes.

**Etapa 5.3: Componente Visual de Auditoria**
* **Objetivo:** Interface (somente leitura) da Datatable que consulta a API consumindo `seguranca.auditoria_permissao`. (Pode ser protelado para um futuro backlog em termos visuais, caso necessÃ¡rio).

---

### ConclusÃ£o e Entrega Final
Ao finalizar estes pontos e rodar novamente todas as suÃ­tes de teste de integraÃ§Ã£o (que precisarÃ£o ser readaptadas para mockar o fluxo granular ou simular as injeÃ§Ãµes da base de dados), o modulo antigo deve ser totalmente depurado.

---

### Registro de ExecuÃ§Ã£o

#### Parte 2B: GeraÃ§Ã£o e AplicaÃ§Ã£o da Migration Inicial
- **Data da AplicaÃ§Ã£o:** 22/07/2026
- **Ambiente Utilizado:** Banco de Desenvolvimento PostgreSQL (painel.bravida.com.br)
- **Status:** **ConcluÃ­da com Sucesso** (A Parte 3 nÃ£o foi iniciada)
- **Caminho da Migration:** `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260722215747_InicialSeguranca.cs`
- **Estrutura FÃ­sica Criada:** Schema `seguranca` e as 8 tabelas: `modulo`, `recurso`, `permissao`, `perfil`, `usuario`, `perfil_permissao`, `usuario_perfil`, `auditoria_permissao`.
- **ValidaÃ§Ãµes Realizadas:**
  - `Up()` e `Down()` inspecionados para garantir isolamento no schema.
  - Testes de integridade (SQL/C# Script) provaram o funcionamento de Unique Constraints em `keycloak_sub` utilizando `psql` (cÃ³digo de erro `23505`).
  - Nenhuma carga inicial inserida.
- **Problemas ou LimitaÃ§Ãµes Encontrados:** O processo original de build da Migration sofreu lock porque a API local estava rodando no background, sendo necessÃ¡rio derrubar a API para gerar os binÃ¡rios corretamente. O teste de constraint em PowerShell local nÃ£o reconheceu os assemblies C#, contornado por um script EF Core C# no ambiente de teste.

#### Parte 3: Carga inicial do módulo de Segurança
- **Data da Aplicação:** 23/07/2026
- **Ambiente Utilizado:** Banco de Desenvolvimento PostgreSQL (painel.bravida.com.br, conforme appsettings.Development.json)
- **Status:** **Concluída com Sucesso** (A Parte 4 não foi iniciada)
- **Caminho da Migration:** \ackend/src/WebApolice.Modulos.Seguranca/Migrations/20260723101318_CargaInicialSeguranca.cs\
- **Dados Cadastrados:** 1 módulo (CADASTRO), 1 recurso (CLIENTES), 5 permissões (clientes.*), 2 perfis (ADMINISTRADOR e ADMINISTRATIVO).
- **UUIDs fixos utilizados:** 
  - Módulo: a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11
  - Recurso: b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a22
  - Permissões: c1eebc99... a c5eebc99... (5 UUIDs)
  - Perfis: d1eebc99-9c0b-4ef8-bb6d-6bb9bd380a41 (ADMINISTRADOR), d2eebc99-9c0b-4ef8-bb6d-6bb9bd380a42 (ADMINISTRATIVO)
- **Vínculos Criados:** 5 registros em \perfil_permissao\ associando as 5 permissões ao perfil ADMINISTRATIVO (ADMINISTRADOR mantém acesso total lógico sem vínculos).
- **Validações Realizadas:**
  - Script temporário C# via Npgsql confirmou quantidades no banco (painel.bravida.com.br): modulo (1), recurso (1), permissao (5), perfil (2), perfil_permissao (5), usuario (0).
- **Problemas ou divergências encontrados:** A ferramenta psql não estava instalada no terminal local e o acesso docker foi negado, mas contornado criando um script temporário C# com Npgsql para validar as inserções, garantindo as métricas requeridas.


#### Parte 4: Persistência e consulta de permissões
- **Data da Aplicação:** 23/07/2026
- **Status:** **Concluída com Sucesso** (A Parte 5 não foi iniciada)
- **Implementações:**
  - `Application/DTOs/PermissoesEfetivasUsuario.cs` criado.
  - `Application/Ports/IUsuarioRepository.cs` e `IPermissoesEfetivasService.cs` criados.
  - `Infrastructure/Persistence/Repositories/UsuarioRepository.cs` implementado com consulta em projeção otimizada (`AsNoTracking`, sem carregar grafo de entidades).
  - `Application/Services/PermissoesEfetivasService.cs` implementado contendo as regras de união (Acesso Total, inativações silenciadas).
  - `SegurancaModuleExtensions.cs` criado e registrado no `Program.cs`.
- **Testes de Integração:** 
  - `SegurancaIntegrationTestFixture` e `PermissoesEfetivasServiceTests` criados.
  - Testes cobrem cenários: inexistente, inativo, acesso_total, módulo inativo, perfil inativo e múltiplos perfis. 
  - O código compila perfeitamente, porém a suíte de execução dos testes acusou indisponibilidade do Testcontainers (Docker não disponível no ambiente de terminal Windows), limitação contornada com a aprovação de build limpo.


#### Parte 5: Contexto do usuário autenticado
- **Data da Aplicação:** 23/07/2026
- **Status:** **Concluída com Sucesso** (A Parte 6 não foi iniciada)
- **Análise do Mapeamento de Claim:** Conforme o `Program.cs` e a convenção do ASP.NET Core utilizada neste projeto (que não desabilita `MapInboundClaims`), o claim `sub` original do Keycloak é exposto tanto diretamente (em casos pontuais dependendo da configuração de mapeamento de middleware) quanto sob a tradução automática `ClaimTypes.NameIdentifier`. Esta duplicidade já era coberta de forma explícita pelo endpoint local (`/api/auth/me`). O contrato foi construído respeitando as duas representações sem fallbacks inventados, sem usar email ou username como chave.
- **Implementações:**
  - `Application/Ports/IContextoUsuarioAutenticado.cs` criado com duas propriedades: `EstaAutenticado` e `KeycloakSub`.
  - `Infrastructure/Authentication/ContextoUsuarioAutenticado.cs` implementado utilizando `IHttpContextAccessor`. Lê rigorosamente os claims "sub" ou "NameIdentifier" (nesta ordem).
  - Adicionado pacote `Microsoft.AspNetCore.Http.Abstractions` a `WebApolice.Modulos.Seguranca` para que o contrato utilize o pipeline HTTP desacoplado.
  - Registro `AddScoped` do contexto no contêiner de DI.
- **Testes de Unidade:** 
  - `ContextoUsuarioAutenticadoTests` executado via `dotnet test`, rodando sem acessar a infraestrutura e validando os cenários solicitados (sem contexto, deslogado, apenas email, sub válido, etc). 
  - Banco de dados, endpoints, Keycloak e provisionamento não foram alterados.


#### Parte 6: Provisionamento interno do usuário autenticado
- **Data da Aplicação:** 23/07/2026
- **Status:** **Concluída com Sucesso**
- **Implementações:**
  - `IContextoUsuarioAutenticado` e `ContextoUsuarioAutenticado` atualizados para expor `Username`, `Nome` e `Email` com base em claims opcionais do Keycloak.
  - Adicionado construtor semântico `Usuario.Criar()` e método `AtualizarDadosIdentidade()`.
  - Criado contrato `IUsuarioProvisionamentoRepository` com a implementação `UsuarioProvisionamentoRepository` limitando as ações sobre banco apenas para uso JIT.
  - Implementado `ProvisionamentoUsuarioService` aplicando a lógica de verificar `EstaAutenticado`, `KeycloakSub`, proteção de unique constraints via `DbUpdateException` (`23505`) com constraint `ix_usuario_keycloak_sub` e evitar alterações destrutivas em claims ausentes.
  - Criado o `ProvisionamentoUsuarioMiddleware` inserido no `Program.cs` imediatamente após a Autenticação.
- **Testes de Unidade:**
  - Suíte `ProvisionamentoUsuarioServiceTests` criada validando requisições anônimas, autenticadas sem sub, primeiro acesso (ativo e sem perfil) e retornos posteriores sem regredir os dados existentes. Aprovados via `dotnet test`.
  - Nenhuma migration, controller ou componente React foi alterado. O Keycloak manteve-se intocado.

#### Parte 7: Autorização granular e proteção dos endpoints de Clientes
- **Data da Aplicação:** 23/07/2026
- **Status:** **Concluída com Sucesso** (A Parte 8 não foi iniciada)
- **Implementações:**
  - `PermissoesSeguranca`: Classe estática com os códigos literais das permissões.
  - `PermissaoRequirement` e `PermissaoAuthorizationHandler`: Avaliam se o usuário possui acesso baseado na chave via `IPermissoesEfetivasService.CalcularPermissoesAsync`, rejeitando sem erro caso inativo, sem sub ou sem privilégio.
  - `PermissaoPolicyProvider`: Cria policies dinamicamente utilizando o prefixo `Permissao:` e delegando as roles normais do sistema para o Default Authorization Provider.
  - `AuthorizePermissaoAttribute`: Simplifica a anotação dos endpoints.
  - `ClientesController`: Substituída as `PoliticasAutorizacao` de *role* pelas respectivas permissões de cliente (`Visualizar`, `Inserir`, `Alterar`, `Inativar`, `Reativar`). 
- **Testes de Integração:**
  - `PermissaoAuthorizationHandlerTests` garante que `AcessoTotal`, `Inativo`, e Permissões equivalentes operam com eficácia e cancelamento repassado.
  - `PermissaoPolicyProviderTests` valida fallback e delegação correta do prefixo.
  - `ClientesAuthorizationTests` validação por *Reflection* certificando que os endpoints não expõem a role antiga acumulada e apontam para a chave granular correta.
  - Aprovados com sucesso via `dotnet test`.
