import { useState, useEffect } from 'react';
import { listarApoliceHistorico } from '../api/apolices.api';
import type { ApoliceHistoricoResult, PagedResult } from '../types/apolice.types';

export function useApoliceHistorico(publicId: string | undefined, page: number = 1, pageSize: number = 20) {
  const [data, setData] = useState<PagedResult<ApoliceHistoricoResult> | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!publicId) return;

    const controller = new AbortController();

    async function fetchHistorico() {
      try {
        setIsLoading(true);
        setError(null);
        const result = await listarApoliceHistorico(publicId!, page, pageSize, controller.signal);
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setData(result);
      } catch (err: any) {
        if (err.name === 'AbortError') return;
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setError(err);
      } finally {
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setIsLoading(false);
      }
    }

    fetchHistorico();

    return () => controller.abort();
  }, [publicId, page, pageSize]);

  return { data, isLoading, error };
}
