/**
 * httpError.ts
 *
 * Classe de erro normalizado e funções de conversão.
 * Centraliza o tratamento de erros HTTP da API.
 */
import type { ApiError, ValidationErrors } from './http.types';

/** Classe de erro da aplicação — representa erros normalizados da API */
export class HttpApiError extends Error {
  public readonly status?: number;
  public readonly code?: string;
  public readonly details?: unknown;
  public readonly traceId?: string;

  constructor(error: ApiError) {
    super(error.message);
    this.name = 'HttpApiError';
    this.status = error.status;
    this.code = error.code;
    this.details = error.details;
    this.traceId = error.traceId;
  }

  /** Verifica se é um erro de não-autenticado */
  isUnauthorized(): boolean {
    return this.status === 401;
  }

  /** Verifica se é um erro de acesso negado */
  isForbidden(): boolean {
    return this.status === 403;
  }

  /** Verifica se é um erro de recurso não encontrado */
  isNotFound(): boolean {
    return this.status === 404;
  }

  /** Verifica se é um erro de validação */
  isValidationError(): boolean {
    return this.status === 422 || this.status === 400;
  }

  /** Verifica se é um erro de conflito */
  isConflict(): boolean {
    return this.status === 409;
  }

  /** Extrai erros de validação por campo */
  getValidationErrors(): ValidationErrors {
    if (!this.details || typeof this.details !== 'object') return {};
    const details = this.details as Record<string, unknown>;
    // Compatível com ProblemDetails do ASP.NET Core (campo "errors")
    const errors = details['errors'] ?? details;
    if (typeof errors !== 'object' || errors === null) return {};
    return errors as ValidationErrors;
  }
}

/** Mensagens amigáveis por código HTTP */
const HTTP_MESSAGES: Record<number, string> = {
  400: 'Requisição inválida. Verifique os dados enviados.',
  401: 'Sua sessão expirou. Por favor, faça login novamente.',
  403: 'Você não tem permissão para acessar este recurso.',
  404: 'O recurso solicitado não foi encontrado.',
  409: 'Conflito: o registro já existe ou está em uso.',
  422: 'Os dados enviados contêm erros de validação.',
  429: 'Muitas tentativas. Aguarde um momento antes de tentar novamente.',
  500: 'Erro interno do servidor. Tente novamente em instantes.',
  503: 'Serviço temporariamente indisponível. Tente novamente em instantes.',
};

export function normalizeNetworkError(error: unknown): HttpApiError {
  if (error instanceof DOMException && error.name === 'AbortError') {
    throw error;
  }

  if (error instanceof Error && error.name === 'AbortError') {
    throw error;
  }

  return new HttpApiError({
    code: 'NETWORK_ERROR',
    message: 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.',
    details: error instanceof Error ? { name: error.name } : undefined,
  });
}

/**
 * Normaliza qualquer erro em um `HttpApiError` tipado.
 * Garante que erros brutos nunca cheguem aos componentes.
 */
export async function normalizeError(
  response: Response,
  bodyText: string
): Promise<HttpApiError> {
  let body: Record<string, unknown> = {};

  try {
    body = JSON.parse(bodyText) as Record<string, unknown>;
  } catch {
    // Body não é JSON — usa texto bruto como mensagem
  }

  // Suporte a ProblemDetails do ASP.NET Core
  const message =
    (body['detail'] as string) ??
    (body['message'] as string) ??
    (body['title'] as string) ??
    HTTP_MESSAGES[response.status] ??
    `Erro HTTP ${response.status}`;

  const error: ApiError = {
    status: response.status,
    code: (body['code'] as string) ?? (body['type'] as string) ?? undefined,
    message,
    details: body['errors'] ?? body['details'] ?? undefined,
    traceId: (body['traceId'] as string) ?? undefined,
  };

  return new HttpApiError(error);
}

/**
 * Converte erros de validação do backend em formato de formulário.
 * @param errors - `ValidationErrors` extraídos do erro da API
 * @returns Objeto com o primeiro erro de cada campo
 */
export function flattenValidationErrors(errors: ValidationErrors): Record<string, string> {
  return Object.fromEntries(
    Object.entries(errors).map(([field, messages]) => [
      field.charAt(0).toLowerCase() + field.slice(1), // camelCase
      messages[0] ?? '',
    ])
  );
}
