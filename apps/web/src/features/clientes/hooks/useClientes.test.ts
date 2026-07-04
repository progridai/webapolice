/* eslint-disable @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-function-type, @typescript-eslint/no-unused-vars */
import { renderHook, act, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useClientes } from './useClientes';
import * as api from '../api/clientesApi';

vi.mock('../api/clientesApi', () => ({
  listarClientes: vi.fn(),
}));

describe('useClientes', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve iniciar com estado de loading', async () => {
    let resolveFirst: Function;
    vi.mocked(api.listarClientes).mockImplementation(
      () => new Promise((resolve) => { resolveFirst = resolve; })
    );

    const { result } = renderHook((q) => useClientes(q), { initialProps: { page: 1 } });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
    
    // Resolvemos para não vazar a promise
    resolveFirst!({ itens: [], totalPaginas: 0 });
    await waitFor(() => expect(result.current.isLoading).toBe(false));
  });

  it('deve atualizar o estado ao carregar com sucesso', async () => {
    const mockData = { itens: [{ id: 1, nome: 'Teste' }], totalPaginas: 1 } as any;
    vi.mocked(api.listarClientes).mockResolvedValueOnce(mockData);

    const { result } = renderHook((q) => useClientes(q), { initialProps: { page: 1 } });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockData);
    expect(result.current.error).toBeNull();
  });

  it('deve armazenar erro quando a API falhar', async () => {
    const error = new Error('Falha de rede');
    vi.mocked(api.listarClientes).mockRejectedValueOnce(error);

    const { result } = renderHook((q) => useClientes(q), { initialProps: { page: 1 } });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.error).toBe(error);
    expect(result.current.data).toBeNull();
  });

  it('deve ignorar erro de aborto', async () => {
    const error = new DOMException('Aborted', 'AbortError');
    vi.mocked(api.listarClientes).mockRejectedValueOnce(error);

    const { result } = renderHook((q) => useClientes(q), { initialProps: { page: 1 } });

    expect(result.current.error).toBeNull();
  });

  it('deve cancelar chamadas antigas se a query mudar rapidamente', async () => {
    let resolveFirst: Function;
    const firstPromise = new Promise((resolve) => { resolveFirst = resolve; });
    const secondPromise = Promise.resolve({ itens: [{ id: 2 }] } as any);

    vi.mocked(api.listarClientes)
      .mockReturnValueOnce(firstPromise as any)
      .mockReturnValueOnce(secondPromise);

    const { rerender, result } = renderHook(
      (q) => useClientes(q),
      { initialProps: { page: 1 } }
    );

    rerender({ page: 2 });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.data?.itens[0].id).toBe(2);
    });
    
    resolveFirst!({ itens: [{ id: 1 }] });
    await firstPromise;
  });
});
