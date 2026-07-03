#!/usr/bin/env bash
set -euo pipefail

echo "Iniciando provisionamento do realm Keycloak..."

# Validar se as variaveis de ambiente obrigatorias estao definidas
if [ -z "${KEYCLOAK_REALM:-}" ] || [ -z "${KEYCLOAK_WEB_CLIENT_ID:-}" ] || [ -z "${KEYCLOAK_API_CLIENT_ID:-}" ] || [ -z "${KEYCLOAK_API_CLIENT_SECRET:-}" ] || [ -z "${KEYCLOAK_DEV_ADMIN_USERNAME:-}" ] || [ -z "${KEYCLOAK_DEV_ADMIN_PASSWORD:-}" ] || [ -z "${KEYCLOAK_DEV_ADMIN_EMAIL:-}" ]; then
  echo "Erro: Variaveis de ambiente KEYCLOAK_* obrigatorias nao foram fornecidas."
  exit 1
fi

# Funcoes auxiliares para obter IDs sem depender do JQ ou de opcoes desconhecidas do CLI
get_client_id() {
  local client_id="$1"
  /opt/keycloak/bin/kcadm.sh get clients -r "$KEYCLOAK_REALM" -q clientId="$client_id" --fields id | grep '"id" :' | cut -d'"' -f4 | head -n1 || true
}

get_user_id() {
  local username="$1"
  /opt/keycloak/bin/kcadm.sh get users -r "$KEYCLOAK_REALM" -q username="$username" --fields id | grep '"id" :' | cut -d'"' -f4 | head -n1 || true
}

# Aguardar o Keycloak estar saudavel no painel de gerenciamento (porta 9000)
echo "Aguardando o Keycloak estar pronto..."
until exec 3<>/dev/tcp/127.0.0.1/9000 && echo -e "GET /health/live HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n" >&3 && cat <&3 | grep -q "200 OK"; do
  echo "Keycloak ainda nao esta pronto. Aguardando 2 segundos..."
  sleep 2
done
echo "Keycloak esta pronto!"

# Autenticar no Keycloak CLI (kcadm.sh) usando as credenciais master administrativas
echo "Autenticando no CLI do Keycloak..."
/opt/keycloak/bin/kcadm.sh config credentials \
  --server http://localhost:8080 \
  --realm master \
  --user "$KC_BOOTSTRAP_ADMIN_USERNAME" \
  --password "$KC_BOOTSTRAP_ADMIN_PASSWORD"

# Verificar se o realm ja existe
echo "Verificando se o realm '$KEYCLOAK_REALM' existe..."
REALM_EXISTS=$(/opt/keycloak/bin/kcadm.sh get realms/"$KEYCLOAK_REALM" --fields realm 2>/dev/null || true)

if [ -z "$REALM_EXISTS" ]; then
  echo "Realm '$KEYCLOAK_REALM' nao encontrado. Criando a partir do JSON de exportacao..."
  /opt/keycloak/bin/kcadm.sh create realms -f /opt/keycloak/keycloak_shared/realm/webapolice-realm.json
else
  echo "Realm '$KEYCLOAK_REALM' ja existe."
fi

# Garantir a criacao/atualizacao dos clients de forma idempotente

# 1. Client web (Public, PKCE obrigatorio S256)
echo "Configurando o client '$KEYCLOAK_WEB_CLIENT_ID'..."
WEB_ID=$(get_client_id "$KEYCLOAK_WEB_CLIENT_ID")

if [ -z "$WEB_ID" ]; then
  echo "Criando client '$KEYCLOAK_WEB_CLIENT_ID'..."
  /opt/keycloak/bin/kcadm.sh create clients -r "$KEYCLOAK_REALM" \
    -s clientId="$KEYCLOAK_WEB_CLIENT_ID" \
    -s enabled=true \
    -s protocol=openid-connect \
    -s publicClient=true \
    -s bearerOnly=false \
    -s standardFlowEnabled=true \
    -s implicitFlowEnabled=false \
    -s directAccessGrantsEnabled=false \
    -s serviceAccountsEnabled=false \
    -s rootUrl="http://127.0.0.1:5173" \
    -s baseUrl="http://127.0.0.1:5173" \
    -s "redirectUris=[\"http://127.0.0.1:5173/*\"]" \
    -s "webOrigins=[\"http://127.0.0.1:5173\"]" \
    -s consentRequired=false \
    -s "attributes={ \"pkce.code.challenge.method\": \"S256\", \"pkce.proof.key.required\": \"true\", \"post.logout.redirect.uris\": \"http://127.0.0.1:5173/*\" }"
else
  echo "Client '$KEYCLOAK_WEB_CLIENT_ID' ja existe. Atualizando configuracoes..."
  /opt/keycloak/bin/kcadm.sh update clients/"$WEB_ID" -r "$KEYCLOAK_REALM" \
    -s enabled=true \
    -s publicClient=true \
    -s standardFlowEnabled=true \
    -s implicitFlowEnabled=false \
    -s directAccessGrantsEnabled=false \
    -s serviceAccountsEnabled=false \
    -s rootUrl="http://127.0.0.1:5173" \
    -s baseUrl="http://127.0.0.1:5173" \
    -s "redirectUris=[\"http://127.0.0.1:5173/*\"]" \
    -s "webOrigins=[\"http://127.0.0.1:5173\"]" \
    -s consentRequired=false \
    -s "attributes={ \"pkce.code.challenge.method\": \"S256\", \"pkce.proof.key.required\": \"true\", \"post.logout.redirect.uris\": \"http://127.0.0.1:5173/*\" }"
fi

# 2. Client API (Confidencial)
echo "Configurando o client '$KEYCLOAK_API_CLIENT_ID'..."
API_ID=$(get_client_id "$KEYCLOAK_API_CLIENT_ID")

if [ -z "$API_ID" ]; then
  echo "Criando client '$KEYCLOAK_API_CLIENT_ID'..."
  /opt/keycloak/bin/kcadm.sh create clients -r "$KEYCLOAK_REALM" \
    -s clientId="$KEYCLOAK_API_CLIENT_ID" \
    -s enabled=true \
    -s protocol=openid-connect \
    -s publicClient=false \
    -s secret="$KEYCLOAK_API_CLIENT_SECRET" \
    -s bearerOnly=false \
    -s standardFlowEnabled=false \
    -s implicitFlowEnabled=false \
    -s directAccessGrantsEnabled=false \
    -s serviceAccountsEnabled=false
else
  echo "Client '$KEYCLOAK_API_CLIENT_ID' ja existe. Atualizando configuracoes e secret..."
  /opt/keycloak/bin/kcadm.sh update clients/"$API_ID" -r "$KEYCLOAK_REALM" \
    -s enabled=true \
    -s publicClient=false \
    -s secret="$KEYCLOAK_API_CLIENT_SECRET" \
    -s bearerOnly=false \
    -s standardFlowEnabled=false \
    -s implicitFlowEnabled=false \
    -s directAccessGrantsEnabled=false \
    -s serviceAccountsEnabled=false
fi

# Garantir a criacao das roles caso nao existam
for role in admin gestor operador; do
  ROLE_EXISTS=$(/opt/keycloak/bin/kcadm.sh get roles/"$role" -r "$KEYCLOAK_REALM" --fields name 2>/dev/null || true)
  if [ -z "$ROLE_EXISTS" ]; then
    echo "Criando role de realm: $role..."
    /opt/keycloak/bin/kcadm.sh create roles -r "$KEYCLOAK_REALM" -s name="$role"
  else
    echo "Role '$role' ja existe."
  fi
done

# Garantir o usuario administrativo de desenvolvimento dev.admin de forma idempotente
echo "Configurando o usuario de desenvolvimento '$KEYCLOAK_DEV_ADMIN_USERNAME'..."
USER_ID=$(get_user_id "$KEYCLOAK_DEV_ADMIN_USERNAME")

if [ -z "$USER_ID" ]; then
  echo "Criando usuario '$KEYCLOAK_DEV_ADMIN_USERNAME'..."
  USER_ID=$(/opt/keycloak/bin/kcadm.sh create users -r "$KEYCLOAK_REALM" \
    -s username="$KEYCLOAK_DEV_ADMIN_USERNAME" \
    -s email="$KEYCLOAK_DEV_ADMIN_EMAIL" \
    -s enabled=true \
    -s emailVerified=true \
    -i)
else
  echo "Usuario '$KEYCLOAK_DEV_ADMIN_USERNAME' ja existe. Atualizando dados cadastrais..."
  /opt/keycloak/bin/kcadm.sh update users/"$USER_ID" -r "$KEYCLOAK_REALM" \
    -s email="$KEYCLOAK_DEV_ADMIN_EMAIL" \
    -s enabled=true \
    -s emailVerified=true
fi

# Definir a senha do usuario (local e nao versionada)
echo "Atualizando credenciais do usuario '$KEYCLOAK_DEV_ADMIN_USERNAME'..."
/opt/keycloak/bin/kcadm.sh set-password -r "$KEYCLOAK_REALM" --username "$KEYCLOAK_DEV_ADMIN_USERNAME" --new-password "$KEYCLOAK_DEV_ADMIN_PASSWORD"

# Associar a role global admin ao usuario dev.admin
echo "Associando a role 'admin' ao usuario '$KEYCLOAK_DEV_ADMIN_USERNAME'..."
/opt/keycloak/bin/kcadm.sh add-roles -r "$KEYCLOAK_REALM" --uusername "$KEYCLOAK_DEV_ADMIN_USERNAME" --rolename admin

# Configurar o audience mapper no client web para incluir webapolice-api no claim aud
echo "Configurando audience mapper 'webapolice-api-audience' no client '$KEYCLOAK_WEB_CLIENT_ID'..."
WEB_ID_FOR_MAPPER=$(get_client_id "$KEYCLOAK_WEB_CLIENT_ID")

if [ -n "$WEB_ID_FOR_MAPPER" ]; then
  # Verificar se o mapper ja existe
  MAPPER_EXISTS=$(/opt/keycloak/bin/kcadm.sh get clients/"$WEB_ID_FOR_MAPPER"/protocol-mappers/models -r "$KEYCLOAK_REALM" 2>/dev/null | grep '"webapolice-api-audience"' || true)

  if [ -z "$MAPPER_EXISTS" ]; then
    echo "Criando audience mapper 'webapolice-api-audience'..."
    /opt/keycloak/bin/kcadm.sh create clients/"$WEB_ID_FOR_MAPPER"/protocol-mappers/models -r "$KEYCLOAK_REALM" \
      -s name="webapolice-api-audience" \
      -s protocol=openid-connect \
      -s protocolMapper=oidc-audience-mapper \
      -s consentRequired=false \
      -s "config={\"included.client.audience\":\"$KEYCLOAK_API_CLIENT_ID\",\"access.token.claim\":\"true\",\"id.token.claim\":\"false\"}"
    echo "Audience mapper 'webapolice-api-audience' criado com sucesso."
  else
    echo "Audience mapper 'webapolice-api-audience' ja existe. Nenhuma acao necessaria."
  fi
else
  echo "Aviso: Nao foi possivel encontrar o client '$KEYCLOAK_WEB_CLIENT_ID' para configurar o audience mapper."
fi

echo "Provisionamento do realm Keycloak concluido com sucesso!"
