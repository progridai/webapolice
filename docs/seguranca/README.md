# Módulo de Segurança WebApólice

> **Aviso:** Este diretório documenta a especificação técnica do novo módulo de Segurança. A estrutura física do banco de dados (schema `seguranca` e as 8 tabelas base) **já foi criada e validada via Migrations (Parte 2)**, e **a carga inicial de dados já foi inserida (Parte 3)** no ambiente de desenvolvimento. Porém, as integrações, endpoints e regras de negócio **ainda não possuem implementação (Aguardando Parte 4)**.

## Objetivo

Este diretório concentra toda a documentação técnica, decisões arquiteturais, modelos de dados e fluxos de negócio relacionados ao futuro módulo de Segurança e Controle de Acesso Granular do WebApólice.

O objetivo do módulo é substituir a autorização atual — baseada nas Realm Roles fixas (`admin`, `gestor`, `operador`) — por um modelo dinâmico e flexível baseado em permissões, mantendo o controle centralizado e seguro.

## Índice

1. [Visão Geral e Decisões](01-visao-geral-e-decisoes.md)
2. [Modelo Conceitual de Dados](02-modelo-conceitual-dados.md)
3. [Fluxo de Autenticação e Autorização](03-fluxo-autenticacao-autorizacao.md)
4. [Perfis e Permissões Iniciais](04-perfis-permissoes-iniciais.md)
5. [Auditoria de Segurança](05-auditoria-seguranca.md)
6. [Plano de Implementação](06-plano-implementacao.md)
7. [Guia Oficial para Implementação de Novos Módulos](guia-implementacao-novos-modulos.md)

## Estado Atual

Atualmente, o WebApólice confia integralmente na estrutura RBAC (Role-Based Access Control) global fornecida via JWT do Keycloak. As políticas da API são criadas baseadas nestas roles fixas. O novo módulo extrairá a responsabilidade de Autorização Granular para o banco de dados interno, mantendo apenas a Autenticação no provedor OIDC.

## Decisões Aprovadas

- **Keycloak:** O Keycloak continua sendo exclusivamente responsável pelo login, armazenamento seguro de senhas, sessão do usuário, emissão e validação primária dos tokens OIDC. A API administrativa do Keycloak não será utilizada pela plataforma num primeiro momento; as contas continuarão sendo criadas e geridas diretamente no painel do IdP.
- **Identificação:** O elo entre o token fornecido pelo Keycloak e o usuário da base de dados local sempre será o claim imutável `sub`.
- **Autorização (PostgreSQL):** O controle de Perfis, Módulos, Recursos e Permissões será governado por entidades mapeadas no PostgreSQL, de forma granular. 
- **Concessões:** Todo o acesso concedido ocorrerá estritamente através do intermédio de Perfis. Um usuário poderá ter diversos perfis, sendo seu acesso o somatório (união) das permissões ativas que possui.
- **Proteção Unificada:** O backend atuará como barreira intransponível (obrigatória). O frontend fará apenas o controle visual das ações permitidas (ocultando menus, botões e rotas) embasado pelas respostas do backend.

---

## Status da Implementação

- [x] **Parte 1 — Documentação técnica**
- [x] **Parte 2 — Estrutura física e EF Core**
- [x] **Parte 3 — Carga inicial (Seed)**
- [x] **Parte 4 — Persistência e consulta de permissões**
- [x] **Parte 5 — Contexto do usuário autenticado**
- [x] **Parte 6 — Provisionamento interno do usuário**
- [x] **Parte 7 — Autorização granular e proteção dos endpoints de Clientes**

## Executando o Projeto (Ambiente Híbrido: Local + VPS)

Caso precise executar o projeto localmente apontando para a base de dados (PostgreSQL) e o Keycloak hospedados em uma VPS, siga o guia abaixo para ter o sistema rodando na sua máquina:

### 1. Requisitos
- **.NET SDK 10.0** instalado e configurado no PATH do seu SO. (Se instalou recentemente, reinicie o computador).
- **Node.js 24 LTS** instalado. *(Dica: Use um gerenciador como o `fnm` rodando `fnm install 24` e `fnm use 24` no terminal).*

### 2. Configurando as Variáveis de Ambiente do Frontend

1. Na raiz do repositório (`c:\BRAVIDA\webapolice`), copie o arquivo `.env.example` e crie um novo chamado `.env`.
2. Abra o `.env` e configure a seção do frontend (Vite) para apontar para sua API local e o Keycloak remoto:

```env
# URL base da API local do backend
VITE_API_BASE_URL=http://localhost:5007

# URL base do Keycloak remoto
VITE_KEYCLOAK_URL=https://auth.bravida.com.br
VITE_KEYCLOAK_REALM=webapolice
VITE_KEYCLOAK_CLIENT_ID=webapolice-web
```

### 3. Rodando o Backend (API)

O backend já está parametrizado no arquivo `appsettings.Development.json` para utilizar as conexões remotas.
Abra um terminal, navegue até a pasta da API e inicie o projeto:

```bash
cd backend/src/WebApolice.Api
dotnet run
```
A API inicializará ouvindo na porta **5007**.

### 4. Rodando o Frontend (React)

Abra **um novo terminal** separado, ative o Node.js 24 e inicie o servidor:

```bash
cd apps/web
npm install
npm run dev
```

A interface estará disponível em **http://localhost:5173**. Ao tentar fazer login, o fluxo redirecionará corretamente para o Keycloak na VPS, autenticará o usuário, e voltará para fazer as chamadas restritas na sua API local (porta 5007).
