# ADR-006: Centralização de Identidade e Acesso com Keycloak

## Status
Aceito (Accepted)

## Contexto
O ecossistema **WebApólice** necessita de um mecanismo robusto, escalável e seguro para autenticação de usuários e autorização de recursos baseada em perfis/roles (RBAC). 

No desenvolvimento frontend moderno com Single Page Applications (SPA), a autenticação direta contra bancos de dados ou o uso de fluxos de login proprietários no backend aumenta o acoplamento do sistema e eleva os riscos de vulnerabilidades críticas. Além disso, a segurança do fluxo OAuth 2.0 clássico em navegadores requer salvaguardas específicas para evitar o vazamento de chaves secretas ou credenciais.

Portanto, precisamos estabelecer:
1. Um provedor centralizado de identidades (IdP).
2. Protocolos de comunicação seguros padronizados pelo mercado (OpenID Connect / OIDC).
3. Uma topologia segura de clients OIDC separando frontend público de backend confidencial.
4. Um fluxo com suporte obrigatório a PKCE (Proof Key for Code Exchange) para o SPA React.
5. Um mecanismo idempotente de automação que configure este IdP localmente e permita a auditoria automática de segurança antes da integração de código.
6. A prevenção rígida de vazamento ou versionamento de chaves ou segredos de client no Git.

## Decisão
Adotamos o **Keycloak** (rodando localmente em container Docker na versão LTS `26.6.4`) como o provedor central de identidade e acesso do WebApólice.

A implementação e as políticas técnicas de segurança definidas são:

1. **Realm Dedicado**: Criação do realm `webapolice` dedicado ao domínio do ERP.
2. **Client Frontend (`webapolice-web`)**:
   * Definido como client público (`publicClient: true`).
   * Habilitado para fluxo padrão (*Standard Flow* / *Authorization Code*).
   * Desabilitados fluxos inseguros como *Implicit Flow* e *Direct Access Grants* (ROPC).
   * **Mandatório o uso de PKCE** com método de desafio **`S256`** e chave de prova (*Proof Key*) obrigatória para todas as requisições de autorização.
   * Redirecionamentos (*redirectUris* e *webOrigins*) limitados de forma estrita às origens locais (`http://127.0.0.1:5173/*`).
3. **Client Backend/API (`webapolice-api`)**:
   * Definido como client confidencial (`publicClient: false`).
   * Fluxos interativos de tela e *Direct Access Grants* desativados.
   * Credencial de client obtida em tempo de execução via variável de ambiente `KEYCLOAK_API_CLIENT_SECRET`.
4. **Provisionamento Idempotente (`configure-realm.sh`)**:
   * Script automatizado que faz o boot do realm, importa as definições declarativas básicas do arquivo `webapolice-realm.json` e atualiza segredos e o usuário administrativo de desenvolvimento `dev.admin` conforme o arquivo local `.env`.
5. **Auditoria de Conformidade (`validate-realm.sh`)**:
   * Script automatizado executado de forma simples contra a API de administração do Keycloak local, verificando se todos os parâmetros de segurança descritos acima estão corretos e bloqueando execuções com erro.
6. **Políticas de Segredos**:
   * Proibido o armazenamento de senhas e client secrets no Git ou em exportações de JSON. Eles devem ser referenciados apenas por variáveis do `.env` (excluídas via `.gitignore`).

## Consequências

### Positivas
* **Padronização de Segurança**: O uso de OpenID Connect + PKCE S256 segue as melhores práticas atuais recomendadas pela IETF e OWASP para segurança de SPAs.
* **Automação Repeatable**: Qualquer desenvolvedor pode recriar a infraestrutura e o realm Keycloak em segundos com um único comando, garantindo paridade de ambiente.
* **Prevenção de Erros de Configuração**: O script de auditoria de segurança (`validate-realm.sh`) serve como uma barreira automática que valida as regras de OIDC antes de deploys ou testes locais, prevenindo que clients sejam configurados incorretamente ou sem PKCE por engano.
* **Segurança de Credenciais**: Credenciais administrativas do container, do realm e de usuários de desenvolvimento são armazenadas exclusivamente no host do desenvolvedor via `.env` local, eliminando riscos de vazamento acidental no repositório público.

### Negativas
* **Aumento no Overhead Local**: O container do Keycloak consome memória física (RAM) do host e adiciona um pequeno overhead ao processo de inicialização inicial.
* **Complexidade do Fluxo OIDC**: A integração do frontend exigirá o uso de bibliotecas de client OIDC compatíveis com PKCE (ex: `oidc-client-ts` ou `keycloak-js`) e gerenciamento de expiração de tokens em memória.
