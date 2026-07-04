/**
 * keycloak.ts
 *
 * Instância singleton do cliente Keycloak.
 * Configurada com PKCE obrigatório (S256), conforme regras do projeto.
 *
 * REGRA: Nenhum componente deve acessar esta instância diretamente.
 * Utilizar o hook `useAuth` exportado de `src/auth`.
 */
import Keycloak from 'keycloak-js';
import { ENV } from '../app/config/env';

let _instance: Keycloak | null = null;

/**
 * Retorna a instância singleton do Keycloak.
 * Criada na primeira chamada e reutilizada nas demais.
 */
export function getKeycloakInstance(): Keycloak {
  if (!_instance) {
    _instance = new Keycloak({
      url: ENV.KEYCLOAK_URL,
      realm: ENV.KEYCLOAK_REALM,
      clientId: ENV.KEYCLOAK_CLIENT_ID,
    });
  }
  return _instance;
}

/**
 * Reseta a instância singleton.
 * Uso exclusivo em testes automatizados.
 */
export function _resetKeycloakInstance(): void {
  _instance = null;
}
