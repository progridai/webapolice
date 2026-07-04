# WebApolice - ERP Web

## Descrição Resumida
O **WebApolice** é a nova versão web e responsiva do sistema de ERP, concebida para substituir progressivamente o sistema ERP legado em VB.NET com banco SQL Server. Ele servirá como o motor centralizado de dados e processos, operando de forma 100% web.

## Requisitos Locais e Padronização de Versões

### Node.js (Frontend)
* **Versão Oficial**: Node.js **24 LTS** e npm **11**. Novas versões major precisam ser validadas formalmente antes da adoção.
* **Controle de Versão**:
  * Os arquivos [.nvmrc](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/.nvmrc) e [.node-version](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/.node-version) na raiz especificam a versão major `24`.
  * Ferramentas de gerenciamento de versão de Node (como `nvm` ou `fnm`) detectam automaticamente esses arquivos para alternar para a versão oficial local.
* **Comando de Verificação**:
  ```bash
  node --version
  ```

### .NET SDK (Backend)
* **Versão Oficial**: .NET SDK **10.0.301** (ou superior da mesma versão major).
* **Pino de SDK (global.json)**:
  * O arquivo [global.json](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/global.json) na raiz garante que toda a equipe de engenharia e os pipelines de integração contínua (CI) utilizem a mesma versão base do SDK .NET 10.
  * A política de `rollForward` está definida para `latestPatch`, aceitando atualizações corretivas e patches compatíveis de segurança, sem permitir a alteração silenciosa de versão major ou minor.
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

### Infraestrutura Local (Docker Compose)
Para inicializar os serviços locais de apoio (PostgreSQL e Keycloak) necessários para o projeto:
1. Copie o arquivo `.env.example` para `.env` na raiz do projeto e ajuste as senhas locais.
2. Navegue até a pasta `infrastructure/` e execute:
   ```bash
   docker compose --env-file ../.env up -d
   ```
Para detalhes completos de inicialização, logs, volumes e conectividade externa de banco, consulte a documentação em [infrastructure/README.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/infrastructure/README.md).

#### Provisionamento e Auditoria do Keycloak
Após inicializar os containers e garantir que o Keycloak está saudável:
1. **Provisionamento Automatizado**: Aplique as configurações declarativas do realm `webapolice`, clients OIDC, roles globais e o usuário administrativo de desenvolvimento executando:
   ```bash
   docker compose --env-file ../.env exec keycloak bash /opt/keycloak/keycloak_shared/scripts/configure-realm.sh
   ```
2. **Auditoria de Segurança**: Para validar se as configurações locais cumprem todas as regras de segurança exigidas (como PKCE obrigatório S256, Standard Flow ativo e fluxos inseguros desativados), execute:
   ```bash
   docker compose --env-file ../.env exec keycloak bash /opt/keycloak/keycloak_shared/scripts/validate-realm.sh
   ```

> [!IMPORTANT]
> **Integração do Frontend Pendente**:
> O banco de dados e o Keycloak estão provisionados, e a API Backend (ASP.NET Core) já está **totalmente integrada** ao Keycloak para validação de JWT (Bearer), Audience e RBAC (Role-Based Access Control). No entanto, a integração funcional do *Frontend React* com o fluxo PKCE e persistência ocorrerá nas próximas fases de desenvolvimento.

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

Navegue para o diretório do backend:
```bash
cd backend
```

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

## Status da Fundação Técnica e Módulos de Negócio
A fundação técnica inicial e o primeiro módulo de negócio foram finalizados com êxito:
* Frontend e backend utilizam as versões estáveis mais recentes das tecnologias de base (React 19, Vite 8, .NET 10).
* Endpoints técnicos `/api/health`, `/api/health/live` e `/api/health/ready` encontram-se disponíveis no backend.
* A fundação de Persistência com PostgreSQL 18.4 e Entity Framework Core está completa, com isolamento transacional local via `ClientesTransactionManager` (sem MSDTC, 100% suportado no Linux/Docker).
* **Módulo Clientes (Backend)**: Totalmente implementado com regras estritas de domínio, concorrência otimizada, paginação dinâmica na base, proteção de dados pessoais (CPF mascarado) e matriz de autorização via JWT/Keycloak.
* **Testes de Arquitetura**: Os testes arquiteturais em `WebApolice.Architecture.Tests` garantem o fluxo unidirecional e isolamento dos módulos.
* **Bateria de Testes**: Suíte robusta com 147 testes (100% aprovados) executados localmente e em ambiente Linux nativo (Docker SDK 10).

> [!NOTE]
> **Identidade Visual e Design System Reutilizável (Concluído)**:
> A identidade visual oficial, os design tokens com suporte a temas claro/escuro/sistema e o catálogo completo de componentes UI reutilizáveis estão totalmente implementados e homologados. Os 16 componentes (`Button`, `FormField`, `Input`, `Textarea`, `Select`, `Checkbox`, `Alert`, `Spinner`, `Skeleton`, `Card`, `Badge`, `EmptyState`, `Modal`, `ConfirmDialog`, `Table`, `Pagination`) estão disponíveis via barril `src/components/ui`. O script `npm run lint:design-system` verifica continuamente a conformidade de tokens (zero cores hex/rgb fixadas nos componentes). O catálogo visual está disponível em `/#design-system`.
> 
> - [docs/12-identidade-visual-design-system.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/12-identidade-visual-design-system.md) — Guia de marca e tokens
> - [docs/13-componentes-design-system.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/13-componentes-design-system.md) — Catálogo de componentes e regras de uso

---

## Referência aos ADRs (Architectural Decision Records)
As escolhas estruturais do projeto estão justificadas e documentadas em:
* [ADR-001: Estrutura do Repositório](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-001-estrutura-do-repositorio.md) (Uso de monorepo compartilhado)
* [ADR-002: Frontend React e Vite](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-002-frontend-react-vite.md) (Uso de SPA com empacotador veloz e postergação do Design System)
* [ADR-003: Backend ASP.NET Core](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-003-backend-aspnet-core.md) (ASP.NET Core como centralizador de regras e integrações)
* [ADR-004: Monólito Modular](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-004-monolito-modular.md) (Divisão lógica de fronteiras e controle de acoplamento)
* [ADR-005: Infraestrutura Local com Docker Compose](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-005-infraestrutura-local-docker.md) (Uso de Docker Compose para PostgreSQL e Keycloak local)
* [ADR-006: Keycloak como Provedor Central de Identidade](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/adr/ADR-006-keycloak-identidade-central.md) (Uso do Keycloak local, fluxo PKCE, clients OIDC e provisionamento automatizado)
