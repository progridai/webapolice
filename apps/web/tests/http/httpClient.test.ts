import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import {
  HttpApiError,
  httpClient,
  setTokenProvider,
} from '../../src/services/http/httpClient';

const mockFetch = vi.fn();

vi.stubGlobal('fetch', mockFetch);

vi.mock('../../src/services/http/apiConfig', () => ({
  API_CONFIG: {
    BASE_URL: 'http://localhost:5000',
    DEFAULT_TIMEOUT_MS: 5000,
    DEFAULT_HEADERS: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
  },
}));

function mockResponse(
  status: number,
  body: unknown = null,
  ok?: boolean,
): Response {
  const isOk = ok ?? (status >= 200 && status < 300);

  return {
    ok: isOk,
    status,
    text: vi.fn().mockResolvedValue(body !== null ? JSON.stringify(body) : ''),
    headers: new Headers(),
  } as unknown as Response;
}

describe('httpClient', () => {
  afterEach(() => {
    vi.useRealTimers();
  });
  beforeEach(() => {
    vi.clearAllMocks();
    setTokenProvider(null);
  });

  it('adds a Bearer token when a token provider is configured', async () => {
    setTokenProvider(async () => 'test-access-token');
    mockFetch.mockResolvedValue(mockResponse(200, { id: 1 }));

    await httpClient.get('/api/resource');

    const [, options] = mockFetch.mock.calls[0];
    expect((options as RequestInit).headers).toMatchObject({
      Authorization: 'Bearer test-access-token',
    });
  });

  it('builds the final URL without duplicated slashes', async () => {
    mockFetch.mockResolvedValue(mockResponse(200, { id: 1 }));

    await httpClient.get('/api/clientes');

    expect(mockFetch.mock.calls[0][0]).toBe(
      'http://localhost:5000/api/clientes',
    );
  });

  it('serializes POST bodies as JSON', async () => {
    mockFetch.mockResolvedValue(mockResponse(201, { id: 42 }));

    await httpClient.post('/api/resource', { nome: 'Teste' });

    const [, options] = mockFetch.mock.calls[0];
    expect((options as RequestInit).body).toBe(
      JSON.stringify({ nome: 'Teste' }),
    );
    expect((options as RequestInit).method).toBe('POST');
  });

  it('normaliza dados humanos no envio e preserva contratos técnicos', async () => {
    mockFetch.mockResolvedValue(mockResponse(201, {}));
    await httpClient.post('/api/clientes', {
      nome: 'JOÃO DA SILVA',
      email: 'ABC@EXAMPLE.COM',
      codigo: 'abc-X',
      dependentes: [{ nome: 'MARIA SOUZA' }],
    });
    expect(JSON.parse(mockFetch.mock.calls[0][1].body)).toEqual({
      nome: 'João da Silva',
      email: 'ABC@EXAMPLE.COM',
      codigo: 'abc-X',
      dependentes: [{ nome: 'Maria Souza' }],
    });
  });

  it('handles 204 responses without a body', async () => {
    mockFetch.mockResolvedValue(mockResponse(204, null));

    const result = await httpClient.delete('/api/resource/1');

    expect(result.status).toBe(204);
    expect(result.data).toBeUndefined();
  });

  it('throws HttpApiError for 400 responses', async () => {
    mockFetch.mockResolvedValue(
      mockResponse(400, {
        title: 'Bad Request',
        errors: { nome: ['Campo obrigatorio'] },
      }),
    );

    await expect(httpClient.post('/api/resource', {})).rejects.toThrow(
      HttpApiError,
    );
  });

  it('marks 401 responses as unauthorized', async () => {
    mockFetch.mockResolvedValue(mockResponse(401, { title: 'Unauthorized' }));

    await expect(httpClient.get('/api/protected')).rejects.toMatchObject({
      status: 401,
    });
  });

  it('marks 403 responses as forbidden', async () => {
    mockFetch.mockResolvedValue(mockResponse(403, { title: 'Forbidden' }));

    try {
      await httpClient.get('/api/admin');
      throw new Error('Expected request to fail');
    } catch (error) {
      expect(error).toBeInstanceOf(HttpApiError);
      expect((error as HttpApiError).isForbidden()).toBe(true);
    }
  });

  it('exposes validation errors for 422 responses', async () => {
    mockFetch.mockResolvedValue(
      mockResponse(422, {
        title: 'Unprocessable Entity',
        errors: { Nome: ['Nome obrigatorio'], Email: ['Email invalido'] },
      }),
    );

    try {
      await httpClient.post('/api/clientes', {});
      throw new Error('Expected request to fail');
    } catch (error) {
      expect(error).toBeInstanceOf(HttpApiError);
      expect((error as HttpApiError).status).toBe(422);
      expect((error as HttpApiError).isValidationError()).toBe(true);
      expect((error as HttpApiError).getValidationErrors().Nome).toContain(
        'Nome obrigatorio',
      );
    }
  });

  it('returns parsed JSON on successful GET requests', async () => {
    const expectedData = { id: 1, nome: 'WebApolice' };
    mockFetch.mockResolvedValue(mockResponse(200, expectedData));

    const result = await httpClient.get<{ id: number; nome: string }>(
      '/api/resource',
    );

    expect(result.data).toEqual(expectedData);
    expect(result.status).toBe(200);
  });

  it('propagates abort errors', async () => {
    const controller = new AbortController();
    controller.abort();
    mockFetch.mockRejectedValue(
      new DOMException('The user aborted a request.', 'AbortError'),
    );

    await expect(
      httpClient.get('/api/slow', { signal: controller.signal }),
    ).rejects.toThrow();
    expect(mockFetch).not.toHaveBeenCalled();
  });

  it('cancela imediatamente durante a obtenção do token sem iniciar fetch', async () => {
    vi.useFakeTimers();
    const controller = new AbortController();
    let resolveToken: (value: string) => void = () => {};
    setTokenProvider(
      () =>
        new Promise<string>((resolve) => {
          resolveToken = resolve;
        }),
    );
    const add = vi.spyOn(controller.signal, 'addEventListener');
    const remove = vi.spyOn(controller.signal, 'removeEventListener');
    const request = httpClient.get('/api/cancelar', {
      signal: controller.signal,
    });
    controller.abort();
    await expect(request).rejects.toMatchObject({ name: 'AbortError' });
    resolveToken('token-atrasado');
    await Promise.resolve();
    expect(mockFetch).not.toHaveBeenCalled();
    expect(remove).toHaveBeenCalledWith('abort', add.mock.calls[0][1]);
    expect(vi.getTimerCount()).toBe(0);
  });

  it('não consulta o provedor de token quando o signal já está cancelado', async () => {
    const provider = vi.fn(async () => 'token');
    setTokenProvider(provider);
    const controller = new AbortController();
    controller.abort();
    await expect(
      httpClient.get('/api/cancelado', { signal: controller.signal }),
    ).rejects.toMatchObject({ name: 'AbortError' });
    expect(provider).not.toHaveBeenCalled();
    expect(mockFetch).not.toHaveBeenCalled();
  });

  it('mantém o cancelamento enquanto o corpo da resposta ainda está sendo lido', async () => {
    vi.useFakeTimers();
    const controller = new AbortController();
    let finish: (body: string) => void = () => {};
    const response = mockResponse(200);
    response.text = () =>
      new Promise<string>((resolve) => {
        finish = resolve;
      });
    mockFetch.mockResolvedValue(response);
    const pending = httpClient.get('/api/corpo-pendente', {
      signal: controller.signal,
    });
    const assertion = expect(pending).rejects.toMatchObject({
      name: 'AbortError',
    });
    await Promise.resolve();
    controller.abort();
    finish('{}');
    await assertion;
    expect(vi.getTimerCount()).toBe(0);
  });

  it('libera listeners e timers após concluir token e fetch', async () => {
    vi.useFakeTimers();
    const controller = new AbortController();
    const add = vi.spyOn(controller.signal, 'addEventListener');
    const remove = vi.spyOn(controller.signal, 'removeEventListener');
    setTokenProvider(async () => 'token');
    mockFetch.mockResolvedValue(mockResponse(200, {}));
    await httpClient.get('/api/concluir', { signal: controller.signal });
    expect(remove.mock.calls).toEqual(
      add.mock.calls.map(([event, listener]) => [event, listener]),
    );
    expect(vi.getTimerCount()).toBe(0);
  });

  it('propaga cancelamento durante fetch e libera recursos mesmo na falha', async () => {
    vi.useFakeTimers();
    const controller = new AbortController();
    mockFetch.mockImplementation(
      (_url: string, options: RequestInit) =>
        new Promise((_resolve, reject) => {
          options.signal?.addEventListener(
            'abort',
            () => reject(options.signal?.reason),
            { once: true },
          );
        }),
    );
    const request = httpClient.get('/api/pendente', {
      signal: controller.signal,
    });
    controller.abort();
    await expect(request).rejects.toMatchObject({ name: 'AbortError' });
    expect(vi.getTimerCount()).toBe(0);
  });

  it('trata timeout como erro de rede e não como cancelamento do usuário', async () => {
    vi.useFakeTimers();
    mockFetch.mockImplementation(
      (_url: string, options: RequestInit) =>
        new Promise((_resolve, reject) => {
          options.signal?.addEventListener(
            'abort',
            () => reject(options.signal?.reason),
            { once: true },
          );
        }),
    );
    const assertion = expect(
      httpClient.get('/api/lento', { timeoutMs: 10 }),
    ).rejects.toMatchObject({ code: 'NETWORK_ERROR' });
    await vi.advanceTimersByTimeAsync(10);
    await assertion;
    expect(vi.getTimerCount()).toBe(0);
  });

  it('normalizes network errors without exposing the browser message', async () => {
    mockFetch.mockRejectedValue(new TypeError('Failed to fetch'));

    try {
      await httpClient.get('/api/clientes');
      throw new Error('Expected request to fail');
    } catch (error) {
      expect(error).toBeInstanceOf(HttpApiError);
      expect((error as HttpApiError).code).toBe('NETWORK_ERROR');
      expect((error as HttpApiError).message).not.toBe('Failed to fetch');
      expect((error as HttpApiError).message).toContain(
        'Não foi possível conectar',
      );
    }
  });
});
