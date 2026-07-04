/* eslint-disable @typescript-eslint/no-explicit-any */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { listarClientes } from './clientesApi';
import { httpClient } from '../../../services/http/httpClient';

vi.mock('../../../services/http/httpClient', () => ({
  httpClient: {
    get: vi.fn(),
  },
}));

describe('clientesApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve chamar httpClient.get com a URL e parâmetros corretos', async () => {
    const mockResponse = { data: { itens: [], totalItens: 0 } };
    vi.mocked(httpClient.get).mockResolvedValueOnce(mockResponse as any);

    const result = await listarClientes({
      page: 2,
      pageSize: 50,
      nome: 'João',
      status: 1,
    });

    expect(httpClient.get).toHaveBeenCalledTimes(1);
    const callArgs = vi.mocked(httpClient.get).mock.calls[0];
    
    // Verifica a URL serializada corretamente
    expect(callArgs[0]).toContain('/api/clientes?');
    expect(callArgs[0]).toContain('pagina=2');
    expect(callArgs[0]).toContain('tamanho_pagina=50');
    expect(callArgs[0]).toContain('nome=Jo%C3%A3o');
    expect(callArgs[0]).toContain('status=1');
    expect(result).toEqual(mockResponse.data);
  });

  it('deve omitir parâmetros não preenchidos na URL', async () => {
    vi.mocked(httpClient.get).mockResolvedValueOnce({ data: {} } as any);

    await listarClientes({
      page: 1,
      // pageSize não fornecido
      nome: '', // vazio
      status: '', // vazio
    });

    const callArgs = vi.mocked(httpClient.get).mock.calls[0];
    expect(callArgs[0]).toBe('/api/clientes?pagina=1');
  });

  it('deve passar o AbortSignal para o httpClient', async () => {
    vi.mocked(httpClient.get).mockResolvedValueOnce({ data: {} } as any);
    const controller = new AbortController();

    await listarClientes({ page: 1 }, controller.signal);

    const callArgs = vi.mocked(httpClient.get).mock.calls[0];
    expect(callArgs[1]?.signal).toBe(controller.signal);
  });
});
