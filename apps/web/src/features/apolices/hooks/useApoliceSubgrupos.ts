import { useState, useEffect, useCallback } from 'react';
import { listarApoliceSubgrupos } from '../api/apolices.api';
import type { ApoliceSubgrupoResult } from '../types/apolice.types';

export function useApoliceSubgrupos(publicId: string | undefined) {
  const [data, setData] = useState<ApoliceSubgrupoResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchSubgrupos = useCallback(async () => {
    if (!publicId) return;
    try {
      setIsLoading(true);
      setError(null);
      const result = await listarApoliceSubgrupos(publicId);
      setData(result);
    } catch (err: unknown) {
      if (
        (err instanceof Error || err instanceof DOMException) &&
        err.name === 'AbortError'
      )
        return;
      setError(
        err instanceof Error
          ? err
          : new Error('Erro ao carregar subgrupos da apólice'),
      );
    } finally {
      setIsLoading(false);
    }
  }, [publicId]);

  useEffect(() => {
    fetchSubgrupos();
  }, [fetchSubgrupos]);

  return { data, isLoading, error, refetch: fetchSubgrupos };
}
