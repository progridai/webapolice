/**
 * http.types.ts
 *
 * Tipos internos do cliente HTTP.
 */

/** Erro normalizado da API — usado em toda a aplicação */
export interface ApiError {
  /** Código de status HTTP */
  status?: number;
  /** Código interno de erro (ex: 'VALIDATION_ERROR', 'NOT_FOUND') */
  code?: string;
  /** Mensagem legível ao desenvolvedor */
  message: string;
  /** Detalhes adicionais (erros de validação por campo, etc.) */
  details?: unknown;
  /** ID de rastreabilidade do backend para debug */
  traceId?: string;
}

/** Erros de validação por campo — compatível com ProblemDetails do ASP.NET Core */
export type ValidationErrors = Record<string, string[]>;

/** Opções de uma requisição HTTP */
export interface HttpOptions {
  /** Headers adicionais */
  headers?: Record<string, string>;
  /** Signal para cancelamento via AbortController */
  signal?: AbortSignal;
  /** Timeout em milissegundos (padrão: 30000) */
  timeoutMs?: number;
  /** Não incluir o token de autorização (para rotas públicas) */
  skipAuth?: boolean;
  /** Query parameters */
  params?: Record<string, string | number | boolean>;
}

/** Resposta tipada do cliente HTTP */
export interface HttpResponse<T> {
  data: T;
  status: number;
}
