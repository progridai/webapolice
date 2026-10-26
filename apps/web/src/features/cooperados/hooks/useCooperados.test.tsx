import { StrictMode } from 'react';
import { act, renderHook, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useCooperados } from './useCooperados';
import { listarCooperados } from '../api/cooperadosApi';
import type { CooperadosFiltersState } from '../types/cooperados.types';
import { HttpApiError } from '../../../services/http';

vi.mock('../api/cooperadosApi');
type Result = Awaited<ReturnType<typeof listarCooperados>>;
const pageA: Result = {
  itens: [
    {
      publicId: 'cooperado-A',
      nome: 'Pessoa A',
      cpfMascarado: '***',
      tipo: 1,
      desativado: false,
      dataCadastroUtc: '2026-10-22',
    },
  ],
  totalItens: 1,
  totalPaginas: 1,
  paginaAtual: 1,
  tamanhoPagina: 20,
};
const pageB: Result = {
  ...pageA,
  itens: [{ ...pageA.itens[0], publicId: 'cooperado-B', nome: 'Pessoa B' }],
  paginaAtual: 2,
};
const query: CooperadosFiltersState = { page: 1, limit: 20 };
function deferred<T>() {
  let resolve!: (data: T) => void;
  let reject!: (error: unknown) => void;
  const promise = new Promise<T>((ok, fail) => {
    resolve = ok;
    reject = fail;
  });
  return { promise, resolve, reject };
}
function preparePair() {
  const old = deferred<Result>();
  const current = deferred<Result>();
  vi.mocked(listarCooperados)
    .mockReturnValueOnce(old.promise)
    .mockReturnValueOnce(current.promise);
  return { old, current };
}
beforeEach(() => vi.resetAllMocks());

describe('Consultas de Cooperados', () => {
  it('inicia carregando e encaminha sinal e filtros antes de expor a página recebida', async () => {
    vi.mocked(listarCooperados).mockResolvedValue(pageA);
    const { result } = renderHook(() => useCooperados(query));
    expect(result.current.isLoading).toBe(true);
    expect(result.current.data).toBeNull();
    await waitFor(() => expect(result.current.data).toEqual(pageA));
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBeNull();
    expect(listarCooperados).toHaveBeenCalledExactlyOnceWith(
      {
        ...query,
        nome: undefined,
        cpf: undefined,
        status: undefined,
        sortBy: undefined,
        direction: undefined,
      },
      expect.any(AbortSignal),
    );
  });

  it.each([
    ['page', 2],
    ['limit', 50],
    ['nome', '  JOÃO da SILVA  '],
    ['cpf', '111.222.333-44'],
    ['status', '2'],
    ['sortBy', 'nome'],
    ['direction', 'desc'],
  ] as const)(
    'reconsulta quando %s muda e mantém o parâmetro na nova tentativa',
    async (field, value) => {
      vi.mocked(listarCooperados).mockResolvedValue(pageA);
      const { result, rerender } = renderHook(
        ({ filters }: { filters: CooperadosFiltersState }) =>
          useCooperados(filters),
        { initialProps: { filters: query } },
      );
      await waitFor(() => expect(result.current.isLoading).toBe(false));
      const signal = vi.mocked(listarCooperados).mock.calls[0][1];
      rerender({ filters: { ...query, [field]: value } });
      expect(signal?.aborted).toBe(true);
      expect(result.current.data).toBeNull();
      expect(result.current.isLoading).toBe(true);
      await waitFor(() => expect(result.current.isLoading).toBe(false));
      await act(async () => result.current.retry());
      expect(vi.mocked(listarCooperados).mock.calls[2][0]).toMatchObject({
        [field]: value,
      });
    },
  );

  it('não reconsulta por identidade/ordem das propriedades de filtros equivalentes', async () => {
    vi.mocked(listarCooperados).mockResolvedValue(pageA);
    const { result, rerender } = renderHook(
      ({ filters }: { filters: CooperadosFiltersState }) =>
        useCooperados(filters),
      {
        initialProps: { filters: { ...query, status: '1', direction: 'asc' } },
      },
    );
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const retry = result.current.retry;
    rerender({
      filters: { direction: 'asc', status: '1', limit: 20, page: 1 },
    });
    expect(listarCooperados).toHaveBeenCalledTimes(1);
    expect(result.current.retry).toBe(retry);
  });

  it.each([false, true])(
    'descarta resposta antiga sem encerrar carregamento atual (falha: %s)',
    async (failure) => {
      const { old, current } = preparePair();
      const { result, rerender } = renderHook(
        ({ filters }) => useCooperados(filters),
        { initialProps: { filters: query } },
      );
      const oldRetry = result.current.retry;
      rerender({ filters: { ...query, page: 2 } });
      await act(async () => oldRetry());
      expect(listarCooperados).toHaveBeenCalledTimes(2);
      expect(vi.mocked(listarCooperados).mock.calls[0][1]?.aborted).toBe(true);
      await act(async () => {
        if (failure) old.reject(new Error('Falha Antiga'));
        else old.resolve(pageA);
      });
      expect(result.current.isLoading).toBe(true);
      expect(result.current.error).toBeNull();
      expect(result.current.data).toBeNull();
      await act(async () => current.resolve(pageB));
      expect(result.current.data).toEqual(pageB);
    },
  );

  it.each([false, true])(
    'descarta conclusão antiga depois que a atual já terminou (falha: %s)',
    async (failure) => {
      const { old, current } = preparePair();
      const { result, rerender } = renderHook(
        ({ filters }) => useCooperados(filters),
        { initialProps: { filters: query } },
      );
      rerender({ filters: { ...query, page: 2 } });
      await act(async () => current.resolve(pageB));
      await act(async () => {
        if (failure)
          old.reject(
            new HttpApiError({ status: 500, message: 'Falha Antiga' }),
          );
        else old.resolve(pageA);
      });
      expect(result.current.data).toEqual(pageB);
      expect(result.current.error).toBeNull();
      expect(result.current.isLoading).toBe(false);
    },
  );

  it('limpa erro anterior e oculta dados ao trocar de consulta', async () => {
    vi.mocked(listarCooperados)
      .mockResolvedValueOnce(pageA)
      .mockRejectedValueOnce(new Error('Falha Atual'))
      .mockResolvedValueOnce(pageB);
    const { result, rerender } = renderHook(
      ({ filters }) => useCooperados(filters),
      { initialProps: { filters: query } },
    );
    await waitFor(() => expect(result.current.data).toEqual(pageA));
    await act(async () => result.current.retry());
    expect(result.current.data).toEqual(pageA);
    expect(result.current.error?.message).toBe('Falha Atual');
    rerender({ filters: { ...query, page: 2 } });
    expect(result.current.error).toBeNull();
    expect(result.current.data).toBeNull();
    await waitFor(() => expect(result.current.data).toEqual(pageB));
  });

  it('retry aborta chamada pendente e sua Promise acompanha a conclusão', async () => {
    const { old, current } = preparePair();
    const { result } = renderHook(() => useCooperados(query));
    let retry!: Promise<void>;
    act(() => {
      retry = result.current.retry();
    });
    expect(vi.mocked(listarCooperados).mock.calls[0][1]?.aborted).toBe(true);
    await act(async () => old.resolve(pageA));
    expect(result.current.isLoading).toBe(true);
    await act(async () => {
      current.resolve(pageB);
      await retry;
    });
    expect(result.current.data).toEqual(pageB);
    expect(result.current.isLoading).toBe(false);
  });

  it('mantém dados da mesma consulta durante retry e aborta retry na desmontagem', async () => {
    const { old, current } = preparePair();
    const { result, unmount } = renderHook(() => useCooperados(query));
    await act(async () => old.resolve(pageA));
    const retry = result.current.retry;
    let completion!: Promise<void>;
    act(() => {
      completion = retry();
    });
    expect(result.current.data).toEqual(pageA);
    expect(result.current.isLoading).toBe(true);
    unmount();
    expect(vi.mocked(listarCooperados).mock.calls[1][1]?.aborted).toBe(true);
    await act(async () => {
      current.reject(new Error('Falha Tardia'));
      await completion;
      await retry();
    });
    expect(listarCooperados).toHaveBeenCalledTimes(2);
  });

  it.each([
    {
      name: 'HTTP',
      error: new HttpApiError({ status: 422, message: 'Revise os dados.' }),
      message: 'Revise os dados.',
    },
    {
      name: 'HTTP sem mensagem',
      error: new HttpApiError({ status: 500, message: '' }),
      message: 'Erro ao comunicar com a API',
    },
    {
      name: 'Error',
      error: new Error('Falha Temporária'),
      message: 'Falha Temporária',
    },
    { name: 'null', error: null, message: 'Erro desconhecido' },
    { name: 'undefined', error: undefined, message: 'Erro desconhecido' },
    {
      name: 'objeto sem contrato',
      error: { message: 'Mensagem avulsa' },
      message: 'Erro desconhecido',
    },
  ])(
    'preserva tratamento de $name e permite recuperar',
    async ({ error, message }) => {
      vi.mocked(listarCooperados)
        .mockRejectedValueOnce(error)
        .mockResolvedValueOnce(pageA);
      const { result } = renderHook(() => useCooperados(query));
      await waitFor(() => expect(result.current.isLoading).toBe(false));
      expect(result.current.error?.message).toBe(message);
      if (error instanceof HttpApiError) {
        expect(result.current.error).not.toBe(error);
        expect(result.current.error?.cause).toBe(error);
      } else if (error instanceof Error)
        expect(result.current.error).toBe(error);
      await act(async () => result.current.retry());
      expect(result.current.data).toEqual(pageA);
      expect(result.current.error).toBeNull();
    },
  );

  it.each([
    new DOMException('Cancelada', 'AbortError'),
    Object.assign(new Error('Cancelada'), { name: 'AbortError' }),
    Object.assign(new Error('Cancelada'), { name: 'CanceledError' }),
  ])(
    'trata $name como cancelamento sem manter carregamento permanente',
    async (error) => {
      vi.mocked(listarCooperados).mockRejectedValueOnce(error);
      const { result } = renderHook(() => useCooperados(query));
      await waitFor(() => expect(result.current.isLoading).toBe(false));
      expect(result.current.error).toBeNull();
    },
  );

  it('suporta montagem dupla em StrictMode e aborta a chamada atual na desmontagem', async () => {
    const { old, current } = preparePair();
    const { result, unmount } = renderHook(() => useCooperados(query), {
      wrapper: StrictMode,
    });
    expect(listarCooperados).toHaveBeenCalledTimes(2);
    expect(vi.mocked(listarCooperados).mock.calls[0][1]?.aborted).toBe(true);
    await act(async () => current.resolve(pageB));
    await act(async () => old.resolve(pageA));
    expect(result.current.data).toEqual(pageB);
    unmount();
    expect(vi.mocked(listarCooperados).mock.calls[1][1]?.aborted).toBe(true);
  });
});
