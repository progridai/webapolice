import { act, renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useApoliceVidas } from './useApoliceVidas';
import { listarApoliceVidas } from '../api/apolices.api';
import type {
  ApoliceVidaQuery,
  ApoliceVidaListItem,
  PagedResult,
} from '../types/apolice.types';

vi.mock('../api/apolices.api', () => ({ listarApoliceVidas: vi.fn() }));
const empty: PagedResult<ApoliceVidaListItem> = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 10,
};

describe('Atualização dos filtros de Vidas', () => {
  it.each([
    'apoliceSubgrupoPublicId',
    'apoliceModuloPublicId',
    'vigenciaDataReferencia',
  ] as const)(
    'reconsulta quando %s muda e mantém o cancelamento da requisição anterior',
    async (field) => {
      vi.mocked(listarApoliceVidas).mockResolvedValue(empty);
      const { result, rerender, unmount } = renderHook(
        ({ query }: { query: ApoliceVidaQuery }) =>
          useApoliceVidas('apolice-1', query),
        { initialProps: { query: { page: 1, pageSize: 10 } } },
      );
      await waitFor(() => expect(result.current.data).toEqual(empty));
      const oldSignal = vi.mocked(listarApoliceVidas).mock.calls[0][2];
      const next = {
        page: 1,
        pageSize: 10,
        [field]:
          field === 'vigenciaDataReferencia' ? '2026-10-21' : 'uuid-do-filtro',
      };
      rerender({ query: next });
      await waitFor(() => expect(listarApoliceVidas).toHaveBeenCalledTimes(2));
      expect(oldSignal?.aborted).toBe(true);
      expect(vi.mocked(listarApoliceVidas).mock.calls[1][1]).toMatchObject(
        next,
      );
      await waitFor(() => expect(result.current.isLoading).toBe(false));
      unmount();
      expect(vi.mocked(listarApoliceVidas).mock.calls[1][2]?.aborted).toBe(
        true,
      );
    },
  );

  it('mantém a resposta atual quando uma consulta cancelada termina depois', async () => {
    let resolveOld!: (value: PagedResult<ApoliceVidaListItem>) => void;
    vi.mocked(listarApoliceVidas).mockReturnValueOnce(
      new Promise((resolve) => {
        resolveOld = resolve;
      }),
    );
    vi.mocked(listarApoliceVidas).mockResolvedValueOnce({ ...empty, page: 2 });
    const { result, rerender } = renderHook(
      ({ query }: { query: ApoliceVidaQuery }) =>
        useApoliceVidas('apolice-1', query),
      { initialProps: { query: { page: 1 } } },
    );
    rerender({ query: { page: 2 } });
    await waitFor(() => expect(result.current.data?.page).toBe(2));
    await act(async () => resolveOld(empty));
    expect(result.current.data?.page).toBe(2);
    expect(result.current.error).toBeNull();
  });

  it('expõe a falha e permite tentar novamente com os filtros atuais', async () => {
    vi.mocked(listarApoliceVidas)
      .mockRejectedValueOnce(new Error('Falha temporária'))
      .mockResolvedValueOnce(empty);
    const { result } = renderHook(() =>
      useApoliceVidas('apolice-1', { apoliceModuloPublicId: 'modulo-1' }),
    );
    await waitFor(() =>
      expect(result.current.error?.message).toBe('Falha temporária'),
    );
    await act(async () => result.current.retry());
    expect(result.current.data).toEqual(empty);
    expect(result.current.error).toBeNull();
    expect(
      vi.mocked(listarApoliceVidas).mock.calls[1][1].apoliceModuloPublicId,
    ).toBe('modulo-1');
  });
});
