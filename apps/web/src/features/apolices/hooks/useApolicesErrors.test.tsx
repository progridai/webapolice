import { act, renderHook, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import * as api from '../api/apolices.api';
import { HttpApiError } from '../../../services/http/httpError';
import { useApolices } from './useApolices';
import { useApoliceDetalhe } from './useApoliceDetalhe';
import { useApoliceHistorico } from './useApoliceHistorico';
import { useApoliceModulos } from './useApoliceModulos';
import { useApoliceSubestipulantes } from './useApoliceSubestipulantes';
import { useApoliceSubgrupos } from './useApoliceSubgrupos';

vi.mock('../api/apolices.api');

interface HookState {
  data: unknown;
  error: Error | null;
  isLoading: boolean;
  retry?: () => Promise<void>;
  refetch?: () => Promise<void>;
}

interface Scenario {
  name: string;
  useHook: (page: number) => HookState;
  reject: (error: unknown) => void;
  resolve: () => void;
  fallback: string;
  ignoresAbort: boolean;
}

const scenarios: Scenario[] = [
  {
    name: 'Listagem',
    useHook: () => useApolices({ page: 1, busca: 'Contrato' }),
    reject: (error) =>
      vi.mocked(api.listarApolices).mockRejectedValueOnce(error),
    resolve: () =>
      vi
        .mocked(api.listarApolices)
        .mockResolvedValue({ items: [], page: 1, pageSize: 20, totalCount: 0 }),
    fallback: 'Erro ao carregar apólices',
    ignoresAbort: true,
  },
  {
    name: 'Detalhe',
    useHook: () => useApoliceDetalhe('apolice-1'),
    reject: (error) => vi.mocked(api.obterApolice).mockRejectedValueOnce(error),
    resolve: () =>
      vi.mocked(api.obterApolice).mockResolvedValue({
        publicId: 'apolice-1',
        nome: 'Contrato',
        estipulanteId: 1,
        estipulanteNome: 'Empresa',
        seguradoraId: 2,
        seguradoraNome: 'Seguradora',
        ativo: true,
        status: 'Ativa',
        ramos: [],
      }),
    fallback: 'Erro ao carregar detalhes da apólice',
    ignoresAbort: false,
  },
  {
    name: 'Histórico',
    useHook: (page) => useApoliceHistorico('apolice-1', page),
    reject: (error) =>
      vi.mocked(api.listarApoliceHistorico).mockRejectedValueOnce(error),
    resolve: () =>
      vi
        .mocked(api.listarApoliceHistorico)
        .mockResolvedValue({ items: [], page: 2, pageSize: 20, totalCount: 0 }),
    fallback: 'Erro ao carregar histórico da apólice',
    ignoresAbort: true,
  },
  {
    name: 'Módulos',
    useHook: () => useApoliceModulos('apolice-1'),
    reject: (error) =>
      vi.mocked(api.listarApoliceModulos).mockRejectedValueOnce(error),
    resolve: () => vi.mocked(api.listarApoliceModulos).mockResolvedValue([]),
    fallback: 'Erro ao carregar módulos da apólice',
    ignoresAbort: true,
  },
  {
    name: 'Subestipulantes',
    useHook: () => useApoliceSubestipulantes('apolice-1'),
    reject: (error) =>
      vi.mocked(api.listarApoliceSubestipulantes).mockRejectedValueOnce(error),
    resolve: () =>
      vi.mocked(api.listarApoliceSubestipulantes).mockResolvedValue([]),
    fallback: 'Erro ao carregar subestipulantes da apólice',
    ignoresAbort: true,
  },
  {
    name: 'Subgrupos',
    useHook: () => useApoliceSubgrupos('apolice-1'),
    reject: (error) =>
      vi.mocked(api.listarApoliceSubgrupos).mockRejectedValueOnce(error),
    resolve: () => vi.mocked(api.listarApoliceSubgrupos).mockResolvedValue([]),
    fallback: 'Erro ao carregar subgrupos da apólice',
    ignoresAbort: true,
  },
];

beforeEach(() => vi.resetAllMocks());

describe.each(scenarios)(
  'Erros de Apólices: $name',
  ({ useHook, reject, resolve, fallback, ignoresAbort }) => {
    it.each([
      {
        name: 'HTTP normalizado',
        error: new HttpApiError({ status: 422, message: 'Revise os dados.' }),
      },
      { name: 'Error', error: new Error('Falha temporária') },
      { name: 'null', error: null },
      { name: 'undefined', error: undefined },
      {
        name: 'objeto sem contrato de erro',
        error: { message: 'Mensagem sem normalização' },
      },
    ])(
      'expõe $name sem falhar e permite recuperar a consulta',
      async ({ error }) => {
        reject(error);
        resolve();
        const { result, rerender } = renderHook(({ page }) => useHook(page), {
          initialProps: { page: 1 },
        });
        expect(result.current.isLoading).toBe(true);
        await waitFor(() => expect(result.current.error).toBeInstanceOf(Error));
        expect(result.current.isLoading).toBe(false);
        if (error instanceof Error) expect(result.current.error).toBe(error);
        else expect(result.current.error?.message).toBe(fallback);

        const retry = result.current.retry ?? result.current.refetch;
        if (retry) await act(async () => retry());
        else rerender({ page: 2 }); // Histórico reconsulta ao mudar a página.
        await waitFor(() => expect(result.current.isLoading).toBe(false));
        expect(result.current.error).toBeNull();
        expect(result.current.data).not.toBeNull();
      },
    );

    if (ignoresAbort) {
      it('não apresenta cancelamento como falha de consulta', async () => {
        reject(new DOMException('Consulta cancelada', 'AbortError'));
        const { result } = renderHook(() => useHook(1));
        await waitFor(() => expect(result.current.isLoading).toBe(false));
        expect(result.current.error).toBeNull();
      });
    }
  },
);

it('mantém a compatibilidade com CanceledError na listagem', async () => {
  vi.mocked(api.listarApolices).mockRejectedValueOnce(
    Object.assign(new Error('Cancelado'), { name: 'CanceledError' }),
  );
  const { result } = renderHook(() => useApolices({ page: 1 }));
  await waitFor(() => expect(result.current.isLoading).toBe(false));
  expect(result.current.error).toBeNull();
});
