/**
 * env.ts
 *
 * Centraliza a leitura de todas as variáveis de ambiente VITE_*.
 * Evita o uso de `import.meta.env` disperso pelo projeto.
 * Falha com mensagem clara quando variáveis obrigatórias estão ausentes.
 */

function getRequired(key: string): string {
  const value = import.meta.env[key];
  if (!value || typeof value !== 'string' || value.trim() === '') {
    throw new Error(
      `[webapolice] Variável de ambiente obrigatória ausente ou vazia: ${key}\n` +
        `Certifique-se de que o arquivo .env contém ${key}=<valor>.`
    );
  }
  return value;
}

function getOptional(key: string, defaultValue: string): string {
  const value = import.meta.env[key];
  if (!value || typeof value !== 'string' || value.trim() === '') {
    return defaultValue;
  }
  return value;
}

export const ENV = {
  /** URL base da API do backend */
  API_BASE_URL: getRequired('VITE_API_BASE_URL'),

  /** URL do servidor Keycloak */
  KEYCLOAK_URL: getRequired('VITE_KEYCLOAK_URL'),

  /** Nome do realm Keycloak */
  KEYCLOAK_REALM: getRequired('VITE_KEYCLOAK_REALM'),

  /** Client ID público do Keycloak */
  KEYCLOAK_CLIENT_ID: getRequired('VITE_KEYCLOAK_CLIENT_ID'),

  /** Habilita o catálogo do Design System (deve ser combinado com role admin) */
  ENABLE_DESIGN_SYSTEM: getOptional('VITE_ENABLE_DESIGN_SYSTEM', 'false') === 'true',

  /** Modo de ambiente (development, production, test) */
  MODE: import.meta.env.MODE as 'development' | 'production' | 'test',

  /** Flag de desenvolvimento */
  IS_DEV: import.meta.env.DEV === true,

  /** Versão da aplicação */
  APP_VERSION: getOptional('VITE_APP_VERSION', '0.1.0'),
} as const;
