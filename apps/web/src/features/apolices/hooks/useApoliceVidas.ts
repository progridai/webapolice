import { useState, useEffect, useCallback } from 'react';
import { listarApoliceVidas } from '../api/apolices.api';
import type { PagedResult, ApoliceVidaListItem } from '../types/apolice.types';

export function useApoliceVidas(publicId: string | undefined, query: import('../types/apolice.types').ApoliceVidaQuery) {
  const { page, pageSize, busca, status, apoliceSubgrupoPublicId, apoliceModuloPublicId, vigenciaDataReferencia } = query;
  const [data, setData] = useState<PagedResult<ApoliceVidaListItem> | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchData = useCallback(async (abortSignal?: AbortSignal) => {
    if (!publicId) return;

    setIsLoading(true);
    setError(null);
    try {
      const result = await listarApoliceVidas(publicId, {
        page, pageSize, busca, status, apoliceSubgrupoPublicId, apoliceModuloPublicId, vigenciaDataReferencia,
      }, abortSignal);
      if (!abortSignal?.aborted) setData(result);
    } catch (err: unknown) {
      if (!abortSignal?.aborted && !(err instanceof Error && (err.name === 'AbortError' || err.name === 'CanceledError'))) {
        setError(err instanceof Error ? err : new Error('Erro ao carregar vidas da apólice'));
      }
    } finally {
      if (!abortSignal?.aborted) setIsLoading(false);
    }
  }, [publicId, page, pageSize, busca, status, apoliceSubgrupoPublicId, apoliceModuloPublicId, vigenciaDataReferencia]);

  useEffect(() => {
    const controller = new AbortController();
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchData(controller.signal);
    return () => {
      controller.abort();
    };
  }, [fetchData]);

  return { data, isLoading, error, retry: () => fetchData() };
}
