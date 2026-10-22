import { useState, useEffect, useCallback } from 'react';
import { listarApoliceSubestipulantes } from '../api/apolices.api';
import type { ApoliceSubestipulanteResult } from '../types/apolice.types';

export function useApoliceSubestipulantes(publicId: string | undefined) {
  const [data, setData] = useState<ApoliceSubestipulanteResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchSubestipulantes = useCallback(async () => {
    if (!publicId) return;
    try {
      setIsLoading(true);
      setError(null);
      const result = await listarApoliceSubestipulantes(publicId);
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
          : new Error('Erro ao carregar subestipulantes da apólice'),
      );
    } finally {
      setIsLoading(false);
    }
  }, [publicId]);

  useEffect(() => {
    fetchSubestipulantes();
  }, [fetchSubestipulantes]);

  return { data, isLoading, error, refetch: fetchSubestipulantes };
}
