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
        setData(result);
      } catch (err: unknown) {
        if (
          (err instanceof Error || err instanceof DOMException) &&
          err.name === 'AbortError'
        ) return;
        setError(
          err instanceof Error ? err : new Error('Erro ao carregar histórico da apólice'),
        );
      } finally {
        setIsLoading(false);
      }
    }

    fetchHistorico();

    return () => controller.abort();
  }, [publicId, page, pageSize]);

  return { data, isLoading, error };
}
