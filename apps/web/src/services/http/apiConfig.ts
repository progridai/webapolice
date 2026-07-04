/**
 * apiConfig.ts
 *
 * Configuração global da API do backend.
 */
import { ENV } from '../../app/config/env';

export const API_CONFIG = {
  /** URL base da API — injetada via variável de ambiente */
  BASE_URL: ENV.API_BASE_URL,

  /** Timeout padrão em milissegundos */
  DEFAULT_TIMEOUT_MS: 30_000,

  /** Headers padrão para todas as requisições */
  DEFAULT_HEADERS: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
} as const;
