import { StrictMode } from 'react';
import { act, renderHook, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import * as api from '../api/apolices.api';
import type { ApolicesQuery, ApoliceVidaQuery } from '../types/apolice.types';
import { useApolices } from './useApolices';
import { useApoliceDetalhe } from './useApoliceDetalhe';
import { useApoliceVidas } from './useApoliceVidas';
import { useApoliceHistorico } from './useApoliceHistorico';
import { useApoliceUniversoPermitido } from './useApoliceUniversoPermitido';
import { useApoliceModulos } from './useApoliceModulos';
import { useApoliceSubgrupos } from './useApoliceSubgrupos';
import { useApoliceSubestipulantes } from './useApoliceSubestipulantes';

vi.mock('../api/apolices.api');
beforeEach(() => vi.resetAllMocks());

function deferred<T>() {
  let resolve!: (data: T) => void;
  let reject!: (error: unknown) => void;
  const promise = new Promise<T>((ok, fail) => {
    resolve = ok;
    reject = fail;
  });
  return { promise, resolve, reject };
}

function preparePair<T>(
  enqueue: (promise: Promise<T>) => void,
  oldValue: T,
  currentValue: T,
) {
  const old = deferred<T>();
  const current = deferred<T>();
  enqueue(old.promise);
  enqueue(current.promise);
  return {
    oldValue,
    currentValue,
    resolveOld: () => old.resolve(oldValue),
    rejectOld: () => old.reject(new Error('Falha antiga')),
    resolveCurrent: () => current.resolve(currentValue),
  };
}

interface Scenario {
  name: string;
  requiresId: boolean;
  supportsRefetch?: boolean;
  emptyData?: unknown;
  useHook: (key: string | undefined) => {
    data: unknown;
    error: Error | null;
    isLoading: boolean;
    refetch?: () => Promise<void>;
  };
  prepare: () => {
    oldValue: unknown;
    currentValue: unknown;
    resolveOld: () => void;
    rejectOld: () => void;
    resolveCurrent: () => void;
  };
  signal: (index: number) => AbortSignal | undefined;
  calls: () => number;
}

const pageA: Awaited<ReturnType<typeof api.listarApolices>> = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 20,
};
const pageB: typeof pageA = { ...pageA, page: 2 };
const modulosA: Awaited<ReturnType<typeof api.listarApoliceModulos>> = [
  {
    publicId: 'vinculo-A',
    moduloPublicId: 'modulo-A',
    nome: 'Módulo A',
    ativo: true,
  },
];
const modulosB: typeof modulosA = [
  { ...modulosA[0], publicId: 'vinculo-B', nome: 'Módulo B' },
];
const subgruposA: Awaited<ReturnType<typeof api.listarApoliceSubgrupos>> = [
  { subgrupoPublicId: 'subgrupo-A', nome: 'Subgrupo A', ativo: true },
];
const subgruposB: typeof subgruposA = [
  { ...subgruposA[0], subgrupoPublicId: 'subgrupo-B', nome: 'Subgrupo B' },
];
const subsA: Awaited<ReturnType<typeof api.listarApoliceSubestipulantes>> = [
  { subestipulantePublicId: 'empresa-A', nome: 'Empresa A', ativo: true },
];
const subsB: typeof subsA = [
  { ...subsA[0], subestipulantePublicId: 'empresa-B', nome: 'Empresa B' },
];

function getRefetch(state: { refetch?: () => Promise<void> }) {
  if (!state.refetch) throw new Error('Hook deve oferecer nova tentativa');
  return state.refetch;
}

const detalheA: Awaited<ReturnType<typeof api.obterApolice>> = {
  publicId: 'A',
  nome: 'Apólice A',
  estipulanteId: 1,
  estipulanteNome: 'Empresa',
  seguradoraId: 2,
  seguradoraNome: 'Seguradora',
  status: 'Ativa',
  ativo: true,
  ramos: [],
};
const detalheB: typeof detalheA = {
  ...detalheA,
  publicId: 'B',
  nome: 'Apólice B',
};
const vidasA: Awaited<ReturnType<typeof api.listarApoliceVidas>> = {
  items: [
    {
      apoliceVidaPublicId: 'vida-A',
      clientePublicId: 'cliente-A',
      clienteNome: 'Pessoa A',
      ativo: true,
      status: 'Ativa',
    },
  ],
  totalCount: 1,
  page: 1,
  pageSize: 20,
};
const vidasB: typeof vidasA = { ...vidasA, page: 2 };
const historicoA: Awaited<ReturnType<typeof api.listarApoliceHistorico>> = {
  ...pageA,
  items: [
    {
      acao: 'INCLUSAO',
      descricao: 'EVIDÊNCIA ORIGINAL',
      dataAcao: '2026-10-22',
    },
  ],
};
const historicoB: typeof historicoA = { ...historicoA, page: 2 };
const universoA: Awaited<ReturnType<typeof api.obterApoliceUniversoPermitido>> =
  {
    produtos: [{ produtoIdInternal: 1, ativo: true, planos: [] }],
  };
const universoB: typeof universoA = { produtos: [] };

const scenarios: Scenario[] = [
  {
    name: 'Detalhe',
    requiresId: true,
    emptyData: null,
    useHook: (key) => {
      const { retry, ...state } = useApoliceDetalhe(key);
      return { ...state, refetch: retry };
    },
    prepare: () =>
      preparePair(
        (p) => vi.mocked(api.obterApolice).mockReturnValueOnce(p),
        detalheA,
        detalheB,
      ),
    signal: (i) => vi.mocked(api.obterApolice).mock.calls[i]?.[1],
    calls: () => vi.mocked(api.obterApolice).mock.calls.length,
  },
  {
    name: 'Vidas',
    requiresId: true,
    emptyData: null,
    useHook: (key) => {
      const { retry, ...state } = useApoliceVidas(key, {
        page: 1,
        pageSize: 20,
      });
      return { ...state, refetch: retry };
    },
    prepare: () =>
      preparePair(
        (p) => vi.mocked(api.listarApoliceVidas).mockReturnValueOnce(p),
        vidasA,
        vidasB,
      ),
    signal: (i) => vi.mocked(api.listarApoliceVidas).mock.calls[i]?.[2],
    calls: () => vi.mocked(api.listarApoliceVidas).mock.calls.length,
  },
  {
    name: 'Histórico',
    requiresId: true,
    emptyData: null,
    supportsRefetch: false,
    useHook: useApoliceHistorico,
    prepare: () =>
      preparePair(
        (p) => vi.mocked(api.listarApoliceHistorico).mockReturnValueOnce(p),
        historicoA,
        historicoB,
      ),
    signal: (i) => vi.mocked(api.listarApoliceHistorico).mock.calls[i]?.[3],
    calls: () => vi.mocked(api.listarApoliceHistorico).mock.calls.length,
  },
  {
    name: 'Universo Permitido',
    requiresId: true,
    emptyData: null,
    useHook: useApoliceUniversoPermitido,
    prepare: () =>
      preparePair(
        (p) =>
          vi.mocked(api.obterApoliceUniversoPermitido).mockReturnValueOnce(p),
        universoA,
        universoB,
      ),
    signal: (i) =>
      vi.mocked(api.obterApoliceUniversoPermitido).mock.calls[i]?.[1],
    calls: () => vi.mocked(api.obterApoliceUniversoPermitido).mock.calls.length,
  },
  {
    name: 'Listagem',
    requiresId: false,
    useHook: (key) => {
      const { retry, ...state } = useApolices({ busca: key });
      return { ...state, refetch: retry };
    },
    prepare: () =>
      preparePair(
        (p) => vi.mocked(api.listarApolices).mockReturnValueOnce(p),
        pageA,
        pageB,
      ),
    signal: (i) => vi.mocked(api.listarApolices).mock.calls[i]?.[1],
    calls: () => vi.mocked(api.listarApolices).mock.calls.length,
  },
  {
    name: 'Módulos',
    requiresId: true,
    useHook: useApoliceModulos,
    prepare: () =>
      preparePair(
        (p) => vi.mocked(api.listarApoliceModulos).mockReturnValueOnce(p),
        modulosA,
        modulosB,
      ),
    signal: (i) => vi.mocked(api.listarApoliceModulos).mock.calls[i]?.[1],
    calls: () => vi.mocked(api.listarApoliceModulos).mock.calls.length,
  },
  {
    name: 'Subgrupos',
    requiresId: true,
    useHook: useApoliceSubgrupos,
    prepare: () =>
      preparePair(
        (p) => vi.mocked(api.listarApoliceSubgrupos).mockReturnValueOnce(p),
        subgruposA,
        subgruposB,
      ),
    signal: (i) => vi.mocked(api.listarApoliceSubgrupos).mock.calls[i]?.[1],
    calls: () => vi.mocked(api.listarApoliceSubgrupos).mock.calls.length,
  },
  {
    name: 'Subestipulantes',
    requiresId: true,
    useHook: useApoliceSubestipulantes,
    prepare: () =>
      preparePair(
        (p) =>
          vi.mocked(api.listarApoliceSubestipulantes).mockReturnValueOnce(p),
        subsA,
        subsB,
      ),
    signal: (i) =>
      vi.mocked(api.listarApoliceSubestipulantes).mock.calls[i]?.[1],
    calls: () => vi.mocked(api.listarApoliceSubestipulantes).mock.calls.length,
  },
];

describe.each(scenarios)('Ciclo de consulta: $name', (scenario) => {
  it.each([false, true])(
    'descarta término antigo e mantém o carregamento atual (falha: %s)',
    async (failure) => {
      const pending = scenario.prepare();
      const { result, rerender } = renderHook(
        ({ key }) => scenario.useHook(key),
        { initialProps: { key: 'A' } },
      );
      expect(result.current.isLoading).toBe(true);
      const oldRetry = result.current.refetch;
      rerender({ key: 'B' });
      expect(scenario.signal(0)?.aborted).toBe(true);
      await act(async () => oldRetry?.());
      expect(scenario.calls()).toBe(2);
      expect(scenario.signal(1)?.aborted).toBe(false);
      await act(async () => {
        if (failure) pending.rejectOld();
        else pending.resolveOld();
      });
      expect(result.current.isLoading).toBe(true);
      expect(result.current.error).toBeNull();
      expect(result.current.data).not.toEqual(pending.oldValue);
      await act(async () => pending.resolveCurrent());
      expect(result.current.data).toEqual(pending.currentValue);
      expect(result.current.isLoading).toBe(false);
    },
  );

  it('descarta falha antiga depois que a consulta atual já terminou', async () => {
    const pending = scenario.prepare();
    const { result, rerender } = renderHook(
      ({ key }) => scenario.useHook(key),
      { initialProps: { key: 'A' } },
    );
    rerender({ key: 'B' });
    await act(async () => pending.resolveCurrent());
    await act(async () => pending.rejectOld());
    expect(result.current.data).toEqual(pending.currentValue);
    expect(result.current.error).toBeNull();
    expect(result.current.isLoading).toBe(false);
  });

  it('limpa o erro anterior ao trocar de consulta', async () => {
    const pending = scenario.prepare();
    const { result, rerender } = renderHook(
      ({ key }) => scenario.useHook(key),
      { initialProps: { key: 'A' } },
    );
    await act(async () => pending.rejectOld());
    expect(result.current.error?.message).toBe('Falha antiga');
    rerender({ key: 'B' });
    expect(result.current.error).toBeNull();
    expect(result.current.isLoading).toBe(true);
    await act(async () => pending.resolveCurrent());
    expect(result.current.data).toEqual(pending.currentValue);
  });

  it('oculta dados da consulta anterior enquanto carrega outra', async () => {
    const pending = scenario.prepare();
    const { result, rerender } = renderHook(
      ({ key }) => scenario.useHook(key),
      { initialProps: { key: 'A' } },
    );
    await act(async () => pending.resolveOld());
    expect(result.current.data).toEqual(pending.oldValue);
    rerender({ key: 'B' });
    expect(result.current.data).not.toEqual(pending.oldValue);
    expect(result.current.isLoading).toBe(true);
    await act(async () => pending.resolveCurrent());
    expect(result.current.data).toEqual(pending.currentValue);
  });

  if (scenario.supportsRefetch !== false) {
    it('nova tentativa aborta a consulta pendente e retorna uma Promise até terminar', async () => {
      const pending = scenario.prepare();
      const { result } = renderHook(() => scenario.useHook('A'));
      let retry!: Promise<void>;
      act(() => {
        retry = getRefetch(result.current)();
      });
      expect(scenario.signal(0)?.aborted).toBe(true);
      await act(async () => pending.resolveOld());
      expect(result.current.isLoading).toBe(true);
      await act(async () => {
        pending.resolveCurrent();
        await retry;
      });
      expect(result.current.data).toEqual(pending.currentValue);
      expect(result.current.isLoading).toBe(false);
    });

    it('mantém os dados da mesma consulta durante a nova tentativa', async () => {
      const pending = scenario.prepare();
      const { result } = renderHook(() => scenario.useHook('A'));
      await act(async () => pending.resolveOld());
      let retry!: Promise<void>;
      act(() => {
        retry = getRefetch(result.current)();
      });
      expect(result.current.data).toEqual(pending.oldValue);
      expect(result.current.isLoading).toBe(true);
      await act(async () => {
        pending.resolveCurrent();
        await retry;
      });
      expect(result.current.data).toEqual(pending.currentValue);
    });

    it('aborta nova tentativa na desmontagem e ignora callback guardado', async () => {
      const pending = scenario.prepare();
      const { result, unmount } = renderHook(() => scenario.useHook('A'));
      const retry = getRefetch(result.current);
      let completion!: Promise<void>;
      act(() => {
        completion = retry();
      });
      unmount();
      expect(scenario.signal(1)?.aborted).toBe(true);
      await act(async () => {
        pending.resolveOld();
        pending.resolveCurrent();
        await completion;
        await retry();
      });
      expect(scenario.calls()).toBe(2);
    });
  }

  it('suporta a montagem dupla do StrictMode sem aceitar o resultado cancelado', async () => {
    const pending = scenario.prepare();
    const { result } = renderHook(() => scenario.useHook('A'), {
      wrapper: StrictMode,
    });
    expect(scenario.calls()).toBe(2);
    expect(scenario.signal(0)?.aborted).toBe(true);
    await act(async () => pending.resolveCurrent());
    await act(async () => pending.resolveOld());
    expect(result.current.data).toEqual(pending.currentValue);
    expect(result.current.isLoading).toBe(false);
  });

  if (scenario.requiresId) {
    it('limpa a consulta ao remover o identificador e não busca sem apólice', async () => {
      const pending = scenario.prepare();
      const initialProps: { key: string | undefined } = { key: 'A' };
      const { result, rerender } = renderHook(
        ({ key }: { key: string | undefined }) => scenario.useHook(key),
        { initialProps },
      );
      await act(async () => pending.resolveOld());
      rerender({ key: undefined });
      expect(scenario.signal(0)?.aborted).toBe(true);
      expect(result.current.data).toEqual(
        scenario.emptyData === null ? null : [],
      );
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toBeNull();
      await act(async () => result.current.refetch?.());
      expect(scenario.calls()).toBe(1);
    });
  }
});

describe('Filtros e atualização da listagem', () => {
  it.each([
    ['page', 2],
    ['pageSize', 50],
    ['busca', 'Contrato'],
    ['status', 'Inativa'],
    ['ativo', false],
    ['estipulanteId', 'empresa-1'],
    ['seguradoraId', 'seguradora-1'],
    ['tipoRamo', 'VIDA'],
    ['vigenciaDataReferencia', '2026-10-22'],
  ] as const)('reconsulta quando %s muda', async (field, value) => {
    vi.mocked(api.listarApolices).mockResolvedValue(pageA);
    const { rerender, result } = renderHook(
      ({ query }: { query: ApolicesQuery }) => useApolices(query),
      { initialProps: { query: {} } },
    );
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const old = vi.mocked(api.listarApolices).mock.calls[0][1];
    rerender({ query: { [field]: value } });
    expect(old?.aborted).toBe(true);
    await waitFor(() => expect(api.listarApolices).toHaveBeenCalledTimes(2));
    expect(vi.mocked(api.listarApolices).mock.calls[1][0]).toMatchObject({
      [field]: value,
    });
  });

  it('não reconsulta por identidade ou ordem das propriedades de filtros equivalentes', async () => {
    vi.mocked(api.listarApolices).mockResolvedValue(pageA);
    const { result, rerender } = renderHook(
      ({ query }: { query: ApolicesQuery }) => useApolices(query),
      { initialProps: { query: { page: 1, ativo: false } } },
    );
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    rerender({ query: { ativo: false, page: 1 } });
    expect(api.listarApolices).toHaveBeenCalledTimes(1);
  });

  it('preserva refreshTrigger e nova tentativa com os filtros atuais', async () => {
    vi.mocked(api.listarApolices).mockResolvedValue(pageA);
    const { result, rerender } = renderHook(
      ({ refresh }) => useApolices({ busca: 'Atual', ativo: false }, refresh),
      { initialProps: { refresh: 0 } },
    );
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const old = vi.mocked(api.listarApolices).mock.calls[0][1];
    rerender({ refresh: 1 });
    expect(result.current.data).toEqual(pageA);
    expect(result.current.isLoading).toBe(true);
    await waitFor(() => expect(api.listarApolices).toHaveBeenCalledTimes(2));
    expect(old?.aborted).toBe(true);
    await act(async () => result.current.retry());
    expect(vi.mocked(api.listarApolices).mock.calls[2][0]).toMatchObject({
      busca: 'Atual',
      ativo: false,
    });
  });
});

describe('Filtros de Vidas e paginação do Histórico', () => {
  it.each([
    ['page', 2],
    ['pageSize', 50],
    ['busca', 'Pessoa'],
    ['status', 'Inativa'],
    ['apoliceSubgrupoPublicId', 'subgrupo-B'],
    ['apoliceModuloPublicId', 'modulo-B'],
    ['vigenciaDataReferencia', '2026-10-22'],
  ] as const)('reconsulta Vidas quando %s muda', async (field, value) => {
    vi.mocked(api.listarApoliceVidas).mockResolvedValue(vidasA);
    const { result, rerender } = renderHook(
      ({ query }: { query: ApoliceVidaQuery }) => useApoliceVidas('A', query),
      { initialProps: { query: {} } },
    );
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    const old = vi.mocked(api.listarApoliceVidas).mock.calls[0][2];
    rerender({ query: { [field]: value } });
    expect(result.current.data).toBeNull();
    expect(result.current.isLoading).toBe(true);
    expect(old?.aborted).toBe(true);
    await waitFor(() =>
      expect(api.listarApoliceVidas).toHaveBeenCalledTimes(2),
    );
    await act(async () => result.current.retry());
    expect(vi.mocked(api.listarApoliceVidas).mock.calls[2][1]).toMatchObject({
      [field]: value,
    });
  });

  it('não reconsulta Vidas por identidade ou ordem de filtros equivalentes', async () => {
    vi.mocked(api.listarApoliceVidas).mockResolvedValue(vidasA);
    const { result, rerender } = renderHook(
      ({ query }: { query: ApoliceVidaQuery }) => useApoliceVidas('A', query),
      { initialProps: { query: { page: 1, status: 'Ativa' } } },
    );
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    rerender({ query: { status: 'Ativa', page: 1 } });
    expect(api.listarApoliceVidas).toHaveBeenCalledTimes(1);
  });

  it.each(['page', 'pageSize'] as const)(
    'cancela página anterior do Histórico quando %s muda',
    async (field) => {
      const pending = preparePair(
        (p) => vi.mocked(api.listarApoliceHistorico).mockReturnValueOnce(p),
        historicoA,
        historicoB,
      );
      const { result, rerender } = renderHook(
        ({ page, pageSize }) => useApoliceHistorico('A', page, pageSize),
        { initialProps: { page: 1, pageSize: 20 } },
      );
      rerender({ page: 1, pageSize: 20, [field]: 2 });
      expect(
        vi.mocked(api.listarApoliceHistorico).mock.calls[0][3]?.aborted,
      ).toBe(true);
      expect(
        vi.mocked(api.listarApoliceHistorico).mock.calls[1].slice(0, 3),
      ).toEqual(['A', field === 'page' ? 2 : 1, field === 'pageSize' ? 2 : 20]);
      await act(async () => pending.resolveCurrent());
      await act(async () => pending.resolveOld());
      expect(result.current.data).toEqual(historicoB);
      expect(result.current.isLoading).toBe(false);
    },
  );
});
