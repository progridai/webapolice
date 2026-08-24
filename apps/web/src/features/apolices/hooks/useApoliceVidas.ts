import { useState, useEffect, useCallback } from 'react';
import { listarApoliceVidas } from '../api/apolices.api';
import type { PagedResult, ApoliceVidaListItem } from '../types/apolice.types';

export function useApoliceVidas(publicId: string | undefined, query: import('../types/apolice.types').ApoliceVidaQuery) {
  const [data, setData] = useState<PagedResult<ApoliceVidaListItem> | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchData = useCallback(async (abortSignal?: AbortSignal) => {
    if (!publicId) return;

    setIsLoading(true);
    setError(null);
    try {
      const result = await listarApoliceVidas(publicId, query, abortSignal);
      setData(result);
    } catch (err: any) {
      if (err.name !== 'AbortError' && err.name !== 'CanceledError') {
        setError(err instanceof Error ? err : new Error(err.message || 'Erro ao carregar vidas da apólice'));
      }
    } finally {
      setIsLoading(false);
    }
  }, [publicId, query.page, query.pageSize, query.busca, query.status, query.subestipulantePublicId, query.moduloPublicId]);

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
