import { useState, useEffect, useCallback } from 'react';
import { listarApolices } from '../api/apolices.api';
import type { ApoliceListItem, ApolicesQuery, PagedResult } from '../types/apolice.types';

export function useApolices(query: ApolicesQuery, refreshTrigger: number = 0) {
  const [data, setData] = useState<PagedResult<ApoliceListItem> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchData = useCallback(async (abortSignal?: AbortSignal) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await listarApolices(query, abortSignal);
      setData(result);
    } catch (err: any) {
      if (err.name !== 'AbortError' && err.name !== 'CanceledError') {
        setError(err instanceof Error ? err : new Error(err.message || 'Erro ao carregar apólices'));
      }
    } finally {
      setIsLoading(false);
    }
  }, [JSON.stringify(query), refreshTrigger]); // eslint-disable-line react-hooks/exhaustive-deps

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
