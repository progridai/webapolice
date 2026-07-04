/**
 * httpClient.ts
 *
 * Cliente HTTP centralizado baseado em fetch nativo.
 *
 * Responsabilidades:
 * - Inserir Bearer token de forma transparente
 * - Renovar token quando necessário antes das chamadas
 * - Serializar e deserializar JSON
 * - Tratar respostas sem conteúdo (204)
 * - Normalizar todos os erros HTTP em HttpApiError
 * - Suportar cancelamento via AbortSignal
 * - Suportar timeout configurável
 * - Não acoplar a componentes React — agnóstico de UI
 *
 * REGRA: Nenhuma chamada HTTP deve ser feita diretamente em componentes visuais.
 * Use sempre este cliente ou funções de api/ dentro de features.
 */
import { API_CONFIG } from './apiConfig';
import { HttpApiError, normalizeError } from './httpError';
import type { HttpOptions, HttpResponse } from './http.types';

/** Função injetável para obter o token de acesso — evita acoplamento ao Keycloak */
type TokenProvider = (() => Promise<string | undefined>) | null;

let _tokenProvider: TokenProvider = null;

/**
 * Registra o provedor de token.
 * Chamado pelo AuthProvider após inicialização.
 */
export function setTokenProvider(provider: TokenProvider): void {
  _tokenProvider = provider;
}

/** Constrói a URL completa a partir do path */
function buildUrl(path: string): string {
  const base = API_CONFIG.BASE_URL.replace(/\/$/, '');
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${base}${normalizedPath}`;
}

/** Executa uma requisição com timeout */
async function fetchWithTimeout(
  url: string,
  options: RequestInit,
  timeoutMs: number
): Promise<Response> {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

  // Combina o signal externo com o de timeout
  const externalSignal = (options as { signal?: AbortSignal }).signal;
  if (externalSignal) {
    externalSignal.addEventListener('abort', () => controller.abort());
  }

  try {
    return await fetch(url, { ...options, signal: controller.signal });
  } finally {
    clearTimeout(timeoutId);
  }
}

/** Executa uma requisição HTTP genérica */
async function request<T>(
  method: string,
  path: string,
  body?: unknown,
  options: HttpOptions = {}
): Promise<HttpResponse<T>> {
  const { headers = {}, signal, timeoutMs = API_CONFIG.DEFAULT_TIMEOUT_MS, skipAuth = false } =
    options;

  // Obtém o token de acesso (se disponível e necessário)
  const authHeaders: Record<string, string> = {};
  if (!skipAuth && _tokenProvider) {
    try {
      const token = await _tokenProvider();
      if (token) {
        authHeaders['Authorization'] = `Bearer ${token}`;
      }
    } catch {
      // Falha silenciosa — a API retornará 401 e o fluxo de renovação cuidará disso
    }
  }

  const requestOptions: RequestInit = {
    method,
    headers: {
      ...API_CONFIG.DEFAULT_HEADERS,
      ...authHeaders,
      ...headers,
    },
    signal,
  };

  if (body !== undefined) {
    requestOptions.body = JSON.stringify(body);
  }

  const url = buildUrl(path);
  const response = await fetchWithTimeout(url, requestOptions, timeoutMs);

  // Resposta sem conteúdo
  if (response.status === 204) {
    return { data: undefined as T, status: 204 };
  }

  const bodyText = await response.text();

  // Resposta de erro
  if (!response.ok) {
    throw await normalizeError(response, bodyText);
  }

  // Resposta com conteúdo
  try {
    const data = JSON.parse(bodyText) as T;
    return { data, status: response.status };
  } catch {
    return { data: bodyText as unknown as T, status: response.status };
  }
}

/** Cliente HTTP com métodos tipados */
export const httpClient = {
  /**
   * GET — busca um recurso
   */
  get<T>(path: string, options?: HttpOptions): Promise<HttpResponse<T>> {
    return request<T>('GET', path, undefined, options);
  },

  /**
   * POST — cria um recurso
   */
  post<T>(path: string, body?: unknown, options?: HttpOptions): Promise<HttpResponse<T>> {
    return request<T>('POST', path, body, options);
  },

  /**
   * PUT — substitui um recurso
   */
  put<T>(path: string, body?: unknown, options?: HttpOptions): Promise<HttpResponse<T>> {
    return request<T>('PUT', path, body, options);
  },

  /**
   * PATCH — atualiza parcialmente um recurso
   */
  patch<T>(path: string, body?: unknown, options?: HttpOptions): Promise<HttpResponse<T>> {
    return request<T>('PATCH', path, body, options);
  },

  /**
   * DELETE — remove um recurso
   */
  delete<T>(path: string, options?: HttpOptions): Promise<HttpResponse<T>> {
    return request<T>('DELETE', path, undefined, options);
  },
};

export { HttpApiError };
