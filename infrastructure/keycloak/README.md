# Infraestrutura de Identidade e Acesso - Keycloak

Esta pasta contém a especificação declarativa do Realm e dos scripts de provisionamento e validação de segurança para o **Keycloak**, utilizado como o provedor central de identidade (IdP) do ecossistema WebApólice.

## Estrutura de Diretórios

* **`realm/`**: Contém o arquivo declarativo [webapolice-realm.json](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/infrastructure/keycloak/realm/webapolice-realm.json), contendo as configurações básicas de realm, clients OIDC e roles de segurança globais (sem senhas ou segredos expostos).
* **`scripts/`**: Contém scripts bash executados dentro do container do Keycloak para gerenciar o ambiente de desenvolvimento local de forma automatizada e idempotente:
  * [configure-realm.sh](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/infrastructure/keycloak/scripts/configure-realm.sh): Script responsável por aguardar a inicialização do Keycloak, importar o realm do JSON (se ainda não existir), atualizar as credenciais dos clients OIDC a partir do arquivo `.env` e provisionar de forma idempotente o usuário administrativo de desenvolvimento.
  * [validate-realm.sh](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/infrastructure/keycloak/scripts/validate-realm.sh): Script de auditoria automatizada que verifica se o realm está configurado corretamente de acordo com os padrões OIDC e PKCE descritos abaixo.

## Configuração do Realm e OIDC Clients

O realm de desenvolvimento local chama-se `webapolice`. Ele define os seguintes clients OpenID Connect (OIDC):

### 1. Client Frontend: `webapolice-web`
* **Tipo**: Público (não requer segredo de client).
* **Fluxo**: Standard Flow (Authorization Code) ativado.
* **Segurança de Fluxo**:
  * **Implicit Flow**: Desabilitado.
  * **Direct Access Grants (Resource Owner Password Credentials)**: Desabilitado.
  * **Proof Key for Code Exchange (PKCE)**: Obrigatório com método de desafio **`S256`** (`pkce.code.challenge.method: "S256"` e `pkce.proof.key.required: "true"`).
* **Redirecionamento de Segurança**: Restrito estritamente a URLs locais de desenvolvimento (`http://127.0.0.1:5173/*`).

### 2. Client Backend/API: `webapolice-api`
* **Tipo**: Confidencial (requer segredo de client).
* **Fluxo**: Fluxo interativo desativado (Standard Flow e Direct Access Grants desativados).
* **Segredo**: Lido dinamicamente da variável de ambiente `KEYCLOAK_API_CLIENT_SECRET` (do `.env` local).

### 3. Roles Globais
Foram definidas três roles globais iniciais no realm para controle de acesso baseado em roles (RBAC):
* `admin`: Administrador com permissões totais sobre as funcionalidades.
* `gestor`: Gestor de negócios operacionais e relatórios.
* `operador`: Operador básico para cadastros e consultas de apólices.

### 4. Usuário de Desenvolvimento Administrativo
* **Username**: `dev.admin`
* **Email**: `dev.admin@local.test`
* **Role**: `admin`
* **Senha**: Definida de forma segura no arquivo `.env` local (`KEYCLOAK_DEV_ADMIN_PASSWORD`).

## Instruções de Execução

Todos os scripts devem ser executados através do container docker do Keycloak a partir da pasta `infrastructure/`:

### 1. Inicializar a infraestrutura
```bash
docker compose --env-file ../.env up -d
```

### 2. Provisionar o Realm e Usuários
```bash
docker compose --env-file ../.env exec keycloak bash /opt/keycloak/keycloak_shared/scripts/configure-realm.sh
```

### 3. Executar Auditoria de Segurança do Realm
```bash
docker compose --env-file ../.env exec keycloak bash /opt/keycloak/keycloak_shared/scripts/validate-realm.sh
```

## Regras de Segurança Importantes

> [!WARNING]
> * **Nenhum Segredo no Git**: Nunca coloque senhas reais ou segredos (como o secret do client `webapolice-api` ou senhas de banco) no arquivo JSON do realm ou em scripts de provisionamento. Eles devem ser recuperados em tempo de execução a partir do `.env` local.
> * **Auditoria no CI**: O script `validate-realm.sh` deve ser utilizado de forma automatizada em pipelines de validação ou de forma local antes de qualquer commit que afete o provisionamento de identidade.
