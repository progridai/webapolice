#!/usr/bin/env bash
set -euo pipefail

echo "Iniciando auditoria de conformidade de seguranca do realm Keycloak..."

# Validar se as variaveis de ambiente obrigatorias estao definidas
if [ -z "${KEYCLOAK_REALM:-}" ] || [ -z "${KEYCLOAK_WEB_CLIENT_ID:-}" ] || [ -z "${KEYCLOAK_API_CLIENT_ID:-}" ] || [ -z "${KEYCLOAK_DEV_ADMIN_USERNAME:-}" ]; then
  echo "Erro: Variaveis de ambiente KEYCLOAK_* obrigatorias nao foram fornecidas."
  exit 1
fi

# Autenticar no Keycloak CLI (kcadm.sh) usando as credenciais master administrativas
echo "Autenticando no CLI para validacao..."
/opt/keycloak/bin/kcadm.sh config credentials \
  --server http://localhost:8080 \
  --realm master \
  --user "$KC_BOOTSTRAP_ADMIN_USERNAME" \
  --password "$KC_BOOTSTRAP_ADMIN_PASSWORD"

# 1. Validar existencia e ativacao do Realm
echo "Verificando se o realm '$KEYCLOAK_REALM' existe e esta ativo..."
REALM_DATA=$(/opt/keycloak/bin/kcadm.sh get realms/"$KEYCLOAK_REALM")
if ! echo "$REALM_DATA" | grep -q '"enabled" : true'; then
  echo "[-] ERRO: Realm '$KEYCLOAK_REALM' nao esta habilitado!"
  exit 1
fi
echo "[+] Realm '$KEYCLOAK_REALM' existe e esta ativo."

# 2. Validar client web (publico, PKCE S256, Standard Flow, sem Implicit/Direct)
echo "Validando client web '$KEYCLOAK_WEB_CLIENT_ID'..."
WEB_DATA=$(/opt/keycloak/bin/kcadm.sh get clients -r "$KEYCLOAK_REALM" -q clientId="$KEYCLOAK_WEB_CLIENT_ID")

if ! echo "$WEB_DATA" | grep -q '"publicClient" : true'; then
  echo "[-] ERRO: Client '$KEYCLOAK_WEB_CLIENT_ID' nao e publico!"
  exit 1
fi
if ! echo "$WEB_DATA" | grep -q '"standardFlowEnabled" : true'; then
  echo "[-] ERRO: Standard Flow esta desabilitado para o client '$KEYCLOAK_WEB_CLIENT_ID'!"
  exit 1
fi
if ! echo "$WEB_DATA" | grep -q '"implicitFlowEnabled" : false'; then
  echo "[-] ERRO: Implicit Flow esta habilitado para o client '$KEYCLOAK_WEB_CLIENT_ID'!"
  exit 1
fi
if ! echo "$WEB_DATA" | grep -q '"directAccessGrantsEnabled" : false'; then
  echo "[-] ERRO: Direct Access Grants esta habilitado para o client '$KEYCLOAK_WEB_CLIENT_ID'!"
  exit 1
fi
if ! echo "$WEB_DATA" | grep -q '"pkce.code.challenge.method" : "S256"'; then
  echo "[-] ERRO: Metodo de challenge PKCE S256 nao configurado ou incorreto!"
  exit 1
fi
if ! echo "$WEB_DATA" | grep -q '"pkce.proof.key.required" : "true"'; then
  echo "[-] ERRO: Proof Key (PKCE) obrigatorio nao esta ativo!"
  exit 1
fi
# Validar redirect URIs estritas para localhost
if echo "$WEB_DATA" | grep -q '"redirectUris" : \[ "[^*]*\*"' && ! echo "$WEB_DATA" | grep -q '"redirectUris" : \[ "http://127.0.0.1:5173/\*"'; then
  echo "[-] ERRO: URL de redirecionamento insegura ou nao restrita ao localhost!"
  exit 1
fi
echo "[+] Client '$KEYCLOAK_WEB_CLIENT_ID' em conformidade com as regras de seguranca."

# 3. Validar client API (confidencial, sem login interativo, etc)
echo "Validando client API '$KEYCLOAK_API_CLIENT_ID'..."
API_DATA=$(/opt/keycloak/bin/kcadm.sh get clients -r "$KEYCLOAK_REALM" -q clientId="$KEYCLOAK_API_CLIENT_ID")

if ! echo "$API_DATA" | grep -q '"publicClient" : false'; then
  echo "[-] ERRO: Client '$KEYCLOAK_API_CLIENT_ID' nao e confidencial!"
  exit 1
fi
if ! echo "$API_DATA" | grep -q '"standardFlowEnabled" : false'; then
  echo "[-] ERRO: Standard Flow esta habilitado para o client '$KEYCLOAK_API_CLIENT_ID'!"
  exit 1
fi
if ! echo "$API_DATA" | grep -q '"implicitFlowEnabled" : false'; then
  echo "[-] ERRO: Implicit Flow esta habilitado para o client '$KEYCLOAK_API_CLIENT_ID'!"
  exit 1
fi
if ! echo "$API_DATA" | grep -q '"directAccessGrantsEnabled" : false'; then
  echo "[-] ERRO: Direct Access Grants esta habilitado para o client '$KEYCLOAK_API_CLIENT_ID'!"
  exit 1
fi
echo "[+] Client '$KEYCLOAK_API_CLIENT_ID' em conformidade com as regras de seguranca."

# 4. Validar existencia das roles globais
echo "Validando roles do realm..."
for role in admin gestor operador; do
  if ! /opt/keycloak/bin/kcadm.sh get roles/"$role" -r "$KEYCLOAK_REALM" >/dev/null 2>&1; then
    echo "[-] ERRO: Role de realm '$role' nao foi encontrada!"
    exit 1
  fi
  echo "[+] Role '$role' validada."
done

# 5. Validar usuario administrativo dev.admin e a role admin
echo "Validando usuario de desenvolvimento '$KEYCLOAK_DEV_ADMIN_USERNAME'..."
USER_DATA=$(/opt/keycloak/bin/kcadm.sh get users -r "$KEYCLOAK_REALM" -q username="$KEYCLOAK_DEV_ADMIN_USERNAME")
if [ -z "$USER_DATA" ] || [ "$USER_DATA" == "[]" ]; then
  echo "[-] ERRO: Usuario '$KEYCLOAK_DEV_ADMIN_USERNAME' nao existe!"
  exit 1
fi

ROLES_LIST=$(/opt/keycloak/bin/kcadm.sh get-roles -r "$KEYCLOAK_REALM" --uusername "$KEYCLOAK_DEV_ADMIN_USERNAME")
if ! echo "$ROLES_LIST" | grep -q '"name" : "admin"'; then
  echo "[-] ERRO: Usuario '$KEYCLOAK_DEV_ADMIN_USERNAME' nao possui a role 'admin' associada!"
  exit 1
fi
echo "[+] Usuario '$KEYCLOAK_DEV_ADMIN_USERNAME' validado com a role 'admin'."

# 6. Validar audience mapper no client web
echo "Validando audience mapper no client '$KEYCLOAK_WEB_CLIENT_ID'..."
WEB_ID_FOR_MAPPER=$(/opt/keycloak/bin/kcadm.sh get clients -r "$KEYCLOAK_REALM" -q clientId="$KEYCLOAK_WEB_CLIENT_ID" --fields id | grep '"id" :' | cut -d'"' -f4 | head -n1 || true)
if [ -z "$WEB_ID_FOR_MAPPER" ]; then
  echo "[-] ERRO: Nao foi possivel encontrar o client '$KEYCLOAK_WEB_CLIENT_ID' para validar o audience mapper!"
  exit 1
fi
MAPPERS_DATA=$(/opt/keycloak/bin/kcadm.sh get clients/"$WEB_ID_FOR_MAPPER"/protocol-mappers/models -r "$KEYCLOAK_REALM" 2>/dev/null)
if ! echo "$MAPPERS_DATA" | grep -q '"webapolice-api-audience"'; then
  echo "[-] ERRO: Audience mapper 'webapolice-api-audience' nao encontrado no client '$KEYCLOAK_WEB_CLIENT_ID'!"
  echo "    Execute configure-realm.sh para criar o mapper."
  exit 1
fi
if ! echo "$MAPPERS_DATA" | grep -q '"oidc-audience-mapper"'; then
  echo "[-] ERRO: O mapper encontrado nao e do tipo 'oidc-audience-mapper'!"
  exit 1
fi
echo "[+] Audience mapper 'webapolice-api-audience' validado no client '$KEYCLOAK_WEB_CLIENT_ID'."

echo "[SUCCESS] Realm '$KEYCLOAK_REALM' e todas as configuracoes estao em plena conformidade operacional e de seguranca!"
exit 0
