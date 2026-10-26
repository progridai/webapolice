# WebApolice - ERP Web

Revisão técnica atual, de 23/10/2026: consulte a [análise de arquitetura, segurança, qualidade e documentação](docs/76-revisao-arquitetura-seguranca-qualidade.md). Ela registra os achados pendentes e o alcance das validações atuais; afirmações históricas de conclusão abaixo não substituem essa avaliação.

## Descrição Resumida
O **WebApolice** é a nova versão web e responsiva do sistema de ERP, concebida para substituir progressivamente o sistema ERP legado em VB.NET com banco SQL Server. Ele servirá como o motor centralizado de dados e processos, operando de forma 100% web.

## Requisitos Locais e Padronização de Versões

Para configurar sua máquina rapidamente, utilize os comandos recomendados abaixo.

### Node.js (Frontend)
* **Versão Oficial**: Node.js **24 LTS** e npm **11**. Novas versões major precisam ser validadas formalmente antes da adoção.
* **Instalação (Winget)**:
  ```powershell
  winget install OpenJS.NodeJS.LTS
  ```
* **Controle de Versão**:
  * Os arquivos `.nvmrc` e `.node-version` na raiz especificam a versão major `24`.
  * Ferramentas de gerenciamento de versão de Node (como `nvm` ou `fnm`) detectam automaticamente esses arquivos para alternar para a versão oficial local.
* **Comando de Verificação**:
  ```bash
  node --version
  ```

### .NET SDK (Backend)
* **Versão Oficial**: .NET SDK **10.0.400** (ou superior da mesma versão major).
* **Instalação (Winget)** *(abra o PowerShell como Administrador)*:
  ```powershell
  winget install Microsoft.DotNet.SDK.10
  ```
* **Pino de SDK (global.json)**:
  * O arquivo `global.json` na raiz garante que toda a equipe e os pipelines de CI utilizem a versão correta do SDK.
* **Comando de Verificação**:
  ```bash
  dotnet --version
  ```

---

## Estrutura Técnica do Repositório
O repositório está organizado sob um monorepo lógico:
```text
webapolice/
├── apps/
│   └── web/                   # Frontend React 19 + TypeScript + Vite 8
├── backend/                   # Solução backend .NET 10 LTS
│   ├── src/
│   │   ├── WebApolice.Api/    # Web API ASP.NET Core (Minimal APIs)
│   │   ├── WebApolice.SharedKernel/ # Biblioteca de tipos comuns transversais
│   │   ├── WebApolice.Shared.Infrastructure/ # Infraestrutura compartilhada transversa (e segurança)
│   │   └── WebApolice.Modulos.Clientes/ # Módulo de negócio Clientes (Backend)
│   └── tests/
│       ├── WebApolice.Api.Tests/ # Testes de integração da API e autorização Keycloak
│       ├── WebApolice.Architecture.Tests/ # Testes arquiteturais por reflexão (NetArchTest)
│       └── WebApolice.Modulos.Clientes.Tests/ # Testes de domínio e caso de uso do módulo Clientes
├── docs/                      # Documentação técnica e estratégica
│   └── adr/                   # Registros de Decisão de Arquitetura (ADRs)
└── prompts/                   # Templates de prompts padronizados para IA
```

---

## Comandos para Execução e Testes

### Infraestrutura (Nuvem)
Os serviços de apoio (PostgreSQL e Keycloak) estão hospedados na nuvem, portanto **não é necessário** rodar contêineres locais via Docker Compose.
Certifique-se de que o seu arquivo `.env` (ou `.env.local` do backend) está configurado corretamente com as credenciais e conexões da nuvem.

> [!IMPORTANT]
> **Integração Frontend/API**:
> O frontend React está integrado ao Keycloak via Authorization Code + PKCE e consome a API com token Bearer pelo cliente HTTP centralizado. Em desenvolvimento local, use `http://127.0.0.1:5173` para o frontend, `http://localhost:5007` para a API e a URL da nuvem para o Keycloak.

### Configuração Local do Frontend e API

As variáveis `VITE_*` ficam no `.env` da raiz e são lidas na inicialização do Vite. Após alterá-las, reinicie `npm run dev`.

```env
VITE_API_BASE_URL=http://localhost:5007
VITE_KEYCLOAK_URL=https://auth.bravida.com.br
VITE_KEYCLOAK_REALM=webapolice
VITE_KEYCLOAK_CLIENT_ID=webapolice-web
VITE_ENABLE_DESIGN_SYSTEM=true
```

A API permite CORS para as origens locais configuradas em `Cors:FrontendOrigins` no `appsettings.Development.json`. A listagem de Clientes usa `GET /api/clientes`.

Se aparecer `Failed to fetch`, verifique: API em execução, `VITE_API_BASE_URL`, reinício do Vite, CORS/preflight e protocolo HTTP/HTTPS.

### Frontend (`apps/web/`)

Navegue para o diretório do frontend:
```bash
cd apps/web
```

* **Instalação de Dependências**:
  ```bash
  npm install
  ```
* **Executar em Desenvolvimento**:
  ```bash
  npm run dev
  ```
* **Verificação de Tipos (TypeScript)**:
  ```bash
  npm run typecheck
  ```
* **Linting de Código**:
  ```bash
  npm run lint
  ```
* **Formatação (Prettier)**:
  ```bash
  npm run format:check
  ```
* **Executar Testes Unitários (Vitest)**:
  ```bash
  npm run test:run
  ```
* **Build de Produção**:
  ```bash
  npm run build
  ```

### Backend (`backend/`)

**Executar o backend localmente**

Para iniciar a API corretamente, o projeto exige a configuração das variáveis de ambiente com foco no isolamento das bases de testes e produção.

1. Crie o arquivo `backend/.env.local` e configure as seguintes chaves (substitua as senhas/hosts pelos valores fornecidos pela equipe de infraestrutura):
   ```env
   ASPNETCORE_ENVIRONMENT=Development
   ConnectionStrings__PostgreSqlProducao=Host=<host>;Port=5432;Database=webapolice;Username=<user>;Password=<senha>
   ConnectionStrings__PostgreSql=Host=<host>;Port=5432;Database=webapolice_teste;Username=<user>;Password=<senha>
   ConnectionStrings__PostgreSqlTestes=Host=<host>;Port=5432;Database=webapolice_teste;Username=<user>;Password=<senha>
   KeycloakAdmin__BaseUrl=https://auth.bravida.com.br/
   KeycloakAdmin__Realm=webapolice
   KeycloakAdmin__ClientId=webapolice-backend-admin
   KeycloakAdmin__ClientSecret=<seu_client_secret>
   Testes__PermitirBancoCompartilhado=true
   ```
2. Acesse a pasta do projeto e execute:
   ```cmd
   backend\run-api-local.bat
   ```
3. O script lerá automaticamente as credenciais e iniciará a API em segundo plano.

**Sobre o gerenciamento de configurações locais:**
* O arquivo `backend/.env.local` é pessoal e **nunca deve ser commitado** (já configurado no `.gitignore`).
* As chaves com `__` são mapeadas automaticamente para as configurações aninhadas no .NET.
* A separação dos nomes das connection strings organiza os destinos, mas não garante isolamento. As ferramentas exigem configuração externa explícita; conferir servidor, base e permissões antes de qualquer execução autorizada, conforme o documento 78.

**Outros comandos úteis (Diretório `backend/`):**

* **Restaurar Dependências NuGet**:
  ```bash
  dotnet restore WebApolice.slnx
  ```
* **Compilar Solução**:
  ```bash
  dotnet build WebApolice.slnx
  ```
* **Executar Bateria de Testes**:
  ```bash
  dotnet test WebApolice.slnx
  ```

---

## Estado Atual — Consolidação de 23/10/2026

A [revisão 76](docs/76-revisao-arquitetura-seguranca-qualidade.md) é o índice de achados, correções, testes e pendências. A stack e o monólito modular foram preservados; implementação e verificações locais não equivalem a homologação com o legado.

- Configurações operacionais são externas; audiência JWT é obrigatória, erros internos recebem resposta genérica e catálogo indisponível/inativo nega acesso, inclusive AcessoTotal.
- Clientes usam portas sem Infrastructure/EF em Application; alterações auditadas desse fluxo, Cooperados e Estipulantes usam o coordenador transacional existente. Outras dependências indevidas ainda são detectadas pelos testes arquiteturais e impedem declarar conformidade integral.
- O frontend obtém permissões de `/api/seguranca/me` por AuthorizationProvider. Roles do Keycloak permanecem nas políticas técnicas; não concedem automaticamente permissão de negócio no ERP.
- [Propostas](docs/77-consolidacao-propostas.md): vínculo Cliente/Pessoa e matriz aprovados, implementação em consolidação; código de status inicial e homologação integrada continuam pendentes. Regras financeiras serão definidas posteriormente.
- [Operação e validação](docs/78-operacao-e-validacao-consolidacao.md): gate local e workflow versionados, dependências corretivas e controles HTTP. Docker/Keycloak reais, implantação, restauração e execução remota do CI ainda precisam de evidência.

> [!NOTE]
> **Identidade Visual e Design System Reutilizável (Concluído)**:
> A identidade visual oficial, os design tokens com suporte a temas claro/escuro/sistema e o catálogo completo de componentes UI reutilizáveis estão implementados; validações locais e limitações de homologação estão na revisão 76. Os componentes (`Button`, `FormField`, `Input`, `Textarea`, `Select`, `Checkbox`, `Alert`, `Spinner`, `Skeleton`, `Card`, `Badge`, `EmptyState`, `Modal`, `ConfirmDialog`, `Table`, `Pagination`) estão disponíveis via barril `src/components/ui`. O script `npm run lint:design-system` verifica continuamente a conformidade de tokens (zero cores hex/rgb fixadas nos componentes). O catálogo visual está disponível em `/#design-system`.
> 
> - [docs/12-identidade-visual-design-system.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/12-identidade-visual-design-system.md) — Guia de marca e tokens
> - [docs/13-componentes-design-system.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/13-componentes-design-system.md) — Catálogo de componentes e regras de uso

> [!NOTE]
> **Modelagem de Banco de Dados e Domínio**:
> O projeto utiliza PostgreSQL como banco de dados oficial, com separação de responsabilidades por schemas e rastreabilidade legada.
> 
> - [docs/17-modelagem-banco-dados-webapolice.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/17-modelagem-banco-dados-webapolice.md) — Modelagem geral do banco de dados
> - [docs/18-modelagem-clientes-core-cadastro.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/18-modelagem-clientes-core-cadastro.md) — Modelagem de Clientes (Core e Cadastro)

---

## Referência aos ADRs (Architectural Decision Records)
As escolhas estruturais do projeto estão justificadas e documentadas em:
* [ADR-001: Estrutura do Repositório](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-001-estrutura-do-repositorio.md) (Uso de monorepo compartilhado)
* [ADR-002: Frontend React e Vite](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-002-frontend-react-vite.md) (Uso de SPA com empacotador veloz e postergação do Design System)
* [ADR-003: Backend ASP.NET Core](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-003-backend-aspnet-core.md) (ASP.NET Core como centralizador de regras e integrações)
* [ADR-004: Monólito Modular](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-004-monolito-modular.md) (Divisão lógica de fronteiras e controle de acoplamento)
* [ADR-005: Infraestrutura Local com Docker Compose](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-005-infraestrutura-local-docker.md) (Uso de Docker Compose para PostgreSQL e Keycloak local)
* [ADR-006: Keycloak como Provedor Central de Identidade](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-006-keycloak-identidade-central.md) (Uso do Keycloak local, fluxo PKCE, clients OIDC e provisionamento automatizado)

## Regra obrigatória de dados: Title Case

Todo texto humano deve ser normalizado em todas as entradas e na persistência. Campos técnicos e evidências históricas preservam suas regras. Consulte a [política geral](docs/26-padronizacao-title-case.md) e o `AGENTS.md` da raiz antes de implementar novos campos, módulos ou importadores.
